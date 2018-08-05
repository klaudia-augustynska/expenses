using Expenses.ApiRepository;
using log4net;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Expenses.TestApp.ViewModels
{
    class ProfilVm : BazowyVm
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Nawigacja _nawigacja;
        private readonly Repository _repozytorium;
        private readonly StronaGlownaVm _stronaGlownaVm;

        public ProfilVm(Nawigacja nawigacja, Repository repozytorium, StronaGlownaVm stronaGlownaVm)
        {
            _repozytorium = repozytorium;
            _nawigacja = nawigacja;
            _stronaGlownaVm = stronaGlownaVm;
            UsunKonto = new DelegateCommand(UsunKontoExecute);
        }

        public DelegateCommand UsunKonto { get; }

        private async void UsunKontoExecute()
        {
            await _repozytorium.UsersRepository.Delete(
                login: RegistryPomocnik.NazwaZalogowanegoUzytkownika,
                key: RegistryPomocnik.KluczUzytkownika)
                .ContinueWith(x =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (x.Status == TaskStatus.RanToCompletion)
                        {
                            MessageBox.Show("Usunięto użytkowika. Nastąpi wylogowanie");
                            _stronaGlownaVm.Wyloguj.Execute();
                        }
                        else
                        {
                            Log.Info("Wystąpił jakiś błąd przy wylogowaniu.", x.Exception);
                            MessageBox.Show("Wystąpił jakiś błąd przy wylogowaniu. " + x.Exception.ToString());
                        }
                    });
                });
        }
    }
}
