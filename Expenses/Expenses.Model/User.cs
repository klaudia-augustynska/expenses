using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Expenses.Model
{
    public class User : TableEntity
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Key { get; set; }
    }
}
