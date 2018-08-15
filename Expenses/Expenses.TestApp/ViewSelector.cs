using Expenses.TestApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Expenses.TestApp
{
    public class ViewSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is RejestracjaVm)
                return Rejestracja;
            if (item is LogowanieVm)
                return Logowanie;
            if (item is StronaGlownaVm)
                return StronaGlowna;
            if (item is ProfilVm)
                return Profil;
            if (item is WstepnaKonfiguracjaVm)
                return WstepnaKonfiguracja;
            if (item is WiadomosciVm)
                return Wiadomosci;
            if (item is GospodarstwoVm)
                return Gospodarstwo;
            if (item is KategorieVm)
                return Kategorie;
            if (item is DodajParagonVm)
                return DodajParagon;
            return base.SelectTemplate(item, container);
        }

        public DataTemplate Rejestracja { get; set; }
        public DataTemplate Logowanie { get; set; }
        public DataTemplate StronaGlowna { get; set; }
        public DataTemplate Profil { get; set; }
        public DataTemplate WstepnaKonfiguracja { get; set; }
        public DataTemplate Wiadomosci { get; set; }
        public DataTemplate Gospodarstwo { get; set; }
        public DataTemplate Kategorie { get; set; }
        public DataTemplate DodajParagon { get; set; }
    }
}
