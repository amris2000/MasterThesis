﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis;

namespace MasterThesis.ExcelInterface
{


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

    public static class InstrumentFactoryFunctions
    {
        public static void InstrumentFactory_Make(string baseName, DateTime asOf)
        {
            ObjectMap.InstrumentFactories[baseName] = new InstrumentFactory(asOf);
        }
        
        // ADD functions
        public static void InstrumentFactory_AddSwaps(string baseName, string[] swapStrings)
        {
            ObjectMap.InstrumentFactories[baseName].AddSwaps(swapStrings);
        }

        public static void InstrumentFactory_AddBasisSwaps(string baseName, string[] swapStrings)
        {
            ObjectMap.InstrumentFactories[baseName].AddBasisSwaps(swapStrings);
        }

        public static void InstrumentFactory_AddFras(string baseName, string[] fraStrings)
        {
            ObjectMap.InstrumentFactories[baseName].AddFras(fraStrings);
        }

        public static void InstrumentFactory_AddFutures(string baseName, string[] futureStrings)
        {
            ObjectMap.InstrumentFactories[baseName].AddFutures(futureStrings);
        }

        public static void InstrumentFactory_AddFwdStartingSwaps(string baseName, string[] swapStrings)
        {
            ObjectMap.InstrumentFactories[baseName].AddFwdStartingSwaps(swapStrings);
        }

        // NAME SHOULD BE CHANGED: CALCULATES PAR RATE
        public static double InstrumentFactory_ValueSwap(string instrumentFactory, string model, string instrument)
        {
            IrSwap swap = ObjectMap.InstrumentFactories[instrumentFactory].IrSwaps[instrument];
            return ObjectMap.LinearRateModels[model].IrParSwapRate(swap);
        }

        // NAME SHOULD BE CHANGED: CALCULATES PAR SPREAD
        public static double InstrumentFactory_ValueBasisSwap(string instrumentFactory, string model, string instrument)
        {
            BasisSwap swap = ObjectMap.InstrumentFactories[instrumentFactory].BasisSwaps[instrument];
            return ObjectMap.LinearRateModels[model].ParBasisSpread(swap);
        }

        // NAME SHOULD BE CHANGED: CALCULATES OIS RATE
        public static double InstrumentFactory_ValueOisSwap(string instrumentFactory, string model, string instrument)
        {
            OisSwap swap = ObjectMap.InstrumentFactories[instrumentFactory].OisSwaps[instrument];
            return ObjectMap.LinearRateModels[model].OisRate(swap);
        }

        // NAME SHOULD BE CHANGED: CALCULATES PAR FRA RATE
        public static double InstrumentFactory_ParFraRate(string instrumentFactory, string model, string instrument)
        {
            Fra fra = ObjectMap.InstrumentFactories[instrumentFactory].Fras[instrument];
            return ObjectMap.LinearRateModels[model].ParFraRate(fra);
        }

        public static double InstrumentFactory_ParFutureRate(string instrumentFactory, string model, string instrument)
        {
            Future future = ObjectMap.InstrumentFactories[instrumentFactory].Futures[instrument];
            return ObjectMap.LinearRateModels[model].ParFutureRate(future);
        }
    }

    public static class LinearRateFunctions
    {
        // ------- General Curve functionality
        public static double DiscCurve_GetValue(string baseName, DateTime date, InterpMethod interpolation)
        {
            return ObjectMap.DiscCurves[baseName].Interp(date, interpolation);
        }

        public static double FwdCurve_GetValue(string baseName, DateTime date, InterpMethod interpolation)
        {
            return ObjectMap.FwdCurves[baseName].Interp(date, interpolation);
        }


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
        public static void LinearRateModel_Make(string baseName, string fwdCurveCollectionName, string discCurveName, InterpMethod interpolation)
        {
            FwdCurves fwdCurves = ObjectMap.FwdCurveCollections[fwdCurveCollectionName];
            Curve discCurve = ObjectMap.DiscCurves[discCurveName];
            LinearRateModel model = new LinearRateModel(discCurve, fwdCurves, interpolation);
            ObjectMap.LinearRateModels[baseName] = model;
        }

        public static double LinearRateModel_SwapValue(string baseName, string swapName)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[baseName];
            IrSwap swap = ObjectMap.IrSwaps[swapName];
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
            IrSwap swap = ObjectMap.IrSwaps[swapName];
            return model.IrParSwapRate(swap);
        }

        public static double LinearRateModel_BasisSwapValue(string baseName, string swapName)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[baseName];
            BasisSwap swap = ObjectMap.BasisSwaps[swapName];
            return model.BasisSwapPv(swap);
        }

        public static double LinearRateModel_BasisParSpread(string modelName, string basisSwapName)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[modelName];
            BasisSwap swap = ObjectMap.BasisSwaps[basisSwapName];
            return model.ParBasisSpread(swap);
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

            ObjectMap.IrSwaps[baseName] = swap;
        }

        // -------- BASIS SWAP FUNTIONS
        public static void BasisSwap_Make(string baseName, string floatLegNoSpreadName, string floatLegSpreadName)
        {
            FloatLeg floatLegNoSpread = ObjectMap.FloatLegs[floatLegNoSpreadName];
            FloatLeg floatLegSpread = ObjectMap.FloatLegs[floatLegSpreadName];
            BasisSwap swap = new MasterThesis.BasisSwap(floatLegNoSpread, floatLegSpread);

            ObjectMap.BasisSwaps[baseName] = swap;
        }



    }
}