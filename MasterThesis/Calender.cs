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

    public class OisSchedule
    {
        public List<DateTime> UnAdjStartDates = new List<DateTime>();
        public List<DateTime> UnAdjEndDates = new List<DateTime>();
        public List<DateTime> AdjStartDates = new List<DateTime>();
        public List<DateTime> AdjEndDates = new List<DateTime>();
        public List<double> Coverages = new List<double>();
        public DateTime AsOf;
        public DateTime StartDate, EndDate;
        public DayCount DayCount;
        public DayRule DayRule;
        public string EndTenor;
        public uint Periods;

        public static double ConvertTenorToYearFraction(string tenor)
        {
            double multiplier = 0.0;
            int number = Convert.ToInt16(tenor.Replace(tenor.Right(1), ""));

            switch(tenor.Right(1).ToUpper())
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
                case "B": case "D":
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

        public OisSchedule(DateTime asOf, string startTenor, string endTenor, string settlementLag, DayCount dayCount, DayRule dayRule)
        {
            AsOf = asOf;
            DateTime startDate = Functions.AddTenorAdjust(asOf, settlementLag, dayRule);
            DateTime endDate = Functions.AddTenorAdjust(startDate, endTenor, dayRule);
            StartDate = startDate;
            EndDate = endDate;
            Tenor startTenorEnum = GetLetterFromTenor(startTenor);
            Tenor endTenorEnum = GetLetterFromTenor(endTenor);

            UnAdjStartDates.Add(startDate);
            AdjStartDates.Add(startDate);


            // Just to make sure we compare with both "1Y" and "12M" (because i'm lazy in correcting for DayCount)
            if (CompareTenors(endTenor, "1Y") == false && CompareTenors(endTenor, "12M") == false)
            {
                // Simple OIS swap
                double cvg = Functions.Cvg(startDate, endDate, dayCount);
                AdjEndDates.Add(endDate);
                AdjStartDates.Add(startDate);
                Coverages.Add(cvg);
                return;
            }
            else
            {
                bool onlyWholePeriods = false;
                int months = 0;

                // 1Y periods + stub
                int periods, years;

                if (endTenorEnum == Tenor.Y)
                {
                    periods = GetNumberFromTenor(endTenor);
                    years = periods;
                    onlyWholePeriods = true;
                }
                else if (endTenorEnum == Tenor.M)
                {
                    years = (int)Math.Truncate(GetNumberFromTenor(endTenor) / 12.0);

                    if (GetNumberFromTenor(endTenor) % 12 == 0)
                    {
                        months = 0;
                        periods = years;
                        onlyWholePeriods = true;
                    }
                    else
                    {
                        months = GetNumberFromTenor(endTenor) - 12 * years;
                        periods = years + 1;
                        onlyWholePeriods = false;
                    }
                }
                else
                    throw new InvalidOperationException("OIS Schedule only works for Y,M endTenors");

                UnAdjEndDates.Add(Functions.AddTenorAdjust(startDate, "1Y"));
                AdjEndDates.Add(Functions.AdjustDate(UnAdjEndDates[0], DayRule.N));
                Coverages.Add(Functions.Cvg(AdjStartDates[0], AdjEndDates[0], DayCount));

                // Generate Schedule
                // We start from 1 since first days are filled out
                // We only end here if we have more than 1 period
                for (int j = 1; j <= periods; j++)
                {
                    if (periods > years && periods == j + 1) // In case we have tenor like "18M" and have to create a stub periods
                    {
                        string excessTenor = months.ToString() + "M";
                        UnAdjStartDates.Add(Functions.AddTenorAdjust(UnAdjStartDates[j - 1], "1Y", dayRule));
                        AdjStartDates.Add(Functions.AdjustDate(UnAdjStartDates[j], dayRule));
                        UnAdjEndDates.Add(Functions.AddTenorAdjust(UnAdjEndDates[j - 1], excessTenor, dayRule));
                        AdjEndDates.Add(Functions.AdjustDate(UnAdjEndDates[j], dayRule));
                        Coverages.Add(Functions.Cvg(AdjStartDates[j], AdjEndDates[j], DayCount));
                    }
                    else
                    {
                        if (j<periods)
                        {
                            UnAdjStartDates.Add(Functions.AddTenorAdjust(UnAdjStartDates[j - 1], "1Y", dayRule));
                            AdjStartDates.Add(Functions.AdjustDate(UnAdjStartDates[j], dayRule));
                            UnAdjEndDates.Add(Functions.AddTenorAdjust(UnAdjEndDates[j - 1], "1Y", dayRule));
                            AdjEndDates.Add(Functions.AdjustDate(UnAdjEndDates[j], dayRule));
                            Coverages.Add(Functions.Cvg(AdjStartDates[j], AdjEndDates[j], DayCount));
                        }
                    }
                }
            }
        }

        public OisSchedule(DateTime asOf, DateTime startDate, DayCount dayCount, DayRule dayRule, string Tenor)
        {
            double TenorNumber = Functions.ParseTenor(Tenor).Item1;
            string TenorLetter = Functions.ParseTenor(Tenor).Item2;

            this.AsOf = asOf;
            this.StartDate = startDate;
            this.EndDate = Functions.AddTenorAdjust(StartDate, Tenor);

            DateTime AdjStart = Functions.AdjustDate(startDate, dayRule);
            DateTime AdjEnd = Functions.AdjustDate(EndDate, DayRule);

            int Years = 0;
            int Periods = 0;
            string DivTenor = "0B";

            if (TenorLetter.ToUpper() == "M")
            {
                Years = (int)Math.Truncate(TenorNumber) / 12;
                if (TenorNumber % 12 == 0)
                    Periods = Years;
                else
                {
                    Periods = Years + 1;
                    DivTenor = (TenorNumber % 12).ToString() + "M";
                }
            }
            else if (TenorLetter.ToUpper() == "Y")
            {
                Years = (int)Math.Truncate(TenorNumber);
                Periods = Years;
            }
            else if (TenorLetter.ToUpper() == "W" || TenorLetter.ToUpper() == "B")
                Periods = 1;
            else
                throw new ArgumentException("This only works for M and Y tenors..");

            UnAdjStartDates.Add(StartDate);
            AdjStartDates.Add(AdjStart);


            if (Periods == 1)
            {
                UnAdjEndDates.Add(EndDate);
                AdjEndDates.Add(AdjEnd);
                Coverages.Add(Functions.Cvg(AdjStart, AdjEnd, DayCount));
                return;
            }
            else
            {
                UnAdjEndDates.Add(Functions.AddTenorAdjust(StartDate, "1Y"));
                AdjEndDates.Add(Functions.AdjustDate(UnAdjEndDates[0], DayRule.N));
                Coverages.Add(Functions.Cvg(AdjStartDates[0], AdjEndDates[0], DayCount));
            }

            for (int j = 1; j < Periods; j++)
            {
                if (Periods > Years && Periods == j + 1) // In case we have tenor like "18M" and have to create a stub periods
                {
                    UnAdjStartDates.Add(Functions.AddTenorAdjust(UnAdjStartDates[j - 1], "1Y", dayRule));
                    AdjStartDates.Add(Functions.AdjustDate(UnAdjStartDates[j], dayRule));
                    UnAdjEndDates.Add(Functions.AddTenorAdjust(UnAdjEndDates[j - 1], DivTenor, dayRule));
                    AdjEndDates.Add(Functions.AdjustDate(UnAdjEndDates[j], dayRule));
                    Coverages.Add(Functions.Cvg(AdjStartDates[j], AdjEndDates[j], DayCount));
                }
                else
                {
                    UnAdjStartDates.Add(Functions.AddTenorAdjust(UnAdjStartDates[j - 1], "1Y", dayRule));
                    AdjStartDates.Add(Functions.AdjustDate(UnAdjStartDates[j], dayRule));
                    UnAdjEndDates.Add(Functions.AddTenorAdjust(UnAdjEndDates[j - 1], "1Y", dayRule));
                    AdjEndDates.Add(Functions.AdjustDate(UnAdjEndDates[j], dayRule));
                    Coverages.Add(Functions.Cvg(AdjStartDates[j], AdjEndDates[j], DayCount));
                }
            }
        }
        public void Print()
        {
            Console.WriteLine("");
            Console.WriteLine("--------- Printing OIS Schedule ---------");
            Console.WriteLine("AsOf.......: " + AsOf);
            Console.WriteLine("DayCount...: " + DayCount);
            Console.WriteLine("DayRule....: " + DayRule);
            Console.WriteLine("Frequency..: " + EndTenor);
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

    public class SwapSchedule
    {
        // To do: convert to list (or perhabs not)
        public List<DateTime> UnAdjStartDates = new List<DateTime>();
        public List<DateTime> UnAdjEndDates = new List<DateTime>();
        public List<DateTime> AdjStartDates = new List<DateTime>();
        public List<DateTime> AdjEndDates = new List<DateTime>();
        public List<double> Coverages = new List<double>();
        public DateTime AsOf;
        public DateTime StartDate, EndDate;
        public DayCount DayCount;
        public DayRule DayRule;
        public CurveTenor Freq;
        public uint Periods;

        public SwapSchedule(DateTime asOf, DateTime startDate, DateTime endDate, DayCount dayCount, DayRule dayRule, CurveTenor tenor, StubPlacement stub = StubPlacement.NullStub)
        {
            this.AsOf = asOf;
            this.DayCount = dayCount;
            this.DayRule = dayRule;
            this.Freq = tenor;
            this.StartDate = startDate;
            this.EndDate = endDate;

            GenerateSchedule(asOf, startDate, endDate, dayCount, dayRule, tenor, stub);
        }

        private void GenerateSchedule(DateTime asOf, DateTime startDate, DateTime endDate, DayCount dayCount, DayRule dayRule, CurveTenor tenor, StubPlacement stub = StubPlacement.NullStub)
        {
            // This only works for short stubs atm, although NullStub will generate a long stub

            DateTime AdjStart = Functions.AdjustDate(startDate, dayRule);
            DateTime AdjEnd = Functions.AdjustDate(endDate, dayRule);

            string FreqStr = EnumToStr.CurveTenor(tenor);
            string TenorLetter = Functions.ParseTenor(FreqStr).Item2;
            double TenorNumber = Functions.ParseTenor(FreqStr).Item1;

            // Create estimate of how long the schedule should be
            double YearsUpper = Functions.Cvg(AdjStart, AdjEnd, dayCount);
            double YearLower = Functions.Cvg(AsOf, AdjStart, dayCount);

            int WholePeriods = 0;
#pragma warning disable CS0219 // Variable is assigned but its value is never used
            double Excess = 0.0;
#pragma warning restore CS0219 // Variable is assigned but its value is never used

            // Will be sorted at end (when coverages are also calculated)
            UnAdjStartDates.Add(StartDate);
            UnAdjEndDates.Add(EndDate);
            AdjStartDates.Add(AdjStart);
            AdjEndDates.Add(AdjEnd);

            if (TenorLetter.ToUpper() == "M")
            {
                WholePeriods = (int) Math.Truncate(YearsUpper) * 12 / (int) Math.Round(TenorNumber);
            }
            else if (TenorLetter.ToUpper() == "Y")
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
                    UnAdjEndDates.Add(Functions.AddTenorAdjust(UnAdjEndDates[i-1], "-" + FreqStr));
                    AdjEndDates.Add(Functions.AdjustDate(UnAdjEndDates[i],DayRule));
                    UnAdjStartDates.Add(UnAdjEndDates[i]);
                    AdjStartDates.Add(Functions.AdjustDate(UnAdjStartDates[i], DayRule));
                }

            }
            else if (stub == StubPlacement.End)
            {
                WholePeriods += 1 * 12 / (int)Math.Round(TenorNumber);
                for (int i = 1; i < WholePeriods; i++)
                {
                    UnAdjStartDates.Add(Functions.AddTenorAdjust(UnAdjStartDates[i - 1], FreqStr));
                    AdjStartDates.Add(Functions.AdjustDate(UnAdjStartDates[i], DayRule));
                    UnAdjEndDates.Add(UnAdjStartDates[i]);
                    AdjEndDates.Add(Functions.AdjustDate(UnAdjEndDates[i], DayRule));
                }
            }
            else if (stub == StubPlacement.NullStub)
            {
                for (int i = 1; i<WholePeriods; i++)
                {
                    UnAdjEndDates.Add(Functions.AddTenorAdjust(UnAdjEndDates[i - 1], "-" + FreqStr));
                    AdjEndDates.Add(Functions.AdjustDate(UnAdjEndDates[i], DayRule));
                    UnAdjStartDates.Add(UnAdjEndDates[i]);
                    AdjStartDates.Add(Functions.AdjustDate(UnAdjStartDates[i], DayRule));
                }

            }
            // Sort dates according to date
            UnAdjStartDates.Sort(new Comparison<DateTime>((x, y) => x.CompareTo(y)));
            UnAdjEndDates.Sort(new Comparison<DateTime>((x, y) => x.CompareTo(y)));
            AdjStartDates.Sort(new Comparison<DateTime>((x, y) => x.CompareTo(y)));
            AdjEndDates.Sort(new Comparison<DateTime>((x, y) => x.CompareTo(y)));

            for (int i = 0; i<AdjStartDates.Count; i++)
            {
                Coverages.Add(Functions.Cvg(AdjStartDates[i], AdjEndDates[i], DayCount));
            }
        }

        // Constructer with tenors instead of dates.
        public SwapSchedule(DateTime AsOf, string StartTenor, string EndTenor, DayCount DayCountBasis, DayRule DayRule, CurveTenor Freq)
        {
            this.AsOf = AsOf;
            DayCount = DayCountBasis;
            this.DayRule = DayRule;
            this.Freq = Freq;
            DateTime UnAdjStartDate = Functions.AddTenorAdjust(AsOf, StartTenor);
            DateTime AdjStartDate = Functions.AdjustDate(AsOf, DayRule);
            DateTime UnAdjEndDate = Functions.AddTenorAdjust(AdjStartDate, EndTenor);

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
            for (int j = 0; j<UnAdjStartDates.Count; j++)
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
}
