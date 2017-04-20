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
    public enum CurveTenor { Simple, DiscOis, DiscLibor, Fwd1D, Fwd1M, Fwd3M, Fwd6M, Fwd1Y }
    public enum DayRule {  MF, F, P, N }
    public enum StubPlacement { Beginning, End, NullStub };
    public enum DayCount { ACT360, ACT365, ACT36525, THIRTY360 }
    public enum InterpMethod { Constant, Linear, LogLinear, Hermite, Catrom }
    public enum Instrument { IrSwap, BasisSwap, Fra, Futures, OisSwap };
    public enum QuoteType { ParSwapRate, ParBasisSpread, OisRate, FraRate, FuturesRate, Deposit };
    public enum InstrumentFormatType { Swaps, Fras, Futures, BasisSpreads, FwdStartingSwaps };
    public enum Tenor { D, B, W, M, Y };
    public enum Direction { Pay, Rec };


    // OLD / UNUSED
    public enum MarketDataInstrument { IrSwapRate, OisRate, BaseSpread, Fra, Future, BasisSwap, Cash, Fixing }
    public enum SwapQuoteType { Vanilla, ShortSwap }
    public enum InstrumentComplexity { Linear, NonLinear }
    public enum QuoteTypeOld { SwapRate, BasisSpread, Fixing, FraRate, FutureRate, BaseSpread }
    public enum InstrumentType { Swap, Fra, Future, IrSwap, MmBasisSwap, BasisSwap, FxFwd, OisSwap, Deposit, Swaption, Fixing }

    public static class EnumHelpers
    {
        public static double TradeSignToDouble(Direction tradeSign)
        {
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

    /// <summary>
    /// Used to inspect instrument in Excel Layer. 
    /// </summary>
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

        public static CurveTenor CurveIdent(string CurveIdent)
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

        public static MarketDataInstrument TypeIdent(string TypeIdent)
        {
            switch (TypeIdent)
            {
                case "SWAP":
                    return MarketDataInstrument.IrSwapRate;
                case "BASIS SWAP":
                    return MarketDataInstrument.BasisSwap;
                case "BASESPREAD":
                    return MarketDataInstrument.BaseSpread;
                case "CASH":
                    return MarketDataInstrument.Cash;
                case "FRA":
                    return MarketDataInstrument.Fra;
                case "FUTURE":
                    return MarketDataInstrument.Future;
                case "FIXING":
                    return MarketDataInstrument.Fixing;
                default:
                    throw new ArgumentException("CANNOT FIND MARKETDATAINSTRUMENT FOR INPUT STRING.");
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

    public static class ExcelUtility
    {
        public static string WorkbookName = "CURVEBOOK";
        public static string Sheet = "CURVES";
        public static string Path = @"C:\Users\Frede\Dropbox\Polit\12. SPECIALE\ExcelFiles\Data\Test.xlsx";
        public static DataSet MyData;
        public static Curve Fwd1d, Fwd1m, Fwd3m, Fwd6m, Fwd1y;
        public static Curve DiscOis, DiscLibor;

        public static void DataReader()
        {
            FileStream Stream = File.Open(Path, FileMode.Open, FileAccess.Read);

            IExcelDataReader ExcelReader = ExcelReaderFactory.CreateOpenXmlReader(Stream);

            //DataSet Result = ExcelReader.AsDataSet();

            ExcelReader.IsFirstRowAsColumnNames = true;

            MyData = ExcelReader.AsDataSet();
            MyData.DataSetName = "CURVES";

            ExcelReader.Close();

        }

        public static void LoadCurvesFromFile()
        {
            int i = MyData.Tables[0].Rows.Count;
            List<DateTime> Fwd1d = new List<DateTime>();
            List<DateTime> Fwd1m = new List<DateTime>();
            List<DateTime> Fwd3m = new List<DateTime>();
            List<DateTime> Fwd6m = new List<DateTime>();
            List<DateTime> Fwd1y = new List<DateTime>();
            List<DateTime> DiscOis = new List<DateTime>();
            List<DateTime> DiscLibor = new List<DateTime>();
            List<double> Fwd1dVal = new List<double>();
            List<double> Fwd1mVal = new List<double>();
            List<double> Fwd3mVal = new List<double>();
            List<double> Fwd6mVal = new List<double>();
            List<double> Fwd1yVal = new List<double>();
            List<double> DiscOisVal = new List<double>();
            List<double> DiscLiborVal = new List<double>();

            int j = 0;
            foreach (DataRow Row in MyData.Tables[0].Rows)
            {
                if (MyData.Tables[0].Rows[j]["FWD1D_DATE"].ToString() != "")
                    Fwd1d.Add((DateTime)MyData.Tables[0].Rows[j]["FWD1D_DATE"]);
                if (MyData.Tables[0].Rows[j]["FWD1M_DATE"].ToString() != "")
                    Fwd1m.Add((DateTime)MyData.Tables[0].Rows[j]["FWD1M_DATE"]);
                if (MyData.Tables[0].Rows[j]["FWD3M_DATE"].ToString() != "")
                    Fwd3m.Add((DateTime)MyData.Tables[0].Rows[j]["FWD3M_DATE"]);
                if (MyData.Tables[0].Rows[j]["FWD6M_DATE"].ToString() != "")
                    Fwd6m.Add((DateTime)MyData.Tables[0].Rows[j]["FWD6M_DATE"]);
                if (MyData.Tables[0].Rows[j]["FWD1Y_DATE"].ToString() != "")
                    Fwd1y.Add((DateTime)MyData.Tables[0].Rows[j]["FWD1Y_DATE"]);
                if (MyData.Tables[0].Rows[j]["DISCOIS_DATE"].ToString() != "")
                    DiscOis.Add((DateTime)MyData.Tables[0].Rows[j]["DISCOIS_DATE"]);
                if (MyData.Tables[0].Rows[j]["DISCLIBOR_DATE"].ToString() != "")
                    DiscLibor.Add((DateTime)MyData.Tables[0].Rows[j]["DISCLIBOR_DATE"]);

                if (MyData.Tables[0].Rows[j]["FWD1D_VAL"].ToString() != "")
                    Fwd1dVal.Add((double)MyData.Tables[0].Rows[j]["FWD1D_VAL"]);
                if (MyData.Tables[0].Rows[j]["FWD1M_VAL"].ToString() != "")
                    Fwd1mVal.Add((double)MyData.Tables[0].Rows[j]["FWD1M_VAL"]);
                if (MyData.Tables[0].Rows[j]["FWD3M_VAL"].ToString() != "")
                    Fwd3mVal.Add((double)MyData.Tables[0].Rows[j]["FWD3M_VAL"]);
                if (MyData.Tables[0].Rows[j]["FWD6M_VAL"].ToString() != "")
                    Fwd6mVal.Add((double)MyData.Tables[0].Rows[j]["FWD6M_VAL"]);
                if (MyData.Tables[0].Rows[j]["FWD1Y_VAL"].ToString() != "")
                    Fwd1yVal.Add((double)MyData.Tables[0].Rows[j]["FWD1Y_VAL"]);
                if (MyData.Tables[0].Rows[j]["DISCOIS_VAL"].ToString() != "")
                    DiscOisVal.Add((double)MyData.Tables[0].Rows[j]["DISCOIS_VAL"]);
                if (MyData.Tables[0].Rows[j]["DISCLIBOR_VAL"].ToString() != "")
                    DiscLiborVal.Add((double)MyData.Tables[0].Rows[j]["DISCLIBOR_VAL"]);
                j = j + 1;
            }

            FwdCurveContainer MyCurves = new MasterThesis.FwdCurveContainer();
            MyCurves.AddCurve(new Curve(Fwd1d, Fwd1dVal), CurveTenor.Fwd1D);
            MyCurves.AddCurve(new Curve(Fwd1m, Fwd1mVal), CurveTenor.Fwd1M);
            MyCurves.AddCurve(new Curve(Fwd3m, Fwd3mVal), CurveTenor.Fwd3M);
            MyCurves.AddCurve(new Curve(Fwd6m, Fwd6mVal), CurveTenor.Fwd6M);
            MyCurves.AddCurve(new Curve(Fwd1y, Fwd1yVal), CurveTenor.Fwd1Y);
            //Store.FwdCurveCollections["MYCURVES"] = MyCurves;
            //Store.Curves[CurveTenor.DiscLibor] = new MasterThesis.Curve(DiscLibor, DiscLiborVal, CurveTenor.DiscLibor);
            //Store.Curves[CurveTenor.DiscOis] = new MasterThesis.Curve(DiscOis, DiscOisVal, CurveTenor.DiscOis);
        }

        public static void CreateCurveOld()
        {
            int i = MyData.Tables[0].Rows.Count;
            DateTime[] Fwd1d = new DateTime[i];
            DateTime[] Fwd1m = new DateTime[i];
            DateTime[] Fwd3m = new DateTime[i];
            DateTime[] Fwd6m = new DateTime[i];
            DateTime[] Fwd1y = new DateTime[i];
            DateTime[] DiscOis = new DateTime[i];
            DateTime[] DiscLibor = new DateTime[i];
            double[] Fwd1dVal = new double[i];
            double[] Fwd1mVal = new double[i];
            double[] Fwd3mVal = new double[i];
            double[] Fwd6mVal = new double[i];
            double[] Fwd1yVal = new double[i];
            double[] DiscOisVal = new double[i];
            double[] DiscLiborVal = new double[i];

            int j = 0;
            foreach (DataRow Row in MyData.Tables[0].Rows)
            {
                if (MyData.Tables[0].Rows[j]["FWD1D_DATE"].ToString() != "")
                    Fwd1d[j] = (DateTime)MyData.Tables[0].Rows[j]["FWD1D_DATE"];
                if (MyData.Tables[0].Rows[j]["FWD1M_DATE"].ToString() != "")
                    Fwd1m[j] = (DateTime)MyData.Tables[0].Rows[j]["FWD1M_DATE"];
                if (MyData.Tables[0].Rows[j]["FWD3M_DATE"].ToString() != "")
                    Fwd3m[j] = (DateTime)MyData.Tables[0].Rows[j]["FWD3M_DATE"];
                if (MyData.Tables[0].Rows[j]["FWD6M_DATE"].ToString() != "")
                    Fwd6m[j] = (DateTime)MyData.Tables[0].Rows[j]["FWD6M_DATE"];
                if (MyData.Tables[0].Rows[j]["FWD1Y_DATE"].ToString() != "")
                    Fwd1y[j] = (DateTime)MyData.Tables[0].Rows[j]["FWD1Y_DATE"];
                if (MyData.Tables[0].Rows[j]["DISCOIS_DATE"].ToString() != "")
                    DiscOis[j] = (DateTime)MyData.Tables[0].Rows[j]["DISCOIS_DATE"];
                if (MyData.Tables[0].Rows[j]["DISCLIBOR_DATE"].ToString() != "")
                    DiscLibor[j] = (DateTime)MyData.Tables[0].Rows[j]["DISCLIBOR_DATE"];

                if (MyData.Tables[0].Rows[j]["FWD1D_VAL"].ToString() != "")
                    Fwd1dVal[j] = (double)MyData.Tables[0].Rows[j]["FWD1D_VAL"];
                if (MyData.Tables[0].Rows[j]["FWD1M_VAL"].ToString() != "")
                    Fwd1mVal[j] = (double)MyData.Tables[0].Rows[j]["FWD1M_VAL"];
                if (MyData.Tables[0].Rows[j]["FWD3M_VAL"].ToString() != "")
                    Fwd3mVal[j] = (double)MyData.Tables[0].Rows[j]["FWD3M_VAL"];
                if (MyData.Tables[0].Rows[j]["FWD6M_VAL"].ToString() != "")
                    Fwd6mVal[j] = (double)MyData.Tables[0].Rows[j]["FWD6M_VAL"];
                if (MyData.Tables[0].Rows[j]["FWD1Y_VAL"].ToString() != "")
                    Fwd1yVal[j] = (double)MyData.Tables[0].Rows[j]["FWD1Y_VAL"];
                if (MyData.Tables[0].Rows[j]["DISCOIS_VAL"].ToString() != "")
                    DiscOisVal[j] = (double)MyData.Tables[0].Rows[j]["DISCOIS_VAL"];
                if (MyData.Tables[0].Rows[j]["DISCLIBOR_VAL"].ToString() != "")
                    DiscLiborVal[j] = (double)MyData.Tables[0].Rows[j]["DISCLIBOR_VAL"];
                j = j + 1;
            }
        }
    }
    public static class PrintUtility
    {
        public static string PrintListNicely(List<string[]> lines, int padding = 1)
        {
            // Calculate maximum numbers for each element accross all lines
            var numElements = lines[0].Length;
            var maxValues = new int[numElements];
            for (int i = 0; i < numElements; i++)
            {
                maxValues[i] = lines.Max(x => x[i].Length) + padding;
            }

            var sb = new StringBuilder();
            // Build the output
            bool isFirst = true;
            foreach (var line in lines)
            {
                if (!isFirst)
                {
                    sb.AppendLine();
                }
                isFirst = false;

                for (int i = 0; i < line.Length; i++)
                {
                    var value = line[i];
                    // Append the value with padding of the maximum length of any value for this element
                    sb.Append(value.PadRight(maxValues[i]));
                }
            }
            return sb.ToString();
        }

    }
}
