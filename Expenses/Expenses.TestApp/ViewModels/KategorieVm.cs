using Expenses.ApiRepository;
using Expenses.Common;
using Expenses.Model;
using Expenses.Model.Dto;
using log4net;
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
    class KategorieVm : BazowyVm
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Repository _repozytorium;

        public KategorieVm(Nawigacja nawigacja, Repository repozytorium) : base(nawigacja)
        {
            _repozytorium = repozytorium;
            Kategorie = new ObservableCollection<Category>();
            DodajKategorie = new DelegateCommand(DodajKategorieExecute, DodajKategorieCanExecute);
        }

        public async override void PodczasLadowania(BazowyVm poprzedniaStrona)
        {
            base.PodczasLadowania(poprzedniaStrona);
            if (!_kategorieZaladowane)
            {
                await LadujKategorie();
            }
        }

        private async Task LadujKategorie()
        {
            PokazProgressLadujKategorie = true;
            Kategorie.Clear();
            await _repozytorium.CategoriesRepository.GetAll(RegistryPomocnik.NazwaZalogowanegoUzytkownika, RegistryPomocnik.KluczUzytkownika)
                .ContinueWith(async task =>
                {
                    if (task.Result != null
                        && task.Result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var dtoResponse = await task.Result.Content.ReadAsDeserializedJson<GetCategoriesResponseDto>();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            dtoResponse.Categories.ForEach(x => Kategorie.Add(x));
                            PokazProgressLadujKategorie = false;
                            _kategorieZaladowane = true;
                        });
                    }
                    else
                    {
                        MessageBox.Show("Coś poszło nie tak ale nie zapisaliśmy szczegółów bo mniej kodu");
                    }
                });

        }

        private bool _kategorieZaladowane;
        public ObservableCollection<Category> Kategorie { get; private set; }



        private bool _pokazProgressLadujKategorie;
        public bool PokazProgressLadujKategorie
        {
            get { return _pokazProgressLadujKategorie; }
            private set
            {
                _pokazProgressLadujKategorie = value;
                NotifyPropertyChanged(nameof(PokazProgressLadujKategorie));
            }
        }

        private bool _pokazProgressDodajKategorie;
        public bool PokazProgressDodajKategorie
        {
            get { return _pokazProgressDodajKategorie; }
            private set
            {
                _pokazProgressDodajKategorie = value;
                NotifyPropertyChanged(nameof(PokazProgressDodajKategorie));
            }
        }

        public DelegateCommand DodajKategorie { get; private set; }
        private async void DodajKategorieExecute()
        {
            PokazProgressDodajKategorie = true;
            await _repozytorium.CategoriesRepository.Add(RegistryPomocnik.NazwaZalogowanegoUzytkownika, RegistryPomocnik.KluczUzytkownika, NazwaKategorii)
                .ContinueWith(task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion
                    && task.Result != null
                    && task.Result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Kategorie.Add(new Category() { Name = NazwaKategorii });
                            NazwaKategorii = string.Empty;
                            PokazProgressDodajKategorie = false;
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("błąd");
                        });
                    }
                });
        }
        private bool DodajKategorieCanExecute()
        {
            return !string.IsNullOrEmpty(NazwaKategorii);
        }

        private string _nazwaKategorii;
        public string NazwaKategorii
        {
            get { return _nazwaKategorii; }
            set
            {
                _nazwaKategorii = value;
                NotifyPropertyChanged(nameof(NazwaKategorii));
                DodajKategorie.RaiseCanExecuteChanged();
            }
        }
    }
}
