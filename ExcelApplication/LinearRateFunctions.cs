using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDna.Integration;
using MasterThesis;
using MasterThesis.ExcelInterface;

namespace ExcelApplication
{
    public class LinearRateFunctions2
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
        public static string LinearRate_DiscCurve_Make(string baseName, object[] dates, object[] values, string curveType)
        {

            var dValues = values.Cast<double>();
            var dDates = dates.Cast<double>();
            DateTime[] actualDates = ConvertDoublesToDateTimes(dDates.ToArray());


            CurveTenor curveTypeEnum = StrToEnum.CurveTenorConvert(curveType);
            List<DateTime> datesList = actualDates.ToList();
            List<double> doubleList = dValues.ToList();
            LinearRateFunctions.DiscCurve_Make(baseName, datesList, doubleList, curveTypeEnum);
            return baseName;
        }

        [ExcelFunction(Description = "My First function in Excel", Name = "mt.LinearRate.FwdCurve.Make", IsVolatile = true)]
        public static string LinearRate_FwdCurve_Make(string baseName, object[] dates, object[] values, string curveType)
        {

            var dValues = values.Cast<double>();
            var dDates = dates.Cast<double>();
            DateTime[] actualDates = ConvertDoublesToDateTimes(dDates.ToArray());

            CurveTenor curveTypeEnum = StrToEnum.CurveTenorConvert(curveType);
            List<DateTime> datesList = actualDates.ToList();
            List<double> doubleList = dValues.ToList();
            LinearRateFunctions.FwdCurve_Make(baseName, datesList, doubleList, curveTypeEnum);
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

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRate.FwdCurve.GetFromCollection", IsVolatile = true)]
        public static object[,] LinearRate_DiscCurve_Get(string collectionName, string tenor)
        {
            CurveTenor tenorEnum = StrToEnum.CurveTenorConvert(tenor);
            return LinearRateFunctions.FwdCurve_GetFromCollection(collectionName, tenorEnum);
        }

        // ------- LINEAR RATE FUNTIONS

        [ExcelFunction(Description = "Some description.", Name = "mt.LinearRateModel.Make", IsVolatile = true)]
        public static string LinearRate_LinearRateModel_Make(string baseName, string fwdCurveCollectionName, string discCurveName)
        {
            LinearRateFunctions.LinearRateModel_Make(baseName, fwdCurveCollectionName, discCurveName);
            return baseName;
        }

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




    }
}