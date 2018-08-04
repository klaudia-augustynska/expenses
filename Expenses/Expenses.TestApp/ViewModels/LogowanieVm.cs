﻿using Expenses.ApiRepository;
using Expenses.Common;
using log4net;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Expenses.TestApp.ViewModels
{
    class LogowanieVm : BazowyVm
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Nawigacja _nawigacja;
        private readonly Repository _repozytorium;
        private readonly StronaGlownaVm _stronaGlownaVm;

        public LogowanieVm(Nawigacja nawigacja, Repository repozytorium, StronaGlownaVm stronaGlownaVm)
        {
            _nawigacja = nawigacja;
            _repozytorium = repozytorium;
            _stronaGlownaVm = stronaGlownaVm;

            Zaloguj = new DelegateCommand<PasswordBox>(ZalogujExecute, ZalogujCanExecute);
        }

        private string _login;
        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                NotifyPropertyChanged(nameof(Login));
                Zaloguj.RaiseCanExecuteChanged();
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
        public DelegateCommand<PasswordBox> Zaloguj { get; }
        private async void ZalogujExecute(PasswordBox passwordBox)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                PokazProgress = true;
            });

            string hasloZahaszowane = null;
            await Task.Run(async () =>
            {
                var solOdpowiedz = await _repozytorium.UsersRepository.GetSalt(Login);
                if (solOdpowiedz.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var sol = await solOdpowiedz.Content.ReadAsStringAsync();
                    hasloZahaszowane = HashUtil.Hash(passwordBox.Password, sol);
                    return await _repozytorium.UsersRepository.LogIn(Login, hasloZahaszowane);
                };
                throw new Exception("Źle skonstruowane zapytanie o sól");
            }).ContinueWith(async x =>
                {
                    if (x.Status == TaskStatus.RanToCompletion)
                    {
                        if (x.Result != null
                        && x.Result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            RegistryPomocnik.ZapiszKlucz(
                                nazwaKlucza: RegistryPomocnik.KluczUzytkownikaRegistryKey,
                                wartoscKlucza: await x.Result.Content.ReadAsStringAsync());
                            RegistryPomocnik.ZapiszKlucz(
                                nazwaKlucza: RegistryPomocnik.NazwaZalogowanegoUzytkownikaRegistryKey,
                                wartoscKlucza: Login);
                            RegistryPomocnik.ZapiszKlucz(
                                nazwaKlucza: RegistryPomocnik.ZahaszowaneHasloZalogowanegoUzytkownikaRegistryKey,
                                wartoscKlucza: hasloZahaszowane);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                PokazProgress = false;
                                _nawigacja.IdzDo(_stronaGlownaVm);
                                _nawigacja.KasujHistorie();
                            });
                        }
                        else
                        {
                            var blad = await x.Result.Content.ReadAsStringAsync();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                PokazProgress = false;
                                var msg = string.Format("Błąd logowania. Status http: {0}, Błąd: {1}",
                                    x.Result.StatusCode.ToString(),
                                    blad);
                                MessageBox.Show(msg);
                                Log.Debug(msg);
                            });
                        }
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            PokazProgress = false;
                            var msg = string.Format("Błąd logowania. Status: {0}, Błędy: {1}", x.Status, x.Exception);
                            MessageBox.Show(msg);
                            Log.Debug(msg);
                        });
                    }
                });
        }
        private bool ZalogujCanExecute(PasswordBox passwordBox)
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
            Zaloguj.RaiseCanExecuteChanged();
        }
    }
}
