using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MasterThesis
{
    public class LinearRateModel
    {
        public Curve DiscCurve;
        public FwdCurves FwdCurveCollection;
        public InterpMethod Interpolation = InterpMethod.Linear;

        public LinearRateModel(Curve discCurve, FwdCurves fwdCurveCollection, InterpMethod interpolation = InterpMethod.Linear)
        {
            Interpolation = interpolation;
            DiscCurve = discCurve;
            FwdCurveCollection = fwdCurveCollection;
        }

        public double Annuity(DateTime AsOf, DateTime StartDate, DateTime EndDate, CurveTenor Tenor, DayCount DayCount, DayRule DayRule, InterpMethod Method)
        {
            SwapSchedule AnnuitySchedule = new SwapSchedule(AsOf, StartDate, EndDate, DayCount, DayRule, Tenor);
            double Out = 0.0;
            DateTime PayDate;
            for (int i = 0; i<AnnuitySchedule.AdjEndDates.Count; i++)
            {
                PayDate = AnnuitySchedule.AdjEndDates[i]; 
                Out += AnnuitySchedule.Coverages[i] * DiscCurve.DiscFactor(AsOf, PayDate, Method);
            }
            return Out;
        }
        public double Annuity(SwapSchedule Schedule, InterpMethod Method)
        {
            return Annuity(Schedule.AsOf, Schedule.StartDate, Schedule.EndDate, Schedule.Frequency, Schedule.DayCount, Schedule.DayRule, Method);
        }

        public double ParFraRate(Fra fra)
        {
            Curve fwdCurve = this.FwdCurveCollection.GetCurve(fra.ReferenceIndex);
            double rate = fwdCurve.FwdRate(fra.AsOf, fra.StartDate, fra.EndDate, fra.FloatDayRule, fra.FloatDayCount, Interpolation);
            return rate;
        }

        public double ParFutureRate(Future future)
        {
            return ParFraRate(future.FraSameSpec) + future.Convexity;
        }

        #region SWAPS
        public double ValueFloatLeg(FloatLeg floatLeg)
        {
            double floatValue = 0.0;
            double spread = floatLeg.Spread;

            for (int i = 0; i < floatLeg.Schedule.AdjStartDates.Count; i++)
            {
                DateTime startDate = floatLeg.Schedule.AdjStartDates[i];
                DateTime endDate = floatLeg.Schedule.AdjEndDates[i];
                double cvg = floatLeg.Schedule.Coverages[i];
                double fwdRate = FwdCurveCollection.GetCurve(floatLeg.Tenor).FwdRate(floatLeg.AsOf, startDate, endDate, floatLeg.Schedule.DayRule, floatLeg.Schedule.DayCount, Interpolation);
                double discFactor = DiscCurve.DiscFactor(floatLeg.AsOf, endDate, Interpolation);
                floatValue += (fwdRate + spread) * cvg * discFactor;
            }

            return floatValue * floatLeg.Notional;
        }

        public double ValueFloatLegNoSpread(FloatLeg floatLeg)
        {
            double floatValue = 0.0;
            double spread = 0.0;

            for (int i = 0; i < floatLeg.Schedule.AdjStartDates.Count; i++)
            {
                DateTime startDate = floatLeg.Schedule.AdjStartDates[i];
                DateTime endDate = floatLeg.Schedule.AdjEndDates[i];
                double cvg = floatLeg.Schedule.Coverages[i];
                double fwdRate = FwdCurveCollection.GetCurve(floatLeg.Tenor).FwdRate(floatLeg.AsOf, startDate, endDate, floatLeg.Schedule.DayRule, floatLeg.Schedule.DayCount, Interpolation);
                double discFactor = DiscCurve.DiscFactor(floatLeg.AsOf, endDate, Interpolation);
                floatValue += (fwdRate + spread) * cvg * discFactor;
            }
            return floatValue * floatLeg.Notional;
        }

        public double ValueFixedLeg(FixedLeg FixedLeg)
        {
            double FixedAnnuity = Annuity(FixedLeg.Schedule, Interpolation);
            return FixedLeg.FixedRate * FixedAnnuity * FixedLeg.Notional;
        }
        public double IrSwapPv(IrSwap Swap)
        {
            return ValueFloatLeg((FloatLeg) Swap.Leg1) - ValueFixedLeg((FixedLeg) Swap.Leg2);
        }
        public double IrParSwapRate(IrSwap Swap)
        {
            double FloatPv = ValueFloatLeg((FloatLeg)Swap.Leg1)/Swap.Leg1.Notional;
            double FixedAnnuity = Annuity(Swap.Leg2.Schedule, Interpolation);
            return FloatPv / FixedAnnuity;
        }

        #endregion

        public double BasisSwapPv(BasisSwap swap)
        {
            return ValueFloatLeg((FloatLeg)swap.Leg1) - ValueFloatLeg((FloatLeg)swap.Leg2);
        }

        public double ParBasisSpread(BasisSwap swap)
        {
            double PvNoSpread = ValueFloatLegNoSpread(swap.FloatLegNoSpread)/swap.FloatLegNoSpread.Notional;
            double PvSpread = ValueFloatLegNoSpread(swap.FloatLegSpread)/swap.FloatLegNoSpread.Notional;
            double AnnuityNoSpread = Annuity(swap.FloatLegSpread.Schedule, Interpolation);
            return (PvNoSpread - PvSpread) / AnnuityNoSpread;
        }

        // Not used
        public double SwapFixedPv(SwapSimple MySwap)
        {
            int FixedPeriods = MySwap.FixedSchedule.AdjStartDates.Count;
            double FixedAnnuity = Annuity(MySwap.AsOf, MySwap.StartDate, MySwap.EndDate, MySwap.FixedFreq, MySwap.FixedSchedule.DayCount, MySwap.FixedSchedule.DayRule, Interpolation);
            return MySwap.FixedRate * FixedAnnuity;
        }

        public double SwapRate(SwapSimple MySwap)
        {
            double FloatPv = SwapFloatPv(MySwap);
            double FixedAnnuity = Annuity(MySwap.AsOf, MySwap.StartDate, MySwap.EndDate, MySwap.FixedFreq, MySwap.FixedSchedule.DayCount, MySwap.FixedSchedule.DayRule, Interpolation);
            return FloatPv / FixedAnnuity;
        }

        public double SwapFloatPv(SwapSimple MySwap)
        {
            int FloatPeriods = MySwap.FloatSchedule.AdjStartDates.Count;
            double FloatValue = 0.0;

            for (int i = 0; i < FloatPeriods; i++)
            {
                DateTime startDate = MySwap.FloatSchedule.AdjStartDates[i];
                DateTime endDate = MySwap.FloatSchedule.AdjEndDates[i];
                double cvg = MySwap.FloatSchedule.Coverages[i];
                double fwdRate = FwdCurveCollection.GetCurve(MySwap.FloatFreq).FwdRate(MySwap.AsOf, startDate, endDate, MySwap.FloatSchedule.DayRule, MySwap.FloatSchedule.DayCount, Interpolation);
                double discFactor = DiscCurve.DiscFactor(MySwap.AsOf, endDate, Interpolation);
                FloatValue += fwdRate * cvg * discFactor;
            }

            return FloatValue;
        }

        public double OisRateSimple(OisSwap swap)
        {
            return DiscCurve.OisRateSimple(swap, Interpolation);
        }

        public double OisRate(OisSwap swap)
        {
            return DiscCurve.OisRate(swap, Interpolation);
        }

    }

    // Not used
    public class LinearRateModelSimple : LinearRateModel
    {
        public LinearRateModelSimple(Curve discCurve) : base (discCurve, new FwdCurves(discCurve)) { }
    }


}
