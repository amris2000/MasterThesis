using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis;

namespace Sandbox
{
    public static class AADTests
    {
        public static void CollectionOfSimpleFunctionsTest()
        {
            AADTestFunctions.Func1(new ADouble(3.0), new ADouble(2.0), new ADouble(5.0));
            AADTestFunctions.Func11(new ADouble(3.0), new ADouble(2.0), new ADouble(5.0));
            AADTestFunctions.FuncExp(new ADouble(3));
            AADTestFunctions.FuncLog(new ADouble(10));
            AADTestFunctions.FuncDiv(new ADouble(1));
            AADTestFunctions.FuncDiv2(new ADouble(2));
            AADTestFunctions.FuncLog(new ADouble(5));
            AADTestFunctions.FuncPow(new ADouble(5), 2.0);

            // http://www.math.drexel.edu/~pg/fin/VanillaCalculator.html
            AADTestFunctions.BlackScholes(new ADouble(0.20), new ADouble(100.0), new ADouble(0.05), new ADouble(0.0), new ADouble(1.0), new ADouble(90.0));
            AADTestFunctions.FuncDiv3(new ADouble(10), new ADouble(-2.5), 5.0);
            

        }

        public static void GoalFunctionTest()
        {
            AADTape.ResetTape();

            ADouble x1 = 10.0;
            List<ADouble> activeVariables = new List<ADouble>();
            activeVariables.Add(x1);

            // Initialize tape with x1 and x2
            AADTape.Initialize(activeVariables.ToArray());

            // Compute the function value of f. Tape is now running
            ADouble result = AADTestFunctions.TestingPow(x1);

            // Once complete, interpret the tape
            AADTape.InterpretTape();
            AADTape.PrintTape();

            AADTape.ResetTape();
            ///////////

            activeVariables = new List<ADouble>();
            activeVariables.Add(x1);

            // Initialize tape with x1 and x2
            AADTape.Initialize(activeVariables.ToArray());

            // Compute the function value of f. Tape is now running
            ADouble result2 = AADTestFunctions.TestingPowInverse(x1);

            // Once complete, interpret the tape
            AADTape.InterpretTape();
            AADTape.PrintTape();

        }

        public static void CalculateDerivativesByAd()
        {
            Console.Title = "AD Example.";
            ADouble x1 = 10.0;
            ADouble x2 = 2.0;
            List<ADouble> activeVariables = new List<ADouble>();
            activeVariables.Add(x1);
            activeVariables.Add(x2);

            string[] identifiers = new string[] { "x1", "x2" };

            // Initialize tape with x1 and x2
            AADTape.Initialize(activeVariables.ToArray(), identifiers);

            // Compute the function value of f. Tape is now running
            ADouble result = AADTestFunctions.ExampleFunctionThesis(x1, x2);

            // Once complete, interpret the tape
            AADTape.InterpretTape();
            AADTape.PrintTape();

            var gradient = AADTape.GetGradient();
        }

        public static void ResultSetTestOnBlackScholes()
        {
            ADouble vol = 0.2;
            ADouble spot = 100.0;
            ADouble rate = 0.05;
            ADouble time = 0.0;
            ADouble mat = 1.0;
            ADouble strike = 90.0;

            ADouble[] parameters = new ADouble[] { vol, spot, rate, time, mat, strike };
            //List<Ref<ADouble>> parameters = new List<Ref<ADouble>>();
            //parameters.Add(new Ref<ADouble> { Value = vol });
            //parameters.Add(new Ref<ADouble> { Value = spot });
            //parameters.Add(new Ref<ADouble> { Value = rate });
            //parameters.Add(new Ref<ADouble> { Value = time });
            //parameters.Add(new Ref<ADouble> { Value = mat });
            //parameters.Add(new Ref<ADouble> { Value = strike });

            string[] identifiers = new string[] { "Vol", "Spot", "Rate", "Time0", "Mat", "Strike" };

            AADTape.Initialize(parameters, identifiers);
            AADTestFunctions.BlackScholesNoReset(vol,spot,rate,time,mat,strike);
            AADTape.InterpretTape();
            AADTape.PrintResultSet();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }
    }
}
