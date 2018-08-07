using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expenses.Model.Enums;
using Microsoft.WindowsAzure.Storage.Table;

namespace Expenses.Model.Entities
{
    public class User : TableEntity
    {
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string Key { get; set; }
        public string Salt { get; set; }
        public string Name { get; set; }
        public double? Weight { get; set; }
        public double? Height { get; set; }
        /// <summary>
        /// true for female
        /// </summary>
        public bool? Sex { get; set; }
        /// <summary>
        /// List of Wallet
        /// </summary>
        public string Wallets { get; set; }
        /// <summary>
        /// PK of household
        /// </summary>
        public string HouseholdId { get; set; }
    }
}
