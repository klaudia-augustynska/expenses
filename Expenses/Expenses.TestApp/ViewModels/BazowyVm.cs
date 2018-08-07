using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;

namespace Expenses.TestApp.ViewModels
{
    abstract class BazowyVm : INotifyPropertyChanged
    {
        protected readonly Nawigacja _nawigacja;

        protected BazowyVm(Nawigacja nawigacja)
        {
            _nawigacja = nawigacja;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public virtual void PodczasLadowania(BazowyVm poprzedniaStrona)
        {

        }
    }
}
