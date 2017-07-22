using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis;
using MathNet.Numerics.LinearAlgebra;

namespace MasterThesis
{
    public class Portfolio
    {
        // Simple container for LinearRateProducts. Used to calculate risk on multiple assets at a time.

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

        // This method aggregates the risk on all the portfolios elements and outputs them in a riskOutputContainer.
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

    public class RiskEngine
    {
        // This class takes as input a jacobian and a portfolio.
        // It is responsible calculating the outright risk based on
        // the zero-coupon risk and the jacobian.

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

        public void CalculateZcbRisk()
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
