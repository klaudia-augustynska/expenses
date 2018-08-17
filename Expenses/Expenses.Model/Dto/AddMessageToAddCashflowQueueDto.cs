using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Model.Dto
{
    public class AddMessageToAddCashflowQueueDto
    {
        public Money Amount { get; set; }
        public string HouseholdPk { get; set; }
        public string HouseholdRk { get; set; }
        public Guid WalletGuid { get; set; }
        public Guid CategoryGuid { get; set; }
        public List<CashFlowDetail> Details { get; set; }
        public bool UserBelongsToGroup { get; set; }
        public string Login { get; set; }
    }
}
