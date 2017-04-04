using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MasterThesis;
using System.Threading;
using MasterThesis.Extensions;

namespace Sandbox
{
    class Program
    {
      static void Main(string[] args)
      {
            //CalenderTests.DateTest();
            //CalenderTests.DayCompoundingTest();

            //MathTests.InterpolationTest();
            //AADTests.AADTest();

            //CurveCalibrationTests.SimpleBootStrap();
            //CurveCalibrationTests.CurvesFromFile();
            //CurveCalibrationTests.OisBootStrap();

            //PricingTests.ModelTesting();
            //PricingTests.TestOisSwap();
            //PricingTests.OisSwapPricingTest();

            //Console.WriteLine(MiscTests.StringIsDate("15-Sep-26"));

            AADTests.ResultSetTest();
            AADFunc.BlackScholes();

            

            //MiscTests.TestIt();
            //MultiThreadingTests.SimpleTest();

            Console.ReadLine();
        }
    }
}