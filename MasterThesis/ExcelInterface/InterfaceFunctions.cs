using System;
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
                    output[i, 0] = "";
                    output[i, 1] = header;
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

    public static class RiskEngineFunctionsNew
    {
        public static void CalibrationInstrumentSet_Make(string baseHandle, string[] linearRateProductHandles, string curveTenor)
        {

            CurveTenor tenor = StrToEnum.CurveTenorConvert(curveTenor);
            List<CalibrationInstrument> calibrationInstruments = new List<CalibrationInstrument>();

            for (int i = 0; i < linearRateProductHandles.Length; i++)
            {
                LinearRateProduct product = ObjectMap.Products[linearRateProductHandles[i]];
                calibrationInstruments.Add(new CalibrationInstrument(linearRateProductHandles[i], product, tenor));
            }

            ObjectMap.CalibrationInstrumentSets[baseHandle] = calibrationInstruments;
        }

        public static void Portfolio_Make(string baseHandle, string[] linearRateProductHandles)
        {
            List<LinearRateProduct> products = new List<LinearRateProduct>();

            for (int i = 0; i < linearRateProductHandles.Length; i++)
                products.Add(ObjectMap.Products[linearRateProductHandles[i]]);

            ObjectMap.Portfolios[baseHandle] = new Portfolio();
            ObjectMap.Portfolios[baseHandle].AddProducts(products.ToArray());
        }

        public static void RiskJacobian_Make(string baseHandle, string linearRateModelHandle, DateTime asOf, string[] calibSetsHandles, string[] curveTenors)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[linearRateModelHandle];
            RiskJacobian jacobian = new RiskJacobian(model, asOf);

            if (calibSetsHandles.Length != curveTenors.Length)
                throw new InvalidOperationException("CurveTenor and Calibration set handles must have same dimension.");

            for (int i = 0; i < calibSetsHandles.Length; i++)
                jacobian.AddInstruments(ObjectMap.CalibrationInstrumentSets[calibSetsHandles[i]], StrToEnum.CurveTenorConvert(curveTenors[i]));

            ObjectMap.RiskJacobians[baseHandle] = jacobian;
        }

        public static void RiskEngineNew_Make(string baseHandle, string portfolioHandle, string riskJacobianHandle)
        {
            RiskJacobian jacobian = ObjectMap.RiskJacobians[riskJacobianHandle];
            LinearRateModel model = jacobian.Model;
            Portfolio portfolio = ObjectMap.Portfolios[portfolioHandle];
            ObjectMap.RiskEnginesNew[baseHandle] = new RiskEngineNew(model, portfolio, jacobian);
        }

        public static void RiskEngineNew_StoreZcbRisk(string baseHandle, string riskEngineHandle)
        {
            RiskEngineNew riskEngine = ObjectMap.RiskEnginesNew[riskEngineHandle];
            riskEngine.CalculateZcbRiskBumpAndRun();

            ObjectMap.RiskOutputContainers[baseHandle] = riskEngine.RiskOutput;
        }

        public static void RiskOutput_StoreFromRiskOutputContainer(string baseHandle, string riskOutputContainerHandle, string tenor)
        {
            CurveTenor tenorEnum = StrToEnum.CurveTenorConvert(tenor);

            if (tenorEnum == CurveTenor.DiscOis)
                ObjectMap.RiskOutputs[baseHandle] = ObjectMap.RiskOutputContainers[riskOutputContainerHandle].DiscRisk;
            else
                ObjectMap.RiskOutputs[baseHandle] = ObjectMap.RiskOutputContainers[riskOutputContainerHandle].FwdRiskCollection[tenorEnum];
        }

        public static object[,] RiskOutput_Get(string baseHandle)
        {
            return ObjectMap.RiskOutputs[baseHandle].CreateRiskArray();
        }
    }

    public static class RiskEngineFunctions
    {
        public static void RiskEngine_Make(string baseName, string linearRateModel, string instrumentFactory, string product)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[linearRateModel];
            InstrumentFactory factory = ObjectMap.InstrumentFactories[instrumentFactory];
            LinearRateProduct swap = ObjectMap.Products[product];
            ObjectMap.RiskEngines[baseName] = new RiskEngine(model, factory, swap);
        }

        public static void RiskEngine_RiskSwap(string baseName)
        {
            ObjectMap.RiskEngines[baseName].CurveRiskSwap();
        }

        public static object[,] RiskEngine_GetFwdRiskOutput(string baseName, CurveTenor tenor)
        {
            return ObjectMap.RiskEngines[baseName].RiskOutput.FwdRiskCollection[tenor].CreateRiskArray();
        }

        public static object[,] RiskEngine_GetDiscRiskOutput(string baseName)
        {
            return ObjectMap.RiskEngines[baseName].RiskOutput.DiscRisk.CreateRiskArray();
        }
    }


    public static class CalibrationFunctions
    {
        public static void CalibrationSpec_Make(string baseName, double precision, double scaling, double diffStep, InterpMethod interpolation, int maxIterations, double startingValues, int bfgs_m, bool useAd, int[] calibrationOrder = null)
        {
            CalibrationSpec spec = new CalibrationSpec(precision, scaling, diffStep, interpolation, maxIterations, startingValues, bfgs_m, useAd, calibrationOrder);
            ObjectMap.CalibrationSettings[baseName] = spec;
        }


        public static List<InstrumentQuote> CreateInstrumentList(string instrumentFactoryName, string[] identifiers, double[] quotes)
        {
            List<InstrumentQuote> instrumentQuotes = new List<InstrumentQuote>();

            for (int i = 0; i < identifiers.Length; i++)
            {
                DateTime curvePoint = ObjectMap.InstrumentFactories[instrumentFactoryName].CurvePointMap[identifiers[i]];
                QuoteType type = ObjectMap.InstrumentFactories[instrumentFactoryName].InstrumentTypeMap[identifiers[i]];
                instrumentQuotes.Add(new InstrumentQuote(identifiers[i], type, curvePoint, quotes[i]));
            }

            return instrumentQuotes;
        }

        public static void CurveCalibrationProblem_Make(string baseName, string instrumentFactoryName, string[] identifiers, double[] quotes)
        {
            InstrumentFactory factory = ObjectMap.InstrumentFactories[instrumentFactoryName];
            List<InstrumentQuote> instrumentQuotes = CreateInstrumentList(instrumentFactoryName, identifiers, quotes);
            ObjectMap.CurveCalibrationProblems[baseName] = new CurveCalibrationProblem(factory, instrumentQuotes);
        }

        public static void FwdCurveCalibrationProblem_Make(string baseName, string discCurveName, string[] problemNames, CurveTenor[] tenors, string settingsName)
        {
            CurveCalibrationProblem[] problems = new CurveCalibrationProblem[problemNames.Length];

            for (int i = 0; i<problems.Length; i++)
                problems[i] = ObjectMap.CurveCalibrationProblems[problemNames[i]];

            ObjectMap.FwdCurveConstructors[baseName] = new FwdCurveConstructor(ObjectMap.DiscCurves[discCurveName], problems, tenors, ObjectMap.CalibrationSettings[settingsName]);
        }

        public static void FwdCurveCollection_MakeFromCalibrationProblem(string baseName, string calibrationProblem)
        {
            FwdCurveConstructor constructor = ObjectMap.FwdCurveConstructors[calibrationProblem];
            constructor.CalibrateAllCurves();
            ObjectMap.FwdCurveCollections[baseName] = constructor.GetFwdCurves();
        }

        public static void DiscCurve_MakeFromCalibrationProblem(string baseName, string curveCalibrationProblem, string settingsName)
        {
            DiscCurveConstructor constructor = new DiscCurveConstructor(ObjectMap.CurveCalibrationProblems[curveCalibrationProblem], ObjectMap.CalibrationSettings[settingsName]);
            constructor.CalibrateCurve();
            ObjectMap.DiscCurves[baseName] = constructor.GetCurve();
        }
    }

    public static class InstrumentFactoryFunctions
    {
        public static object[,] InstrumentFactory_GetInstrumentInfo(string baseName, string identifier)
        {
            return ConstructInstrumentInspector.MakeExcelOutput(ObjectMap.InstrumentFactories[baseName], identifier);
        }

        public static void InstrumentFactory_StoreInstrumentsInMap(string baseName)
        {
            InstrumentFactory factory = ObjectMap.InstrumentFactories[baseName];

            foreach (string key in factory.IrSwaps.Keys)
                ObjectMap.Products[key] = factory.IrSwaps[key];

            foreach (string key in factory.OisSwaps.Keys)
                ObjectMap.Products[key] = factory.OisSwaps[key];

            foreach (string key in factory.BasisSwaps.Keys)
                ObjectMap.Products[key] = factory.BasisSwaps[key];

            foreach (string key in factory.Futures.Keys)
                ObjectMap.Products[key] = factory.Futures[key];

            foreach (string key in factory.Fras.Keys)
                ObjectMap.Products[key] = factory.Fras[key];
        }

        public static void InstrumentFactory_Make(string baseName, DateTime asOf)
        {
            ObjectMap.InstrumentFactories[baseName] = new InstrumentFactory(asOf);
        }

        public static double ValueInstrument(string instrument, string instrumentFactory, string linearRateModel)
        {
            return ObjectMap.InstrumentFactories[instrumentFactory].ValueInstrumentFromFactory(ObjectMap.LinearRateModels[linearRateModel], instrument);
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
            return ObjectMap.LinearRateModels[model].OisRateSimple(swap);
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

        public static void DiscCurve_Make(string baseName, List<DateTime> dates, List<double> values)
        {
                Curve output = new MasterThesis.Curve(dates, values);
                ObjectMap.DiscCurves[baseName] = output;
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

        public static void FwdCurve_Make(string baseName, List<DateTime> dates, List<double> values)
        {
            Curve fwdCurve = new MasterThesis.Curve(dates, values);
            ObjectMap.FwdCurves[baseName] = fwdCurve;
        }

        public static object[,] FwdCurve_GetFromCollection(string collectionName, CurveTenor tenor)
        {
            ObjectMap.CheckExists(ObjectMap.FwdCurveCollections, collectionName, "Fwd curve collection does not exist");
            Curve fwdCurve = ObjectMap.FwdCurveCollections[collectionName].GetCurve(tenor);
            return ExcelUtilities.BuildObjectArrayFromCurve(fwdCurve, collectionName);
        }

        public static void FwdCurve_StoreFromCollection(string baseName, string collectionName, CurveTenor tenor)
        {
            ObjectMap.FwdCurves[baseName] = ObjectMap.FwdCurveCollections[collectionName].GetCurve(tenor);
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
            IrSwap swap = (IrSwap) ObjectMap.Products[swapName];
            //IrSwap swap = ObjectMap.IrSwaps[swapName];
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
            IrSwap swap = (IrSwap) ObjectMap.Products[swapName];
            //IrSwap swap = ObjectMap.IrSwaps[swapName];
            return model.IrParSwapRate(swap);
        }

        public static double LinearRateModel_BasisSwapValue(string baseName, string swapName)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[baseName];
            BasisSwap swap = (BasisSwap) ObjectMap.Products[swapName];
            //BasisSwap swap = ObjectMap.BasisSwaps[swapName];
            return model.BasisSwapPv(swap);
        }

        public static double LinearRateModel_BasisParSpread(string modelName, string basisSwapName)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[modelName];
            BasisSwap swap = (BasisSwap)ObjectMap.Products[basisSwapName];
            //BasisSwap swap = ObjectMap.BasisSwaps[basisSwapName];
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

        public static void PlainVanillaSwap_Make(string baseName, string fixedLegName, string floatLegName)
        {
            FixedLeg fixedLeg = ObjectMap.FixedLegs[fixedLegName];
            FloatLeg floatLeg = ObjectMap.FloatLegs[floatLegName];
            IrSwap swap = new MasterThesis.IrSwap(floatLeg, fixedLeg);

            ObjectMap.Products[baseName] = swap;
            ObjectMap.IrSwaps[baseName] = swap;
        }

        // -------- BASIS SWAP FUNTIONS
        public static void BasisSwap_Make(string baseName, string floatLegNoSpreadName, string floatLegSpreadName)
        {
            FloatLeg floatLegNoSpread = ObjectMap.FloatLegs[floatLegNoSpreadName];
            FloatLeg floatLegSpread = ObjectMap.FloatLegs[floatLegSpreadName];
            BasisSwap swap = new MasterThesis.BasisSwap(floatLegNoSpread, floatLegSpread);

            ObjectMap.Products[baseName] = swap;
            ObjectMap.BasisSwaps[baseName] = swap;
        }



    }
}
