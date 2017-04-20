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
        public FwdCurveContainer FwdCurveCollection;
        public InterpMethod Interpolation;
        private Random _random;

        public LinearRateModel(Curve discCurve, FwdCurveContainer fwdCurveCollection, InterpMethod interpolation = InterpMethod.Linear)
        {
            Interpolation = interpolation;
            DiscCurve = discCurve;
            FwdCurveCollection = fwdCurveCollection;
            _random = new Random();
        }

        // --------- RELATED TO BUMP-AND-RUN RISK -------------
        // ... Remember to make sure that deep copy actually works.

        private LinearRateModel BumpFwdCurveAndReturn(CurveTenor fwdCurve, int curvePoint, double bump = 0.0001)
        {
            Curve newCurve = FwdCurveCollection.GetCurve(fwdCurve).Copy();
            newCurve.BumpCurvePoint(curvePoint, bump);
            return ReturnModelWithReplacedFwdCurve(fwdCurve, newCurve);
        }

        private LinearRateModel ReturnModelWithReplacedFwdCurve(CurveTenor tenor, Curve newCurve)
        {
            FwdCurveContainer newCollection = FwdCurveCollection.Copy();
            newCollection.AddCurve(newCurve, tenor);
            return new LinearRateModel(DiscCurve.Copy(), newCollection, Interpolation);
        }
        
        private LinearRateModel BumpDiscCurveAndReturn(int curvePoint, double bump = 0.0001)
        {
            Curve newCurve = DiscCurve.Copy();
            newCurve.BumpCurvePoint(curvePoint, bump);
            return ReturnModelWithReplacedDiscCurve(newCurve);
        }

        private LinearRateModel ReturnModelWithReplacedDiscCurve(Curve newDiscCurve)
        {
            return new LinearRateModel(newDiscCurve, FwdCurveCollection.Copy(), Interpolation);
        } 

        /// <summary>
        /// Risk value of a basis point. 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="fwdCurve"></param>
        /// <param name="curvePoint"></param>
        /// <param name="bump"></param>
        /// <returns></returns>
        public double BumpAndRunFwdRisk(LinearRateProduct product, CurveTenor fwdCurve, int curvePoint, double bump = 0.0001)
        {
            double valueNoBump = ValueLinearRateProduct(product);
            LinearRateModel newModel = BumpFwdCurveAndReturn(fwdCurve, curvePoint, bump);
            double valueBump = newModel.ValueLinearRateProduct(product);
            return (valueBump - valueNoBump)/bump*0.0001; 
        }

        /// <summary>
        /// Risk value of a  basis point.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="curvePoint"></param>
        /// <param name="bump"></param>
        /// <returns></returns>
        public double BumpAndRunDisc(LinearRateProduct product, int curvePoint, double bump = 0.0001)
        {
            double valueNoBump = ValueLinearRateProduct(product);
            LinearRateModel newModel = BumpDiscCurveAndReturn(curvePoint, bump);
            double valueBump = newModel.ValueLinearRateProduct(product);
            return (valueBump - valueNoBump)/bump*0.0001; 
        }

        public ZcbRiskOutput CalculateZcbRiskBumpAndRun(LinearRateProduct product, CurveTenor tenor, DateTime asOf)
        {
            ZcbRiskOutput output = new MasterThesis.ZcbRiskOutput(asOf);

            if (tenor == CurveTenor.DiscOis || tenor == CurveTenor.DiscLibor)
            {
                for (int i = 0; i < DiscCurve.Values.Count; i++)
                {
                    DateTime curvePoint = DiscCurve.Dates[i];
                    double riskValue = BumpAndRunDisc(product, i);
                    output.AddRiskCalculation(CurveTenor.DiscOis, curvePoint, riskValue);
                }
            }
            else if (EnumHelpers.IsFwdTenor(tenor))
            {
                for (int j = 0; j < FwdCurveCollection.GetCurve(tenor).Dates.Count; j++)
                {
                    DateTime curvePoint = FwdCurveCollection.GetCurve(tenor).Dates[j];
                    double riskValue = BumpAndRunFwdRisk(product, tenor, j);
                    output.AddRiskCalculation(tenor, curvePoint, riskValue);
                }
            }
            else
                throw new InvalidOperationException("tenor is not valid.");

            return output;
        }

        public ZcbRiskOutputContainer RiskAgainstAllCurvesBumpAndRun(LinearRateProduct product, DateTime asOf)
        {
            ZcbRiskOutputContainer output = new ZcbRiskOutputContainer();

            List<CurveTenor> tenors = new CurveTenor[] { CurveTenor.Fwd1M, CurveTenor.Fwd3M, CurveTenor.Fwd6M, CurveTenor.Fwd1Y }.ToList();

            foreach (CurveTenor tenor in tenors)
            {
                if (FwdCurveCollection.CurveExist(tenor) == false)
                    throw new InvalidOperationException(tenor.ToString() + " does not exist in model.");

                output.AddForwardRisk(tenor, CalculateZcbRiskBumpAndRun(product, tenor, asOf));
            }

            output.AddDiscRisk(CalculateZcbRiskBumpAndRun(product, CurveTenor.DiscOis, asOf));
            return output;
        }

        private double TempRandomNumber(Random random)
        {
            return (random.NextDouble() * (0.015 - 0.05) + 0.05)*10000.0;
        }

        // -------- RELATED TO VALUING INSTRUMENTS -------------

        public double ValueLinearRateProduct(LinearRateProduct product)
        {
            switch (product.GetInstrumentType())
            {
                case Instrument.IrSwap:
                    return IrSwapNpv((IrSwap)product);
                case Instrument.Fra:
                    return FraNpv((Fra)product);
                case Instrument.Futures:
                    return FuturesNpv((Futures)product);
                case Instrument.OisSwap:
                    return DiscCurve.OisSwapNpv((OisSwap)product, Interpolation);
                case Instrument.BasisSwap:
                    return BasisSwapNpv((BasisSwap)product);
                default:
                    throw new InvalidOperationException("product instrument type is not valid.");
            }
        }

        /// <summary>
        /// Calculate the value of an annuity
        /// </summary>
        /// <param name="AsOf"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="Tenor"></param>
        /// <param name="DayCount"></param>
        /// <param name="DayRule"></param>
        /// <param name="Method"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Calculate value from annuity from a swap schedule
        /// </summary>
        /// <param name="Schedule"></param>
        /// <param name="Method"></param>
        /// <returns></returns>
        public double Annuity(SwapSchedule Schedule, InterpMethod Method)
        {
            return Annuity(Schedule.AsOf, Schedule.StartDate, Schedule.EndDate, Schedule.Frequency, Schedule.DayCount, Schedule.DayRule, Method);
        }

        /// <summary>
        /// Calculate the par fra rate (used for curve calibration)
        /// </summary>
        /// <param name="fra"></param>
        /// <returns></returns>
        public double ParFraRate(Fra fra)
        {
            Curve fwdCurve = this.FwdCurveCollection.GetCurve(fra.ReferenceIndex);
            double rate = fwdCurve.FwdRate(fra.AsOf, fra.StartDate, fra.EndDate, fra.DayRule, fra.DayCount, Interpolation);
            return rate;
        }

        /// <summary>
        /// NPV of a FRA 
        /// </summary>
        /// <param name="fra"></param>
        /// <returns></returns>
        public double FraNpv(Fra fra)
        {
            double fraRate = FwdCurveCollection.GetCurve(fra.ReferenceIndex).FwdRate(fra.AsOf, fra.StartDate, fra.EndDate, fra.DayRule, fra.DayCount, Interpolation);
            double notional = fra.Notional;
            double discFactor = DiscCurve.DiscFactor(fra.AsOf, fra.EndDate, Interpolation);
            double coverage = DateHandling.Cvg(fra.StartDate, fra.EndDate, fra.DayCount);
            return fra.TradeSign * notional * discFactor * (fraRate - fra.FixedRate) * coverage;
        }

        /// <summary>
        /// Calculate par futures rate (used for curve calibration). Here, we value
        /// futures as the par value of a fra + a convexity adjustment.
        /// </summary>
        /// <param name="future"></param>
        /// <returns></returns>
        public double ParFutureRate(Futures future)
        {
            return ParFraRate(future.FraSameSpec) + future.Convexity;
        }

        public double FuturesNpv(Futures future)
        {
            Fra fra = future.FraSameSpec;
            double fraRate = ParFraRate(fra);
            double notional = fra.Notional;
            double convexity = future.Convexity;
            double futuresRate = fraRate + convexity;
            return notional * (1 - futuresRate);
        }

        public double ValueFloatLeg(FloatLeg floatLeg)
        {
            double floatValue = 0.0;
            double spread = floatLeg.Spread;

            for (int i = 0; i < floatLeg.
                Schedule.AdjStartDates.Count; i++)
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

        public double IrSwapNpv(IrSwap Swap)
        {
            return ValueFloatLeg(Swap.FloatLeg) - ValueFixedLeg(Swap.FixedLeg);
        }

        public double IrParSwapRate(IrSwap Swap)
        {
            double FloatPv = ValueFloatLeg(Swap.FloatLeg)/Swap.FloatLeg.Notional;
            double FixedAnnuity = Annuity(Swap.FixedLeg.Schedule, Interpolation);
            return FloatPv / FixedAnnuity;
        }

        public double BasisSwapNpv(BasisSwap swap)
        {
            return ValueFloatLeg(swap.FloatLegNoSpread) - ValueFloatLeg(swap.FloatLegSpread);
        }

        public double ParBasisSpread(BasisSwap swap)
        {
            double PvNoSpread = ValueFloatLegNoSpread(swap.FloatLegNoSpread)/swap.FloatLegNoSpread.Notional;
            double PvSpread = ValueFloatLegNoSpread(swap.FloatLegSpread)/swap.FloatLegNoSpread.Notional;
            double AnnuityNoSpread = Annuity(swap.FloatLegSpread.Schedule, Interpolation);
            return (PvNoSpread - PvSpread) / AnnuityNoSpread;
        }

        public double OisRateSimple(OisSwap swap)
        {
            return DiscCurve.OisRateSimple(swap, Interpolation);
        }

    }

}
