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
        public StronaGlownaVm(Nawigacja nawigacja) : base(nawigacja)
        {
            PokazProfil = new DelegateCommand(PokazProfilExecute, () => true);
            PokazListeDlugow = new DelegateCommand(PokazListeDlugowExecute, () => false);
            PokazListeStalychWydatkow = new DelegateCommand(PokazListeStalychWydatkowExecute, () => false);
            PokazListeOsobWGospodarstwie = new DelegateCommand(PokazListeOsobWGospodarstwieExecute, () => true);
            PokazKategorie = new DelegateCommand(PokazKategorieExecute, () => false);
            PokazWiadomosci = new DelegateCommand(PokazWiadomosciExecute, () => true);
            PokazHistorie = new DelegateCommand(PokazHistorieExecute, () => false);
            PokazUstawienia = new DelegateCommand(PokazUstawieniaExecute, () => false);
            Wyloguj = new DelegateCommand(WylogujExecute, () => true);
            DodajParagon = new DelegateCommand(DodajParagonExecute, () => false);
        }

        public override void PodczasLadowania(BazowyVm poprzedniaStrona)
        {
            base.PodczasLadowania(poprzedniaStrona);
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
            _nawigacja.IdzDo<LogowanieVm>();
            _nawigacja.KasujHistorie();
        }

        public DelegateCommand DodajParagon { get; }

        private void DodajParagonExecute()
        {

        }


    }
}
