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
        public static void AADTest()
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
            AADFunc.BlackScholes(new ADouble(0.20), new ADouble(100.0), new ADouble(0.05), new ADouble(0.0), new ADouble(1.0), new ADouble(90.0));
            AADFunc.FuncDiv3(new ADouble(10), new ADouble(-2.5), 5.0);
        }

        public static void ResultSetTest()
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
            AADFunc.BlackScholesNoReset(vol,spot,rate,time,mat,strike);
            AADTape.InterpretTape();
            AADTape.PrintResultSet();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }
    }
}
