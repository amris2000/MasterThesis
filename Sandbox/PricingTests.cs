using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MasterThesis;

namespace Sandbox
{
    public static class PricingTests
    {

        public static void OisSwapPricingTest()
        {
            ExcelUtility.DataReader();
            ExcelUtility.LoadCurvesFromFile();

            FwdCurves fwdCurves = Store.FwdCurveCollections["MYCURVES"];
            Curve discCurve = Store.Curves[CurveTenor.DiscOis];
            discCurve = fwdCurves.GetCurve(CurveTenor.Fwd1D);

            LinearRateModel model = new LinearRateModel(discCurve, fwdCurves);

            DayCount dayCountFixed = DayCount.THIRTY360;
            DayCount dayCountFloat = DayCount.ACT360;
            DayRule dayRuleFixed = DayRule.MF;
            DayRule dayRuleFloat = DayRule.MF;

            DateTime asof = new DateTime(2017, 1, 31);
            OisSwap euron = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "1B", 0.04, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eurtn = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "2B", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eursw = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "1W", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur2w = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "2W", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur3w = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "3W", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur1m = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "1M", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur2m = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "2M", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur3m = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "3M", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur4m = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "4M", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur5m = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "5M", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur6m = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "6M", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur7m = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "7M", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur8m = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "8M", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur9m = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "9M", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur10m = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "10M", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur11m = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "11M", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur1y = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "1Y", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur18m = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "18M", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur2y = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "2Y", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur3y = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "3Y", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur4y = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "4Y", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur5y = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "5Y", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur6y = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "6Y", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur7y = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "7Y", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur8y = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "8Y", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur9y = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "9Y", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);
            OisSwap eur10y = new OisSwap(asof, Calender.AddTenor(asof, "2b", DayRule.F), "10Y", 0.01, dayCountFixed, dayCountFloat, dayRuleFixed, dayRuleFloat, 1);


            MasterThesis.Extensions.Logging.WriteSectionHeader("OIS swap pricing test using Nordea curves");

            Console.WriteLine("euron: " + model.OisRate(euron));
            Console.WriteLine("eurtn: " + model.OisRate(eurtn));
            Console.WriteLine("eursw: " + model.OisRate(eursw));
            Console.WriteLine("eur2w: " + model.OisRate(eur2w));
            Console.WriteLine("eur3w: " + model.OisRate(eur3w));
            Console.WriteLine("eur1m: " + model.OisRate(eur1m));
            Console.WriteLine("eur2m: " + model.OisRate(eur2m));
            Console.WriteLine("eur3m: " + model.OisRate(eur3m));
            Console.WriteLine("eur4m: " + model.OisRate(eur4m));
            Console.WriteLine("eur5m: " + model.OisRate(eur5m));
            Console.WriteLine("eur6m: " + model.OisRate(eur6m));
            Console.WriteLine("eur7m: " + model.OisRate(eur7m));
            Console.WriteLine("eur8m: " + model.OisRate(eur8m));
            Console.WriteLine("eur9m: " + model.OisRate(eur9m));
            Console.WriteLine("eur10m: " + model.OisRate(eur10m));
            Console.WriteLine("eur11m: " + model.OisRate(eur11m));
            Console.WriteLine("eur1y: " + model.OisRate(eur1y));
            Console.WriteLine("eur18m: " + model.OisRate(eur18m));
            Console.WriteLine("eur2y: " + model.OisRate(eur2y));
            Console.WriteLine("eur3y: " + model.OisRate(eur3y));
            Console.WriteLine("eur4y: " + model.OisRate(eur4y));
            Console.WriteLine("eur5y: " + model.OisRate(eur5y));
            Console.WriteLine("eur6y: " + model.OisRate(eur6y));
            Console.WriteLine("eur6y simple: " + model.OisRateSimple(eur6y));
            Console.WriteLine("eur6y simple: " + model.OisRateSimple2(eur6y));
            Console.WriteLine("eur7y: " + model.OisRate(eur7y));
            Console.WriteLine("eur8y: " + model.OisRate(eur8y));
            Console.WriteLine("eur9y: " + model.OisRate(eur9y));
            Console.WriteLine("eur10y: " + model.OisRate(eur10y));

        }

        public static void ModelTesting()
        {
            Stopwatch sv = new Stopwatch();
            sv.Start();
            ExcelUtility.DataReader();
            ExcelUtility.LoadCurvesFromFile();
            Console.WriteLine("LOAD - Elapsed time: " + sv.ElapsedMilliseconds);

            sv.Reset();
            sv.Start();
            LinearRateModel MyModel = new LinearRateModel(Store.Curves[CurveTenor.DiscOis], Store.FwdCurveCollections["MYCURVES"]);
            Console.WriteLine("Model - Elapsed time: " + sv.ElapsedMilliseconds);

            sv.Reset();
            sv.Start();

            DateTime AsOf = new DateTime(2017, 1, 31);
            DateTime StartDate = Calender.AddTenor(AsOf, "2B", DayRule.MF);
            DateTime EndDate = Calender.AddTenor(AsOf, "4Y", DayRule.MF);

            CurveTenor Float = CurveTenor.Fwd6M;
            CurveTenor Fixed = CurveTenor.Fwd1Y;


            SwapSimple MySwap = new SwapSimple(AsOf, StartDate, EndDate, 0.01, Fixed, Float, DayCount.THIRTY360, DayCount.ACT360, DayRule.MF, DayRule.MF, 1000000.0);

            double SwapRate = MyModel.SwapRate(MySwap);
            MySwap = new SwapSimple(AsOf, StartDate, EndDate, SwapRate, Fixed, Float, DayCount.THIRTY360, DayCount.ACT360, DayRule.MF, DayRule.MF, 1000000.0);

            IrSwap IrSwap = new IrSwap(AsOf, StartDate, EndDate, 0.01, Fixed, Float, DayCount.THIRTY360, DayCount.ACT360, DayRule.MF, DayRule.MF, 1000000.0);
            double SwapRate2 = MyModel.IrParSwapRate(IrSwap);

            //for (int i = 0; i < 100; i++)
            //{
            //    MySwap = new Swap(DateTime.Now.Date, new DateTime(2017, 2, 28), new DateTime(2021, 2, 28),
            //        0.01, "1Y", "6M", "30/360", "ACT/360", "MF", "MF", 1000000.0);
            //    Value = MyModel.ValueInstrument(MySwap);
            //}
          
            Console.WriteLine("Par swap rate: " + Math.Round(SwapRate * 100, 4) + "%");
            Console.WriteLine("Par swap rate: " + Math.Round(SwapRate2 * 100, 4) + "%");
            Console.WriteLine("Price Swap - Elapsed time: " + sv.ElapsedMilliseconds);

            Console.WriteLine(" ");
            Console.WriteLine(" PARSING MARKET DATA INSTRUMENT ");
            OldMarketDataQuote MyQuote = new OldMarketDataQuote("EURAB6E22Y", "SWAP", "6M");
            Console.WriteLine(MyQuote.InstrumentString);
            MyQuote = new OldMarketDataQuote("EUREON11Y", "SWAP", "1D");
            Console.WriteLine(MyQuote.InstrumentString);

            RawMarketData Data = new RawMarketData(AsOf, "EURAB1E18M", "SWAP", "1M", 0.02);
            MarketQuote SwapQuote = QuoteFactory.CreateMarketQuote(Data);

            RawMarketData Data2 = new RawMarketData(AsOf, "EUR12X1S", "SWAP", "1M", -0.003);
            MarketQuote SwapQuote2 = QuoteFactory.CreateMarketQuote(Data2);

            Console.WriteLine("IMM: " + Calender.NextIMMDate(new DateTime(2019, 3, 1)).DayOfWeek);
            Console.WriteLine("IMM: " + Calender.IMMDate(2019, 4).DayOfWeek);
            Console.WriteLine("Press anything to exit...");
        }

        public static void TestOisSwap()
        {
            Stopwatch sv = new Stopwatch();
            sv.Start();
            ExcelUtility.DataReader();
            ExcelUtility.LoadCurvesFromFile();
            Console.WriteLine("LOAD - Elapsed time: " + sv.ElapsedMilliseconds);

            sv.Reset();
            sv.Start();
            LinearRateModel MyModel = new LinearRateModel(Store.Curves[CurveTenor.DiscOis], Store.FwdCurveCollections["MYCURVES"]);
            Console.WriteLine("Model - Elapsed time: " + sv.ElapsedMilliseconds);

            sv.Reset();
            sv.Start();

            DateTime AsOf = new DateTime(2017, 2, 15);
            DateTime StartDate = Calender.AddTenor(AsOf, "2B", DayRule.F);
            DateTime EndDate = Calender.AddTenor(AsOf, "4Y", DayRule.MF);

            var OisSwap = new OisSwap(AsOf, StartDate, "9Y", 0.02, DayCount.ACT360, DayCount.THIRTY360, DayRule.MF, DayRule.MF, 1000000.0);

            MyModel.OisRate(OisSwap);
            Console.WriteLine("OisRate: " + MyModel.OisRate(OisSwap));

        }
    }
}
