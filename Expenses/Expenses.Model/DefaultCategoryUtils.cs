using Expenses.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Model
{
    public static class DefaultCategoryUtils
    {
        private static Dictionary<DefaultCategory, DefaultCategoryType> CategoryTypeMapping { get; set; }

        public static DefaultCategoryType GetCategoryType(DefaultCategory category)
        {
            // lazy loading
            if (CategoryTypeMapping == null)
            {
                CategoryTypeMapping = GetCategoryTypeMapping();
            }
            return CategoryTypeMapping[category];
        }

        private static Dictionary<DefaultCategory, DefaultCategoryType> GetCategoryTypeMapping()
        {
            return new Dictionary<DefaultCategory, DefaultCategoryType>
            {
                { DefaultCategory.Alcohol, DefaultCategoryType.Calories },
                { DefaultCategory.Bills, DefaultCategoryType.EqualDivision },
                { DefaultCategory.Hygiene, DefaultCategoryType.EqualDivision },
                { DefaultCategory.NormalFood, DefaultCategoryType.Calories },
                { DefaultCategory.Other, DefaultCategoryType.EqualDivision },
                { DefaultCategory.Transport, DefaultCategoryType.EqualDivision },
                { DefaultCategory.UnhealthyFood, DefaultCategoryType.Calories }
            };
        }

        public static List<Category> GetDefaultCategories(string login)
        {
            var universalFactorInThisCase = new Dictionary<string, double>()
                    {
                        { login, 100 }
                    };
            return new List<Category>()
            {
                new Category()
                {
                    Name = "Jedzenie",
                    DefaultCategory = DefaultCategory.NormalFood,
                    Factor = universalFactorInThisCase
                },
                new Category()
                {
                    Name = "Niezdrowa żywność",
                    DefaultCategory = DefaultCategory.UnhealthyFood,
                    Factor = universalFactorInThisCase
                },
                new Category()
                {
                    Name = "Alkohol",
                    DefaultCategory = DefaultCategory.Alcohol,
                    Factor = universalFactorInThisCase
                },
                new Category()
                {
                    Name = "Higiena",
                    DefaultCategory = DefaultCategory.Hygiene,
                    Factor = universalFactorInThisCase
                },
                new Category()
                {
                    Name = "Opłaty",
                    DefaultCategory = DefaultCategory.Bills,
                    Factor = universalFactorInThisCase
                },
                new Category()
                {
                    Name = "Transport",
                    DefaultCategory = DefaultCategory.Transport,
                    Factor = universalFactorInThisCase
                },
                new Category()
                {
                    Name = "Różne",
                    DefaultCategory = DefaultCategory.Other,
                    Factor = universalFactorInThisCase
                }
            };
        }
    }
}
