using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Model.Entities
{
    public class Cashflow : TableEntity
    {
        public Cashflow()
        {

        }

        public Cashflow(Cashflow cashflow)
        {
            DateTime = cashflow.DateTime;
            CategoryGuid = cashflow.CategoryGuid;
            Amount = cashflow.Amount;
            Details = cashflow.Details;
            WalletGuid = cashflow.WalletGuid;
        }

        public DateTime DateTime { get; set; }
        public Guid CategoryGuid { get; set; }
        /// <summary>
        /// Money serialized
        /// </summary>
        public string Amount { get; set; }
        /// <summary>
        /// List of CashFlowDetail serialized
        /// </summary>
        public string Details { get; set; }
        public Guid WalletGuid { get; set; }
    }
}
