using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
    /* General information:
     * This file contains classes for all the linear interest rate
     * derivatives considered in the thesis. Also, the "LinearRateInstrument"
     * is defined here as well. This interface contains common functionality, that
     * a class has to implement in order to be a "LinearRateInstrument".
     */

    public interface LinearRateInstrument
    {
        DateTime GetCurvePoint();
        void UpdateFixedRateToPar(LinearRateModel model);
        Instrument GetInstrumentType();
    }

    public abstract class SwapLeg
    {
        public SwapSchedule Schedule;
        public CurveTenor Tenor;
        public DateTime AsOf, StartDate, EndDate;
        public double Notional;
        public DayRule DayRule;
        public DayCount DayCount;

        protected SwapLeg(DateTime asOf, DateTime startDate, DateTime endDate, CurveTenor referenceTenor, DayCount dayCount, DayRule dayRule, double notional)
        {
            Schedule = new MasterThesis.SwapSchedule(asOf, startDate, endDate, dayCount, dayRule, referenceTenor);
            this.Tenor = referenceTenor;
            this.DayRule = dayRule;
            this.DayCount = dayCount;
            this.Notional = notional;
            this.AsOf = asOf;
            this.StartDate = startDate;
            this.EndDate = endDate;
        }

    }
    public class FloatLeg : SwapLeg
    {
        public double Spread;

        public FloatLeg(DateTime AsOf, DateTime StartDate, DateTime EndDate,
                        CurveTenor Frequency, DayCount DayCount, DayRule DayRule, double Notional, double Spread = 0.0, StubPlacement stub = StubPlacement.NullStub)
            : base(AsOf, StartDate, EndDate, Frequency, DayCount, DayRule, Notional)
        {
            this.Spread = Spread;
        }
    }
    public class FixedLeg : SwapLeg
    {
        public double FixedRate;

        public FixedLeg(DateTime AsOf, DateTime StartDate, DateTime EndDate, double FixedRate,
                        CurveTenor Frequency, DayCount DayCount, DayRule DayRule, double Notional, StubPlacement stub = StubPlacement.NullStub) : base(AsOf, StartDate, EndDate, Frequency, DayCount, DayRule, Notional)
        {
            this.FixedRate = FixedRate;
        }
    }

    public class OisSwap : LinearRateInstrument
    {
        public OisSchedule FloatSchedule, FixedSchedule;
        public DateTime AsOf, StartDate, EndDate;
        public double Notional;
        public double FixedRate;
        public int TradeSign;

        public OisSwap(DateTime asOf, string startTenor, string endTenor, string settlementLag, DayCount dayCountFixed, 
                DayCount dayCountFloat, DayRule dayRule, double notional, double fixedRate, int tradeSign)
        {
            DateTime startDate = DateHandling.AddTenorAdjust(asOf, startTenor, dayRule);
            startDate = DateHandling.AddTenorAdjust(startDate, settlementLag, dayRule);
            DateTime endDate = DateHandling.AddTenorAdjust(startDate, endTenor, dayRule);
            this.AsOf = asOf;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.FloatSchedule = new OisSchedule(asOf, startTenor, endTenor, settlementLag, dayCountFloat, dayRule);
            this.FixedSchedule = new OisSchedule(asOf, startTenor, endTenor, settlementLag, dayCountFixed, dayRule);
            this.Notional = notional;
            this.FixedRate = fixedRate;
            this.TradeSign = tradeSign;
        }

        public void UpdateFixedRateToPar(LinearRateModel model)
        {
            FixedRate = model.DiscCurve.OisRateSimple(this, InterpMethod.Hermite);
        }

        public DateTime GetCurvePoint()
        {
            if (EndDate == null)
                throw new NullReferenceException("EndDate has not been set.");

            return EndDate;
        }

        public Instrument GetInstrumentType()
        {
            return Instrument.OisSwap;
        }
    }

    public class IrSwap : LinearRateInstrument
    {
        public FloatLeg FloatLeg { get; private set; }
        public FixedLeg FixedLeg { get; private set; }
        public int TradeSign { get; private set; }

        public IrSwap(FloatLeg floatLeg, FixedLeg fixedLeg, int tradeSign) 
        {
            FloatLeg = floatLeg;
            FixedLeg = fixedLeg;

            if (tradeSign == 1 || tradeSign == -1)
                TradeSign = tradeSign;
            else
                throw new InvalidOperationException("TradeSign has to be 1 (pay fixed) or -1 (pay float)");
        }

        public IrSwap(DateTime asOf, DateTime startDate, DateTime endDate, double fixedRate,
                        CurveTenor fixedFreq, CurveTenor floatFreq, DayCount fixedDayCount, DayCount floatDayCount,
                        DayRule fixedDayRule, DayRule floatDayRule, double notional, int tradeSign, double spread = 0.0)
        {
            FloatLeg = new FloatLeg(asOf, startDate, endDate, floatFreq, floatDayCount, floatDayRule, notional, spread);
            FixedLeg = new FixedLeg(asOf, startDate, endDate, fixedRate, fixedFreq, fixedDayCount, fixedDayRule, notional);

            if (tradeSign == 1 || tradeSign == -1)
                TradeSign = tradeSign;
            else
                throw new InvalidOperationException("TradeSign has to be 1 (pay fixed) or -1 (pay float)");
        }

        public void UpdateFixedRateToPar(LinearRateModel model)
        {
            FixedLeg.FixedRate = model.IrParSwapRate(this);
        }

        public  DateTime GetCurvePoint()
        {
            return FloatLeg.EndDate;
        }

        public Instrument GetInstrumentType()
        {
            return Instrument.IrSwap;
        }
    }

    public class TenorBasisSwap : LinearRateInstrument
    {
        public FloatLeg FloatLegNoSpread { get; private set; }
        public FloatLeg FloatLegSpread { get; private set; }
        public IrSwap SwapSpread { get; private set; }
        public IrSwap SwapNoSpread { get; private set; }
        public int TradeSign { get; private set; }             // TradeSign = 1: Receive spread
        public bool ConstructedFromFloatingLegs { get; private set; }

        private void CheckTradeSign()
        {
            if (!(TradeSign == 1 || TradeSign == -1))
                throw new InvalidOperationException("TradeSign of basis swap need to be =1 (rec spread) or =-1 (pay spread)");
        }

        public TenorBasisSwap(FloatLeg floatLegSpread, FloatLeg floatLegNoSpread, int tradeSign) 
        {
            this.FloatLegNoSpread = floatLegNoSpread.Copy();
            this.FloatLegSpread = floatLegSpread.Copy();

            // Construct default FixedLeg - quick and dirty
            FixedLeg tempFixedLeg = new FixedLeg(floatLegNoSpread.AsOf, floatLegSpread.StartDate, floatLegSpread.EndDate, 0.01, CurveTenor.Fwd1Y, DayCount.THIRTY360, DayRule.MF, floatLegSpread.Notional);
            SwapSpread = new IrSwap(floatLegSpread, tempFixedLeg, tradeSign);
            SwapNoSpread = new IrSwap(floatLegNoSpread, tempFixedLeg, -1 * tradeSign);

            TradeSign = tradeSign;
            CheckTradeSign();

            ConstructedFromFloatingLegs = true;
        }

        public TenorBasisSwap(IrSwap swapSpread, IrSwap swapNoSpread, int tradeSign)
        {
            this.FloatLegNoSpread = swapNoSpread.FloatLeg.Copy();
            this.FloatLegSpread = swapSpread.FloatLeg.Copy();
            this.SwapSpread = swapSpread.Copy();
            this.SwapNoSpread = swapNoSpread.Copy();

            TradeSign = tradeSign;
            CheckTradeSign();

            ConstructedFromFloatingLegs = false;
        }

        public void UpdateFixedRateToPar(LinearRateModel model)
        {
            FloatLegSpread.Spread = model.ParBasisSpread(this);
        }

        public DateTime GetCurvePoint()
        {
            if (FloatLegSpread.EndDate == null)
                throw new NullReferenceException("EndDate has not been set.");

            return FloatLegSpread.EndDate;
        }

        public Instrument GetInstrumentType()
        {
            return Instrument.BasisSwap;
        }
    }

    public class Fra : LinearRateInstrument
    {
        public DateTime StartDate, EndDate, AsOf;
        public DayCount DayCount;
        public DayRule DayRule;
        public CurveTenor ReferenceIndex;
        public double FixedRate;
        public double Notional;
        public double TradeSign;
        
        public Fra(DateTime asOf, DateTime startDate, DateTime endDate, CurveTenor referenceIndex, DayCount dayCount, DayRule dayRule, double fixedRate, double notional, int tradeSign)
        {
            Initialize(asOf, startDate, endDate, referenceIndex, dayCount, dayRule, fixedRate, notional, tradeSign);
        }

        private void Initialize(DateTime asOf, DateTime startDate, DateTime endDate, CurveTenor referenceIndex, DayCount dayCount, DayRule dayRule, double fixedRate, double notional, int tradeSign)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.DayCount = dayCount;
            this.DayRule = dayRule;
            this.FixedRate = fixedRate;
            this.ReferenceIndex = referenceIndex;
            this.AsOf = asOf;
            this.Notional = notional;
            if (tradeSign == 1 || tradeSign == -1)
                TradeSign = tradeSign;
            else
                throw new InvalidOperationException("TradeSign of FRA has to be 1 or -1 (1 = pay fixed)");
        }

        public Fra(DateTime asOf, string startTenor, string endTenor, CurveTenor referenceIndex, DayCount dayCount, DayRule dayRule, double fixedRate, double notional, int tradeSign)
        {
            DateTime startDate = DateHandling.AddTenorAdjust(asOf, startTenor, dayRule);
            DateTime endDate = DateHandling.AddTenorAdjust(startDate, endTenor, dayRule);
            Initialize(asOf, startDate, endDate, referenceIndex, dayCount, dayRule, fixedRate, notional, tradeSign);
        }

        public void UpdateFixedRateToPar(LinearRateModel model)
        {
            FixedRate = model.FwdCurveCollection.GetCurve(ReferenceIndex).FwdRate(AsOf, StartDate, EndDate, DayRule, DayCount, InterpMethod.Hermite);
        }

        public DateTime GetCurvePoint()
        {
            if (EndDate == null)
                throw new NullReferenceException("EndDate has not been set.");

            return EndDate;
        }

        public Instrument GetInstrumentType()
        {
            return Instrument.Fra;
        }
    }

    public class Deposit : LinearRateInstrument
    {
        public DateTime StartDate, EndDate, AsOf;
        public DayCount DayCount;
        public DayRule DayRule;
        public CurveTenor ReferenceIndex;
        public double FixedRate;
        public double Notional;
        public double TradeSign;

        public Deposit(DateTime asOf, string startTenor, string endTenor, string settlementLag, double fixedRate, DayCount dayCount, DayRule dayRule, double notional, int tradeSign)
        {
            AsOf = asOf;
            StartDate = DateHandling.AddTenorAdjust(asOf, startTenor, dayRule);
            StartDate = DateHandling.AddTenorAdjust(StartDate, settlementLag, dayRule);
            EndDate = DateHandling.AddTenor(StartDate, endTenor, dayRule);
            Notional = notional;
            TradeSign = tradeSign;
            FixedRate = fixedRate;
            DayRule = dayRule;
            DayCount = dayCount;
        }

        public void UpdateFixedRateToPar(LinearRateModel model)
        {
            FixedRate = model.ParDepositRate(this);
        }

        public DateTime GetCurvePoint()
        {
            return EndDate;
        }

        public Instrument GetInstrumentType()
        {
            return Instrument.Deposit;
        }
    }

    public class Futures : LinearRateInstrument
    {
        public double Convexity;
        public Fra FraSameSpec;

        public Futures(DateTime asOf, DateTime startDate, DateTime endDate, CurveTenor referenceIndex, DayCount dayCount, DayRule dayRule, double fixedRate, double notional, int tradeSign, double? convexity = null)
        {
            FraSameSpec = new Fra(asOf, startDate, endDate, referenceIndex, dayCount, dayRule, fixedRate, notional, tradeSign);
            if (convexity == null)
                Convexity = CalcSimpleConvexity(asOf, startDate, endDate, dayCount);
            else
                Convexity = (double) convexity;
        }

        public Futures(DateTime asOf, string startTenor, string endTenor, CurveTenor referenceIndex, DayCount dayCount, DayRule dayRule, double fixedRate, double notional, int tradeSign, double? convexity = null)
        {
            DateTime startDate = DateHandling.AddTenorAdjust(asOf, startTenor, dayRule);
            DateTime endDate = DateHandling.AddTenorAdjust(startDate, endTenor, dayRule);
            FraSameSpec = new Fra(asOf, startDate, endDate, referenceIndex, dayCount, dayRule, fixedRate, notional, tradeSign);
            if (convexity == null)
                Convexity = CalcSimpleConvexity(asOf, startDate, endDate, dayCount);
            else
                Convexity = (double)convexity;
        }

        public void UpdateFixedRateToPar(LinearRateModel model)
        {
            FraSameSpec.UpdateFixedRateToPar(model);
        }

        public Futures(Fra fra, double? convexity)
        {
            FraSameSpec = fra;
            if (convexity == null)
                Convexity = CalcSimpleConvexity(fra.AsOf, fra.StartDate, fra.EndDate, fra.DayCount);
            else
                Convexity = (double)convexity;
        }

        private double CalcSimpleConvexity(DateTime asOf, DateTime startDate, DateTime endDate, DayCount dayCount)
        {

            // i.e. Convexity Adjustment = 0.5*vol^2*T*(T+delta), T's measured in year fractions.
            // Source: Linderstrøm
            double cvgAsOfToStart = DateHandling.Cvg(asOf, startDate, dayCount);
            double cvgAsOfToEnd = DateHandling.Cvg(asOf, endDate, dayCount);
            return 0.5 * 0.0012 * 0.0012 * cvgAsOfToEnd * cvgAsOfToEnd;
        }

        public DateTime GetCurvePoint()
        {
            if (FraSameSpec.EndDate == null)
                throw new NullReferenceException("EndDate has not been set.");

            return FraSameSpec.EndDate;
        }

        public Instrument GetInstrumentType()
        {
            return Instrument.Futures;
        }

    }

}
