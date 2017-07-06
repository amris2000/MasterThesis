using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Excel;
using System.Data;
using MasterThesis.ExcelInterface;

namespace MasterThesis
{
    // Enums related to scheduling, curves and linear rate instruments
    public enum CurveTenor { Simple, DiscOis, DiscLibor, Fwd1D, Fwd1M, Fwd3M, Fwd6M, Fwd1Y }
    public enum DayRule {  MF, F, P, N }
    public enum StubPlacement { Beginning, End, NullStub };
    public enum DayCount { ACT360, ACT365, ACT36525, THIRTY360 }
    public enum InterpMethod { Constant, Linear, LogLinear, Hermite, Catrom }
    public enum Instrument { IrSwap, BasisSwap, Fra, Futures, OisSwap, Deposit };
    public enum QuoteType { ParSwapRate, ParBasisSpread, OisRate, FraRate, FuturesRate, Deposit };
    public enum InstrumentFormatType { Swaps, Fras, Futures, BasisSpreads, FwdStartingSwaps };
    public enum Tenor { D, B, W, M, Y };
    public enum Direction { Pay, Rec };

    public static class EnumHelpers
    {
        public static int TradeSignToInt(Direction tradeSign)
        {
            // Swaps: Pay fixed, basis swaps: pay spread.
            if (tradeSign == Direction.Pay)
                return 1;
            else
                return -1;
        }

        public static double TradeSignToDouble(Direction tradeSign)
        {
            // Swaps: Pay fixed, Basis swaps pay spread
            if (tradeSign == Direction.Pay)
                return 1.0;
            else
                return -1.0;
        }

        public static bool IsFwdTenor(CurveTenor tenor)
        {
            switch(tenor)
            {
                case CurveTenor.Fwd1M:
                    return true;
                case CurveTenor.Fwd3M:
                    return true;
                case CurveTenor.Fwd6M:
                    return true;
                case CurveTenor.Fwd1Y:
                    return true;
                default:
                    return false;
            }
        }
    }

    // Used to inspect instrument from a factory in Excel Layer. 
    public static class ConstructInstrumentInspector
    {
        public static object[,] MakeExcelOutput(InstrumentFactory factory, string identifier)
        {
            Schedule schedule1 = null;
            Schedule schedule2 = null;
            QuoteType quoteType = factory.InstrumentTypeMap[identifier];
            bool instrumentHasSchedule = false;

            switch(quoteType)
            {
                case QuoteType.ParSwapRate:
                    schedule1 = factory.IrSwaps[identifier].FloatLeg.Schedule;
                    schedule2 = factory.IrSwaps[identifier].FixedLeg.Schedule;
                    instrumentHasSchedule = true;
                    break;
                case QuoteType.ParBasisSpread:
                    schedule1 = factory.BasisSwaps[identifier].FloatLegNoSpread.Schedule;
                    schedule2 = factory.BasisSwaps[identifier].FloatLegSpread.Schedule;
                    instrumentHasSchedule = true;
                    break;
                case QuoteType.OisRate:
                    schedule1 = factory.OisSwaps[identifier].FloatSchedule;
                    schedule2 = factory.OisSwaps[identifier].FixedSchedule;
                    instrumentHasSchedule = true;
                    break;
            }

            if (instrumentHasSchedule)
            {
                object[,] schedule1Object = MakeScheduleArray(schedule1);
                object[,] schedule2Object = MakeScheduleArray(schedule2);
                object[,] infoArray = MakeInstrumentInfoArray(factory, identifier);

                int length = infoArray.GetLength(0);
                int totalLength = length + Math.Max(schedule1Object.GetLength(0), schedule2Object.GetLength(0)) + 1;

                object[,] output = new object[totalLength, 11];

                for (int i = 0; i < totalLength; i ++)
                {
                    for (int j = 0; j < 11; j++)
                        output[i, j] = "";
                }

                for (int i = 0; i<length; i++)
                {
                    output[i, 0] = infoArray[i, 0];
                    output[i, 1] = infoArray[i, 1];
                }

                // Fill out first schedule
                for (int i = length; i < schedule1Object.GetLength(0) + length; i++)
                {
                    output[i, 0] = schedule1Object[i - length, 0];
                    output[i, 1] = schedule1Object[i - length, 1];
                    output[i, 2] = schedule1Object[i - length, 2];
                    output[i, 3] = schedule1Object[i - length, 3];
                    output[i, 4] = schedule1Object[i - length, 4];
                }

                // Fill out second schedule
                for (int i = length; i < schedule2Object.GetLength(0) + length; i++)
                {
                    output[i, 6] = schedule2Object[i - length, 0];
                    output[i, 7] = schedule2Object[i - length, 1];
                    output[i, 8] = schedule2Object[i - length, 2];
                    output[i, 9] = schedule2Object[i - length, 3];
                    output[i, 10] = schedule2Object[i - length, 4];
                }

                return output;

            }
            else
            {
                return MakeInstrumentInfoArray(factory, identifier);
            }

            

        }

        public static object[,] MakeInstrumentInfoArray(InstrumentFactory factory, string identifier)
        {
            object[,] output = new object[9,2];
            string whiteSpace = "";

            QuoteType quoteType = factory.InstrumentTypeMap[identifier];

            output[0, 0] = quoteType.ToString();
            output[0, 1] = whiteSpace;

            output[4, 0] = whiteSpace;
            output[4, 1] = whiteSpace;
            output[7, 0] = whiteSpace;
            output[7, 1] = whiteSpace;
            output[8, 0] = whiteSpace;
            output[8, 1] = whiteSpace;

            // InstrumentStrings
            output[5, 0] = InstrumentFactoryHeaders.GetHeaders[factory.InstrumentFormatTypeMap[identifier]];
            output[5, 1] = whiteSpace;
            output[6, 0] = factory.IdentifierStringMap[identifier];
            output[6, 1] = whiteSpace;

            switch (quoteType)
            {
                case QuoteType.ParSwapRate:
                    output[1, 0] = factory.IrSwaps[identifier].FloatLeg.StartDate.ToString("dd/MM/yyyy");
                    output[1, 1] = factory.IrSwaps[identifier].FixedLeg.StartDate.ToString("dd/MM/yyyy");
                    output[2, 0] = factory.IrSwaps[identifier].FloatLeg.EndDate.ToString("dd/MM/yyyy");
                    output[2, 1] = factory.IrSwaps[identifier].FixedLeg.EndDate.ToString("dd/MM/yyyy");
                    output[3, 0] = factory.IrSwaps[identifier].FloatLeg.Tenor.ToString();
                    output[3, 1] = factory.IrSwaps[identifier].FixedLeg.Tenor.ToString();

                    break;
                case QuoteType.OisRate:
                    output[1, 0] = factory.OisSwaps[identifier].StartDate.ToString("dd/MM/yyyy");
                    output[1, 1] = whiteSpace;
                    output[2, 0] = factory.OisSwaps[identifier].EndDate.ToString("dd/MM/yyyy");
                    output[2, 1] = whiteSpace;

                    //output[3, 1] = factory.OisSwaps[dentifier].
                    output[3, 0] = whiteSpace;
                    output[3, 1] = whiteSpace;
                    output[8, 0] = "(Float schedule -- Fixed schedule)";

                    break;
                case QuoteType.FraRate:
                    output[1, 0] = factory.Fras[identifier].StartDate.ToString("dd/MM/yyyy");
                    output[1, 1] = whiteSpace;
                    output[2, 0] = factory.Fras[identifier].EndDate.ToString("dd/MM/yyyy");
                    output[2, 1] = whiteSpace;
                    output[3, 0] = factory.Fras[identifier].ReferenceIndex.ToString();
                    output[3, 1] = whiteSpace;

                    break;
                case QuoteType.ParBasisSpread:
                    output[1, 0] = factory.BasisSwaps[identifier].FloatLegNoSpread.StartDate.ToString("dd/MM/yyyy");
                    output[1, 1] = factory.BasisSwaps[identifier].FloatLegSpread.StartDate.ToString("dd/MM/yyyy");
                    output[2, 0] = factory.BasisSwaps[identifier].FloatLegNoSpread.EndDate.ToString("dd/MM/yyyy");
                    output[2, 1] = factory.BasisSwaps[identifier].FloatLegSpread.EndDate.ToString("dd/MM/yyyy");
                    output[3, 0] = factory.BasisSwaps[identifier].FloatLegNoSpread.Tenor.ToString();
                    output[3, 1] = factory.BasisSwaps[identifier].FloatLegSpread.Tenor.ToString(); 

                    break;
                case QuoteType.FuturesRate:
                    output[1, 0] = factory.Futures[identifier].FraSameSpec.StartDate.ToString("dd/MM/yyyy");
                    output[1, 1] = whiteSpace;
                    output[2, 0] = factory.Futures[identifier].FraSameSpec.EndDate.ToString("dd/MM/yyyy");
                    output[2, 1] = whiteSpace;
                    output[3, 0] = factory.Futures[identifier].FraSameSpec.ReferenceIndex.ToString();
                    output[3, 1] = whiteSpace;
                    output[8, 0] = "Convexity adjustment: " + Math.Round(factory.Futures[identifier].Convexity,4).ToString();

                    break;
                case QuoteType.Deposit:
                    // do something
                    break;
                default:
                    throw new InvalidOperationException("QuoteType for identifier is not valid.");
            }
            return output;
        }

        public static object[,] MakeScheduleArray(Schedule schedule)
        {
            int rows = schedule.AdjEndDates.Count + 2;
            object[,] output = new object[rows,5];

            output[0, 0] = "Daycount = " + schedule.DayCount.ToString() + ", DayRule = " + schedule.DayRule.ToString();
            output[1, 0] = "UnadjStartDates";
            output[1, 1] = "UnadjEndDates";
            output[1, 2] = "AdjStartDates";
            output[1, 3] = "AdjEndDates";
            output[1, 4] = "Coverage";

            for (int i = 0; i<schedule.AdjEndDates.Count; i++)
            {
                output[i + 2, 0] = schedule.UnAdjStartDates[i].ToString("dd/MM/yyyy");
                output[i + 2, 1] = schedule.UnAdjEndDates[i].ToString("dd/MM/yyyy");
                output[i + 2, 2] = schedule.AdjStartDates[i].ToString("dd/MM/yyyy");
                output[i + 2, 3] = schedule.AdjEndDates[i].ToString("dd/MM/yyyy");
                output[i + 2, 4] = Math.Round(schedule.Coverages[i],4).ToString();
            }

            return output;
        }
    }

    #region Functionality to convert strings to enums and visa versa. Needed for Excel/C# communication
    public static class StrToEnum
    {
        public static Tenor ConvertTenorLetter(string tenorLetter)
        {
            switch(tenorLetter.ToUpper())
            {
                case "D":
                    return Tenor.D;
                case "B":
                    return Tenor.B;
                case "W":
                    return Tenor.W;
                case "M":
                    return Tenor.M;
                case "Y":
                    return Tenor.Y;
                default:
                    throw new InvalidOperationException("Tenorletter " + tenorLetter + " is not valid.");
            }
        }

        public static InterpMethod InterpolationConvert(string interp)
        {
            interp = interp.ToUpper();

            switch (interp)
            {
                case "LINEAR":
                    return InterpMethod.Linear;
                case "LOGLINEAR":
                    return InterpMethod.LogLinear;
                case "HERMITE":
                    return InterpMethod.Hermite;
                case "CONSTANT":
                    return InterpMethod.Constant;
                default:
                    throw new InvalidOperationException("Interpolation method " + interp + " is not valid.");
            }
        }

        public static CurveTenor CurveTenorConvert(string tenor)
        {
            tenor = tenor.ToUpper();

            switch (tenor)
            {
                case "FWD1D":
                    return MasterThesis.CurveTenor.Fwd1D;
                case "FWD1M":
                    return MasterThesis.CurveTenor.Fwd1M;
                case "FWD3M":
                    return MasterThesis.CurveTenor.Fwd3M;
                case "FWD6M":
                    return MasterThesis.CurveTenor.Fwd6M;
                case "FWD1Y":
                    return MasterThesis.CurveTenor.Fwd1Y;
                case "DISCOIS":
                    return MasterThesis.CurveTenor.DiscOis;
                case "DISCLIBOR":
                    return MasterThesis.CurveTenor.DiscLibor;
                default:
                    throw new InvalidOperationException("Invalid CurveTenor " + tenor);
            }
        }

        public static CurveTenor CurveTenorFromSimpleTenor(string tenor)
        {
            tenor = tenor.ToUpper();

            switch (tenor)
            {
                case "1D":
                    return CurveTenor.Fwd1D;
                case "1M":
                    return CurveTenor.Fwd1M;
                case "3M":
                    return CurveTenor.Fwd3M;
                case "6M":
                    return CurveTenor.Fwd6M;
                case "1Y":
                    return CurveTenor.Fwd1Y;
                case "12M":
                    return CurveTenor.Fwd1Y;
                default:
                    throw new InvalidOperationException("Invalid simple curve tenor " + tenor);
            }
        }

        public static DayCount DayCountConvert(string dayCount)
        {
            dayCount = dayCount.ToUpper();

            switch (dayCount)
            {
                case "ACT/360":
                    return MasterThesis.DayCount.ACT360;
                case "ACT/365":
                    return MasterThesis.DayCount.ACT365;
                case "ACT/365.25":
                    return MasterThesis.DayCount.ACT36525;
                case "30/360":
                    return MasterThesis.DayCount.THIRTY360;
                default:
                    throw new InvalidOperationException("Invalid dayCount: " + dayCount);
            }
        }

        public static DayRule DayRuleConvert(string dayRule)
        {
            dayRule = dayRule.ToUpper();

            switch (dayRule)
            {
                case "MF":
                    return MasterThesis.DayRule.MF;
                case "F":
                    return MasterThesis.DayRule.F;
                case "P":
                    return MasterThesis.DayRule.P;
                case "NONE":
                    return MasterThesis.DayRule.N;
                case "N":
                    return MasterThesis.DayRule.N;
                default:
                    throw new InvalidOperationException("Invalid dayRule: " + dayRule);
            }
        }

        public static CurveTenor ConvertCurveIdentToCurveTenor(string CurveIdent)
        {
            switch (CurveIdent)
            {
                case "1D":
                    return CurveTenor.Fwd1D;
                case "1M":
                    return CurveTenor.Fwd1M;
                case "3M":
                    return CurveTenor.Fwd3M;
                case "6M":
                    return CurveTenor.Fwd6M;
                case "1Y":
                    return CurveTenor.Fwd1Y;
                case "OIS":
                    return CurveTenor.DiscOis;
                case "LIBOR":
                    return CurveTenor.DiscLibor;
                default:
                    throw new ArgumentException("CANNOT FIND CURVETENOR FOR INPUT STRING.");
            }
        }

        public static Tenor TenorConvert(string tenor)
        {
            tenor = tenor.ToUpper();

            switch(tenor)
            {
                case "D":
                    return Tenor.D;
                case "B":
                    return Tenor.B;
                case "W":
                    return Tenor.W;
                case "M":
                    return Tenor.M;
                case "Y":
                    return Tenor.Y;
                default:
                    throw new InvalidOperationException("Tenor " + tenor + " is not a valid tenor."); 
            }
        }
    }

    public static class EnumToStr
    {
        public static string CurveTenor(CurveTenor CurveTenor)
        {
            switch(CurveTenor)
            {
                case CurveTenor.Fwd1D:
                    return "1D";
                case CurveTenor.Fwd1M:
                    return "1M";
                case CurveTenor.Fwd3M:
                    return "3M";
                case CurveTenor.Fwd6M:
                    return "6M";
                case CurveTenor.Fwd1Y:
                    return "1Y";
                default:
                    throw new ArgumentException("Wrong CurveTenor");
            }
        }

        public static string DayRule(DayRule DayRule)
        {
            switch(DayRule)
            {
                case DayRule.MF:
                    return "MF";
                case DayRule.F:
                    return "F";
                case DayRule.P:
                    return "P";
                case DayRule.N:
                    return "N";
                default:
                    return "ERROR";
            }
        }

        public static string DayCount(DayCount DayCount)
        {
            switch (DayCount)
            {
                case DayCount.ACT360:
                    return "ACT/360";
                case DayCount.ACT365:
                    return "ACT/365";
                case DayCount.ACT36525:
                    return "ACT/365.25";
                case DayCount.THIRTY360:
                    return "30/360";
                default:
                    return "ERROR";
            }
        }

        public static string TenorConvert(Tenor tenor)
        {
            switch(tenor)
            {
                case Tenor.D:
                    return "D";
                case Tenor.B:
                    return "B";
                case Tenor.W:
                    return "W";
                case Tenor.M:
                    return "M";
                case Tenor.Y:
                    return "Y";
                default:
                    throw new InvalidOperationException("Tenor is not valid");
            }
        }
    }
    #endregion

}
