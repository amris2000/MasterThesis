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

    // Create fwd curve representation from zeroCurve.
    // Works for both AD and non-AD curves.
    public class FwdCurveRepresentation
    {
        public List<DateTime> Dates;
        public List<double> Values;
        private List<double> _zcbValues;
        private Curve _zcbCurve;
        public Curve FwdCurve { get; private set; }
        public int Dimension { get; private set; }
        public CurveTenor Tenor { get; private set; }
        private string _tenorStr;
        public DateTime AsOf { get; private set; }
        private DayCount _fwdDayCount;
        private DayRule _fwdDayRule;
        private InterpMethod _interpolation;

        public FwdCurveRepresentation(Curve curve, CurveTenor tenor, DateTime asOf, DayCount dayCount, DayRule dayRule, InterpMethod interpolation)
        {
            Dates = curve.Dates;
            _zcbValues = curve.Values;
            Dimension = curve.Dimension;
            Tenor = tenor;
            AsOf = asOf;

            _fwdDayCount = dayCount;
            _fwdDayRule = dayRule;
            _interpolation = interpolation;

            ConstructZcbCurveFromDatesAndValues();
            ConstructFwdRates();
        }

        public FwdCurveRepresentation(Curve_AD curve, CurveTenor tenor, DateTime asOf, DayCount dayCount, DayRule dayRule, InterpMethod interpolation)
        {
            Dates = curve.Dates;
            Dimension = curve.Dimension;
            Tenor = tenor;
            AsOf = asOf;

            _fwdDayCount = dayCount;
            _fwdDayRule = dayRule;
            _interpolation = interpolation;

            for (int i = 0; i < Dimension; i++)
                Values.Add(curve.Values[i].Value);

            ConstructZcbCurveFromDatesAndValues();
            ConstructFwdRates();
        }

        private void ConstructZcbCurveFromDatesAndValues()
        {
            _tenorStr = EnumToStr.CurveTenor(Tenor);
            _zcbCurve = new MasterThesis.Curve(Dates, _zcbValues);
        }

        private void ConstructFwdRates()
        {
            for (int i = 0; i < Dimension; i ++)
            {
                DateTime startDate = Dates[i];
                DateTime endDate = DateHandling.AddTenorAdjust(startDate, _tenorStr);
                double fwdRate = _zcbCurve.FwdRate(AsOf, startDate, endDate, _fwdDayRule, _fwdDayCount, _interpolation);
                Values.Add(fwdRate);
            }

            FwdCurve = new MasterThesis.Curve(Dates, Values);
        }
    }

    public class Curve
    {
        public List<DateTime> Dates;
        public List<double> Values;
        public int Dimension { get; private set; }
        public CurveTenor Frequency { get; private set; }

        public Curve(List<DateTime> Dates, List<double> Values)
        {
            this.Dates = Dates;
            this.Values = Values;
            this.Frequency = CurveTenor.Simple;
            this.Dimension = Values.Count;
        }

        /// <summary>
        /// Used for risk calculations.
        /// </summary>
        /// <param name="curvePoint"></param>
        /// <param name="bump"></param>
        public void BumpCurvePoint(int curvePoint, double bump)
        {
            if (curvePoint < 0)
                throw new InvalidOperationException("CurvePoint has to be non-zero."); // Redundant?

            if (curvePoint > Values.Count)
                throw new InvalidOperationException("CurvePoint is larger than length of value array.");

            Values[curvePoint] += bump;

        }

        public double Interp(DateTime date, InterpMethod interpolation)
        {
            return MyMath.InterpolateCurve(Dates, date, Values, interpolation);
        }
        public double ZeroRate(DateTime maturityDate, InterpMethod interpolation)
        {
            return this.Interp(maturityDate, interpolation);
        }
        public double DiscFactor(DateTime asOf, DateTime date, DayCount dayCount, InterpMethod interpolation)
        {
            return Math.Exp(-ZeroRate(date, interpolation) * DateHandling.Cvg(asOf, date, dayCount));
        }
        public double FwdRate(DateTime asOf, DateTime startDate, DateTime endDate, DayRule dayRule, DayCount dayCount, InterpMethod interpolation)
        {
            double ps = DiscFactor(asOf, startDate, dayCount, interpolation);
            double pe = DiscFactor(asOf, endDate, dayCount, interpolation);
            double cvg = DateHandling.Cvg(startDate, endDate, dayCount);

            return (ps / pe - 1) / cvg;
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
        public double OisAnnuity(OisSchedule schedule, InterpMethod interpolation)
        {
            double output = 0.0;
            double discFactor;
            for (int i = 0; i < schedule.AdjEndDates.Count; i++)
            {
                discFactor = DiscFactor(schedule.AsOf, schedule.AdjEndDates[i], schedule.DayCount, interpolation);
                output += schedule.Coverages[i] * discFactor;
            }
            return output;
        }


        public double OisSwapNpv(OisSwap swap, InterpMethod interpolation)
        {
            double oisAnnuity = OisAnnuity(swap.FloatSchedule, interpolation);
            double notional = swap.Notional;
            double oisRate = OisRateSimple(swap, interpolation);
            return swap.TradeSign * notional * (oisRate - swap.FixedRate) * oisAnnuity;
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
        public double OisCompoundedRate(DateTime asOf, DateTime startDate, DateTime endDate, DayRule dayRule, DayCount dayCount, InterpMethod interpolation)
        {
            double compoundedRate = 1;
            double compoundedRate2 = 1;
            DateTime rollDate = startDate;
            while (rollDate.Date < endDate.Date)
            {
                DateTime NextBusinessDay = DateHandling.AddTenorAdjust(rollDate, "1B", DayRule.F);
                //double Rate = DiscCurve.ZeroRate(asOf, startDate, RollDate, dayRule, dayCount, method);
                double rate = ZeroRate(NextBusinessDay, interpolation);
                double fwdOisRate = FwdRate(asOf, rollDate, NextBusinessDay, DayRule.F, dayCount, interpolation);

                double disc1 = DiscFactor(asOf, rollDate, dayCount, interpolation);
                double disc2 = DiscFactor(asOf, NextBusinessDay, dayCount, interpolation);

                double Days = NextBusinessDay.Subtract(rollDate).TotalDays;
                double shortCvg = DateHandling.Cvg(rollDate, NextBusinessDay, dayCount);
                rollDate = NextBusinessDay;
                compoundedRate *= (1 + fwdOisRate * shortCvg);
                compoundedRate2 *= disc1 / disc2;
            }
            double coverage = DateHandling.Cvg(startDate, endDate, dayCount);
            return (compoundedRate2 - 1) / coverage;
        }

        /// <summary>
        /// Calculate the par OIS rate by compounding. Slow.
        /// </summary>
        /// <param name="swap"></param>
        /// <param name="interpolation"></param>
        /// <returns></returns>
        public double OisRate(OisSwap swap, InterpMethod interpolation)
        {
            double floatContribution = 0.0;
            double annuity = OisAnnuity(swap.FixedSchedule, interpolation);

            DateTime asOf = swap.FloatSchedule.AsOf;

            for (int i = 0; i < swap.FloatSchedule.AdjEndDates.Count; i++)
            {
                DateTime startDate = swap.FloatSchedule.AdjStartDates[i];
                DateTime endDate = swap.FloatSchedule.AdjEndDates[i];
                double compoundedRate = OisCompoundedRate(asOf, startDate, endDate, swap.FloatSchedule.DayRule, swap.FloatSchedule.DayCount, interpolation);
                double discFactor = DiscFactor(asOf, endDate, swap.FixedSchedule.DayCount, interpolation);
                double coverage = DateHandling.Cvg(startDate, endDate, swap.FloatSchedule.DayCount);
                floatContribution += discFactor * compoundedRate * coverage;
            }
            return floatContribution / annuity;
        }

        /// <summary>
        /// Simple calculation of par OIS rate. Holds only under the assumption
        /// that FRA's can perfecetly hedge something.
        /// </summary>
        /// <param name="swap"></param>
        /// <param name="interpolation"></param>
        /// <returns></returns>
        public double OisRateSimple(OisSwap swap, InterpMethod interpolation)
        {
            double annuity = OisAnnuity(swap.FixedSchedule, interpolation);
            DateTime asOf = swap.AsOf;

            return (DiscFactor(asOf, swap.StartDate, swap.FixedSchedule.DayCount, interpolation) - DiscFactor(asOf, swap.EndDate, swap.FixedSchedule.DayCount, interpolation)) / annuity;
        }

    }

    public class FwdCurveContainer
    {
        public IDictionary<CurveTenor, Curve> Curves { get; private set; }

        public FwdCurveContainer()
        {
            Curves = new Dictionary<CurveTenor, Curve>();
        }
        public void AddCurve(Curve curve, CurveTenor curveType)
        {
            Curves[curveType] = curve;
        }
        public void AddCurve(List<DateTime> dates, List<double> values, CurveTenor tenor)
        {

            Curve newCurve = new MasterThesis.Curve(dates, values);
            Curves[tenor] = newCurve;
        }

        public FwdCurveContainer(Curve discCurve)
        {
            OneCurveToRuleThemAll(discCurve);
        }

        public bool CurveExist(CurveTenor tenor)
        {
            return Curves.ContainsKey(tenor);
        }

        public void UpdateCurveValues(List<double> values, CurveTenor tenor)
        {
            Curves[tenor].Values = values;
        }

        public Curve GetCurve(CurveTenor curveType)
        {
            return Curves[curveType];
        }

        public void OneCurveToRuleThemAll(Curve curve)
        {
            AddCurve(curve, CurveTenor.DiscLibor);
            AddCurve(curve, CurveTenor.DiscOis);
            AddCurve(curve, CurveTenor.Fwd1D);
            AddCurve(curve, CurveTenor.Fwd1M);
            AddCurve(curve, CurveTenor.Fwd3M);
            AddCurve(curve, CurveTenor.Fwd6M);
            AddCurve(curve, CurveTenor.Fwd1Y);
        }

    }
}
