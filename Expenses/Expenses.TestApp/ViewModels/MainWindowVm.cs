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
        public MainWindowVm(Nawigacja nawigacja) : base(nawigacja)
        {
            _nawigacja.ZmianaIlosciOtwartychStron += 
                () =>
                {
                    NotifyPropertyChanged(nameof(IloscOtwartychStron));
                    Wroc.RaiseCanExecuteChanged();
                };
            Wroc = new DelegateCommand(WrocExecute, WrocCanExecute);
        }

        private BazowyVm _vm;
        public BazowyVm Vm
        {
            get
            {
                if (_vm != null)
                    return _vm;
                return _nawigacja.DomyslnyVm;
            }
            set
            {
                _vm = value;
                NotifyPropertyChanged(nameof(Vm));
            }
        }

        public int IloscOtwartychStron
        {
            get { return _nawigacja.IloscOtwartych; }
        }

        public DelegateCommand Wroc { get; }
        private void WrocExecute()
        {
            _nawigacja.Wroc();
        }
        private bool WrocCanExecute()
        {
            return _nawigacja.IloscOtwartych > 0;
        }
    }
}
