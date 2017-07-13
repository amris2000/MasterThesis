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
      static void Main(string[] args)
      {
            //AADTests.ResultSetTestOnBlackScholes();
            //AADTests.CollectionOfSimpleFunctionsTest();
            AADTests.CalculateDerivativesByAd();
            //AADTests.GoalFunctionTest();
            Console.ReadLine();
        }
    }
}