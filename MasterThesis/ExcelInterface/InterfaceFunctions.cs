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

    public static class ADFunctions
    {
        public static object[,] ZcbRiskAD(string linearRateModelHandle, string productHandle)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[linearRateModelHandle];
            LinearRateInstrument product = ObjectMap.LinearRateInstruments[productHandle];
            return model.CreateTestOutputAD(product);
        }
    }

    public static class RiskEngineFunctions
    {
        public static void CalibrationInstrumentSet_Make(string baseHandle, string[] linearRateProductHandles, string curveTenor)
        {
            CurveTenor tenor = StrToEnum.CurveTenorConvert(curveTenor);
            List<CalibrationInstrument> calibrationInstruments = new List<CalibrationInstrument>();

            for (int i = 0; i < linearRateProductHandles.Length; i++)
            {
                LinearRateInstrument product = ObjectMap.LinearRateInstruments[linearRateProductHandles[i]];
                calibrationInstruments.Add(new CalibrationInstrument(linearRateProductHandles[i], product, tenor));
            }

            ObjectMap.CalibrationInstrumentSets[baseHandle] = calibrationInstruments;
        }

        public static void Portfolio_Make(string baseHandle, string[] linearRateProductHandles)
        {
            List<LinearRateInstrument> products = new List<LinearRateInstrument>();

            for (int i = 0; i < linearRateProductHandles.Length; i++)
                products.Add(ObjectMap.LinearRateInstruments[linearRateProductHandles[i]]);

            ObjectMap.Portfolios[baseHandle] = new Portfolio();
            ObjectMap.Portfolios[baseHandle].AddProducts(products.ToArray());
        }

        public static void RiskJacobian_Make(string baseHandle, string linearRateModelHandle, DateTime asOf, string[] calibSetsHandles, string[] curveTenors, bool useAd = false)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[linearRateModelHandle];
            RiskJacobian jacobian = new RiskJacobian(model, asOf);

            if (calibSetsHandles.Length != curveTenors.Length)
                throw new InvalidOperationException("CurveTenor and Calibration set handles must have same dimension.");

            for (int i = 0; i < calibSetsHandles.Length; i++)
                jacobian.AddInstruments(ObjectMap.CalibrationInstrumentSets[calibSetsHandles[i]], StrToEnum.CurveTenorConvert(curveTenors[i]));

            // Using this procedure, the instruments are actually sorted.
            jacobian.Initialize();

            if (useAd)
                // Construct using AD
                jacobian.ConstructUsingAD();
            else
                jacobian.ConstructUsingBumpAndRun();

            ObjectMap.RiskJacobians[baseHandle] = jacobian;
        }

        public static void RiskEngineNew_Make(string baseHandle, string portfolioHandle, string riskJacobianHandle, bool useAd)
        {
            RiskJacobian jacobian = ObjectMap.RiskJacobians[riskJacobianHandle];
            double determinant = jacobian.Jacobian.Determinant();
            LinearRateModel model = jacobian.Model;
            Portfolio portfolio = ObjectMap.Portfolios[portfolioHandle];
            ObjectMap.RiskEngines[baseHandle] = new RiskEngine(model, portfolio, jacobian, useAd);
        }

        public static void RiskEngineNew_StoreZcbRisk(string baseHandle, string riskEngineHandle)
        {
            RiskEngine riskEngine = ObjectMap.RiskEngines[riskEngineHandle];
            riskEngine.CalculateZcbRiskBumpAndRun();

            ObjectMap.ZcbRiskOutputContainers[baseHandle] = riskEngine.ZcbRiskOutput;
        }

        public static void RiskEngineNew_StoreOutrightRisk(string baseHandle, string riskEngineHandle)
        {
            RiskEngine riskEngine = ObjectMap.RiskEngines[riskEngineHandle];
            ObjectMap.OutrightRiskContainers[baseHandle] = riskEngine.OutrightRiskOutput;
        }

        public static void OutrightRiskOutput_StoreFromRiskOutContainer(string baseHandle, string riskOutputContainerHandle, string tenor)
        {
            CurveTenor tenorEnum = StrToEnum.CurveTenorConvert(tenor);

            if (tenorEnum == CurveTenor.DiscOis)
                ObjectMap.OutrightRiskOutputs[baseHandle] = ObjectMap.OutrightRiskContainers[riskOutputContainerHandle].DiscRisk;
            else
                ObjectMap.OutrightRiskOutputs[baseHandle] = ObjectMap.OutrightRiskContainers[riskOutputContainerHandle].FwdRiskCollection[tenorEnum];
        }

        public static object[,] OutrightRiskOutput_Get(string outrightRiskOutputHandle)
        {
            return ObjectMap.OutrightRiskOutputs[outrightRiskOutputHandle].CreateRiskArray();
        }

        public static void RiskOutput_StoreFromRiskOutputContainer(string baseHandle, string riskOutputContainerHandle, string tenor)
        {
            CurveTenor tenorEnum = StrToEnum.CurveTenorConvert(tenor);

            if (tenorEnum == CurveTenor.DiscOis)
                ObjectMap.ZcbRiskOutputs[baseHandle] = ObjectMap.ZcbRiskOutputContainers[riskOutputContainerHandle].DiscRisk;
            else
                ObjectMap.ZcbRiskOutputs[baseHandle] = ObjectMap.ZcbRiskOutputContainers[riskOutputContainerHandle].FwdRiskCollection[tenorEnum];
        }

        public static object[,] ZcbRiskOutput_Get(string zcbRiskoutputHandle)
        {
            return ObjectMap.ZcbRiskOutputs[zcbRiskoutputHandle].CreateRiskArray();
        }
    }

    public static class CalibrationFunctions
    {
        public static void CalibrationSpec_Make(string baseName, double precision, double scaling, double diffStep, InterpMethod interpolation, int maxIterations, double startingValues, int bfgs_m, bool useAd, bool inheritDiscSize, double stepSizeOfInheritance, int[] calibrationOrder = null)
        {
            CalibrationSpec spec = new CalibrationSpec(precision, scaling, diffStep, interpolation, maxIterations, startingValues, bfgs_m, useAd, inheritDiscSize, stepSizeOfInheritance, calibrationOrder);
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

        public static void FwdCurveCollection_MakeFromCalibrationProblem(string baseName, string calibrationProblem, bool useAD)
        {
            FwdCurveConstructor constructor = ObjectMap.FwdCurveConstructors[calibrationProblem];

            if (useAD)
            {
                //constructor.CalibrateCurves_AD(); // Old method that calibrates all curves simultanously
                constructor.CalibrateAllCurves_AD();
            }
            else
                constructor.CalibrateAllCurves();

            ObjectMap.FwdCurveCollections[baseName] = constructor.GetFwdCurves();
        }

        public static void FwdCurveCollection_MakeFromCalibrationProblemAndExistingCurves(string baseName, string calibrationProblem, string fwdCurveCollectionHandle)
        {
            FwdCurveConstructor constructor = ObjectMap.FwdCurveConstructors[calibrationProblem];
            FwdCurveContainer fwdCurves = ObjectMap.FwdCurveCollections[fwdCurveCollectionHandle];

            constructor.SetExistingFwdCurves(fwdCurves);
            constructor.CalibrateAllCurves_AD(true);

            ObjectMap.FwdCurveCollections[baseName] = constructor.GetFwdCurves();
        }

        public static void DiscCurve_MakeFromCalibrationProblem(string baseName, string curveCalibrationProblem, string settingsName, bool useAd)
        {
            DiscCurveConstructor constructor = new DiscCurveConstructor(ObjectMap.CurveCalibrationProblems[curveCalibrationProblem], ObjectMap.CalibrationSettings[settingsName]);

            if (useAd)
                constructor.CalibrateCurveAd();
            else
                constructor.CalibrateCurve();

            ObjectMap.DiscCurves[baseName] = constructor.GetCurve();
        }
    }

    public static class InstrumentFactoryFunctions
    {
        public static void InstrumentFactory_UpdateAllInstrumentsToPar(string baseName, string modelHandle)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[modelHandle];
            InstrumentFactory factory = ObjectMap.InstrumentFactories[baseName];
            factory.UpdateAllInstrumentsToParGivenModel(model);
        }

        public static object[,] InstrumentFactory_GetInstrumentInfo(string baseName, string identifier)
        {
            return ConstructInstrumentInspector.MakeExcelOutput(ObjectMap.InstrumentFactories[baseName], identifier);
        }

        public static void InstrumentFactory_StoreInstrumentsInMap(string baseName)
        {
            InstrumentFactory factory = ObjectMap.InstrumentFactories[baseName];

            foreach (string key in factory.IrSwaps.Keys)
                ObjectMap.LinearRateInstruments[key] = factory.IrSwaps[key];

            foreach (string key in factory.OisSwaps.Keys)
                ObjectMap.LinearRateInstruments[key] = factory.OisSwaps[key];

            foreach (string key in factory.BasisSwaps.Keys)
                ObjectMap.LinearRateInstruments[key] = factory.BasisSwaps[key];

            foreach (string key in factory.Futures.Keys)
                ObjectMap.LinearRateInstruments[key] = factory.Futures[key];

            foreach (string key in factory.Fras.Keys)
                ObjectMap.LinearRateInstruments[key] = factory.Fras[key];
        }

        public static void InstrumentFactory_Make(string baseName, DateTime asOf)
        {
            ObjectMap.InstrumentFactories[baseName] = new InstrumentFactory(asOf);
        }

        public static double ValueInstrument(string instrument, string instrumentFactory, string linearRateModel)
        {
            return ObjectMap.InstrumentFactories[instrumentFactory].ValueInstrumentFromFactory(ObjectMap.LinearRateModels[linearRateModel], instrument);
        }
        
        // Functions to add instruments to instrumentfactory
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
            Futures future = ObjectMap.InstrumentFactories[instrumentFactory].Futures[instrument];
            return ObjectMap.LinearRateModels[model].ParFutureRate(future);
        }
    }

    public static class LinearRateFunctions
    {
        // ------- General Curve functionality

        public static double Curve_GetValue(string baseHandle, DateTime date, InterpMethod interpolation)
        {
            if (ObjectMap.DiscCurves.ContainsKey(baseHandle))
            {
                return ObjectMap.DiscCurves[baseHandle].Interp(date, interpolation);
            }
            else if (ObjectMap.FwdCurves.ContainsKey(baseHandle))
            {
                return ObjectMap.FwdCurves[baseHandle].Interp(date, interpolation);
            }
            else
                throw new InvalidOperationException("Curve " + baseHandle + " does not exist in the ObjectMap.");
        }

        public static double Curve_GetDiscFactor(string baseHandle, DateTime asOf, DateTime date, DayCount dayCount, InterpMethod interpolation)
        {
            if (ObjectMap.DiscCurves.ContainsKey(baseHandle))
            {
                return ObjectMap.DiscCurves[baseHandle].DiscFactor(asOf, date, dayCount, interpolation);
            }
            else if (ObjectMap.FwdCurves.ContainsKey(baseHandle))
            {
                return ObjectMap.DiscCurves[baseHandle].DiscFactor(asOf, date, dayCount, interpolation);
            }
            else
                throw new InvalidOperationException("Curve " + baseHandle + " does not exist in the ObjectMap.");
        }

        public static double Curve_GetFwdRate(string curveHandle, DateTime asOf, DateTime date, CurveTenor tenor, DayCount dayCount, DayRule dayRule, InterpMethod interpolation)
        {
            DateTime endDate = DateHandling.AddTenorAdjust(date, EnumToStr.CurveTenor(tenor), dayRule);

            if (ObjectMap.DiscCurves.ContainsKey(curveHandle))
            {
                Curve curve = ObjectMap.DiscCurves[curveHandle];
                return curve.FwdRate(asOf, date, endDate, dayRule, dayCount, interpolation);
            }
            else if (ObjectMap.FwdCurves.ContainsKey(curveHandle))
            {
                Curve curve = ObjectMap.FwdCurves[curveHandle];
                return curve.FwdRate(asOf, date, endDate, dayRule, dayCount, interpolation);
            }
            else
                throw new InvalidOperationException(curveHandle + " does not exist in the objectMap.");
        }

        public static void Curve_BumpCurveAndStore(string baseHandle, string originalCurveHandle, int curvePoint, double bumpSize)
        {
            Curve originalCurve, bumpedCurve;

            if (ObjectMap.DiscCurves.ContainsKey(originalCurveHandle))
            {
                originalCurve = ObjectMap.DiscCurves[originalCurveHandle];
                bumpedCurve = originalCurve.Copy();
                bumpedCurve.Values[curvePoint] += bumpSize;
                ObjectMap.DiscCurves[baseHandle] = bumpedCurve;
            }
            else if (ObjectMap.FwdCurves.ContainsKey(originalCurveHandle))
            {
                originalCurve = ObjectMap.FwdCurves[originalCurveHandle];
                bumpedCurve = originalCurve.Copy();
                bumpedCurve.Values[curvePoint] += bumpSize;
                ObjectMap.FwdCurves[baseHandle] = bumpedCurve;
            }
            else
                throw new InvalidOperationException("Curve " + originalCurveHandle + " does not exist in the ObjectMap.");
        }

        // ------- DISC CURVE FUNCTIONS
        public static void DiscCurve_Make(string baseName, List<DateTime> dates, List<double> values)
        {
                Curve output = new MasterThesis.Curve(dates, values);
                ObjectMap.DiscCurves[baseName] = output;
        }

        public static double DiscCurve_GetValue(string baseName, DateTime date, InterpMethod interpolation)
        {
            return ObjectMap.DiscCurves[baseName].Interp(date, interpolation);
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
                FwdCurveContainer fwdCurves = new FwdCurveContainer();

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

        public static void FwdCurveRepresentation_MakeFromFwdCurve(string baseName, string fwdCurveHandle, CurveTenor tenor, DateTime asOf, DayCount dayCount, DayRule dayRule, InterpMethod interpolation)
        {
            Curve fwdCurve = ObjectMap.FwdCurves[fwdCurveHandle];
            ObjectMap.FwdCurveRepresentations[baseName] = new FwdCurveRepresentation(fwdCurve, tenor, asOf, dayCount, dayRule, interpolation);
        }

        public static void FwdCurveRepresentation_MakeFromDiscCurve(string baseName, string discCurveHandle, CurveTenor tenor, DateTime asOf, DayCount dayCount, DayRule dayRule, InterpMethod interpolation)
        {
            Curve discCurve = ObjectMap.DiscCurves[discCurveHandle];
            ObjectMap.FwdCurveRepresentations[baseName] = new FwdCurveRepresentation(discCurve, tenor, asOf, dayCount, dayRule, interpolation);
        }

        public static object[,] FwdCurveRepresentation_Get(string handle)
        {
            ObjectMap.CheckExists(ObjectMap.DiscCurves, handle, "Disc Curve does not exist");

            return ExcelUtilities.BuildObjectArrayFromCurve(ObjectMap.FwdCurveRepresentations[handle].FwdCurve, handle);
        }

        public static double FwdCurve_GetValue(string baseName, DateTime date, InterpMethod interpolation)
        {
            return ObjectMap.FwdCurves[baseName].Interp(date, interpolation);
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
        public static double LinearRateModel_Value(string linearRateModelHandle, string productHandle)
        {
            LinearRateInstrument product = ObjectMap.LinearRateInstruments[productHandle];
            LinearRateModel model = ObjectMap.LinearRateModels[linearRateModelHandle];
            return model.ValueLinearRateProduct(product);
        }

        public static void LinearRateModel_Make(string baseName, string fwdCurveCollectionName, string discCurveName, InterpMethod interpolation)
        {
            FwdCurveContainer fwdCurves = ObjectMap.FwdCurveCollections[fwdCurveCollectionName];
            Curve discCurve = ObjectMap.DiscCurves[discCurveName];
            LinearRateModel model = new LinearRateModel(discCurve, fwdCurves, interpolation);
            ObjectMap.LinearRateModels[baseName] = model;
        }

        public static double LinearRateModel_SwapValue(string baseName, string swapName)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[baseName];
            IrSwap swap = (IrSwap) ObjectMap.LinearRateInstruments[swapName];
            //IrSwap swap = ObjectMap.IrSwaps[swapName];
            return model.IrSwapNpv(swap);
        }

        // .. Value functions
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
            IrSwap swap = (IrSwap) ObjectMap.LinearRateInstruments[swapName];
            //IrSwap swap = ObjectMap.IrSwaps[swapName];
            return model.IrParSwapRate(swap);
        }

        public static double LinearRateModel_BasisSwapValue(string baseName, string swapName)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[baseName];
            BasisSwap swap = (BasisSwap) ObjectMap.LinearRateInstruments[swapName];
            //BasisSwap swap = ObjectMap.BasisSwaps[swapName];
            return model.BasisSwapNpv(swap);
        }

        public static double LinearRateModel_BasisParSpread(string modelName, string basisSwapName)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[modelName];
            BasisSwap swap = (BasisSwap)ObjectMap.LinearRateInstruments[basisSwapName];
            //BasisSwap swap = ObjectMap.BasisSwaps[basisSwapName];
            return model.ParBasisSpread(swap);
        }

        public static double LinearRateModel_OisSwapNpv(string modelName, string oisSwapName)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[modelName];
            OisSwap swap = (OisSwap)ObjectMap.LinearRateInstruments[oisSwapName];
            return model.DiscCurve.OisSwapNpv(swap, InterpMethod.Hermite);
        }

        public static double LinearRateModel_OisRate(string modelName, string oisSwapName)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[modelName];
            OisSwap swap = (OisSwap)ObjectMap.LinearRateInstruments[oisSwapName];
            return model.DiscCurve.OisRateSimple(swap, InterpMethod.Hermite);
        }

        public static double LinearRateModel_OisRateComplex(string modelName, string oisSwapName)
        {
            LinearRateModel model = ObjectMap.LinearRateModels[modelName];
            OisSwap swap = (OisSwap)ObjectMap.LinearRateInstruments[oisSwapName];
            return model.DiscCurve.OisRate(swap, InterpMethod.Hermite);
        }

        // .. Constructors
        public static void FixedLeg_Make(string baseName, DateTime asOf, DateTime startDate, DateTime endDate, double fixedRate,
                        CurveTenor frequency, DayCount dayCount, DayRule dayRule, double notional, StubPlacement stub = StubPlacement.NullStub)
        {
            ObjectMap.FixedLegs[baseName] = new FixedLeg(asOf, startDate, endDate, fixedRate, frequency, dayCount, dayRule, notional);
        }

        public static void FloatLeg_Make(string baseName, DateTime asOf, DateTime startDate, DateTime endDate,
                        CurveTenor frequency, DayCount dayCount, DayRule dayRule, double notional, double spread, StubPlacement stub = StubPlacement.NullStub)
        {
            ObjectMap.FloatLegs[baseName] = new FloatLeg(asOf, startDate, endDate, frequency, dayCount, dayRule, notional, spread, stub);
        }

        public static void PlainVanillaSwap_Make(string baseName, string fixedLegName, string floatLegName, int tradeSign)
        {
            FixedLeg fixedLeg = ObjectMap.FixedLegs[fixedLegName];
            FloatLeg floatLeg = ObjectMap.FloatLegs[floatLegName];
            IrSwap swap = new MasterThesis.IrSwap(floatLeg, fixedLeg, tradeSign);

            ObjectMap.LinearRateInstruments[baseName] = swap;
            ObjectMap.IrSwaps[baseName] = swap;
        }

        public static void OisSwap_Make(string baseName, DateTime asOf, string startTenor, string endTenor, string settlementLag, DayCount dayCountFixed,
                DayCount dayCountFloat, DayRule dayRule, double notional, double fixedRate, int tradeSign)
        {
            ObjectMap.LinearRateInstruments[baseName] = new OisSwap(asOf, startTenor, endTenor, settlementLag, dayCountFixed, dayCountFloat, dayRule, notional, fixedRate, tradeSign);
        }

        public static void BasisSwap_Make(string baseName, string floatLegNoSpreadName, string floatLegSpreadName, int tradeSign)
        {
            FloatLeg floatLegNoSpread = ObjectMap.FloatLegs[floatLegNoSpreadName];
            FloatLeg floatLegSpread = ObjectMap.FloatLegs[floatLegSpreadName];
            BasisSwap swap = new MasterThesis.BasisSwap(floatLegNoSpread, floatLegSpread, tradeSign);

            ObjectMap.LinearRateInstruments[baseName] = swap;
            ObjectMap.BasisSwaps[baseName] = swap;
        }



    }
}
