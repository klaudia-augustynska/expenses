using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Model.Dto
{
    public class AddCashFlowDto
    {
        public DateTime DateTime { get; set; }
        public Guid CategoryGuid { get; set; }
        public Money Amount { get; set; }
        public List<CashFlowDetail> Details { get; set; }
        public Guid WalletGuid { get; set; }
    }
}
