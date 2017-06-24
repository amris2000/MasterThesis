using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis.Extensions;
using MasterThesis;

namespace MasterThesis
{
    /* --- General information
     * This file contains the general date handling functionality.
     * Placeholder methods has been created for holiday calender functionality
     * but this has not been implemented. The file contains methods to
     * add tenors to dates and methods to adjust dates using appropiate day rules
     * if the rolled dates falls on non-business days (here defined as weekends.)
     * */

    public static class DateHandling
    {
        public static bool StrIsConvertableToDate(string str)
        {
            try
            {
                DateTime myDate = Convert.ToDateTime(str);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ConvertDateToTenorString(DateTime date, DateTime asOf)
        {
            double tenor = date.Subtract(asOf).TotalDays / 365;
            int years = (int)Math.Truncate(tenor);
            double leftover = tenor - years;

            string tenorLetter;
            int tenorNumber;

            if (years == 0)
            {
                tenorLetter = "M";
                tenorNumber = (int)Math.Round(leftover * 12.0);
                if (tenorNumber == 12)
                {
                    tenorLetter = "Y";
                    tenorNumber = 1;
                }
            }
            else if (years == 1)
            {
                if (leftover < 0.95)
                {
                    tenorNumber = 12 + (int)Math.Round(leftover * 12.0);
                    tenorLetter = "M";
                }
                else
                {
                    tenorNumber = 2;
                    tenorLetter = "Y";
                }
            }
            else if (years >= 2)
            {
                tenorLetter = "Y";
                if (leftover < 0.5)
                    tenorNumber = years;
                else
                    tenorNumber = years + 1;
            }
            else
            {
                tenorLetter = "?";
                tenorNumber = 0;
            }

            return tenorNumber.ToString() + tenorLetter;
        }

        public static Tenor GetTenorFromTenor(string tenor)
        {
            return StrToEnum.ConvertTenorLetter(tenor.Right(1));
        }

        public static string GetTenorLetterFromTenor(string tenor)
        {
            return tenor.Right(1);
        }

        public static int GetTenorNumberFromTenor(string tenor)
        {
            return Convert.ToInt16(tenor.Replace(tenor.Right(1), ""));
        }

        public static bool IsBusinessDay(DateTime date)
        {
            if (DateIsOnAWeekend(date))
                return false;
            else if (DateIsAHoliday(date))
            {
                return false;
            }
            else
                return true;
        }

        // Could make a look up in a holiday calender here. Just returning false for now.
        public static bool DateIsAHoliday(DateTime date)
        {
            return false;
        }

        public static bool DateIsOnAWeekend(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return true;
            else
                return false;
        }

        // Does only work if non-business days are weekends only (used for ON rate compounding)
        public static DateTime AddBusinessDay(DateTime date)
        {
            if (DateIsOnAWeekend(date) || date.DayOfWeek == DayOfWeek.Friday)
                return date.Next(DayOfWeek.Monday);
            else
                return date.AddDays(1);
        }

        // Does only work if non-business days are weekends only (used for ON rate compounding)
        public static DateTime SubtractBusinessDay(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Monday)
                return date.AddDays(-3);
            else if (date.DayOfWeek == DayOfWeek.Sunday)
                return date.AddDays(-2);
            if (date.DayOfWeek == DayOfWeek.Saturday)
                return date.AddDays(-1);
            else
                return date.AddDays(-1);
        }

        public static DateTime AddBusinessDays(DateTime date, int days)
        {
            DateTime outputDate = date;

            if (days == 0)
                return date;

            if (days>0)
            {
                for (int i = 0; i < days; i++)
                    outputDate = AddBusinessDay(outputDate);
            }
            else
            {
                for (int i = 0; i > days; i--)
                    outputDate = SubtractBusinessDay(outputDate);
            }

            return outputDate;
        }

        public static DateTime AddTenor(DateTime date, string tenor, DayRule dayRule = DayRule.N)
        {
            // To do: proper handling of business days and so forth.
            int tenorNumber = GetTenorNumberFromTenor(tenor);
            DateTime newDate;

            // Roll date forward (unadjusted)
            switch (GetTenorFromTenor(tenor))
            {
                case Tenor.D:
                    newDate = date.AddDays((double)tenorNumber);
                    break;
                case Tenor.B:
                    // Accounts for weekends but not holidays.
                    newDate = AddBusinessDays(date, tenorNumber);
                    break;
                case Tenor.W:
                    newDate = date.AddDays((double)tenorNumber * 7);
                    break;
                case Tenor.M:
                    newDate = date.AddMonths(tenorNumber);
                    break;
                case Tenor.Y:
                    newDate = date.AddYears(tenorNumber);
                    break;
                default:
                    throw new InvalidOperationException("Tenorletter not valid (input D,B,W,M or Y)");
            }

            return newDate;
        }

        public static DateTime AdjustDate(DateTime unadjustedDate, DayRule dayRule = DayRule.N)
        {
            // if unadjusted date is business day: no adjustment.
            if (IsBusinessDay(unadjustedDate) || dayRule == DayRule.N)
                return unadjustedDate;
            else if (dayRule == DayRule.F)
                return AddBusinessDay(unadjustedDate);
            else if (dayRule == DayRule.P)
                return SubtractBusinessDay(unadjustedDate);
            else if (dayRule == DayRule.MF)
            {
                int monthUnadjusted = unadjustedDate.Month;
                int monthAdjusted = AddBusinessDay(unadjustedDate).Month;

                if (monthUnadjusted == monthAdjusted)
                    return AddBusinessDay(unadjustedDate);
                else
                    return SubtractBusinessDay(unadjustedDate);
            }
            else
                throw new InvalidOperationException("DayRule is not valid.");
        }

        public static DateTime AddTenorAdjust(DateTime date, string tenor, DayRule dayRule = DayRule.N)
        {
            // Current implementation: holidays is only weekends. This means that if we add tenor
            // a business day tenor (B) we will never risk ending up on a holiday.
            DateTime unadjustedDate = AddTenor(date, tenor, dayRule);
            return AdjustDate(unadjustedDate, dayRule);
        }

        public static double Cvg(DateTime startDate, DateTime endDate, DayCount dayCount)
        {
            double Coverage = 0.0;
            switch (dayCount)
            {
                case DayCount.ACT360:
                    Coverage = endDate.Subtract(startDate).TotalDays / 360.0;
                    break;
                case DayCount.ACT365:
                    Coverage = endDate.Subtract(startDate).TotalDays / 365.0;
                    break;
                case DayCount.ACT36525:
                    Coverage = endDate.Subtract(startDate).TotalDays / 365.25;
                    break;
                case DayCount.THIRTY360:
                    Coverage = (double)((endDate.Year - startDate.Year) * 360 +
                                (endDate.Month - startDate.Month) * 30 +
                                Math.Min(30, (int)endDate.Day) - Math.Min(30, (int)startDate.Day)) / 360;
                    break;
                default:
                    throw new InvalidOperationException("DayCount convention not valid.");
            }

            return Coverage;
        }
    }
}
