using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Expenses.Model.Entities
{
    /// <summary>
    /// User's details associated with a household
    /// </summary>
    public class UserDetails : TableEntity
    {
        public UserDetails()
        {
        }

        public UserDetails(UserDetails userDetails)
        {
            PartitionKey = userDetails.PartitionKey;
            RowKey = userDetails.RowKey;

            Login = userDetails.Login;
            Name = userDetails.Name;
            Weight = userDetails.Weight;
            Height = userDetails.Height;
            DateOfBirth = userDetails.DateOfBirth;
            Sex = userDetails.Sex;
            Wallets = userDetails.Wallets;
            Categories = userDetails.Categories;
            Pal = userDetails.Pal;
            Charges = userDetails.Charges;
        }

        public string Login { get; set; }
        public string Name { get; set; }
        public double? Weight { get; set; }
        public double? Height { get; set; }
        public DateTime? DateOfBirth { get; set; }
        /// <summary>
        /// true for female
        /// </summary>
        public bool? Sex { get; set; }
        /// <summary>
        /// List of Wallet
        /// </summary>
        public string Wallets { get; set; }
        /// <summary>
        /// List of Category
        /// </summary>
        public string Categories { get; set; }
        public double? Pal { get; set; }
        /// <summary>
        /// Dictionary, where Key is string (login) and value is List of Money. Negative values indicate that the person has a debt.
        /// </summary>
        public string Charges { get; set; }
    }
}
