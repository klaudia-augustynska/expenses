using Expenses.TestApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace Expenses.TestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var container = new UnityContainer();
            container.RegisterSingleton<Nawigacja>();
            container.RegisterSingleton<RejestracjaVm>();
            container.RegisterSingleton<MainWindowVm>();
            var nawigacja = container.Resolve<Nawigacja>();
            nawigacja.DomyslnyVm = container.Resolve<RejestracjaVm>();
            nawigacja.GlownyVm = container.Resolve<MainWindowVm>();

            var mainWindow = new MainWindow();
            mainWindow.DataContext = container.Resolve<MainWindowVm>();
            mainWindow.Show();
        }
    }
}
