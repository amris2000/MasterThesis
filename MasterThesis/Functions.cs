using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis.Extensions;
using MasterThesis;

namespace MasterThesis
{
    public static class Functions
    {
        public static Tuple<double, string> ParseTenor(string tenor)
        {
            string TenorLetter = tenor.Right(1);
            tenor = tenor.Replace(TenorLetter, "");
            double Number = Convert.ToInt16(tenor);

            return new Tuple<double, string>(Number, TenorLetter);
        }
        // Something fucks up here ... It Ends at around 2022/01/14 at some points and loops
        public static List<DateTime> IMMSchedule(DateTime StartDate, DateTime EndDate)
        {
            DateTime TempDate = StartDate;
            List<DateTime> MyList = new List<DateTime>();
            int i = 0;
            while (TempDate < EndDate && i < 100)
            {
                MyList.Add(NextIMMDate(TempDate));
                TempDate = MyList[i];
                i = i + 1;
            }

            return MyList;
        }
        public static void PrintDateList(List<DateTime> MyList, string HeadLine = "")
        {
            Console.WriteLine("------ PRINTING DATE LIST -------");
            Console.WriteLine("HeadLine ...:" + HeadLine);
            for (int i = 0; i < MyList.Count; i++)
            {
                Console.WriteLine(i + "\t" + MyList[i].ToString("dd/MM/yyyy") + ". Day : " + MyList[i].DayOfWeek);
            }
        }
        public static DateTime IMMDate(int Year, int Month)
        {
            return NextIMMDate(new DateTime(Year, Month, 1));
        }
        public static DateTime FindThirdWeekdayOfMonth(int Year, int Month, int MyDayOfWeek)
        {

            DateTime MyDate = new DateTime(Year, Month, 1);
            int Subtract = MyDayOfWeek - Convert.ToInt16(MyDate.DayOfWeek);

            if (Subtract < 0)
                Subtract = Subtract + 7;

            int ThirdWedOfMonth = Subtract + 14 + 1;

            return new DateTime(Year, Month, ThirdWedOfMonth);
        }
        public static DateTime NextIMMDate(DateTime Date)
        {
            int Year, Month, Day;

            Year = Date.Year;
            Month = Date.Month;
            Day = Date.Day;

            DateTime ThirdWedMarch, ThirdWedJune, ThirdWedSeptember, ThirdWedDecember;
            ThirdWedMarch = FindThirdWeekdayOfMonth(Year, 3, 3);
            ThirdWedJune = ThirdWedMarch = FindThirdWeekdayOfMonth(Year, 6, 3);
            ThirdWedSeptember = ThirdWedMarch = FindThirdWeekdayOfMonth(Year, 9, 3);
            ThirdWedDecember = ThirdWedMarch = FindThirdWeekdayOfMonth(Year, 12, 3);

            if (Month == 12)
            {
                if (Date < ThirdWedDecember)
                    return ThirdWedDecember;
                else
                    return FindThirdWeekdayOfMonth(Year + 1, 3, 3);
            }
            else if (Month <= 3)
            {
                if (Date < ThirdWedMarch)
                    return ThirdWedMarch;
                else
                    return ThirdWedJune;
            }
            else if (Month > 3 && Month <= 6)
            {
                if (Date < ThirdWedJune)
                    return ThirdWedJune;
                else
                    return ThirdWedSeptember;
            }
            else if (Month > 6 && Month <= 9)
            {
                if (Date < ThirdWedSeptember)
                    return ThirdWedSeptember;
                else
                    return ThirdWedDecember;
            }
            else if (Month > 9 && Month <= 12)
            {
                return ThirdWedDecember;
            }
            else
                return DateTime.Now;
        }

        public static DateTime AddTenor(DateTime date, string tenor, DayRule dayRule = DayRule.N)
        {
            // To do: proper handling of business days and so forth.

            string tenorType = tenor.Right(1);
            int tenorNumber = Convert.ToInt16(tenor.Left(tenor.Length - 1));

            int AddDays = 0;

            // Roll date forward (unadjusted)

            DateTime newDate;

            switch (tenorType.ToUpper())
            {
                case "D":
                    newDate = date.AddDays((double)tenorNumber);
                    break;
                case "B":
                    newDate = date.AddDays((double)tenorNumber);
                    break;
                case "W":
                    // No method for "AddWeeks" exists by default
                    newDate = date.AddDays((double)tenorNumber * 7);
                    break;
                case "M":
                    newDate = date.AddMonths(tenorNumber);
                    break;
                case "Y":
                    newDate = date.AddYears(tenorNumber);
                    break;
                default:
                    newDate = date;
                    break;
            }

            // Roll date according to dayRule
            string dayRuleStr = EnumToStr.DayRule(dayRule);

            if (dayRule == DayRule.MF || dayRule == DayRule.F)
            {
                switch (newDate.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                    case DayOfWeek.Tuesday:
                    case DayOfWeek.Wednesday:
                    case DayOfWeek.Thursday:
                    case DayOfWeek.Friday:
                        AddDays = 0;
                        break;
                    case DayOfWeek.Saturday:
                        AddDays = 2;
                        break;
                    case DayOfWeek.Sunday:
                        AddDays = 1;
                        break;
                    default:
                        AddDays = 0;
                        break;
                }
            }
            else if (dayRule == DayRule.P)
            {
                switch (newDate.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                    case DayOfWeek.Tuesday:
                    case DayOfWeek.Wednesday:
                    case DayOfWeek.Thursday:
                    case DayOfWeek.Friday:
                        AddDays = 0;
                        break;
                    case DayOfWeek.Saturday:
                        AddDays = -1;
                        break;
                    case DayOfWeek.Sunday:
                        AddDays = -2;
                        break;
                    default:
                        AddDays = 0;
                        break;
                }
            }
            else if (dayRuleStr == "N")
                AddDays = 0;

            // Adjust finally in the case of modified following
            // Should really check here, that the adjusted date is actually a weekday
            if (newDate.Month != date.Month && dayRuleStr.ToUpper() == "MF")
                newDate = newDate.AddDays(AddDays - 3);
            else
                newDate = newDate.AddDays(AddDays);

            return newDate;
        }

        public static DateTime AddTenorAdjust(DateTime date, string tenor, DayRule dayRule = DayRule.N)
        {
            //return AdjustDate(AddTenor(date, tenor, dayRule), dayRule);
            return AddTenor(date, tenor, dayRule);
        }
        public static DateTime AdjustDate(DateTime StartDate, DayRule DayRule)
        {
            return AddTenorAdjust(StartDate, "0B", DayRule);
        }
        public static double Cvg(DateTime StartDate, DateTime EndDate, DayCount DayCountBasis)
        {
            double Coverage = 0.0;

            switch (DayCountBasis)
            {
                case DayCount.ACT360:
                    Coverage = EndDate.Subtract(StartDate).TotalDays / 360.0;
                    break;
                case DayCount.ACT365:
                    Coverage = EndDate.Subtract(StartDate).TotalDays / 365.0;
                    break;
                case DayCount.ACT36525:
                    Coverage = EndDate.Subtract(StartDate).TotalDays / 365.25;
                    break;

                case DayCount.THIRTY360:
                    Coverage = (double)((EndDate.Year - StartDate.Year) * 360 +
                                (EndDate.Month - StartDate.Month) * 30 +
                                Math.Min(30, (int)EndDate.Day) - Math.Min(30, (int)StartDate.Day)) / 360;
                    break;
                default:
                    Coverage = 0.0;
                    break;
            }

            return Coverage;
        }
    }


}
