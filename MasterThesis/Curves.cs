using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis.Extensions;

namespace MasterThesis
{
    public class Curve
    {
        public List<DateTime> Dates;
        public List<double> Values;
        public int Dimension;
        public CurveTenor Frequency { get; set; }

        //public Curve(List<DateTime> dates, List<double> values)
        //{
        //    this.Dates = dates;
        //    this.values = values;
        //    this.Frequency = curveType;
        //    this.Dimension = values.Count;
        //}
        public Curve(List<DateTime> Dates, List<double> Values)
        {
            this.Dates = Dates;
            this.Values = Values;
            this.Frequency = CurveTenor.Simple;
            this.Dimension = Values.Count;
        }

        public double Interp(DateTime date, InterpMethod interpolation)
        {
            return Maths.InterpolateCurve(Dates, date, Values, interpolation);
        }
        public double ZeroRate(DateTime maturityDate, InterpMethod interpolation)
        {
            return this.Interp(maturityDate, interpolation);
        }
        public double DiscFactor(DateTime asOf, DateTime date, InterpMethod Method)
        {
            return Math.Exp(-ZeroRate(date, Method) * DateHandling.Cvg(asOf, date, DayCount.ACT360));
        }
        public double FwdRate(DateTime asOf, DateTime startDate, DateTime endDate, DayRule dayRule, DayCount dayCount, InterpMethod interpolation)
        {
            double ps = DiscFactor(asOf, startDate, interpolation);
            double pe = DiscFactor(asOf, endDate, interpolation);
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

        #region OIS SWAPS
        public double OisAnnuity(OisSchedule schedule, InterpMethod interpolation)
        {
            double output = 0.0;
            DateTime payDate;
            double discFactor;
            for (int i = 0; i < schedule.AdjEndDates.Count; i++)
            {
                payDate = schedule.AdjEndDates[i];
                discFactor = DiscFactor(schedule.AsOf, schedule.AdjEndDates[i], interpolation);
                output += schedule.Coverages[i] * discFactor;
            }
            return output;
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
            double CompoundedRate = 1;
            double CompoundedRate2 = 1;
            DateTime RollDate = startDate;
            while (RollDate.Date < endDate.Date)
            {
                DateTime NextBusinessDay = DateHandling.AddTenorAdjust(RollDate, "1B", DayRule.F);
                //double Rate = DiscCurve.ZeroRate(asOf, startDate, RollDate, dayRule, dayCount, method);
                double Rate = ZeroRate(NextBusinessDay, interpolation);
                double fwdOisRate = FwdRate(asOf, RollDate, NextBusinessDay, DayRule.F, dayCount, interpolation);

                double disc1 = DiscFactor(asOf, RollDate, interpolation);
                double disc2 = DiscFactor(asOf, NextBusinessDay, interpolation);

                double Days = NextBusinessDay.Subtract(RollDate).TotalDays;
                double shortCvg = DateHandling.Cvg(RollDate, NextBusinessDay, dayCount);
                RollDate = NextBusinessDay;
                CompoundedRate *= (1 + fwdOisRate * shortCvg);
                CompoundedRate2 *= disc1 / disc2;
            }
            double coverage = DateHandling.Cvg(startDate, endDate, dayCount);
            return (CompoundedRate2 - 1) / coverage;
        }
        public double OisRate(OisSwap swap, InterpMethod interpolation)
        {
            double FloatContribution = 0.0;
            double Annuity = OisAnnuity(swap.FixedSchedule, interpolation);

            DateTime asOf = swap.FloatSchedule.AsOf;

            for (int i = 0; i < swap.FloatSchedule.AdjEndDates.Count; i++)
            {
                DateTime Start = swap.FloatSchedule.AdjStartDates[i];
                DateTime End = swap.FloatSchedule.AdjEndDates[i];
                double CompoundedRate = OisCompoundedRate(asOf, Start, End, swap.FloatSchedule.DayRule, swap.FloatSchedule.DayCount, interpolation);
                double DiscountFactor = DiscFactor(asOf, End, interpolation);
                double coverage = DateHandling.Cvg(Start, End, swap.FloatSchedule.DayCount);
                FloatContribution += DiscountFactor * CompoundedRate * coverage;
            }
            return FloatContribution / Annuity;
        }

        public double OisRateSimple(OisSwap swap, InterpMethod interpolation)
        {
            double Annuity = OisAnnuity(swap.FixedSchedule, interpolation);
            DateTime asOf = swap.AsOf;
            //double FloatContribution = 0.0;
            //for (int i = 0; i < swap.FloatSchedule.AdjEndDates.Count; i++)
            //{
            //    DateTime Start = swap.FloatSchedule.AdjStartDates[i];
            //    DateTime End = swap.FloatSchedule.AdjEndDates[i];
            //    double cvg = Functions.Cvg(Start, End, swap.FloatSchedule.DayCount);
            //    double disc1 = DiscFactor(asOf, Start, interpolation);
            //    double disc2 = DiscFactor(asOf, End, interpolation);
            //    FloatContribution += (disc1 - disc2);
            //}

            return (DiscFactor(asOf, swap.StartDate, interpolation) - DiscFactor(asOf, swap.EndDate, interpolation)) / Annuity;

            //return FloatContribution / Annuity;
        }
        #endregion

    }

    public class FwdCurves
    {
        private IDictionary<CurveTenor, Curve> FwdCurveCollection;

        public FwdCurves()
        {
            FwdCurveCollection = new Dictionary<CurveTenor, Curve>();
        }
        public void AddCurve(Curve curve, CurveTenor curveType)
        {
            FwdCurveCollection[curveType] = curve;
        }
        public void AddCurve(List<DateTime> dates, List<double> values, CurveTenor tenor)
        {

            Curve newCurve = new MasterThesis.Curve(dates, values);
            FwdCurveCollection[tenor] = newCurve;
        }

        public FwdCurves(Curve discCurve)
        {
            OneCurveToRuleThemAll(discCurve);
        }

        public void UpdateCurveValues(List<double> values, CurveTenor tenor)
        {
            FwdCurveCollection[tenor].Values = values;
        }

        public Curve GetCurve(CurveTenor curveType)
        {
            return FwdCurveCollection[curveType];
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
