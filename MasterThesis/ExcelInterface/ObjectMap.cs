using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis.ExcelInterface
{
    /* General information:
     * This file contains the ObjectMap used to store objects from 
     * Excel. As discussed in the thesis, this is defind as a static class
     * meaning that it never runs out of scope as long as the application is
     * is running. The objects themselves are stored in "dictionaries" using the
     * handles specified from Excel.
     */

    public static class ObjectMap
    {
        #region Curves and models.
        public static IDictionary<string, Curve> DiscCurves = new Dictionary<string, Curve>();
        public static IDictionary<string, FwdCurveContainer> FwdCurveCollections = new Dictionary<string, FwdCurveContainer>();
        public static IDictionary<string, Curve> FwdCurves = new Dictionary<string, Curve>();
        public static IDictionary<string, LinearRateModel> LinearRateModels = new Dictionary<string, LinearRateModel>();
        public static IDictionary<string, FwdCurveRepresentation> FwdCurveRepresentations = new Dictionary<string, FwdCurveRepresentation>();
        #endregion

        #region Derivative objects
        public static IDictionary<string, FixedLeg> FixedLegs = new Dictionary<string, FixedLeg>();
        public static IDictionary<string, FloatLeg> FloatLegs = new Dictionary<string, FloatLeg>();
        public static IDictionary<string, IrSwap> IrSwaps = new Dictionary<string, IrSwap>();
        public static IDictionary<string, TenorBasisSwap> BasisSwaps = new Dictionary<string, TenorBasisSwap>();
        public static IDictionary<string, LinearRateInstrument> LinearRateInstruments = new Dictionary<string, LinearRateInstrument>();
        #endregion

        #region Related to InstrumentFactory
        public static IDictionary<string, InstrumentFactory> InstrumentFactories = new Dictionary<string, InstrumentFactory>();
        #endregion

        #region Related to curve calibration
        public static IDictionary<string, CurveCalibrationProblem> CurveCalibrationProblems = new Dictionary<string, CurveCalibrationProblem>();
        public static IDictionary<string, FwdCurveConstructor> FwdCurveConstructors = new Dictionary<string, FwdCurveConstructor>();
        public static IDictionary<string, CalibrationSpec> CalibrationSettings = new Dictionary<string, CalibrationSpec>();
        #endregion

        #region Related to RiskEngine
        public static IDictionary<string, List<CalibrationInstrument>> CalibrationInstrumentSets = new Dictionary<string, List<CalibrationInstrument>>();
        public static IDictionary<string, ZcbRiskOutputContainer> ZcbRiskOutputContainers = new Dictionary<string, ZcbRiskOutputContainer>();
        public static IDictionary<string, OutrightRiskContainer> OutrightRiskContainers = new Dictionary<string, OutrightRiskContainer>();
        public static IDictionary<string, RiskEngine> RiskEngines = new Dictionary<string, RiskEngine>();
        public static IDictionary<string, Portfolio> Portfolios = new Dictionary<string, Portfolio>();
        public static IDictionary<string, RiskJacobian> RiskJacobians = new Dictionary<string, RiskJacobian>();
        public static IDictionary<string, ZcbRiskOutput> ZcbRiskOutputs = new Dictionary<string, ZcbRiskOutput>();
        public static IDictionary<string, OutrightRiskOutput> OutrightRiskOutputs = new Dictionary<string, OutrightRiskOutput>();
        #endregion

        public static void CheckExists<T>(IDictionary<string, T> dictionary, string key, string errMessage)
        {
            if (dictionary.ContainsKey(key) == false)
                throw new InvalidOperationException(errMessage);

        }
    }
}
