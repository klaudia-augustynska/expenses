using Expenses.TestApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Expenses.TestApp
{
    class Nawigacja
    {
        private readonly UnityContainer _unityContainer;

        public Nawigacja(UnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
            _otwarteStrony = new Stack<BazowyVm>();
        }

        private BazowyVm _domyslnyVm;
        public BazowyVm DomyslnyVm
        {
            get { return _domyslnyVm; }
            set
            {
                _domyslnyVm = value;
                _domyslnyVm.PodczasLadowania(poprzedniaStrona: null);
            }
        }

        public MainWindowVm GlownyVm { get; set; }

        private Stack<BazowyVm> _otwarteStrony;

        public event Action ZmianaIlosciOtwartychStron;

        public void IdzDo<T>() where T : BazowyVm
        {
            var vm = _unityContainer.Resolve<T>();
            vm.PodczasLadowania(poprzedniaStrona: GlownyVm.Vm);
            GlownyVm.Vm = vm;
            _otwarteStrony.Push(vm);
            ZmianaIlosciOtwartychStron?.Invoke();
        }

        public void Wroc()
        {
            _otwarteStrony.Pop();
            var poprzedniVm = _otwarteStrony.Count > 0 
                ? _otwarteStrony.Peek() : null;
            GlownyVm.Vm = poprzedniVm ?? DomyslnyVm;
            ZmianaIlosciOtwartychStron?.Invoke();
        }

        public int IloscOtwartych
        {
            get { return _otwarteStrony.Count; }
        }

        public void KasujHistorie()
        {
            _otwarteStrony.Clear();
            ZmianaIlosciOtwartychStron?.Invoke();
        }
    }
}
