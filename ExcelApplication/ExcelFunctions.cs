using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ExcelDna.Integration;
using MasterThesis;
using MasterThesis.ExcelInterface;

namespace ExcelApplication
{
    /* General information:
     * This file contains the actual function definitions as seen from Excel.
     * The functions defined here can therefore be used directly from excel.
     * The setup is build such that all the functions below call methods
     * within the "InterfaceFunctions"-class in the library.
     */

    public class ExcelFunctions
    {
        #region Curves: make functions
        public static DateTime[] ConvertDoublesToDateTimes(double[] doubles)
        {
            DateTime[] dates = new DateTime[doubles.Length];
            for (int i = 0; i < doubles.Length; i++)
                dates[i] = DateTime.FromOADate(doubles[i]);

            return dates;
        }

        [ExcelFunction(Description = "Create discounting curve from dates and values.", Name = "mt.Curve.DiscCurve.Make", IsVolatile = true)]
        public static string LinearRate_DiscCurve_Make(string baseHandle, object[] dates, object[] values)
        {

            var dValues = values.Cast<double>();
            var dDates = dates.Cast<double>();
            DateTime[] actualDates = ConvertDoublesToDateTimes(dDates.ToArray());

            List<DateTime> datesList = actualDates.ToList();
            List<double> doubleList = dValues.ToList();
            LinearRateFunctions.DiscCurve_Make(baseHandle, datesList, doubleList);
            return baseHandle;
        }

        [ExcelFunction(Description = "Create forward curves from dates and values.", Name = "mt.Curve.FwdCurve.Make", IsVolatile = true)]
        public static string LinearRate_FwdCurve_Make(string baseHandle, object[] dates, object[] values)
        {

            var dValues = values.Cast<double>();
            var dDates = dates.Cast<double>();
            DateTime[] actualDates = ConvertDoublesToDateTimes(dDates.ToArray());

            List<DateTime> datesList = actualDates.ToList();
            List<double> doubleList = dValues.ToList();
            LinearRateFunctions.FwdCurve_Make(baseHandle, datesList, doubleList);
            return baseHandle;
        }

        [ExcelFunction(Description = "Create forward curve collection from forward curves. Used as input in LinearRateModel.", Name = "mt.Curve.FwdCurveCollection.Make", IsVolatile = true)]
        public static string LinearRate_FwdCurveCollection_Make(string baseHandle, object[] fwdCurveHandles, object[] tenorNames)
        {
            CurveTenor[] tenorEnums = new CurveTenor[tenorNames.Length];
            var fwdCurveNamesString = fwdCurveHandles.Cast<string>().ToArray();

            for (int i = 0; i < tenorEnums.Length; i++)
                tenorEnums[i] = StrToEnum.CurveTenorConvert((string)tenorNames[i]);

            string output = LinearRateFunctions.FwdCurveCollection_Make(baseHandle, fwdCurveNamesString, tenorEnums);

            return output;
        }

        [ExcelFunction(Description = "Bump and existing curve and store it with a new handle.", Name = "mt.Curve.BumpAndStore", IsVolatile = true)]
        public static string LinearRate_BumpCurveAndStore(string baseHandle, string originalCurveHandle, int curvePoint, double bumpSize)
        {
            LinearRateFunctions.Curve_BumpCurveAndStore(baseHandle, originalCurveHandle, curvePoint, bumpSize);
            return baseHandle;
        }

        [ExcelFunction(Description = "From a forward collection, store a forward curve of a given tenor.", Name = "mt.Curve.FwdCurve.StoreFromCollection")]
        public static string LinearRate_FwdCurve_StoreFromCollection(string baseHandle, string fwdCurveCollectionHandle, string tenor)
        {
            CurveTenor tenorActual = StrToEnum.CurveTenorConvert(tenor);
            LinearRateFunctions.FwdCurve_StoreFromCollection(baseHandle, fwdCurveCollectionHandle, tenorActual);
            return baseHandle;
        }
        #endregion

        #region Curves: functions to get curves or calculate values on curves
        [ExcelFunction(Description = "Print discounting curve.", Name = "mt.Curve.DiscCurve.Get", IsVolatile = true)]
        public static object[,] LinearRate_DiscCurve_Get(string discCurveHandle)
        {
            return LinearRateFunctions.DiscCurve_Get(discCurveHandle);
        }

        [ExcelFunction(Description = "Print forward curve from forward curve collection.", Name = "mt.Curve.FwdCurve.GetFromCollection", IsVolatile = true)]
        public static object[,] LinearRate_FwdCurve_GetFromCollection(string fwdCurveCollectionHandle, string fwdCurveTenor)
        {
            CurveTenor tenorEnum = StrToEnum.CurveTenorConvert(fwdCurveTenor);
            return LinearRateFunctions.FwdCurve_GetFromCollection(fwdCurveCollectionHandle, tenorEnum);
        }

        //[ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.DiscCurve.GetValue", IsVolatile = true)]
        //public static double LinearRate_DiscCurve_GetValue(string baseHandle, double date, string interpolationMethod)
        //{
        //    DateTime dateTime = DateTime.FromOADate(date);
        //    InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationMethod);
        //    return LinearRateFunctions.DiscCurve_GetValue(baseHandle, dateTime, interpolation);
        //}

        [ExcelFunction(Description = "Some description.", Name = "mt.Curve.CalcFwdRate", IsVolatile = true)]
        public static double LinearRate_Curve_GetFwdRate(string curveHandle, DateTime asOf, DateTime startDate, string tenorStr, string dayCountStr, string dayRuleStr, string interpolationStr)
        {
            CurveTenor tenor = StrToEnum.CurveTenorFromSimpleTenor(tenorStr);
            InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationStr);
            DayCount dayCount = StrToEnum.DayCountConvert(dayCountStr);
            DayRule dayRule = StrToEnum.DayRuleConvert(dayRuleStr);

            return LinearRateFunctions.Curve_GetFwdRate(curveHandle, asOf, startDate, tenor, dayCount, dayRule, interpolation);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.Curve.CalcValue", IsVolatile = true)]
        public static double LinearRate_Curve_GetValue(string baseHandle, double date, string interpolationMethod)
        {
            DateTime dateTime = DateTime.FromOADate(date);
            InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationMethod);
            return LinearRateFunctions.Curve_GetValue(baseHandle, dateTime, interpolation);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.Curve.CalcDiscFactor", IsVolatile = true)]
        public static double LinearRate_Curve_GetValue(string baseHandle, DateTime asOf, DateTime date, string dayCountStr, string interpolationStr)
        {
            InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationStr);
            DayCount dayCount = StrToEnum.DayCountConvert(dayCountStr);
            return LinearRateFunctions.Curve_GetDiscFactor(baseHandle, asOf, date, dayCount, interpolation);
        }

        //[ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurve.GetValue", IsVolatile = true)]
        //public static double LinearRate_FwdCurve_GetValue(string baseHandle, double date, string interpolationMethod)
        //{
        //    DateTime dateTime = DateTime.FromOADate(date);
        //    InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationMethod);
        //    return LinearRateFunctions.FwdCurve_GetValue(baseHandle, dateTime, interpolation);
        //}

        // .. FWD CURVE REPRESENTATIONS

        //[ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurveRepresentation.MakeFromFwdCurve", IsVolatile = true)]
        //public static string LinearRate_FwdCurveRepresentation_MakeFromFwdCurve(string baseHandle, string fwdCurveHandle, string curveTenorStr, DateTime asOf
        //    , string dayCountStr, string dayRuleStr, string interpolationStr)
        //{
        //    CurveTenor tenor = StrToEnum.CurveTenorFromSimpleTenor(curveTenorStr);
        //    InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationStr);
        //    DayCount dayCount = StrToEnum.DayCountConvert(dayCountStr);
        //    DayRule dayRule = StrToEnum.DayRuleConvert(dayRuleStr);

        //    LinearRateFunctions.FwdCurveRepresentation_MakeFromFwdCurve(baseHandle, fwdCurveHandle, tenor, asOf, dayCount, dayRule, interpolation);
        //    return baseHandle;
        //}

        //[ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurveRepresentation.MakeFromDiscCurve", IsVolatile = true)]
        //public static string LinearRate_FwdCurveRepresentation_MakeFromDiscCurve(string baseHandle, string discCurveHandle, string curveTenorStr, DateTime asOf
        //    , string dayCountStr, string dayRuleStr, string interpolationStr)
        //{
        //    CurveTenor tenor = StrToEnum.CurveTenorConvert(curveTenorStr);
        //    InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationStr);
        //    DayCount dayCount = StrToEnum.DayCountConvert(dayCountStr);
        //    DayRule dayRule = StrToEnum.DayRuleConvert(dayRuleStr);

        //    LinearRateFunctions.FwdCurveRepresentation_MakeFromDiscCurve(baseHandle, discCurveHandle, tenor, asOf, dayCount, dayRule, interpolation);
        //    return baseHandle;
        //}

        //[ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurveRepresentation.Get", IsVolatile = true)]
        //public static object[,] LinearRate_FwdCurveRepresentation_Get(string baseHandle)
        //{
        //    return LinearRateFunctions.FwdCurveRepresentation_Get(baseHandle);
        //}

        #endregion

        #region LinearRateModel: functions related to making models and valuing linearRateInstruments

        [ExcelFunction(Description = "Construct LinearRateModel from curve objects.", Name = "mt.LinearRateModel.Make", IsVolatile = true)]
        public static string LinearRate_LinearRateModel_Make(string baseHandle, string fwdCurveCollectionHandle, string discCurveHandle, string interpolation)
        {
            InterpMethod interpMethod = StrToEnum.InterpolationConvert(interpolation);
            LinearRateFunctions.LinearRateModel_Make(baseHandle, fwdCurveCollectionHandle, discCurveHandle, interpMethod);
            return baseHandle;
        }

        [ExcelFunction(Description = "Value LinearRateInstrument object using LinearRateModel.", Name = "mt.LinearRateModel.Value", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_Value(string linearRateModelHandle, string linearRateInstrumentHandle)
        {
            return LinearRateFunctions.LinearRateModel_Value(linearRateModelHandle, linearRateInstrumentHandle);
        }

        // Swap functions
        [ExcelFunction(Description = "Calculate swap value.", Name = "mt.LinearRateModel.SwapValue", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_ValueSwap(string linearRateModelHandle, string swapHandle)
        {
            return LinearRateFunctions.LinearRateModel_SwapValue(linearRateModelHandle, swapHandle);
        }

        [ExcelFunction(Description = "Calculate par swap rate.", Name = "mt.LinearRateModel.SwapParRate", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_ParRate(string linearRateModelHandle, string swapHandle)
        {
            return LinearRateFunctions.LinearRateModel_SwapParRate(linearRateModelHandle, swapHandle);
        }

        // Ois Swap Valuation
        [ExcelFunction(Description = "Calculate OIS value.", Name = "mt.LinearRateModel.OisSwapValue", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_OisSwapValue(string linearRateModelHandle, string swapHandle)
        {
            return LinearRateFunctions.LinearRateModel_OisSwapNpv(linearRateModelHandle, swapHandle);
        }

        [ExcelFunction(Description = "Calculate OIS par rate using the simple formula.", Name = "mt.LinearRateModel.OisRate", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_OisRate(string linearRateModelHandle, string swapHandle)
        {
            return LinearRateFunctions.LinearRateModel_OisRate(linearRateModelHandle, swapHandle);
        }

        [ExcelFunction(Description = "Calculate OIS par rate using the complex formula (daily compounding).", Name = "mt.LinearRateModel.OisRateComplex", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_OisRateComplex(string linearRateModelHandle, string swapHandle)
        {
            return LinearRateFunctions.LinearRateModel_OisRateComplex(linearRateModelHandle, swapHandle);
        }

        // BasisSwap Valuation
        [ExcelFunction(Description = "Value basis tenor swap.", Name = "mt.LinearRateModel.BasisSwapValue", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_ValueBasisSwap(string linearRateModelHandle, string swapHandle)
        {
            return LinearRateFunctions.LinearRateModel_BasisSwapValue(linearRateModelHandle, swapHandle);
        }

        [ExcelFunction(Description = "Calculate par basis spread.", Name = "mt.LinearRateModel.ParBasisSpread", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_ParBasisSpread(string linearRateModelHandle, string basisSwapHandle)
        {
            return LinearRateFunctions.LinearRateModel_BasisParSpread(linearRateModelHandle, basisSwapHandle);
        }

        // SwapLeg valuation
        [ExcelFunction(Description = "Value floating leg.", Name = "mt.LinearRateModel.FloatLegValue", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_FloatLegValue(string linearRateModelHandle, string floatLegHandle)
        {
            return LinearRateFunctions.LinearRateModel_FloatLegValue(linearRateModelHandle, floatLegHandle);
        }

        [ExcelFunction(Description = "Value fixed leg.", Name = "mt.LinearRateModel.FixedLegValue", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_FixedLegValue(string linearRateModelHandle, string fixedLegHandle)
        {
            return LinearRateFunctions.LinearRateModel_FixedLegValue(linearRateModelHandle, fixedLegHandle);
        }

        #endregion

        #region LinearRateInstrument: functions related to constructing linearRateInstrument objects from Excel

        [ExcelFunction(Description = "Construct fixed-for-floating interest rate swap.", Name = "mt.LinearRateInstrument.IrSwap.Make", IsVolatile = true)]
        public static string LinearRate_PlainVanillaSwap_Make(string baseHandle, string fixedLegHandle, string floatLegHandle, int tradeSign)
        {
            LinearRateFunctions.PlainVanillaSwap_Make(baseHandle, fixedLegHandle, floatLegHandle, tradeSign);
            return baseHandle;
        }

        [ExcelFunction(Description = "Construct overnight-indexed swap.", Name = "mt.LinearRateInstrument.OisSwap.Make", IsVolatile = true)]
        public static string LinearRate_OisSwap_Make(string baseHandle, DateTime asOf, string startTenor, string endTenor, string settlementLag, string dayCountFixedStr,
                string dayCountFloatStr, string dayRuleStr, double notional, double fixedRate, int tradeSign)
        {
            DayCount dayCountFloat = StrToEnum.DayCountConvert(dayCountFloatStr);
            DayCount dayCountFixed = StrToEnum.DayCountConvert(dayCountFixedStr);
            DayRule dayRule = StrToEnum.DayRuleConvert(dayRuleStr);

            LinearRateFunctions.OisSwap_Make(baseHandle, asOf, startTenor, endTenor, settlementLag, dayCountFixed, dayCountFloat, dayRule, notional, fixedRate, tradeSign);
            return baseHandle;
        }

        [ExcelFunction(Description = "Construct tenor basis swap.", Name = "mt.LinearRateInstrument.BasisSwap.Make", IsVolatile = true)]
        public static string LinearRate_BasisSwap_Make(string baseHandle, string floatLegSpreadHandle, string floatLegNoSpreadHandle, int tradeSign)
        {
            LinearRateFunctions.BasisSwap_Make(baseHandle, floatLegNoSpreadHandle, floatLegSpreadHandle, tradeSign);
            return baseHandle;
        }

        [ExcelFunction(Description = "Construct floating leg.", Name = "mt.LinearRateInstrument.FloatLeg.Make", IsVolatile = true)]
        public static string LinearRate_FloatLeg_Make(string baseHandle, DateTime asOf, DateTime startDate, DateTime endDate,
                        string frequency, string dayCount, string dayRule, double notional, double spread)
        {
            CurveTenor tenorEnum = StrToEnum.CurveTenorConvert(frequency);
            DayCount dayCountEnum = StrToEnum.DayCountConvert(dayCount);
            DayRule dayRuleEnum = StrToEnum.DayRuleConvert(dayRule);

            LinearRateFunctions.FloatLeg_Make(baseHandle, asOf, startDate, endDate, tenorEnum, dayCountEnum, dayRuleEnum, notional, spread);
            return baseHandle;
        }

        [ExcelFunction(Description = "Construct fixed leg.", Name = "mt.LinearRateInstrument.FixedLeg.Make", IsVolatile = true)]
        public static string LinearRate_FixedLeg_Make(string baseHandle, DateTime asOf, DateTime startDate, DateTime endDate, double fixedRate,
                        string frequency, string dayCount, string dayRule, double notional)
        {
            CurveTenor tenorEnum = StrToEnum.CurveTenorConvert(frequency);
            DayCount dayCountEnum = StrToEnum.DayCountConvert(dayCount);
            DayRule dayRuleEnum = StrToEnum.DayRuleConvert(dayRule);

            LinearRateFunctions.FixedLeg_Make(baseHandle, asOf, startDate, endDate, fixedRate, tenorEnum, dayCountEnum, dayRuleEnum, notional);
            return baseHandle;
        }

        [ExcelFunction(Description = "Construct tenor basis swap from two fixed-for-floating interest rate swaps.", Name = "mt.LinearRateInstrument.BasisSwap.MakeFromIrSwaps")]
        public static string LinearRate_BasisSwap_MakeFromIrSwaps(string baseHandle, string swapSpread, string swapNoSpread, int tradeSign)
        {
            LinearRateFunctions.BasisSwap_MakeFromIrs(baseHandle, swapSpread, swapNoSpread, tradeSign);
            return baseHandle;
        }

        #endregion

        #region Calibration: functions related to calibrating curves

        [ExcelFunction(Description = "Define a curve calibration problem from instrument handles and quotes.", Name = "mt.Calibration.CurveCalibrationProblem.Make")]
        public static string Calibration_CurveCalibrationProblem_Make(string baseHandle, string instrumentFactoryHandle, object[] quoteIdentifiers, object[] quoteValues)
        {
            CalibrationFunctions.CurveCalibrationProblem_Make(baseHandle, instrumentFactoryHandle, quoteIdentifiers.Cast<string>().ToArray(), quoteValues.Cast<double>().ToArray());
            return baseHandle;
        }

        [ExcelFunction(Description = "Construct a calibration settings object.", Name = "mt.Calibration.CalibrationSettings.Make")]
        public static string Calibration_CalibrationSettings_Make(string baseHandle, double precision, double scaling, double diffStep, string interpolation, int maxIterations, double startingValues, int bfgs_m, bool useAd, bool inheritDiscSize, double stepSizeOfInheritance, object[] calibrationOrder = null)
        {
            InterpMethod interp = StrToEnum.InterpolationConvert(interpolation);

            if (calibrationOrder[0] is ExcelMissing)
            {
                calibrationOrder = null;
                CalibrationFunctions.CalibrationSpec_Make(baseHandle, precision, scaling, diffStep, interp, maxIterations, startingValues, bfgs_m, useAd, inheritDiscSize, stepSizeOfInheritance);
            }
            else
            {
                // Need to do this, since object[] cannot be cast to int[]...
                int[] intCalibrationOrder = new int[calibrationOrder.Length];
                for (int i = 0; i < calibrationOrder.Length; i++)
                    intCalibrationOrder[i] = Convert.ToInt32(calibrationOrder[i]);

                CalibrationFunctions.CalibrationSpec_Make(baseHandle, precision, scaling, diffStep, interp, maxIterations, startingValues, bfgs_m, useAd, inheritDiscSize, stepSizeOfInheritance, intCalibrationOrder);

            }

            return baseHandle;
        }

        [ExcelFunction(Description = "Construct discounting curve from calibration problem.", Name = "mt.Calibration.MakeDiscCurveFromProblem")]
        public static string Calibration_DiscCurve_MakeFromCalibrationProblem(string baseHandle, string curveCalibProblemHandle, string calibSpecHandle, bool useAd)
        {
            if (ExcelDnaUtil.IsInFunctionWizard())
                return "No calulation in wizard.";

            CalibrationFunctions.DiscCurve_MakeFromCalibrationProblem(baseHandle, curveCalibProblemHandle, calibSpecHandle, useAd);
            return baseHandle;
        }

        [ExcelFunction(Description = "Define forward curve calibration problem from disc curve and calibration problems.", Name = "mt.Calibration.FwdCurveCalibrationProblem.Make")]
        public static string Calibration_FwdCurveCalibrationProblem_Make(string baseHandle, string discCurveHandle, object[] curveCalibHandles, object[] fwdCurveTenors, string calibSpecHandle)
        {

            string[] problemNamesString = curveCalibHandles.Cast<string>().ToArray();
            string[] tenorsString = fwdCurveTenors.Cast<string>().ToArray();
            CurveTenor[] tenorsActual = new CurveTenor[tenorsString.Length];

            for (int i = 0; i < tenorsString.Length; i++)
                tenorsActual[i] = StrToEnum.CurveTenorConvert(tenorsString[i]);

            CalibrationFunctions.FwdCurveCalibrationProblem_Make(baseHandle, discCurveHandle, problemNamesString, tenorsActual, calibSpecHandle);
            return baseHandle;
        }

        [ExcelFunction(Description = "Construct forward curve collection from forward curve calibration problem.", Name = "mt.Calibration.MakeFwdCurveCollectionFromProblem")]
        public static string Calibration_FwdCurveCollection_MakeFromCalibrationProblem(string baseHandle, string fwdCurveConstructorHandle, bool useAd)
        {
            if (ExcelDnaUtil.IsInFunctionWizard())
                return "No calulation in wizard.";

            CalibrationFunctions.FwdCurveCollection_MakeFromCalibrationProblem(baseHandle, fwdCurveConstructorHandle, useAd);
            return baseHandle;

        }

        #endregion

        #region InstrumentFactory: related to creating and updating instrumentFactories

        [ExcelFunction(Description = "Given a LinearRateModel, update all instruments in an InstrumentFactory to par.", Name = "mt.InstrumentFactory.UpdateAllInstrumentsToPar", IsVolatile = true)]
        public static string Factory_UpdateAllInstrumentsToPar(string baseHandle, string modelHandle)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            InstrumentFactoryFunctions.InstrumentFactory_UpdateAllInstrumentsToPar(baseHandle, modelHandle);
            sw.Stop();
            return "Updated all instruments to par in factory " + baseHandle + " using model " + modelHandle + ". CalcTime: " + sw.ElapsedMilliseconds;
        }

        [ExcelFunction(Description = "Construct InstrumentFactory.", Name = "mt.InstrumentFactory.Make", IsVolatile = true)]
        public static string Factory_InstrumentFactory_Make(string baseHandle, DateTime asOf)
        {
            InstrumentFactoryFunctions.InstrumentFactory_Make(baseHandle, asOf);
            return baseHandle;
        }

        [ExcelFunction(Description = "Transfer all objects in factory to the ObjectMap", Name = "mt.InstrumentFactory.AddInstrumentsToProductMap")]
        public static string Factory_InstrumentFactory_Make(string instrumentFactoryHandle)
        {
            InstrumentFactoryFunctions.InstrumentFactory_StoreInstrumentsInMap(instrumentFactoryHandle);
            return "Instruments added to ObjectMap.";
        }

        [ExcelFunction(Description = "Add plain vanilla swaps to InstrumentFactory from strings.", Name = "mt.InstrumentFactory.AddSwaps", IsVolatile = true)]
        public static string Factory_InstrumentFactory_AddSwaps(string instrumentFactoryHandle, object[] swapStrings)
        {
            var swapStringsString = swapStrings.Cast<string>().ToArray();
            InstrumentFactoryFunctions.InstrumentFactory_AddSwaps(instrumentFactoryHandle, swapStringsString);
            return "Added " + swapStrings.Length + " swap-instruments to " + instrumentFactoryHandle;
        }

        [ExcelFunction(Description = "Add tenor basis swaps to InstrumentFactory from strings.", Name = "mt.InstrumentFactory.AddBasisSwaps", IsVolatile = true)]
        public static string Factory_InstrumentFactory_AddBasisSwaps(string instrumentFactoryHandle, object[] swapStrings)
        {
            var swapStringsString = swapStrings.Cast<string>().ToArray();
            InstrumentFactoryFunctions.InstrumentFactory_AddBasisSwaps(instrumentFactoryHandle, swapStringsString);
            return "Added " + swapStrings.Length + " MmBasisSwap-instruments to " + instrumentFactoryHandle;
        }

        [ExcelFunction(Description = "Add forward rate agreements to InstrumentFactory from strings.", Name = "mt.InstrumentFactory.AddFras", IsVolatile = true)]
        public static string Factory_InstrumentFactory_AddFras(string instrumentFactoryHandle, object[] fraStrings)
        {
            var fraStringsString = fraStrings.Cast<string>().ToArray();
            InstrumentFactoryFunctions.InstrumentFactory_AddFras(instrumentFactoryHandle, fraStringsString);
            return "Added " + fraStrings.Length + " fra-instruments to " + instrumentFactoryHandle;
        }

        [ExcelFunction(Description = "Add futures to InstrumentFactory from strings.", Name = "mt.InstrumentFactory.AddFutures", IsVolatile = true)]
        public static string Factory_InstrumentFactory_AddFutures(string instrumentFactoryHandle, object[] futureStrings)
        {
            var futureStringsString = futureStrings.Cast<string>().ToArray();
            InstrumentFactoryFunctions.InstrumentFactory_AddFutures(instrumentFactoryHandle, futureStringsString);
            return "Added " + futureStrings.Length + " futures-instruments to " + instrumentFactoryHandle;
        }

        [ExcelFunction(Description = "Add forward starting swaps to factory from strings (these has a different definition than plain vanilla swaps).", Name = "mt.InstrumentFactory.AddFwdStartingSwaps", IsVolatile = true)]
        public static string Factory_InstrumentFactory_AddFwdStartingSwaps(string instrumentFactoryHandle, object[] swapStrings)
        {
            var swapStringsString = swapStrings.Cast<string>().ToArray();
            InstrumentFactoryFunctions.InstrumentFactory_AddFwdStartingSwaps(instrumentFactoryHandle, swapStringsString);
            return "Added " + swapStrings.Length + " swap-instruments to " + instrumentFactoryHandle;
        }

        [ExcelFunction(Description = "Value instrument from factory.", Name = "mt.InstrumentFactory.ValueInstrument", IsVolatile = true)]
        public static double Factory_InstrumentFactory_ValueInstrument(string instrumentHandle, string instrumentFactoryHandle, string linearRateModelHandle)
        {
            return InstrumentFactoryFunctions.ValueInstrument(instrumentHandle, instrumentFactoryHandle, linearRateModelHandle);
        }

        [ExcelFunction(Description = "Get information on instrument from factory.", Name = "mt.InstrumentFactory.GetInstrumentInfo", IsVolatile = true)]
        public static object[,] Factory_InstrumentFactory_GetInstrumentInfo(string instrumentHandle, string instrumentFactoryHandle)
        {
            return InstrumentFactoryFunctions.InstrumentFactory_GetInstrumentInfo(instrumentFactoryHandle, instrumentHandle);
        }

        #endregion

        #region RiskEngine: related to computing risk 
        [ExcelFunction(Description = "Calculate zero-coupon risk on a LinearRateInstrument using automatic differentiation.", Name = "mt.RiskEngine.ZcbRiskAd", IsVolatile = true)]
        public static object[,] RiskAdTest(string modelHandle, string productHandle)
        {
            return ADFunctions.ZcbRiskAD(modelHandle, productHandle);
        }

        [ExcelFunction(Description = "Create CalibrationInstrument set.", Name = "mt.RiskEngine.CalibrationInstrumentSet.Make", IsVolatile = true)]
        public static string CalibrationInstrumentSet_Make(string baseHandle, object[] linearRateProductHandles, string curveTenor)
        {
            RiskEngineFunctions.CalibrationInstrumentSet_Make(baseHandle, linearRateProductHandles.Cast<string>().ToArray(), curveTenor);
            return baseHandle;
        }

        [ExcelFunction(Description = "Create portfolio from LinearRateInstruments.", Name = "mt.RiskEngine.Portfolio.Make", IsVolatile = true)]
        public static string Portfolio_Make(string baseHandle, object[] linearRateProductHandles)
        {
            RiskEngineFunctions.Portfolio_Make(baseHandle, linearRateProductHandles.Cast<string>().ToArray());
            return baseHandle;
        }

        [ExcelFunction(Description = "Construct RiskJacobian from calibration sets.", Name = "mt.RiskEngine.RiskJacobian.Make", IsVolatile = true)]
        public static string RiskEngine_RiskJacobian_Make(string baseHandle, string linearRateModelHandle, DateTime asOf, object[] calibSetHandles, object[] curveTenors, bool useAd)
        {
            RiskEngineFunctions.RiskJacobian_Make(baseHandle, linearRateModelHandle, asOf, calibSetHandles.Cast<string>().ToArray(), curveTenors.Cast<string>().ToArray(), useAd);
            return baseHandle;
        }

        [ExcelFunction(Description = "Construct RiskEngine.", Name = "mt.RiskEngine.Make", IsVolatile = true)]
        public static string RiskEngine_Make(string baseHandle, string portfolioHandle, string jacobianHandle, bool useAd)
        {
            RiskEngineFunctions.RiskEngineNew_Make(baseHandle, portfolioHandle, jacobianHandle, useAd);
            return baseHandle;
        }

        [ExcelFunction(Description = "Store outright risk output from RiskEngine.", Name = "mt.RiskEngine.OutrightRiskOutput.Store", IsVolatile = true)]
        public static string RiskEngineNew_StoreOutrightRisk(string baseHandle, string riskEngineHandle)
        {
            RiskEngineFunctions.RiskEngineNew_StoreOutrightRisk(baseHandle, riskEngineHandle);
            return baseHandle;
        }

        [ExcelFunction(Description = "Store outright risk output from RiskContainer.", Name = "mt.RiskEngine.OutrightRiskOutput.StoreFromContainer", IsVolatile = true)]
        public static string RiskOutput_StoreOutrightRiskFromContainer(string baseHandle, string riskOutputContainerHandle, string tenor)
        {
            RiskEngineFunctions.OutrightRiskOutput_StoreFromRiskOutContainer(baseHandle, riskOutputContainerHandle, tenor);
            return baseHandle;
        }

        [ExcelFunction(Description = "Print OutrightRiskOutput.", Name = "mt.RiskEngine.OutrightRiskOutput.Get", IsVolatile = true)]
        public static object[,] OutrightRiskOUtput_Get(string baseHandle)
        {
            return RiskEngineFunctions.OutrightRiskOutput_Get(baseHandle);
        }

        [ExcelFunction(Description = "Store zero-coupon risk from risk output.", Name = "mt.RiskEngine.ZcbRiskOutput.Store", IsVolatile = true)]
        public static string RiskEngineNew_StoreZcbRisk(string baseHandle, string riskEngineHandle)
        {
            RiskEngineFunctions.RiskEngineNew_StoreZcbRisk(baseHandle, riskEngineHandle);
            return baseHandle;
        }

        [ExcelFunction(Description = "Store zero-coupon risk from risk output container.", Name = "mt.RiskEngine.ZcbRiskOutput.StoreFromContainer", IsVolatile = true)]
        public static string RiskOutput_StoreFromRiskOutputContainer(string baseHandle, string riskOutputContainerHandle, string tenor)
        {
            RiskEngineFunctions.RiskOutput_StoreFromRiskOutputContainer(baseHandle, riskOutputContainerHandle, tenor);
            return baseHandle;
        }

        [ExcelFunction(Description = "Print zero-coupon risk.", Name = "mt.RiskEngine.ZcbRiskOutput.Get", IsVolatile=true)]
        public static object[,] RiskOutput_Get(string baseHandle)
        {
            return RiskEngineFunctions.ZcbRiskOutput_Get(baseHandle);
        }

        [ExcelFunction(Description = "Re-calculate risk multiple times for AD and bump-and-run comparison.", Name = "mt.RiskEngine.ZcbRisk.CalculateMultipleTimes")]
        public static string RiskEngineNew_CalcZcbRiskMultipleTimes(string baseHandle, string riskEngineHandle, int times)
        {
            RiskEngineFunctions.RiskEngineNew_CalcZcbRisk(baseHandle, riskEngineHandle, times);
            return baseHandle;
        }

        #endregion
    }
}