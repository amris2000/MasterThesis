using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
    public abstract class Asset
    {
        //public InstrumentComplexity Complexity;
        //public InstrumentType Type;

        //protected Asset(InstrumentComplexity Complexity, InstrumentType Type)
        //{
        //    this.Complexity = Complexity;
        //    this.Type = Type;
        //}
    }

    public interface LinearRateProduct
    {
        DateTime GetCurvePoint();
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

    // MAKE OISFLOATFLEG A SWAP LEG TO MAKE THEM CONSISTENT.
    //public class OisFloatLeg
    //{
    //    public OisSchedule Schedule;
    //    public DateTime AsOf, StartDate, EndDate;
    //    public double Notional;
    //    public DayRule DayRule;
    //    public DayCount DayCount;

    //    //public OisFloatLeg(DateTime AsOf, DateTime StartDate, string Tenor, DayCount DayCount, DayRule DayRule, double notional)
    //    //{
    //    //    this.AsOf = AsOf;
    //    //    this.StartDate = StartDate;
    //    //    this.EndDate = Functions.AddTenorAdjust(StartDate, Tenor, DayRule.N);
    //    //    Schedule = new OisSchedule(AsOf, StartDate, DayCount, DayRule, Tenor);
    //    //    this.Notional = notional;
    //    //}
    //}

    //public class OisFixedLeg
    //{

    //}

    // MAKE OISSWAP A SWAP!
    // CONSIDER IF WE REALLY NEED AN "OIS SCHEDULE". Really just schedule with a stub
    public class OisSwap : LinearRateProduct
    {
        public OisSchedule FloatSchedule, FixedSchedule;
        public DateTime AsOf, StartDate, EndDate;
        public double Notional;
        public double FixedRate;

        //public OisSwap(DateTime AsOf, DateTime StartDate, string tenor, double fixedRate, DayCount dayCountFixed,
        //                    DayCount dayCountFloat, DayRule dayRuleFixed, DayRule dayRuleFloat, double notional) 
        //    //: base (InstrumentComplexity.Linear, InstrumentType.OisSwap)
        //{
        //    this.AsOf = AsOf;
        //    this.StartDate = StartDate;
        //    this.EndDate = Functions.AddTenorAdjust(StartDate, tenor, dayRuleFloat);
        //    this.FloatSchedule = new OisSchedule(AsOf, StartDate, dayCountFloat, dayRuleFloat, tenor);
        //    this.FixedSchedule = new OisSchedule(AsOf, StartDate, dayCountFixed, dayRuleFixed, tenor);
        //    this.Notional = notional;
        //    this.FixedRate = notional;
        //}

        public OisSwap(DateTime asOf, string startTenor, string endTenor, string settlementLag, DayCount dayCountFixed, DayCount dayCountFloat, DayRule dayRule, double notional, double fixedRate)
        {
            DateTime startDate = DateHandling.AddTenorAdjust(asOf, settlementLag, dayRule);
            DateTime endDate = DateHandling.AddTenorAdjust(startDate, endTenor, dayRule);
            this.AsOf = asOf;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.FloatSchedule = new OisSchedule(asOf, startTenor, endTenor, settlementLag, dayCountFloat, dayRule);
            this.FixedSchedule = new OisSchedule(asOf, startTenor, endTenor, settlementLag, dayCountFixed, dayRule);
            this.Notional = notional;
            this.FixedRate = fixedRate;
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

    public abstract class Swap : LinearRateProduct
    {
        public SwapLeg Leg1;
        public SwapLeg Leg2;

        public Swap(FloatLeg SwapLeg1, FloatLeg SwapLeg2) 
            //: base(InstrumentComplexity.Linear, InstrumentType.MmBasisSwap)
        {
            Leg1 = SwapLeg1;
            Leg2 = SwapLeg2;
        }

        public Swap(FloatLeg SwapLeg1, FixedLeg SwapLeg2) 
            //: base(InstrumentComplexity.Linear, InstrumentType.Swap)
        {
            Leg1 = SwapLeg1;
            Leg2 = SwapLeg2;
        }

        public Swap() 
            // : base(InstrumentComplexity.Linear, InstrumentType.Swap)
        {

        }

        public abstract DateTime GetCurvePoint();
        public abstract Instrument GetInstrumentType();
    }

    public class IrSwap : Swap
    {
        public IrSwap(FloatLeg FloatLeg, FixedLeg FixedLeg) 
            : base(FloatLeg, FixedLeg)
        {
        }

        public IrSwap(DateTime asOf, DateTime startDate, DateTime endDate, double fixedRate,
                        CurveTenor fixedFreq, CurveTenor floatFreq, DayCount fixedDayCount, DayCount floatDayCount,
                        DayRule fixedDayRule, DayRule floatDayRule, double notional, double spread = 0.0)
        {
            Leg1 = new FloatLeg(asOf, startDate, endDate, floatFreq, floatDayCount, floatDayRule, notional, spread);
            Leg2 = new FixedLeg(asOf, startDate, endDate, fixedRate, fixedFreq, fixedDayCount, fixedDayRule, notional);
        }

        public override DateTime GetCurvePoint()
        {
            return Leg1.EndDate;
        }

        public override Instrument GetInstrumentType()
        {
            return Instrument.IrSwap;
        }
    }

    public class BasisSwap : Swap
    {
        public FloatLeg FloatLegNoSpread;
        public FloatLeg FloatLegSpread;

        public BasisSwap(FloatLeg floatLegSpread, FloatLeg floatLegNoSpread) 
            : base (floatLegNoSpread, floatLegSpread)
        {
            this.FloatLegNoSpread = floatLegNoSpread;
            this.FloatLegSpread = floatLegSpread;
        }

        public BasisSwap(IrSwap swapSpread, IrSwap swapNoSpread)
        {
            this.FloatLegNoSpread = (FloatLeg) swapNoSpread.Leg1;
            this.FloatLegSpread = (FloatLeg) swapSpread.Leg1;
        }

        public override DateTime GetCurvePoint()
        {
            if (FloatLegSpread.EndDate == null)
                throw new NullReferenceException("EndDate has not been set.");

            return FloatLegSpread.EndDate;
        }

        public override Instrument GetInstrumentType()
        {
            return Instrument.BasisSwap;
        }
    }


    public class SwapSimple : Asset
    {
        public SwapSchedule FloatSchedule, FixedSchedule;
        public double Notional, FixedRate;
        public DateTime AsOf, StartDate, EndDate;
        public CurveTenor FixedFreq, FloatFreq;

        public SwapSimple(DateTime AsOf, DateTime StartDate, DateTime EndDate, double FixedRate,
                        CurveTenor FixedFreq, CurveTenor FloatFreq, DayCount FixedDayCount, DayCount FloatDayCount,
                        DayRule FixedDayRule, DayRule FloatDayRule, double Notional) 
            //: base(InstrumentComplexity.Linear, InstrumentType.Swap)
        {
            FloatSchedule = new MasterThesis.SwapSchedule(AsOf, StartDate, EndDate, FloatDayCount, FloatDayRule, FloatFreq);
            FixedSchedule = new SwapSchedule(AsOf, StartDate, EndDate, FixedDayCount, FixedDayRule, FixedFreq);
            this.Notional = Notional;
            this.AsOf = AsOf;
            this.FixedRate = FixedRate;
            this.StartDate = StartDate;
            this.EndDate = EndDate;
            this.FixedFreq = FixedFreq;
            this.FloatFreq = FloatFreq;
        }
    }
    public class Fra : LinearRateProduct
    {
        public DateTime StartDate, EndDate, AsOf;
        public DayCount FloatDayCount;
        public DayRule FloatDayRule;
        public CurveTenor ReferenceIndex;
        public double FixedRate;
        public double Notional;
        
        public Fra(DateTime asOf, DateTime startDate, DateTime endDate, CurveTenor referenceIndex, DayCount dayCount, DayRule dayRule, double fixedRate, double notional = 1.0)
        {
            Initialize(asOf, startDate, endDate, referenceIndex, dayCount, dayRule, fixedRate, notional);
        }

        private void Initialize(DateTime asOf, DateTime startDate, DateTime endDate, CurveTenor referenceIndex, DayCount dayCount, DayRule dayRule, double fixedRate, double notional)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.FloatDayCount = dayCount;
            this.FloatDayRule = dayRule;
            this.FixedRate = fixedRate;
            this.ReferenceIndex = referenceIndex;
            this.AsOf = asOf;
            this.Notional = notional;
        }

        public Fra(DateTime asOf, string startTenor, string endTenor, CurveTenor referenceIndex, DayCount dayCount, DayRule dayRule, double fixedRate, double notional = 1.0)
        {
            DateTime startDate = DateHandling.AddTenorAdjust(asOf, startTenor, dayRule);
            DateTime endDate = DateHandling.AddTenorAdjust(startDate, endTenor, dayRule);
            Initialize(asOf, startDate, endDate, referenceIndex, dayCount, dayRule, fixedRate, notional);
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
    public class Future : LinearRateProduct
    {
        public double Convexity;
        public Fra FraSameSpec;

        public Future(DateTime asOf, DateTime startDate, DateTime endDate, CurveTenor referenceIndex, DayCount dayCount, DayRule dayRule, double fixedRate, double? convexity = null)
        {
            FraSameSpec = new Fra(asOf, startDate, endDate, referenceIndex, dayCount, dayRule, fixedRate);
            if (convexity == null)
                Convexity = CalcSimpleConvexity(asOf, startDate, endDate, dayCount);
            else
                Convexity = (double) convexity;
        }

        public Future(DateTime asOf, string startTenor, string endTenor, CurveTenor referenceIndex, DayCount dayCount, DayRule dayRule, double fixedRate, double? convexity = null)
        {
            DateTime startDate = DateHandling.AddTenorAdjust(asOf, startTenor, dayRule);
            DateTime endDate = DateHandling.AddTenorAdjust(startDate, endTenor, dayRule);
            FraSameSpec = new Fra(asOf, startDate, endDate, referenceIndex, dayCount, dayRule, fixedRate);
            if (convexity == null)
                Convexity = CalcSimpleConvexity(asOf, startDate, endDate, dayCount);
            else
                Convexity = (double)convexity;
        }

        public Future(Fra fra, double? convexity)
        {
            FraSameSpec = fra;
            if (convexity == null)
                Convexity = CalcSimpleConvexity(fra.AsOf, fra.StartDate, fra.EndDate, fra.FloatDayCount);
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
            return Instrument.Future;
        }

    }

}
