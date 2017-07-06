using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MasterThesis
{
    /* --- General information
     * This file contains the part of the linear rate model class
     * that works with doubles. It contains functions to value and risk
     * linear rate instruments and hold curves and an interpolation method
     * as members.
     * 
     * Because of time-constraints, a compromise had to made which meant that
     * a seperate implementataion of the linear rate model had to be created
     * to work proberly with automatic differention. Ideally, the ADouble class
     * should be the only number class used throughout the library, but there
     * was not enough time to ensure to rewrite the whole library in terms
     * of the "ADouble" class.
     * 
     */

    public partial class LinearRateModel
    {
        public Curve DiscCurve;
        public FwdCurveContainer FwdCurveCollection;
        public InterpMethod Interpolation;

        public LinearRateModel(Curve discCurve, FwdCurveContainer fwdCurveCollection, InterpMethod interpolation)
        {
            Interpolation = interpolation;
            DiscCurve = discCurve;
            FwdCurveCollection = fwdCurveCollection;
        }

        // --- Related to Bump-and-run risk calculations
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

        // Calculate BnR forward risk on a linear rate instrument
        public double BumpAndRunFwdRisk(LinearRateInstrument product, CurveTenor fwdCurve, int curvePoint, double bump = 0.0001)
        {
            double valueNoBump = ValueLinearRateProduct(product);
            LinearRateModel newModel = BumpFwdCurveAndReturn(fwdCurve, curvePoint, bump);
            double valueBump = newModel.ValueLinearRateProduct(product);
            return (valueBump - valueNoBump)/bump*0.0001; 
        }

        // Calcualate BnR disc risk on a linear rate instrument 
        public double BumpAndRunDisc(LinearRateInstrument product, int curvePoint, double bump = 0.0001)
        {
            double valueNoBump = ValueLinearRateProduct(product);
            LinearRateModel newModel = BumpDiscCurveAndReturn(curvePoint, bump);
            double valueBump = newModel.ValueLinearRateProduct(product);
            return (valueBump - valueNoBump)/bump*0.0001; 
        }

        public ZcbRiskOutput CalculateZcbRiskBumpAndRun(LinearRateInstrument product, CurveTenor tenor, DateTime asOf)
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

        public ZcbRiskOutputContainer RiskAgainstAllCurvesBumpAndRun(LinearRateInstrument product, DateTime asOf)
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

        // --- Related to valuing linear rate instrument (non-AD)
        public double ValueLinearRateProduct(LinearRateInstrument product)
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
                    return BasisSwapNpv((TenorBasisSwap)product);
                case Instrument.Deposit:
                    return DepositNpv((Deposit)product);
                default:
                    throw new InvalidOperationException("product instrument type is not valid.");
            }
        }

        public double Annuity(DateTime asOf, DateTime startDate, DateTime endDate, CurveTenor tenor, DayCount dayCount, DayRule dayRule, InterpMethod interpolation)
        {
            SwapSchedule annuitySchedule = new SwapSchedule(asOf, startDate, endDate, dayCount, dayRule, tenor);
            double result = 0.0;
            for (int i = 0; i<annuitySchedule.AdjEndDates.Count; i++)
            {
                result += annuitySchedule.Coverages[i] * DiscCurve.DiscFactor(asOf, annuitySchedule.AdjEndDates[i], dayCount, interpolation);
            }
            return result;
        }

        public double Annuity(SwapSchedule Schedule, InterpMethod Method)
        {
            return Annuity(Schedule.AsOf, Schedule.StartDate, Schedule.EndDate, Schedule.Frequency, Schedule.DayCount, Schedule.DayRule, Method);
        }

        // Fras
        public double ParFraRate(Fra fra)
        {
            Curve fwdCurve = this.FwdCurveCollection.GetCurve(fra.ReferenceIndex);
            double rate = fwdCurve.FwdRate(fra.AsOf, fra.StartDate, fra.EndDate, fra.DayRule, fra.DayCount, Interpolation);
            return rate;
        }

        public double FraNpv(Fra fra)
        {
            double fraRate = FwdCurveCollection.GetCurve(fra.ReferenceIndex).FwdRate(fra.AsOf, fra.StartDate, fra.EndDate, fra.DayRule, fra.DayCount, Interpolation);
            double notional = fra.Notional;
            double discFactor = DiscCurve.DiscFactor(fra.AsOf, fra.StartDate, fra.DayCount, Interpolation); // Note from today and to startDate => Market FRA
            double coverage = DateHandling.Cvg(fra.StartDate, fra.EndDate, fra.DayCount);
            return fra.TradeSign * notional * discFactor * coverage * (fra.FixedRate - fraRate);
        }

        // Futures
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

        // Swap legs
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
                double discFactor = DiscCurve.DiscFactor(floatLeg.AsOf, endDate, floatLeg.Schedule.DayCount, Interpolation);
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
                double discFactor = DiscCurve.DiscFactor(floatLeg.AsOf, endDate, floatLeg.Schedule.DayCount, Interpolation);
                floatValue += (fwdRate + spread) * cvg * discFactor;
            }
            return floatValue * floatLeg.Notional;
        }

        public double ValueFixedLeg(FixedLeg FixedLeg)
        {
            double FixedAnnuity = Annuity(FixedLeg.Schedule, Interpolation);
            return FixedLeg.FixedRate * FixedAnnuity * FixedLeg.Notional;
        }

        // Interest rate swap
        public double IrSwapNpv(IrSwap swap)
        {
            return swap.TradeSign*(ValueFixedLeg(swap.FixedLeg)- ValueFloatLeg(swap.FloatLeg));
        }

        public double IrParSwapRate(IrSwap swap)
        {
            double floatPV = ValueFloatLeg(swap.FloatLeg)/swap.FloatLeg.Notional;
            double fixedAnnuity = Annuity(swap.FixedLeg.Schedule, Interpolation);
            return floatPV / fixedAnnuity;
        }

        // Basis swaps
        public double BasisSwapNpv(TenorBasisSwap swap)
        {
            return swap.TradeSign * (ValueLinearRateProduct(swap.SwapNoSpread) - ValueLinearRateProduct(swap.SwapSpread));
        }

        public double BasisSwapNpvTwoLegs(TenorBasisSwap swap)
        {
            return swap.TradeSign * (ValueFloatLeg(swap.FloatLegNoSpread) - ValueFloatLeg(swap.FloatLegSpread));
        }

        public double ParBasisSpread(TenorBasisSwap swap)
        {
            double pvNoSpread = ValueFloatLegNoSpread(swap.FloatLegNoSpread)/swap.FloatLegNoSpread.Notional;
            double pvSpread = ValueFloatLegNoSpread(swap.FloatLegSpread)/swap.FloatLegSpread.Notional;
            double annuityOfSpreadFixedLeg = Annuity(swap.SwapSpread.FixedLeg.Schedule, Interpolation);
            double annuityOfNoSpreadFixedLeg = Annuity(swap.SwapNoSpread.FixedLeg.Schedule, Interpolation);

            // Temp
            //annuityOfSpreadFixedLeg = Annuity(swap.FloatLegSpread.Schedule, Interpolation);
            //annuityOfNoSpreadFixedLeg = Annuity(swap.FloatLegSpread.Schedule, Interpolation);

            return pvNoSpread / annuityOfNoSpreadFixedLeg - pvSpread / annuityOfSpreadFixedLeg;
        }
        public double ParBasisSpreadTwoLegs(TenorBasisSwap swap)
        {
            double PvNoSpread = ValueFloatLegNoSpread(swap.FloatLegNoSpread) / swap.FloatLegNoSpread.Notional;
            double PvSpread = ValueFloatLegNoSpread(swap.FloatLegSpread) / swap.FloatLegSpread.Notional;
            double AnnuityNoSpread = Annuity(swap.FloatLegNoSpread.Schedule, Interpolation);
            return (PvSpread - PvNoSpread) / AnnuityNoSpread;
        }

        // Ois swaps (more functions are located under the Curve-classes)
        public double OisRateSimple(OisSwap swap)
        {
            return DiscCurve.OisRateSimple(swap, Interpolation);
        }

        public double DepositNpv(Deposit deposit)
        {
            double discFactor = DiscCurve.DiscFactor(deposit.AsOf, deposit.EndDate, deposit.DayCount, Interpolation);
            double cvg = DateHandling.Cvg(deposit.StartDate, deposit.EndDate, deposit.DayCount);
            return deposit.TradeSign * discFactor * deposit.Notional * (1.0 + deposit.FixedRate * cvg);
        }

        public double ParDepositRate(Deposit deposit)
        {
            double discFactor = DiscCurve.DiscFactor(deposit.StartDate, deposit.EndDate, deposit.DayCount, Interpolation);
            double cvg = DateHandling.Cvg(deposit.StartDate, deposit.EndDate, deposit.DayCount);
            return 1.0 / cvg * (1.0 / discFactor - 1.0);
        }

    }

}
