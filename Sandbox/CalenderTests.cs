using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MasterThesis;

namespace Sandbox
{
    public class CalenderTests
    {
        public static void DateTest()
        {
            Console.WriteLine("--------- Day roll test");
            DateTime MyDate = new DateTime(2017, 1, 27);
            DateTime MyDate2 = new DateTime(2025, 3, 27);
            Console.WriteLine("MyDate: " + DateHandling.AddTenorAdjust(MyDate, "5b", DayRule.F));
            Console.WriteLine("MyDate: " + DateHandling.AddTenorAdjust(MyDate, "5b", DayRule.MF));
            Console.WriteLine("MyDate: " + DateHandling.AddTenorAdjust(MyDate, "5b", DayRule.P));
            Console.WriteLine("--------- Coverage test");
            Console.WriteLine("ACT/360: " + DateHandling.Cvg(MyDate, MyDate2, DayCount.ACT360));
            Console.WriteLine("30/360: " + DateHandling.Cvg(MyDate, MyDate2, DayCount.THIRTY360));

            SwapSchedule MySchedule = new SwapSchedule(DateTime.Now, MyDate, MyDate2, DayCount.ACT360, DayRule.MF, CurveTenor.Fwd6M);
            MySchedule.Print();

            MySchedule = new SwapSchedule(DateTime.Now, MyDate, MyDate2, DayCount.THIRTY360, DayRule.MF, CurveTenor.Fwd6M);
            MySchedule.Print();

            SwapSchedule MySchedule2 = new SwapSchedule(DateTime.Now, MyDate, MyDate2, DayCount.THIRTY360, DayRule.MF, CurveTenor.Fwd1Y);
            MySchedule2.Print();

        }

        public static void OisCalender()
        {

        }

        public static void DayCompoundingTest()
        {

            DateTime AsOf = new DateTime(2017, 1, 15);
            DateTime Start = new DateTime(2017, 1, 31);
            DateTime End = new DateTime(2017, 2, 19);
            DayRule DayRule = DayRule.F;
            DayCount DayCount = DayCount.ACT360;

            DateTime Temp = Start;
            double Compound = 1;

            while (Temp < End)
            {
                double Rate = 0.1;
                DateTime NewDate = DateHandling.AddTenorAdjust(Temp, "1B", DayRule);
                double Days = NewDate.Subtract(Temp).TotalDays;

                Console.WriteLine("Day: " + Temp.DayOfWeek + " to " + NewDate.DayOfWeek + ". Days: " + Days);
                Temp = NewDate;
                Compound *= (1 + Rate * Days / 365);
                Console.Write("  .. Compound: " + Compound + " . ");
            }

            Compound = (Compound - 1) / (DateHandling.Cvg(Start, End, DayCount));
            Console.WriteLine("Compound: " + Compound);

            DateTime End2 = DateHandling.AddTenorAdjust(Start, "68M");
            SwapSchedule SwapSchedule = new SwapSchedule(AsOf, Start, End2, DayCount.ACT360, DayRule.MF, CurveTenor.Fwd6M, StubPlacement.Beginning);
            SwapSchedule.Print();
            SwapSchedule SwapSchedule2 = new SwapSchedule(AsOf, Start, End2, DayCount.ACT360, DayRule.MF, CurveTenor.Fwd6M, StubPlacement.End);
            SwapSchedule2.Print();
        }
    }
}
