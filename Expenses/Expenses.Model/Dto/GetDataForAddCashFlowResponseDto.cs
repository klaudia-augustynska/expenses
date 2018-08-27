using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Model.Dto
{
    public class GetDataForAddCashFlowResponseDto
    {
        public List<Category> Categories { get; set; }
        public List<Wallet> Wallets { get; set; }
    }
}
