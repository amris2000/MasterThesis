using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDna.Integration;

namespace ExcelApplication
{
    public class TestFunctions
    {
        [ExcelFunction(Description = "My First function in Excel", Name = "xx.MyFunc1")]
        public static double MyFunction1(double x1, double x2)
        {
            return x1 * x2;
        }

        [ExcelFunction(Description = "My First function in Excel", Name = "xx.MyFunc2")]
        public static double MyFunction2(double x1, double x2)
        {
            return 100*x1 * x2;
        }

    }
}
