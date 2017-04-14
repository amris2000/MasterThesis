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
    /// Simple container for assets
    /// </summary>
    public class Portfolio
    {
        List<LinearRateProduct> _products;

        public Portfolio()
        {
            _products = new List<LinearRateProduct>();
        }

        public void AddProducts(params LinearRateProduct[] products)
        {
            for (int i = 0; i < products.Length; i++)
            {
                _products.Add(products[i]);
            }
        }

        public LinearRateProduct[] GetProducts()
        {
            return _products.ToArray();
        }
    }

    /// <summary>
    /// Class that constructs and holds the Jacobian used to
    /// calculate outright risk (in terms of the input instruments).
    /// </summary>
    public class RiskJacobian
    {
        List<CalibrationInstrument> _instruments;
        IDictionary<string, List<double>> _fullGradients;
        Matrix<double> _jacobian;
        LinearRateModel _model;
        DateTime _asOf;
        int _dimension;
        bool _hasBeenCreated = false;


        public RiskJacobian(LinearRateModel model, DateTime asOf)
        {
            _model = model;
            _asOf = asOf;

            _instruments = new List<CalibrationInstrument>();
            _fullGradients = new Dictionary<string, List<double>>();
        }


        private void SetJacobian()
        {
            // Should set the dimensions of the Jacobian based on number of instruments
            // Remember N = M. Matrix initialized with zeros.
            _dimension = _instruments.Count;
            _jacobian = Matrix<double>.Build.Dense(_dimension, _dimension);
        }

        public void AddInstruments(List<CalibrationInstrument> instruments)
        {
            _instruments.AddRange(instruments);
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
            m += _model.DiscCurve.Dimension;

            foreach (CurveTenor tenor in _model.FwdCurveCollection.Curves.Keys)
                m += _model.FwdCurveCollection.Curves[tenor].Dimension;

            if (_dimension != m)
                throw new InvalidOperationException("Number of curve points and input instruments has to be equal.");
        }

        private void BumpAndRunRiskInstruments()
        {
            foreach (CalibrationInstrument product in _instruments)
            {
                product.RiskInstrumentBumpAndRun(_model, _asOf);
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
    public class RiskOutput
    {
        IDictionary<string, double> _riskLookUp;
        IDictionary<DateTime, string> _identifierToPoint;
        DateTime _asOf;

        public RiskOutput(DateTime asOf)
        {
            _riskLookUp = new Dictionary<string, double>();
            _identifierToPoint = new Dictionary<DateTime, string>();
            _asOf = asOf;
        }

        public void AddRiskCalculation(CurveTenor curveTenor, DateTime curvePoint, double number)
        {
            string tenor = DateHandling.ConvertDateToTenorString(curvePoint, _asOf);
            //string riskIdentifier = curveTenor.ToString() + "-" + curvePoint.ToString("dd/MM/yyyy");
            string riskIdentifier = curveTenor.ToString() + "-" + tenor;
            _riskLookUp[riskIdentifier] = number; 
            _identifierToPoint[curvePoint] = riskIdentifier;
        }

        public List<double> ConstructDeltaVector()
        {
            List<double> outputList = new List<double>();
            List<DateTime> orderedDates = new List<DateTime>();

            foreach (DateTime key in _identifierToPoint.Keys)
                orderedDates.Add(key);

            orderedDates.OrderBy(x => x.Date).ToList();

            // Fetch number in an ordered matter
            foreach (DateTime date in orderedDates)
                outputList.Add(_riskLookUp[_identifierToPoint[date]]);

            return outputList;
        }

        public object[,] CreateRiskArray()
        {
            object[,] output = new object[_riskLookUp.Count+1, 2];

            output[0, 0] = "Point";
            output[0, 1] = "Value";

            int i = 1;

            foreach (string key in _riskLookUp.Keys)
            {
                output[i, 0] = key;
                output[i, 1] = Math.Round(_riskLookUp[key], 6);
                i = i + 1;
            }

            return output;
        }
    }

    public class RiskOutputContainer
    {
        public IDictionary<CurveTenor, RiskOutput> FwdRiskCollection { get; private set; }
        public RiskOutput DiscRisk { get; private set; }
        public IDictionary<CurveTenor, List<double>> DeltaVectors { get; private set; }
        public List<double> FullGradient { get; private set; }

        public RiskOutputContainer()
        {
            FwdRiskCollection = new Dictionary<CurveTenor, RiskOutput>();
            DeltaVectors = new Dictionary<CurveTenor, List<double>>();
            FullGradient = new List<double>();
        }

        public void AddForwardRisk(CurveTenor tenor, RiskOutput riskOutput)
        {
            FwdRiskCollection[tenor] = riskOutput;
            DeltaVectors[tenor] = riskOutput.ConstructDeltaVector();
        }

        public void AddDiscRisk(RiskOutput riskOutput)
        {
            DiscRisk = riskOutput;
            DeltaVectors[CurveTenor.DiscOis] = riskOutput.ConstructDeltaVector();
        }

        public void ConstructFullGradient()
        {
            List<CurveTenor> tenors = new CurveTenor[] { CurveTenor.DiscOis, CurveTenor.Fwd1M, CurveTenor.Fwd3M, CurveTenor.Fwd6M, CurveTenor.Fwd1Y }.ToList();

            foreach (CurveTenor tenor in tenors)
            {
                FullGradient.AddRange(DeltaVectors[tenor]);
            }
        }
    }


    public class RiskEngine
    {
        private LinearRateModel _linearRateModel;
        private InstrumentFactory _factory;
        private Portfolio _portfolio;
        public RiskOutputContainer RiskOutput;
        private LinearRateProduct _tempProduct;

        public RiskEngine(LinearRateModel model, InstrumentFactory factory, Portfolio portfolio)
        {
            _linearRateModel = model;
            _factory = factory;
            _portfolio = portfolio;
            RiskOutput = new RiskOutputContainer();
        }

        public RiskEngine(LinearRateModel model, InstrumentFactory factory, LinearRateProduct product)
        {
            _linearRateModel = model;
            _factory = factory;
            _tempProduct = product;
            RiskOutput = new RiskOutputContainer();
        }

        public void AddPortfolioProducts(Portfolio portfolio)
        {
            _portfolio.AddProducts(portfolio.GetProducts());
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
                RiskOutput fwdRiskOutput = new MasterThesis.RiskOutput(_factory.AsOf);

                for (int j = 0; j<_linearRateModel.FwdCurveCollection.GetCurve(tenors[i]).Dates.Count; j++)
                {
                    DateTime curvePoint = _linearRateModel.FwdCurveCollection.GetCurve(tenors[i]).Dates[j];
                    double riskValue = _linearRateModel.BumpAndRunFwdRisk(_tempProduct, tenors[i], j);
                    fwdRiskOutput.AddRiskCalculation(tenors[i], curvePoint, riskValue);
                }

                RiskOutput.AddForwardRisk(tenors[i], fwdRiskOutput);
            }

            RiskOutput discRiskOutput = new MasterThesis.RiskOutput(_factory.AsOf);

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
