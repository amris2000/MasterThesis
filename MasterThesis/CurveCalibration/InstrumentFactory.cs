using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterThesis;

namespace MasterThesis
{
    public static class InstrumentFactoryHeaders
    {
        // Used for inspecting instrument schedule in Excel Layer.
        // See the "CheckInstrumentSchedule" sheet.

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

    /* General information:
    *  The instrumentfactory contains a number of derivative objects
    *  and a map between names and the objects. This is used when we 
    *  choose the instruments that makes up the calibration procedure
    *  from the Excel-interface. This way, we can associate an identifier,
    *  i.e. EURAB6E15Y for a 15Y fixed-for-floating swap referencing 6M EURIBOR,
    *  with an actual C# object created from a string of parameter. The class
    *  contains members to parse strings into objects.
    */
    public class InstrumentFactory
    {
        public IDictionary<string, Fra> Fras;
        public IDictionary<string, Futures> Futures;
        public IDictionary<string, IrSwap> IrSwaps;
        public IDictionary<string, TenorBasisSwap> BasisSwaps;
        public IDictionary<string, OisSwap> OisSwaps;
        public IDictionary<string, Deposit> Deposits;
        public DateTime AsOf;

        public IDictionary<string, QuoteType> InstrumentTypeMap;
        public IDictionary<string, InstrumentFormatType> InstrumentFormatTypeMap;
        public IDictionary<string, string> IdentifierStringMap;
        public IDictionary<string, DateTime> CurvePointMap;

        // Default values. We set notional to 1 so that our
        // outright risk calculations tells us the notional
        // at which we should trade a given instrument. 
        // The fixedRate does not matter in the calibration, and will be updated
        // to par ex post (could set it equal to the quote, but that would require a coupling
        // between two classes). Default trade sign is "pay whatever" i.e. the fixedLeg or the spreadLeg.
        private double _defaultNotional = 1.0;
        private int _defaultTradeSign = 1;
        private double _defaultFixedRate = 0.01;

        public InstrumentFactory(DateTime asOf)
        {
            Fras = new Dictionary<string, Fra>();
            Futures = new Dictionary<string, Futures>();
            IrSwaps = new Dictionary<string, IrSwap>();
            OisSwaps = new Dictionary<string, OisSwap>();
            BasisSwaps = new Dictionary<string, TenorBasisSwap>();
            Deposits = new Dictionary<string, Deposit>();
            InstrumentTypeMap = new Dictionary<string, QuoteType>();
            InstrumentFormatTypeMap = new Dictionary<string, InstrumentFormatType>();
            IdentifierStringMap = new Dictionary<string, string>();
            CurvePointMap = new Dictionary<string, DateTime>();
            AsOf = asOf;
        }

        public void UpdateAllInstrumentsToParGivenModel(LinearRateModel model)
        {
            // Given a linear rate model, updates fixed rates of all 
            // instruments in factory to par. This is used when we calculate risk.
            // We want to hedge with liquid instruments, and trades are usually
            // initiated at par. 

            foreach (string key in Fras.Keys)
                Fras[key].UpdateFixedRateToPar(model);

            foreach (string key in Futures.Keys)
                Futures[key].UpdateFixedRateToPar(model);

            foreach (string key in IrSwaps.Keys)
                IrSwaps[key].UpdateFixedRateToPar(model);

            foreach (string key in BasisSwaps.Keys)
                BasisSwaps[key].UpdateFixedRateToPar(model);

            foreach (string key in OisSwaps.Keys)
                OisSwaps[key].UpdateFixedRateToPar(model);

            foreach (string key in Deposits.Keys)
                Deposits[key].UpdateFixedRateToPar(model);
        }

        public double ValueInstrumentFromFactory(LinearRateModel model, string instrument)
        {
            // Values an instrument contained in the instrument factory using a 
            // linear rate model. Note that value here means calculating the value
            // par rate (which is how trades are quoted).

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
                case QuoteType.Deposit:
                    return model.ParDepositRate(Deposits[instrument]);
                default:
                    throw new InvalidOperationException("Instrument QuoteType not supported...");
            }
        }

        public ADouble ValueInstrumentFromFactoryAD(LinearRateModel model, string instrument)
        {
            // Values an instrument contained in the instrument factory using a 
            // linear rate model. Note that value here means calculating the value
            // par rate (which is how trades are quoted).

            QuoteType type = InstrumentTypeMap[instrument];

            switch (type)
            {
                case QuoteType.ParSwapRate:
                    return model.IrParSwapRateAD(IrSwaps[instrument]);
                case QuoteType.ParBasisSpread:
                    return model.ParBasisSpreadAD(BasisSwaps[instrument]);
                case QuoteType.OisRate:
                    return model.OisRateSimpleAD(OisSwaps[instrument]);
                case QuoteType.FraRate:
                    return model.ParFraRateAD(Fras[instrument]);
                case QuoteType.FuturesRate:
                    return model.ParFutureRateAD(Futures[instrument]);
                case QuoteType.Deposit:
                    return model.ParDepositRateAD(Deposits[instrument]);
                default:
                    throw new InvalidOperationException("Instrument QuoteType not supported...");
            }
        }

        #region Add instruments to factory from strings
        /* Functions below takes as input an array of strings in a given format
         * and then parses the string to parameters to be used in the construction
         * of instances of interest rate derivative objects.
         */

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
                InterpretTenorBasisSwapString(swapString[i]);
        }

        public void AddFras(string[] fraString)
        {
            for (int i = 0; i < fraString.Length; i++)
                InterpretFraString(fraString[i]);
        }
        #endregion

        #region Parsin functionality
        /* Functions below are the actual functions used to 
         * parse instrument strings into actual objects. We've created
         * a method for each of the instrument classes we consider.
         * Note that the string has to have a very specific format.
         */

        private void InterpretTenorBasisSwapString(string instrumentString)
        {
            string identifier, type, currency, swapNoSpreadIdent, swapSpreadIdent;
            string[] infoArray = instrumentString.Split(',').ToArray();

            identifier = infoArray[0];
            type = infoArray[1];
            currency = infoArray[2];

            // In accordance with market practice, we put the spread on the short leg
            swapSpreadIdent = infoArray[3];
            swapNoSpreadIdent = infoArray[4];

            try
            {
                IrSwap swapNoSpread = IrSwaps[swapNoSpreadIdent];
                IrSwap swapSpread = IrSwaps[swapSpreadIdent];
                TenorBasisSwap swap = new TenorBasisSwap(swapSpread, swapNoSpread, _defaultTradeSign);
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

            DateTime startDate, endDate;

            try
            {
                startDate = Convert.ToDateTime(fwdTenor);
                endDate = DateHandling.AddTenorAdjust(startDate, floatPayFreq, dayRule);

                curveTenor = StrToEnum.CurveTenorFromSimpleTenor(floatPayFreq);
                Fra fra = new MasterThesis.Fra(AsOf, startDate, endDate, curveTenor, dayCount, dayRule, _defaultFixedRate, _defaultNotional, _defaultTradeSign);
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

            if (type == "DEPOSIT")
            {
                //handle deposits - only used for LIBOR discounting. Not implemented
            }
            else
            {
                // handle FRA
                // Has to consider both FwdTenor and SettlementLag here..
                curveTenor = StrToEnum.CurveTenorFromSimpleTenor(floatPayFreq);
                Fra fra = new MasterThesis.Fra(AsOf, fwdTenor, endTenor, curveTenor, dayCount, dayRule, _defaultFixedRate, _defaultNotional, _defaultTradeSign);
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

            try
            {
                if (referenceIndex == "EONIA")
                {
                    // Handle OIS case
                    // Error with endTenor here and string parsing 

                    // TEMPORARY
                    //settlementLag = "0D";
                    //dayRule = DayRule.F;

                    // This is a dirty hack to value deposits
                    if (identifier == "EUREONON" || identifier == "EUREONTN")
                    {
                        Deposit deposit = new Deposit(AsOf, startTenor, endTenor, settlementLag, _defaultFixedRate, floatDayCount, dayRule, _defaultNotional, _defaultTradeSign);
                        Deposits[identifier] = deposit;
                        CurvePointMap[identifier] = deposit.GetCurvePoint();
                        InstrumentTypeMap[identifier] = QuoteType.Deposit;
                        InstrumentFormatTypeMap[identifier] = InstrumentFormatType.Swaps;
                        IdentifierStringMap[identifier] = instrumentString;
                    }
                    else
                    {
                        OisSwap oisSwap = new OisSwap(AsOf, startTenor, endTenor, settlementLag, fixedDayCount, floatDayCount, dayRule, _defaultNotional, _defaultFixedRate, _defaultTradeSign);
                        OisSwaps[identifier] = oisSwap;
                        CurvePointMap[identifier] = oisSwap.GetCurvePoint();
                        InstrumentTypeMap[identifier] = QuoteType.OisRate;
                        InstrumentFormatTypeMap[identifier] = InstrumentFormatType.Swaps;
                        IdentifierStringMap[identifier] = instrumentString;
                    }



                }
                else
                {
                    // Handle non-OIS case
                    IrSwap swap = new IrSwap(AsOf, startDate, endDate, _defaultFixedRate, fixedTenor, floatTenor, fixedDayCount, floatDayCount, dayRule, dayRule, _defaultNotional, _defaultTradeSign, 0.0);
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
        #endregion
    }
}
