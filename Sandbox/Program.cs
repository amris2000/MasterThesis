using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MasterThesis;

namespace Sandbox
{
    class Program
    {
        private static void DateTest()
        {
            Console.WriteLine("--------- Day roll test");
            DateTime MyDate = new DateTime(2017, 1, 27);
            DateTime MyDate2 = new DateTime(2025, 3, 27);
            Console.WriteLine("MyDate: " + Calender.AddTenor(MyDate, "5b", DayRule.F));
            Console.WriteLine("MyDate: " + Calender.AddTenor(MyDate, "5b", DayRule.MF));
            Console.WriteLine("MyDate: " + Calender.AddTenor(MyDate, "5b", DayRule.P));
            Console.WriteLine("--------- Coverage test");
            Console.WriteLine("ACT/360: " + Calender.Cvg(MyDate, MyDate2, DayCount.ACT360));
            Console.WriteLine("30/360: " + Calender.Cvg(MyDate, MyDate2, DayCount.THIRTY360));

            SwapSchedule MySchedule = new SwapSchedule(DateTime.Now, MyDate, MyDate2, DayCount.ACT360, DayRule.MF, CurveTenor.Fwd6M);
            MySchedule.Print();

            MySchedule = new SwapSchedule(DateTime.Now, MyDate, MyDate2, DayCount.THIRTY360, DayRule.MF, CurveTenor.Fwd6M);
            MySchedule.Print();

            SwapSchedule MySchedule2 = new SwapSchedule(DateTime.Now, MyDate, MyDate2, DayCount.THIRTY360, DayRule.MF, CurveTenor.Fwd1Y);
            MySchedule2.Print();

            Calender.PrintDateList(Calender.IMMSchedule(DateTime.Now, new DateTime(2040, 1, 1)), "IMM Dates");
        }
        private static void AADTest()
        {
            AADFunc.Func1(new ADouble(3.0), new ADouble(2.0), new ADouble(5.0));
            AADFunc.Func11(new ADouble(3.0), new ADouble(2.0), new ADouble(5.0));

            AADFunc.FuncExp(new ADouble(3));
            AADFunc.FuncLog(new ADouble(10));
            AADFunc.FuncDiv(new ADouble(1));
            AADFunc.FuncDiv2(new ADouble(2));
            AADFunc.FuncLog(new ADouble(5));
            AADFunc.FuncPow(new ADouble(5), 2.0);

            // http://www.math.drexel.edu/~pg/fin/VanillaCalculator.html
            AADFunc.BlackScholes(new ADouble(0.20), new ADouble(100.0), new ADouble(0.05), new ADouble (0.0), new ADouble(1.0), new ADouble(90.0));
            AADFunc.FuncDiv3(new ADouble(10), new ADouble(-2.5), 5.0);
        }
        private static void InterpolationTest()
        {

            double[] MyValues = new double[] { 11.2, 13.5, 17.2, 18.4, 20 };
            DateTime[] MyDates = new DateTime[] {   new DateTime(2017, 1, 10),
                                                    new DateTime(2017, 2, 10),
                                                    new DateTime(2017, 3, 10),
                                                    new DateTime(2017, 4, 10),
                                                    new DateTime(2017, 5, 10)};
            DateTime MyDate = new DateTime(2017, 1, 15);

            Console.WriteLine("Interpolation Test " + MyDate.ToString("dd/MM/yyyy") + ". Value: " + Maths.InterpolateCurve(MyDates, MyDate, MyValues, InterpMethod.Constant));
            Console.WriteLine("Interpolation Test " + MyDate.ToString("dd/MM/yyyy") + ". Value: " + Maths.InterpolateCurve(MyDates, MyDate, MyValues, InterpMethod.Linear));
            Console.WriteLine("Interpolation Test " + MyDate.ToString("dd/MM/yyyy") + ". Value: " + Maths.InterpolateCurve(MyDates, MyDate, MyValues, InterpMethod.Hermite));
            Console.WriteLine("Interpolation Test " + MyDate.ToString("dd/MM/yyyy") + ". Value: " + Maths.InterpolateCurve(MyDates, MyDate, MyValues, InterpMethod.LogLinear));
        }
        private static double DelegateTest(double x, double y, double z)
        {
            return x + y + z;
        }
        delegate double Del(double x, double y);
        public static void TestIt()
        {
            Del MyDeletegate = (x, y) => DelegateTest(x, y, 0.2);
            Console.WriteLine(MyDeletegate(2.0, 3.0));
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

        public static void DayCompoundingTest()
        {

            DateTime AsOf = new DateTime(2017, 1, 15);
            DateTime Start = new DateTime(2017, 1, 31);
            DateTime End = new DateTime(2017, 2, 19);
            DayRule DayRule = DayRule.F;
            DayCount DayCount = DayCount.ACT360;

            DateTime Temp = Start;
            double Compound = 1;

            while (Temp<End)
            {
                double Rate = 0.1;
                DateTime NewDate = Calender.AddTenor(Temp, "1B", DayRule);
                double Days = NewDate.Subtract(Temp).TotalDays;

                Console.WriteLine("Day: " + Temp.DayOfWeek + " to " + NewDate.DayOfWeek + ". Days: " + Days);
                Temp = NewDate;
                Compound *= (1 + Rate * Days/365);
                Console.Write("  .. Compound: " + Compound + " . ");
            }

            Compound = (Compound - 1) / (Calender.Cvg(Start, End, DayCount));
            Console.WriteLine("Compound: " + Compound);



            OisSchedule Schedule1 = new OisSchedule(AsOf, Start, DayCount.ACT360, DayRule.MF, "5B");
            Schedule1.Print();

            DateTime End2 = Calender.AddTenor(Start, "68M");
            SwapSchedule SwapSchedule = new SwapSchedule(AsOf, Start, End2, DayCount.ACT360, DayRule.MF, CurveTenor.Fwd6M, StubPlacement.Beginning);
            SwapSchedule.Print();
            SwapSchedule SwapSchedule2 = new SwapSchedule(AsOf, Start, End2, DayCount.ACT360, DayRule.MF, CurveTenor.Fwd6M, StubPlacement.End);
            SwapSchedule2.Print();



        }

        public static void BootStrappingTest()
        {

            var sw = new Stopwatch();

            sw.Start();
            DateTime AsOf = new DateTime(2017, 1, 31);
            List<RawMarketData> rawMarketData = new List<RawMarketData>();

            //rawMarketData.Add(new RawMarketData(AsOf, "R6M", "FIXING", "6M", -0.001));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E6M", "SWAP", "6M", -0.0015)); // Synthetic
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E1Y", "SWAP", "6M", -0.00216));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E18M", "SWAP", "6M", -0.001819));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E2Y", "SWAP", "6M", -0.001436));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E3Y", "SWAP", "6M", -0.000439));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E4Y", "SWAP", "6M", -0.000762));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E5Y", "SWAP", "6M", 0.0021));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E6Y", "SWAP", "6M", 0.003439));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E7Y", "SWAP", "6M", 0.004761));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E8Y", "SWAP", "6M", 0.006078));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E9Y", "SWAP", "6M", 0.007319));

            List<MarketQuote> Quotes = QuoteFactory.CreateMarketQuoteCollection(rawMarketData);

            CurveFactory Factory = new CurveFactory(Quotes, CurveTenor.DiscOis);
            Curve MyCurve = Factory.BootstrapCurve();

            sw.Stop();
            Console.WriteLine("Run-time: " + sw.ElapsedMilliseconds);

            List<RawMarketData> OisBootstrap = new List<RawMarketData>();

            OisBootstrap.Add(new RawMarketData(AsOf, "EUREONON", "SWAP", "OIS", -0.0035));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREONTN", "SWAP", "OIS", -0.0035));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREONSW", "SWAP", "OIS", -0.00352));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON2W", "SWAP", "OIS", -0.00352));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON3W", "SWAP", "OIS", -0.00352));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON1M", "SWAP", "OIS", -0.00351));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON2M", "SWAP", "OIS", -0.0035));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON3M", "SWAP", "OIS", -0.0035));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON4M", "SWAP", "OIS", -0.0035));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON5M", "SWAP", "OIS", -0.00348));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON6M", "SWAP", "OIS", -0.003471));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON7M", "SWAP", "OIS", -0.00345));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON8M", "SWAP", "OIS", -0.00343));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON9M", "SWAP", "OIS", -0.00341));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON10M", "SWAP", "OIS", -0.00326));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON11M", "SWAP", "OIS", -0.00304));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON1Y", "SWAP", "OIS", -0.002273));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON18M", "SWAP", "OIS", -0.001282));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON2Y", "SWAP", "OIS", -0.000112));

            List<MarketQuote> QuotesOis = QuoteFactory.CreateMarketQuoteCollection(OisBootstrap);

            CurveFactory FactoryOis = new CurveFactory(QuotesOis, CurveTenor.DiscOis);
            Curve MyCurve2 = FactoryOis.BootstrapCurve();

        }
        static void Main(string[] args)
      {
            //DateTest();
            //InterpolationTest();
            //AADTest();
            //ModelTesting();

            BootStrappingTest();

            //DayCompoundingTest();

            TestIt();
           
            Console.ReadLine();
        }
    }
}