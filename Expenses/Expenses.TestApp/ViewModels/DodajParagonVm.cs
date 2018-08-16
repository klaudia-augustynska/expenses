using Expenses.ApiRepository;
using Expenses.Common;
using Expenses.Model;
using Expenses.Model.Dto;
using Expenses.Model.Enums;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Expenses.TestApp.ViewModels
{
    class DodajParagonVm : BazowyVm
    {
        private readonly Repository _repozytorium;
        public DodajParagonVm(Nawigacja nawigacja, Repository repository) : base(nawigacja)
        {
            _repozytorium = repository;
            WydatekNowy = new Wydatek();
            Wydatki = new ObservableCollection<Wydatek>();
            DodajWydatek = new DelegateCommand(DodajWydatekExecute, DodajWydatekCanExecute);
            DodajParagon = new DelegateCommand(DodajParagonExecute, DodajParagonCanExecute);
        }

        public override async void PodczasLadowania(BazowyVm poprzedniaStrona)
        {
            base.PodczasLadowania(poprzedniaStrona);
            // Note: w prawdziwym życiu to tak nie może być.
            // Za dużo transferu by zjadało.
            // Ale do appki testowej robienie cache jest imo bez sensu :-)
            if (Kategorie == null)
            {
                Kategorie = new List<string>();
                await _repozytorium.CategoriesRepository.GetAll(RegistryPomocnik.NazwaZalogowanegoUzytkownika, RegistryPomocnik.KluczUzytkownika)
                    .ContinueWith(async task =>
                    {
                        if (task.Status == TaskStatus.RanToCompletion)
                        {
                            var response = task.Result;
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                var kategorie = await response.Content.ReadAsDeserializedJson<GetCategoriesResponseDto>();
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    _kategorieNaSerwerze = kategorie.Categories;
                                    _kategorieNaSerwerze.ForEach(x => Kategorie.Add(x.Name));
                                });
                            }
                            else
                            {
                                MessageBox.Show("Błąd http");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Błąd taska");
                        }
                    });
            }

            if (Wallets == null)
            {
                await _repozytorium.UsersRepository.GetWallets(RegistryPomocnik.NazwaZalogowanegoUzytkownika, RegistryPomocnik.GospodarstwoId, RegistryPomocnik.KluczUzytkownika)
                    .ContinueWith(async task =>
                    {
                        if (task.Status == TaskStatus.RanToCompletion)
                        {
                            var response = task.Result;
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                var wallets = await response.Content.ReadAsDeserializedJson<List<Wallet>>();
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    Wallets = wallets;
                                });
                            }
                            else
                            {
                                MessageBox.Show("Błąd http");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Błąd taska");
                        }
                    });
            }
        }

        private int _dzien = DateTime.Now.Day;
        public int Dzien
        {
            get { return _dzien; }
            set
            {
                _dzien = value;
                NotifyPropertyChanged(nameof(Dzien));
                DodajParagon.RaiseCanExecuteChanged();
            }
        }

        private int _miesiac = DateTime.Now.Month;
        public int Miesiac
        {
            get { return _miesiac; }
            set
            {
                _miesiac = value;
                NotifyPropertyChanged(nameof(Miesiac));
                DodajParagon.RaiseCanExecuteChanged();
            }
        }

        private int _rok = DateTime.Now.Year;
        public int Rok
        {
            get { return _rok; }
            set
            {
                _rok = value;
                NotifyPropertyChanged(nameof(Rok));
                DodajParagon.RaiseCanExecuteChanged();
            }
        }

        private List<Category> _kategorieNaSerwerze;
        private List<string> _kategorie;
        public List<string> Kategorie
        {
            get { return _kategorie; }
            set
            {
                _kategorie = value;
                NotifyPropertyChanged(nameof(Kategorie));
            }
        }

        private string _wybranaKategoria;
        public string WybranaKategoria
        {
            get { return _wybranaKategoria; }
            set
            {
                _wybranaKategoria = value;
                NotifyPropertyChanged(nameof(WybranaKategoria));
                DodajParagon.RaiseCanExecuteChanged();
            }
        }

        private decimal _ile;
        public decimal Ile
        {
            get { return _ile; }
            set
            {
                _ile = value;
                NotifyPropertyChanged(nameof(Ile));
                DodajParagon.RaiseCanExecuteChanged();
            }
        }

        private ObservableCollection<Wydatek> _wydatki;
        public ObservableCollection<Wydatek> Wydatki
        {
            get { return _wydatki; }
            set
            {
                _wydatki = value;
                NotifyPropertyChanged(nameof(Wydatki));
            }
        }

        private Wydatek _wydatekNowy;
        public Wydatek WydatekNowy
        {
            get { return _wydatekNowy; }
            private set
            {
                _wydatekNowy = value;
                NotifyPropertyChanged(nameof(WydatekNowy));
                _wydatekNowy.PropertyChanged += (x, y) =>
                {
                    DodajWydatek.RaiseCanExecuteChanged();
                };
            }
        }

        public DelegateCommand DodajWydatek { get; }
        private void DodajWydatekExecute()
        {
            Wydatki.Add(WydatekNowy);
            WydatekNowy = new Wydatek();
        }
        private bool DodajWydatekCanExecute()
        {
            return WydatekNowy != null
                && WydatekNowy.Ile > 0
                && Kategorie.Contains(WydatekNowy.WybranaKategoria);
        }

        public DelegateCommand DodajParagon { get; }
        private async void DodajParagonExecute()
        {
            var dto = new AddCashFlowDto()
            {
                Amount = new Money()
                {
                    Amount = Ile,
                    Currency = Wallets.First(x => x.Guid == Portfel).Money.Currency
                },
                CategoryGuid = _kategorieNaSerwerze.First(x => x.Name == WybranaKategoria).Guid,
                DateTime = new DateTime(Rok, Miesiac, Dzien),
                Details = InneWydatkiNaListeDto(Wydatki),
                WalletGuid = Portfel
            };
            await _repozytorium.CashFlowsRepository.Add(RegistryPomocnik.NazwaZalogowanegoUzytkownika, RegistryPomocnik.KluczUzytkownika, dto)
                .ContinueWith(task =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (task.Status == TaskStatus.RanToCompletion)
                        {
                            var result = task.Result;
                            if (result != null
                            && result.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                MessageBox.Show("dodało się");
                            }
                            else
                            {
                                MessageBox.Show("błąd http");
                            }
                        }
                        else
                        {
                            MessageBox.Show("błąd taska");
                        }
                    });
                });
        }

        private List<CashFlowDetail> InneWydatkiNaListeDto(ObservableCollection<Wydatek> wydatki)
        {
            var result = new List<CashFlowDetail>();
            foreach (var item in wydatki)
            {
                result.Add(new CashFlowDetail()
                {
                    Amount = item.Ile,
                    CategoryGuid = _kategorieNaSerwerze.First(x => x.Name == item.WybranaKategoria).Guid,
                    Comment = item.Komentarz
                });
            }
            return result;
        }

        private bool DodajParagonCanExecute()
        {
            return Rok > 0
                && !string.IsNullOrEmpty(WybranaKategoria)
                && Ile > 0;
        }

        private List<Wallet> _wallets;
        public List<Wallet> Wallets
        {
            get { return _wallets; }
            set
            {
                _wallets = value;
                NotifyPropertyChanged(nameof(Wallets));
            }
        }

        private Guid _portfel;
        public Guid Portfel
        {
            get { return _portfel; }
            set
            {
                _portfel = value;
                NotifyPropertyChanged(nameof(Portfel));
            }
        }
    }

    public class Wydatek : PropertyChangedVm
    {
        private string _wybranaKategoria;
        public string WybranaKategoria
        {
            get { return _wybranaKategoria; }
            set
            {
                _wybranaKategoria = value;
                NotifyPropertyChanged(nameof(WybranaKategoria));
            }
        }

        private decimal _ile;
        public decimal Ile
        {
            get { return _ile; }
            set
            {
                _ile = value;
                NotifyPropertyChanged(nameof(Ile));
            }
        }

        private string _komentarz;
        public string Komentarz
        {
            get { return _komentarz; }
            set
            {
                _komentarz = value;
                NotifyPropertyChanged(nameof(Komentarz));
            }
        }
    }
}
