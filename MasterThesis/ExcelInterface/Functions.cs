using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis;

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
        public static IDictionary<string, IrSwap> Swaps = new Dictionary<string, IrSwap>();


        public static void CheckExists<T>(IDictionary<string, T> dictionary, string key, string errMessage)
        {
            if (dictionary.ContainsKey(key) == false)
                throw new InvalidOperationException(errMessage);

        }
    }

    public static class ExcelUtilities
    {
        public static object[,] BuildObjectArrayFromCurve(Curve curve, string header)
        {
            int dimension = curve.Dimension;
            object[,] output = new object[dimension + 1, 2];

            for (int i = 0; i < dimension; i++)
            {
                if (i == 0)
                {
                    output[i, 0] = header;
                    output[i, 1] = curve.Frequency.ToString();
                }
                else
                {
                    output[i, 0] = curve.Dates[i];
                    output[i, 1] = curve.Values[i];
                }
            }
            return output;
        }

    }

    public static class LinearRateFunctions
    {
        // ------- DISC CURVE FUNCTIONS

        public static void DiscCurve_Make(string baseName, List<DateTime> dates, List<double> values, CurveTenor curveType)
        {
            if (curveType == CurveTenor.DiscLibor || curveType == CurveTenor.DiscOis)
            {
                Curve output = new MasterThesis.Curve(dates, values, curveType);
                ObjectMap.DiscCurves[baseName] = output;
            }
            else
                throw new InvalidOperationException("MakeDiscCurve: has to be OIS or LIBOR curve");
        }

        public static object[,] DiscCurve_Get(string name)
        {
            ObjectMap.CheckExists(ObjectMap.DiscCurves, name, "Disc Curve does not exist");

            return ExcelUtilities.BuildObjectArrayFromCurve(ObjectMap.DiscCurves[name], name);
        }

        // ------- FWD CURVE FUNCTIONS
        public static string FwdCurveCollection_Make(string baseName, string[] fwdCurveNames, CurveTenor[] tenors)
        {
            try
            {
                FwdCurves fwdCurves = new FwdCurves();

                for (int i = 0; i < fwdCurveNames.Length; i++)
                    fwdCurves.AddCurve(ObjectMap.FwdCurves[fwdCurveNames[i]], tenors[i]);

                ObjectMap.FwdCurveCollections[baseName] = fwdCurves;
                return baseName;
            }
            catch (Exception e)
            {
                throw;
                return e.ToString();
            }

        }

        public static void FwdCurve_Make(string baseName, List<DateTime> dates, List<double> values, CurveTenor tenor)
        {
            Curve fwdCurve = new MasterThesis.Curve(dates, values, tenor);
            ObjectMap.FwdCurves[baseName] = fwdCurve;
        }

        public static object[,] FwdCurve_GetFromCollection(string collectionName, CurveTenor tenor)
        {
            ObjectMap.CheckExists(ObjectMap.FwdCurveCollections, collectionName, "Fwd curve collection does not exist");
            Curve fwdCurve = ObjectMap.FwdCurveCollections[collectionName].GetCurve(tenor);
            return ExcelUtilities.BuildObjectArrayFromCurve(fwdCurve, collectionName);
        }

        // ------- LINEAR RATE MODEL FUNTIONS
        public static void LinearRateModel_Make(string baseName, string fwdCurveCollectionName, string discCurveName)
        {
            FwdCurves fwdCurves = ObjectMap.FwdCurveCollections[fwdCurveCollectionName];
            Curve discCurve = ObjectMap.DiscCurves[discCurveName];
            LinearRateModel model = new LinearRateModel(discCurve, fwdCurves);
            ObjectMap.LinearRateModels[baseName] = model;
        }

        public static double LinearRateModel_SwapValue(string baseName, string swapName)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[baseName];
            IrSwap swap = ObjectMap.Swaps[swapName];
            return model.IrSwapPv(swap);
        }

        public static double LinearRateModel_FixedLegValue(string baseName, string fixedLegName)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[baseName];
            FixedLeg fixedLeg = ObjectMap.FixedLegs[fixedLegName];
            return model.ValueFixedLeg(fixedLeg);
        }

        public static double LinearRateModel_FloatLegValue(string baseName, string floatLegName)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[baseName];
            FloatLeg fixedLeg = ObjectMap.FloatLegs[floatLegName];
            return model.ValueFloatLeg(fixedLeg);
        }

        public static double LinearRateModel_SwapParRate(string baseName, string swapName)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[baseName];
            IrSwap swap = ObjectMap.Swaps[swapName];
            return model.IrParSwapRate(swap);
        }


        // ------- Swap functions
        public static void FixedLeg_Make(string baseName, DateTime AsOf, DateTime StartDate, DateTime EndDate, double FixedRate,
                        CurveTenor Frequency, DayCount DayCount, DayRule DayRule, double Notional, StubPlacement stub = StubPlacement.NullStub)
        {
            ObjectMap.FixedLegs[baseName] = new FixedLeg(AsOf, StartDate, EndDate, FixedRate, Frequency, DayCount, DayRule, Notional);
        }



        public static void FloatLeg_Make(string baseName, DateTime AsOf, DateTime StartDate, DateTime EndDate,
                        CurveTenor Frequency, DayCount DayCount, DayRule DayRule, double Notional, double Spread, StubPlacement stub = StubPlacement.NullStub)
        {
            ObjectMap.FloatLegs[baseName] = new FloatLeg(AsOf, StartDate, EndDate, Frequency, DayCount, DayRule, Notional, Spread, stub);
        }

        public static void PlainVanillaSwap_Make(string baseName, string floatLegName, string fixedLegName)
        {
            FixedLeg fixedLeg = ObjectMap.FixedLegs[fixedLegName];
            FloatLeg floatLeg = ObjectMap.FloatLegs[floatLegName];
            IrSwap swap = new MasterThesis.IrSwap(floatLeg, fixedLeg);

            ObjectMap.Swaps[baseName] = swap;
        }



    }
}
