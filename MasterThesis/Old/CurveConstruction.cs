using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis.Extensions;

namespace MasterThesis
{
    public class CurveFactory
    {
        private List<DateTime> _curveDates = new List<DateTime>();
        private List<double> _curveValues = new List<double>();
        private List<double> _quotes = new List<double>();
        private List<MarketQuote> _marketQuotes;
        private int _curvePoints;
        private CurveTenor _tenor;

        // This allows us to define Quote calculation function as a function of the curve
        protected delegate double QuoteValue(Curve curve);

        public CurveFactory(List<MarketQuote> marketQuotes, CurveTenor curveTenor)
        {
            this._tenor = curveTenor;

            // Sort MarketQuote objects according to EndDate (does not check if they are overlapping here)
            marketQuotes.Sort(new Comparison<MarketQuote>((x, y) => DateTime.Compare(x.EndDate, y.EndDate)));
            _marketQuotes = marketQuotes;
            _curvePoints = _marketQuotes.Count;

            for (int i = 0; i < _curvePoints; i++)
            {
                _curveDates.Add(marketQuotes[i].EndDate);
                _quotes.Add(marketQuotes[i].Quote);
                _curveValues.Add(0.001);
            }
        }

        private QuoteValue DeclareDelegate(SwapSimple swap)
        {
            return x => new LinearRateModelSimple(x).SwapRate(swap);
        }

        private QuoteValue DeclareDelegate(OisSwap swap)
        {
            return x => new LinearRateModelSimple(x).OisRate(swap);
        }

        public Curve BootstrapCurve()
        {
            Curve Out = new MasterThesis.Curve(_curveDates, _curveValues);
            for (int i = 0; i < _curveDates.Count; i++)
            {
                _curveValues[i] = BootstrapCurveValue(Out, i, _quotes[i], _marketQuotes[i].InstrumentType);
                Console.WriteLine(_curveDates[i].ToString("dd/MM/yyyy") + " " + _curveValues[i]);
            }
            return Out;
        }

        public double BootstrapCurveValue(Curve curve, int Index, double quote, MarketDataInstrument type)
        {
            switch(type)
            {
                case MarketDataInstrument.IrSwapRate:
                    return BiSectionCurveValues(DeclareDelegate((SwapSimple)_marketQuotes[Index].Instrument).Invoke, curve, Index, quote, -0.03, 0.03);
                case MarketDataInstrument.OisRate:
                    return BiSectionCurveValues(DeclareDelegate((OisSwap)_marketQuotes[Index].Instrument).Invoke, curve, Index, quote, -0.03, 0.03);
                default:
                    throw new ArgumentException("MarketDataInstrument is not supported for bisection");
            }
        }

        public static double BiSectionCurveValues(Func<Curve, double> Function, Curve MyCurve, int ValueIndex, double Quote, double InitLower, double InitUpper)
        {
            double X = 0.1;
            double Tolerance = 0.0001;
            uint MaxIterations = 190;

            List<double> SolutionVector = new List<double>();
            List<double> LowerVector = new List<double>();
            List<double> UpperVector = new List<double>();

            for (int j = 0; j<MyCurve.Values.Count; j++)
            {
                SolutionVector.Add(MyCurve.Values[j]);
                LowerVector.Add(MyCurve.Values[j]);
                UpperVector.Add(MyCurve.Values[j]);
            }

            List<DateTime> Times = MyCurve.Dates;

            double Lower = InitLower;
            double Upper = InitUpper;
            double SolutionValue = 0.0;
            double LowerValue = 0.0;

            Curve LowerCurve = new Curve(Times, LowerVector);
            Curve UpperCurve = new Curve(Times, UpperVector);
            Curve SolutionCurve = new Curve(Times, SolutionVector);

            int i = 0;
            while (i < MaxIterations)
            {
                X = (Upper + Lower) * 0.5;
                SolutionVector[ValueIndex] = X;
                LowerVector[ValueIndex] = Lower;
                UpperVector[ValueIndex] = Upper;

                LowerCurve = new Curve(Times, LowerVector);
                SolutionCurve = new Curve(Times, SolutionVector);

                SolutionValue = Function(SolutionCurve);
                LowerValue = Function(LowerCurve);
                //Function(SolutionCurve) - Quote == 0.0 ||
                if ( (Upper - Lower) * 0.5*10000 < Tolerance*10000)
                {
                    Console.Write("   Iterations: " + i + "    ");
                    return SolutionVector[ValueIndex];
                }
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
