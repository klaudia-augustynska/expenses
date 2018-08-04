using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.TestApp
{
    static class RegistryPomocnik
    {
        public const string KluczUzytkownikaRegistryKey = "KluczZalogowanegoUzytkownika";
        public const string NazwaZalogowanegoUzytkownikaRegistryKey = "NazwaZalogowanegoUzytkownika";
        public const string ZahaszowaneHasloZalogowanegoUzytkownikaRegistryKey = "ZahaszowaneHasloZalogowanegoUzytkownika";

        public static void ZapiszKlucz(string nazwaKlucza, object wartoscKlucza)
        {
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ExpensesTestApp");
            subKey.SetValue(nazwaKlucza, wartoscKlucza);
            subKey.Close();
        }

        public static T CzytajKlucz<T>(string nazwaKlucza) where T : class
        {
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ExpensesTestApp");
            return (T)(subKey.GetValue(nazwaKlucza));
        }

        public static T CzytajKlucz<T>(string nazwaKlucza, T domyslnaWartosc) where T : struct
        {
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ExpensesTestApp");
            var value = subKey.GetValue(nazwaKlucza);
            return value != null ? (T)value : domyslnaWartosc;
        }
    }
}
