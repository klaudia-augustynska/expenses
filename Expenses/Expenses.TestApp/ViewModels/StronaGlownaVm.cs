using Expenses.ApiRepository;
using Expenses.Common;
using Expenses.Model;
using Expenses.Model.Dto;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Expenses.TestApp.ViewModels
{
    class StronaGlownaVm : BazowyVm
    {
        private readonly Repository _repozytorium;

        public StronaGlownaVm(Nawigacja nawigacja, Repository repozytorium) : base(nawigacja)
        {
            _repozytorium = repozytorium;
            PokazProfil = new DelegateCommand(PokazProfilExecute, () => true);
            PokazListeDlugow = new DelegateCommand(PokazListeDlugowExecute, () => false);
            PokazListeStalychWydatkow = new DelegateCommand(PokazListeStalychWydatkowExecute, () => false);
            PokazListeOsobWGospodarstwie = new DelegateCommand(PokazListeOsobWGospodarstwieExecute, () => true);
            PokazKategorie = new DelegateCommand(PokazKategorieExecute, () => true);
            PokazWiadomosci = new DelegateCommand(PokazWiadomosciExecute, () => true);
            PokazHistorie = new DelegateCommand(PokazHistorieExecute, () => false);
            PokazUstawienia = new DelegateCommand(PokazUstawieniaExecute, () => false);
            Wyloguj = new DelegateCommand(WylogujExecute, () => true);
            DodajParagon = new DelegateCommand(DodajParagonExecute, () => true);
            Odswiez = new DelegateCommand(OdswiezExecute);
        }

        public override void PodczasLadowania(BazowyVm poprzedniaStrona)
        {
            base.PodczasLadowania(poprzedniaStrona);
            if (!CzyPobralo)
            {
                OdswiezExecute();
            }
        }

        public DelegateCommand PokazProfil { get; }

        private void PokazProfilExecute()
        {
            _nawigacja.IdzDo<ProfilVm>();
        }

        public DelegateCommand PokazListeDlugow { get; }

        private void PokazListeDlugowExecute()
        {

        }
        
        public DelegateCommand PokazListeStalychWydatkow { get; }

        private void PokazListeStalychWydatkowExecute()
        {

        }

        public DelegateCommand PokazListeOsobWGospodarstwie { get; }

        private void PokazListeOsobWGospodarstwieExecute()
        {
            _nawigacja.IdzDo<GospodarstwoVm>();
        }

        public DelegateCommand PokazKategorie { get; }

        private void PokazKategorieExecute()
        {
            _nawigacja.IdzDo<KategorieVm>();
        }

        public DelegateCommand PokazWiadomosci { get; }

        private void PokazWiadomosciExecute()
        {
            _nawigacja.IdzDo<WiadomosciVm>();
        }

        public DelegateCommand PokazHistorie { get; }

        private void PokazHistorieExecute()
        {

        }

        public DelegateCommand PokazUstawienia { get; }

        private void PokazUstawieniaExecute()
        {

        }

        public DelegateCommand Wyloguj { get; }

        private void WylogujExecute()
        {
            RegistryPomocnik.CzyZalogowany = false;
            RegistryPomocnik.CzySkonfigurowany = false;
            RegistryPomocnik.KluczUzytkownika = string.Empty;
            RegistryPomocnik.NazwaZalogowanegoUzytkownika = string.Empty;
            RegistryPomocnik.ZahaszowaneHasloZalogowanegoUzytkownika = string.Empty;
            RegistryPomocnik.GospodarstwoId = string.Empty;
            RegistryPomocnik.CzyNalezyDoGospodarstwa = false;
            CzyPobralo = false;
            _nawigacja.IdzDo<LogowanieVm>();
            _nawigacja.KasujHistorie();
        }

        public DelegateCommand DodajParagon { get; }

        private void DodajParagonExecute()
        {
            _nawigacja.IdzDo<DodajParagonVm>();
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
        
        private bool _czyPobralo;
        public bool CzyPobralo
        {
            get { return _czyPobralo; }
            private set
            {
                _czyPobralo = value;
                NotifyPropertyChanged(nameof(CzyPobralo));
            }
        }

        private bool _czyBlad;
        public bool CzyBlad
        {
            get { return _czyBlad; }
            private set
            {
                _czyBlad = value;
                NotifyPropertyChanged(nameof(CzyBlad));
            }
        }

        private string _wydatkiDomu;
        public string WydatkiDomu
        {
            get { return _wydatkiDomu; }
            private set
            {
                _wydatkiDomu = value;
                NotifyPropertyChanged(nameof(WydatkiDomu));
            }
        }

        private string _pieniadzeDomu;
        public string PieniadzeDomu
        {
            get { return _pieniadzeDomu; }
            private set
            {
                _pieniadzeDomu = value;
                NotifyPropertyChanged(nameof(PieniadzeDomu));
            }
        }

        private string _wydatkiUz;
        public string WydatkiUz
        {
            get { return _wydatkiUz; }
            private set
            {
                _wydatkiUz = value;
                NotifyPropertyChanged(nameof(WydatkiUz));
            }
        }

        private string _pieniadzeUz;
        public string PieniadzeUz
        {
            get { return _pieniadzeUz; }
            private set
            {
                _pieniadzeUz = value;
                NotifyPropertyChanged(nameof(PieniadzeUz));
            }
        }

        public string Login
        {
            get { return RegistryPomocnik.NazwaZalogowanegoUzytkownika; }
        }

        public DelegateCommand Odswiez { get; private set; }
        private async void OdswiezExecute()
        {
            var date = DateTime.Now;
            PokazProgress = true;
            await _repozytorium.CashFlowsRepository.GetSummary(
                RegistryPomocnik.GospodarstwoId,
                RegistryPomocnik.NazwaZalogowanegoUzytkownika,
                new DateTime(date.Year, date.Month, 1),
                date,
                RegistryPomocnik.KluczUzytkownika)
                .ContinueWith(async task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        var result = task.Result;
                        if (result != null)
                        {
                            var dto = await result.Content.ReadAsDeserializedJson<GetCashSummaryResponseDto>();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                PieniadzeDomu = Formatuj(dto.HouseholdMoney);
                                WydatkiDomu = Formatuj(dto.HouseholdExpenses);
                                PieniadzeUz = Formatuj(dto.UserWallets);
                                WydatkiUz = Formatuj(dto.UserExpenses);
                                CzyPobralo = true;
                                CzyBlad = false;
                            });
                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                CzyPobralo = false;
                                CzyBlad = true;
                            });
                        }
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CzyPobralo = false;
                            CzyBlad = true;
                        });
                    }
                    PokazProgress = false;
                });
        }

        private string Formatuj(List<Wallet> list)
        {
            var sb = new StringBuilder();
            if (list != null)
            {
                var i = 0;
                foreach (var item in list)
                {
                    if (i++ > 0)
                        sb.Append(", ");
                    sb.Append(item.Name);
                    sb.Append(": ");
                    sb.Append(item.Money.Amount);
                    sb.Append(' ');
                    sb.Append(item.Money.Currency.ToString());
                }
            }
            return sb.ToString();
        }

        private string Formatuj(List<Money> list)
        {
            var sb = new StringBuilder();
            if (list != null)
            {
                var i = 0;
                foreach (var item in list)
                {
                    if (i++ > 0)
                        sb.Append(", ");
                    sb.Append(item.Amount);
                    sb.Append(' ');
                    sb.Append(item.Currency.ToString());
                }
            }
            return sb.ToString();
        }
    }
}
