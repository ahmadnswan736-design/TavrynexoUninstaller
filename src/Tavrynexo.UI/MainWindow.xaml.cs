using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Tavrynexo.UI.Services;
using Tavrynexo.UI.ViewModels;

namespace Tavrynexo.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = AppHost.Services.GetRequiredService<InstalledAppsViewModel>();
    }
}
