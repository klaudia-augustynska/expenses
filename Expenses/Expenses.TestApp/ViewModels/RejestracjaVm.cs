using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.TestApp.ViewModels
{
    class RejestracjaVm : BazowyVm
    {
        private readonly Nawigacja _nawigacja;
        public RejestracjaVm(Nawigacja navigationService, LogowanieVm logowanieVm)
        {
            _nawigacja = navigationService;
            Zarejestruj = new DelegateCommand(ZarejestrujExec);
            PrzejdzDoLogowania = new DelegateCommand(PrzejdzDoLogowaniaExec);
            LogowanieVm = logowanieVm;
        }

        private string _login;
        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                NotifyPropertyChanged(nameof(Login));
            }
        }

        private LogowanieVm LogowanieVm { get; }

        public DelegateCommand Zarejestruj { get; }
        private void ZarejestrujExec()
        {
            _nawigacja.IdzDo(LogowanieVm);
        }

        public DelegateCommand PrzejdzDoLogowania { get; }
        private void PrzejdzDoLogowaniaExec()
        {
            _nawigacja.IdzDo(LogowanieVm);
        }
    }
}
