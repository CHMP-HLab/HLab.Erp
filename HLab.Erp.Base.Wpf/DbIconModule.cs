﻿using System.Threading.Tasks;
using HLab.Core.Annotations;
using HLab.DependencyInjection.Annotations;
using HLab.Erp.Base.Data;
using HLab.Erp.Data;
using HLab.Icons.Annotations.Icons;
using HLab.Icons.Wpf;
using HLab.Icons.Wpf.Icons.Providers;
using HLab.Icons.Wpf.Providers;

namespace HLab.Erp.Base.Wpf
{
    public class DbIconModule : IBootloader
    {
        private readonly IIconService _icons;
        private readonly IDataService _data;

        [Import] public DbIconModule(IIconService icons, IDataService data)
        {
            _icons = icons;
            _data = data;
        }

        public void Load(IBootContext b)
        {
            if (_data.ServiceState != ServiceState.Available)
            {
                b.Requeue(); return;
            }

            LoadAsync();
        }

        public async void LoadAsync()
        {
            var icons =  _data.FetchAsync<Icon>().ConfigureAwait(true);

            try
            {
                await foreach (var icon in icons)
                {
                    var path = icon.Path.ToLower();

                    if (!string.IsNullOrWhiteSpace(icon.SourceXaml))
                    {
                        _icons.AddIconProvider(path, new IconProviderXamlFromSource(icon.SourceXaml, path, icon.Foreground));
                    }
                    else if (!string.IsNullOrWhiteSpace(icon.SourceSvg))
                    {
                        _icons.AddIconProvider(path, new IconProviderSvgFromSource(icon.SourceSvg, path, icon.Foreground));
                    }
                }
            }
            catch (DataException)
            {

            }
        }
    }

}