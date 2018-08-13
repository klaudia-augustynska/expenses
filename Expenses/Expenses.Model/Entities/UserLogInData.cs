using Microsoft.WindowsAzure.Storage.Table;

namespace Expenses.Model.Entities
{
    /// <summary>
    /// Data used only for logging in with a reference to a household
    /// </summary>
    public class UserLogInData : TableEntity
    {
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string Key { get; set; }
        public string Salt { get; set; }
        /// <summary>
        /// PK of household
        /// </summary>
        public string HouseholdId { get; set; }
    }
}
