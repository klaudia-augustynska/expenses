using Expenses.ApiRepository;
using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Controls;
using log4net;
using Expenses.Common;

namespace Expenses.TestApp.ViewModels
{
    class RejestracjaVm : BazowyVm
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly Repository _repozytorium;

        public RejestracjaVm(Nawigacja nawigacja, Repository repozytorium)
            : base(nawigacja)
        {
            _nawigacja.ZmianaIlosciOtwartychStron += 
                () => Zarejestruj.RaiseCanExecuteChanged();
            _repozytorium = repozytorium;

            Zarejestruj = new DelegateCommand<PasswordBox>(ZarejestrujExecute, ZarejestrujCanExecute);
            PrzejdzDoLogowania = new DelegateCommand(PrzejdzDoLogowaniaExecute, PrzejdzDoLogowaniaCanExecute);
        }

        private string _login;
        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                NotifyPropertyChanged(nameof(Login));
                Zarejestruj.RaiseCanExecuteChanged();
            }
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

        private PasswordBox ostatnioSledzonyPasswordBox;
        public DelegateCommand<PasswordBox> Zarejestruj { get; }
        private async void ZarejestrujExecute(PasswordBox passwordBox)
        {
            Application.Current.Dispatcher.Invoke(() => {
                PokazProgress = true;
            });
            var sol = HashUtil.GenerateSalt();
            var zahaszowaneHaslo = HashUtil.Hash(passwordBox.Password, sol);
            await _repozytorium.UsersRepository.Add(Login, zahaszowaneHaslo, sol)
                .ContinueWith(async x =>
                {
                    if (x.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                    {
                        if (x.Result != null 
                        && x.Result.StatusCode == System.Net.HttpStatusCode.Created)
                        {
                            Application.Current.Dispatcher.Invoke(() => {
                                PokazProgress = false;
                                _nawigacja.IdzDo<LogowanieVm>();
                            });
                        }
                        else
                        {
                            var blad = await x.Result.Content.ReadAsStringAsync();
                            Application.Current.Dispatcher.Invoke(() => {
                                PokazProgress = false;
                                var msg = string.Format("Nie można utworzyć użytkownika. Status http: {0}, Błąd: {1}",
                                    x.Result.StatusCode.ToString(),
                                    blad);
                                MessageBox.Show(msg);
                                Log.Debug(msg);
                            });
                        }
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            PokazProgress = false;
                            var msg = string.Format("Błąd przy tworzeniu użytkownika. Status: {0}, Błędy: {1}", x.Status, x.Exception);
                            MessageBox.Show(msg);
                            Log.Debug(msg);
                        });
                    }
                });
        }
        private bool ZarejestrujCanExecute(PasswordBox passwordBox)
        {
            if (passwordBox != ostatnioSledzonyPasswordBox)
            {
                if (ostatnioSledzonyPasswordBox != null)
                {
                    ostatnioSledzonyPasswordBox.PasswordChanged -= OstatnioSledzonyPasswordBox_PasswordChanged;
                }
                if (passwordBox != null)
                {
                    passwordBox.PasswordChanged += OstatnioSledzonyPasswordBox_PasswordChanged;
                }
                ostatnioSledzonyPasswordBox = passwordBox;
            }

            return !string.IsNullOrEmpty(Login)
                && !string.IsNullOrEmpty(passwordBox.Password);
        }

        private void OstatnioSledzonyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Zarejestruj.RaiseCanExecuteChanged();
        }

        public DelegateCommand PrzejdzDoLogowania { get; }
        private void PrzejdzDoLogowaniaExecute()
        {
            _nawigacja.IdzDo<LogowanieVm>();
        }
        private bool PrzejdzDoLogowaniaCanExecute()
        {
            return _nawigacja != null;
        }
    }
}
