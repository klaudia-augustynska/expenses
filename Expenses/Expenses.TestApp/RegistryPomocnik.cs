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
        private const string CzySkonfigurowanyRegistryKey = "CzySkonfigurowany";

        public static string KluczUzytkownika
        {
            get { return CzytajKlucz(KluczUzytkownikaRegistryKey); }
            set { ZapiszKlucz(KluczUzytkownikaRegistryKey, value); }
        }

        public static string NazwaZalogowanegoUzytkownika
        {
            get { return CzytajKlucz(NazwaZalogowanegoUzytkownikaRegistryKey); }
            set { ZapiszKlucz(NazwaZalogowanegoUzytkownikaRegistryKey, value); }
        }

        public static string ZahaszowaneHasloZalogowanegoUzytkownika
        {
            get { return CzytajKlucz(ZahaszowaneHasloZalogowanegoUzytkownikaRegistryKey); }
            set { ZapiszKlucz(ZahaszowaneHasloZalogowanegoUzytkownikaRegistryKey, value); }
        }

        public static bool CzyZalogowany
        {
            get { return CzytajKlucz(CzyZalogowanyRegistryKey, domyslnaWartosc: false); }
            set { ZapiszKlucz(CzyZalogowanyRegistryKey, value); }
        }

        public static bool CzySkonfigurowany
        {
            get { return CzytajKlucz(CzySkonfigurowanyRegistryKey, domyslnaWartosc: false); }
            set { ZapiszKlucz(CzySkonfigurowanyRegistryKey, value); }
        }

        private static void ZapiszKlucz(string nazwaKlucza, object wartoscKlucza)
        {
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ExpensesTestApp");
            subKey.SetValue(nazwaKlucza, wartoscKlucza);
            subKey.Close();
        }

        private static string CzytajKlucz(string nazwaKlucza) 
        {
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ExpensesTestApp");
            return subKey.GetValue(nazwaKlucza) as string;
        }

        private static bool CzytajKlucz(string nazwaKlucza, bool domyslnaWartosc)
        {
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ExpensesTestApp");
            var value = subKey.GetValue(nazwaKlucza) as string;
            return value == null
                ? domyslnaWartosc
                : value == "True";
        }
    }
}
