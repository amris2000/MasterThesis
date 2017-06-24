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
    public class ExcelFunctions
    {
        // ------- CURVE FUNCTIONS

        public static DateTime[] ConvertDoublesToDateTimes(double[] doubles)
        {
            DateTime[] dates = new DateTime[doubles.Length];
            for (int i = 0; i < doubles.Length; i++)
                dates[i] = DateTime.FromOADate(doubles[i]);

            return dates;
        }

        [ExcelFunction(Description = "My First function in Excel", Name = "mt.LinearRate.DiscCurve.Make", IsVolatile = true)]
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

        [ExcelFunction(Description = "My First function in Excel", Name = "mt.LinearRate.FwdCurve.Make", IsVolatile = true)]
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

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurveCollection.Make", IsVolatile = true)]
        public static string LinearRate_FwdCurveCollection_Make(string baseHandle, object[] fwdCurveHandles, object[] tenorNames)
        {
            CurveTenor[] tenorEnums = new CurveTenor[tenorNames.Length];
            var fwdCurveNamesString = fwdCurveHandles.Cast<string>().ToArray();

            for (int i = 0; i < tenorEnums.Length; i++)
                tenorEnums[i] = StrToEnum.CurveTenorConvert((string)tenorNames[i]);

            string output = LinearRateFunctions.FwdCurveCollection_Make(baseHandle, fwdCurveNamesString, tenorEnums);

            return output;
        }

        // .. Get functions



        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.DiscCurve.Get", IsVolatile = true)]
        public static object[,] LinearRate_DiscCurve_Get(string discCurveHandle)
        {
            return LinearRateFunctions.DiscCurve_Get(discCurveHandle);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.BumpCurveAndStore", IsVolatile = true)]
        public static string LinearRate_BumpCurveAndStore(string baseHandle, string originalCurveHandle, int curvePoint, double bumpSize)
        {
            LinearRateFunctions.Curve_BumpCurveAndStore(baseHandle, originalCurveHandle, curvePoint, bumpSize);
            return baseHandle;
        }       

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurve.StoreFromCollection")]
        public static string LinearRate_FwdCurve_StoreFromCollection(string baseHandle, string FwdCurveCollectionHandle, string tenor)
        {
            CurveTenor tenorActual = StrToEnum.CurveTenorConvert(tenor);
            LinearRateFunctions.FwdCurve_StoreFromCollection(baseHandle, FwdCurveCollectionHandle, tenorActual);
            return baseHandle;
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.Curve.GetFwdRate", IsVolatile = true)]
        public static double LinearRate_Curve_GetFwdRate(string curveHandle, DateTime asOf, DateTime startDate, string tenorStr, string dayCountStr, string dayRuleStr, string interpolationStr)
        {
            CurveTenor tenor = StrToEnum.CurveTenorFromSimpleTenor(tenorStr);
            InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationStr);
            DayCount dayCount = StrToEnum.DayCountConvert(dayCountStr);
            DayRule dayRule = StrToEnum.DayRuleConvert(dayRuleStr);

            return LinearRateFunctions.Curve_GetFwdRate(curveHandle, asOf, startDate, tenor, dayCount, dayRule, interpolation);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurve.GetFromCollection", IsVolatile = true)]
        public static object[,] LinearRate_FwdCurve_GetFromCollection(string fwdCurveCollectionHandle, string fwdCurveTenor)
        {
            CurveTenor tenorEnum = StrToEnum.CurveTenorConvert(fwdCurveTenor);
            return LinearRateFunctions.FwdCurve_GetFromCollection(fwdCurveCollectionHandle, tenorEnum);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.DiscCurve.GetValue", IsVolatile = true)]
        public static double LinearRate_DiscCurve_GetValue(string baseHandle, double date, string interpolationMethod)
        {
            DateTime dateTime = DateTime.FromOADate(date);
            InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationMethod);
            return LinearRateFunctions.DiscCurve_GetValue(baseHandle, dateTime, interpolation);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.Curve.GetValue", IsVolatile = true)]
        public static double LinearRate_Curve_GetValue(string baseHandle, double date, string interpolationMethod)
        {
            DateTime dateTime = DateTime.FromOADate(date);
            InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationMethod);
            return LinearRateFunctions.Curve_GetValue(baseHandle, dateTime, interpolation);
        }


        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.Curve.GetDiscFactor", IsVolatile = true)]
        public static double LinearRate_Curve_GetValue(string baseHandle, DateTime asOf, DateTime date, string dayCountStr, string interpolationStr)
        {
            InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationStr);
            DayCount dayCount = StrToEnum.DayCountConvert(dayCountStr);
            return LinearRateFunctions.Curve_GetDiscFactor(baseHandle, asOf, date, dayCount, interpolation);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurve.GetValue", IsVolatile = true)]
        public static double LinearRate_FwdCurve_GetValue(string baseHandle, double date, string interpolationMethod)
        {
            DateTime dateTime = DateTime.FromOADate(date);
            InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationMethod);
            return LinearRateFunctions.FwdCurve_GetValue(baseHandle, dateTime, interpolation);
        }

        // .. FWD CURVE REPRESENTATIONS

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurveRepresentation.MakeFromFwdCurve", IsVolatile = true)]
        public static string LinearRate_FwdCurveRepresentation_MakeFromFwdCurve(string baseHandle, string fwdCurveHandle, string curveTenorStr, DateTime asOf
            , string dayCountStr, string dayRuleStr, string interpolationStr)
        {
            CurveTenor tenor = StrToEnum.CurveTenorFromSimpleTenor(curveTenorStr);
            InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationStr);
            DayCount dayCount = StrToEnum.DayCountConvert(dayCountStr);
            DayRule dayRule = StrToEnum.DayRuleConvert(dayRuleStr);

            LinearRateFunctions.FwdCurveRepresentation_MakeFromFwdCurve(baseHandle, fwdCurveHandle, tenor, asOf, dayCount, dayRule, interpolation);
            return baseHandle;
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurveRepresentation.MakeFromDiscCurve", IsVolatile = true)]
        public static string LinearRate_FwdCurveRepresentation_MakeFromDiscCurve(string baseHandle, string discCurveHandle, string curveTenorStr, DateTime asOf
            , string dayCountStr, string dayRuleStr, string interpolationStr)
        {
            CurveTenor tenor = StrToEnum.CurveTenorConvert(curveTenorStr);
            InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationStr);
            DayCount dayCount = StrToEnum.DayCountConvert(dayCountStr);
            DayRule dayRule = StrToEnum.DayRuleConvert(dayRuleStr);

            LinearRateFunctions.FwdCurveRepresentation_MakeFromDiscCurve(baseHandle, discCurveHandle, tenor, asOf, dayCount, dayRule, interpolation);
            return baseHandle;
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurveRepresentation.Get", IsVolatile = true)]
        public static object[,] LinearRate_FwdCurveRepresentation_Get(string baseHandle)
        {
            return LinearRateFunctions.FwdCurveRepresentation_Get(baseHandle);
        }

        // ------- LINEAR RATE FUNTIONS

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.Make", IsVolatile = true)]
        public static string LinearRate_LinearRateModel_Make(string baseHandle, string fwdCurveCollectionHandle, string discCurveHandle, string interpolation)
        {
            InterpMethod interpMethod = StrToEnum.InterpolationConvert(interpolation);
            LinearRateFunctions.LinearRateModel_Make(baseHandle, fwdCurveCollectionHandle, discCurveHandle, interpMethod);
            return baseHandle;
        }

        [ExcelFunction(Description = "Some description", Name = "mt.LinearRateModel.Value", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_Value(string linearRateModelHandle, string productHandle)
        {
            return LinearRateFunctions.LinearRateModel_Value(linearRateModelHandle, productHandle);
        }

        // Swap functions
        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.SwapValue", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_ValueSwap(string linearRateModelHandle, string swapHandle)
        {
            return LinearRateFunctions.LinearRateModel_SwapValue(linearRateModelHandle, swapHandle);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.SwapParRate", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_ParRate(string linearRateModelHandle, string swapHandle)
        {
            return LinearRateFunctions.LinearRateModel_SwapParRate(linearRateModelHandle, swapHandle);
        }

        // Ois Swap Valuation
        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.OisSwapValue", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_OisSwapValue(string linearRateModelHandle, string swapHandle)
        {
            return LinearRateFunctions.LinearRateModel_OisSwapNpv(linearRateModelHandle, swapHandle);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.OisRate", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_OisRate(string linearRateModelHandle, string swapHandle)
        {
            return LinearRateFunctions.LinearRateModel_OisRate(linearRateModelHandle, swapHandle);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.OisRateComplex", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_OisRateComplex(string linearRateModelHandle, string swapHandle)
        {
            return LinearRateFunctions.LinearRateModel_OisRateComplex(linearRateModelHandle, swapHandle);
        }

        // BasisSwap Valuation
        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.BasisSwapValue", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_ValueBasisSwap(string linearRateModelHandle, string swapHandle)
        {
            return LinearRateFunctions.LinearRateModel_BasisSwapValue(linearRateModelHandle, swapHandle);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.ParBasisSpread", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_ParBasisSpread(string linearRateModelHandle, string basisSwapHandle)
        {
            return LinearRateFunctions.LinearRateModel_BasisParSpread(linearRateModelHandle, basisSwapHandle);
        }

        // SwapLeg valuation
        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.FloatLegValue", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_FloatLegValue(string linearRateModelHandle, string floatLegHandle)
        {
            return LinearRateFunctions.LinearRateModel_FloatLegValue(linearRateModelHandle, floatLegHandle);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.FixedLegValue", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_FixedLegValue(string linearRateModelHandle, string fixedLegHandle)
        {
            return LinearRateFunctions.LinearRateModel_FixedLegValue(linearRateModelHandle, fixedLegHandle);
        }

        // ------- SWAP FUNCTIONS
        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.IrSwap.Make", IsVolatile = true)]
        public static string LinearRate_PlainVanillaSwap_Make(string baseHandle, string fixedLegHandle, string floatLegHandle, int tradeSign)
        {
            LinearRateFunctions.PlainVanillaSwap_Make(baseHandle, fixedLegHandle, floatLegHandle, tradeSign);
            return baseHandle;
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.OisSwap.Make", IsVolatile = true)]
        public static string LinearRate_OisSwap_Make(string baseHandle, DateTime asOf, string startTenor, string endTenor, string settlementLag, string dayCountFixedStr,
                string dayCountFloatStr, string dayRuleStr, double notional, double fixedRate, int tradeSign)
        {
            DayCount dayCountFloat = StrToEnum.DayCountConvert(dayCountFloatStr);
            DayCount dayCountFixed = StrToEnum.DayCountConvert(dayCountFixedStr);
            DayRule dayRule = StrToEnum.DayRuleConvert(dayRuleStr);

            LinearRateFunctions.OisSwap_Make(baseHandle, asOf, startTenor, endTenor, settlementLag, dayCountFixed, dayCountFloat, dayRule, notional, fixedRate, tradeSign);
            return baseHandle;
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.BasisSwap.Make", IsVolatile = true)]
        public static string LinearRate_BasisSwap_Make(string baseHandle, string floatLegSpreadHandle, string floatLegNoSpreadHandle, int tradeSign)
        {
            LinearRateFunctions.BasisSwap_Make(baseHandle, floatLegNoSpreadHandle, floatLegSpreadHandle, tradeSign);
            return baseHandle;
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FloatLeg.Make", IsVolatile = true)]
        public static string LinearRate_FloatLeg_Make(string baseHandle, DateTime asOf, DateTime startDate, DateTime endDate,
                        string frequency, string dayCount, string dayRule, double notional, double spread)
        {
            CurveTenor tenorEnum = StrToEnum.CurveTenorConvert(frequency);
            DayCount dayCountEnum = StrToEnum.DayCountConvert(dayCount);
            DayRule dayRuleEnum = StrToEnum.DayRuleConvert(dayRule);

            LinearRateFunctions.FloatLeg_Make(baseHandle, asOf, startDate, endDate, tenorEnum, dayCountEnum, dayRuleEnum, notional, spread);
            return baseHandle;
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FixedLeg.Make", IsVolatile = true)]
        public static string LinearRate_FixedLeg_Make(string baseHandle, DateTime asOf, DateTime startDate, DateTime endDate, double fixedRate,
                        string frequency, string dayCount, string dayRule, double notional)
        {
            CurveTenor tenorEnum = StrToEnum.CurveTenorConvert(frequency);
            DayCount dayCountEnum = StrToEnum.DayCountConvert(dayCount);
            DayRule dayRuleEnum = StrToEnum.DayRuleConvert(dayRule);

            LinearRateFunctions.FixedLeg_Make(baseHandle, asOf, startDate, endDate, fixedRate, tenorEnum, dayCountEnum, dayRuleEnum, notional);
            return baseHandle;
        }

        // ------ CALIBRATION RELATED
        [ExcelFunction(Description = "Some description.", Name = "mt.CurveCalibrationProblem.Make")]
        public static string Calibration_CurveCalibrationProblem_Make(string baseHandle, string instrumentFactoryHandle, object[] quoteIdentifiers, object[] quoteValues)
        {
            CalibrationFunctions.CurveCalibrationProblem_Make(baseHandle, instrumentFactoryHandle, quoteIdentifiers.Cast<string>().ToArray(), quoteValues.Cast<double>().ToArray());
            return baseHandle;
        }

        [ExcelFunction(Description = "Make CalibrationSpec", Name = "mt.Calibration.CalibrationSettings.Make")]
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

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.DiscCurve.MakeFromCalibrationProblem")]
        public static string Calibration_DiscCurve_MakeFromCalibrationProblem(string baseHandle, string curveCalibProblemHandle, string calibSpecHandle, bool useAd)
        {
            if (ExcelDnaUtil.IsInFunctionWizard())
                return "No calulation in wizard.";

            CalibrationFunctions.DiscCurve_MakeFromCalibrationProblem(baseHandle, curveCalibProblemHandle, calibSpecHandle, useAd);
            return baseHandle;
        }

        [ExcelFunction(Description = "some description.", Name = "mt.FwdCurveCalibrationProblem.Make")]
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

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurveCollection.MakeFromCalibrationProblem")]
        public static string Calibration_FwdCurveCollection_MakeFromCalibrationProblem(string baseHandle, string fwdCurveConstructorHandle, bool useAd)
        {
            if (ExcelDnaUtil.IsInFunctionWizard())
                return "No calulation in wizard.";

            CalibrationFunctions.FwdCurveCollection_MakeFromCalibrationProblem(baseHandle, fwdCurveConstructorHandle, useAd);
            return baseHandle;

        }

        // ------ INSTRUMENT FACTORY RELATED

        [ExcelFunction(Description = "some description", Name = "mt.InstrumentFactory.UpdateAllInstrumentsToPar", IsVolatile = true)]
        public static string Factory_UpdateAllInstrumentsToPar(string baseHandle, string modelHandle)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            InstrumentFactoryFunctions.InstrumentFactory_UpdateAllInstrumentsToPar(baseHandle, modelHandle);
            sw.Stop();
            return "Updated all instruments to par in factory " + baseHandle + " using model " + modelHandle + ". CalcTime: " + sw.ElapsedMilliseconds;
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.InstrumentFactory.Make", IsVolatile = true)]
        public static string Factory_InstrumentFactory_Make(string baseHandle, DateTime asOf)
        {
            InstrumentFactoryFunctions.InstrumentFactory_Make(baseHandle, asOf);
            return baseHandle;
        }

        [ExcelFunction(Description = "Some description", Name = "mt.InstrumentFactory.AddInstrumentsToProductMap")]
        public static string Factory_InstrumentFactory_Make(string instrumentFactoryHandle)
        {
            InstrumentFactoryFunctions.InstrumentFactory_StoreInstrumentsInMap(instrumentFactoryHandle);
            return "Instruments added to ObjectMap.";
        }

        [ExcelFunction(Description = "some description.", Name = "mt.InstrumentFactory.AddSwaps", IsVolatile = true)]
        public static string Factory_InstrumentFactory_AddSwaps(string instrumentFactoryHandle, object[] swapStrings)
        {
            var swapStringsString = swapStrings.Cast<string>().ToArray();
            InstrumentFactoryFunctions.InstrumentFactory_AddSwaps(instrumentFactoryHandle, swapStringsString);
            return "Added " + swapStrings.Length + " swap-instruments to " + instrumentFactoryHandle;
        }

        [ExcelFunction(Description = "some description.", Name = "mt.InstrumentFactory.AddBasisSwaps", IsVolatile = true)]
        public static string Factory_InstrumentFactory_AddBasisSwaps(string instrumentFactoryHandle, object[] swapStrings)
        {
            var swapStringsString = swapStrings.Cast<string>().ToArray();
            InstrumentFactoryFunctions.InstrumentFactory_AddBasisSwaps(instrumentFactoryHandle, swapStringsString);
            return "Added " + swapStrings.Length + " MmBasisSwap-instruments to " + instrumentFactoryHandle;
        }

        [ExcelFunction(Description = "some description.", Name = "mt.InstrumentFactory.AddFras", IsVolatile = true)]
        public static string Factory_InstrumentFactory_AddFras(string instrumentFactoryHandle, object[] fraStrings)
        {
            var fraStringsString = fraStrings.Cast<string>().ToArray();
            InstrumentFactoryFunctions.InstrumentFactory_AddFras(instrumentFactoryHandle, fraStringsString);
            return "Added " + fraStrings.Length + " fra-instruments to " + instrumentFactoryHandle;
        }

        [ExcelFunction(Description = "some description.", Name = "mt.InstrumentFactory.AddFutures", IsVolatile = true)]
        public static string Factory_InstrumentFactory_AddFutures(string instrumentFactoryHandle, object[] futureStrings)
        {
            var futureStringsString = futureStrings.Cast<string>().ToArray();
            InstrumentFactoryFunctions.InstrumentFactory_AddFutures(instrumentFactoryHandle, futureStringsString);
            return "Added " + futureStrings.Length + " futures-instruments to " + instrumentFactoryHandle;
        }

        [ExcelFunction(Description = "some description.", Name = "mt.InstrumentFactory.AddFwdStartingSwaps", IsVolatile = true)]
        public static string Factory_InstrumentFactory_AddFwdStartingSwaps(string instrumentFactoryHandle, object[] swapStrings)
        {
            var swapStringsString = swapStrings.Cast<string>().ToArray();
            InstrumentFactoryFunctions.InstrumentFactory_AddFwdStartingSwaps(instrumentFactoryHandle, swapStringsString);
            return "Added " + swapStrings.Length + " swap-instruments to " + instrumentFactoryHandle;
        }

        //[ExcelFunction(Description = "Some description.", Name = "mt.InstrumentFactory.ValueSwap", IsVolatile = true)]
        //public static double Factory_InstrumentFactory_ValueSwap(string instrumentFactory, string model, string instrument)
        //{
        //    return InstrumentFactoryFunctions.InstrumentFactory_ValueSwap(instrumentFactory, model, instrument);
        //}

        //[ExcelFunction(Description = "Some description.", Name = "mt.InstrumentFactory.ValueBasisSwap", IsVolatile = true)]
        //public static double Factory_InstrumentFactory_ValueBasisSwap(string instrumentFactory, string model, string instrument)
        //{
        //    return InstrumentFactoryFunctions.InstrumentFactory_ValueBasisSwap(instrumentFactory, model, instrument);
        //}

        //[ExcelFunction(Description = "Some description.", Name = "mt.InstrumentFactory.ValueOisSwap", IsVolatile = true)]
        //public static double Factory_InstrumentFactory_ValueOisSwap(string instrumentFactory, string model, string instrument)
        //{
        //    return InstrumentFactoryFunctions.InstrumentFactory_ValueOisSwap(instrumentFactory, model, instrument);
        //}

        //[ExcelFunction(Description = "Some description.", Name = "mt.InstrumentFactory.ValueFra", IsVolatile = true)]
        //public static double Factory_InstrumentFactory_ValueFra(string instrumentFactory, string model, string instrument)
        //{
        //    return InstrumentFactoryFunctions.InstrumentFactory_ParFraRate(instrumentFactory, model, instrument);
        //}

        //[ExcelFunction(Description = "Some description.", Name = "mt.InstrumentFactory.ValueFuture", IsVolatile = true)]
        //public static double Factory_InstrumentFactory_ValueFuture(string instrumentFactory, string model, string instrument)
        //{
        //    return InstrumentFactoryFunctions.InstrumentFactory_ParFutureRate(instrumentFactory, model, instrument);
        //}

        //[ExcelFunction(Description = "Some description.", Name = "mt.InstrumentFactory.ValueFutureWithConvexity", IsVolatile = true)]
        //public static double Factory_InstrumentFactory_ValueFutureWithConvexity(string instrumentFactory, string model, string instrument, double convexity)
        //{
        //    Fra fra = ObjectMap.InstrumentFactories[instrumentFactory].Futures[instrument].FraSameSpec;
        //    LinearRateModel theModel = ObjectMap.LinearRateModels[model];
        //    return theModel.ParFraRate(fra) + convexity;
        //}

        [ExcelFunction(Description = "some description.", Name = "mt.InstrumentFactory.ValueInstrument", IsVolatile = true)]
        public static double Factory_InstrumentFactory_ValueInstrument(string instrumentHandle, string instrumentFactoryHandle, string linearRateModelHandle)
        {
            return InstrumentFactoryFunctions.ValueInstrument(instrumentHandle, instrumentFactoryHandle, linearRateModelHandle);
        }

        [ExcelFunction(Description = "some description", Name = "mt.InstrumentFactory.GetInstrumentInfo", IsVolatile = true)]
        public static object[,] Factory_InstrumentFactory_GetInstrumentInfo(string instrumentHandle, string instrumentFactoryHandle)
        {
            return InstrumentFactoryFunctions.InstrumentFactory_GetInstrumentInfo(instrumentFactoryHandle, instrumentHandle);
        }

        [ExcelFunction(Description = "some description", Name = "mt.ParseStringAndOutput", IsVolatile = true)]
        public static object[,] ParseStringAndOutput(string instrumentString)
        {
            object[] output = instrumentString.Split(',');
            object[,] realOutput = new object[output.Length, 1];
            for (int i = 0; i < output.Length; i++)
            {
                realOutput[i, 0] = output[i];
            }

            return realOutput;
        }

        // --- RELATED TO AD
        [ExcelFunction(Description = "Some description", Name = "mt.RiskEngine.ZcbRiskAd", IsVolatile = true)]
        public static object[,] RiskAdTest(string modelHandle, string productHandle)
        {
            return ADFunctions.ZcbRiskAD(modelHandle, productHandle);
        }


        // --- RELATED TO RISK ENGINE (NEW)
        [ExcelFunction(Description = " Some decription", Name = "mt.CalibrationInstrumentSet.Make", IsVolatile = true)]
        public static string CalibrationInstrumentSet_Make(string baseHandle, object[] linearRateProductHandles, string curveTenor)
        {
            RiskEngineFunctions.CalibrationInstrumentSet_Make(baseHandle, linearRateProductHandles.Cast<string>().ToArray(), curveTenor);
            return baseHandle;
        }

        [ExcelFunction(Description = "Some description", Name = "mt.Portfolio.Make", IsVolatile = true)]
        public static string Portfolio_Make(string baseHandle, object[] linearRateProductHandles)
        {
            RiskEngineFunctions.Portfolio_Make(baseHandle, linearRateProductHandles.Cast<string>().ToArray());
            return baseHandle;
        }

        [ExcelFunction(Description = "Some description", Name = "mt.RiskEngine.RiskJacobian.Make", IsVolatile = true)]
        public static string RiskEngine_RiskJacobian_Make(string baseHandle, string linearRateModelHandle, DateTime asOf, object[] calibSetHandles, object[] curveTenors, bool useAd)
        {
            RiskEngineFunctions.RiskJacobian_Make(baseHandle, linearRateModelHandle, asOf, calibSetHandles.Cast<string>().ToArray(), curveTenors.Cast<string>().ToArray(), useAd);
            return baseHandle;
        }

        [ExcelFunction(Description = "Some description", Name = "mt.RiskEngine.Make", IsVolatile = true)]
        public static string RiskEngine_Make(string baseHandle, string portfolioHandle, string jacobianHandle, bool useAd)
        {
            RiskEngineFunctions.RiskEngineNew_Make(baseHandle, portfolioHandle, jacobianHandle, useAd);
            return baseHandle;
        }

        [ExcelFunction(Description = "Some description", Name = "mt.RiskEngine.StoreZcbRisk", IsVolatile = true)]
        public static string RiskEngineNew_StoreZcbRisk(string baseHandle, string riskEngineHandle)
        {
            RiskEngineFunctions.RiskEngineNew_StoreZcbRisk(baseHandle, riskEngineHandle);
            return baseHandle;
        }

        [ExcelFunction(Description = "SOme descritoin", Name = "mt.RiskEngine.StoreOutrightRisk", IsVolatile = true)]
        public static string RiskEngineNew_StoreOutrightRisk(string baseHandle, string riskEngineHandle)
        {
            RiskEngineFunctions.RiskEngineNew_StoreOutrightRisk(baseHandle, riskEngineHandle);
            return baseHandle;
        }

        [ExcelFunction(Description = "Some description", Name = "mt.RiskEngine.StoreOutrightRiskFromContainer", IsVolatile = true)]
        public static string RiskOutput_StoreOutrightRiskFromContainer(string baseHandle, string riskOutputContainerHandle, string tenor)
        {
            RiskEngineFunctions.OutrightRiskOutput_StoreFromRiskOutContainer(baseHandle, riskOutputContainerHandle, tenor);
            return baseHandle;
        }

        [ExcelFunction(Description =" Some description.", Name = "mt.RiskEngine.OutrightRiskOutput.Get", IsVolatile = true)]
        public static object[,] OutrightRiskOUtput_Get(string baseHandle)
        {
            return RiskEngineFunctions.OutrightRiskOutput_Get(baseHandle);
        }

        [ExcelFunction(Description = "Some description", Name = "mt.RiskEngine.StoreRiskOutputFromContainer", IsVolatile = true)]
        public static string RiskOutput_StoreFromRiskOutputContainer(string baseHandle, string riskOutputContainerHandle, string tenor)
        {
            RiskEngineFunctions.RiskOutput_StoreFromRiskOutputContainer(baseHandle, riskOutputContainerHandle, tenor);
            return baseHandle;
        }

        [ExcelFunction(Description = "Some description", Name = "mt.RiskEngine.RiskOutput.Get", IsVolatile=true)]
        public static object[,] RiskOutput_Get(string baseHandle)
        {
            return RiskEngineFunctions.ZcbRiskOutput_Get(baseHandle);
        }

        // --- RELATED TO RISK ENGINE
        //[ExcelFunction(Description = "some description", Name = "mt.RiskEngine.MakeOld", IsVolatile = true)]
        //public static string RiskEngine_MakeOld(string baseHandle, string linearRateModelHandle, string instrumentFactoryHandle, string portfolioHandle)
        //{
        //    RiskEngineFunctions.RiskEngine_Make(baseHandle, linearRateModelHandle, instrumentFactoryHandle, portfolioHandle);
        //    return baseHandle;
        //}

        //[ExcelFunction(Description = "seom description", Name = "mt.RiskEngine.RiskPortfolio", IsVolatile = true)]
        //public static string RiskEngine_RiskPortfolio(string riskEngineHandle)
        //{
        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();
        //    RiskEngineFunctions.RiskEngine_RiskSwap(riskEngineHandle);
        //    sw.Stop();
        //    return  sw.ElapsedMilliseconds + " ms.";
        //}

        //[ExcelFunction(Description = "some description", Name = "mt.RiskEngine.GetFwdRiskOutput", IsVolatile = true)]
        //public static object[,] RiskEngin_GetFwdRiskOutput(string riskEngineHandle, string curveTenor)
        //{
        //    CurveTenor tenor = StrToEnum.CurveTenorConvert(curveTenor);
        //    return RiskEngineFunctions.RiskEngine_GetFwdRiskOutput(riskEngineHandle, tenor);
        //}

        //[ExcelFunction(Description = "some description", Name = "mt.RiskEngine.GetDiscRiskOutput", IsVolatile = true)]
        //public static object[,] RiskEngin_GetDiscRiskOutput(string riskEngineHandle)
        //{
        //    return RiskEngineFunctions.RiskEngine_GetDiscRiskOutput(riskEngineHandle);
        //}

    }
}