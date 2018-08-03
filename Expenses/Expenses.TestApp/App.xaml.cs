using Expenses.ApiRepository;
using Expenses.TestApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unity;
using Unity.Injection;
using log4net;

namespace Expenses.TestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            base.OnStartup(e);

            var container = new UnityContainer();
            container.RegisterSingleton<Nawigacja>();
            container.RegisterSingleton<RejestracjaVm>();
            container.RegisterSingleton<MainWindowVm>();
            container.RegisterType<Repository>(
                new InjectionConstructor("http://localhost:7071/"));
                //new InjectionProperty("host", "http://localhost:7071/"));
            var nawigacja = container.Resolve<Nawigacja>();
            nawigacja.DomyslnyVm = container.Resolve<RejestracjaVm>();
            nawigacja.GlownyVm = container.Resolve<MainWindowVm>();

            var mainWindow = new MainWindow
            {
                DataContext = container.Resolve<MainWindowVm>()
            };
            mainWindow.Show();
        }
    }
}
