﻿using Expenses.ApiRepository;
using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Controls;
using log4net;

namespace Expenses.TestApp.ViewModels
{
    class RejestracjaVm : BazowyVm
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Nawigacja _nawigacja;
        private readonly Repository _repozytorium;

        public RejestracjaVm(Nawigacja navigationService, LogowanieVm logowanieVm, Repository repozytorium)
        {
            _nawigacja = navigationService;
            _repozytorium = repozytorium;
            Zarejestruj = new DelegateCommand<PasswordBox>(ZarejestrujExecute, ZarejestrujCanExecute);
            PrzejdzDoLogowania = new DelegateCommand(PrzejdzDoLogowaniaExecute, PrzejdzDoLogowaniaCanExecute);
            LogowanieVm = logowanieVm;
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

        private LogowanieVm LogowanieVm { get; }

        private bool czySledziszZmianyWPasswordBox;
        public DelegateCommand<PasswordBox> Zarejestruj { get; }
        private async void ZarejestrujExecute(PasswordBox passwordBox)
        {
            Application.Current.Dispatcher.Invoke(() => {
                PokazProgress = true;
            });
            await _repozytorium.UsersRepository.Add(Login, passwordBox.Password)
                .ContinueWith(x =>
                {
                    if (x.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            PokazProgress = false;
                            _nawigacja.IdzDo(LogowanieVm);
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            PokazProgress = false;
                            var msg = string.Format("Błąd logowania. Status: {0}, Błędy: {1}", x.Status, x.Exception);
                            MessageBox.Show(msg);
                            Log.Debug(msg);
                        });
                    }
                });
        }
        private bool ZarejestrujCanExecute(PasswordBox passwordBox)
        {
            if (passwordBox != null && !czySledziszZmianyWPasswordBox)
            {
                passwordBox.PasswordChanged += (x, y) => Zarejestruj.RaiseCanExecuteChanged();
                czySledziszZmianyWPasswordBox = true;
            }

            return !string.IsNullOrEmpty(Login)
                && !string.IsNullOrEmpty(passwordBox.Password);
        }

        public DelegateCommand PrzejdzDoLogowania { get; }
        private void PrzejdzDoLogowaniaExecute()
        {
            _nawigacja.IdzDo(LogowanieVm);
        }
        private bool PrzejdzDoLogowaniaCanExecute()
        {
            return _nawigacja != null && LogowanieVm != null;
        }
    }
}
