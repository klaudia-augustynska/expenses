using Microsoft.WindowsAzure.Storage.Table;

namespace Expenses.Model.Entities
{
    /// <summary>
    /// User's details associated with a household
    /// </summary>
    public class UserDetails : TableEntity
    {
        public string Login { get; set; }
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
    }
}
