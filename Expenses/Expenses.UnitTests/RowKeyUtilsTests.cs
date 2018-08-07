using Expenses.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expenses.UnitTests
{
    [TestFixture]
    class RowKeyUtilsTests
    {
        [Test]
        public void GetInvertedDateString_InvertsDate()
        {
            var dateNotInverted = DateTime.MaxValue;
            var dateInverted = DateTime.MinValue;
            var dateInvertedString = dateInverted.ToString(RowKeyUtils.DateFormat, CultureInfo.InvariantCulture);

            var actualDateInverted = RowKeyUtils.GetInvertedDateString(dateNotInverted);

            Assert.AreEqual(
                expected: dateInvertedString, 
                actual: actualDateInverted);
        }

        [Test]
        public void GetInvertedDateTime_InvertsDate()
        {
            var dateString = "0001.01.01_00:00:00";
            var expectedDate = DateTime.MaxValue;

            var actualDate = RowKeyUtils.GetInvertedDateTime(invertedDate: dateString);

            Assert.AreEqual(expectedDate, actualDate);
        }

        [Test]
        public void GetInvertedDateString_IsInverseFunctionOf_GetInvertedDateTime()
        {
            var dateString = "1234.12.03_01:02:03";

            var invertedDateTime = RowKeyUtils.GetInvertedDateTime(dateString);
            var invertedDateString = RowKeyUtils.GetInvertedDateString(invertedDateTime);

            Assert.AreEqual(dateString, invertedDateString);
        }

        [Test]
        public void GetInvertedDateTime_IsInverseFunctionOf_GetInvertedDateString()
        {
            var date = DateTime.Now;

            var invertedDateString = RowKeyUtils.GetInvertedDateString(date);
            var invertedDateTime = RowKeyUtils.GetInvertedDateTime(invertedDateString);

            Assert.AreEqual(date.Year, invertedDateTime.Year);
            Assert.AreEqual(date.Month, invertedDateTime.Month);
            Assert.AreEqual(date.Day, invertedDateTime.Day);
            Assert.AreEqual(date.Hour, invertedDateTime.Hour);
            Assert.AreEqual(date.Minute, invertedDateTime.Minute);
            Assert.AreEqual(date.Second, invertedDateTime.Second);
        }
    }
}
