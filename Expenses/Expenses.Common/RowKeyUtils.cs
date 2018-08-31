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
            DateTime resultDate;
            if (Math.Abs((DateTime.MaxValue - dateWithZeroTime).Days) >= 1)
            {
                var dateADayEarlier = dateWithZeroTime.AddDays(1);
                resultDate = dateADayEarlier;
            }
            else
            {
                resultDate = dateWithZeroTime;
            }
            return resultDate.ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        public static string ConvertInvertedDateToDateTo(string invertedDate)
        {
            var date = DateTime.ParseExact(invertedDate, DateFormat, CultureInfo.InvariantCulture);
            var dateWithZeroTime = new DateTime(date.Year, date.Month, date.Day);
            return dateWithZeroTime.ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        public static string InvertDateString(string date)
        {
            var dateTimeFormt = "yyyy.MM.dd";
            DateTime dateTimeFrom = DateTime.ParseExact(date, dateTimeFormt, CultureInfo.InvariantCulture);
            return RowKeyUtils.GetInvertedDateString(dateTimeFrom);
        }

        public static string InvertDateTimeString(string dateTime)
        {
            var dateTimeFormt = DateFormat;
            DateTime dateTimeFrom = DateTime.ParseExact(dateTime, dateTimeFormt, CultureInfo.InvariantCulture);
            return RowKeyUtils.GetInvertedDateString(dateTimeFrom);
        }
    }
}
