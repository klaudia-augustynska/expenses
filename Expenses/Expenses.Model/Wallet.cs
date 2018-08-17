using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Model
{
    public class Wallet
    {
        private Guid _guid;
        public Guid Guid
        {
            get
            {
                if (_guid == default(Guid))
                    _guid = Guid.NewGuid();
                return _guid;
            }
            set
            {
                _guid = value;
            }
        }
        public string Name { get; set; }
        public Money Money { get; set; }
    }
}
