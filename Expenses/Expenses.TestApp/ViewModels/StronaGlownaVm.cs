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
        private string klucz;

        public override void PodczasLadowania(BazowyVm poprzedniaStrona)
        {
            base.PodczasLadowania(poprzedniaStrona);
            if (poprzedniaStrona is LogowanieVm)
            {
                klucz = RegistryPomocnik.CzytajKlucz<string>(RegistryPomocnik.KluczUzytkownikaRegistryKey);
                MessageBox.Show(klucz);
            }
        }
    }
}
