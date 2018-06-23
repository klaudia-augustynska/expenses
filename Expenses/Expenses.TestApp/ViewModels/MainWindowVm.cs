using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.TestApp.ViewModels
{
    class MainWindowVm : BazowyVm
    {
        private readonly Nawigacja _navigationService;

        public MainWindowVm(Nawigacja navigationService)
        {
            _navigationService = navigationService;
            _navigationService.ZmianaIlosciOtwartychStron += 
                () =>
                {
                    NotifyPropertyChanged(nameof(IloscOtwartychStron));
                    Wroc.RaiseCanExecuteChanged();
                };
            Wroc = new DelegateCommand(WrocExec, WrocCanExec);
        }

        private BazowyVm _vm;
        public BazowyVm Vm
        {
            get
            {
                if (_vm != null)
                    return _vm;
                return _navigationService.DomyslnyVm;
            }
            set
            {
                _vm = value;
                NotifyPropertyChanged(nameof(Vm));
            }
        }

        public int IloscOtwartychStron
        {
            get { return _navigationService.IloscOtwartych; }
        }

        public DelegateCommand Wroc { get; }
        private void WrocExec()
        {
            _navigationService.Wroc();
        }
        private bool WrocCanExec()
        {
            return _navigationService.IloscOtwartych > 0;
        }
    }
}
