using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MasterThesis;
using MasterThesis.Extensions;

namespace Sandbox
{
    public static class CurveCalibrationTests
    {

        public static void SimpleBootStrap()
        {
            var sw = new Stopwatch();

            Logging.WriteSectionHeader("Simple bootstrap curve test");

            sw.Start();
            DateTime AsOf = new DateTime(2017, 1, 31);
            List<RawMarketData> rawMarketData = new List<RawMarketData>();

            //rawMarketData.Add(new RawMarketData(AsOf, "R6M", "FIXING", "6M", -0.001));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E6M", "SWAP", "6M", -0.0015)); // Synthetic
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E1Y", "SWAP", "6M", -0.00216));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E18M", "SWAP", "6M", -0.001819));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E2Y", "SWAP", "6M", -0.001436));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E3Y", "SWAP", "6M", -0.000439));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E4Y", "SWAP", "6M", -0.000762));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E5Y", "SWAP", "6M", 0.0021));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E6Y", "SWAP", "6M", 0.003439));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E7Y", "SWAP", "6M", 0.004761));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E8Y", "SWAP", "6M", 0.006078));
            rawMarketData.Add(new RawMarketData(AsOf, "EURAB6E9Y", "SWAP", "6M", 0.007319));

            List<MarketQuote> Quotes = QuoteFactory.CreateMarketQuoteCollection(rawMarketData);

            CurveFactory Factory = new CurveFactory(Quotes, CurveTenor.DiscOis);
            Curve MyCurve = Factory.BootstrapCurve();

            sw.Stop();
            Console.WriteLine("Run-time: " + sw.ElapsedMilliseconds);

        }

        public static void OisBootStrap()
        {
            Logging.WriteSectionHeader("Bootstrapping OIS curve test");

            var sw = new Stopwatch();

            sw.Start();
            DateTime AsOf = new DateTime(2017, 1, 31);
            List<RawMarketData> OisBootstrap = new List<RawMarketData>();

            OisBootstrap.Add(new RawMarketData(AsOf, "EUREONON", "SWAP", "OIS", -0.0035));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREONTN", "SWAP", "OIS", -0.0035));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREONSW", "SWAP", "OIS", -0.00352));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON2W", "SWAP", "OIS", -0.00352));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON3W", "SWAP", "OIS", -0.00352));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON1M", "SWAP", "OIS", -0.00351));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON2M", "SWAP", "OIS", -0.0035));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON3M", "SWAP", "OIS", -0.0035));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON4M", "SWAP", "OIS", -0.0035));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON5M", "SWAP", "OIS", -0.00348));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON6M", "SWAP", "OIS", -0.003471));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON7M", "SWAP", "OIS", -0.00345));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON8M", "SWAP", "OIS", -0.00343));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON9M", "SWAP", "OIS", -0.00341));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON10M", "SWAP", "OIS", -0.00326));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON11M", "SWAP", "OIS", -0.00304));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON1Y", "SWAP", "OIS", -0.002273));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON18M", "SWAP", "OIS", -0.001282));
            OisBootstrap.Add(new RawMarketData(AsOf, "EUREON2Y", "SWAP", "OIS", -0.000112));

            List<MarketQuote> QuotesOis = QuoteFactory.CreateMarketQuoteCollection(OisBootstrap);

            CurveFactory FactoryOis = new CurveFactory(QuotesOis, CurveTenor.DiscOis);
            Curve MyCurve2 = FactoryOis.BootstrapCurve();

        }
    }
}
