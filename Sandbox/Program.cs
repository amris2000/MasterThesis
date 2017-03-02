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

            //PricingTests.ModelTesting();
            //PricingTests.TestOisSwap();

            //CurveCalibrationTests.SimpleBootStrap();
            //CurveCalibrationTests.OisBootStrap();

            //MiscTests.TestIt();
            MultiThreadingTests.SimpleTest();

            Console.ReadLine();
        }
    }
}