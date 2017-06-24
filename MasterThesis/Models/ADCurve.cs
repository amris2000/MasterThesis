using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis.Extensions;

namespace MasterThesis
{
    /* --- General information
    * 
    */

    public class Curve_AD
    {
        public List<DateTime> Dates;
        public List<ADouble> Values;
        public int Dimension { get; private set; }
        public CurveTenor Frequency { get; private set; }

        public Curve_AD(List<DateTime> Dates, List<ADouble> Values)
        {
            this.Dates = Dates;
            this.Values = Values;
            this.Frequency = CurveTenor.Simple;
            this.Dimension = Values.Count;
        }

        public ADouble Interp(DateTime date, InterpMethod interpolation)
        {
            return MyMath.InterpolateCurve(Dates, date, Values, interpolation);
        }
        public ADouble ZeroRate(DateTime date, InterpMethod interpolation)
        {
            return this.Interp(date, interpolation);
        }
        public ADouble DiscFactor(DateTime asOf, DateTime date, DayCount dayCount, InterpMethod interpolation)
        {
            return ADouble.Exp(-1.0 * ZeroRate(date, interpolation) * DateHandling.Cvg(asOf, date, dayCount));
        }
        public ADouble FwdRate(DateTime asOf, DateTime startDate, DateTime endDate, DayRule dayRule, DayCount dayCount, InterpMethod interpolation)
        {
            ADouble ps = DiscFactor(asOf, startDate, dayCount, interpolation);
            ADouble pe = DiscFactor(asOf, endDate, dayCount, interpolation);
            ADouble cvg = DateHandling.Cvg(startDate, endDate, dayCount);

            return (ps / pe - 1.0) / cvg;
        }
        public void Print()
        {
            for (int i = 0; i < Dates.Count; i++)
            {
                Console.WriteLine(Dates[i].Date + " " + Math.Round(Values[i], 5));
            }
        }

        /// <summary>
        /// Calculate annuity of an OIS schedule.
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="interpolation"></param>
        /// <returns></returns>
        public ADouble OisAnnuityAD(OisSchedule schedule, InterpMethod interpolation)
        {
            ADouble output = 0.0;
            ADouble discFactor;
            for (int i = 0; i < schedule.AdjEndDates.Count; i++)
            {
                discFactor = DiscFactor(schedule.AsOf, schedule.AdjEndDates[i], schedule.DayCount, interpolation);
                output = output + schedule.Coverages[i] * discFactor;
            }
            return output;
        }


        public ADouble OisSwapNpvAD(OisSwap swap, InterpMethod interpolation)
        {
            ADouble oisAnnuity = OisAnnuityAD(swap.FloatSchedule, interpolation);
            double notional = swap.TradeSign*swap.Notional;
            ADouble oisRate = OisRateSimpleAD(swap, interpolation);
            return notional * (swap.FixedRate - 1.0*oisRate) * oisAnnuity;
        }

        /// <summary>
        /// Not used
        /// </summary>
        /// <param name="asOf"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="dayRule"></param>
        /// <param name="dayCount"></param>
        /// <param name="interpolation"></param>
        /// <returns></returns>
        public double OisCompoundedRateAD(DateTime asOf, DateTime startDate, DateTime endDate, DayRule dayRule, DayCount dayCount, InterpMethod interpolation)
        {
            double CompoundedRate = 1;
            double CompoundedRate2 = 1;
            DateTime RollDate = startDate;
            while (RollDate.Date < endDate.Date)
            {
                DateTime NextBusinessDay = DateHandling.AddTenorAdjust(RollDate, "1B", DayRule.F);
                //double Rate = DiscCurve.ZeroRate(asOf, startDate, RollDate, dayRule, dayCount, method);
                double Rate = ZeroRate(NextBusinessDay, interpolation);
                double fwdOisRate = FwdRate(asOf, RollDate, NextBusinessDay, DayRule.F, dayCount, interpolation);

                double disc1 = DiscFactor(asOf, RollDate, dayCount, interpolation);
                double disc2 = DiscFactor(asOf, NextBusinessDay, dayCount, interpolation);

                double Days = NextBusinessDay.Subtract(RollDate).TotalDays;
                double shortCvg = DateHandling.Cvg(RollDate, NextBusinessDay, dayCount);
                RollDate = NextBusinessDay;
                CompoundedRate *= (1 + fwdOisRate * shortCvg);
                CompoundedRate2 *= disc1 / disc2;
            }
            double coverage = DateHandling.Cvg(startDate, endDate, dayCount);
            return (CompoundedRate2 - 1) / coverage;
        }

        /// <summary>
        /// Calculate the par OIS rate by compounding. Slow and probably not entirely correct (although close)
        /// </summary>
        /// <param name="swap"></param>
        /// <param name="interpolation"></param>
        /// <returns></returns>
        public ADouble OisRateAD(OisSwap swap, InterpMethod interpolation)
        {
            ADouble floatContribution = 0.0;
            ADouble annuity = OisAnnuityAD(swap.FixedSchedule, interpolation);

            DateTime asOf = swap.FloatSchedule.AsOf;

            for (int i = 0; i < swap.FloatSchedule.AdjEndDates.Count; i++)
            {
                DateTime startDate = swap.FloatSchedule.AdjStartDates[i];
                DateTime endDate = swap.FloatSchedule.AdjEndDates[i];
                ADouble compoundedRate = OisCompoundedRateAD(asOf, startDate, endDate, swap.FloatSchedule.DayRule, swap.FloatSchedule.DayCount, interpolation);
                ADouble discFactor = DiscFactor(asOf, endDate, swap.FixedSchedule.DayCount, interpolation);
                ADouble coverage = DateHandling.Cvg(startDate, endDate, swap.FloatSchedule.DayCount);
                floatContribution = floatContribution + discFactor * compoundedRate * coverage;
            }
            return floatContribution / annuity;
        }

        /// <summary>
        /// Simple calculation of par OIS rate. Holds only under the assumption
        /// that FRA's can be perfectly hedged by OIS zero coupon bonds.
        /// </summary>
        /// <param name="swap"></param>
        /// <param name="interpolation"></param>
        /// <returns></returns>
        public ADouble OisRateSimpleAD(OisSwap swap, InterpMethod interpolation)
        {
            ADouble annuity = OisAnnuityAD(swap.FixedSchedule, interpolation);

            return (DiscFactor(swap.AsOf, swap.StartDate, swap.FixedSchedule.DayCount, interpolation) - DiscFactor(swap.AsOf, swap.EndDate, swap.FixedSchedule.DayCount, interpolation)) / annuity;
        }
    }

    public class ADFwdCurveContainer
    {
        public IDictionary<CurveTenor, Curve_AD> Curves { get; private set; }

        public ADFwdCurveContainer()
        {
            Curves = new Dictionary<CurveTenor, Curve_AD>();
        }
        public void AddCurve(Curve_AD curve, CurveTenor curveType)
        {
            Curves[curveType] = curve;
        }
        public void AddCurve(List<DateTime> dates, List<ADouble> values, CurveTenor tenor)
        {

            Curve_AD newCurve = new MasterThesis.Curve_AD(dates, values);
            Curves[tenor] = newCurve;
        }

        public bool CurveExist(CurveTenor tenor)
        {
            return Curves.ContainsKey(tenor);
        }

        public void UpdateCurveValues(List<ADouble> values, CurveTenor tenor)
        {
            Curves[tenor].Values = values;
        }

        public Curve_AD GetCurve(CurveTenor curveType)
        {
            return Curves[curveType];
        }
    }
}
