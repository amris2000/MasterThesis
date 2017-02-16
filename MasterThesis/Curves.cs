using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
    public class Curve
    {
        public List<DateTime> Dates;
        public List<double> Values;
        public CurveTenor Frequency { get; set; }
        public CurveType Type { get; set; }

        public Curve(List<DateTime> dates, List<double> values, CurveType curveType, CurveTenor frequency = CurveTenor.Simple)
        {
            this.Dates = dates;
            this.Values = values;
            this.Type = curveType;
            this.Frequency = frequency;
        }

        public Curve(List<DateTime> Dates, List<double> Values)
        {
            this.Dates = Dates;
            this.Values = Values;
            this.Type = CurveType.None;
            this.Frequency = CurveTenor.None;
        }

        public double Interp(DateTime Date, InterpMethod Method)
        {
            return Maths.InterpolateCurve(Dates, Date, Values, Method);
        }
    }
    public abstract class FwdCurve : Curve
    {
        public FwdCurve(List<DateTime> Dates, List<double> Values, CurveTenor Frequency) : base(Dates, Values, CurveType.Fwd, Frequency)
        {
        }

        public double CurveRate(DateTime MaturityDate, InterpMethod Method)
        {
            return this.Interp(MaturityDate, Method);
        }

        private double DiscFactor(DateTime AsOf, DateTime MaturityDate, InterpMethod Method)
        {
            return Math.Exp(-CurveRate(MaturityDate, Method) * Calender.Cvg(AsOf, MaturityDate, DayCount.ACT365));
        }

        public double FwdRate(DateTime AsOf, DateTime StartDate, DateTime EndDate, DayRule DayRule, DayCount DayCount, InterpMethod Method)
        {
            double Ps = DiscFactor(AsOf, StartDate, Method);
            double Pe = DiscFactor(AsOf, EndDate, Method);
            double Cvg = Calender.Cvg(StartDate, EndDate, DayCount);

            return (Ps / Pe - 1) / Cvg;
        }

    }
    public class FwdCurves
    {
        private List<FwdCurve> FwdCurvesCollection = new List<FwdCurve>();
        private List<CurveTenor> TenorCollection = new List<CurveTenor>();

        public FwdCurves()
        {

        }
        
        public void AddCurve(FwdCurve MyCurve)
        {
            FwdCurvesCollection.Add(MyCurve);
            TenorCollection.Add(MyCurve.Frequency);
        }

        public FwdCurve GetCurve(CurveTenor Frequency)
        {
            return FwdCurvesCollection[TenorCollection.FindIndex(x => x == Frequency)];
        }

    }
    public class DiscCurve : Curve
    {
        public DiscCurve(List<DateTime> Dates, List<double> Values) : base(Dates, Values, CurveType.DiscOis)
        {

        }

        public double ZeroRate(DateTime MaturityDate, InterpMethod Method)
        {
            return this.Interp(MaturityDate, Method);
        }

        public double DiscFactor(DateTime AsOf, DateTime MaturityDate, InterpMethod Method)
        {
            return Math.Exp(-ZeroRate(MaturityDate, Method) * Calender.Cvg(AsOf, MaturityDate, DayCount.ACT365));
        }
    }
    public class FwdCurveSimple : Curve
    {
        public FwdCurveSimple(List<DateTime> Dates, List<double> Values) : base(Dates, Values, CurveType.Fwd)
        {
        }
    }
    public class FwdCurveAdvanced : FwdCurve
    {
        public FwdCurveAdvanced(List<DateTime> Dates, List<double> Values, CurveTenor Frequency) : base(Dates, Values, Frequency)
        {
        }
    }
}
