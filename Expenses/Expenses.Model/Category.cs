using Expenses.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemberLogin = System.String;
using Percent = System.Double;

namespace Expenses.Model
{
    public class Category
    {
        private Guid _guid;
        public Guid Guid
        {
            get
            {
                if (_guid == null)
                    _guid = Guid.NewGuid();
                return _guid;
            }
            set
            {
                _guid = value;
            }
        }
        public DefaultCategory? DefaultCategory { get; set; }
        public string Name { get; set; }
        public Dictionary<MemberLogin, Percent> Factor { get; set; }
    }
}
