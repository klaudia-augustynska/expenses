using Expenses.ApiRepository;
using Expenses.Model.Dto;
using Expenses.Model.Enums;
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
    class WstepnaKonfiguracjaVm : BazowyVm
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly Repository _repozytorium;

        public WstepnaKonfiguracjaVm(Nawigacja nawigacja, Repository repozytorium)
            : base(nawigacja)
        {
            _repozytorium = repozytorium;

            UzupelnijDane = new DelegateCommand(UzupelnijDaneExecute, UzupelnijDaneCanExecute);
        }

        private double _waga;
        public double Waga
        {
            get { return _waga; }
            set
            {
                _waga = value;
                NotifyPropertyChanged(nameof(Waga));
                UzupelnijDane.RaiseCanExecuteChanged();
            }
        }
        private double _wzrost;
        public double Wzrost
        {
            get { return _wzrost; }
            set
            {
                _wzrost = value;
                NotifyPropertyChanged(nameof(Wzrost));
                UzupelnijDane.RaiseCanExecuteChanged();
            }
        }

        private Plec _plec;
        public Plec Plec
        {
            get { return _plec; }
            set
            {
                _plec = value;
                NotifyPropertyChanged(nameof(Plec));
            }
        }

        private string _imie;
        public string Imie
        {
            get { return _imie; }
            set
            {
                _imie = value;
                NotifyPropertyChanged(nameof(Imie));
                UzupelnijDane.RaiseCanExecuteChanged();
            }
        }

        private decimal _bankPln;
        public decimal BankPln
        {
            get { return _bankPln; }
            set
            {
                _bankPln = value;
                NotifyPropertyChanged(nameof(BankPln));
            }
        }

        private decimal _gotowkaPln;
        public decimal GotowkaPln
        {
            get { return _gotowkaPln; }
            set
            {
                _gotowkaPln = value;
                NotifyPropertyChanged(nameof(GotowkaPln));
            }
        }

        private decimal _bankEur;
        public decimal BankEur
        {
            get { return _bankEur; }
            set
            {
                _bankEur = value;
                NotifyPropertyChanged(nameof(BankEur));
            }
        }

        private decimal _gotowkaEur;

        public decimal GotowkaEur
        {
            get { return _gotowkaEur; }
            set
            {
                _gotowkaEur = value;
                NotifyPropertyChanged(nameof(GotowkaEur));
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

        public DelegateCommand UzupelnijDane { get; }
        private async void UzupelnijDaneExecute()
        {
            PokazProgress = true;
            await _repozytorium.UsersRepository.ConfigureUser(
                login: RegistryPomocnik.NazwaZalogowanegoUzytkownika,
                key: RegistryPomocnik.KluczUzytkownika,
                configureUserDto: new ConfigureUserDto()
                {
                    Name = Imie,
                    Weight = Waga,
                    Height = Wzrost,
                    Sex = Plec == Plec.Kobieta ? Sex.Female : Sex.Male,
                    Wallets = new List<Model.Wallet>
                    {
                        new Model.Wallet()
                        {
                            Name = "Bank PLN",
                            Money = new Model.Money()
                            {
                                Amount = BankPln,
                                Currency = Currency.PLN
                            }
                        },
                        new Model.Wallet()
                        {
                            Name = "Gotówka PLN",
                            Money = new Model.Money()
                            {
                                Amount = GotowkaPln,
                                Currency = Currency.PLN
                            }
                        },
                        new Model.Wallet()
                        {
                            Name = "Bank EUR",
                            Money = new Model.Money()
                            {
                                Amount = BankEur,
                                Currency = Currency.EUR
                            }
                        },
                        new Model.Wallet()
                        {
                            Name = "Gotówka EUR",
                            Money = new Model.Money()
                            {
                                Amount = GotowkaEur,
                                Currency = Currency.EUR
                            }
                        }
                    }
                })
                .ContinueWith(x =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PokazProgress = false;
                        if (x.Status == TaskStatus.RanToCompletion)
                        {
                            RegistryPomocnik.CzySkonfigurowany = true;
                            _nawigacja.IdzDo<StronaGlownaVm>();
                            _nawigacja.KasujHistorie();
                        }
                        else
                        {
                            Log.Info("Błąd podczas próby dokonania wstępnej konfiguracji", x.Exception);
                            MessageBox.Show("Błąd podczas próby dokonania wstępnej konfiguracji: " + x.Exception.ToString());
                        }
                    });
                });
        }
        private bool UzupelnijDaneCanExecute()
        {
            return Waga > 0
                && Wzrost > 0
                && !string.IsNullOrEmpty(Imie);
        }
    }
    public enum Plec
    {
        Kobieta,
        Mezczyzna
    } 
}
