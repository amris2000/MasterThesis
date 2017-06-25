using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis;

namespace MasterThesis
{
    public class FwdCurveConstructor
    {
        private CurveTenor[] _tenors;
        private IDictionary<CurveTenor, bool> _hasBeenCalibrated;
        private FwdCurveContainer _fwdCurveCollection;
        private FwdCurveContainer _existingFwdCurveCollection;
        private Curve _discCurve;
        IDictionary<CurveTenor, CurveCalibrationProblem> _problemMap;
        private int[] _curvePoints;
        private int _dimension;
        private int _internalState;
        private int _internalStateMax;
        private CalibrationSpec _settings;
        private CurveTenor[] _currentTenors;
        private int[] _currentCurvePoints;
        private int _problemDimension;

        // For constructing multiple curves at a time
        public FwdCurveConstructor(Curve discCurve, CurveCalibrationProblem[] problems, CurveTenor[] tenors, CalibrationSpec settings, int[] OrderOfCalibration = null)
        {
            SetEverything(discCurve, problems, tenors, settings, OrderOfCalibration);
        }

        public FwdCurveContainer GetFwdCurves()
        {
            if (_fwdCurveCollection == null)
                throw new NullReferenceException("FwdCurves has not been calibrated...");
            else
                return _fwdCurveCollection;
        }

        public void SetCurrentCalibrationProblems(int order)
        {
            _problemDimension = 0;
            List<CurveTenor> tenors = new List<CurveTenor>();
            List<int> curvePoints = new List<int>();

            // need to check that calibrationOrder is not null here..
            for (int i = 0; i<_settings.CalibrationOrder.Length; i++)
            {
                if (order == _settings.CalibrationOrder[i])
                {
                    // Adding the fwd tenor to be calibrated now
                    tenors.Add(_tenors[i]);

                    // Set this tenor as "has not been calibrated". This is used for AD calibration
                    _hasBeenCalibrated[_tenors[i]] = false;

                    // Build curve points array here..
                    curvePoints.Add(_problemMap[tenors[_problemDimension]].CurvePoints.Count);

                    _problemDimension += 1;
                }
            }

            _currentCurvePoints = curvePoints.ToArray();
            _currentTenors = tenors.ToArray();

        }

        public void SetEverything(Curve discCurve, CurveCalibrationProblem[] problems, CurveTenor[] tenors, CalibrationSpec settings, int[] OrderOfCalibration = null)
        {
            if (problems.Length != tenors.Length)
                throw new InvalidOperationException("Number of problems and number of tenors have to match. ");

            _problemMap = new Dictionary<CurveTenor, CurveCalibrationProblem>();
            _hasBeenCalibrated = new Dictionary<CurveTenor, bool>();
            _discCurve = discCurve;
            _tenors = tenors;
            _settings = settings;
            _fwdCurveCollection = new FwdCurveContainer();


            List<int> curvePoints = new List<int>();

            for (int i = 0; i < problems.Length; i++)
            {
                _problemMap[tenors[i]] = problems[i];
                _fwdCurveCollection.AddCurve(problems[i].CurveToBeCalibrated, tenors[i]);
                curvePoints.Add(_fwdCurveCollection.GetCurve(tenors[i]).Dimension);
            }

            _curvePoints = curvePoints.ToArray();
            _internalState = 0;

        }

        // Construct array that holds the number of points on each of the FwdCurves (based on number of instruments)
        private void ConstructCurvePointsArray()
        {
            int[] output = new int[_currentTenors.Length];
            _dimension = 0;
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = _problemMap[_currentTenors[i]].CurvePoints.Count;
                _dimension += output[i];
            }

            _currentCurvePoints = output;
        }

        /* Sets starting values for the curve calibration problem. Here, i'm exploiting the fact
        *  that i know that curves on longer maturity fixings are above the disc curve.
        *  Taking the disc curve as a starting point (inherit the shape)*/
        public double[] ConstructStartingValues()
        {
            List<double> output = new List<double>();
            for (int i = 0; i < _currentTenors.Length; i++)
            {
                for (int j = 0; j < _currentCurvePoints[i]; j++)
                {
                    if (_settings.InheritDiscShape)
                        output.Add(_discCurve.Interp(_problemMap[_currentTenors[i]].CurvePoints[j], _settings.Interpolation) + (i + 1) * _settings.StepSizeOfInheritance / 10000.0);
                    else
                        output.Add(_settings.StartingValues);
                }
            }

            return output.ToArray();
        }

        #region CODE FOR CONSTRUCTING FWD CURVES WITH BUMP + RUN

        /* This is the main optimization function used to calibrate the curves.
        *  It is dependent on the current calibration settings
        *  defind by _currentTenor and _currentCurvePoints.*/
        private void OptimizationFunction(double[] curveValues, ref double func, object obj)
        {
            _internalState += 1;

            List<List<double>> curveValueCollection = new List<List<double>>();

            int n = 0;
            for (int i = 0; i < _currentCurvePoints.Length; i++)
            {
                List<double> temp = new List<double>();
                for (int j = 0; j < _currentCurvePoints[i]; j++)
                {
                    temp.Add(curveValues[n]);
                    n += 1;
                }
                curveValueCollection.Add(temp);
            }

            for (int i = 0; i < _currentCurvePoints.Length; i++)
            {
                _fwdCurveCollection.UpdateCurveValues(curveValueCollection[i], _currentTenors[i]);
                //Curve tempFwdCurve = new Curve(_problemMap[_tenors[i]].CurvePoints, doubles[i]);
                //_fwdCurveCollection.AddCurve(tempFwdCurve, _tenors[i]);
            }

            // If max iterations has been hit, exit. For some reason
            // AlgLibs build in maxIterations does not exit the procedure..
            if (_internalState > _internalStateMax)
                return;

            // Construct new LinearRateModel based on the next iteration.
            LinearRateModel tempModel = new LinearRateModel(_discCurve, _fwdCurveCollection, _settings.Interpolation);
            func = GoalFunction(_currentTenors, tempModel);
        }

        /// <summary>
        /// This method calibrates all the curves and should be called
        /// by the Excel layer.
        /// </summary>
        public void CalibrateAllCurves()
        {
            int maxOrder = _settings.CalibrationOrder.Max();

            for (int i = 0; i<=maxOrder; i++)
            {
                SetCurrentCalibrationProblems(i);
                CalibrateCurves();
            }
        }

        /// <summary>
        /// This method calibrates the curves given current calibration settings.
        /// This method is looped over to calibrate curves according to the
        /// calibration order.
        /// </summary>
        public void CalibrateCurves()
        {
            double[] curveValues = ConstructStartingValues();
            _internalState = 0;
            _internalStateMax = _settings.MaxIterations;

            double epsg = _settings.Precision;
            double epsf = 0.0; // Related to stopping when increment is small
            double epsx = 0.0;

            alglib.minlbfgsstate state;
            alglib.minlbfgsreport rep;

            alglib.minlbfgscreatef(_settings.M, curveValues, _settings.DiffStep, out state);
            alglib.minlbfgssetcond(state, epsg, epsf, epsx, _settings.MaxIterations);
            alglib.minlbfgsoptimize(state, OptimizationFunction, null, null);
            alglib.minlbfgsresults(state, out curveValues, out rep);
            
        }
        #endregion

        #region CODE FOR CONSTRUCTING ALL FWD CURVES AT THE SAME TIME WITH AD
        public void CalibrateCurves_AD()
        {
            double[] curveValues = ConstructStartingValues_AD();
            _internalState = 0;
            _internalStateMax = _settings.MaxIterations;

            double epsg = _settings.Precision;
            double epsf = 0.0; // Related to stopping when increment is small
            double epsx = 0.0;

            alglib.minlbfgsstate state;
            alglib.minlbfgsreport rep;

            alglib.minlbfgscreate(_settings.M, curveValues, out state);
            alglib.minlbfgssetcond(state, epsg, epsf, epsx, _settings.MaxIterations);
            alglib.minlbfgsoptimize(state, OptimizationFunction_AD, null, null);
            alglib.minlbfgsresults(state, out curveValues, out rep);
        }

        private void OptimizationFunction_AD(double[] curveValues, ref double func, double[] grad, object obj)
        {
            _internalState += 1;

            if (_internalState > _internalStateMax)
                return;

            List<List<double>> curveValueCollection = new List<List<double>>();
            List<ADouble> curveValuesAd = new List<ADouble>();

            int n = 0;
            for (int i = 0; i < _curvePoints.Length; i++)
            {
                List<double> temp = new List<double>();
                for (int j = 0; j < _curvePoints[i]; j++)
                {
                    temp.Add(curveValues[n]);
                    curveValuesAd.Add(new ADouble(curveValues[n]));
                    n += 1;
                }
                curveValueCollection.Add(temp);
            }

            for (int i = 0; i < _curvePoints.Length; i++)
                _fwdCurveCollection.UpdateCurveValues(curveValueCollection[i], _tenors[i]);

            AADTape.ResetTape();

            LinearRateModel tempModel = new LinearRateModel(_discCurve, _fwdCurveCollection, _settings.Interpolation);
            tempModel.ADFwdCurveCollection = new ADFwdCurveContainer();
            AADTape.Initialize(curveValuesAd.ToArray());
            tempModel.SetAdDiscCurveFromOrdinaryCurve();


            n = 0;
            for (int i = 0; i < _curvePoints.Length; i++)
            {
                List<ADouble> tempADouble = new List<ADouble>();
                for (int j = 0; j < _curvePoints[i]; j++)
                {
                    tempADouble.Add(curveValuesAd[n]);
                    n += 1;
                }

                tempModel.ADFwdCurveCollection.AddCurve(new Curve_AD(_fwdCurveCollection.GetCurve(_tenors[i]).Dates, tempADouble), _tenors[i]);
            }

            func = GoalFunction_AD(_tenors, tempModel);
            AADTape.InterpretTape();
            double[] gradient = AADTape.GetGradient();
            for (int i = 0; i < gradient.Length; i++)
                grad[i] = gradient[i];

            AADTape.ResetTape();

        }

        public double[] ConstructStartingValues_AD()
        {
            List<double> output = new List<double>();
            for (int i = 0; i < _tenors.Length; i++)
            {
                for (int j = 0; j < _curvePoints[i]; j++)
                    //output.Add(-0.0035);
                    output.Add(_discCurve.Interp(_problemMap[_tenors[i]].CurvePoints[j], _settings.Interpolation) + (i + 1) * 3 / 10000.0);
            }

            return output.ToArray();
        }
        #endregion

        #region CODE FOR CONSTRUCTING CURVES BASED ON INPUT ORDER WITH AD

        public double[] ConstructStartingValues_AD_Current()
        {
            List<double> output = new List<double>();
            for (int i = 0; i < _currentTenors.Length; i++)
            {
                for (int j = 0; j < _currentCurvePoints[i]; j++)
                {
                    if (_settings.InheritDiscShape)
                        output.Add(_discCurve.Interp(_problemMap[_tenors[i]].CurvePoints[j], _settings.Interpolation) + (i + 1) * _settings.StepSizeOfInheritance / 10000.0);
                    else
                        output.Add(_settings.StartingValues);
                }
            }

            return output.ToArray();
        }

        public double[] ConstructStartingValuesFromCurves_AD_Current(FwdCurveContainer fwdCurves)
        {
            List<double> output = new List<double>();
            for (int i = 0; i < _currentTenors.Length; i++)
            {
                for (int j = 0; j < _currentCurvePoints[i]; j++)
                    output.Add(fwdCurves.GetCurve(_currentTenors[i]).Values[j]);
            }

            return output.ToArray();
        }

        // The old version
        private void OptimizationFunction_AD_Current2(double[] curveValues, ref double func, double[] grad, object obj)
        {
            _internalState += 1;

            if (_internalState > _internalStateMax)
                return;

            List<List<double>> curveValueCollection = new List<List<double>>();
            List<ADouble> curveValuesAd = new List<ADouble>();

            int n = 0;
            for (int i = 0; i < _currentCurvePoints.Length; i++)
            {
                List<double> temp = new List<double>();
                for (int j = 0; j < _currentCurvePoints[i]; j++)
                {
                    temp.Add(curveValues[n]);
                    curveValuesAd.Add(new ADouble(curveValues[n]));
                    n += 1;
                }
                curveValueCollection.Add(temp);
            }

            for (int i = 0; i < _currentCurvePoints.Length; i++)
                _fwdCurveCollection.UpdateCurveValues(curveValueCollection[i], _currentTenors[i]);

            AADTape.ResetTape();

            LinearRateModel tempModel = new LinearRateModel(_discCurve, _fwdCurveCollection, _settings.Interpolation);
            tempModel.ADFwdCurveCollection = new ADFwdCurveContainer();
            AADTape.Initialize(curveValuesAd.ToArray());
            tempModel.SetAdDiscCurveFromOrdinaryCurve();


            n = 0;

            // ERROR HERE. NEED TO SET THE CURVES FROM THE CALIBATION BEFORE
            // Use _hasCurvesBeenCalibrated.
            for (int i = 0; i < _currentCurvePoints.Length; i++)
            {
                List<ADouble> tempADouble = new List<ADouble>();
                for (int j = 0; j < _currentCurvePoints[i]; j++)
                {
                    tempADouble.Add(curveValuesAd[n]);
                    n += 1;
                }

                tempModel.ADFwdCurveCollection.AddCurve(new Curve_AD(_fwdCurveCollection.GetCurve(_currentTenors[i]).Dates, tempADouble), _currentTenors[i]);
            }

            func = GoalFunction_AD(_currentTenors, tempModel);
            AADTape.InterpretTape();
            double[] gradient = AADTape.GetGradient();
            for (int i = 0; i < gradient.Length; i++)
                grad[i] = gradient[i];

            AADTape.ResetTape();

        }

        // The new version
        private void OptimizationFunction_AD_Current(double[] curveValues, ref double func, double[] grad, object obj)
        {
            _internalState += 1;

            if (_internalState > _internalStateMax)
                return;

            List<List<double>> curveValueCollection = new List<List<double>>();

            int n = 0;
            for (int i = 0; i < _currentCurvePoints.Length; i++)
            {
                List<double> temp = new List<double>();
                for (int j = 0; j < _currentCurvePoints[i]; j++)
                {
                    temp.Add(curveValues[n]);
                    n += 1;
                }
                curveValueCollection.Add(temp);
            }

            for (int i = 0; i < _currentCurvePoints.Length; i++)
                _fwdCurveCollection.UpdateCurveValues(curveValueCollection[i], _currentTenors[i]);

            LinearRateModel tempModel = new LinearRateModel(_discCurve, _fwdCurveCollection, _settings.Interpolation);
            tempModel.SetAdCurvesFromOrdinaryCurve();
            //tempModel.ADFwdCurveCollection = new ADFwdCurveContainer();
            //tempModel.SetAdDiscCurveFromOrdinaryCurve();
            n = 0;


            AADTape.ResetTape();

            List<ADouble> curveValuesAd = new List<ADouble>();
            for (int i = 0; i < _currentCurvePoints.Length; i++)
            {
                for (int j = 0; j < _currentCurvePoints[i]; j++)
                {
                    curveValuesAd.Add(new ADouble(curveValues[n]));
                    n += 1;
                }
                //curveValueCollection.Add(temp);
            }



            // ERROR HERE. NEED TO SET THE CURVES FROM THE CALIBATION BEFORE

            //foreach (CurveTenor tempTenor in _hasBeenCalibrated.Keys)
            //{
            //    if (_hasBeenCalibrated[tempTenor])
            //    {
            //        Curve tempCurve = _fwdCurveCollection.GetCurve(tempTenor).Copy();
            //        List<ADouble> tempValues = new List<ADouble>();


            //        for (int i = 0; i < _fwdCurveCollection.GetCurve(tempTenor).Values.Count; i++)
            //            tempValues.Add(new ADouble(_fwdCurveCollection.GetCurve(tempTenor).Values[i]));

            //        Curve_AD tempADCurve = new Curve_AD(_fwdCurveCollection.GetCurve(tempTenor).Dates, tempValues);
            //        tempModel.ADFwdCurveCollection.AddCurve(tempADCurve, tempTenor);
            //    }
            //}

            //foreach (CurveTenor tempTenor in _hasBeenCalibrated.Keys)
            //{
            //    if (_hasBeenCalibrated[tempTenor])
            //        tempModel.ADFwdCurveCollection.AddCurve(tempADCurve, tempTenor);
            //}


            AADTape.Initialize(curveValuesAd.ToArray());

            n = 0;
            // Use _hasCurvesBeenCalibrated.
            for (int i = 0; i < _currentCurvePoints.Length; i++)
            {
                List<ADouble> tempADouble = new List<ADouble>();
                for (int j = 0; j < _currentCurvePoints[i]; j++)
                {
                    tempADouble.Add(curveValuesAd[n]);
                    n += 1;
                }
                tempModel.ADFwdCurveCollection.AddCurve(new Curve_AD(_fwdCurveCollection.GetCurve(_currentTenors[i]).Dates, tempADouble), _currentTenors[i]);
            }

            func = GoalFunction_AD(_currentTenors, tempModel);
            AADTape.InterpretTape();
            double[] gradient = AADTape.GetGradient();
            for (int i = 0; i < gradient.Length; i++)
                grad[i] = gradient[i];

            AADTape.ResetTape();

        }

        public void CalibrateAllCurves_AD(bool useExistingCurves = false)
        {
            int maxOrder = _settings.CalibrationOrder.Max();

            for (int i = 0; i <= maxOrder; i++)
            {
                SetCurrentCalibrationProblems(i);
                CalibrateCurves_AD_Current(useExistingCurves);
            }
        }

        private void SetCurrentTenorsAsCalibrated()
        {
            for (int i = 0; i < _currentTenors.Length; i++)
                _hasBeenCalibrated[_currentTenors[i]] = true;
        }

        public void SetExistingFwdCurves(FwdCurveContainer fwdCurves)
        {
            _existingFwdCurveCollection = fwdCurves;
        }

        public void CalibrateCurves_AD_Current(bool useExistingCurves = false)
        {
            double[] curveValues;
            if (useExistingCurves)
                curveValues = ConstructStartingValuesFromCurves_AD_Current(_existingFwdCurveCollection);
            else
                curveValues = ConstructStartingValues_AD_Current();

            _internalState = 0;
            _internalStateMax = _settings.MaxIterations;

            double epsg = _settings.Precision;
            double epsf = 0.0; // Related to stopping when increment is small
            double epsx = 0.0;

            alglib.minlbfgsstate state;
            alglib.minlbfgsreport rep;

            alglib.minlbfgscreate(_settings.M, curveValues, out state);
            alglib.minlbfgssetcond(state, epsg, epsf, epsx, _settings.MaxIterations);
            alglib.minlbfgsoptimize(state, OptimizationFunction_AD_Current, null, null);
            alglib.minlbfgsresults(state, out curveValues, out rep);

            SetCurrentTenorsAsCalibrated();
        }
        #endregion

        // This is used to solve for multiple curves simultanously
        private double GoalFunction(CurveTenor[] tenors, LinearRateModel model)
        {
            double goalSum = 0.0;

            for (int i = 0; i < tenors.Length; i++)
                goalSum += _problemMap[tenors[i]].GoalFunction(model, _settings.Scaling);

            return goalSum;
        }

        private double GoalFunction_AD(CurveTenor[] tenors, LinearRateModel model)
        {
            ADouble goalSum = 0.0;

            for (int i = 0; i < tenors.Length; i++)
                goalSum = goalSum + _problemMap[tenors[i]].GoalFunction_AD(model, _settings.Scaling);

            return goalSum.Value;
        }
    }

    public class DiscCurveConstructor
    {
        private Curve _discCurve;
        private CurveCalibrationProblem _problem;
        private CalibrationSpec _settings;
        private int _internalState;
        private int _internalStateMax;

        public DiscCurveConstructor(CurveCalibrationProblem discProblem, CalibrationSpec settings)
        {
            _settings = settings;
            _problem = discProblem;
            _internalStateMax = settings.MaxIterations;
        }

        private void OptimizationFunction(double[] x, ref double func, object obj)
        {
            Curve tempDiscCurve = new Curve(_problem.CurvePoints, x.ToList());

            FwdCurveContainer tempFwdCurves = new FwdCurveContainer();
            LinearRateModel tempModel = new LinearRateModel(tempDiscCurve, new FwdCurveContainer(), _settings.Interpolation);

            _internalState += 1;

            if (_internalState > _internalStateMax)
                return;

            func = _problem.GoalFunction(tempModel, _settings.Scaling);
        }

        public void OptimizationFunction_AD_grad(double[] x, ref double func, double[] grad, object obj)
        {
            Curve tempDiscCurve = new Curve(_problem.CurvePoints, x.ToList());

            FwdCurveContainer tempFwdCurves = new FwdCurveContainer();
            LinearRateModel tempModel = new LinearRateModel(tempDiscCurve, new FwdCurveContainer(), _settings.Interpolation);

            _internalState += 1;
             
            if (_internalState > _internalStateMax)
                return;


            // Initialize AADTape with curve values
            AADTape.ResetTape();
            List<ADouble> adCurveValues = new List<ADouble>();

            for (int i = 0; i < tempDiscCurve.Dimension; i++)
                adCurveValues.Add(new ADouble(x[i]));

            // Initialize the tape with curve values defined above
            AADTape.Initialize(adCurveValues.ToArray());
            Curve_AD adCurve = new Curve_AD(tempDiscCurve.Dates, adCurveValues);
            tempModel.ADDiscCurve = adCurve;
            
            func = _problem.GoalFunction_AD(tempModel, _settings.Scaling);
            AADTape.InterpretTape();
            double[] gradient = AADTape.GetGradient();
            for (int i = 0; i < gradient.Length; i++)
                grad[i] = gradient[i];

            AADTape.ResetTape();
        }

        // Not used
        private double[] CalculateInitialGradient(double[] x)
        {
            Curve tempDiscCurve = new Curve(_problem.CurvePoints, x.ToList());

            FwdCurveContainer tempFwdCurves = new FwdCurveContainer();
            LinearRateModel tempModel = new LinearRateModel(tempDiscCurve, new FwdCurveContainer(), _settings.Interpolation);

            // Initialize AADTape with curve values
            AADTape.ResetTape();
            List<ADouble> aDoubles = new List<ADouble>();

            for (int i = 0; i < x.Length; i++)
                aDoubles.Add(new ADouble(x[i]));

            AADTape.Initialize(aDoubles.ToArray());

            Curve_AD curve = new Curve_AD(tempDiscCurve.Dates, aDoubles);
            tempModel.ADDiscCurve = curve;

            _problem.GoalFunction_AD(tempModel, _settings.Scaling);
            AADTape.InterpretTape();
            double[] output = AADTape.GetGradient();

            AADTape.ResetTape();

            return output;
        }

        public void CalibrateCurveAd()
        {
            double[] x = new double[_problem.CurvePoints.Count];
            for (int i = 0; i < x.Length; i++)
                x[i] = _settings.StartingValues;

            _internalState = 0;

            double epsg = _settings.Precision;
            double epsf = 0;
            double epsx = 0;

            alglib.minlbfgsstate state;
            alglib.minlbfgsreport rep;

            // Calculate initial gradient.
            //double[] _initialGrad = CalculateInitialGradient(x);

            alglib.minlbfgscreate(_settings.M, x, out state);
            alglib.minlbfgssetcond(state, epsg, epsf, epsx, 0);
            alglib.minlbfgsoptimize(state, OptimizationFunction_AD_grad, null, null);
            alglib.minlbfgsresults(state, out x, out rep);

            _discCurve = new Curve(_problem.CurvePoints, x.ToList());
        }

        public void CalibrateCurve()
        {
            double[] x = new double[_problem.CurvePoints.Count];
            for (int i = 0; i < x.Length; i++)
                x[i] = _settings.StartingValues;

            _internalState = 0;

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
