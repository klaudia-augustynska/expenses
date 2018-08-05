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
            var nawigacja = new Nawigacja(container);
            container.RegisterInstance(nawigacja);
            container.RegisterSingleton<RejestracjaVm>();
            container.RegisterSingleton<LogowanieVm>();
            container.RegisterSingleton<StronaGlownaVm>();
            container.RegisterSingleton<ProfilVm>();
            container.RegisterSingleton<MainWindowVm>();
            container.RegisterType<Repository>(
                new InjectionConstructor("http://localhost:7071/"));
            
            var czyZalogowany = RegistryPomocnik.CzyZalogowany;
            nawigacja.DomyslnyVm = czyZalogowany
                ? container.Resolve<StronaGlownaVm>() as BazowyVm
                : container.Resolve<RejestracjaVm>();
            nawigacja.GlownyVm = container.Resolve<MainWindowVm>();

            var mainWindow = new MainWindow
            {
                DataContext = container.Resolve<MainWindowVm>()
            };
            mainWindow.Show();
        }
    }
}
