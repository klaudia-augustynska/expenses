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
        public Category()
        {
            Guid = Guid.NewGuid();
        }

        public Guid Guid { get; private set; }
        public DefaultCategory? DefaultCategory { get; set; }
        public string Name { get; set; }
        public Dictionary<MemberLogin, Percent> Factor { get; set; }
    }
}
