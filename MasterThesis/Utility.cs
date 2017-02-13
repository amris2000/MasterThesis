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

namespace MasterThesis
{
    public enum CurveTenor { Simple, DiscOis, DiscLibor, Fwd1D, Fwd1M, Fwd3M, Fwd6M, Fwd1Y }
    public enum CurveType { Fwd, DiscOis, DiscLibor }
    public enum QuoteType { SwapRate, BasisSpread, Fixing, FraRate, FutureRate, BaseSpread}
    public enum InstrumentType { Swap, Fra, Future, IrSwap, MmBasisSwap, BasisSwap, FxFwd, Deposit, Swaption }
    public enum InstrumentComplexity { Linear, NonLinear }
    public enum Tenor { T1D, T1M, T3M, T6M, T1Y }
    public enum DayRule {  MF, F, P, N }
    public enum DayCount { ACT360, ACT365, ACT36525, THIRTY360 }
    public enum InterpMethod { Constant, Linear, LogLinear, Hermite, Catrom }
    public enum MarketDataInstrument { Swap, BaseSpread, Fra, Future, BasisSwap, Cash }
    public enum SwapQuoteType { Vanilla, ShortSwap }

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
    }

    public static class ExcelUtility
    {
        public static string WorkbookName = "CURVEBOOK";
        public static string Sheet = "CURVES";
        public static string Path = @"C:\Users\Frede\Dropbox\Polit\12. SPECIALE\ExcelFiles\Data\Test.xlsx";
        public static DataSet MyData;
        public static FwdCurve Fwd1d, Fwd1m, Fwd3m, Fwd6m, Fwd1y;
        public static DiscCurve DiscOis, DiscLibor;

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

            FwdCurves MyCurves = new MasterThesis.FwdCurves();
            MyCurves.AddCurve(new FwdCurveAdvanced(Fwd1d, Fwd1dVal, CurveTenor.Fwd1D));
            MyCurves.AddCurve(new FwdCurveAdvanced(Fwd1m, Fwd1mVal, CurveTenor.Fwd1M));
            MyCurves.AddCurve(new FwdCurveAdvanced(Fwd3m, Fwd3mVal, CurveTenor.Fwd3M));
            MyCurves.AddCurve(new FwdCurveAdvanced(Fwd6m, Fwd6mVal, CurveTenor.Fwd6M));
            MyCurves.AddCurve(new FwdCurveAdvanced(Fwd1y, Fwd1yVal, CurveTenor.Fwd1Y));
            Store.FwdCurveCollections["MYCURVES"] = MyCurves;

            Store.FwdCurves["1D"] = new FwdCurveAdvanced(Fwd1d, Fwd1dVal, CurveTenor.Fwd1D);
            Store.FwdCurves["1M"] = new FwdCurveAdvanced(Fwd1d, Fwd1dVal, CurveTenor.Fwd1M);
            Store.FwdCurves["3M"] = new FwdCurveAdvanced(Fwd1d, Fwd1dVal, CurveTenor.Fwd3M);
            Store.FwdCurves["6M"] = new FwdCurveAdvanced(Fwd1d, Fwd1dVal, CurveTenor.Fwd6M);
            Store.FwdCurves["1Y"] = new FwdCurveAdvanced(Fwd1d, Fwd1dVal, CurveTenor.Fwd1Y);
            Store.DiscCurves["OIS"] = new MasterThesis.DiscCurve(DiscOis, DiscOisVal);
            Store.DiscCurves["LIBOR"] = new MasterThesis.DiscCurve(DiscLibor, DiscLiborVal);
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
