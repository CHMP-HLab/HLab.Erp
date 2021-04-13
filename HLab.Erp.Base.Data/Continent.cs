using HLab.Erp.Data;
using HLab.Notify.PropertyChanged;
using NPoco;

namespace HLab.Erp.Base.Data
{
    public class Continent : Entity, IEntityWithExportId
    {
        public Continent() => HD<Continent>.Initialize(this);

        public string Name
        {
            get => _name.Get();
            set => _name.Set(value);
        }

        private readonly IProperty<string> _name = HD<Continent>.Property<string>(c => c.Default(""));
        public string Code
        {
            get => _code.Get();
            set => _code.Set(value);
        }

        private readonly IProperty<string> _code = HD<Continent>.Property<string>(c => c.Default(""));
        [Ignore] public string ExportId => Code;
    }
}
