using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis;

namespace Sandbox
{
    public static class MathTests
    {
        public static void InterpolationTest()
        {

            double[] MyValues = new double[] { 11.2, 13.5, 17.2, 18.4, 20 };
            DateTime[] MyDates = new DateTime[] {   new DateTime(2017, 1, 10),
                                                    new DateTime(2017, 2, 10),
                                                    new DateTime(2017, 3, 10),
                                                    new DateTime(2017, 4, 10),
                                                    new DateTime(2017, 5, 10)};
            DateTime MyDate = new DateTime(2017, 1, 15);
        }
    }
}
