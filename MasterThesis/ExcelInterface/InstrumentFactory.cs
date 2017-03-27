using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis.ExcelInterface
{
    public class InstrumentQuote
    {
        public string Identifier;
        public QuoteTypeOld Type;
        public double QuoteValue;
        public DateTime CurvePoint;

        public InstrumentQuote(string identifier, QuoteTypeOld type, DateTime curvePoint, double quoteValue)
        {
            this.Identifier = identifier;
            this.Type = type;
            this.QuoteValue = quoteValue;
            this.CurvePoint = curvePoint;
        }
    }

    // TO DO: ADD AUTOMATIC FETCHING OF QUOTE TYPE BASED ON SOMETHING
    // Not really sure InstrumentFactory should know anything about quotes. Think abut it.
    public class InstrumentFactory
    {
        public IDictionary<string, Fra> Fras;
        public IDictionary<string, Future> Futures;
        public IDictionary<string, IrSwap> IrSwaps;
        public IDictionary<string, BasisSwap> BasisSwaps;
        public IDictionary<string, OisSwap> OisSwaps;
        public DateTime AsOf;

        // Probably remove this: should not know anything about quotes
        public IDictionary<string, InstrumentQuote> Quotes;

        public IDictionary<string, QuoteType> InstrumentTypeMap;

        public InstrumentFactory(DateTime asOf)
        {
            Fras = new Dictionary<string, Fra>();
            Futures = new Dictionary<string, Future>();
            IrSwaps = new Dictionary<string, IrSwap>();
            OisSwaps = new Dictionary<string, OisSwap>();
            BasisSwaps = new Dictionary<string, BasisSwap>();
            Quotes = new Dictionary<string, InstrumentQuote>();
            InstrumentTypeMap = new Dictionary<string, QuoteType>();
            AsOf = asOf;

        }

        public void AddQuotes(string[] identifiers, double[] quotes)
        {

        }

        public void AddQuote(string identifier, double quote)
        {

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

        private void InterpretSpreadString(string spreadString)
        {
            string identifier, type, currency, swapNoSpreadIdent, swapSpreadIdent;

            string[] infoArray = spreadString.Split(',').ToArray();

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
                InstrumentTypeMap[identifier] = QuoteType.ParBasisSpread;
            }
            catch
            {
                // Ignore instrument
            }

        }

        private void InterpretFuturesString(string futureString)
        {
            string identifier, type, currency, endTenor, settlementLag, floatPayFreq, floatFixingTenor, fwdTenor;
            DayRule dayRule;
            DayCount dayCount;
            CurveTenor curveTenor;

            string[] infoArray = futureString.Split(',').ToArray();

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
                endDate = Calender.AddTenor(startDate, floatPayFreq, dayRule);

                curveTenor = StrToEnum.CurveTenorFromSimpleTenor(floatPayFreq);
                Fra fra = new MasterThesis.Fra(AsOf, startDate, endDate, curveTenor, dayCount, dayRule, fixedRate);
                Futures[identifier] = new MasterThesis.Future(fra, null);
                InstrumentTypeMap[identifier] = QuoteType.FuturesRate;
            }
            catch
            {
                // Ignore instrument
            }

        }

        private void InterpretFraString(string fraString)
        {
            string identifier, type, currency, endTenor, settlementLag, floatPayFreq, floatFixingTenor, fwdTenor;
            DayRule dayRule;
            DayCount dayCount;
            CurveTenor curveTenor;

            string[] infoArray = fraString.Split(',').ToArray();

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
                // handle deposits
                InstrumentTypeMap[identifier] = QuoteType.Deposit;
            }
            else
            {
                // handle FRA
                // Has to consider both FwdTenor and SettlementLag here..
                curveTenor = StrToEnum.CurveTenorFromSimpleTenor(floatPayFreq);
                Fra fra = new MasterThesis.Fra(AsOf, fwdTenor, endTenor, curveTenor, dayCount, dayRule, fixedRate);
                Fras[identifier] = fra;
                InstrumentTypeMap[identifier] = QuoteType.FraRate;
            }
        }

        private bool StrIsConvertableToDate(string str)
        {
            try
            {
                DateTime myDate = Convert.ToDateTime(str);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void InterpretSwapString(string swapString)
        {
            string identifier, type, currency, startTenor, endTenor, settlementLag, fixedFreq, floatFreq, referenceIndex;
            DayRule dayRule;
            DayCount floatDayCount, fixedDayCount;
            CurveTenor floatTenor, fixedTenor;

            string[] infoArray = swapString.Split(',').ToArray();

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
            if (StrIsConvertableToDate(startTenor))
                startDate = Convert.ToDateTime(startTenor);
            else
                startDate = Calender.AddTenor(AsOf, settlementLag, dayRule);

            if (StrIsConvertableToDate(endTenor))
                endDate = Convert.ToDateTime(endTenor);
            else
                endDate = Calender.AddTenor(AsOf, endTenor, dayRule);

            double fixedRate = 0.01;

            try
            {
                if (referenceIndex == "EONIA")
                {
                    // Handle OIS case
                    // Error with endTenor here and string parsing 
                    OisSwap oisSwap = new OisSwap(AsOf, startDate, endTenor, fixedRate, fixedDayCount, floatDayCount, dayRule, dayRule, 1);
                    OisSwaps[identifier] = oisSwap;
                    InstrumentTypeMap[identifier] = QuoteType.OisRate;
                }
                else
                {
                    // Handle non-OIS case
                    IrSwap swap = new IrSwap(AsOf, startDate, endDate, fixedRate, fixedTenor, floatTenor, fixedDayCount, floatDayCount, dayRule, dayRule, 1.0, 0.0);
                    IrSwaps[identifier] = swap;
                    InstrumentTypeMap[identifier] = QuoteType.ParSwapRate;
                }
            } 
            catch
            {
                // Ignore instrument.
            }
        }

    }
}
