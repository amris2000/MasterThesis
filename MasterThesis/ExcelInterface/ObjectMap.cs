using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis.ExcelInterface
{
    public static class ObjectMap
    {
        // Curves and models
        public static IDictionary<string, Curve> DiscCurves = new Dictionary<string, Curve>();
        public static IDictionary<string, FwdCurveContainer> FwdCurveCollections = new Dictionary<string, FwdCurveContainer>();
        public static IDictionary<string, Curve> FwdCurves = new Dictionary<string, Curve>();
        public static IDictionary<string, LinearRateModel> LinearRateModels = new Dictionary<string, LinearRateModel>();
        public static IDictionary<string, FwdCurveRepresentation> FwdCurveRepresentations = new Dictionary<string, FwdCurveRepresentation>();

        // Derivatives
        public static IDictionary<string, FixedLeg> FixedLegs = new Dictionary<string, FixedLeg>();
        public static IDictionary<string, FloatLeg> FloatLegs = new Dictionary<string, FloatLeg>();
        public static IDictionary<string, IrSwap> IrSwaps = new Dictionary<string, IrSwap>();
        public static IDictionary<string, TenorBasisSwap> BasisSwaps = new Dictionary<string, TenorBasisSwap>();
        public static IDictionary<string, LinearRateInstrument> LinearRateInstruments = new Dictionary<string, LinearRateInstrument>();

        // InstrumentFactories
        public static IDictionary<string, InstrumentFactory> InstrumentFactories = new Dictionary<string, InstrumentFactory>();

        // Calibration
        public static IDictionary<string, CurveCalibrationProblem> CurveCalibrationProblems = new Dictionary<string, CurveCalibrationProblem>();
        public static IDictionary<string, FwdCurveConstructor> FwdCurveConstructors = new Dictionary<string, FwdCurveConstructor>();
        public static IDictionary<string, CalibrationSpec> CalibrationSettings = new Dictionary<string, CalibrationSpec>();

        // RiskEngine
        public static IDictionary<string, List<CalibrationInstrument>> CalibrationInstrumentSets = new Dictionary<string, List<CalibrationInstrument>>();
        public static IDictionary<string, ZcbRiskOutputContainer> ZcbRiskOutputContainers = new Dictionary<string, ZcbRiskOutputContainer>();
        public static IDictionary<string, OutrightRiskContainer> OutrightRiskContainers = new Dictionary<string, OutrightRiskContainer>();
        public static IDictionary<string, RiskEngine> RiskEngines = new Dictionary<string, RiskEngine>();
        public static IDictionary<string, Portfolio> Portfolios = new Dictionary<string, Portfolio>();
        public static IDictionary<string, RiskJacobian> RiskJacobians = new Dictionary<string, RiskJacobian>();
        public static IDictionary<string, ZcbRiskOutput> ZcbRiskOutputs = new Dictionary<string, ZcbRiskOutput>();
        public static IDictionary<string, OutrightRiskOutput> OutrightRiskOutputs = new Dictionary<string, OutrightRiskOutput>();


        public static void CheckExists<T>(IDictionary<string, T> dictionary, string key, string errMessage)
        {
            if (dictionary.ContainsKey(key) == false)
                throw new InvalidOperationException(errMessage);

        }
    }
}
