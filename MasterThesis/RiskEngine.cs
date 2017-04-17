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
        public IDictionary<int, LinearRateProduct> Products { get; private set; }
        public IDictionary<int, ZcbRiskOutputContainer> RiskOutputs { get; private set; }
        int _productCounter;

        public Portfolio()
        {
            Products = new Dictionary<int, LinearRateProduct>();
            RiskOutputs = new Dictionary<int, ZcbRiskOutputContainer>();
            _productCounter = 0;
        }

        public void AddProducts(params LinearRateProduct[] products)
        {
            for (int i = 0; i < products.Length; i++)
            {
                Products[_productCounter] = products[i];
                _productCounter += 1;
            }
        }

        public void RiskInstrument(int instrumentNumber, LinearRateModel model, DateTime asOf)
        {
            if (Products.ContainsKey(instrumentNumber) == false)
                throw new InvalidOperationException("Portfolio does not contain " + instrumentNumber + " as an identififer for a linearRateProduct.");

            RiskOutputs[instrumentNumber] = model.RiskAgainstAllCurvesBumpAndRun(Products[instrumentNumber], asOf);
        }

        public void RiskAllInstruments(LinearRateModel model, DateTime asOf)
        {
            if (Products.Count == 0)
                throw new InvalidOperationException("Portfolio does not contain any instruments to risk");

            foreach (int key in Products.Keys)
                RiskInstrument(key, model, asOf);
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
        List<CalibrationInstrument> _instruments;
        IDictionary<CurveTenor, List<CalibrationInstrument>> _instrumentDictionary;
        IDictionary<string, List<double>> _fullGradients;
        Matrix<double> _jacobian;
        public LinearRateModel Model { get; private set; }
        public DateTime AsOf { get; private set; }
        int _dimension;
        bool _hasBeenCreated = false;


        public RiskJacobian(LinearRateModel model, DateTime asOf)
        {
            Model = model;
            AsOf = asOf;

            _instruments = new List<CalibrationInstrument>();
            _fullGradients = new Dictionary<string, List<double>>();
            _instrumentDictionary = new Dictionary<CurveTenor, List<CalibrationInstrument>>();
        }


        private void SetJacobian()
        {
            // Should set the dimensions of the Jacobian based on number of instruments
            // Remember N = M. Matrix initialized with zeros.
            _dimension = _instruments.Count;
            _jacobian = Matrix<double>.Build.Dense(_dimension, _dimension);
        }

        public void AddInstruments(List<CalibrationInstrument> instruments, CurveTenor tenor)
        {
            _instruments.AddRange(instruments);
            _instrumentDictionary[tenor] = instruments;
            SortInstruments();
        }

        private void SortInstruments()
        {

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

        private void BumpAndRunRiskInstruments()
        {
            foreach (CalibrationInstrument product in _instruments)
            {
                product.RiskInstrumentBumpAndRun(Model, AsOf);
                _fullGradients[product.Identifier] = product.RiskOutput.FullGradient;
            }
        }

        public Matrix<double> Get()
        {
            if (_hasBeenCreated)
                return _jacobian;
            else
                throw new InvalidOperationException("Jacobian has not been constructed.");
        }

        public void ConstructUsingAD()
        {
            VerifyModelDimension();
            SetJacobian();
            SortInstruments();

            _hasBeenCreated = true;
        }

        public void ConstructUsingBumpAndRun()
        {
            VerifyModelDimension();
            SetJacobian();
            SortInstruments();
            BumpAndRunRiskInstruments();

            _hasBeenCreated = true;
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
            //string riskIdentifier = curveTenor.ToString() + "-" + curvePoint.ToString("dd/MM/yyyy");
            string riskIdentifier = curveTenor.ToString() + "-" + tenor;
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

    public class RiskEngineNew
    {
        private LinearRateModel _linearRateModel;
        private Portfolio _portfolio;
        public ZcbRiskOutputContainer RiskOutput { get; private set; }
        private RiskJacobian _jacobian;
        private List<double> _fullGradient;
        DateTime _asOf;

        // NEED METHOD TO DETERMINE OUTRIGHT DELTA VECTORS

        /// <summary>
        /// Checks that the riskOutputs have the same dimension for all the 
        /// instruments in the portfolio before risking.
        /// </summary>
        /// <returns></returns>
        public bool IsPreconditionSatisfied()
        {
            int currentDimension = _portfolio.RiskOutputs[0].FwdRiskCollection.Count;
            
            // Check that each instrument has risk calculated against same number of fwdCurves
            foreach (int key in _portfolio.RiskOutputs.Keys)
            {
                if (currentDimension != _portfolio.RiskOutputs[0].FwdRiskCollection.Count)
                    return false;
            }

            return true;
        }

        public RiskEngineNew(LinearRateModel model, Portfolio portfolio, RiskJacobian jacobian)
        {
            _linearRateModel = model;
            _portfolio = portfolio;
            _jacobian = jacobian;
            RiskOutput = new ZcbRiskOutputContainer();
            _asOf = jacobian.AsOf;
        }

        public void CalculateZcbRiskBumpAndRun()
        {
            _portfolio.RiskAllInstruments(_linearRateModel, _asOf);
            RiskOutput = _portfolio.AggregateRisk(_asOf);
            RiskOutput.ConstructFullGradient();

            // Assumes numbers are aligned from disc curve to fwd1y curve.
            _fullGradient = RiskOutput.FullGradient;
        }
    }

    public class RiskEngine
    {
        private LinearRateModel _linearRateModel;
        private InstrumentFactory _factory;
        private Portfolio _portfolio;
        public ZcbRiskOutputContainer RiskOutput;
        private LinearRateProduct _tempProduct;

        public RiskEngine(LinearRateModel model, InstrumentFactory factory, Portfolio portfolio)
        {
            _linearRateModel = model;
            _factory = factory;
            _portfolio = portfolio;
            RiskOutput = new ZcbRiskOutputContainer();
        }

        public RiskEngine(LinearRateModel model, InstrumentFactory factory, LinearRateProduct product)
        {
            _linearRateModel = model;
            _factory = factory;
            _tempProduct = product;
            RiskOutput = new ZcbRiskOutputContainer();
        }

        public void AddTradeTradeToPortfolio(LinearRateProduct product)
        {
            _portfolio.AddProducts(product);
        }

        public void CurveRiskSwap()
        {
            CurveTenor[] tenors = new CurveTenor[] { CurveTenor.Fwd1M, CurveTenor.Fwd3M, CurveTenor.Fwd6M, CurveTenor.Fwd1Y };

            // Forward risk
            for (int i = 0; i < tenors.Length; i++)
            {
                ZcbRiskOutput fwdRiskOutput = new MasterThesis.ZcbRiskOutput(_factory.AsOf);

                for (int j = 0; j<_linearRateModel.FwdCurveCollection.GetCurve(tenors[i]).Dates.Count; j++)
                {
                    DateTime curvePoint = _linearRateModel.FwdCurveCollection.GetCurve(tenors[i]).Dates[j];
                    double riskValue = _linearRateModel.BumpAndRunFwdRisk(_tempProduct, tenors[i], j);
                    fwdRiskOutput.AddRiskCalculation(tenors[i], curvePoint, riskValue);
                }

                RiskOutput.AddForwardRisk(tenors[i], fwdRiskOutput);
            }

            ZcbRiskOutput discRiskOutput = new MasterThesis.ZcbRiskOutput(_factory.AsOf);

            for (int i = 0; i<_linearRateModel.DiscCurve.Values.Count; i++)
            {
                DateTime curvePoint = _linearRateModel.DiscCurve.Dates[i];
                double riskValue = _linearRateModel.BumpAndRunDisc(_tempProduct, i);
                discRiskOutput.AddRiskCalculation(CurveTenor.DiscOis, curvePoint, riskValue);
            }

            RiskOutput.AddDiscRisk(discRiskOutput);
        }
    }
}
