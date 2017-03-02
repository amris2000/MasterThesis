using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    public class MiscTests
    {
        private static double DelegateTest(double x, double y, double z)
        {
            return x + y + z;
        }
        delegate double Del(double x, double y);
        public static void TestIt()
        {
            Del MyDeletegate = (x, y) => DelegateTest(x, y, 0.2);
            Console.WriteLine(MyDeletegate(2.0, 3.0));
        }
    }
}
