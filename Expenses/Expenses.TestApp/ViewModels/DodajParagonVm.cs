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
            Waluty = Enum.GetValues(typeof(Currency))
                .Cast<Currency>()
                .Select(x => x.ToString())
                .ToList();
            Waluta = Waluty.FirstOrDefault();
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
                                _kategorieNaSerwerze = kategorie.Categories;
                                _kategorieNaSerwerze.ForEach(x => Kategorie.Add(x.Name));
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

        private int _dzien;
        public int Dzien
        {
            get { return _dzien; }
            set
            {
                _dzien = value;
                NotifyPropertyChanged(nameof(Dzien));
            }
        }

        private int _miesiac;
        public int Miesiac
        {
            get { return _miesiac; }
            set
            {
                _miesiac = value;
                NotifyPropertyChanged(nameof(Miesiac));
            }
        }

        private int _rok;
        public int Rok
        {
            get { return _rok; }
            set
            {
                _rok = value;
                NotifyPropertyChanged(nameof(Rok));
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

        public List<string> Waluty { get; }

        private string _waluta;
        public string Waluta
        {
            get { return _waluta; }
            set
            {
                _waluta = value;
                NotifyPropertyChanged(nameof(Waluta));
            }
        }

        public DelegateCommand DodajParagon { get; }
        private void DodajParagonExecute()
        {

        }
        private bool DodajParagonCanExecute()
        {
            return false;
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
