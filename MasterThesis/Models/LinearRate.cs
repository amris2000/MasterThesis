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

        public LinearRateModel(Curve MyDiscCurve, FwdCurves fwdCurveCollection, InterpMethod interpolation = InterpMethod.Linear)
        {
            Interpolation = interpolation;
            DiscCurve = MyDiscCurve;
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
            return Annuity(Schedule.AsOf, Schedule.StartDate, Schedule.EndDate, Schedule.Freq, Schedule.DayCount, Schedule.DayRule, Method);
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


        #region OIS SWAPS
        public double OisAnnuity(OisSchedule Schedule, InterpMethod method)
        {
            double Out = 0.0;
            DateTime PayDate;
            for (int i = 0; i<Schedule.AdjEndDates.Count; i++)
            {

                PayDate = Schedule.AdjEndDates[i];
                Out += Schedule.Coverages[i] * DiscCurve.DiscFactor(Schedule.AsOf, Schedule.EndDate, method);
            }
            return Out;
        }
        public double OisCompoundedRate(DateTime asOf, DateTime startDate, DateTime endDate, DayRule dayRule, DayCount dayCount, InterpMethod method)
        {
            double CompoundedRate = 1;
            double CompoundedRate2 = 1;
            DateTime RollDate = startDate;
            while (RollDate.Date<endDate.Date)
            {
                DateTime NextBusinessDay = Calender.AddTenor(RollDate, "1B", DayRule.F);
                //double Rate = DiscCurve.ZeroRate(asOf, startDate, RollDate, dayRule, dayCount, method);
                double Rate = DiscCurve.ZeroRate(NextBusinessDay, InterpMethod.Linear);
                double fwdOisRate = DiscCurve.FwdRate(asOf, RollDate, NextBusinessDay, DayRule.F, dayCount, method);

                double disc1 = DiscCurve.DiscFactor(asOf, RollDate, method);
                double disc2 = DiscCurve.DiscFactor(asOf, NextBusinessDay, method);

                double Days = NextBusinessDay.Subtract(RollDate).TotalDays;
                double shortCvg = Calender.Cvg(RollDate, NextBusinessDay, dayCount);
                RollDate = NextBusinessDay;
                CompoundedRate *= (1 + fwdOisRate * shortCvg);
                CompoundedRate2 *= disc1 / disc2;
            }
            double coverage = Calender.Cvg(startDate, endDate, dayCount);
            return (CompoundedRate2 - 1) / coverage;
        }
        public double OisRate(OisSwap swap)
        {
            double FloatContribution = 0.0;
            double Annuity = OisAnnuity(swap.FixedSchedule, InterpMethod.Linear);

            DateTime AsOf = swap.FloatSchedule.AsOf;

            for (int i = 0; i < swap.FloatSchedule.AdjEndDates.Count; i++)
            {
                DateTime Start = swap.FloatSchedule.AdjStartDates[i];
                DateTime End = swap.FloatSchedule.AdjEndDates[i];
                double CompoundedRate = OisCompoundedRate(AsOf, Start, End, swap.FloatSchedule.DayRule, swap.FloatSchedule.DayCount, Interpolation);
                double DiscountFactor = DiscCurve.DiscFactor(AsOf, End, Interpolation);
                double coverage = Calender.Cvg(Start, End, swap.FloatSchedule.DayCount);
                FloatContribution += DiscountFactor * CompoundedRate * coverage;
            }
            return FloatContribution / Annuity;
        }

        public double OisRateSimple(OisSwap swap)
        {
            double Annuity = OisAnnuity(swap.FixedSchedule, Interpolation);
            DateTime AsOf = swap.AsOf;
            DateTime Start = swap.StartDate;
            DateTime End = swap.EndDate;
            double disc1 = DiscCurve.DiscFactor(AsOf, Start, Interpolation);
            double disc2 = DiscCurve.DiscFactor(AsOf, End, Interpolation);
            return (disc1 - disc2) / Annuity;
        }

        public double OisRateSimple2(OisSwap swap)
        {
            double Annuity = OisAnnuity(swap.FixedSchedule, Interpolation);
            DateTime asOf = swap.AsOf;
            double FloatContribution = 0.0;
            
            for (int i = 0; i < swap.FloatSchedule.AdjEndDates.Count; i++)
            {
                DateTime Start = swap.FloatSchedule.AdjStartDates[i];
                DateTime End = swap.FloatSchedule.AdjEndDates[i];
                double cvg = Calender.Cvg(Start, End, swap.FloatSchedule.DayCount);
                double disc1 = DiscCurve.DiscFactor(asOf, Start, Interpolation);
                double disc2 = DiscCurve.DiscFactor(asOf, End, Interpolation);
                FloatContribution += (disc1 - disc2) / cvg;
            }

            return FloatContribution / Annuity;
        }
        #endregion

        #region SWAPS
        public double ValueFloatLeg(FloatLeg floatLeg)
        {
            double FloatValue = 0.0;
            double spread = floatLeg.Spread;

            for (int i = 0; i < floatLeg.Schedule.AdjStartDates.Count; i++)
            {
                DateTime Begin = floatLeg.Schedule.AdjStartDates[i];
                DateTime End = floatLeg.Schedule.AdjEndDates[i];
                double Cvg = floatLeg.Schedule.Coverages[i];
                double FwdRate = FwdCurveCollection.GetCurve(floatLeg.Tenor).FwdRate(floatLeg.AsOf, Begin, End, floatLeg.Schedule.DayRule, floatLeg.Schedule.DayCount, Interpolation);
                double DiscFactor = DiscCurve.DiscFactor(floatLeg.AsOf, End, Interpolation);
                FloatValue += (FwdRate + spread) * Cvg * DiscFactor;
            }

            return FloatValue * floatLeg.Notional;
        }

        public double ValueFloatLegNoSpread(FloatLeg floatLeg)
        {
            double FloatValue = 0.0;
            double spread = 0.0;

            for (int i = 0; i < floatLeg.Schedule.AdjStartDates.Count; i++)
            {
                DateTime Begin = floatLeg.Schedule.AdjStartDates[i];
                DateTime End = floatLeg.Schedule.AdjEndDates[i];
                double Cvg = floatLeg.Schedule.Coverages[i];
                double FwdRate = FwdCurveCollection.GetCurve(floatLeg.Tenor).FwdRate(floatLeg.AsOf, Begin, End, floatLeg.Schedule.DayRule, floatLeg.Schedule.DayCount, Interpolation);
                double DiscFactor = DiscCurve.DiscFactor(floatLeg.AsOf, End, Interpolation);
                FloatValue += (FwdRate + spread) * Cvg * DiscFactor;
            }
            return FloatValue * floatLeg.Notional;
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
                DateTime Begin = MySwap.FloatSchedule.AdjStartDates[i];
                DateTime End = MySwap.FloatSchedule.AdjEndDates[i];
                double Cvg = MySwap.FloatSchedule.Coverages[i];
                double FwdRate = FwdCurveCollection.GetCurve(MySwap.FloatFreq).FwdRate(MySwap.AsOf, Begin, End, MySwap.FloatSchedule.DayRule, MySwap.FloatSchedule.DayCount, Interpolation);
                double DiscFactor = DiscCurve.DiscFactor(MySwap.AsOf, End, Interpolation);
                FloatValue += FwdRate * Cvg * DiscFactor;
            }

            return FloatValue;
        }



        //public double Value(Asset MyInstrument)
        //{
        //    double TheValue = 0.0;
        //    InstrumentType Type = MyInstrument.Type;
        //    //InstrumentComplexity Complexity = MyInstrument.Complexity;

        //    // To do, verify that it's a linear product

        //    switch (Type)
        //    {
        //        case InstrumentType.Swap:
        //            {
        //                SwapSimple MySwap = (SwapSimple) MyInstrument;
        //                double FloatValue = SwapFloatPv(MySwap);
        //                double FixedValue = SwapFixedPv(MySwap);
        //                TheValue = MySwap.Notional*(FloatValue - FixedValue);
        //            }
        //            break;
        //        case InstrumentType.Fra:
        //            {
        //                TheValue = 1.0;
        //            }
        //            break;
        //        default:
        //            TheValue = 0.0;
        //            break;
        //    }

        //    return TheValue;
        //}
    }

    public class LinearRateModelSimple : LinearRateModel
    {
        public LinearRateModelSimple(Curve discCurve) : base (discCurve, new FwdCurves(discCurve)) { }
    }


}
