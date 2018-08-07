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
    class GospodarstwoVm : BazowyVm
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Repository _repozytorium;

        public GospodarstwoVm(Nawigacja nawigacja, Repository repozytorium) : base(nawigacja)
        {
            _repozytorium = repozytorium;
            WyslijZaproszenie = new DelegateCommand(WyslijZaproszenieExecute, WyslijZaproszenieCanExecute);
        }

        private string _loginOsoby;
        public string LoginOsoby
        {
            get { return _loginOsoby; }
            set
            {
                _loginOsoby = value;
                NotifyPropertyChanged(nameof(LoginOsoby));
                WyslijZaproszenie.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand WyslijZaproszenie { get; }

        private async void WyslijZaproszenieExecute()
        {
            await _repozytorium.HouseholdsRepository.Invite(
                invitersLogin: RegistryPomocnik.NazwaZalogowanegoUzytkownika,
                invitedLogin: LoginOsoby,
                key: RegistryPomocnik.KluczUzytkownika)
                .ContinueWith(x =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (x.Status == TaskStatus.RanToCompletion
                        && x.Result != null
                        && x.Result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            MessageBox.Show("Zaproszenie zostało wysłane do wybranego użytkownika. Poczekaj aż potwierdzi.");
                            LoginOsoby = string.Empty;
                        }
                        else if (x.Status == TaskStatus.RanToCompletion
                            && x.Result != null
                            && x.Result.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            var msg = $"Błąd przy zapraszaniu. StatusCode: {x.Result.StatusCode}, Message: {x.Result.Content.ReadAsStringAsync()}";
                            Log.Info(msg);
                            MessageBox.Show(msg);
                        }
                        else
                        {
                            Log.Info("Błąd przy zapraszaniu", x.Exception);
                            MessageBox.Show("Błąd przy zapraszaniu: " + x.Exception);
                        }
                    });
                });
        }

        private bool WyslijZaproszenieCanExecute()
        {
            return !string.IsNullOrEmpty(LoginOsoby);
        }

        private bool _pokazProgress;
        public bool PokazProgress
        {
            get { return _pokazProgress; }
            private set
            {
                _pokazProgress = value;
                NotifyPropertyChanged(nameof(PokazProgress));
            }
        }
    }
}
