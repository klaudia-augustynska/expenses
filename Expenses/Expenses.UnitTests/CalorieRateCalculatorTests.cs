using Expenses.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.UnitTests
{
    [TestFixture]
    class CalorieRateCalculatorTests
    {
        [TestCase(true, 64, 166, 24, ExpectedResult = 1471.95)]
        [TestCase(false, 86, 185, 30, ExpectedResult = 1972.025)]
        public double BmrHarrisAndBenedict_Test(bool sex, double weight, double height, int age)
        {
            var result = CalorieRateCalculator.BmrHarrisAndBenedict(sex, weight, height, age);
            return Math.Round(result, 3);
        }

        [TestCase(1472, 1.4, ExpectedResult = 2061)]
        [TestCase(1972, 1.7, ExpectedResult = 3352)]
        public double Tmr_Test(double bmr, double pal)
        {
            var result = CalorieRateCalculator.Tmr(bmr, pal);
            return Math.Round(result, 0);
        }

        [Test]
        public void Tmr_PalOutOfRange_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()
                => CalorieRateCalculator.Tmr(1234, pal: 1.3));
            Assert.Throws<ArgumentOutOfRangeException>(()
                => CalorieRateCalculator.Tmr(1234, pal: 2.5));
        }
    }
}
