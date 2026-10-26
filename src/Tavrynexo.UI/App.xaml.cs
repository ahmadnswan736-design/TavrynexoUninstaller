using System.Windows;
using Tavrynexo.UI.Services;

namespace Tavrynexo.UI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        AppHost.ConfigureServices();

        var window = new MainWindow();
        MainWindow = window;
        window.Show();
    }
}
