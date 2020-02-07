using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HLab.Core.Annotations;
using HLab.DependencyInjection.Annotations;
using HLab.Erp.Acl;
using HLab.Erp.Core.DragDrops;
using HLab.Mvvm.Annotations;
using HLab.Notify.PropertyChanged;

namespace HLab.Erp.Core.ApplicationServices
{
    using H = NotifyHelper<MainWpfViewModel>;

    [Export(typeof(MainWpfViewModel)), Singleton]
    public class MainWpfViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWpfViewModel()
        {
            H.Initialize(this,a => PropertyChanged?.Invoke(this,a));
        }

        [Import]
        public IAclService Acl {get; }

        [Import]
        private readonly IDragDropService _dragDrop;
        [Import]
        private readonly IMessageBus _message;
        [Import(InjectLocation.AfterConstructor)]
        private readonly IDocumentService _doc;

        [Import]
        private readonly IApplicationInfoService _info;

        [Import]
        private readonly Func<object, SelectedMessage> _getSelectedMessage;

        [Import]
        public ILocalizationService LocalizationService { get; }

        public ObservableCollection<object> Anchorables { get; } = new ObservableCollection<object>();
        public ObservableCollection<object> Documents { get; } = new ObservableCollection<object>();

        public bool IsActive
        {
            get => _isActive.Get();
            set => _isActive.Set(value);
        }
        private readonly IProperty<bool> _isActive = H.Property<bool>(c => c.Default(true));

        public FrameworkElement ActiveDocument
        {
            get => _activeDocument.Get();
            set
            {
                if (_activeDocument.Set(value))
                {
                    var message = _getSelectedMessage(value);
                    _message.Publish(message);
                }
            }
        }
        private readonly IProperty<FrameworkElement> _activeDocument = H.Property<FrameworkElement>();

        public Canvas DragCanvas => _dragCanvas.Get();
        private readonly IProperty<Canvas> _dragCanvas = H.Property<Canvas>( c => c
            .Set( e => {
                    var canvas = new Canvas();
                    e._dragDrop.RegisterDragCanvas(canvas);
                    return canvas;
                }
            )
        );

        public Menu Menu { get; } = new Menu {IsMainMenu = true}; 

        public string Title => _title.Get();
        private readonly IProperty<string> _title = H.Property<string>(c => c.Set(e => e._info.Name));

        public ICommand Exit  { get; } = H.Command(c => c
            .Action(e => Application.Current.Shutdown())
        );
        public ICommand OpenUserCommand  { get; } = H.Command(c => c
            .Action(e => e._doc.OpenDocumentAsync(e.Acl.Connection.User))
        );

        public bool RegisterMenu(string[] parents, MenuItem newMenuItem, ItemCollection items=null)
        {
            if (items == null) items = Menu.Items;

            if (!parents.Any())
            {
                items.Add(newMenuItem);
                return true;
            }

            foreach (MenuItem menu in items)
            {
                if (menu.Name == parents[0])
                {
                    return RegisterMenu(parents.Skip(1).ToArray(),newMenuItem, menu.Items);
                }
            }
            return false;
        }

    }
}