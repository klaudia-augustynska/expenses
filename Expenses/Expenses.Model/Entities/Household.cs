using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Expenses.Model.Entities
{
    public class Household : TableEntity
    {
        /// <summary>
        /// List of Member
        /// </summary>
        public string Members { get; set; }
        public string MoneyAggregated { get; set; }
    }
}
