using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExcelDna.Integration;
using MasterThesis;

namespace ExcelApplication
{
    /* General information:
     * This file contains simple functions not directly related to 
     * interest rate derivative pricing. This includes utility functions
     * plus date rolling utility.
     */

    public class FreeFunctions
    {
        [ExcelFunction(Description = "Some description", Name = "mt.Helpers.AddTenorAdjust")]
        public static DateTime Helpers_AddTenorAdjust(DateTime date, string Tenor, string dayRule)
        {
            DayRule dayRuleEnum = StrToEnum.DayRuleConvert(dayRule);
            return DateHandling.AddTenorAdjust(date, Tenor, dayRuleEnum);
        }

        [ExcelFunction(Description = "Some description", Name = "mt.Helpers.AdjustDate")]
        public static DateTime Helpers_AdjustDate(DateTime startDate, string dayRule)
        {
            DayRule dayRuleEnum = StrToEnum.DayRuleConvert(dayRule);
            return DateHandling.AdjustDate(startDate, dayRuleEnum);
        }

        [ExcelFunction(Description = "some description", Name = "mt.Helpers.ParseStringAndOutput", IsVolatile = true)]
        public static object[,] ParseStringAndOutput(string instrumentString)
        {
            object[] output = instrumentString.Split(',');
            object[,] realOutput = new object[output.Length, 1];
            for (int i = 0; i < output.Length; i++)
            {
                realOutput[i, 0] = output[i];
            }

            return realOutput;
        }

        [ExcelFunction(Description = "some description", Name = "mt.Helpers.IsAddinLoaded", IsVolatile = true)]
        public static bool IsAddinLoaded()
        {
            return true;
        }

    }
}
