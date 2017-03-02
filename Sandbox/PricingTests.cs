﻿using System;
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

            double Value = MyModel.Value(MySwap);

            double SwapRate = MyModel.SwapRate(MySwap);
            MySwap = new SwapSimple(AsOf, StartDate, EndDate, SwapRate, Fixed, Float, DayCount.THIRTY360, DayCount.ACT360, DayRule.MF, DayRule.MF, 1000000.0);
            double ValueSwapRate = MyModel.Value(MySwap);

            IrSwap IrSwap = new IrSwap(AsOf, StartDate, EndDate, 0.01, Fixed, Float, DayCount.THIRTY360, DayCount.ACT360, DayRule.MF, DayRule.MF, 1000000.0);
            double SwapRate2 = MyModel.IrParSwapRate(IrSwap);

            //for (int i = 0; i < 100; i++)
            //{
            //    MySwap = new Swap(DateTime.Now.Date, new DateTime(2017, 2, 28), new DateTime(2021, 2, 28),
            //        0.01, "1Y", "6M", "30/360", "ACT/360", "MF", "MF", 1000000.0);
            //    Value = MyModel.ValueInstrument(MySwap);
            //}

            Console.WriteLine("SwapValue: " + Value);
            Console.WriteLine("Par swap rate: " + Math.Round(SwapRate * 100, 4) + "%");
            Console.WriteLine("Par swap rate: " + Math.Round(SwapRate2 * 100, 4) + "%");
            Console.WriteLine("SwapValue at par: " + ValueSwapRate);
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
