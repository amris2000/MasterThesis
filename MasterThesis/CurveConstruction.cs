using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
    public class CurveFactory
    {
        List<DateTime> CurveDates;
        List<double> CurveValues;
        List<double> Quotes;
        FwdCurves Fwd;

        List<MarketQuote> MarketQuotes;
        int CurvePoints;
        CurveType Type;
        CurveTenor Tenor;

        // This allows us to define Quote calculation function as a function of the curve
        protected delegate double QuoteValue(Curve curve);

        public CurveFactory(List<MarketQuote> marketQuotes, CurveType curveType, CurveTenor curveTenor)
        {
            this.Type = curveType;
            this.Tenor = curveTenor;

            // Sort MarketQuote objects according to EndDate (does not check if they are overlapping here)
            marketQuotes.Sort(new Comparison<MarketQuote>((x, y) => DateTime.Compare(x.EndDate, y.EndDate)));
            MarketQuotes = marketQuotes;
            CurvePoints = MarketQuotes.Count;

            for (int i = 0; i < CurvePoints; i++)
                CurveDates.Add(marketQuotes[i].EndDate);
            for (int i = 0; i < CurvePoints; i++)
                Quotes.Add(marketQuotes[i].Quote);

        }

        private QuoteValue DeclareDelegate(IrSwap swap)
        {
            return x => new LinearRateModelAdvanced(Fwd, (DiscCurve)x).IrSwapPv(swap);
        }

        public double BootstrapCurveValue(Curve curve, int Index, double quote)
        {
            return BiSectionCurveValues(DeclareDelegate((IrSwap)MarketQuotes[Index].Instrument).Invoke, curve, Index, quote, 0.001, 0.2);
        }

        public static double BiSectionCurveValues(Func<Curve, double> Function, Curve MyCurve, int ValueIndex, double Quote, double InitLower, double InitUpper)
        {
            double X = 0.1;
            double Tolerance = 0.00001;
            uint MaxIterations = 50;

            List<double> SolutionVector = MyCurve.Values;
            List<double> LowerVector = MyCurve.Values;
            List<double> UpperVector = MyCurve.Values;
            List<DateTime> Times = MyCurve.Dates;

            double Lower = InitLower;
            double Upper = InitUpper;

            Curve LowerCurve = new Curve(Times, LowerVector);
            Curve UpperCurve = new Curve(Times, UpperVector);
            Curve SolutionCurve = new Curve(Times, SolutionVector);

            int i = 0;
            while (i < MaxIterations)
            {
                SolutionVector[ValueIndex] = X;
                LowerVector[ValueIndex] = Lower;
                UpperVector[ValueIndex] = Upper;

                if (Function(SolutionCurve) - Quote == 0.0 || (Function(UpperCurve) - Function(LowerCurve)) * 0.5 < Tolerance)
                    return SolutionVector[ValueIndex];

                i = i + 1;
                if (Math.Sign(Function(SolutionCurve) - Quote) == Math.Sign(Function(LowerCurve) - Quote))
                    Lower = X;
                else
                    Upper = X;
            }
            throw new ArgumentException("Algorithm did not converge. Values: lower = " + Lower + ", upper = " + Upper + ", X = " + X);

        }

    }
}
