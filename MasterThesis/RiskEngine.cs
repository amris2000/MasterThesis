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
        Matrix<double> _jacobian;
        bool _hasBeenCreated = false;


        public RiskJacobian(List<CalibrationInstrument> instruments)
        {
            _instruments = instruments;

            SetJacobian();
            SortInstruments();
        }


        private void SetJacobian()
        {
            // Should set the dimensions of the Jacobian based on number of instruments
            // Remember N = M. Matrix initialized with zeros.
            _jacobian = Matrix<double>.Build.Dense(_instruments.Count, _instruments.Count);
        }

        private void SortInstruments()
        {

        }

        public void VerifyModelDimension(LinearRateModel model)
        {
            int n = _instruments.Count;

            // Calculate number of curve points in model
            int m = 0;

            if (n != m)
                throw new InvalidOperationException("Number of curve points and input instruments has to be equal.");
        }

        public void ConstructUsingAD(LinearRateModel model)
        {
            VerifyModelDimension(model);

            _hasBeenCreated = true;
        }

        public void ConstructUsingBumpAndRun(LinearRateModel model)
        {
            VerifyModelDimension(model);

            _hasBeenCreated = true;
        }

    }

    // Consider making "PortfolioRiskOutput" that act as a container
    // for multiple RiskOutputs and can aggregate the results.
    public class RiskOutput
    {
        IDictionary<string, double> _riskLookUp;
        DateTime _asOf;

        public RiskOutput(DateTime asOf)
        {
            _riskLookUp = new Dictionary<string, double>();
            _asOf = asOf;
        }

        private string convertDateToTenor(DateTime date)
        {
            double tenor = date.Subtract(_asOf).TotalDays / 365;
            int years = (int)Math.Truncate(tenor);
            double leftover = tenor - years;

            string tenorLetter;
            int tenorNumber;

            if (years == 0)
            {
                tenorLetter = "M";
                tenorNumber = (int)Math.Round(leftover * 12.0);
                if (tenorNumber == 12)
                {
                    tenorLetter = "Y";
                    tenorNumber = 1;
                }
            }
            else if (years == 1)
            {
                if (leftover < 0.95)
                {
                    tenorNumber = 12 + (int)Math.Round(leftover * 12.0);
                    tenorLetter = "M";
                }
                else
                {
                    tenorNumber = 2;
                    tenorLetter = "Y";
                }
            }
            else if (years >= 2)
            {
                tenorLetter = "Y";
                if (leftover < 0.5)
                    tenorNumber = years;
                else
                    tenorNumber = years + 1;
            }
            else
            {
                tenorLetter = "?";
                tenorNumber = 0;
            }

            return tenorNumber.ToString() + tenorLetter;
        }

        public void AddRiskCalculation(CurveTenor curveTenor, DateTime curvePoint, double number)
        {
            string tenor = convertDateToTenor(curvePoint);
            //string riskIdentifier = curveTenor.ToString() + "-" + curvePoint.ToString("dd/MM/yyyy");
            string riskIdentifier = curveTenor.ToString() + "-" + tenor;
            _riskLookUp[riskIdentifier] = number;
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
                output[i, 1] = Math.Round(_riskLookUp[key], 6).ToString();
                i = i + 1;
            }

            return output;
        }
    }

    public class RiskOutputContainer
    {
        public IDictionary<CurveTenor, RiskOutput> FwdRiskCollection;
        public RiskOutput DiscRisk;

        public RiskOutputContainer()
        {
            FwdRiskCollection = new Dictionary<CurveTenor, RiskOutput>();
        }

        public void AddForwardRisk(CurveTenor tenor, RiskOutput riskOutput)
        {
            FwdRiskCollection[tenor] = riskOutput;
        }

        public void AddDiscRisk(RiskOutput riskOutput)
        {
            DiscRisk = riskOutput;
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
