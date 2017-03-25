using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExcelDna.Integration;
using MasterThesis;

namespace ExcelApplication
{
    public class FreeFunctions
    {
        [ExcelFunction(Description = "Some description", Name = "mt.Helpers.AddTenorAdjust")]
        public static DateTime Helpers_AddTenorAdjust(DateTime date, string Tenor, string dayRule)
        {
            DayRule dayRuleEnum = StrToEnum.DayRuleConvert(dayRule);
            return Calender.AddTenor(date, Tenor, dayRuleEnum);
        }

        [ExcelFunction(Description = "Some description", Name = "mt.Helpers.AdjustDate")]
        public static DateTime Helpers_AdjustDate(DateTime startDate, string dayRule)
        {
            DayRule dayRuleEnum = StrToEnum.DayRuleConvert(dayRule);
            return Calender.AdjustDate(startDate, dayRuleEnum);
        }

    }
}
