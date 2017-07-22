using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace MasterThesis
{
    /* General information:
     * The RiskJacobian class constructs and contains the jacobian used
     * for the calculation of out right risk.
     */

    // Class that constructs and holds the Jacobian used to calculate outright risk (in terms of the input instruments).
    public class RiskJacobian
    {
        public List<CalibrationInstrument> Instruments { get; private set; }
        public IDictionary<CurveTenor, List<CalibrationInstrument>> InstrumentDictionary { get; private set; }
        IDictionary<string, List<double>> _fullGradients;
        public Matrix<double> Jacobian { get; private set; }
        public Matrix<double> InvertedJacobian { get; private set; }
        public LinearRateModel Model { get; private set; }
        public DateTime AsOf { get; private set; }
        public IDictionary<CurveTenor, int> CurveDimensions { get; private set; }
        private int _dimension;
        private bool _hasBeenCreated = false;
        private bool _hasBeenInitialized = false;

        public RiskJacobian(LinearRateModel model, DateTime asOf)
        {
            Model = model;
            AsOf = asOf;

            SetCurveDimensions(model);

            Instruments = new List<CalibrationInstrument>();
            _fullGradients = new Dictionary<string, List<double>>();
            InstrumentDictionary = new Dictionary<CurveTenor, List<CalibrationInstrument>>();
        }

        private void SetCurveDimensions(LinearRateModel model)
        {
            CurveDimensions = new Dictionary<CurveTenor, int>();
            foreach (CurveTenor key in model.FwdCurveCollection.Curves.Keys)
                CurveDimensions[key] = model.FwdCurveCollection.Curves[key].Dimension;

            CurveDimensions[CurveTenor.DiscOis] = model.DiscCurve.Dimension;
        }

        public void AddInstruments(List<CalibrationInstrument> instruments, CurveTenor tenor)
        {
            Instruments.AddRange(instruments);
            InstrumentDictionary[tenor] = instruments;
        }

        public void Initialize()
        {
            SetDimension();
            VerifyModelDimension();
            SetJacobian();

            _hasBeenInitialized = true;
        }

        private void SetJacobian()
        {
            // Should set the dimensions of the Jacobian based on number of instruments
            // Remember N = M. Matrix initialized with zeros.
            Jacobian = Matrix<double>.Build.Dense(_dimension, _dimension);
            InvertedJacobian = Matrix<double>.Build.Dense(_dimension, _dimension);
        }

        public void SetDimension()
        {
            _dimension = Instruments.Count;
        }

        private void VerifyModelDimension()
        {
            // Calculate number of curve points in model
            int m = 0;

            // Add disc points
            m += Model.DiscCurve.Dimension;

            foreach (CurveTenor tenor in Model.FwdCurveCollection.Curves.Keys)
                m += Model.FwdCurveCollection.Curves[tenor].Dimension;

            if (_dimension != m)
                throw new InvalidOperationException("Number of curve points and input instruments has to be equal.");
        }

        public Matrix<double> Get()
        {
            if (_hasBeenCreated)
                return Jacobian;
            else
                throw new InvalidOperationException("Jacobian has not been constructed.");
        }

        private void BumpAndRunRiskInstruments()
        {
            foreach (CalibrationInstrument product in Instruments)
            {
                product.RiskInstrumentBumpAndRun(Model, AsOf);
                _fullGradients[product.Identifier] = product.RiskOutput.FullGradient;
            }
        }

        private void ADRiskInstruments()
        {
            foreach (CalibrationInstrument product in Instruments)
                _fullGradients[product.Identifier] = Model.ZcbRiskProductAD(product.Instrument).ToList();
        }

        private void ConstructMatrixFromFullGradients()
        {
            for (int i = 0; i < _dimension; i++)
            {
                for (int j = 0; j < _dimension; j++)
                {
                    // Columns of _jacobian are the delta vector of each asset.
                    // Rows of _jacobian are delta risk to the same curve point.
                    Jacobian[j, i] = _fullGradients[Instruments[i].Identifier][j];

                    // The opposite - wrong and not working.
                    //Jacobian[i, j] = _fullGradients[_instruments[i].Identifier][j];
                }
            }
        }

        public bool AlmostEqual(double a, double b)
        {
            if (Math.Abs(a - b) < 0.000001)
                return true;
            else
                return false;
        }

        private void InvertJacobian()
        {
            InvertedJacobian = Jacobian.Inverse();
        }

        public void ConstructUsingAD()
        {
            if (!_hasBeenInitialized)
                throw new InvalidOperationException("Cannot construct Jacobian - it has not been initialized");

            ADRiskInstruments();
            ConstructMatrixFromFullGradients();
            InvertJacobian();

            _hasBeenCreated = true;
        }

        public void ConstructUsingBumpAndRun()
        {
            if (!_hasBeenInitialized)
                throw new InvalidOperationException("Cannot construct Jacobian - it has not been initialized");

            BumpAndRunRiskInstruments();
            ConstructMatrixFromFullGradients();
            InvertJacobian();

            _hasBeenCreated = true;
        }
    }
}
