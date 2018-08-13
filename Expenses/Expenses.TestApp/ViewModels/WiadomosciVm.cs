using Expenses.ApiRepository;
using Expenses.Common;
using Expenses.Model;
using Expenses.Model.Dto;
using log4net;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Expenses.TestApp.ViewModels
{
    class WiadomosciVm : BazowyVm
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Repository _repozytorium;

        public WiadomosciVm(Nawigacja nawigacja, Repository repozytorium) : base(nawigacja)
        {
            _repozytorium = repozytorium;
            Potwierdz = new DelegateCommand<string>(PotwierdzExecute, PotwierdzCanExecute);
            potwierdzRegex = new Regex(@"\/[a-z]+\/households\/accept\/(?<from>[a-z_]+)\/(?<to>[a-z]+)\/(?<rowKey>[0-9\._:]+)");
        }

        public override void PodczasLadowania(BazowyVm poprzedniaStrona)
        {
            base.PodczasLadowania(poprzedniaStrona);

            PokazProgress = true;

            // Note: data będzie MinValue więc siłą rzeczy pobierze wszystko
            // ale sprawdziłam, działa branie tylko od jakiejś daty
            // Po prostu nie chce mi się teraz robić żeby to co już pobrało
            // zapisywało się do pliku. To przecież tylko appka testowa
            _repozytorium.MessagesRepository.GetNew(
                login: RegistryPomocnik.NazwaZalogowanegoUzytkownika,
                dateFrom: RegistryPomocnik.DataOstatniegoPobieraniaWiadomosci,
                key: RegistryPomocnik.KluczUzytkownika)
                .ContinueWith(x =>
                {
                    Application.Current.Dispatcher.Invoke(async () =>
                    {
                        if (x.Status == TaskStatus.RanToCompletion
                            && x.Result != null
                            && x.Result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var dtoList = await x.Result.Content.ReadAsDeserializedJson<List<GetNewMessagesResponseDto>>();
                            Messages = GetMessagesFromDto(dtoList);
                            PokazProgress = false;
                        }
                        else if (x.Status == TaskStatus.RanToCompletion
                            && x.Result != null
                            && x.Result.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            PokazProgress = false;
                            var msg = $"Problem w pobieraniu wiadomości, status code: {x.Result.StatusCode}, błąd: {x.Result.Content.ReadAsStringAsync()}";
                            Log.Error(msg);
                            MessageBox.Show(msg);
                        }
                        else
                        {
                            PokazProgress = false;
                            Log.Error("Problem w pobieraniu wiadomości", x.Exception);
                            MessageBox.Show($"Problem w pobieraniu wiadomości, błąd: {x.Exception}");
                        }
                    });
                });
        }

        private ObservableCollection<Message> GetMessagesFromDto(List<GetNewMessagesResponseDto> dtoList)
        {
            var list = new ObservableCollection<Message>();
            foreach (var item in dtoList)
            {
                list.Add(new Message()
                {
                    From = item.From,
                    Topic = item.Topic,
                    Content = item.Content,
                    RowKey = item.RowKey
                });
            }
            return list;
        }

        private ObservableCollection<Message> _messages;
        public ObservableCollection<Message> Messages
        {
            get { return _messages; }
            private set
            {
                _messages = value;
                NotifyPropertyChanged(nameof(Messages));
                Potwierdz.RaiseCanExecuteChanged();
            }
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

        internal readonly Regex potwierdzRegex;
        public DelegateCommand<string> Potwierdz { get; }
        private async void PotwierdzExecute(string apiPath)
        {
            // note: nie podoba mi się ten kod ale na ten moment życia 
            // wydało mi się to najbardziej banalne do wdrożenia

            var groups = potwierdzRegex.Match(apiPath).Groups;
            var from = groups["from"].Value as string;
            var to = groups["to"].Value as string;
            var rowKey = groups["rowKey"].Value as string;

            PokazProgress = true;
            await _repozytorium.HouseholdsRepository.AcceptInvitation(from, to, rowKey, RegistryPomocnik.KluczUzytkownika)
                .ContinueWith(task =>
                {
                    Application.Current.Dispatcher.Invoke(async () =>
                    {
                        PokazProgress = false;
                        if (task.Status == TaskStatus.RanToCompletion
                            && task.Result != null
                            && task.Result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            MessageBox.Show("Hurra od teraz należysz do tego gospodarstwa razem z tą osobą");
                            RegistryPomocnik.GospodarstwoId = await task.Result.Content.ReadAsStringAsync();
                            RegistryPomocnik.CzyNalezyDoGospodarstwa = true;
                            var messagesToDelete = Messages.Where(x => x.Content == apiPath).ToList();
                            for (var i = 0; i < messagesToDelete.Count(); ++i)
                            {
                                var msg = messagesToDelete[i];
                                Messages.Remove(msg);
                            }
                        }
                        else if (task.Status == TaskStatus.RanToCompletion
                            && task.Result != null
                            && task.Result.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            var msg = $"Błąd w akceptacji zaproszenia, status {task.Result.StatusCode}, błąd {task.Result.Content.ReadAsStringAsync()}";
                            Log.Error(msg);
                        }
                        else
                        {
                            Log.Error("Błąd w akceptacji zaproszenia", task.Exception);
                            MessageBox.Show("Błąd w akceptacji zaproszenia: " + task.Exception);
                        }
                    });
                });
        }
        private bool PotwierdzCanExecute(string apiPath)
        {
            return apiPath != null
                && potwierdzRegex.IsMatch(apiPath);
        }

        public class Message : PropertyChangedVm
        {
            public UserShort From { get; set; }
            public string Topic { get; set; }
            public string Content { get; set; }
            public string RowKey { get; set; }

            private bool _wasRead;
            public bool WasRead
            {
                get { return _wasRead; }
                set
                {
                    _wasRead = value;
                    NotifyPropertyChanged(nameof(WasRead));
                }
            }
        }
    }
}
