using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis.Extensions;
using MasterThesis;

namespace MasterThesis
{
    public static class AllowedValues
    {
        private static string[] DayRules = new string[] { "MF", "F", "P" };
        private static string[] TenorSuffix = new string[] { "D", "B", "W", "M", "Y" };
        private static string[] CountBasis = new string[] { "ACT/360", "ACT/365", "ACT/365.25", "30/360" };
        private static string[] Frequencies = new string[] { "1D", "1M", "3M", "6M", "1Y" };
    }

    public class SwapSchedule
    {
        // To do: convert to list (or perhabs not)
        public DateTime[] UnAdjStartDates;
        public DateTime[] UnAdjEndDates;
        public DateTime[] StartDates;
        public DateTime[] EndDates;
        public DateTime AsOf;
        public DateTime StartDate, EndDate;
        public DayCount DayCount;
        public DayRule DayRule;
        public CurveTenor Freq;
        public uint Periods;
        public double[] Coverages;

        public SwapSchedule(DateTime AsOf, DateTime StartDate, DateTime EndDate, DayCount DayCount, DayRule DayRule, CurveTenor Freq)
        {
            this.AsOf = AsOf;
            this.DayCount = DayCount;
            this.DayRule = DayRule;
            this.Freq = Freq;
            GenerateSchedule(AsOf, StartDate, EndDate, DayCount, DayRule, Freq);
        }

        private void GenerateSchedule(DateTime AsOf, DateTime StartDate, DateTime EndDate, DayCount DayCountBasis, DayRule DayRule, CurveTenor Freq)
        {
            // To-Do: determine exact "n"
            // Currently only works with initial short stub.

            DateTime AdjStart = Calender.AdjustDate(StartDate, DayRule);
            DateTime AdjEnd = Calender.AdjustDate(EndDate, DayRule);

            this.StartDate = StartDate;
            this.EndDate = EndDate;
            string FreqStr = EnumToStr.CurveTenor(Freq);

            // Create estimate of how long the schedule should be
            double DaysUpper = AdjEnd.Subtract(AdjStart).TotalDays;
            double DaysLower = Calender.AddTenor(AsOf, FreqStr).Subtract(AsOf).TotalDays;

            uint n = (uint) Math.Ceiling(DaysUpper / DaysLower);

            string TenorType = FreqStr.Right(1);
            int TenorNumber = Convert.ToInt16(FreqStr.Left(FreqStr.Length - 1));
            
            // Date roll
            List<DateTime> DatesTemp = new List<DateTime>();

            int i = 0;
            DatesTemp.Add(EndDate.Date);
            while (Calender.AdjustDate(DatesTemp[i], DayRule).Date > AdjStart.Date)
            {
                string TempTenor = Convert.ToString(-1 * (i+1) * TenorNumber) + TenorType;
                DatesTemp.Add(DateTimeExtensions.Max(Calender.AddTenor(DatesTemp[0], TempTenor), AdjStart).Date);
                i = i + 1;
            }

            // Not currently in use
            int k = DatesTemp.Count;

            UnAdjEndDates = new DateTime[k-1];
            UnAdjStartDates = new DateTime[k-1];
            StartDates = new DateTime[k-1];
            EndDates = new DateTime[k-1];
            Coverages = new double[k-1];
            i = k-1;
            Periods = (uint) i;
            for (int j = 0; j<i; j++)
            {
                if (j == 0)
                    UnAdjStartDates[j] = StartDate.Date;
                else
                    UnAdjStartDates[j] = DatesTemp[i - (j)].Date;

                UnAdjEndDates[j] = DatesTemp[i - j-1].Date;
                StartDates[j] = Calender.AdjustDate(UnAdjStartDates[j], DayRule).Date;
                EndDates[j] = Calender.AdjustDate(UnAdjEndDates[j], DayRule).Date;
                Coverages[j] = Calender.Cvg(StartDates[j], EndDates[j], DayCountBasis);
            }

        }

        // Constructer with tenors instead of dates.
        public SwapSchedule(DateTime AsOf, string StartTenor, string EndTenor, DayCount DayCountBasis, DayRule DayRule, CurveTenor Freq)
        {
            this.AsOf = AsOf;
            DayCount = DayCountBasis;
            this.DayRule = DayRule;
            this.Freq = Freq;
            DateTime UnAdjStartDate = Calender.AddTenor(AsOf, StartTenor);
            DateTime AdjStartDate = Calender.AdjustDate(AsOf, DayRule);
            DateTime UnAdjEndDate = Calender.AddTenor(AdjStartDate, EndTenor);

            GenerateSchedule(AsOf, UnAdjStartDate, UnAdjEndDate, DayCountBasis, DayRule, Freq);
        }
        
        public void Print()
        {
            Console.WriteLine("");
            Console.WriteLine("--------- Printing Schedule of ---------");
            Console.WriteLine("AsOf.......: " + AsOf);
            Console.WriteLine("DayCount...: " + DayCount);
            Console.WriteLine("DayRule....: " + DayRule);
            Console.WriteLine("Frequency..: " + Freq);
            Console.WriteLine("StartDate..: " + StartDate.ToString("dd/MM/yyyy"));
            Console.WriteLine("EndDate....: " + EndDate.ToString("dd/MM/yyyy"));

            Console.WriteLine("");

            var Lines = new List<string[]>();
            Lines.Add(new[] { "UnAdjStart", "UnAdjEnd", "Start", "End", "Cvg" });
            for (int j = 0; j<UnAdjStartDates.Length; j++)
            {
                Lines.Add(new[] { UnAdjStartDates[j].ToString("dd/MM/yyyy"),
                                UnAdjEndDates[j].ToString("dd/MM/yyyy"),
                                StartDates[j].ToString("dd/MM/yyyy"),
                                EndDates[j].ToString("dd/MM/yyyy"),
                                Math.Round(Coverages[j], 2).ToString() });
            }
            var Output = PrintUtility.PrintListNicely(Lines, 3);
            Console.WriteLine(Output);

            Console.WriteLine("------------- Schedule end -------------");
        }

    }
    public static class Calender
    {
        // Something fucks up here ... It Ends at around 2022/01/14 at some points and loops
        public static List<DateTime> IMMSchedule(DateTime StartDate, DateTime EndDate)
        {
            DateTime TempDate = StartDate;
            List<DateTime> MyList = new List<DateTime>();
            int i = 0;
            while (TempDate<EndDate && i < 100)
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
            for (int i = 0; i<MyList.Count; i++)
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
                {
                    return ThirdWedDecember;
                }
                else
                {
                    {
                        return FindThirdWeekdayOfMonth(Year+1,3,3);
                    }
                }
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

        public static DateTime AddTenor(DateTime StartDate, string Tenor, DayRule DayRule = DayRule.N)
        {
            // To do: proper handling of business days and so forth.

            string TenorType = Tenor.Right(1);
            int TenorNumber = Convert.ToInt16(Tenor.Left(Tenor.Length - 1));

            int AddDays = 0;

            // Roll date forward (unadjusted)

            DateTime NewDate;

            switch(TenorType.ToUpper())
            {
                case "D":
                    NewDate = StartDate.AddDays((double)TenorNumber);
                    break;
                case "B":
                    NewDate = StartDate.AddDays((double)TenorNumber);
                    break;
                case "W":
                    // No method for "AddWeeks" exists by default
                    NewDate = StartDate.AddDays((double)TenorNumber * 7);
                    break;
                case "M":
                    NewDate = StartDate.AddMonths(TenorNumber);
                    break;
                case "Y":
                    NewDate = StartDate.AddYears(TenorNumber);
                    break;
                default:
                    NewDate = StartDate;
                    break;
            }

            // Roll date according to dayRule
            string DayRuleStr = EnumToStr.DayRule(DayRule);

            if (DayRuleStr.ToUpper() == "MF" || DayRuleStr.ToUpper() == "F")
            {
                switch (NewDate.DayOfWeek)
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
            else if (DayRuleStr.ToUpper() == "P")
            {
                switch (NewDate.DayOfWeek)
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
            else if (DayRuleStr == "N")
                AddDays = 0;

            // Adjust finally in the case of modified following
            // Should really check here, that the adjusted date is actually a weekday
            if (NewDate.Month != StartDate.Month && DayRuleStr.ToUpper() == "MF")
                NewDate = NewDate.AddDays(AddDays - 3);

            return NewDate;
        }
        public static DateTime AdjustDate(DateTime StartDate, DayRule DayRule)
        {
            return AddTenor(StartDate, "0B", DayRule);
        }
        public static double Cvg(DateTime StartDate, DateTime EndDate, DayCount DayCountBasis)
        {
            double Coverage = 0.0;

            switch(DayCountBasis)
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
                    Coverage = (double) ((EndDate.Year - StartDate.Year)*360 + 
                                (EndDate.Month - StartDate.Month)*30 + 
                                Math.Min(30, (int) EndDate.Day) - Math.Min(30, (int) StartDate.Day))/ 360;
                    break;
                default:
                    Coverage = 0.0;
                    break;
            }

            return Coverage;
        } 


    }


}
