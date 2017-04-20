using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis;

namespace MasterThesis
{
    /// <summary>
    /// Used for inspecting instrument schedule in Excel Layer.
    /// </summary>
    public static class InstrumentFactoryHeaders
    {
        public static IDictionary<InstrumentFormatType, string> GetHeaders;

        static InstrumentFactoryHeaders()
        {
            GetHeaders = new Dictionary<InstrumentFormatType, string>();

            GetHeaders[InstrumentFormatType.Swaps] = "Id,InstrumentType,Currency,Start,End,SettlementLag,DayRule,PaymentHoliday,FixingHoliday,FixedPayFreq,FloatPayFreq,FloatFixingTenor,FloatFixingIndex,FixedDayCount,FloatDaycount";
            GetHeaders[InstrumentFormatType.Fras] = "Id,InstrumentType,Currency,Tenor,SettlementLag,DayRule,PaymentHoliday,FixingHoliday,FloatPayFreq,FloatFixingTenor,FloatFixingIndex,FloatDaycount,BaseHoliday,FraType,Interpreter,FwdTenor,QToolkitName";
            GetHeaders[InstrumentFormatType.Futures] = "Id,InstrumentType,Currency,Tenor,SettlementLag,DayRule,PaymentHoliday,FixingHoliday,FloatPayFreq,ConventionTenor,FloatFixingIndex,FloatDaycount,BaseHoliday,FraType,Interpreter,StartDate,QToolkitName";
            GetHeaders[InstrumentFormatType.BasisSpreads] = "Id,InstrumentType,Currency,ModSide,BaseSide";
            GetHeaders[InstrumentFormatType.FwdStartingSwaps] = "Id,InstrumentType,Currency,Start,End,SettlementLag,DayRule,PaymentHoliday,FixingHoliday,FixedPayFreq,FloatPayFreq,FloatFixingTenor,FloatFixingIndex,FixedDayCount,FloatDaycount";
        }
    }

    // TO DO: ADD AUTOMATIC FETCHING OF QUOTE TYPE BASED ON SOMETHING
    // Not really sure InstrumentFactory should know anything about quotes. Think abut it.
    public class InstrumentFactory
    {
        public IDictionary<string, Fra> Fras;
        public IDictionary<string, Futures> Futures;
        public IDictionary<string, IrSwap> IrSwaps;
        public IDictionary<string, BasisSwap> BasisSwaps;
        public IDictionary<string, OisSwap> OisSwaps;
        public DateTime AsOf;

        public IDictionary<string, QuoteType> InstrumentTypeMap;
        public IDictionary<string, InstrumentFormatType> InstrumentFormatTypeMap;
        public IDictionary<string, string> IdentifierStringMap;
        public IDictionary<string, DateTime> CurvePointMap;
        private double _notional = 10000000.0;

        public InstrumentFactory(DateTime asOf)
        {
            Fras = new Dictionary<string, Fra>();
            Futures = new Dictionary<string, Futures>();
            IrSwaps = new Dictionary<string, IrSwap>();
            OisSwaps = new Dictionary<string, OisSwap>();
            BasisSwaps = new Dictionary<string, BasisSwap>();
            InstrumentTypeMap = new Dictionary<string, QuoteType>();
            InstrumentFormatTypeMap = new Dictionary<string, InstrumentFormatType>();
            IdentifierStringMap = new Dictionary<string, string>();
            CurvePointMap = new Dictionary<string, DateTime>();
            AsOf = asOf;
        }

        public double ValueInstrumentFromFactory(LinearRateModel model, string instrument)
        {
            QuoteType type = InstrumentTypeMap[instrument];

            switch (type)
            {
                case QuoteType.ParSwapRate:
                    return model.IrParSwapRate(IrSwaps[instrument]);
                case QuoteType.ParBasisSpread:
                    return model.ParBasisSpread(BasisSwaps[instrument]);
                case QuoteType.OisRate:
                    return model.OisRateSimple(OisSwaps[instrument]);
                case QuoteType.FraRate:
                    return model.ParFraRate(Fras[instrument]);
                case QuoteType.FuturesRate:
                    return model.ParFutureRate(Futures[instrument]);
                default:
                    throw new InvalidOperationException("Instrument QuoteType not supported...");
            }
        }

        public void AddFwdStartingSwaps(string[] swapString)
        {
            for (int i = 0; i < swapString.Length; i++)
                InterpretSwapString(swapString[i]);
        }

        public void AddFutures(string[] futuresString)
        {
            for (int i = 0; i < futuresString.Length; i++)
                InterpretFuturesString(futuresString[i]);
        }

        public void AddSwaps(string[] swapString)
        {
            for (int i = 0; i < swapString.Length; i++)
                InterpretSwapString(swapString[i]);
        }

        public void AddBasisSwaps(string[] swapString)
        {
            for (int i = 0; i < swapString.Length; i++)
                InterpretSpreadString(swapString[i]);
        }

        public void AddFras(string[] fraString)
        {
            for (int i = 0; i < fraString.Length; i++)
                InterpretFraString(fraString[i]);
        }

        private void InterpretSpreadString(string instrumentString)
        {
            string identifier, type, currency, swapNoSpreadIdent, swapSpreadIdent;
            string[] infoArray = instrumentString.Split(',').ToArray();

            identifier = infoArray[0];
            type = infoArray[1];
            currency = infoArray[2];
            swapNoSpreadIdent = infoArray[3];
            swapSpreadIdent = infoArray[4];


            try
            {
                IrSwap swapNoSpread = IrSwaps[swapNoSpreadIdent];
                IrSwap swapSpread = IrSwaps[swapSpreadIdent];
                BasisSwap swap = new BasisSwap(swapNoSpread, swapSpread);
                BasisSwaps[identifier] = swap;
                DateTime curvePoint = swap.GetCurvePoint();
                CurvePointMap[identifier] = swap.GetCurvePoint();
                InstrumentTypeMap[identifier] = QuoteType.ParBasisSpread;
                InstrumentFormatTypeMap[identifier] = InstrumentFormatType.BasisSpreads;
                IdentifierStringMap[identifier] = instrumentString;

            }
            catch
            {
                // Ignore instrument
            }
        }

        private void InterpretFuturesString(string instrumentString)
        {
            string identifier, type, currency, endTenor, settlementLag, floatPayFreq, floatFixingTenor, fwdTenor;
            DayRule dayRule;
            DayCount dayCount;
            CurveTenor curveTenor;

            string[] infoArray = instrumentString.Split(',').ToArray();

            identifier = infoArray[0];
            type = infoArray[1];
            currency = infoArray[2];
            endTenor = infoArray[3];
            settlementLag = infoArray[4];
            dayRule = StrToEnum.DayRuleConvert(infoArray[5]);
            floatPayFreq = infoArray[8];
            floatFixingTenor = infoArray[9];
            fwdTenor = infoArray[15];
            dayCount = StrToEnum.DayCountConvert(infoArray[11]);
            double fixedRate = 0.01;

            DateTime startDate, endDate;

            try
            {
                startDate = Convert.ToDateTime(fwdTenor);
                endDate = DateHandling.AddTenorAdjust(startDate, floatPayFreq, dayRule);

                curveTenor = StrToEnum.CurveTenorFromSimpleTenor(floatPayFreq);
                Fra fra = new MasterThesis.Fra(AsOf, startDate, endDate, curveTenor, dayCount, dayRule, fixedRate, _notional);
                Futures[identifier] = new MasterThesis.Futures(fra, null);
                CurvePointMap[identifier] = fra.GetCurvePoint();
                InstrumentTypeMap[identifier] = QuoteType.FuturesRate;
                InstrumentFormatTypeMap[identifier] = InstrumentFormatType.Futures;
                IdentifierStringMap[identifier] = instrumentString;
            }
            catch
            {
                // Ignore instrument
            }

        }

        private void InterpretFraString(string instrumentString)
        {
            string identifier, type, currency, endTenor, settlementLag, floatPayFreq, floatFixingTenor, fwdTenor;
            DayRule dayRule;
            DayCount dayCount;
            CurveTenor curveTenor;

            string[] infoArray = instrumentString.Split(',').ToArray();

            identifier = infoArray[0];
            type = infoArray[1];
            currency = infoArray[2];
            endTenor = infoArray[3];
            settlementLag = infoArray[4];
            dayRule = StrToEnum.DayRuleConvert(infoArray[5]);
            floatPayFreq = infoArray[8];
            floatFixingTenor = infoArray[9];
            fwdTenor = infoArray[15];
            dayCount = StrToEnum.DayCountConvert(infoArray[11]);
            double fixedRate = 0.01;

            if (type == "DEPOSIT")
            {
                // handle deposits - only used for LIBOR discounting.
                //curveTenor = StrToEnum.CurveTenorFromSimpleTenor(floatPayFreq);
                //Fra fra = new MasterThesis.Fra(AsOf, fwdTenor, endTenor, curveTenor, dayCount, dayRule, fixedRate);
                //Fras[identifier] = fra;
                //InstrumentTypeMap[identifier] = QuoteType.Deposit;
            }
            else
            {
                // handle FRA
                // Has to consider both FwdTenor and SettlementLag here..
                curveTenor = StrToEnum.CurveTenorFromSimpleTenor(floatPayFreq);
                Fra fra = new MasterThesis.Fra(AsOf, fwdTenor, endTenor, curveTenor, dayCount, dayRule, fixedRate, _notional);
                Fras[identifier] = fra;
                CurvePointMap[identifier] = fra.GetCurvePoint();
                InstrumentTypeMap[identifier] = QuoteType.FraRate;
                InstrumentFormatTypeMap[identifier] = InstrumentFormatType.Fras;
                IdentifierStringMap[identifier] = instrumentString;

            }
        }

        private void InterpretSwapString(string instrumentString)
        {
            string identifier, type, currency, startTenor, endTenor, settlementLag, fixedFreq, floatFreq, referenceIndex;
            DayRule dayRule;
            DayCount floatDayCount, fixedDayCount;
            CurveTenor floatTenor, fixedTenor;

            string[] infoArray = instrumentString.Split(',').ToArray();

            identifier = infoArray[0];
            type = infoArray[1];
            currency = infoArray[2];
            startTenor = infoArray[3];
            endTenor = infoArray[4];
            settlementLag = infoArray[5];
            dayRule = StrToEnum.DayRuleConvert(infoArray[6]);
            fixedFreq = infoArray[9];
            floatFreq = infoArray[10];
            referenceIndex = infoArray[12];
            floatDayCount = StrToEnum.DayCountConvert(infoArray[14]);
            fixedDayCount = StrToEnum.DayCountConvert(infoArray[13]);
            floatTenor = StrToEnum.CurveTenorFromSimpleTenor(floatFreq);
            fixedTenor = StrToEnum.CurveTenorFromSimpleTenor(fixedFreq);


            DateTime startDate, endDate;

            // Make sure to get fwd starting stuff right here...
            if (DateHandling.StrIsConvertableToDate(startTenor))
                startDate = Convert.ToDateTime(startTenor);
            else
                startDate = DateHandling.AddTenorAdjust(AsOf, settlementLag, dayRule);

            if (DateHandling.StrIsConvertableToDate(endTenor))
                endDate = Convert.ToDateTime(endTenor);
            else
                endDate = DateHandling.AddTenorAdjust(startDate, endTenor, dayRule);

            double fixedRate = 0.01;

            try
            {
                if (referenceIndex == "EONIA")
                {
                    // Handle OIS case
                    // Error with endTenor here and string parsing 
                    OisSwap oisSwap = new OisSwap(AsOf, startTenor, endTenor, settlementLag, fixedDayCount, floatDayCount, dayRule, _notional, fixedRate);
                    OisSwaps[identifier] = oisSwap;
                    CurvePointMap[identifier] = oisSwap.GetCurvePoint();
                    InstrumentTypeMap[identifier] = QuoteType.OisRate;
                    InstrumentFormatTypeMap[identifier] = InstrumentFormatType.Swaps;
                    IdentifierStringMap[identifier] = instrumentString;

                }
                else
                {
                    // Handle non-OIS case
                    IrSwap swap = new IrSwap(AsOf, startDate, endDate, fixedRate, fixedTenor, floatTenor, fixedDayCount, floatDayCount, dayRule, dayRule, _notional, 0.0);
                    IrSwaps[identifier] = swap;
                    CurvePointMap[identifier] = swap.GetCurvePoint();
                    InstrumentTypeMap[identifier] = QuoteType.ParSwapRate;
                    InstrumentFormatTypeMap[identifier] = InstrumentFormatType.Swaps;
                    IdentifierStringMap[identifier] = instrumentString;

                }
            } 
            catch
            {
                // Ignore instrument.
            }
        }

    }
}
