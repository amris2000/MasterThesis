using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis.Extensions;

namespace MasterThesis
{
    public static class StrToEnum
    {
        public static CurveTenor CurveIdent(string CurveIdent)
        {
            switch(CurveIdent)
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
            switch(TypeIdent)
            {
                case "SWAP":
                    return MarketDataInstrument.Swap;
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
                default:
                    throw new ArgumentException("CANNOT FIND MARKETDATAINSTRUMENT FOR INPUT STRING.");
            }
        }
    }

    public class RawMarketData
    {
        public string Identifier, TypeIdent, CurveIdent;
        public MarketDataInstrument MarketDataInstrument;
        public CurveTenor AsInputFor;
        public double Quote;

        public RawMarketData(string Identifier, string TypeIdent, string CurveIdent, double Quote)
        {
            this.Identifier = Identifier;
            this.TypeIdent = TypeIdent;
            this.CurveIdent = CurveIdent;
            MarketDataInstrument = StrToEnum.TypeIdent(TypeIdent);
            AsInputFor = StrToEnum.CurveIdent(CurveIdent);
            this.Quote = Quote;
        }
    }

    public static class QuoteFactory
    {
        public static MarketQuote CreateMarketQuote(RawMarketData MarketData)
        {
            switch(MarketData.MarketDataInstrument)
            {
                case MarketDataInstrument.Swap:
                        if (IsOisSwap(MarketData.Identifier))
                            return new OisSwapQuote(MarketData);
                        else
                            return new SwapQuote(MarketData);
                case MarketDataInstrument.BaseSpread:
                    return new BaseSpreadQuote(MarketData);
                case MarketDataInstrument.BasisSwap:
                    return new BasisSwapQuote(MarketData);
                case MarketDataInstrument.Fra:
                    return new FraQuote(MarketData);
                case MarketDataInstrument.Future:
                    return new FutureQuote(MarketData);
                case MarketDataInstrument.Cash:
                    return new CashQuote(MarketData);
                default:
                    throw new ArgumentException("Cannot find definition for input");
            }
        }

        public static bool IsOisSwap(string Identifier)
        {
            if (Identifier.Contains("EURE"))
                return true;
            else
                return false;
        }
    }
    
    public abstract class MarketQuote
    {
        public double Quote;
        public string Identifier;
        public string InstrumentString;
        public Instrument Instrument;
        public CurveTenor AsInputFor;
        public MarketDataInstrument InstrumentType;
        public RawMarketData MarketData;

        protected MarketQuote(RawMarketData MarketData)
        {
            this.Quote = MarketData.Quote;
            this.Identifier = MarketData.Identifier;
            this.InstrumentType = MarketData.MarketDataInstrument;
            this.AsInputFor = MarketData.AsInputFor;
            this.MarketData = MarketData;
        }
        protected abstract void ParseQuote();
    }


    public class SwapQuote : MarketQuote
    {
        // I.e. EURAB6E1Y (QuoteType = Vanilla), EUR4X1S (QutoeType = ShortSwap)
        string MaturityTenor;
        string Currency;
        SwapQuoteType QuoteType;
        CurveTenor FloatFreq;

        public SwapQuote(RawMarketData MarketData) : base (MarketData)
        {
            if (Identifier.Contains("X"))
                QuoteType = SwapQuoteType.ShortSwap;
            else
                QuoteType = SwapQuoteType.Vanilla;

            ParseQuote();
        }

        protected override void ParseQuote()
        {
            if (QuoteType == SwapQuoteType.Vanilla)
            {
                string Temp;
                Currency = this.MarketData.Identifier.Left(3);

                Temp = Identifier.Right(Identifier.Length - 5);
                string ReferenceTenor = Temp.Left(1);

                switch (ReferenceTenor)
                {
                    // All of the used instruments of this identifier-type are 6M (there are 2 for 1M - Special conventions?? See documentation)
                    case "6":
                        this.FloatFreq = CurveTenor.Fwd6M;
                        break;
                    case "1":
                        this.FloatFreq = CurveTenor.Fwd1M;
                        break;
                    default:
                        throw new ArgumentException("SwapQuote does not have reference 6M libor");
                }

                Temp = Temp.Right(Temp.Length - 2);
                this.MaturityTenor = Temp;
            }
            else if (QuoteType == SwapQuoteType.ShortSwap)
            {
                // For 1M tenor
            }

        }
    }
    public class OisSwapQuote : MarketQuote
    {
        public OisSwapQuote(RawMarketData MarketData) : base (MarketData)
        {

        }

        protected override void ParseQuote()
        {

        }
    }
    public class BaseSpreadQuote : MarketQuote
    {
        public BaseSpreadQuote(RawMarketData MarketData) : base (MarketData)
        {

        }

       protected override void ParseQuote()
        {

        }

    }
    public class BasisSwapQuote : MarketQuote
    {
        public BasisSwapQuote(RawMarketData MarketData) : base (MarketData)
        {

        }

        protected override void ParseQuote()
        {

        }
    }
    public class FraQuote : MarketQuote
    {
        public FraQuote(RawMarketData MarketData) : base (MarketData)
        {

        }

        protected override void ParseQuote()
        {

        }

    }
    public class FutureQuote : MarketQuote
    {
        public FutureQuote(RawMarketData MarketData) : base (MarketData)
        {

        }

        protected override void ParseQuote()
        {

        }
    }
    public class CashQuote : MarketQuote
    {
        public CashQuote(RawMarketData MarketData) : base (MarketData)
        {

        }

        protected override void ParseQuote()
        {

        }
    }



    public class OldMarketDataQuote
    {
        MarketDataInstrument InstrumentType;
        CurveTenor CurveType;
        Instrument InstrumentObject;
        public string InstrumentString;

        public OldMarketDataQuote(string Identifier, string TypeIdent, string CurveIdent)
        {
            InstrumentType = StrToEnum.TypeIdent(TypeIdent);
            CurveType = StrToEnum.CurveIdent(CurveIdent);

            switch(InstrumentType)
            {
                case MarketDataInstrument.Swap:
                    ParseSwap(Identifier);
                    break;
                default:
                    throw new ArgumentException("CANNOT FIND INSTRUMENT TYPE.");
            }
        }

        public void ParseBaseSpread(string Identifier)
        {

        }

        public void ParseSwap(string Identifier)
        {
            string Temp;
            string Currency = Identifier.Left(3);
            string SwapType;

            if (Identifier.Replace(Currency, "").Left(3) == "EON")
            {
                SwapType = "OVERNIGHT";
                Temp = Identifier.Right(Identifier.Length - 6);
                string MaturityTenor = Temp;
                InstrumentString = "SWAP " + Currency + " " + SwapType + " " + MaturityTenor;
            }
            else
            {
                SwapType = "VANILLA";
                Temp = Identifier.Right(Identifier.Length - 5);

                string ReferenceTenor = Temp.Left(1);
                Temp = Temp.Right(Temp.Length - 2);
                string MaturityTenor = Temp;

                InstrumentString = "SWAP " + Currency + " " + SwapType + " " + ReferenceTenor + " " + MaturityTenor;
            }


        }

    }


    public class InputInstrument
    {
        Instrument Instrument;
        double Quote;
        InstrumentType Type;
        InstrumentComplexity Complexity;
    }
}
