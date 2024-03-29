using HLab.Erp.Data;
using HLab.Mvvm.Application;
using HLab.Notify.PropertyChanged;

namespace HLab.Erp.Base.Data
{
    using H = HD<Country>;

    public class Country : Entity,IListableModel
    {
        public Country() => H.Initialize(this);

        public string Name
        {
            get => _name.Get();
            set => _name.Set(value);
        }

        readonly IProperty<string> _name = H.Property<string>(c => c.Default(""));
        public string IsoA2
        {
            get => _isoA2.Get();
            set => _isoA2.Set(value);
        }

        readonly IProperty<string> _isoA2 = H.Property<string>();
        public string IsoA3
        {
            get => _isoA3.Get();
            set => _isoA3.Set(value);
        }

        readonly IProperty<string> _isoA3 = H.Property<string>();

        public int Iso
       {
           get => _iso.Get();
           set => _iso.Set(value);
       }

        readonly IProperty<int> _iso = H.Property<int>();


        public string IconPath
        {
            get => _iconPath.Get();
            set => _iconPath.Set(value);
        }

        readonly IProperty<string> _iconPath = H.Property<string>();


        public int? ContinentId
        {
            get => _continentId.Get();
            set => _continentId.Set(value);
        }

        readonly IProperty<int?> _continentId = H.Property<int?>();

        public Continent Continent
        {
            set => ContinentId = value?.Id;
            get => _continent.Get();
        }

        public string Caption => _caption.Get();

        readonly IProperty<string> _caption = H.Property<string>(c => c
            .On(e => e.Name)
            .Set(e => string.IsNullOrWhiteSpace(e.Name)?"{New country}":e.Name)
        );

        readonly IProperty<Continent> _continent = H.Property<Continent>(c => c
            .Foreign(e => e.ContinentId));
    }
}
