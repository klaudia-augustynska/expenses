using Expenses.Model.Enums;

namespace Expenses.Model
{
    public class Money
    {
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public double? ExchangeRate { get; set; }
    }
}
