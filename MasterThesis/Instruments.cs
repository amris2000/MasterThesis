using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
    public abstract class Asset
    {
        public InstrumentComplexity Complexity;
        public InstrumentType Type;

        protected Asset(InstrumentComplexity Complexity, InstrumentType Type)
        {
            this.Complexity = Complexity;
            this.Type = Type;
        }
    }

    public abstract class SwapLeg
    {
        public SwapSchedule Schedule;
        public CurveTenor Tenor;
        public DateTime AsOf, StartDate, EndDate;
        public double Notional;
        public DayRule DayRule;
        public DayCount DayCount;

        protected SwapLeg(DateTime AsOf, DateTime StartDate, DateTime EndDate, CurveTenor Tenor, DayCount DayCount, DayRule DayRule, double Notional)
        {
            Schedule = new MasterThesis.SwapSchedule(AsOf, StartDate, EndDate, DayCount, DayRule, Tenor);
            this.Tenor = Tenor;
            this.DayRule = DayRule;
            this.DayCount = DayCount;
            this.Notional = Notional;
            this.AsOf = AsOf;
            this.StartDate = this.EndDate;
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

    public class OisFloatLeg
    {
        public OisSchedule Schedule;
        public DateTime AsOf, StartDate, EndDate;
        public double Notional;
        public DayRule DayRule;
        public DayCount DayCount;

        public OisFloatLeg(DateTime AsOf, DateTime StartDate, string Tenor, DayCount DayCount, DayRule DayRule, double notional)
        {
            this.AsOf = AsOf;
            this.StartDate = StartDate;
            this.EndDate = Calender.AddTenor(StartDate, Tenor, DayRule.N);
            Schedule = new OisSchedule(AsOf, StartDate, DayCount, DayRule, Tenor);
            this.Notional = notional;
        }
    }

    public class OisFixedLeg
    {

    }


    public class OisSwap : Asset
    {
        public OisSchedule FloatSchedule, FixedSchedule;
        public DateTime AsOf, StartDate, EndDate;
        public double Notional;
        public double FixedRate;

        public OisSwap(DateTime AsOf, DateTime StartDate, string tenor, double fixedRate, DayCount dayCountFixed,
                            DayCount dayCountFloat, DayRule dayRuleFixed, DayRule dayRuleFloat, double notional) : base (InstrumentComplexity.Linear, InstrumentType.OisSwap)
        {
            this.AsOf = AsOf;
            this.StartDate = StartDate;
            this.EndDate = Calender.AddTenor(StartDate, tenor, dayRuleFloat);
            this.FloatSchedule = new OisSchedule(AsOf, StartDate, dayCountFloat, dayRuleFloat, tenor);
            this.FixedSchedule = new OisSchedule(AsOf, StartDate, dayCountFixed, dayRuleFixed, tenor);
            this.Notional = notional;
            this.FixedRate = notional;
        }
    }

    public abstract class Swap : Asset
    {
        public SwapLeg Leg1;
        public SwapLeg Leg2;

        public Swap(FloatLeg SwapLeg1, FloatLeg SwapLeg2) : base(InstrumentComplexity.Linear, InstrumentType.MmBasisSwap)
        {
            Leg1 = SwapLeg1;
            Leg2 = SwapLeg2;
        }

        public Swap(FloatLeg SwapLeg1, FixedLeg SwapLeg2) : base(InstrumentComplexity.Linear, InstrumentType.Swap)
        {
            Leg1 = SwapLeg1;
            Leg2 = SwapLeg2;
        }

        public Swap() : base(InstrumentComplexity.Linear, InstrumentType.Swap)
        {

        }
    }

    public class IrSwap : Swap
    {
        public IrSwap(FloatLeg FloatLeg, FixedLeg FixedLeg) : base(FloatLeg, FixedLeg)
        {
        }

        public IrSwap(DateTime AsOf, DateTime StartDate, DateTime EndDate, double FixedRate,
                        CurveTenor FixedFreq, CurveTenor FloatFreq, DayCount FixedDayCount, DayCount FloatDayCount,
                        DayRule FixedDayRule, DayRule FloatDayRule, double Notional, double Spread = 0.0)
        {
            Leg1 = new FloatLeg(AsOf, StartDate, EndDate, FloatFreq, FloatDayCount, FloatDayRule, Notional, Spread);
            Leg2 = new FixedLeg(AsOf, StartDate, EndDate, FixedRate, FixedFreq, FixedDayCount, FixedDayRule, Notional);
        }
    }

    public class MmBasisSwap : Swap
    {
        public MmBasisSwap(FloatLeg FloatLeg1, FloatLeg FloatLeg2) : base(FloatLeg1, FloatLeg2)
        {

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
                        DayRule FixedDayRule, DayRule FloatDayRule, double Notional) : base(InstrumentComplexity.Linear, InstrumentType.Swap)
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
    //public class Fra : Instrument
    //{

    //}
    //public class Future : Instrument
    //{

    //}
    //public class BasisSwap : Instrument
    //{

    //}
    //public class FxFwd : Instrument
    //{

    //}
    //public class Deposit
    //{

    //}

}
