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
using MathNet.Numerics.LinearAlgebra;

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

        public static ADouble f(ADouble x1, ADouble x2)
        {
            return 2.0 * x1 * x2 + ADouble.Log(x1 - 4.0 * x2);
        }

        public static void CalculateDerivativesByAd()
        {
            ADouble x1 = 10.0;
            ADouble x2 = 2.0;
            List<ADouble> activeVariables = new List<ADouble>();
            activeVariables.Add(x1);
            activeVariables.Add(x2);

            string[] identifiers = new string[] { "x1", "x2" };

            // Initialize tape with x1 and x2
            AADTape.Initialize(activeVariables.ToArray(), identifiers);

            // Compute the function value of f. Tape is now running
            ADouble result = f(x1, x2);

            // Once complete, interpret the tape
            AADTape.InterpretTape();
            AADTape.PrintTape();

            var gradient = AADTape.GetGradient();
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
            //testInstrumentOutput();       

            //// Matrix test
            //Matrix<double> myMat = Matrix<double>.Build.Dense(2, 2);
            //myMat[0, 0] = 3;
            //myMat[0, 1] = 1;
            //myMat[1, 0] = 2;
            //myMat[1, 1] = 4;
            //Matrix<double> inverse = myMat.Inverse();

            CalculateDerivativesByAd();

            //MiscTests.TestIt();
            //MultiThreadingTests.SimpleTest();
            Console.ReadLine();
        }
    }
}