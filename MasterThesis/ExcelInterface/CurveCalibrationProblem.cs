using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis.ExcelInterface
{

    public class InstrumentQuote
    {
        private string _identifier;
        private QuoteType _type;
        public double QuoteValue { get; }
        public DateTime CurvePoint { get; }

        public InstrumentQuote(string identifier, QuoteType type , DateTime curvePoint, double quoteValue)
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
    }

    // Class should hold the instruments, their quotes and preferably set
    // up the order of the instruments in the calibration problem.
    // Could create list based on InstrumentQuotes curvePoint (as in old method)
    public class CurveCalibrationProblem
    {
        public List<InstrumentQuote> InputInstruments { get; private set; }
        public InstrumentFactory Factory { get; set; }
        public List<DateTime> CurvePoints = new List<DateTime>();

        public CurveCalibrationProblem(InstrumentFactory instrumentFactory, List<InstrumentQuote> instruments)
        {
            this.Factory = instrumentFactory;
            instruments.Sort(new Comparison<InstrumentQuote>((x, y) => DateTime.Compare(x.CurvePoint, y.CurvePoint)));
            InputInstruments = instruments;

            for (int i = 0; i < InputInstruments.Count; i++)
                CurvePoints.Add(InputInstruments[i].CurvePoint);
        }

        public double GoalFunction(LinearRateModel model)
        {
            double quadraticSum = 0.0;

            for (int i = 0; i < InputInstruments.Count; i++)
                quadraticSum += Math.Pow(InputInstruments[i].ValueInstrument(model, Factory) - InputInstruments[i].QuoteValue, 2.0);

            return quadraticSum;
        }

    }

    public class FwdCurveConstructor
    {
        private CurveTenor[] _tenors;
        private FwdCurves _fwdCurveCollection;
        private Curve _discCurve;
        IDictionary<CurveTenor, CurveCalibrationProblem> _problemMap;
        private int[] CurvePoints;

        public FwdCurveConstructor(Curve discCurve, CurveCalibrationProblem[] problems, CurveTenor[] tenors, int[] OrderOfCalibration)
        {
            SetEverything(discCurve, problems, tenors, OrderOfCalibration);

        }

        public FwdCurveConstructor(Curve discCurve, CurveCalibrationProblem problem, CurveTenor tenor)
        {
            _problemMap = new Dictionary<CurveTenor, CurveCalibrationProblem>();
            SetEverything(discCurve, new CurveCalibrationProblem[] { problem }, new CurveTenor[] { tenor }, new int[] { 0 });
        }

        public void SetEverything(Curve discCurve, CurveCalibrationProblem[] problems, CurveTenor[] tenors, int[] OrderOfCalibration)
        {
            if (problems.Length != tenors.Length)
                throw new InvalidOperationException("Number of problems and number of tenors have to match. ");

            _discCurve = discCurve;
            _tenors = tenors;

            for (int i = 0; i < problems.Length; i++)
                _problemMap[tenors[i]] = problems[i];
        }

        public bool InstrumentTenorCombinationsIsValid() { return true; }

        public FwdCurves GetFwdCurves()
        {
            if (_fwdCurveCollection == null)
                throw new NullReferenceException("FwdCurves has not been calibrated...");
            else
                return _fwdCurveCollection;
        }

        // Construct array that holds the number of points on each of the FwdCurves (based on number of instruments)
        private void ConstructCurvePointsArray()
        {
            int[] output = new int[_problemMap.Count];
            for (int i = 0; i < output.Length; i++)
                output[i] = _problemMap[_tenors[i]].CurvePoints.Count;

            CurvePoints = output;
        }

        // For solving curves simultanously
        private void OptimizationFunction(double[] x, ref double func, object obj, int[] CurvePoints, CurveTenor[] tenors)
        {
            List<List<double>> doubles = new List<List<double>>();

            int n = 0;
            for (int i = 0; i < CurvePoints.Length; i++)
            {
                List<double> temp = new List<double>();
                for (int j = 0; j < CurvePoints[i]; j++)
                {
                    temp.Add(x[n]);
                    n++;
                }
                doubles.Add(temp);
            }

            FwdCurves tempFwdCurves = new FwdCurves();

            for (int i = 0; i<CurvePoints.Length; i++)
            {
                Curve tempFwdCurve = new Curve(_problemMap[tenors[i]].CurvePoints, doubles[i]);
                tempFwdCurves.AddCurve(tempFwdCurve, tenors[i]);
            }

            LinearRateModel tempModel = new LinearRateModel(_discCurve, tempFwdCurves);
            func = GoalFunction(tenors, tempModel);
        }

        // This is used to solve for multiple curves simultanously
        private double GoalFunction(CurveTenor[] tenors, LinearRateModel model)
        {
            double goalSum = 0.0;

            for (int i = 0; i < tenors.Length; i++)
                goalSum += _problemMap[tenors[i]].GoalFunction(model);

            return goalSum;
        }
    }

    public class DiscCurveConstructor
    {
        private Curve _discCurve;
        private CurveCalibrationProblem _problem;
        private InterpMethod _interpolation;

        public DiscCurveConstructor(CurveCalibrationProblem discProblem, InterpMethod interpolation)
        {
            _interpolation = interpolation;
            _problem = discProblem;
        }

        private void OptimizationFunction(double[] x, ref double func, object obj)
        {
            Curve tempDiscCurve = new Curve(_problem.CurvePoints, x.ToList());

            FwdCurves tempFwdCurves = new FwdCurves();
            LinearRateModel tempModel = new LinearRateModel(tempDiscCurve, new FwdCurves(), _interpolation);
            func = _problem.GoalFunction(tempModel);
        }

        public void SetCurve(double precision, double startingValue, int maxIterations, double diffStep)
        {
            double[] x = new double[_problem.CurvePoints.Count];
            for (int i = 0; i < x.Length; i++)
                x[i] = startingValue;

            double epsg = precision;
            double epsf = 0.0;
            double epsx = 0.0;

            alglib.minlbfgsstate state;
            alglib.minlbfgsreport rep;

            alglib.minlbfgscreatef(1, x, diffStep, out state);
            alglib.minlbfgssetcond(state, epsg, epsf, epsx, maxIterations);
            alglib.minlbfgsoptimize(state, OptimizationFunction, null, null);
            alglib.minlbfgsresults(state, out x, out rep);

            _discCurve = new Curve(_problem.CurvePoints, x.ToList());
        }


        public Curve GetCurve()
        {
            if (_discCurve == null)
                throw new NullReferenceException("DiscCurve has not been calibrated...");
            else
                return _discCurve;
        }

    }
}
