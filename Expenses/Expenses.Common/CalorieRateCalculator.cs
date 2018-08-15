using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.Common
{
    public static class CalorieRateCalculator
    {
        /// <summary>
        /// Basal Metabolic Rate, Harris and Benedict's formula
        /// </summary>
        /// <param name="sex">true for female</param>
        /// <returns></returns>
        public static double BmrHarrisAndBenedict(bool sex, double bodyWeightKg, double heightCm, int ageYears)
        {
            const bool Female = true;

            if (sex == Female)
                return 665.09 + (9.56 * bodyWeightKg) + (1.85 * heightCm) - (4.67 * ageYears);
            else
                return 66.47 + (13.75 * bodyWeightKg) + (5.003 * heightCm) - (6.75 * ageYears); 
        }

        /// <summary>
        /// Total Metabolic Rate
        /// </summary>
        /// <param name="bmr">Basal Metabolic Rate</param>
        /// <param name="pal">Phisical Activity Level, in range [1.4-2.4]</param>
        /// <returns></returns>
        public static double Tmr(double bmr, double pal)
        {
            if (pal < 1.4)
            {
                throw new ArgumentOutOfRangeException("pal");
            }
            if (pal > 2.4)
            {
                throw new ArgumentOutOfRangeException("pal");
            }
            return bmr * pal;
        }

        /// <summary>
        /// Total Metabolic Rate
        /// </summary>
        /// <param name="sex">true for female</param>
        /// <param name="pal">Phisical Activity Level, in range [1.4-2.4]</param>
        /// <returns></returns>
        public static double Tmr(bool sex, double bodyWeightKg, double heightCm, int ageYears, double pal)
        {
            return Tmr(BmrHarrisAndBenedict(sex, bodyWeightKg, heightCm, ageYears), pal);
        }
    }
}
