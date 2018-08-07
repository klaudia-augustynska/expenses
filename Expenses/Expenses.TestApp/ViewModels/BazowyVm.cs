using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;

namespace Expenses.TestApp.ViewModels
{
    abstract class BazowyVm : PropertyChangedVm
    {
        protected readonly Nawigacja _nawigacja;

        protected BazowyVm(Nawigacja nawigacja)
        {
            _nawigacja = nawigacja;
        }

        public virtual void PodczasLadowania(BazowyVm poprzedniaStrona)
        {

        }
    }
}
