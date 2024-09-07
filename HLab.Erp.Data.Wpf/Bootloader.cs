using System.Windows;
using HLab.Core.Annotations;
using HLab.Mvvm.Annotations;

namespace HLab.Erp.Data.Wpf
{
    // TODO : move out of wpf 
    public class ErpDataBootloader(IDataService data, IMvvmService mvvm) : IBootloader
    {
        public void Load(IBootContext bootstrapper)
        {
            //if (_mvvm.ServiceState != ServiceState.Available)
            //{
            //    bootstrapper.Requeue();
            //    return;
            //}


            data.SetConfigureAction((string message, string old) =>
            {

                var data = new ConnectionData {Message = message};
                var values = old.Split(';');

                foreach (var param in values)
                {
                    var p = param.Split('=');
                    if (p.Length == 2)
                    {
                        switch (p[0])
                        {
                            case "Host":
                                data.Server = p[1];
                                break;
                            case "Username":
                                data.UserName = p[1];
                                break;
                            //case "Password":
                            //    data.Password = p[1];
                            //    break;
                            case "Database":
                                data.Database = p[1];
                                break;
                        }
                    }
                }

                var view = mvvm.MainContext.GetView(data, typeof(ViewModeDefault), typeof(IViewClassDefault));

                var dialog = new Window
                {
                    Content = view,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    SizeToContent = SizeToContent.WidthAndHeight
                };

                if (!(dialog.ShowDialog() ?? false)) return "";

                return $"Host={data.Server};Username={data.UserName};Password={data.Password};Database={data.Database}";;
            });
        }
    }
}
