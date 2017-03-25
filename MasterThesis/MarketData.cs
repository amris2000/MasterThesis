﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis.Extensions;

namespace MasterThesis
{
    public static class StrToEnum
    {
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
    }

    public class RawMarketData
    {
        public string Identifier, TypeIdent, CurveIdent;
        public MarketDataInstrument MarketDataInstrument;
        public CurveTenor AsInputFor;
        public double Quote;
        public DateTime AsOf;

        public RawMarketData(DateTime asOf, string Identifier, string TypeIdent, string CurveIdent, double Quote)
        {
            this.Identifier = Identifier;
            this.TypeIdent = TypeIdent;
            this.CurveIdent = CurveIdent;
            this.AsOf = asOf;
            MarketDataInstrument = StrToEnum.TypeIdent(TypeIdent);
            AsInputFor = StrToEnum.CurveIdent(CurveIdent);
            this.Quote = Quote;
        }
    }

    public static class QuoteFactory
    {
        public static MarketQuote CreateMarketQuote(RawMarketData MarketData)
        {
            DateTime AsOf = MarketData.AsOf;
            switch (MarketData.MarketDataInstrument)
            {
                case MarketDataInstrument.IrSwapRate:
                    if (IsOisSwap(MarketData.Identifier))
                        return new OisSwapQuote(AsOf, MarketData);
                    else
                        return new SwapQuote(AsOf, MarketData);
                case MarketDataInstrument.BaseSpread:
                    return new BaseSpreadQuote(AsOf, MarketData);
                //case MarketDataInstrument.BasisSwap:
                //    return new BasisSwapQuote(AsOf, MarketData);
                case MarketDataInstrument.Fra:
                    return new FraQuote(AsOf, MarketData);
                case MarketDataInstrument.Future:
                    return new FutureQuote(AsOf, MarketData);
                case MarketDataInstrument.Cash:
                    return new CashQuote(AsOf, MarketData);
                case MarketDataInstrument.Fixing:
                    return new FixingQuote(AsOf, MarketData);
                default:
                    throw new ArgumentException("Cannot find definition for input");
            }
        }

        public static List<MarketQuote> CreateMarketQuoteCollection(List<RawMarketData> rawMarketData)
        {
            List<MarketQuote> Out = new List<MarketQuote>();

            for (int i = 0; i < rawMarketData.Count; i++)
                Out.Add(CreateMarketQuote(rawMarketData[i]));

            return Out;

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
        public Asset Instrument;
        public DateTime AsOf;
        public CurveTenor AsInputFor;
        public MarketDataInstrument InstrumentType;
        public RawMarketData MarketData;
        public DateTime StartDate, EndDate;

        protected MarketQuote(DateTime asOf, RawMarketData MarketData)
        {
            this.AsOf = asOf;
            this.Quote = MarketData.Quote;
            this.Identifier = MarketData.Identifier;
            this.InstrumentType = MarketData.MarketDataInstrument;
            this.AsInputFor = MarketData.AsInputFor;
            this.MarketData = MarketData;
        }
        protected abstract void ParseQuote();
    }

    public class FixingQuote : MarketQuote
    {
        public FixingQuote(DateTime asOf, RawMarketData MarketData) : base(asOf, MarketData)
        {
            StartDate = AsOf;
            EndDate = AsOf;
        }

        protected override void ParseQuote()
        {

        }
    }

    public class SwapQuote : MarketQuote
    {
        // I.e. EURAB6E1Y (QuoteType = Vanilla), EUR4X1S (QutoeType = ShortSwap)
        string MaturityTenor;
        string Currency;
        SwapQuoteType QuoteType;
        CurveTenor FloatFreq;

        public SwapQuote(DateTime asOf, RawMarketData MarketData) : base(asOf, MarketData)
        {
            if (Identifier.Contains("X"))
                QuoteType = SwapQuoteType.ShortSwap;
            else
                QuoteType = SwapQuoteType.Vanilla;

            ParseQuote();

            StartDate = asOf;
            EndDate = Calender.AddTenor(StartDate, MaturityTenor, DayRule.MF);
            Instrument = new SwapSimple(AsOf, StartDate, EndDate, 0.01, CurveTenor.Fwd1Y, CurveTenor.Fwd6M, DayCount.THIRTY360, DayCount.ACT360, DayRule.MF, DayRule.MF, 1);
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
                FloatFreq = CurveTenor.Fwd1M;
                string Temp = Identifier;
                Currency = Temp.Left(3);
                Temp = Temp.Replace(Currency, "");
                Temp = Temp.Replace("X1S", "");
                MaturityTenor = Temp + "M";
            }
        }
    }
    public class OisSwapQuote : MarketQuote
    {

        string Tenor;

        public OisSwapQuote(DateTime AsOf, RawMarketData MarketData) : base(AsOf, MarketData)
        {
            ParseQuote();
            string[] noFwdTenor = new string[] { "0B", "1B" };
            if (noFwdTenor.Contains(Tenor) == false)
                StartDate = Calender.AddTenor(AsOf, "2B", DayRule.F);
            else
                StartDate = AsOf;

            EndDate = Calender.AddTenor(StartDate, Tenor, DayRule.F);
            Instrument = new OisSwap(AsOf, StartDate, Tenor, 0.1, DayCount.ACT360, DayCount.ACT360, DayRule.MF, DayRule.MF, 1);
            InstrumentType = MarketDataInstrument.OisRate;
        }

        protected override void ParseQuote()
        {
            string Temp = Identifier.Replace("EUREON", "");
            
            switch(Temp)
            {
                case "ON":
                    Tenor = "1B";
                    break;
                case "TN":
                    Tenor = "2B";
                    break;
                case "SW":
                    Tenor = "1W";
                    break;
                default:
                    Tenor = Temp;
                    break;
            }
        }
    }
    public class BaseSpreadQuote : MarketQuote
    {
        public BaseSpreadQuote(DateTime asOf, RawMarketData MarketData) : base(asOf, MarketData)
        {

        }

        protected override void ParseQuote()
        {

        }

    }
    //public class BasisSwapQuote : MarketQuote
    //{
    //    public BasisSwapQuote(RawMarketData MarketData) : base(MarketData)
    //    {

    //    }

    //    protected override void ParseQuote()
    //    {

    //    }
    //}
    public class FraQuote : MarketQuote
    {
        public FraQuote(DateTime asOf, RawMarketData MarketData) : base(asOf, MarketData)
        {

        }

        protected override void ParseQuote()
        {

        }

    }
    public class FutureQuote : MarketQuote
    {
        public FutureQuote(DateTime asOf, RawMarketData MarketData) : base(asOf, MarketData)
        {

        }

        protected override void ParseQuote()
        {

        }
    }
    public class CashQuote : MarketQuote
    {
        public CashQuote(DateTime asOf, RawMarketData MarketData) : base(asOf, MarketData)
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
        Asset InstrumentObject;
        public string InstrumentString;

        public OldMarketDataQuote(string Identifier, string TypeIdent, string CurveIdent)
        {
            InstrumentType = StrToEnum.TypeIdent(TypeIdent);
            CurveType = StrToEnum.CurveIdent(CurveIdent);

            switch (InstrumentType)
            {
                case MarketDataInstrument.IrSwapRate:
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



}
