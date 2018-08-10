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
        private const string DataOstatniegoPobieraniaWiadomosciRegistryKey = "DataOstatniegoPobieraniaWiadomosci";
        private const string GospodarstwoIdRegistryKey = "GospodarstwoId";

        public static string KluczUzytkownika
        {
            get { return CzytajKlucz(KluczUzytkownikaRegistryKey); }
            set { ZapiszKlucz(KluczUzytkownikaRegistryKey, value, nameof(KluczUzytkownika)); }
        }

        public static string NazwaZalogowanegoUzytkownika
        {
            get { return CzytajKlucz(NazwaZalogowanegoUzytkownikaRegistryKey); }
            set { ZapiszKlucz(NazwaZalogowanegoUzytkownikaRegistryKey, value, nameof(NazwaZalogowanegoUzytkownika)); }
        }

        public static string ZahaszowaneHasloZalogowanegoUzytkownika
        {
            get { return CzytajKlucz(ZahaszowaneHasloZalogowanegoUzytkownikaRegistryKey); }
            set { ZapiszKlucz(ZahaszowaneHasloZalogowanegoUzytkownikaRegistryKey, value, nameof(ZahaszowaneHasloZalogowanegoUzytkownika)); }
        }

        public static bool CzyZalogowany
        {
            get { return CzytajKlucz(CzyZalogowanyRegistryKey, domyslnaWartosc: false); }
            set { ZapiszKlucz(CzyZalogowanyRegistryKey, value, nameof(CzyZalogowany)); }
        }

        public static bool CzySkonfigurowany
        {
            get { return CzytajKlucz(CzySkonfigurowanyRegistryKey, domyslnaWartosc: false); }
            set { ZapiszKlucz(CzySkonfigurowanyRegistryKey, value, nameof(CzySkonfigurowany)); }
        }

        public static DateTime DataOstatniegoPobieraniaWiadomosci
        {
            get { return CzytajKlucz(DataOstatniegoPobieraniaWiadomosciRegistryKey, domyslnaWartosc: DateTime.MinValue); }
            set { ZapiszKlucz(DataOstatniegoPobieraniaWiadomosciRegistryKey, value, nameof(DataOstatniegoPobieraniaWiadomosci)); }
        }

        public static string GospodarstwoId
        {
            get { return CzytajKlucz(GospodarstwoIdRegistryKey); }
            set { ZapiszKlucz(GospodarstwoIdRegistryKey, value, nameof(GospodarstwoId)); }
        }

        private static void ZapiszKlucz(string nazwaKlucza, object wartoscKlucza, string nazwaWlasciwosci)
        {
            if (wartoscKlucza == null)
                wartoscKlucza = string.Empty;
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ExpensesTestApp");
            subKey.SetValue(nazwaKlucza, wartoscKlucza);
            subKey.Close();
            ZmianaWartosci?.Invoke(nazwaWlasciwosci);
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

        private static DateTime CzytajKlucz(string nazwaKlucza, DateTime domyslnaWartosc)
        {
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ExpensesTestApp");
            var value = subKey.GetValue(nazwaKlucza) as string;
            return value == null
                ? domyslnaWartosc
                : DateTime.Parse(value);
        }

        public static event Action<string> ZmianaWartosci;
    }
}
