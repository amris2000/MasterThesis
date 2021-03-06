﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
    /* --- General information
     * 
     */

    public static class AADTestFunctions
    {
        public static void BlackScholes()
        {
            AADTape.Initialize();
            BlackScholes(new ADouble(0.20), new ADouble(100.0), new ADouble(0.05), new ADouble(0.0), new ADouble(1.0), new ADouble(90.0));
        }

        public static void BlackScholes(ADouble vol, ADouble spot, ADouble rate, ADouble time, ADouble mat, ADouble strike)
        {
            AADTape.ResetTape();
            AADTape.Initialize(new ADouble[] { vol, spot, rate, time, mat, strike });

            ADouble Help1 = vol * ADouble.Sqrt(mat - time);
            ADouble d1 = 1.0 / Help1 * (ADouble.Log(spot / strike) + (rate + 0.5 * ADouble.Pow(vol, 2)) * (mat - time));
            ADouble d2 = d1 - vol * ADouble.Sqrt(mat - time);
            ADouble Out = MyMath.NormalCdf(d1) * spot - strike * ADouble.Exp(-rate * (mat - time)) * MyMath.NormalCdf(d2);

            Console.WriteLine("");
            Console.WriteLine("BLACK-SCHOLES TEST. Value: " + Out.Value);
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        public static void BlackScholesNoReset(ADouble vol, ADouble spot, ADouble rate, ADouble time, ADouble mat, ADouble strike)
        {
            ADouble Help1 = vol * ADouble.Sqrt(mat - time);
            ADouble d1 = 1.0 / Help1 * (ADouble.Log(spot / strike) + (rate + 0.5 * ADouble.Pow(vol, 2)) * (mat - time));
            ADouble d2 = d1 - vol * ADouble.Sqrt(mat - time);
            ADouble Out = MyMath.NormalCdf(d1) * spot - strike * ADouble.Exp(-rate * (mat - time)) * MyMath.NormalCdf(d2);
        }

        public static void Func1(ADouble x, ADouble y, ADouble z)
        {
            AADTape.ResetTape();
            AADTape.Initialize(new ADouble[] { x, y, z });

            // Derivative
            //      Fx(x,y,z) = y*z + 1 + y
            //      Fy(x,y,z) = x*z - 1 + x
            //      Fz(x,y,z) = x*y

            ADouble Out = x * y * z + x - y + x * y;
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        public static void FuncLog(ADouble x)
        {
            AADTape.ResetTape();
            AADTape.Initialize(new ADouble[] { x });

            // Derivative: Fx(x) = 3 + 3/x

            ADouble Temp = 3.0 * x + 3.0 * ADouble.Log(x * 4.0) + 50.0;
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        public static void FuncExp(ADouble x)
        {
            AADTape.ResetTape();
            AADTape.Initialize(new ADouble[] { x });

            // Derivative: Fx(x) = 3 + 3*exp(3*x)

            ADouble Temp = 3.0 * x + ADouble.Exp(3.0 * x) + 50.0;
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        public static void Func11(ADouble x, ADouble y, ADouble z)
        {
            AADTape.ResetTape();
            AADTape.Initialize(new ADouble[] { x, y, z });

            ADouble Out1, Out2, Out3;
            Out1 = x * y * z;
            Out2 = x * y;
            Out3 = Out1 + x - y + Out2;
            Out3 = Out3 * 3.0;
            Out3 = 2.0 * Out3;
            Out3 = 10.0 + Out3;
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        public static void FuncDiv(ADouble x)
        {
            AADTape.ResetTape();
            AADTape.Initialize(new ADouble[] { x });

            ADouble Out = x * x * x / (3.0 + 2.0 * x);
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        public static void FuncDiv2(ADouble x)
        {
            AADTape.ResetTape();
            AADTape.Initialize(new ADouble[] { x });

            ADouble Out = 1.0 / x;
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        public static void FuncDiv3(ADouble x1, ADouble x2, double K)
        {

            AADTape.ResetTape();
            AADTape.Initialize(new ADouble[] { x1, x2 });

            ADouble Out = x1 / x2 + K / x2 + K / x1 + x1 / K + x2 / K;
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        public static void FuncPow(ADouble x, double k)
        {
            AADTape.ResetTape();
            AADTape.Initialize(new ADouble[] { x });

            ADouble Out = 3.0 * ADouble.Log(x) + 5.0 * ADouble.Pow(x, k);
            Console.WriteLine(" ");
            Console.WriteLine("Testing Adjoint differentiation of a function involving powers");
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        // Testing the pow function
        public static ADouble TestingPow(ADouble x)
        {
            return ADouble.Pow(x - 5.0, 2.0);
        }

        public static ADouble TestingPowInverse(ADouble x)
        {
            return ADouble.Pow(5.0 - x, 2.0);
        }

        // For example in thesis
        public static ADouble ExampleFunctionThesis(ADouble x1, ADouble x2)
        {
            return 2.0 * x1 * x2 + ADouble.Log(x1 - 4.0 * x2);
        }
    }
}
