using Expenses.ApiRepository;
using Expenses.Common;
using Expenses.Model;
using Expenses.Model.Dto;
using log4net;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Expenses.TestApp.ViewModels
{
    class GospodarstwoVm : BazowyVm
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Repository _repozytorium;

        public GospodarstwoVm(Nawigacja nawigacja, Repository repozytorium) : base(nawigacja)
        {
            _repozytorium = repozytorium;
            WyslijZaproszenie = new DelegateCommand(WyslijZaproszenieExecute, WyslijZaproszenieCanExecute);
            RegistryPomocnik.ZmianaWartosci += RegistryPomocnik_ZmianaWartosci;
        }

        public override async void PodczasLadowania(BazowyVm poprzedniaStrona)
        {
            base.PodczasLadowania(poprzedniaStrona);
            if (CzyJest)
            {
                Osoby = await PobierzOsoby();
                NotifyPropertyChanged(nameof(Osoby));
            }
        }

        private async void RegistryPomocnik_ZmianaWartosci(string obj)
        {
            if (obj == nameof(RegistryPomocnik.GospodarstwoId)
                && _nawigacja.CzyWidoczny(this))
            {
                NotifyPropertyChanged(nameof(CzyJest));
                Osoby = await PobierzOsoby();
                NotifyPropertyChanged(nameof(Osoby));
            }
        }

        private string _loginOsoby;
        public string LoginOsoby
        {
            get { return _loginOsoby; }
            set
            {
                _loginOsoby = value;
                NotifyPropertyChanged(nameof(LoginOsoby));
                WyslijZaproszenie.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand WyslijZaproszenie { get; }

        private async void WyslijZaproszenieExecute()
        {
            await _repozytorium.HouseholdsRepository.Invite(
                invitersLogin: RegistryPomocnik.NazwaZalogowanegoUzytkownika,
                invitedLogin: LoginOsoby,
                key: RegistryPomocnik.KluczUzytkownika)
                .ContinueWith(x =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (x.Status == TaskStatus.RanToCompletion
                        && x.Result != null
                        && x.Result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            MessageBox.Show("Zaproszenie zostało wysłane do wybranego użytkownika. Poczekaj aż potwierdzi.");
                            LoginOsoby = string.Empty;
                        }
                        else if (x.Status == TaskStatus.RanToCompletion
                            && x.Result != null
                            && x.Result.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            var msg = $"Błąd przy zapraszaniu. StatusCode: {x.Result.StatusCode}, Message: {x.Result.Content.ReadAsStringAsync()}";
                            Log.Info(msg);
                            MessageBox.Show(msg);
                        }
                        else
                        {
                            Log.Info("Błąd przy zapraszaniu", x.Exception);
                            MessageBox.Show("Błąd przy zapraszaniu: " + x.Exception);
                        }
                    });
                });
        }

        private bool WyslijZaproszenieCanExecute()
        {
            return !string.IsNullOrEmpty(LoginOsoby);
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
        private bool _pokazProgress2;
        public bool PokazProgress2
        {
            get { return _pokazProgress2; }
            private set
            {
                _pokazProgress2 = value;
                NotifyPropertyChanged(nameof(PokazProgress2));
            }
        }

        public bool CzyJest
        {
            get { return !string.IsNullOrEmpty(RegistryPomocnik.GospodarstwoId); }
        }

        public List<Osoba> Osoby { get; private set; }

        private async Task<List<Osoba>> PobierzOsoby()
        {
            PokazProgress2 = true;
            List<Osoba> result = null;
            await _repozytorium.HouseholdsRepository.GetMembers(
                householdId: RegistryPomocnik.GospodarstwoId,
                key: RegistryPomocnik.KluczUzytkownika)
                .ContinueWith(task =>
                {
                    Application.Current.Dispatcher.Invoke(async () =>
                    {
                        PokazProgress2 = false;
                        if (task.Status == TaskStatus.RanToCompletion
                            && task.Result != null
                            && task.Result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var dto = await task.Result.Content.ReadAsDeserializedJson<GetMembersResponseDto>();
                            result = ZamienDtoNaListeOsob(dto);
                        }
                        else if (task.Status == TaskStatus.RanToCompletion
                            && task.Result != null
                            && task.Result.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            var msg = $"Nie można pobrać listy członków. Status: {task.Result.StatusCode.ToString()}, Błąd: {await task.Result.Content.ReadAsStringAsync()}";
                            Log.Error(msg);
                            MessageBox.Show(msg);
                        }
                        else
                        {
                            Log.Error("Nie można pobrać listy członków.", task.Exception);
                            MessageBox.Show($"Nie można pobrać listy członków. {task.Exception}");
                        }
                    });
                });
            return result;
        }

        private List<Osoba> ZamienDtoNaListeOsob(GetMembersResponseDto dto)
        {
            var list = new List<Osoba>();
            if (dto != null && dto.Members != null)
            {
                foreach (var member in dto.Members)
                {
                    list.Add(new Osoba()
                    {
                        Imie = member.Name,
                        Podsumowanie = PodsumowanieJakoString(member.WalletSummary)
                    });
                }
            }
            return list;
        }

        private string PodsumowanieJakoString(List<Money> walletSummary)
        {
            var sb = new StringBuilder();
            if (walletSummary != null)
            {
                foreach (var money in walletSummary)
                {
                    sb.AppendLine($"{money.Amount} {money.Currency.ToString()}");
                }
            }
            return sb.ToString();
        }
    }

    public class Osoba
    {
        public string Imie { get; set; }
        public string Podsumowanie { get; set; }
    }
}
