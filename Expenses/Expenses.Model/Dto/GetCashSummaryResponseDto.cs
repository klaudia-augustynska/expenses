using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Model.Dto
{
    public class GetCashSummaryResponseDto
    {
        public List<Money> HouseholdMoney { get; set; }
        public List<Money> HouseholdExpenses { get; set; }
        public List<Wallet> UserWallets { get; set; }
        public List<Money> UserExpenses { get; set; }
        public Dictionary<string, List<Money>> UserCharges { get; set; }
    }
}
