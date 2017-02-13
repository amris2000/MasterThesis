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


        static void Main(string[] args)
       {
            //DateTest();
            //InterpolationTest();
            AADTest();

            Stopwatch sv = new Stopwatch();
            sv.Start();
            ExcelUtility.DataReader();
            ExcelUtility.LoadCurvesFromFile();
            Console.WriteLine("LOAD - Elapsed time: " + sv.ElapsedMilliseconds);

            sv.Reset();
            sv.Start();
            LinearRateModelAdvanced MyModel = new LinearRateModelAdvanced(Store.FwdCurveCollections["MYCURVES"], Store.DiscCurves["LIBOR"]);
            Console.WriteLine("Model - Elapsed time: " + sv.ElapsedMilliseconds);

            sv.Reset();
            sv.Start();

            DateTime AsOf = new DateTime(2017, 1, 31);
            DateTime StartDate = Calender.AddTenor(AsOf, "2B", DayRule.MF);
            DateTime EndDate = Calender.AddTenor(AsOf, "9Y", DayRule.MF);

            CurveTenor Float = CurveTenor.Fwd6M;
            CurveTenor Fixed = CurveTenor.Fwd1Y;


            SwapSimple MySwap = new SwapSimple(AsOf, StartDate, EndDate, 0.01, Fixed, Float, DayCount.THIRTY360, DayCount.ACT360, DayRule.MF, DayRule.MF, 1000000.0);

            double Value = MyModel.ValueInstrument(MySwap);

            double SwapRate = MyModel.SwapRate(MySwap);
            MySwap = new SwapSimple(AsOf, StartDate, EndDate, SwapRate, Fixed, Float, DayCount.THIRTY360, DayCount.ACT360, DayRule.MF, DayRule.MF, 1000000.0);
            double ValueSwapRate = MyModel.ValueInstrument(MySwap);

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

            RawMarketData Data = new RawMarketData("EURAB1E18M", "SWAP", "1M", 0.02);
            MarketQuote SwapQuote = QuoteFactory.CreateMarketQuote(Data);

            Console.WriteLine("IMM: " + Calender.NextIMMDate(new DateTime(2019, 3, 1)).DayOfWeek);
            Console.WriteLine("IMM: " + Calender.IMMDate(2019, 4).DayOfWeek);
            Console.WriteLine("Press anything to exit...");
            Console.ReadLine();
        }
    }
}