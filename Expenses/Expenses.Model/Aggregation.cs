using Expenses.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Model
{
    // TODO: zrefaktoryzować jak będzie czas, np. za 200 lat, bo łamie SOLID
    // i wcale mi się to nie podoba
    public class Aggregation
    {
        public static List<Money> MergeWallets(params List<Wallet>[] wallets)
        {
            return MergeWallets(null, wallets);
        }

        public static List<Money> MergeWallets(List<Money> initialMoney, params List<Wallet>[] wallets)
        {
            var merged = initialMoney ?? new List<Money>();
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


        public static List<Money> ExcludeWallets(params List<Wallet>[] wallets)
        {
            return ExcludeWallets(null, wallets);
        }

        public static List<Money> ExcludeWallets(List<Money> initialMoney, params List<Wallet>[] wallets)
        {
            var merged = initialMoney ?? new List<Money>();
            if (wallets != null)
            {
                foreach (var item in wallets)
                {
                    ExcludeWallets(merged, item);
                }
            }
            return merged;
        }

        private static void ExcludeWallets(List<Money> merged, List<Wallet> wallets)
        {
            foreach (var item in wallets)
            {
                var mergedListElement = merged.FirstOrDefault(x => x.Currency == item.Money.Currency);
                if (mergedListElement != null)
                {
                    mergedListElement.Amount -= item.Money.Amount;
                }
            }
        }

        public static List<Category> MergeCategories(List<Member> members, params List<Category>[] lists)
        {
            return MergeCategories(members, null, lists);
        }

        public static List<Category> MergeCategories(List<Member> members, List<Category> initialList, params List<Category>[] lists)
        {
            var merged = initialList ?? new List<Category>();
            if (lists != null)
            {
                foreach (var list in lists)
                {
                    MergeTwoListsOfCategories(members, merged, list);
                }
            }
            return merged;
        }

        private static void MergeTwoListsOfCategories(List<Member> members, List<Category> merged, List<Category> list)
        {
            foreach (var item in list)
            {
                if (item.DefaultCategory.HasValue)
                {
                    var itemInMergedList = merged.FirstOrDefault(x => x.DefaultCategory == item.DefaultCategory);
                    var itemToUse = itemInMergedList ?? item;
                    var type = DefaultCategoryUtils.GetCategoryType(itemToUse.DefaultCategory.Value);
                    if (type == Enums.DefaultCategoryType.Calories
                        || type == Enums.DefaultCategoryType.EqualDivision)
                    {
                        var mergedItem = FillFactors(members, itemToUse);
                        if (itemInMergedList == null)
                            merged.Add(mergedItem);
                        continue;
                    }
                }
                merged.Add(item);
            }
        }

        private static Category FillFactors(List<Member> members, Category item)
        {
            if (item.DefaultCategory == null)
            {
                throw new ArgumentException("This metod was designed for items that contain the same type of default category. Do not use it in other cases");
            }

            var category = item.DefaultCategory.Value;
            var type = DefaultCategoryUtils.GetCategoryType(category);

            switch (type)
            {
                case Enums.DefaultCategoryType.Calories:
                    return FillFactorsByCalories(members, item);
                case Enums.DefaultCategoryType.EqualDivision:
                    return FillFactorsByEqualDivision(members, item);
                default:
                    throw new InvalidOperationException("The default category type cannot be merged");
            }
        }

        private static Category FillFactorsByEqualDivision(List<Member> members, Category item)
        {
            item.Factor = new Dictionary<string, double>();
            var value = Math.Round(100.0 / members.Count, 2);
            foreach (var member in members)
            {
                item.Factor.Add(member.Login, value);
            }
            return item;
        }

        private static Category FillFactorsByCalories(List<Member> members, Category item)
        {
            var oneHundredPercent = members.Select(x => x.Tmr).Sum();
            item.Factor = new Dictionary<string, double>();
            foreach (var member in members)
            {
                item.Factor.Add(member.Login, Math.Round(member.Tmr.Value * 100.0 / oneHundredPercent.Value, 2));
            }
            return item;
        }
    }
}
