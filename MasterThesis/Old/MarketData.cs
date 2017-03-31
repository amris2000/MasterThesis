using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis.Extensions;

namespace MasterThesis
{


    //public class RawMarketData
    //{
    //    public string Identifier, TypeIdent, CurveIdent;
    //    public MarketDataInstrument MarketDataInstrument;
    //    public CurveTenor AsInputFor;
    //    public double Quote;
    //    public DateTime AsOf;

    //    public RawMarketData(DateTime asOf, string Identifier, string TypeIdent, string CurveIdent, double Quote)
    //    {
    //        this.Identifier = Identifier;
    //        this.TypeIdent = TypeIdent;
    //        this.CurveIdent = CurveIdent;
    //        this.AsOf = asOf;
    //        MarketDataInstrument = StrToEnum.TypeIdent(TypeIdent);
    //        AsInputFor = StrToEnum.CurveIdent(CurveIdent);
    //        this.Quote = Quote;
    //    }
    //}

    //public static class QuoteFactory
    //{
    //    public static MarketQuote CreateMarketQuote(RawMarketData MarketData)
    //    {
    //        DateTime AsOf = MarketData.AsOf;
    //        switch (MarketData.MarketDataInstrument)
    //        {
    //            case MarketDataInstrument.IrSwapRate:
    //                if (IsOisSwap(MarketData.Identifier))
    //                    return new OisSwapQuote(AsOf, MarketData);
    //                else
    //                    return new SwapQuote(AsOf, MarketData);
    //            case MarketDataInstrument.BaseSpread:
    //                return new BaseSpreadQuote(AsOf, MarketData);
    //            //case MarketDataInstrument.BasisSwap:
    //            //    return new BasisSwapQuote(AsOf, MarketData);
    //            case MarketDataInstrument.Fra:
    //                return new FraQuote(AsOf, MarketData);
    //            case MarketDataInstrument.Future:
    //                return new FutureQuote(AsOf, MarketData);
    //            case MarketDataInstrument.Cash:
    //                return new CashQuote(AsOf, MarketData);
    //            case MarketDataInstrument.Fixing:
    //                return new FixingQuote(AsOf, MarketData);
    //            default:
    //                throw new ArgumentException("Cannot find definition for input");
    //        }
    //    }

    //    public static List<MarketQuote> CreateMarketQuoteCollection(List<RawMarketData> rawMarketData)
    //    {
    //        List<MarketQuote> Out = new List<MarketQuote>();

    //        for (int i = 0; i < rawMarketData.Count; i++)
    //            Out.Add(CreateMarketQuote(rawMarketData[i]));

    //        return Out;

    //    }

    //    public static bool IsOisSwap(string Identifier)
    //    {
    //        if (Identifier.Contains("EURE"))
    //            return true;
    //        else
    //            return false;
    //    }
    //}

    //public abstract class MarketQuote
    //{
    //    public double Quote;
    //    public string Identifier;
    //    public string InstrumentString;
    //    public Asset Instrument;
    //    public DateTime AsOf;
    //    public CurveTenor AsInputFor;
    //    public MarketDataInstrument InstrumentType;
    //    public RawMarketData MarketData;
    //    public DateTime StartDate, EndDate;

    //    protected MarketQuote(DateTime asOf, RawMarketData MarketData)
    //    {
    //        this.AsOf = asOf;
    //        this.Quote = MarketData.Quote;
    //        this.Identifier = MarketData.Identifier;
    //        this.InstrumentType = MarketData.MarketDataInstrument;
    //        this.AsInputFor = MarketData.AsInputFor;
    //        this.MarketData = MarketData;
    //    }
    //    protected abstract void ParseQuote();
    //}

    //public class FixingQuote : MarketQuote
    //{
    //    public FixingQuote(DateTime asOf, RawMarketData MarketData) : base(asOf, MarketData)
    //    {
    //        StartDate = AsOf;
    //        EndDate = AsOf;
    //    }

    //    protected override void ParseQuote()
    //    {

    //    }
    //}

    //public class SwapQuote : MarketQuote
    //{
    //    // I.e. EURAB6E1Y (QuoteType = Vanilla), EUR4X1S (QutoeType = ShortSwap)
    //    string MaturityTenor;
    //    string Currency;
    //    SwapQuoteType QuoteType;
    //    CurveTenor FloatFreq;

    //    public SwapQuote(DateTime asOf, RawMarketData MarketData) : base(asOf, MarketData)
    //    {
    //        if (Identifier.Contains("X"))
    //            QuoteType = SwapQuoteType.ShortSwap;
    //        else
    //            QuoteType = SwapQuoteType.Vanilla;

    //        ParseQuote();

    //        StartDate = asOf;
    //        EndDate = Functions.AddTenorAdjust(StartDate, MaturityTenor, DayRule.MF);
    //        Instrument = new SwapSimple(AsOf, StartDate, EndDate, 0.01, CurveTenor.Fwd1Y, CurveTenor.Fwd6M, DayCount.THIRTY360, DayCount.ACT360, DayRule.MF, DayRule.MF, 1);
    //    }

    //    protected override void ParseQuote()
    //    {
    //        if (QuoteType == SwapQuoteType.Vanilla)
    //        {
    //            string Temp;
    //            Currency = this.MarketData.Identifier.Left(3);

    //            Temp = Identifier.Right(Identifier.Length - 5);
    //            string ReferenceTenor = Temp.Left(1);

    //            switch (ReferenceTenor)
    //            {
    //                // All of the used instruments of this identifier-type are 6M (there are 2 for 1M - Special conventions?? See documentation)
    //                case "6":
    //                    this.FloatFreq = CurveTenor.Fwd6M;
    //                    break;
    //                case "1":
    //                    this.FloatFreq = CurveTenor.Fwd1M;
    //                    break;
    //                default:
    //                    throw new ArgumentException("SwapQuote does not have reference 6M libor");
    //            }

    //            Temp = Temp.Right(Temp.Length - 2);
    //            this.MaturityTenor = Temp;
    //        }
    //        else if (QuoteType == SwapQuoteType.ShortSwap)
    //        {
    //            // For 1M tenor
    //            FloatFreq = CurveTenor.Fwd1M;
    //            string Temp = Identifier;
    //            Currency = Temp.Left(3);
    //            Temp = Temp.Replace(Currency, "");
    //            Temp = Temp.Replace("X1S", "");
    //            MaturityTenor = Temp + "M";
    //        }
    //    }
    //}
    //public class OisSwapQuote : MarketQuote
    //{

    //    string Tenor;

    //    public OisSwapQuote(DateTime AsOf, RawMarketData MarketData) : base(AsOf, MarketData)
    //    {
    //        ParseQuote();
    //        string[] noFwdTenor = new string[] { "0B", "1B" };
    //        if (noFwdTenor.Contains(Tenor) == false)
    //            StartDate = Functions.AddTenorAdjust(AsOf, "2B", DayRule.F);
    //        else
    //            StartDate = AsOf;

    //        EndDate = Functions.AddTenorAdjust(StartDate, Tenor, DayRule.F);
    //        Instrument = new OisSwap(AsOf, StartDate, Tenor, 0.1, DayCount.ACT360, DayCount.ACT360, DayRule.MF, DayRule.MF, 1);
    //        InstrumentType = MarketDataInstrument.OisRate;
    //    }

    //    protected override void ParseQuote()
    //    {
    //        string Temp = Identifier.Replace("EUREON", "");
            
    //        switch(Temp)
    //        {
    //            case "ON":
    //                Tenor = "1B";
    //                break;
    //            case "TN":
    //                Tenor = "2B";
    //                break;
    //            case "SW":
    //                Tenor = "1W";
    //                break;
    //            default:
    //                Tenor = Temp;
    //                break;
    //        }
    //    }
    //}
    //public class BaseSpreadQuote : MarketQuote
    //{
    //    public BaseSpreadQuote(DateTime asOf, RawMarketData MarketData) : base(asOf, MarketData)
    //    {

    //    }

    //    protected override void ParseQuote()
    //    {

    //    }

    //}
    ////public class BasisSwapQuote : MarketQuote
    ////{
    ////    public BasisSwapQuote(RawMarketData MarketData) : base(MarketData)
    ////    {

    ////    }

    ////    protected override void ParseQuote()
    ////    {

    ////    }
    ////}
    //public class FraQuote : MarketQuote
    //{
    //    public FraQuote(DateTime asOf, RawMarketData MarketData) : base(asOf, MarketData)
    //    {

    //    }

    //    protected override void ParseQuote()
    //    {

    //    }

    //}
    //public class FutureQuote : MarketQuote
    //{
    //    public FutureQuote(DateTime asOf, RawMarketData MarketData) : base(asOf, MarketData)
    //    {

    //    }

    //    protected override void ParseQuote()
    //    {

    //    }
    //}
    //public class CashQuote : MarketQuote
    //{
    //    public CashQuote(DateTime asOf, RawMarketData MarketData) : base(asOf, MarketData)
    //    {

    //    }

    //    protected override void ParseQuote()
    //    {

    //    }
    //}



    //public class OldMarketDataQuote
    //{
    //    MarketDataInstrument InstrumentType;
    //    CurveTenor CurveType;
    //    Asset InstrumentObject;
    //    public string InstrumentString;

    //    public OldMarketDataQuote(string Identifier, string TypeIdent, string CurveIdent)
    //    {
    //        InstrumentType = StrToEnum.TypeIdent(TypeIdent);
    //        CurveType = StrToEnum.CurveIdent(CurveIdent);

    //        switch (InstrumentType)
    //        {
    //            case MarketDataInstrument.IrSwapRate:
    //                ParseSwap(Identifier);
    //                break;
    //            default:
    //                throw new ArgumentException("CANNOT FIND INSTRUMENT TYPE.");
    //        }
    //    }

    //    public void ParseBaseSpread(string Identifier)
    //    {

    //    }

    //    public void ParseSwap(string Identifier)
    //    {
    //        string Temp;
    //        string Currency = Identifier.Left(3);
    //        string SwapType;

    //        if (Identifier.Replace(Currency, "").Left(3) == "EON")
    //        {
    //            SwapType = "OVERNIGHT";
    //            Temp = Identifier.Right(Identifier.Length - 6);
    //            string MaturityTenor = Temp;
    //            InstrumentString = "SWAP " + Currency + " " + SwapType + " " + MaturityTenor;
    //        }
    //        else
    //        {
    //            SwapType = "VANILLA";
    //            Temp = Identifier.Right(Identifier.Length - 5);

    //            string ReferenceTenor = Temp.Left(1);
    //            Temp = Temp.Right(Temp.Length - 2);
    //            string MaturityTenor = Temp;

    //            InstrumentString = "SWAP " + Currency + " " + SwapType + " " + ReferenceTenor + " " + MaturityTenor;
    //        }


    //    }

    //}



}
