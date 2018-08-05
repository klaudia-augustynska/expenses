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
        private const string KluczUzytkownikaRegistryKey = "KluczZalogowanegoUzytkownika";
        private const string NazwaZalogowanegoUzytkownikaRegistryKey = "NazwaZalogowanegoUzytkownika";
        private const string ZahaszowaneHasloZalogowanegoUzytkownikaRegistryKey = "ZahaszowaneHasloZalogowanegoUzytkownika";
        private const string CzyZalogowanyRegistryKey = "CzyZalogowany";

        public static string KluczUzytkownika
        {
            get { return CzytajKlucz<string>(KluczUzytkownikaRegistryKey); }
            set { ZapiszKlucz(KluczUzytkownikaRegistryKey, value); }
        }

        public static string NazwaZalogowanegoUzytkownika
        {
            get { return CzytajKlucz<string>(NazwaZalogowanegoUzytkownikaRegistryKey); }
            set { ZapiszKlucz(NazwaZalogowanegoUzytkownikaRegistryKey, value); }
        }

        public static string ZahaszowaneHasloZalogowanegoUzytkownika
        {
            get { return CzytajKlucz<string>(ZahaszowaneHasloZalogowanegoUzytkownikaRegistryKey); }
            set { ZapiszKlucz(ZahaszowaneHasloZalogowanegoUzytkownikaRegistryKey, value); }
        }

        public static bool CzyZalogowany
        {
            get { return CzytajKlucz<bool>(CzyZalogowanyRegistryKey, domyslnaWartosc: false); }
            set { ZapiszKlucz(CzyZalogowanyRegistryKey, value); }
        }

        private static void ZapiszKlucz(string nazwaKlucza, object wartoscKlucza)
        {
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ExpensesTestApp");
            subKey.SetValue(nazwaKlucza, wartoscKlucza);
            subKey.Close();
        }

        private static T CzytajKlucz<T>(string nazwaKlucza) where T : class
        {
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ExpensesTestApp");
            return (T)(subKey.GetValue(nazwaKlucza));
        }

        private static T CzytajKlucz<T>(string nazwaKlucza, T domyslnaWartosc) where T : struct
        {
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ExpensesTestApp");
            var value = subKey.GetValue(nazwaKlucza);
            return value != null ? (T)value : domyslnaWartosc;
        }
    }
}
