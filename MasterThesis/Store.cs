using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
    public static class Store
    {
        public static IDictionary<string, FwdCurve> FwdCurves = new Dictionary<string, FwdCurve>();
        public static IDictionary<string, DiscCurve> DiscCurves = new Dictionary<string, DiscCurve>();
        public static IDictionary<string, LinearRateModel> LinearRateModels = new Dictionary<string, LinearRateModel>();
        public static IDictionary<string, FwdCurves> FwdCurveCollections = new Dictionary<string, FwdCurves>();


        public static IDictionary<string, RawMarketData> RawMarketData = new Dictionary<string, RawMarketData>();
        public static IDictionary<string, MarketDataInstrument> MarketDataInstruments = new Dictionary<string, MarketDataInstrument>();

    }
}
