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
    class WiadomosciVm : BazowyVm
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Repository _repozytorium;

        public WiadomosciVm(Nawigacja nawigacja, Repository repozytorium) : base(nawigacja)
        {
            _repozytorium = repozytorium;
            Potwierdz = new DelegateCommand<string>(PotwierdzExecute, PotwierdzCanExecute);
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

        private List<Message> GetMessagesFromDto(List<GetNewMessagesResponseDto> dtoList)
        {
            var list = new List<Message>();
            foreach (var item in dtoList)
            {
                list.Add(new Message()
                {
                    From = item.From,
                    Topic = item.Topic,
                    Content = item.Content
                });
            }
            return list;
        }

        private List<Message> _messages;
        public List<Message> Messages
        {
            get { return _messages; }
            private set
            {
                _messages = value;
                NotifyPropertyChanged(nameof(Messages));
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

        public DelegateCommand<string> Potwierdz { get; }
        private void PotwierdzExecute(string apiPath)
        {

        }
        private bool PotwierdzCanExecute(string apiPath)
        {
            return false;
        }

        public class Message : PropertyChangedVm
        {
            public UserShort From { get; set; }
            public string Topic { get; set; }
            public string Content { get; set; }

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
