using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis.Conv
{
    public static class IrSwap6M
    {
        public static CurveTenor FixedFreq = CurveTenor.Fwd1Y;
        public static CurveTenor FloatFreq = CurveTenor.Fwd6M;
        public static DayRule FixedDayRule = DayRule.MF;
        public static DayRule FloatDayRule = DayRule.MF;
        public static DayCount FixedDayCount = DayCount.THIRTY360;
        public static DayCount FloatDayCount = DayCount.ACT360;
        public static string SpotLag = "2B";
    }

    public static class OldDateHandling
    {
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
    }
    /* Old day rolling
     *             // Only modified following case left


            if (dayRule == DayRule.MF || dayRule == DayRule.F)
            {
                switch (unadjustedDate.DayOfWeek)
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
                switch (unadjustedDate.DayOfWeek)
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
            else if (dayRule = DayRule.N)
                AddDays = 0;

            // Adjust finally in the case of modified following
            // Should really check here, that the adjusted date is actually a weekday
            if (unadjustedDate.Month != date.Month && dayRule == DayRule.MF)
                unadjustedDate = unadjustedDate.AddDays(AddDays - 3);
            else
                unadjustedDate = unadjustedDate.AddDays(AddDays);

            return unadjustedDate;
            //return AdjustDate(AddTenor(date, tenor, dayRule), dayRule);
            return AddTenor(date, tenor, dayRule);
        */
}
