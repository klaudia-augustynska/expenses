using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemberLogin = System.String;
using Percent = System.Double;

namespace Expenses.Model.Dto
{
    public class AddCategoryDto
    {
        public string Name { get; set; }
        public Dictionary<MemberLogin, Percent> Factor { get; set; }
    }
}
