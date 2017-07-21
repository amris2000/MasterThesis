using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis.Extensions
{
    static class DateTimeExtensions
    {
        public static DateTime Max(DateTime Date1, DateTime Date2)
        {
            if (Date1 > Date2)
                return Date1;
            else
                return Date2;
        }

        public static DateTime Next(this DateTime date, DayOfWeek dayOfWeek)
        {
            return date.AddDays((dayOfWeek < date.DayOfWeek ? 7 : 0) + dayOfWeek - date.DayOfWeek);
        }
    }
}
