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
        public QuoteType Type;
        public double QuoteValue;

        public InstrumentQuote(string identifier, QuoteType type, double quoteValue)
        {
            this.Identifier = identifier;
            this.Type = type;
            this.QuoteValue = quoteValue;
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

        public IDictionary<string, InstrumentQuote> Quotes;

        public InstrumentFactory(DateTime asOf)
        {
            Fras = new Dictionary<string, Fra>();
            Futures = new Dictionary<string, Future>();
            IrSwaps = new Dictionary<string, IrSwap>();
            OisSwaps = new Dictionary<string, OisSwap>();
            BasisSwaps = new Dictionary<string, BasisSwap>();
            Quotes = new Dictionary<string, InstrumentQuote>();
            AsOf = asOf;

        }

        public void AddQuotes(string[] identifiers, double[] quotes)
        {

        }

        public void AddQuote(string identifier, double quote)
        {

        }

        public void AddSwaps(string[] swapString)
        {
            for (int i = 0; i < swapString.Length; i++)
                InterpretSwapString(swapString[i]);
        }

        public void AddFras(string[] fraString)
        {
            for (int i = 0; i < fraString.Length; i++)
                InterpretFraString(fraString[i]);
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
            curveTenor = StrToEnum.CurveTenorFromSimpleTenor(floatPayFreq);
            fwdTenor = infoArray[15];
            dayCount = StrToEnum.DayCountConvert(infoArray[11]);

            if (type == "DEPOSIT")
            {
                // handle deposits
            }
            else
            {
                // handle FRA
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
            startDate = Calender.AddTenor(AsOf, settlementLag, dayRule);
            endDate = Calender.AddTenor(AsOf, endTenor, dayRule);
            double fixedRate = 0.01;

            if (referenceIndex == "EONIA")
            {
                // Handle OIS case
                OisSwap oisSwap = new OisSwap(AsOf, startDate, endTenor, fixedRate, fixedDayCount, floatDayCount, dayRule, dayRule, 1);
                OisSwaps[identifier] = oisSwap;
            }
            else
            {
                // Handle non-OIS case
                IrSwap swap = new IrSwap(AsOf, startDate, endDate, fixedRate, fixedTenor, floatTenor, fixedDayCount, floatDayCount, dayRule, dayRule, 1.0, 0.0);
                IrSwaps[identifier] = swap;
            }

        }

    }
}
