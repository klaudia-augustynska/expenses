using Expenses.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Model.Dto
{
    public class ConfigureUserDto
    {
        public string Name { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public Sex Sex { get; set; }
        public List<Wallet> Wallets { get; set; }
        public DateTime DateOfBirth { get; set; }
        public double Pal { get; set; }
    }
}
