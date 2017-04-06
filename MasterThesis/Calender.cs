using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis.Extensions;
using MasterThesis;

namespace MasterThesis
{
    public abstract class Schedule
    {
        public List<DateTime> UnAdjStartDates { get; protected set; }
        public List<DateTime> UnAdjEndDates { get; protected set; }
        public List<DateTime> AdjStartDates { get; protected set; }
        public List<DateTime> AdjEndDates { get; protected set; }
        public List<double> Coverages { get; protected set; }

        public DateTime AsOf { get; protected set; }
        public DateTime StartDate { get; protected set; }
        public DateTime EndDate { get; protected set; }
        public DayCount DayCount { get; protected set; }
        public DayRule DayRule { get; protected set; }
        public uint Periods { get; protected set; }

        // Move these to some other general class
        public static double ConvertTenorToYearFraction(string tenor)
        {
            double multiplier = 0.0;
            int number = Convert.ToInt16(tenor.Replace(tenor.Right(1), ""));

            switch (tenor.Right(1).ToUpper())
            {
                case "M":
                    multiplier = 31;
                    break;
                case "Y":
                    multiplier = 365;
                    break;
                case "W":
                    multiplier = 7;
                    break;
                case "B":
                case "D":
                    multiplier = 1;
                    break;
                default:
                    throw new InvalidOperationException("Cannot ConvertTenorToYearFraction, tenor: " + tenor);
            }
            return number * multiplier / 365;
        }
        public static bool CompareTenors(string tenorIsLarger, string comparisonTenor)
        {
            if (ConvertTenorToYearFraction(tenorIsLarger) > ConvertTenorToYearFraction(comparisonTenor))
                return true;
            else
                return false;
        }
        public static int GetNumberFromTenor(string tenor)
        {
            return Convert.ToInt16(tenor.Replace(tenor.Right(1), ""));
        }
        public static Tenor GetLetterFromTenor(string tenor)
        {
            return StrToEnum.TenorConvert(tenor.Right(1));
        }

        public Schedule(DateTime asOf, DateTime startDate, DateTime endDate, DayCount dayCount, DayRule dayRule)
        {
            SetValues(asOf, startDate, endDate, dayCount, dayRule);
        }

        public Schedule(DateTime asOf, string startTenor, string endTenor, DayCount dayCount, DayRule dayRule)
        {
            DateTime startDate = DateHandling.AddTenor(asOf, startTenor, dayRule);
            DateTime endDate = DateHandling.AddTenor(startDate, endTenor, dayRule);

            SetValues(asOf, startDate, endDate, dayCount, dayRule);
        }

        public void SetValues(DateTime asOf, DateTime startDate, DateTime endDate, DayCount dayCount, DayRule dayRule)
        {
            UnAdjEndDates = new List<DateTime>();
            UnAdjStartDates = new List<DateTime>();
            AdjStartDates = new List<DateTime>();
            AdjEndDates = new List<DateTime>();
            Coverages = new List<double>();

            AsOf = asOf;
            StartDate = startDate;
            EndDate = endDate;
            DayCount = dayCount;
            DayRule = dayRule;
        }

        public void Print()
        {
            Console.WriteLine("");
            Console.WriteLine("--------- Printing OIS Schedule ---------");
            Console.WriteLine("AsOf.......: " + AsOf);
            Console.WriteLine("DayCount...: " + DayCount);
            Console.WriteLine("DayRule....: " + DayRule);
            Console.WriteLine("StartDate..: " + StartDate.ToString("dd/MM/yyyy"));
            Console.WriteLine("EndDate....: " + EndDate.ToString("dd/MM/yyyy"));

            Console.WriteLine("");

            var Lines = new List<string[]>();
            Lines.Add(new[] { "UnAdjStart", "UnAdjEnd", "Start", "End", "Cvg" });
            for (int j = 0; j < UnAdjStartDates.Count; j++)
            {
                Lines.Add(new[] { UnAdjStartDates[j].ToString("dd/MM/yyyy"),
                                UnAdjEndDates[j].ToString("dd/MM/yyyy"),
                                AdjStartDates[j].ToString("dd/MM/yyyy"),
                                AdjEndDates[j].ToString("dd/MM/yyyy"),
                                Math.Round(Coverages[j], 2).ToString() });
            }
            var Output = PrintUtility.PrintListNicely(Lines, 3);
            Console.WriteLine(Output);

            Console.WriteLine("------------- Schedule end -------------");
        }
    }

    // Ideally, this should be a derived class on SwapSchedule, since an
    // OIS schedule (in this context) is either a short period, or 1Y schedule with 
    // a stub in the end. Nice to have
    public class OisSchedule : Schedule
    {
        public OisSchedule(DateTime asOf, string startTenor, string endTenor, string settlementLag, DayCount dayCount, DayRule dayRule)
            : base(asOf, startTenor, endTenor, dayCount, dayRule)
        {
            Tenor startTenorEnum = GetLetterFromTenor(startTenor);
            Tenor endTenorEnum = GetLetterFromTenor(endTenor);

            UnAdjStartDates.Add(StartDate);
            AdjStartDates.Add(StartDate);

            // Just to make sure we compare with both "1Y" and "12M" (because i'm lazy in correcting for DayCount)
            if (CompareTenors(endTenor, "1Y") == false && CompareTenors(endTenor, "12M") == false)
            {
                // Simple OIS swap
                double cvg = DateHandling.Cvg(StartDate, EndDate, dayCount);
                AdjEndDates.Add(EndDate);
                AdjStartDates.Add(StartDate);
                Coverages.Add(cvg);
                return;
            }
            else
            {
                int months = 0;

                // 1Y periods + stub
                int periods, years;

                if (endTenorEnum == Tenor.Y)
                {
                    periods = GetNumberFromTenor(endTenor);
                    years = periods;
                }
                else if (endTenorEnum == Tenor.M)
                {
                    years = (int)Math.Truncate(GetNumberFromTenor(endTenor) / 12.0);

                    if (GetNumberFromTenor(endTenor) % 12 == 0)
                    {
                        months = 0;
                        periods = years;
                    }
                    else
                    {
                        months = GetNumberFromTenor(endTenor) - 12 * years;
                        periods = years + 1;
                    }
                }
                else
                    throw new InvalidOperationException("OIS Schedule only works for Y,M endTenors");

                UnAdjEndDates.Add(DateHandling.AddTenorAdjust(StartDate, "1Y"));
                AdjEndDates.Add(DateHandling.AdjustDate(UnAdjEndDates[0], DayRule.N));
                Coverages.Add(DateHandling.Cvg(AdjStartDates[0], AdjEndDates[0], DayCount));

                // Generate Schedule
                // We start from 1 since first days are filled out
                // We only end here if we have more than 1 period
                for (int j = 1; j <= periods; j++)
                {
                    if (periods > years && periods == j + 1) // In case we have tenor like "18M" and have to create a stub periods
                    {
                        string excessTenor = months.ToString() + "M";
                        UnAdjStartDates.Add(DateHandling.AddTenorAdjust(UnAdjStartDates[j - 1], "1Y", dayRule));
                        AdjStartDates.Add(DateHandling.AdjustDate(UnAdjStartDates[j], dayRule));
                        UnAdjEndDates.Add(DateHandling.AddTenorAdjust(UnAdjEndDates[j - 1], excessTenor, dayRule));
                        AdjEndDates.Add(DateHandling.AdjustDate(UnAdjEndDates[j], dayRule));
                        Coverages.Add(DateHandling.Cvg(AdjStartDates[j], AdjEndDates[j], DayCount));
                    }
                    else
                    {
                        if (j<periods)
                        {
                            UnAdjStartDates.Add(DateHandling.AddTenorAdjust(UnAdjStartDates[j - 1], "1Y", dayRule));
                            AdjStartDates.Add(DateHandling.AdjustDate(UnAdjStartDates[j], dayRule));
                            UnAdjEndDates.Add(DateHandling.AddTenorAdjust(UnAdjEndDates[j - 1], "1Y", dayRule));
                            AdjEndDates.Add(DateHandling.AdjustDate(UnAdjEndDates[j], dayRule));
                            Coverages.Add(DateHandling.Cvg(AdjStartDates[j], AdjEndDates[j], DayCount));
                        }
                    }
                }
            }
        }
    }

    public class SwapSchedule : Schedule
    {
        public CurveTenor Frequency { get; }
        public StubPlacement Stub;

        public SwapSchedule(DateTime asOf, DateTime startDate, DateTime endDate, DayCount dayCount, DayRule dayRule, CurveTenor frequency, StubPlacement stub = StubPlacement.NullStub)
            : base(asOf, startDate, endDate, dayCount, dayRule)
        {
            this.Frequency = frequency;
            this.Stub = stub;
            GenerateSchedule(asOf, startDate, endDate, dayCount, dayRule, frequency, stub);
        }

        private void GenerateSchedule(DateTime asOf, DateTime startDate, DateTime endDate, DayCount dayCount, DayRule dayRule, CurveTenor tenor, StubPlacement stub = StubPlacement.NullStub)
        {
            // This only works for short stubs atm, although NullStub will generate a long stub

            DateTime AdjStart = DateHandling.AdjustDate(startDate, dayRule);
            DateTime AdjEnd = DateHandling.AdjustDate(endDate, dayRule);

            string tenorString = EnumToStr.CurveTenor(tenor);
            string TenorLetter = DateHandling.GetTenorLetterFromTenor(tenorString);
            double TenorNumber = DateHandling.GetTenorNumberFromTenor(tenorString);

            // Create estimate of how long the schedule should be
            double YearsUpper = DateHandling.Cvg(AdjStart, AdjEnd, dayCount);
            double YearLower = DateHandling.Cvg(AsOf, AdjStart, dayCount);

            int WholePeriods = 0;
#pragma warning disable CS0219 // Variable is assigned but its value is never used
            double Excess = 0.0;
#pragma warning restore CS0219 // Variable is assigned but its value is never used

            // Will be sorted at end (when coverages are also calculated)
            UnAdjStartDates.Add(StartDate);
            UnAdjEndDates.Add(EndDate);
            AdjStartDates.Add(AdjStart);
            AdjEndDates.Add(AdjEnd);

            if (StrToEnum.ConvertTenorLetter(TenorLetter) == Tenor.M)
            {
                WholePeriods = (int) Math.Truncate(YearsUpper) * 12 / (int) Math.Round(TenorNumber);
            }
            else if (StrToEnum.ConvertTenorLetter(TenorLetter) == Tenor.Y)
            {
                WholePeriods = (int) Math.Truncate(YearsUpper);
            }
            else
            {
                throw new ArgumentException("Can only roll out swap calender for month and year tenors");
            }
            
            if (stub == StubPlacement.Beginning)
            {
                WholePeriods += 1 * 12 / (int) Math.Round(TenorNumber);
                for (int i = 1; i<WholePeriods; i++)
                {
                    UnAdjEndDates.Add(DateHandling.AddTenorAdjust(UnAdjEndDates[i-1], "-" + tenorString));
                    AdjEndDates.Add(DateHandling.AdjustDate(UnAdjEndDates[i],DayRule));
                    UnAdjStartDates.Add(UnAdjEndDates[i]);
                    AdjStartDates.Add(DateHandling.AdjustDate(UnAdjStartDates[i], DayRule));
                }

            }
            else if (stub == StubPlacement.End)
            {
                WholePeriods += 1 * 12 / (int)Math.Round(TenorNumber);
                for (int i = 1; i < WholePeriods; i++)
                {
                    UnAdjStartDates.Add(DateHandling.AddTenorAdjust(UnAdjStartDates[i - 1], tenorString));
                    AdjStartDates.Add(DateHandling.AdjustDate(UnAdjStartDates[i], DayRule));
                    UnAdjEndDates.Add(UnAdjStartDates[i]);
                    AdjEndDates.Add(DateHandling.AdjustDate(UnAdjEndDates[i], DayRule));
                }
            }
            else if (stub == StubPlacement.NullStub)
            {
                for (int i = 1; i<WholePeriods; i++)
                {
                    UnAdjEndDates.Add(DateHandling.AddTenorAdjust(UnAdjEndDates[i - 1], "-" + tenorString));
                    AdjEndDates.Add(DateHandling.AdjustDate(UnAdjEndDates[i], DayRule));
                    UnAdjStartDates.Add(UnAdjEndDates[i]);
                    AdjStartDates.Add(DateHandling.AdjustDate(UnAdjStartDates[i], DayRule));
                }

            }
            // Sort dates according to date
            UnAdjStartDates.Sort(new Comparison<DateTime>((x, y) => x.CompareTo(y)));
            UnAdjEndDates.Sort(new Comparison<DateTime>((x, y) => x.CompareTo(y)));
            AdjStartDates.Sort(new Comparison<DateTime>((x, y) => x.CompareTo(y)));
            AdjEndDates.Sort(new Comparison<DateTime>((x, y) => x.CompareTo(y)));


            for (int i = 0; i<AdjStartDates.Count; i++)
            {
                Coverages.Add(DateHandling.Cvg(AdjStartDates[i], AdjEndDates[i], DayCount));
            }
        }

        // Constructer with tenors instead of dates.
        public SwapSchedule(DateTime asOf, string startTenor, string endTenor, DayCount dayCount, DayRule dayRule, CurveTenor frequency)
            : base(asOf, startTenor, endTenor, dayCount, dayRule)
        {
            this.Frequency = frequency;
            GenerateSchedule(asOf, StartDate, EndDate, dayCount, dayRule, frequency);
        }
    }
}
