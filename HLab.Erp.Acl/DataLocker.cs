﻿using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using HLab.DependencyInjection.Annotations;
using HLab.Erp.Data;
using HLab.Mvvm.Annotations;
using HLab.Notify.PropertyChanged;

namespace HLab.Erp.Acl
{
    public class DataLockerEntityDesign : IEntity<int>
    {
        private int _id;
        public object Id { get; } = 1;

        int IEntity<int>.Id
        {
            get => 1;
            set => throw new NotImplementedException();
        }

        public bool IsLoaded { get; set; } = true;
        public void OnLoaded()
        {
            throw new NotImplementedException();
        }
    }

    public class DataLockerDesign : DataLocker<DataLockerEntityDesign>, IViewModelDesign
    {
        public DataLockerDesign() : base(new DataLockerEntityDesign())
        {
        }
    }

    [Export(typeof(IDataLocker))]
    public class DataLocker<T> : N<DataLocker<T>>, IDataLocker
    where T : class,IEntity<int>
    {
        private const int HeartBeat = 10000;

        [Import] private readonly IDataService _db;
        [Import] private readonly IAclService _acl;
        private DataLock _lock = null;
        private readonly string _entityClass;
        private readonly int _entityId;
        private readonly Timer _timer;
        private readonly T _entity;

        public EntityPersister<T> Persister {
            get => _persister.Get(); 
            private set => _persister.Set(value); 
        }
        private readonly IProperty<EntityPersister<T>> _persister = H.Property<EntityPersister<T>>();

        private EntityPersister<DataLock> _lockPersister;

        [Import] private Func<T,EntityPersister<T>> _getPersister;
        [Import] private Func<DataLock,EntityPersister<DataLock>> _getLockPersister;

        public DataLocker(T entity):base(false)
        {
            _entity = entity;
            _entityClass = entity.GetType().Name;
            _entityId = (int)entity.Id;
            _timer = new Timer((o) => {
                _lock?.Heartbeat(HeartBeat);
                try
                {
                    Message = null;
                    if (_lockPersister.Save())
                    {
                        IsConnected = true;
                        return;
                    }
                    else CancelAsync();
                }
                catch (DataException ex)
                {
                    Message = "{Disconnected}";
                    IsConnected = false;
                }
            }, null,Timeout.Infinite,Timeout.Infinite);

            Initialize();

            Persister = _getPersister(entity);

            // default to edit mode when entity is new and not saved.
            if (entity.Id < 0)
                IsActive = true;
        }


        public bool IsActive
        {
            get => _isActive.Get();
            private set => _isActive.Set(value);
        }
        private readonly IProperty<bool> _isActive = H.Property<bool>(c => c.Default(false));
        public bool IsEnabled
        {
            get => _isEnabled.Get();
            set => _isEnabled.Set(value);
        }
        private readonly IProperty<bool> _isEnabled = H.Property<bool>(c => c.Default(false));
        public bool IsConnected
        {
            get => _isConnected.Get();
            set => _isConnected.Set(value);
        }
        private readonly IProperty<bool> _isConnected = H.Property<bool>(c => c.Default(false));

        public ICommand ActivateCommand { get; } = H.Command(c => c
            .Action(async e => await e.ActivateAsync().ConfigureAwait(false))
        );

        public ICommand SaveCommand { get; } = H.Command(c => c
            .CanExecute(e => e.Persister.IsDirty)
            .Action(async e => await e.SaveAsync().ConfigureAwait(false))
            .On(e => e.Persister.IsDirty).CheckCanExecute()
        );

        public ICommand CancelCommand { get; } = H.Command(c => c
            .Action(async e => await e.CancelAsync().ConfigureAwait(false))
        );

        public async Task ActivateAsync()
        {
            if (IsActive)
                return;

            var existing =
                await _db.FetchOneAsync<DataLock>(e => e.EntityClass == _entityClass && e.EntityId == _entityId).ConfigureAwait(true);

            if (existing != null)
            {
                //Already locked
                if ((DateTime.Now - existing.HeartbeatTime).TotalMilliseconds < HeartBeat)
                {
                    Message = $"{{Object locked by}} {existing.User.Initials}";
                    return;
                }

                _db.Delete(existing);
            }

            if(_entityId >= 0)
            {
                try
                {
                    //Clear error message
                    Message = null;

                    //Generate data lock token
                    _lock = await _db.AddAsync<DataLock>(t =>
                    {
                        t.UserId = _acl.Connection.UserId;
                        t.EntityClass = _entityClass;
                        t.EntityId = _entityId;
                    }).ConfigureAwait(true);

                    // Get a persister on lock
                    _lockPersister = _getLockPersister(_lock);

                    //Reload entity to be sure no changes occured before locking
                    await _db.ReFetchOneAsync(_entity).ConfigureAwait(true);
                }
                catch (Exception e)
                {
                    Message = e.Message;
                    if(_lock!=null) _db.Delete(_lock);
                    _lock = null;

                    _lockPersister=null;

                    return;
                }

                _timer.Change(HeartBeat, HeartBeat);
            }

            IsActive = true;
        }

        [Import] private Func<IDataTransaction,IAuditTrailProvider> _getAudit;

        public Action<T> BeforeSavingAction { get; set; }

        public async Task SaveAsync()
        {
            BeforeSavingAction?.Invoke(_entity);

            var transaction = _db.GetTransaction();
            try
            {
                Message = null;

                var log = Persister.ToString();

                var action = _entityId < 0 ? "Create" : "Update";

                //TODO : add AclRight needed to do the action
                if (_getAudit(transaction).Audit(action, null, log, _entity))
                {
                    await Persister.SaveAsync(transaction);
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);

                    if (_lock != null)
                        transaction.Delete(_lock);

                    IsActive = false;
                    _lock = null;

                    transaction.Done();
                }
            }
            catch (Exception e)
            {
                Message = e.Message;
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        public async Task CancelAsync()
        {
            try
            {
                Message = null;
                if (_lock != null)
                {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    await _db.ReFetchOneAsync(_entity).ConfigureAwait(true);
                    Persister.Reset();
                    _db.Delete(_lock);
                }

                IsActive = false;
            }
            catch (DataException e)
            {
                Message = e.Message;
            }
            catch (Exception e)
            {
                Message = e.Message;
            }
        }

        public string Message
        {
            get => _message.Get();
            private set => _message.Set(value);
        }

        private readonly IProperty<string> _message = H.Property<string>();

        public bool IsReadOnly => _isReadOnly.Get();
        private readonly IProperty<bool> _isReadOnly = H.Property<bool>(c => c
            .On(e => e.IsActive)
            .Set(e => !e.IsActive)
            .Default(true)
        );
        public async void Dispose()
        {
            if(IsActive)
                await CancelAsync().ConfigureAwait(false);
            _timer.Dispose();
        }

    }
}