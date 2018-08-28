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

        [Test]
        public void GetExpensesByInvertedDate_Test()
        {
            var rows = new List<string>()
            {
                // [0] poprawny wpis 27.08.2018
                "userCashflow_qwerty_7982.05.07_23:59:59_44f17b78-5967-4496-957f-89a7270c8bcb",
                // [1] poorawny wpis 27.08.2018
                "userCashflow_qwerty_7982.05.07_23:59:59_d7d572bc-f0ca-4a95-9aca-f5a5c3cf7c0a",
                // [2] poprawny wpis 27.08.2018 tylko rano
                "userCashflow_qwerty_7982.05.07_00:00:00_d7d572bc-f0ca-4a95-9aca-f5a5c3cf7c0a",
                // [3] niepoprawny wpis - innego użytkownika
                "userCashflow_asdf_7982.05.07_23:59:59_d7d572bc-f0ca-4a95-9aca-f5a5c3cf7c0a",
                // [4] poprawny wpis 28.08.2018
                "userCashflow_qwerty_7982.05.06_23:59:59_d7d572bc-f0ca-4a95-9aca-f5a5c3cf7c0a",
                // [5] niepoprawny wpis 28.10.2018
                "userCashflow_qwerty_7982.03.06_23:59:59_d7d572bc-f0ca-4a95-9aca-f5a5c3cf7c0a",
                // [6] niepoprawny wpis z jakiegoś maja - za wcześnie
                "userCashflow_qwerty_7982.08.06_23:59:59_d7d572bc-f0ca-4a95-9aca-f5a5c3cf7c0a",
                // [7] poprawny wpis w dzień 26.09.2018
                "userCashflow_qwerty_7982.04.07_00:00:00_d7d572bc-f0ca-4a95-9aca-f5a5c3cf7c0a",
                // [8] poprawny wpis w dzień 26.09.2018
                "userCashflow_qwerty_7982.04.07_23:59:59_d7d572bc-f0ca-4a95-9aca-f5a5c3cf7c0a"
            };

            var prefix = "userCashflow_qwerty";
            var dateFrom = RowKeyUtils.ConvertInvertedDateToDateFrom(RowKeyUtils.GetInvertedDateString(new DateTime(2018, 08, 27)));
            var dateTo = RowKeyUtils.ConvertInvertedDateToDateTo(RowKeyUtils.GetInvertedDateString(new DateTime(2018, 09, 26)));

            var dateFromInverted = $"{prefix}_{dateFrom}";
            var dateToInverted = $"{prefix}_{dateTo}";

            // 27.08.2018
            var expectedDateFromInverted = "userCashflow_qwerty_7982.05.08_00:00:00";
            // 26.09.2018
            var expectedDateToInverted = "userCashflow_qwerty_7982.04.07_00:00:00";

            var list = rows
                .Where(x => x.CompareTo(dateFromInverted) < 0)
                .Where(x => x.CompareTo(dateToInverted) > 0)
                .ToList();

            Assert.AreEqual(expectedDateFromInverted, dateFromInverted);
            Assert.AreEqual(expectedDateToInverted, dateToInverted);
            Assert.Contains(rows[0], list);
            Assert.Contains(rows[1], list);
            Assert.Contains(rows[2], list);
            Assert.False(list.Contains(rows[3]));
            Assert.Contains(rows[4], list);
            Assert.False(list.Contains(rows[5]));
            Assert.False(list.Contains(rows[6]));
            Assert.Contains(rows[7], list);
            Assert.Contains(rows[8], list);
        }
    }
}
