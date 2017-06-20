using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis;
using MathNet.Numerics.LinearAlgebra;

namespace MasterThesis
{
    /// <summary>
    /// Simple container for LinearRateProducts.
    /// Used to calculate risk on multiple assets at a time.
    /// </summary>
    public class Portfolio
    {
        public IDictionary<int, LinearRateInstrument> Products { get; private set; }
        public IDictionary<int, ZcbRiskOutputContainer> RiskOutputs { get; private set; }
        int _productCounter;

        public Portfolio()
        {
            Products = new Dictionary<int, LinearRateInstrument>();
            RiskOutputs = new Dictionary<int, ZcbRiskOutputContainer>();
            _productCounter = 0;
        }

        public void AddProducts(params LinearRateInstrument[] products)
        {
            for (int i = 0; i < products.Length; i++)
            {
                Products[_productCounter] = products[i];
                _productCounter += 1;
            }
        }

        public void RiskInstrument(int instrumentNumber, LinearRateModel model, DateTime asOf, bool useAd = false)
        {
            if (Products.ContainsKey(instrumentNumber) == false)
                throw new InvalidOperationException("Portfolio does not contain " + instrumentNumber + " as an identififer for a linearRateProduct.");

            if (useAd)
                RiskOutputs[instrumentNumber] = model.ZcbRiskProductOutputContainer(Products[instrumentNumber], asOf);
            else
                RiskOutputs[instrumentNumber] = model.RiskAgainstAllCurvesBumpAndRun(Products[instrumentNumber], asOf);
        }

        public void RiskAllInstruments(LinearRateModel model, DateTime asOf, bool useAd = false)
        {
            if (Products.Count == 0)
                throw new InvalidOperationException("Portfolio does not contain any instruments to risk");

            foreach (int key in Products.Keys)
                RiskInstrument(key, model, asOf, useAd);
        }

        /// <summary>
        /// This function aggregates the risk on all the portfolios elements
        /// and outputs them in a riskOutputContainer.
        /// </summary>
        /// <param name="asOf"></param>
        /// <returns></returns>
        public ZcbRiskOutputContainer AggregateRisk(DateTime asOf)
        {
            if (_productCounter == 0)
                throw new InvalidOperationException("Portfolio is empty");

            if (RiskOutputs.Keys.Count == 0)
                throw new InvalidOperationException("Risk has not been calculated on this portfolio");

            ZcbRiskOutputContainer output = new ZcbRiskOutputContainer();
            List<CurveTenor> tenors = new CurveTenor[] { CurveTenor.Fwd1M, CurveTenor.Fwd3M, CurveTenor.Fwd6M, CurveTenor.Fwd1Y }.ToList();

            // Loop over all forward curves
            foreach (CurveTenor tenor in tenors)
            {
               
                ZcbRiskOutput tempRiskOutput = new ZcbRiskOutput(asOf);
                
                // Loop over all assets
                foreach (int ident in RiskOutputs.Keys)
                {
                    if (ident == 0)
                    {
                        // Loop over all curve points and set starting valus as asset 0
                        foreach (DateTime key in RiskOutputs[0].FwdRiskCollection[tenor].IdentifierToPoint.Keys)
                        {
                            double value = RiskOutputs[0].FwdRiskCollection[tenor].RiskLookUp[RiskOutputs[0].FwdRiskCollection[tenor].IdentifierToPoint[key]];
                            tempRiskOutput.AddRiskCalculation(tenor, key, value);
                        }
                    }
                    else
                    {
                        // Loop over all curve points and add value to each curve point
                        foreach (DateTime key in RiskOutputs[0].FwdRiskCollection[tenor].IdentifierToPoint.Keys)
                        {
                            double value = RiskOutputs[ident].FwdRiskCollection[tenor].RiskLookUp[RiskOutputs[ident].FwdRiskCollection[tenor].IdentifierToPoint[key]];
                            tempRiskOutput.AddToCurvePoint(key, value);
                        }
                    }

                    // Add aggregated risk to output risk
                    output.AddForwardRisk(tenor, tempRiskOutput);
                }
            }

            // Do the same for the disc curve
            ZcbRiskOutput tempDiscRiskOutput = new ZcbRiskOutput(asOf);

            foreach (int ident in RiskOutputs.Keys)
            {
                if (ident == 0)
                {
                    // Loop over all curve points and set starting valus as asset 0
                    foreach (DateTime key in RiskOutputs[0].DiscRisk.IdentifierToPoint.Keys)
                    {
                        double value = RiskOutputs[0].DiscRisk.RiskLookUp[RiskOutputs[0].DiscRisk.IdentifierToPoint[key]];
                        tempDiscRiskOutput.AddRiskCalculation(CurveTenor.DiscOis, key, value);
                    }
                }
                else
                {
                    // Loop over all curve points and add value to each curve point
                    foreach (DateTime key in RiskOutputs[0].DiscRisk.IdentifierToPoint.Keys)
                    {
                        double value = RiskOutputs[ident].DiscRisk.RiskLookUp[RiskOutputs[ident].DiscRisk.IdentifierToPoint[key]];
                        tempDiscRiskOutput.AddToCurvePoint(key, value);
                    }
                }
            }

            output.AddDiscRisk(tempDiscRiskOutput);
            return output;
        }
    }

    /// <summary>
    /// Class that constructs and holds the Jacobian used to
    /// calculate outright risk (in terms of the input instruments).
    /// </summary>
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
        int _dimension;
        bool _hasBeenCreated = false;
        bool _hasBeenInitialized = false;


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
            SortInstruments();
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

        private void SortInstruments()
        {
            // Idea, create dictionary<int, identififer>
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

                    // The opposite
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
            // It's probably never exactly equal to zero
            //if (AlmostEqual(Jacobian.Determinant(), 0.0))
            //    throw new InvalidOperationException("Jacobian is not invertible. Determinant is 0.0");

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

    public class OutrightRiskOutput
    {
        public IDictionary<string, double> RiskLookUp { get; private set; }
        public CurveTenor Tenor { get; private set; }
        DateTime _asOf;

        public OutrightRiskOutput(DateTime asOf, CurveTenor tenor)
        {
            _asOf = asOf;
            Tenor = tenor;
            RiskLookUp = new Dictionary<string, double>();
        }

        public void AddRiskCalculation(string ident, double number)
        {
            RiskLookUp[ident] = number;
        }

        public object[,] CreateRiskArray()
        {
            object[,] output = new object[RiskLookUp.Count + 1, 2];

            output[0, 0] = "Point";
            output[0, 1] = "Value";

            int i = 1;

            foreach (string key in RiskLookUp.Keys)
            {
                output[i, 0] = key;
                output[i, 1] = Math.Round(RiskLookUp[key], 6);
                i = i + 1;
            }

            return output;
        }

    }

    // Consider making "PortfolioRiskOutput" that act as a container
    // for multiple RiskOutputs and can aggregate the results.
    public class ZcbRiskOutput
    {
        public IDictionary<string, double> RiskLookUp { get; private set; }
        public IDictionary<DateTime, string> IdentifierToPoint { get; private set; }
        DateTime _asOf;

        public ZcbRiskOutput(DateTime asOf)
        {
            RiskLookUp = new Dictionary<string, double>();
            IdentifierToPoint = new Dictionary<DateTime, string>();
            _asOf = asOf;
        }

        public void AddToCurvePoint(DateTime curvePoint, double addition)
        {
            RiskLookUp[IdentifierToPoint[curvePoint]] += addition;
        }

        public void AddRiskCalculation(CurveTenor curveTenor, DateTime curvePoint, double number)
        {
            string tenor = DateHandling.ConvertDateToTenorString(curvePoint, _asOf);
            string riskIdentifier = curveTenor.ToString() + "-" + curvePoint.ToString("dd/MM/yyyy");
            //string riskIdentifier = curveTenor.ToString() + "-" + tenor;
            RiskLookUp[riskIdentifier] = number; 
            IdentifierToPoint[curvePoint] = riskIdentifier;
        }

        public List<double> ConstructDeltaVector()
        {
            List<double> outputList = new List<double>();
            List<DateTime> orderedDates = new List<DateTime>();

            foreach (DateTime key in IdentifierToPoint.Keys)
                orderedDates.Add(key);

            orderedDates.OrderBy(x => x.Date).ToList();

            // Fetch number in an ordered matter
            foreach (DateTime date in orderedDates)
                outputList.Add(RiskLookUp[IdentifierToPoint[date]]);

            return outputList;
        }

        public object[,] CreateRiskArray()
        {
            object[,] output = new object[RiskLookUp.Count+1, 2];

            output[0, 0] = "Point";
            output[0, 1] = "Value";

            int i = 1;

            foreach (string key in RiskLookUp.Keys)
            {
                output[i, 0] = key;
                output[i, 1] = Math.Round(RiskLookUp[key], 6);
                i = i + 1;
            }

            return output;
        }
    }
    public class OutrightRiskContainer
    {
        public IDictionary<CurveTenor, OutrightRiskOutput> FwdRiskCollection { get; private set; }
        public OutrightRiskOutput DiscRisk { get; private set; }
        public IDictionary<CurveTenor, List<double>> DeltaVectors { get; private set; }

        public OutrightRiskContainer()
        {
            FwdRiskCollection = new Dictionary<CurveTenor, OutrightRiskOutput>();
            DeltaVectors = new Dictionary<CurveTenor, List<double>>();
        }

        public void AddForwardRisk(CurveTenor tenor, OutrightRiskOutput riskOutput)
        {
            FwdRiskCollection[tenor] = riskOutput;
        }

        public void AddDiscRisk(OutrightRiskOutput riskOutput)
        {
            DiscRisk = riskOutput;
        }
    }

    public class ZcbRiskOutputContainer
    {
        public IDictionary<CurveTenor, ZcbRiskOutput> FwdRiskCollection { get; private set; }
        public ZcbRiskOutput DiscRisk { get; private set; }
        public IDictionary<CurveTenor, List<double>> DeltaVectors { get; private set; }
        public List<double> FullGradient { get; private set; }

        public ZcbRiskOutputContainer()
        {
            FwdRiskCollection = new Dictionary<CurveTenor, ZcbRiskOutput>();
            DeltaVectors = new Dictionary<CurveTenor, List<double>>();
            FullGradient = new List<double>();
        }

        public void AddForwardRisk(CurveTenor tenor, ZcbRiskOutput riskOutput)
        {
            FwdRiskCollection[tenor] = riskOutput;
            DeltaVectors[tenor] = riskOutput.ConstructDeltaVector();
        }

        public void AddDiscRisk(ZcbRiskOutput riskOutput)
        {
            DiscRisk = riskOutput;
            DeltaVectors[CurveTenor.DiscOis] = riskOutput.ConstructDeltaVector();
        }

        public void ConstructFullGradient()
        {
            List<CurveTenor> tenors = new CurveTenor[] { CurveTenor.DiscOis, CurveTenor.Fwd1M, CurveTenor.Fwd3M, CurveTenor.Fwd6M, CurveTenor.Fwd1Y }.ToList();

            foreach (CurveTenor tenor in tenors)
                FullGradient.AddRange(DeltaVectors[tenor]);
        }
    }

    public class RiskEngine
    {
        private LinearRateModel _linearRateModel;
        private Portfolio _portfolio;
        public ZcbRiskOutputContainer ZcbRiskOutput { get; private set; }
        public OutrightRiskContainer OutrightRiskOutput { get; private set; }
        private RiskJacobian _jacobian;
        private List<double> _fullGradient;
        private List<double> _outrightRisk;
        bool _useAd;
        DateTime _asOf;

        public RiskEngine(LinearRateModel model, Portfolio portfolio, RiskJacobian jacobian, bool useAd = false)
        {
            _linearRateModel = model;
            _portfolio = portfolio;
            _jacobian = jacobian;
            ZcbRiskOutput = new ZcbRiskOutputContainer();
            OutrightRiskOutput = new OutrightRiskContainer();
            _asOf = jacobian.AsOf;
            _useAd = useAd;
        }

        public void CalculateOutrightRiskDeltaVector()
        {
            _outrightRisk = new List<double>();
            Matrix<double> outrightRiskCalculations = _jacobian.InvertedJacobian.Multiply(ConvertGradientToMatrix());
            for (int i = 0; i < outrightRiskCalculations.RowCount; i++)
                _outrightRisk.Add(outrightRiskCalculations[i, 0]);
        }

        public void ConvertOutrightRiskToRiskOutputObject()
        {
            // This is sensitive to the order of instruments ..
            CurveTenor[] tenors = { CurveTenor.DiscOis, CurveTenor.Fwd1M, CurveTenor.Fwd3M, CurveTenor.Fwd6M, CurveTenor.Fwd1Y };

            int j = 0;
            foreach (CurveTenor tenor in tenors)
            {
                OutrightRiskOutput tempRiskOutput = new OutrightRiskOutput(_asOf, tenor);

                if (tenor == CurveTenor.DiscOis)
                {
                    for (int i = 0; i < _jacobian.CurveDimensions[tenor]; i++)
                    {
                        string ident = _jacobian.Instruments[j].Identifier;
                        tempRiskOutput.AddRiskCalculation(ident, _outrightRisk[j]);
                        j += 1;
                    }

                    OutrightRiskOutput.AddDiscRisk(tempRiskOutput);
                }
                else
                {
                    for (int i = 0; i < _jacobian.CurveDimensions[tenor]; i++)
                    {
                        string ident = _jacobian.Instruments[j].Identifier;
                        tempRiskOutput.AddRiskCalculation(ident,_outrightRisk[j]);
                        j += 1;
                    }

                    OutrightRiskOutput.AddForwardRisk(tenor, tempRiskOutput);
                }
            }

        }

        public Matrix<double> ConvertGradientToMatrix()
        {
            Matrix<double> output = Matrix<double>.Build.Dense(_fullGradient.Count, 1);
            for (int i = 0; i < _fullGradient.Count; i++)
                output[i, 0] = _fullGradient[i];

            return output;
        }

        public void CalculateZcbRiskBumpAndRun()
        {
            _portfolio.RiskAllInstruments(_linearRateModel, _asOf, _useAd);
            ZcbRiskOutput = _portfolio.AggregateRisk(_asOf);
            ZcbRiskOutput.ConstructFullGradient();

            // Assumes numbers are aligned from disc curve to fwd1y curve.
            _fullGradient = ZcbRiskOutput.FullGradient;
            CalculateOutrightRiskDeltaVector();
            ConvertOutrightRiskToRiskOutputObject();
        }
    }
}
