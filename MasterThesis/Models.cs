using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MasterThesis
{

    public abstract class RateModel
    {

        protected DiscCurve DiscCurve;
        protected FwdCurves FwdCurvesCollection;

        protected RateModel(FwdCurves MyFwdCurves, DiscCurve MyDiscCurve)
        {
            this.FwdCurvesCollection = MyFwdCurves;
            this.DiscCurve = MyDiscCurve;
        }
    }
    //public abstract class NonLinearRateModel : RateModel
    //{
    //    protected LinearRateModel LinModel;
    //}
    public abstract class LinearRateModel : RateModel
    {
        protected LinearRateModel(FwdCurves MyFwdCurves, DiscCurve MyDiscCurve) : base(MyFwdCurves, MyDiscCurve)
        {

        }

        public double Annuity(DateTime AsOf, DateTime StartDate, DateTime EndDate, CurveTenor Tenor, DayCount DayCount, DayRule DayRule, InterpMethod Method)
        {
            SwapSchedule AnnuitySchedule = new SwapSchedule(AsOf, StartDate, EndDate, DayCount, DayRule, Tenor);
            double Out = 0.0;
            DateTime PayDate;
            for (int i = 0; i<AnnuitySchedule.EndDates.Length; i++)
            {
                PayDate = AnnuitySchedule.EndDates[i]; 
                Out += AnnuitySchedule.Coverages[i] * DiscCurve.DiscFactor(AsOf, PayDate, Method);
            }
            return Out;
        }

        public double Annuity(SwapSchedule Schedule, InterpMethod Method)
        {
            return Annuity(Schedule.AsOf, Schedule.StartDate, Schedule.EndDate, Schedule.Freq, Schedule.DayCount, Schedule.DayRule, Method);
        }


        public double ValueFloatLeg(FloatLeg FloatLeg)
        {
            double FloatValue = 0.0;

            for (int i = 0; i < FloatLeg.Schedule.Periods; i++)
            {
                DateTime Begin = FloatLeg.Schedule.StartDates[i];
                DateTime End = FloatLeg.Schedule.EndDates[i];
                double Cvg = FloatLeg.Schedule.Coverages[i];
                double FwdRate = FwdCurvesCollection.GetCurve(FloatLeg.Tenor).FwdRate(FloatLeg.AsOf, Begin, End, FloatLeg.Schedule.DayRule, FloatLeg.Schedule.DayCount, InterpMethod.Linear);
                double DiscFactor = DiscCurve.DiscFactor(FloatLeg.AsOf, End, InterpMethod.Linear);
                FloatValue += FwdRate * Cvg * DiscFactor;
            }

            return FloatValue;
        }

        public double ValueFixedLeg(FixedLeg FixedLeg)
        {
            double FixedAnnuity = Annuity(FixedLeg.Schedule, InterpMethod.Linear);
            return FixedLeg.FixedRate * FixedAnnuity;
        }

        public double IrSwapPv(IrSwap Swap)
        {
            return Swap.Leg1.Notional * ValueFloatLeg((FloatLeg) Swap.Leg1) - Swap.Leg2.Notional * ValueFixedLeg((FixedLeg) Swap.Leg1);
        }

        public double MmBasisSwapPv(MmBasisSwap Swap)
        {
            return Swap.Leg1.Notional * ValueFloatLeg((FloatLeg)Swap.Leg1) - Swap.Leg2.Notional * ValueFloatLeg((FloatLeg)Swap.Leg2);
        }

        public double IrParSwapRate(IrSwap Swap)
        {
            double FloatPv = ValueFloatLeg((FloatLeg) Swap.Leg1);
            double FixedAnnuity = Annuity(Swap.Leg2.Schedule, InterpMethod.Linear);
            return FloatPv / FixedAnnuity;
        }


        // SimpleMethods
        public double SwapFloatPv(SwapSimple MySwap)
        {
            int FloatPeriods = MySwap.FloatSchedule.StartDates.Length;
            double FloatValue = 0.0;

            for (int i = 0; i < FloatPeriods; i++)
            {
                DateTime Begin = MySwap.FloatSchedule.StartDates[i];
                DateTime End = MySwap.FloatSchedule.EndDates[i];
                double Cvg = MySwap.FloatSchedule.Coverages[i];
                double FwdRate = FwdCurvesCollection.GetCurve(MySwap.FloatFreq).FwdRate(MySwap.AsOf, Begin, End, MySwap.FloatSchedule.DayRule, MySwap.FloatSchedule.DayCount, InterpMethod.Linear);
                double DiscFactor = DiscCurve.DiscFactor(MySwap.AsOf, End, InterpMethod.Linear);
                FloatValue += FwdRate * Cvg * DiscFactor;
            }

            return FloatValue;
        }
        public double SwapFixedPv(SwapSimple MySwap)
        {
            int FixedPeriods = MySwap.FixedSchedule.StartDates.Length;
            double FixedAnnuity = Annuity(MySwap.AsOf, MySwap.StartDate, MySwap.EndDate, MySwap.FixedFreq, MySwap.FixedSchedule.DayCount, MySwap.FixedSchedule.DayRule, InterpMethod.Linear);
            return MySwap.FixedRate * FixedAnnuity;
        }
        public double SwapRate(SwapSimple MySwap)
        {
            double FloatPv = SwapFloatPv(MySwap);
            double FixedAnnuity = Annuity(MySwap.AsOf, MySwap.StartDate, MySwap.EndDate, MySwap.FixedFreq, MySwap.FixedSchedule.DayCount, MySwap.FixedSchedule.DayRule, InterpMethod.Linear);
            return FloatPv / FixedAnnuity;
        }

        public double ValueInstrument(Instrument MyInstrument)
        {
            double TheValue = 0.0;
            InstrumentType Type = MyInstrument.Type;
            InstrumentComplexity Complexity = MyInstrument.Complexity;

            // To do, verify that it's a linear product

            switch (Type)
            {
                case InstrumentType.Swap:
                    {
                        SwapSimple MySwap = (SwapSimple) MyInstrument;
                        double FloatValue = SwapFloatPv(MySwap);
                        double FixedValue = SwapFixedPv(MySwap);
                        TheValue = MySwap.Notional*(FloatValue - FixedValue);
                    }
                    break;
                case InstrumentType.Fra:
                    {
                        TheValue = 1.0;
                    }
                    break;
                default:
                    TheValue = 0.0;
                    break;
            }

            return TheValue;
        }
    }



    //public class LinearRateModelSimple : LinearRateModel { }
    public class LinearRateModelAdvanced : LinearRateModel
    {
        public LinearRateModelAdvanced(FwdCurves FwdCurvesObject, DiscCurve DiscCurve) : base(FwdCurvesObject, DiscCurve)
        {
        }
    }
    //public class Sabr : NonLinearRateModel
    //{

    //}

    class Models
    {
    }
}
