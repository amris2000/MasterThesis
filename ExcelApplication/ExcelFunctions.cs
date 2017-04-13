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
        public static string LinearRate_DiscCurve_Make(string baseName, object[] dates, object[] values)
        {

            var dValues = values.Cast<double>();
            var dDates = dates.Cast<double>();
            DateTime[] actualDates = ConvertDoublesToDateTimes(dDates.ToArray());

            List<DateTime> datesList = actualDates.ToList();
            List<double> doubleList = dValues.ToList();
            LinearRateFunctions.DiscCurve_Make(baseName, datesList, doubleList);
            return baseName;
        }

        [ExcelFunction(Description = "My First function in Excel", Name = "mt.LinearRate.FwdCurve.Make", IsVolatile = true)]
        public static string LinearRate_FwdCurve_Make(string baseName, object[] dates, object[] values)
        {

            var dValues = values.Cast<double>();
            var dDates = dates.Cast<double>();
            DateTime[] actualDates = ConvertDoublesToDateTimes(dDates.ToArray());

            List<DateTime> datesList = actualDates.ToList();
            List<double> doubleList = dValues.ToList();
            LinearRateFunctions.FwdCurve_Make(baseName, datesList, doubleList);
            return baseName;
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurveCollection.Make", IsVolatile = true)]
        public static string LinearRate_FwdCurveCollection_Make(string baseName, object[] fwdCurveNames, object[] tenorNames)
        {
            CurveTenor[] tenorEnums = new CurveTenor[tenorNames.Length];
            var fwdCurveNamesString = fwdCurveNames.Cast<string>().ToArray();

            for (int i = 0; i < tenorEnums.Length; i++)
                tenorEnums[i] = StrToEnum.CurveTenorConvert((string) tenorNames[i]);

            string output = LinearRateFunctions.FwdCurveCollection_Make(baseName, fwdCurveNamesString, tenorEnums);

            return output;
        }

        // .. Get functions

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.DiscCurve.Get", IsVolatile = true)]
        public static object[,] LinearRate_DiscCurve_Get(string curveName)
        {
            return LinearRateFunctions.DiscCurve_Get(curveName);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurve.StoreFromCollection")]
        public static string LinearRate_FwdCurve_StoreFromCollection(string baseName, string collectionName, string tenor)
        {
            CurveTenor tenorActual = StrToEnum.CurveTenorConvert(tenor);
            LinearRateFunctions.FwdCurve_StoreFromCollection(baseName, collectionName, tenorActual);
            return baseName;
        }


        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurve.GetFromCollection", IsVolatile = true)]
        public static object[,] LinearRate_DiscCurve_Get(string collectionName, string tenor)
        {
            CurveTenor tenorEnum = StrToEnum.CurveTenorConvert(tenor);
            return LinearRateFunctions.FwdCurve_GetFromCollection(collectionName, tenorEnum);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.DiscCurve.GetValue", IsVolatile =true)]
        public static double LinearRate_DiscCurve_GetValue(string baseName, double date, string interpolationMethod)
        {
            DateTime dateTime = DateTime.FromOADate(date);
            InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationMethod);
            return LinearRateFunctions.DiscCurve_GetValue(baseName, dateTime, interpolation);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurve.GetValue", IsVolatile = true)]
        public static double LinearRate_FwdCurve_GetValue(string baseName, double date, string interpolationMethod)
        {
            DateTime dateTime = DateTime.FromOADate(date);
            InterpMethod interpolation = StrToEnum.InterpolationConvert(interpolationMethod);
            return LinearRateFunctions.FwdCurve_GetValue(baseName, dateTime, interpolation);
        }

        // ------- LINEAR RATE FUNTIONS

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.Make", IsVolatile = true)]
        public static string LinearRate_LinearRateModel_Make(string baseName, string fwdCurveCollectionName, string discCurveName, string interpolation)
        {
            InterpMethod interpMethod = StrToEnum.InterpolationConvert(interpolation);
            LinearRateFunctions.LinearRateModel_Make(baseName, fwdCurveCollectionName, discCurveName, interpMethod);
            return baseName;
        }

        // Swap functions
        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.SwapValue", IsVolatile =true)]
        public static double LinearRate_LinearRateModel_ValueSwap(string linearRateModel, string swap)
        {
            return LinearRateFunctions.LinearRateModel_SwapValue(linearRateModel, swap);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.SwapParRate", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_ParRate(string linearRateModel, string swap)
        {
            return LinearRateFunctions.LinearRateModel_SwapParRate(linearRateModel, swap);
        }

        // BasisSwap Valuation
        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.BasisSwapValue", IsVolatile =true)]
        public static double LinearRate_LinearRateModel_ValueBasisSwap(string linearRateModel, string swap)
        {
            return LinearRateFunctions.LinearRateModel_BasisSwapValue(linearRateModel, swap);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.ParBasisSpread", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_ParBasisSpread(string model, string basisSwap)
        {
            return LinearRateFunctions.LinearRateModel_BasisParSpread(model, basisSwap);
        }

        // SwapLeg valuation
        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.FloatLegValue", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_FloatLegValue(string linearRateModel, string floatLeg)
        {
            return LinearRateFunctions.LinearRateModel_FloatLegValue(linearRateModel, floatLeg);
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.FixedLegValue", IsVolatile = true)]
        public static double LinearRate_LinearRateModel_FixedLegValue(string linearRateModel, string fixedLeg)
        {
            return LinearRateFunctions.LinearRateModel_FixedLegValue(linearRateModel, fixedLeg);
        }

        // ------- SWAP FUNCTIONS

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.IrSwap.Make", IsVolatile = true)]
        public static string LinearRate_PlainVanillaSwap_Make(string baseName, string fixedLegName, string floatLegName)
        {
            LinearRateFunctions.PlainVanillaSwap_Make(baseName, fixedLegName, floatLegName);
            return baseName;
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.BasisSwap.Make", IsVolatile = true)]
        public static string LinearRate_BasisSwap_Make(string baseName, string floatLegSpreadName, string floatLegNoSpreadName)
        {
            LinearRateFunctions.BasisSwap_Make(baseName, floatLegNoSpreadName, floatLegSpreadName);
            return baseName;
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FloatLeg.Make", IsVolatile =true)]
        public static string LinearRate_FloatLeg_Make(string baseName, DateTime AsOf, DateTime StartDate, DateTime EndDate,
                        string Frequency, string DayCount, string DayRule, double Notional, double Spread)
        {
            CurveTenor tenorEnum = StrToEnum.CurveTenorConvert(Frequency);
            DayCount dayCountEnum = StrToEnum.DayCountConvert(DayCount);
            DayRule dayRuleEnum = StrToEnum.DayRuleConvert(DayRule);

            LinearRateFunctions.FloatLeg_Make(baseName, AsOf, StartDate, EndDate, tenorEnum, dayCountEnum, dayRuleEnum, Notional, Spread);
            return baseName;
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FixedLeg.Make", IsVolatile = true)]
        public static string LinearRate_FixedLeg_Make(string baseName, DateTime AsOf, DateTime StartDate, DateTime EndDate, double FixedRate,
                        string Frequency, string DayCount, string DayRule, double Notional)
        {
            CurveTenor tenorEnum = StrToEnum.CurveTenorConvert(Frequency);
            DayCount dayCountEnum = StrToEnum.DayCountConvert(DayCount);
            DayRule dayRuleEnum = StrToEnum.DayRuleConvert(DayRule);

            LinearRateFunctions.FixedLeg_Make(baseName, AsOf, StartDate, EndDate, FixedRate, tenorEnum, dayCountEnum, dayRuleEnum, Notional);
            return baseName;
        }

        // ------ CALIBRATION RELATED

        [ExcelFunction(Description = "Some description.", Name = "mt.CurveCalibrationProblem.Make")]
        public static string Calibration_CurveCalibrationProblem_Make(string baseName, string instrumentFactoryName, object[] identifiers, object[] quotes)
        {
            CalibrationFunctions.CurveCalibrationProblem_Make(baseName, instrumentFactoryName, identifiers.Cast<string>().ToArray(), quotes.Cast<double>().ToArray());
            return baseName;
        }

        [ExcelFunction(Description = "Make CalibrationSpec", Name = "mt.Calibration.CalibrationSettings.Make")]
        public static string Calibration_CalibrationSettings_Make(string baseName, double precision, double scaling, double diffStep, string interpolation, int maxIterations, double startingValues, int bfgs_m, bool useAd, object[] calibrationOrder = null)
        {
            InterpMethod interp = StrToEnum.InterpolationConvert(interpolation);

            if (calibrationOrder[0] is ExcelMissing)
            {
                calibrationOrder = null;
                CalibrationFunctions.CalibrationSpec_Make(baseName, precision, scaling, diffStep, interp, maxIterations, startingValues, bfgs_m, useAd);
            }
            else
            {
                // Need to do this, since object[] cannot be cast to int[]...
                int[] intCalibrationOrder = new int[calibrationOrder.Length];
                for (int i = 0; i < calibrationOrder.Length; i++)
                    intCalibrationOrder[i] = Convert.ToInt32(calibrationOrder[i]);
                
                CalibrationFunctions.CalibrationSpec_Make(baseName, precision, scaling, diffStep, interp, maxIterations, startingValues, bfgs_m, useAd, intCalibrationOrder);

            }

            return baseName;
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.DiscCurve.MakeFromCalibrationProblem")]
        public static string Calibration_DiscCurve_MakeFromCalibrationProblem(string baseName, string problem, string calibSpec)
        {
            if (ExcelDnaUtil.IsInFunctionWizard())
                return "No calulation in wizard.";

            CalibrationFunctions.DiscCurve_MakeFromCalibrationProblem(baseName, problem, calibSpec);
            return baseName;
        }

        [ExcelFunction(Description = "some description.", Name = "mt.FwdCurveCalibrationProblem.Make")]
        public static string Calibration_FwdCurveCalibrationProblem_Make(string baseName, string discCurveName, object[] problemNames, object[] tenors, string calibSpec)
        {

            string[] problemNamesString = problemNames.Cast<string>().ToArray();
            string[] tenorsString = tenors.Cast<string>().ToArray();
            CurveTenor[] tenorsActual = new CurveTenor[tenorsString.Length];

            for (int i = 0; i<tenorsString.Length; i++)
                tenorsActual[i] = StrToEnum.CurveTenorConvert(tenorsString[i]);

            CalibrationFunctions.FwdCurveCalibrationProblem_Make(baseName, discCurveName, problemNamesString, tenorsActual, calibSpec);
            return baseName;
        }

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurveCollection.MakeFromCalibrationProblem")]
        public static string Calibration_FwdCurveCollection_MakeFromCalibrationProblem(string baseName, string fwdCurveConstructor)
        { 
            if (ExcelDnaUtil.IsInFunctionWizard())
                return "No calulation in wizard.";

            CalibrationFunctions.FwdCurveCollection_MakeFromCalibrationProblem(baseName, fwdCurveConstructor);
            return baseName;

        }

        // ------ INSTRUMENT FACTORY RELATED

        [ExcelFunction(Description = "Some description.", Name = "mt.InstrumentFactory.Make", IsVolatile = true)]
        public static string Factory_InstrumentFactory_Make(string baseName, DateTime asOf)
        {
            InstrumentFactoryFunctions.InstrumentFactory_Make(baseName, asOf);
            return baseName;
        }

        [ExcelFunction(Description = "Some description", Name = "mt.InstrumentFactory.AddInstrumentsToProductMap")]
        public static string Factory_InstrumentFactory_Make(string baseName)
        {
            InstrumentFactoryFunctions.InstrumentFactory_StoreInstrumentsInMap(baseName);
            return "Instruments added to ObjectMap.";
        }

        [ExcelFunction(Description = "some description.", Name = "mt.InstrumentFactory.AddSwaps", IsVolatile = true)]
        public static string Factory_InstrumentFactory_AddSwaps(string baseName, object[] swapStrings)
        {
            var swapStringsString = swapStrings.Cast<string>().ToArray();
            InstrumentFactoryFunctions.InstrumentFactory_AddSwaps(baseName, swapStringsString);
            return "Added " + swapStrings.Length + " swap-instruments to " + baseName;
        }

        [ExcelFunction(Description = "some description.", Name = "mt.InstrumentFactory.AddBasisSwaps", IsVolatile = true)]
        public static string Factory_InstrumentFactory_AddBasisSwaps(string baseName, object[] swapStrings)
        {
            var swapStringsString = swapStrings.Cast<string>().ToArray();
            InstrumentFactoryFunctions.InstrumentFactory_AddBasisSwaps(baseName, swapStringsString);
            return "Added " + swapStrings.Length + " MmBasisSwap-instruments to " + baseName;
        }

        [ExcelFunction(Description = "some description.", Name = "mt.InstrumentFactory.AddFras", IsVolatile = true)]
        public static string Factory_InstrumentFactory_AddFras(string baseName, object[] fraStrings)
        {
            var fraStringsString = fraStrings.Cast<string>().ToArray();
            InstrumentFactoryFunctions.InstrumentFactory_AddFras(baseName, fraStringsString);
            return "Added " + fraStrings.Length + " fra-instruments to " + baseName;
        }

        [ExcelFunction(Description = "some description.", Name = "mt.InstrumentFactory.AddFutures", IsVolatile = true)]
        public static string Factory_InstrumentFactory_AddFutures(string baseName, object[] futureStrings)
        {
            var futureStringsString = futureStrings.Cast<string>().ToArray();
            InstrumentFactoryFunctions.InstrumentFactory_AddFutures(baseName, futureStringsString);
            return "Added " + futureStrings.Length + " futures-instruments to " + baseName;
        }

        [ExcelFunction(Description = "some description.", Name = "mt.InstrumentFactory.AddFwdStartingSwaps", IsVolatile = true)]
        public static string Factory_InstrumentFactory_AddFwdStartingSwaps(string baseName, object[] swapStrings)
        {
            var swapStringsString = swapStrings.Cast<string>().ToArray();
            InstrumentFactoryFunctions.InstrumentFactory_AddFwdStartingSwaps(baseName, swapStringsString);
            return "Added " + swapStrings.Length + " swap-instruments to " + baseName;
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
        public static double Factory_InstrumentFactory_ValueInstrument(string instrument, string factory, string model)
        {
            return InstrumentFactoryFunctions.ValueInstrument(instrument, factory, model);
        }

        [ExcelFunction(Description = "some description", Name = "mt.InstrumentFactory.GetInstrumentInfo", IsVolatile = true)]
        public static object[,] Factory_InstrumentFactory_GetInstrumentInfo(string instrument, string factory)
        {
            return InstrumentFactoryFunctions.InstrumentFactory_GetInstrumentInfo(factory, instrument);
        }

        [ExcelFunction(Description = "some description", Name = "mt.ParseStringAndOutput", IsVolatile = true)]
        public static object[,] ParseStringAndOutput(string str)
        {
            object[] output = str.Split(',');
            object[,] realOutput = new object[output.Length, 1];
            for (int i = 0; i<output.Length; i++)
            {
                realOutput[i, 0] = output[i];
            }

            return realOutput;
        }

        // --- RELATED TO RISK ENGINE
        [ExcelFunction(Description = "some description", Name = "mt.RiskEngine.Make", IsVolatile = true)]
        public static string RiskEngine_Make(string baseName, string linearRateModel, string instrumentFactory, string portfolio)
        {
            RiskEngineFunctions.RiskEngine_Make(baseName, linearRateModel, instrumentFactory, portfolio);
            return baseName;
        }

        [ExcelFunction(Description = "seom description", Name = "mt.RiskEngine.RiskPortfolio", IsVolatile = true)]
        public static string RiskEngine_RiskPortfolio(string baseName)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            RiskEngineFunctions.RiskEngine_RiskSwap(baseName);
            sw.Stop();
            return  sw.ElapsedMilliseconds + " ms.";
        }

        [ExcelFunction(Description = "some description", Name = "mt.RiskEngine.GetFwdRiskOutput", IsVolatile = true)]
        public static object[,] RiskEngin_GetFwdRiskOutput(string baseName, string curveTenor)
        {
            CurveTenor tenor = StrToEnum.CurveTenorConvert(curveTenor);
            return RiskEngineFunctions.RiskEngine_GetFwdRiskOutput(baseName, tenor);
        }

        [ExcelFunction(Description = "some description", Name = "mt.RiskEngine.GetDiscRiskOutput", IsVolatile = true)]
        public static object[,] RiskEngin_GetDiscRiskOutput(string baseName)
        {
            return RiskEngineFunctions.RiskEngine_GetDiscRiskOutput(baseName);
        }

    }
}