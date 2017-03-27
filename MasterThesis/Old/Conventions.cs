using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis.Conv
{
    public static class IrSwap6M
    {
        public static CurveTenor FixedFreq = CurveTenor.Fwd1Y;
        public static CurveTenor FloatFreq = CurveTenor.Fwd6M;
        public static DayRule FixedDayRule = DayRule.MF;
        public static DayRule FloatDayRule = DayRule.MF;
        public static DayCount FixedDayCount = DayCount.THIRTY360;
        public static DayCount FloatDayCount = DayCount.ACT360;
        public static string SpotLag = "2B";
    }
}
