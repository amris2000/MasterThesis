﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
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
}
