﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
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

    // CONSIDER IF WE REALLY NEED AN "OIS SCHEDULE". Really just schedule with a stub
    public class OisSwap : LinearRateProduct
    {
        public OisSchedule FloatSchedule, FixedSchedule;
        public DateTime AsOf, StartDate, EndDate;
        public double Notional;
        public double FixedRate;
        public double TradeSign;

        public OisSwap(DateTime asOf, string startTenor, string endTenor, string settlementLag, DayCount dayCountFixed, DayCount dayCountFloat, DayRule dayRule, double notional, double fixedRate, Direction direction = Direction.Pay)
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
            this.TradeSign = EnumHelpers.TradeSignToDouble(direction);
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

    public class IrSwap : LinearRateProduct
    {
        public FloatLeg FloatLeg { get; private set; }
        public FixedLeg FixedLeg { get; private set; }

        public IrSwap(FloatLeg floatLeg, FixedLeg fixedLeg) 
        {
            FloatLeg = floatLeg;
            FixedLeg = fixedLeg;
        }

        public IrSwap(DateTime asOf, DateTime startDate, DateTime endDate, double fixedRate,
                        CurveTenor fixedFreq, CurveTenor floatFreq, DayCount fixedDayCount, DayCount floatDayCount,
                        DayRule fixedDayRule, DayRule floatDayRule, double notional, double spread = 0.0)
        {
            FloatLeg = new FloatLeg(asOf, startDate, endDate, floatFreq, floatDayCount, floatDayRule, notional, spread);
            FixedLeg = new FixedLeg(asOf, startDate, endDate, fixedRate, fixedFreq, fixedDayCount, fixedDayRule, notional);
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

    public class BasisSwap : LinearRateProduct
    {
        public FloatLeg FloatLegNoSpread;
        public FloatLeg FloatLegSpread;

        public BasisSwap(FloatLeg floatLegSpread, FloatLeg floatLegNoSpread) 
        {
            this.FloatLegNoSpread = floatLegNoSpread;
            this.FloatLegSpread = floatLegSpread;
        }

        public BasisSwap(IrSwap swapSpread, IrSwap swapNoSpread)
        {
            this.FloatLegNoSpread = swapNoSpread.FloatLeg;
            this.FloatLegSpread = swapSpread.FloatLeg;
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

    public class Fra : LinearRateProduct
    {
        public DateTime StartDate, EndDate, AsOf;
        public DayCount DayCount;
        public DayRule DayRule;
        public CurveTenor ReferenceIndex;
        public double FixedRate;
        public double Notional;
        public double TradeSign;
        
        public Fra(DateTime asOf, DateTime startDate, DateTime endDate, CurveTenor referenceIndex, DayCount dayCount, DayRule dayRule, double fixedRate, double notional, Direction direction = Direction.Pay)
        {
            Initialize(asOf, startDate, endDate, referenceIndex, dayCount, dayRule, fixedRate, notional, direction);
        }

        private void Initialize(DateTime asOf, DateTime startDate, DateTime endDate, CurveTenor referenceIndex, DayCount dayCount, DayRule dayRule, double fixedRate, double notional, Direction direction)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.DayCount = dayCount;
            this.DayRule = dayRule;
            this.FixedRate = fixedRate;
            this.ReferenceIndex = referenceIndex;
            this.AsOf = asOf;
            this.Notional = notional;
            TradeSign = EnumHelpers.TradeSignToDouble(direction);
        }

        public Fra(DateTime asOf, string startTenor, string endTenor, CurveTenor referenceIndex, DayCount dayCount, DayRule dayRule, double fixedRate, double notional, Direction direction = Direction.Pay)
        {
            DateTime startDate = DateHandling.AddTenorAdjust(asOf, startTenor, dayRule);
            DateTime endDate = DateHandling.AddTenorAdjust(startDate, endTenor, dayRule);
            Initialize(asOf, startDate, endDate, referenceIndex, dayCount, dayRule, fixedRate, notional, direction);
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

    public class Futures : LinearRateProduct
    {
        public double Convexity;
        public Fra FraSameSpec;

        public Futures(DateTime asOf, DateTime startDate, DateTime endDate, CurveTenor referenceIndex, DayCount dayCount, DayRule dayRule, double fixedRate, double notional, double? convexity = null)
        {
            FraSameSpec = new Fra(asOf, startDate, endDate, referenceIndex, dayCount, dayRule, fixedRate, notional);
            if (convexity == null)
                Convexity = CalcSimpleConvexity(asOf, startDate, endDate, dayCount);
            else
                Convexity = (double) convexity;
        }

        public Futures(DateTime asOf, string startTenor, string endTenor, CurveTenor referenceIndex, DayCount dayCount, DayRule dayRule, double fixedRate, double notional, double? convexity = null)
        {
            DateTime startDate = DateHandling.AddTenorAdjust(asOf, startTenor, dayRule);
            DateTime endDate = DateHandling.AddTenorAdjust(startDate, endTenor, dayRule);
            FraSameSpec = new Fra(asOf, startDate, endDate, referenceIndex, dayCount, dayRule, notional, fixedRate);
            if (convexity == null)
                Convexity = CalcSimpleConvexity(asOf, startDate, endDate, dayCount);
            else
                Convexity = (double)convexity;
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
