using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Model.Enums
{
    public enum DefaultCategoryType
    {
        /// <summary>
        /// Undefined default value to avoid future errors
        /// </summary>
        Default = 0,

        /// <summary>
        /// participation in expenses defined by suggested calorie rate
        /// </summary>
        Calories = 1,

        /// <summary>
        /// each user equally participates in expenses
        /// </summary>
        EqualDivision = 2,

        /// <summary>
        /// only the certain user pays for his things
        /// </summary>
        PerUser = 3,

        /// <summary>
        /// uncommon distribution of participation factor, eg. only adults pay and other users are for better calculations
        /// </summary>
        Other = 4
    }
}
