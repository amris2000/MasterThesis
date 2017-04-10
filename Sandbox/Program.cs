using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MasterThesis;
using System.Threading;
using MasterThesis.Extensions;
using MasterThesis.ExcelInterface;

namespace Sandbox
{
    class Program
    {

       public static void Schedule123()
        {
            DateTime asOf = new DateTime(2016, 1, 1);
            DateTime startDate = new DateTime(2016, 9, 1);
            DateTime endDate = new DateTime(2017, 3, 1);
            SwapSchedule mySchedule = new SwapSchedule(asOf, startDate, endDate, DayCount.ACT360, DayRule.MF, CurveTenor.Fwd3M);

            // 40 years => 80 periods.

            mySchedule.Print();
            Console.WriteLine("");
        }

        public static void testInstrumentOutput()
        {
            string instrumentString = "EURAB6E29Y,SWAP,EUR,0D,29Y,2B,MF,EUR,EUR,1Y,6M,6M,6M_EURIBOR,30/360,ACT/360";
            InstrumentFactory factory = new InstrumentFactory(DateTime.Today);
            factory.AddSwaps(new string[] { instrumentString });

            object[,] output = ConstructInstrumentInspector.MakeExcelOutput(factory, "EURAB6E29Y");
            Console.Write("");

        }

      static void Main(string[] args)
      {
            //CalenderTests.DateTest();
            //CalenderTests.DayCompoundingTest();

            //MathTests.InterpolationTest();
            //AADTests.AADTest();

            //CurveCalibrationTests.SimpleBootStrap();
            //CurveCalibrationTests.CurvesFromFile();
            //CurveCalibrationTests.s();

            //PricingTests.ModelTesting();
            //PricingTests.TestOisSwap();
            //PricingTests.OisSwapPricingTest();

            //Console.WriteLine(MiscTests.StringIsDate("15-Sep-26"));

            //AADTests.ResultSetTest();
            //AADFunc.BlackScholes();

            //ADouble myDouble = 2.0;
            //double x = myDouble;

            //Schedule123();
            testInstrumentOutput();


            //MiscTests.TestIt();
            //MultiThreadingTests.SimpleTest();

            Console.ReadLine();
        }
    }
}