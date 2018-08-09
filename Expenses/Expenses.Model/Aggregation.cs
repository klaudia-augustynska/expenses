using Expenses.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Api.Helpers
{
    public class Aggregation
    {
        public static List<Money> MergeWallets(params List<Wallet>[] wallets)
        {
            var merged = new List<Money>();
            if (wallets != null)
            {
                foreach (var item in wallets)
                {
                    MergeWallets(merged, item);
                }
            }
            return merged;
        }

        private static void MergeWallets(List<Money> merged, List<Wallet> wallets)
        {
            foreach (var item in wallets)
            {
                var mergedListElement = merged.FirstOrDefault(x => x.Currency == item.Money.Currency);
                if (mergedListElement == null)
                {
                    merged.Add(item.Money);
                }
                else
                {
                    mergedListElement.Amount += item.Money.Amount;
                }
            }
        }
    }
}
