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

        public Curve(List<DateTime> dates, List<double> values, CurveTenor curveType = CurveTenor.Simple)
        {
            this.Dates = dates;
            this.Values = values;
            this.Frequency = curveType;
            this.Dimension = values.Count;
        }
        public Curve(List<DateTime> Dates, List<double> Values)
        {
            this.Dates = Dates;
            this.Values = Values;
            this.Frequency = CurveTenor.Simple;
            this.Dimension = Values.Count;
        }

        public double Interp(DateTime Date, InterpMethod Method)
        {
            return Maths.InterpolateCurve(Dates, Date, Values, Method);
        }
        public double ZeroRate(DateTime MaturityDate, InterpMethod Method)
        {
            return this.Interp(MaturityDate, Method);
        }
        public double DiscFactor(DateTime AsOf, DateTime MaturityDate, InterpMethod Method)
        {
            return Math.Exp(-ZeroRate(MaturityDate, Method) * Calender.Cvg(AsOf, MaturityDate, DayCount.ACT365));
        }
        public double FwdRate(DateTime AsOf, DateTime StartDate, DateTime EndDate, DayRule DayRule, DayCount DayCount, InterpMethod Method)
        {
            double Ps = DiscFactor(AsOf, StartDate, Method);
            double Pe = DiscFactor(AsOf, EndDate, Method);
            double Cvg = Calender.Cvg(StartDate, EndDate, DayCount);

            return (Ps / Pe - 1) / Cvg;
        }
        public void Print()
        {
            for (int i = 0; i < Dates.Count; i++)
            {
                Console.WriteLine(Dates[i].Date + " " + Math.Round(Values[i], 5));
            }
        }

    }

    // Not used at the moment
    public abstract class FwdCurve : Curve
    {
        public FwdCurve(List<DateTime> Dates, List<double> Values, CurveTenor Frequency) : base(Dates, Values, Frequency)
        {
        }
    }
    public class FwdCurves
    {
        private List<Curve> FwdCurvesCollection = new List<Curve>();
        private List<CurveTenor> TenorCollection = new List<CurveTenor>();
        public FwdCurves()
        {

        }
        public void AddCurve(Curve MyCurve, CurveTenor curveType)
        {
            FwdCurvesCollection.Add(MyCurve);
            TenorCollection.Add(curveType);
        }
        public void AddCurve(List<DateTime> dates, List<double> values, CurveTenor tenor)
        {
            Curve NewCurve = new MasterThesis.Curve(dates, values, tenor);
            FwdCurvesCollection.Add(NewCurve);
        }

        public FwdCurves(Curve discCurve)
        {
            OneCurveToRuleThemAll(discCurve);
        }

        public Curve GetCurve(CurveTenor curveType)
        {
            return FwdCurvesCollection[TenorCollection.FindIndex(x => x == curveType)];
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
