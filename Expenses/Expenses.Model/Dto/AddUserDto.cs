using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Model.Dto
{
    public class AddUserDto
    {
        public string HashedPassword { get; set; }
        public string Salt { get; set; }
    }
}
