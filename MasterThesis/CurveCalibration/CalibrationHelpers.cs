using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
    public class CalibrationInstrument
    {
        public CurveTenor Tenor { get; private set; }
        public LinearRateInstrument Instrument { get; private set; }
        public DateTime CurvePoint { get; private set; }
        public ZcbRiskOutputContainer RiskOutput { get; private set; }
        public string Identifier { get; private set; }

        public CalibrationInstrument(string identifier, LinearRateInstrument instrument, CurveTenor tenor)
        {
            Tenor = tenor;
            Instrument = instrument;
            CurvePoint = instrument.GetCurvePoint();
            Identifier = identifier;
        }

        public void RiskInstrumentBumpAndRun(LinearRateModel model, DateTime asOf)
        {
            RiskOutput = model.RiskAgainstAllCurvesBumpAndRun(Instrument, asOf);
            RiskOutput.ConstructFullGradient();
        }
    }

    public class InstrumentQuote
    {
        private string _identifier;
        private QuoteType _type;
        public double QuoteValue { get; }
        public DateTime CurvePoint { get; }

        public InstrumentQuote(string identifier, QuoteType type, DateTime curvePoint, double quoteValue)
        {
            this._identifier = identifier;
            this._type = type;
            this.QuoteValue = quoteValue;
            this.CurvePoint = curvePoint;
        }

        public double ValueInstrument(LinearRateModel model, InstrumentFactory factory)
        {
            return factory.ValueInstrumentFromFactory(model, _identifier);
        }

        public ADouble ValueInstrumentAD(LinearRateModel model, InstrumentFactory factory)
        {
            return factory.ValueInstrumentFromFactoryAD(model, _identifier);
        }
    }

    public class CalibrationSpec
    {
        public double Precision { get; }
        public double Scaling { get; }
        public double DiffStep { get; }
        public InterpMethod Interpolation { get; }
        public int MaxIterations { get; }
        public double StartingValues { get; }
        public int M { get; }
        public int[] CalibrationOrder { get; }
        public bool UseAd { get; }
        public bool InheritDiscShape;
        public double StepSizeOfInheritance;

        public CalibrationSpec(double precision, double scaling, double diffStep, InterpMethod interpolation, int maxIterations, double startingValues, int m, bool useAd, bool inheritDiscSize, double stepSizeOfInheritance, int[] calibrationOrder = null)
        {
            Precision = precision;
            Scaling = scaling;
            DiffStep = diffStep;
            Interpolation = interpolation;
            MaxIterations = maxIterations;
            StartingValues = startingValues;
            M = m;
            UseAd = useAd;
            CalibrationOrder = calibrationOrder;
            InheritDiscShape = inheritDiscSize;
            StepSizeOfInheritance = stepSizeOfInheritance;
        }
    }

    public class CurveCalibrationProblem
    {
        public List<InstrumentQuote> InputInstruments { get; private set; }
        public InstrumentFactory Factory { get; set; }
        public List<DateTime> CurvePoints = new List<DateTime>();
        public Curve CurveToBeCalibrated;

        public CurveCalibrationProblem(InstrumentFactory instrumentFactory, List<InstrumentQuote> instruments)
        {
            this.Factory = instrumentFactory;
            instruments.Sort(new Comparison<InstrumentQuote>((x, y) => DateTime.Compare(x.CurvePoint, y.CurvePoint)));
            InputInstruments = instruments;
            List<double> tempValues = new List<double>();

            for (int i = 0; i < InputInstruments.Count; i++)
            {
                CurvePoints.Add(InputInstruments[i].CurvePoint);
                tempValues.Add(1.0);
            }

            CurveToBeCalibrated = new MasterThesis.Curve(CurvePoints, tempValues);

        }

        public double GoalFunction(LinearRateModel model, double scaling = 1.0)
        {
            double quadraticSum = 0.0;

            for (int i = 0; i < InputInstruments.Count; i++)
                quadraticSum += Math.Pow(InputInstruments[i].ValueInstrument(model, Factory) - InputInstruments[i].QuoteValue, 2.0) * scaling;

            return quadraticSum;
        }

        public ADouble GoalFunction_AD(LinearRateModel model, double scaling = 1.0)
        {
            ADouble quadraticSum = 0.0;

            for (int i = 0; i < InputInstruments.Count; i++)
            {
                ADouble tempDifference = InputInstruments[i].ValueInstrumentAD(model, Factory) - InputInstruments[i].QuoteValue;
                quadraticSum = quadraticSum + ADouble.Pow(tempDifference, 2.0) * scaling;
            }

            return quadraticSum;
        }
    }
}
