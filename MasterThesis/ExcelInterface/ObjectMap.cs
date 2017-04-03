using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis.ExcelInterface
{
    public static class ObjectMap
    {
        public static IDictionary<string, Curve> DiscCurves = new Dictionary<string, Curve>();
        public static IDictionary<string, FwdCurves> FwdCurveCollections = new Dictionary<string, FwdCurves>();
        public static IDictionary<string, Curve> FwdCurves = new Dictionary<string, Curve>();
        public static IDictionary<string, LinearRateModel> LinearRateModels = new Dictionary<string, LinearRateModel>();

        // Swap functionality
        public static IDictionary<string, FixedLeg> FixedLegs = new Dictionary<string, FixedLeg>();
        public static IDictionary<string, FloatLeg> FloatLegs = new Dictionary<string, FloatLeg>();
        public static IDictionary<string, IrSwap> IrSwaps = new Dictionary<string, IrSwap>();
        public static IDictionary<string, BasisSwap> BasisSwaps = new Dictionary<string, BasisSwap>();

        // InstrumentFactories
        public static IDictionary<string, InstrumentFactory> InstrumentFactories = new Dictionary<string, InstrumentFactory>();

        // Calibration
        public static IDictionary<string, CurveCalibrationProblem> CurveCalibrationProblems = new Dictionary<string, CurveCalibrationProblem>();
        public static IDictionary<string, FwdCurveConstructor> FwdCurveConstructors = new Dictionary<string, FwdCurveConstructor>();
        public static IDictionary<string, CalibrationSpec> CalibrationSettings = new Dictionary<string, CalibrationSpec>();

        public static void CheckExists<T>(IDictionary<string, T> dictionary, string key, string errMessage)
        {
            if (dictionary.ContainsKey(key) == false)
                throw new InvalidOperationException(errMessage);

        }
    }
}
