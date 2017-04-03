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

    public class CalibrationSpec
    {
        public double Precision { get; }
        public double Scaling { get; }
        public double DiffStep { get; }
        public InterpMethod Interpolation { get; }
        public int MaxIterations { get; }
        public double StartingValues { get; }
        public int M { get; }

        public CalibrationSpec(double precision, double scaling, double diffStep, InterpMethod interpolation, int maxIterations, double startingValues, int m)
        {
            Precision = precision;
            Scaling = scaling;
            DiffStep = diffStep;
            Interpolation = interpolation;
            MaxIterations = maxIterations;
            StartingValues = startingValues;
            M = m;
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

        public double GoalFunction(LinearRateModel model, double scaling = 1.0)
        {
            double quadraticSum = 0.0;

            for (int i = 0; i < InputInstruments.Count; i++)
                quadraticSum += Math.Pow(InputInstruments[i].ValueInstrument(model, Factory) - InputInstruments[i].QuoteValue, 2.0)*scaling;

            return quadraticSum;
        }

    }

    public class FwdCurveConstructor
    {
        private CurveTenor[] _tenors;
        private FwdCurves _fwdCurveCollection;
        private Curve _discCurve;
        IDictionary<CurveTenor, CurveCalibrationProblem> _problemMap;
        private int[] _curvePoints;
        private int _dimension;
        private int _internalState;
        private int _internalStateMax;
        private CalibrationSpec _settings;

        /// <summary>
        /// For constructing multiple curves at a time
        /// </summary>
        /// <param name="discCurve"></param>
        /// <param name="problems"></param>
        /// <param name="tenors"></param>
        /// <param name="OrderOfCalibration"></param>
        public FwdCurveConstructor(Curve discCurve, CurveCalibrationProblem[] problems, CurveTenor[] tenors, CalibrationSpec settings, int[] OrderOfCalibration = null)
        {
            SetEverything(discCurve, problems, tenors, settings, OrderOfCalibration);
        }

        /// <summary>
        /// For constructing a single curve at a time
        /// </summary>
        /// <param name="discCurve"></param>
        /// <param name="problem"></param>
        /// <param name="tenor"></param>
        public FwdCurveConstructor(Curve discCurve, CurveCalibrationProblem problem, CurveTenor tenor, CalibrationSpec settings, InterpMethod interpolation)
        {
            SetEverything(discCurve, new CurveCalibrationProblem[] { problem }, new CurveTenor[] { tenor }, settings, new int[] { 0 });
        }

        public void SetEverything(Curve discCurve, CurveCalibrationProblem[] problems, CurveTenor[] tenors, CalibrationSpec settings, int[] OrderOfCalibration = null)
        {
            if (problems.Length != tenors.Length)
                throw new InvalidOperationException("Number of problems and number of tenors have to match. ");

            _problemMap = new Dictionary<CurveTenor, CurveCalibrationProblem>();

            _discCurve = discCurve;
            _tenors = tenors;
            _settings = settings;

            for (int i = 0; i < problems.Length; i++)
                _problemMap[tenors[i]] = problems[i];

            ConstructCurvePointsArray();
            _internalState = 0;
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
            _dimension = 0;
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = _problemMap[_tenors[i]].CurvePoints.Count;
                _dimension += output[i];
            }

            _curvePoints = output;
        }

        // For solving curves simultanously
        private void OptimizationFunction(double[] x, ref double func, object obj)
        {

            _internalState += 1;

            List<List<double>> doubles = new List<List<double>>();

            int n = 0;
            for (int i = 0; i < _curvePoints.Length; i++)
            {
                List<double> temp = new List<double>();
                for (int j = 0; j < _curvePoints[i]; j++)
                {
                    temp.Add(x[n]);
                    n += 1;
                }
                doubles.Add(temp);
            }

            _fwdCurveCollection = new FwdCurves();

            for (int i = 0; i<_curvePoints.Length; i++)
            {
                Curve tempFwdCurve = new Curve(_problemMap[_tenors[i]].CurvePoints, doubles[i]);
                _fwdCurveCollection.AddCurve(tempFwdCurve, _tenors[i]);
            }

            if (_internalState > _internalStateMax)
                return;


            LinearRateModel tempModel = new LinearRateModel(_discCurve, _fwdCurveCollection, _settings.Interpolation);
            func = GoalFunction(_tenors, tempModel);
        }

        public double[] ConstructStartingValues()
        {
            double[] output = new double[_dimension];
            int k = 0;
            for (int i = 0; i<_tenors.Length; i++)
            {
                for(int j = 0; j < _curvePoints[i]; j++)
                {
                    output[k] = _discCurve.Interp(_problemMap[_tenors[i]].CurvePoints[j], _settings.Interpolation) + (i + 1)*3 / 10000.0;
                    k += 1;
                }
            }

            return output;
        }

        public void CalibrateCurves()
        {
            double[] x = ConstructStartingValues();
            _internalState = 0;
            _internalStateMax = _settings.MaxIterations;

            double epsg = _settings.Precision;
            double epsf = 0.0;
            double epsx = 0.0;

            alglib.minlbfgsstate state;
            alglib.minlbfgsreport rep;

            alglib.minlbfgscreatef(_settings.M, x, _settings.DiffStep, out state);
            alglib.minlbfgssetcond(state, epsg, epsf, epsx, _settings.MaxIterations);
            alglib.minlbfgsoptimize(state, OptimizationFunction, null, null);
            alglib.minlbfgsresults(state, out x, out rep);
            
        }

        // This is used to solve for multiple curves simultanously
        private double GoalFunction(CurveTenor[] tenors, LinearRateModel model)
        {
            double goalSum = 0.0;

            for (int i = 0; i < tenors.Length; i++)
                goalSum += _problemMap[tenors[i]].GoalFunction(model, _settings.Scaling);

            return goalSum;
        }
    }

    public class DiscCurveConstructor
    {
        private Curve _discCurve;
        private CurveCalibrationProblem _problem;
        private CalibrationSpec _settings;

        public DiscCurveConstructor(CurveCalibrationProblem discProblem, CalibrationSpec settings)
        {
            _settings = settings;
            _problem = discProblem;
        }

        private void OptimizationFunction(double[] x, ref double func, object obj)
        {
            Curve tempDiscCurve = new Curve(_problem.CurvePoints, x.ToList());

            FwdCurves tempFwdCurves = new FwdCurves();
            LinearRateModel tempModel = new LinearRateModel(tempDiscCurve, new FwdCurves(), _settings.Interpolation);
            func = _problem.GoalFunction(tempModel, _settings.Scaling);
        }

        public void CalibrateCurve()
        {
            double[] x = new double[_problem.CurvePoints.Count];
            for (int i = 0; i < x.Length; i++)
                x[i] = _settings.StartingValues;

            double epsg = _settings.Precision;
            double epsf = 0.0;
            double epsx = 0.0;

            alglib.minlbfgsstate state;
            alglib.minlbfgsreport rep;

            alglib.minlbfgscreatef(_settings.M, x, _settings.DiffStep, out state);
            alglib.minlbfgssetcond(state, epsg, epsf, epsx, _settings.MaxIterations);
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
