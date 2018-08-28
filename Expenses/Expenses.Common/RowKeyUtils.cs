using System;
using System.Globalization;
using System.Text;

namespace Expenses.Common
{
    public static class RowKeyUtils
    {
        internal const string DateFormat = "yyyy.MM.dd_HH:mm:ss";
        private static readonly DateTime StartDate;

        static RowKeyUtils()
        {
            StartDate = DateTime.MinValue;
        }

        public static string GetInvertedDateString(DateTime dateTime)
        {
            return GetInvertedDateTime(dateToInvert: dateTime).ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime GetInvertedDateTime(string invertedDate)
        {
            DateTime inverted = DateTime.ParseExact(invertedDate, DateFormat, CultureInfo.InvariantCulture);
            return GetInvertedDateTime(dateToInvert: inverted);
        }

        private static DateTime GetInvertedDateTime(DateTime dateToInvert)
        {
            TimeSpan timeSpan = dateToInvert - StartDate;
            return DateTime.MaxValue - timeSpan;
        }

        public static string ConvertInvertedDateToDateFrom(string invertedDate)
        {
            var date = DateTime.ParseExact(invertedDate, DateFormat, CultureInfo.InvariantCulture);
            var dateWithZeroTime = new DateTime(date.Year, date.Month, date.Day);
            var dateADayEarlier = dateWithZeroTime.AddDays(1);
            return dateADayEarlier.ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        public static string ConvertInvertedDateToDateTo(string invertedDate)
        {
            var date = DateTime.ParseExact(invertedDate, DateFormat, CultureInfo.InvariantCulture);
            var dateWithZeroTime = new DateTime(date.Year, date.Month, date.Day);
            return dateWithZeroTime.ToString(DateFormat, CultureInfo.InvariantCulture);
        }
    }
}
