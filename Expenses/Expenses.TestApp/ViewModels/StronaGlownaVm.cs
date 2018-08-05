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
        private readonly Nawigacja _nawigacja;

        public StronaGlownaVm(Nawigacja nawigacja)
        {
            _nawigacja = nawigacja;

            PokazProfil = new DelegateCommand(PokazProfilExecute, PokazProfilCanExecute);
            PokazListeDlugow = new DelegateCommand(PokazListeDlugowExecute, PokazListeDlugowCanExecute);
            PokazListeStalychWydatkow = new DelegateCommand(PokazListeStalychWydatkowExecute, PokazListeStalychWydatkowCanExecute);
            PokazListeOsobWGospodarstwie = new DelegateCommand(PokazListeOsobWGospodarstwieExecute, PokazListeOsobWGospodarstwieCanExecute);
            PokazKategorie = new DelegateCommand(PokazKategorieExecute, PokazKategorieCanExecute);
            PokazHistorie = new DelegateCommand(PokazHistorieExecute, PokazHistorieCanExecute);
            PokazUstawienia = new DelegateCommand(PokazUstawieniaExecute, PokazUstawieniaCanExecute);
            Wyloguj = new DelegateCommand(WylogujExecute, WylogujCanExecute);
            DodajParagon = new DelegateCommand(DodajParagonExecute, DodajParagonCanExecute);
        }

        public override void PodczasLadowania(BazowyVm poprzedniaStrona)
        {
            base.PodczasLadowania(poprzedniaStrona);
        }

        public DelegateCommand PokazProfil { get; }
        
        private bool PokazProfilCanExecute()
        {
            return true;
        }

        private void PokazProfilExecute()
        {
            _nawigacja.IdzDo<ProfilVm>();
        }

        public DelegateCommand PokazListeDlugow { get; }

        private bool PokazListeDlugowCanExecute()
        {
            return false;
        }

        private void PokazListeDlugowExecute()
        {

        }
        
        public DelegateCommand PokazListeStalychWydatkow { get; }

        private bool PokazListeStalychWydatkowCanExecute()
        {
            return false;
        }

        private void PokazListeStalychWydatkowExecute()
        {

        }

        public DelegateCommand PokazListeOsobWGospodarstwie { get; }

        private bool PokazListeOsobWGospodarstwieCanExecute()
        {
            return false;
        }

        private void PokazListeOsobWGospodarstwieExecute()
        {

        }

        public DelegateCommand PokazKategorie { get; }

        private bool PokazKategorieCanExecute()
        {
            return false;
        }

        private void PokazKategorieExecute()
        {

        }

        public DelegateCommand PokazHistorie { get; }

        private bool PokazHistorieCanExecute()
        {
            return false;
        }

        private void PokazHistorieExecute()
        {

        }

        public DelegateCommand PokazUstawienia { get; }

        private bool PokazUstawieniaCanExecute()
        {
            return false;
        }

        private void PokazUstawieniaExecute()
        {

        }

        public DelegateCommand Wyloguj { get; }

        private bool WylogujCanExecute()
        {
            return true;
        }

        private void WylogujExecute()
        {
            RegistryPomocnik.CzyZalogowany = false;
            _nawigacja.IdzDo<LogowanieVm>();
            _nawigacja.KasujHistorie();
        }

        public DelegateCommand DodajParagon { get; }

        private bool DodajParagonCanExecute()
        {
            return false;
        }

        private void DodajParagonExecute()
        {

        }


    }
}
