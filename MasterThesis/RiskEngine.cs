using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis;

namespace MasterThesis
{

    /// <summary>
    /// Simple container for assets
    /// </summary>
    public class Portfolio
    {
        List<LinearRateProduct> _products;
        List<Instrument> _instrumentMap;

        public Portfolio()
        {
            _products = new List<LinearRateProduct>();
            _instrumentMap = new List<Instrument>();
        }

        public void AddProducts(params LinearRateProduct[] products)
        {
            for (int i = 0; i < products.Length; i++)
            {
                _products.Add(products[i]);
                _instrumentMap.Add(products[i].GetInstrumentType());
            }
        }

        public LinearRateProduct[] GetProducts()
        {
            return _products.ToArray();
        }
    }

    // Consider making "PortfolioRiskOutput" that act as a container
    // for multiple RiskOutputs and can aggregate the results.
    public class RiskOutput
    {
        IDictionary<string, double> _riskLookUp;

        public RiskOutput()
        {
            _riskLookUp = new Dictionary<string, double>();
        }

        public void AddRiskCalculation(CurveTenor curveTenor, DateTime curvePoint, double number)
        {
            string riskIdentifier = curveTenor.ToString() + "-" + curvePoint.ToString("dd/MM/yyyy");
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
                RiskOutput fwdRiskOutput = new MasterThesis.RiskOutput();

                for (int j = 0; j<_linearRateModel.FwdCurveCollection.GetCurve(tenors[i]).Dates.Count; j++)
                {
                    DateTime curvePoint = _linearRateModel.FwdCurveCollection.GetCurve(tenors[i]).Dates[j];
                    double riskValue = _linearRateModel.BumpAndRunFwdRisk(_tempProduct, tenors[i], j);
                    fwdRiskOutput.AddRiskCalculation(tenors[i], curvePoint, riskValue);
                }

                RiskOutput.AddForwardRisk(tenors[i], fwdRiskOutput);
            }

            RiskOutput discRiskOutput = new MasterThesis.RiskOutput();

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
