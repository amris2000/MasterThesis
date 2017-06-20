using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace MasterThesis
{
    /// <summary>
    /// AAD part of the LinearRateModel class
    /// </summary>
    public partial class LinearRateModel
    {
        public Curve_AD ADDiscCurve;
        public ADFwdCurveContainer ADFwdCurveCollection;
        bool ADCurvesHasBeenSet = false;

        public LinearRateModel(Curve_AD discCurve, ADFwdCurveContainer fwdCurveCollection, InterpMethod interpolation = InterpMethod.Linear)
        {
            ADDiscCurve = discCurve;
            ADFwdCurveCollection = fwdCurveCollection;
            Interpolation = interpolation;
        }

        public ADouble ValueLinearRateProductAD(LinearRateInstrument product)
        {
            switch (product.GetInstrumentType())
            {
                case Instrument.IrSwap:
                    return IrSwapNpvAD((IrSwap)product);
                case Instrument.Fra:
                    return FraNpvAD((Fra)product);
                case Instrument.Futures:
                    return FuturesNpvAD((Futures)product);
                case Instrument.OisSwap:
                    return ADDiscCurve.OisSwapNpvAD((OisSwap)product, Interpolation);
                case Instrument.BasisSwap:
                    return BasisSwapNpvAD((BasisSwap)product);
                default:
                    throw new InvalidOperationException("product instrument type is not valid.");
            }
        }

        // --------- RELATED TO AD RISK 

        public string[] CreateIdentArray()
        {
            List<string> identifiers = new List<string>();

            for (int i = 0; i < DiscCurve.Dates.Count; i++)
                identifiers.Add(CurveTenor.DiscOis.ToString() + DiscCurve.Dates[i].ToString("dd/MM/yyyy"));

            foreach (CurveTenor tenor in FwdCurveCollection.Curves.Keys)
            {
                for (int i = 0; i < FwdCurveCollection.Curves[tenor].Dates.Count; i++)
                    identifiers.Add(tenor.ToString() + FwdCurveCollection.Curves[tenor].Dates[i].ToString("dd/MM/yyyy"));
            }

            return identifiers.ToArray();
        }

        public void InitiateTapeFromModel()
        {
            string[] identifiers = CreateIdentArray();
            ADouble[] variables = CreateAdVariableArray();

            AADTape.Initialize(variables, identifiers);
        }

        public void InitiateTapeFromModelFwdCurvesOnly()
        {
            ADouble[] variables = CreateAdVariableArrayFwdCurvesOnly();
            AADTape.Initialize(variables);
        }

        public object[,] CreateTestOutputAD(LinearRateInstrument product)
        {
            double[] values = ZcbRiskProductAD(product);
            string[] identifiers = CreateIdentArray();
            object[,] output = new object[values.Length, 2];

            for (int i = 0; i < values.Length; i++)
            {
                output[i, 0] = identifiers[i];
                output[i, 1] = values[i];
            }

            return output;
        }

        public double[] ZcbRiskProductAD(LinearRateInstrument product)
        {
            AADTape.ResetTape();
            if (ADCurvesHasBeenSet == false)
                SetAdCurvesFromOrdinaryCurve();

            InitiateTapeFromModel();
            ValueLinearRateProductAD(product);
            AADTape.InterpretTape();
            double[] output = AADTape.GetGradient();
            AADTape.ResetTape();
            return ScaleGradient(0.0001, output);

        }

        private double[] ScaleGradient(double scale, double[] toBeScaled)
        {
            for (int i = 0; i < toBeScaled.Length; i++)
                toBeScaled[i] = toBeScaled[i] * scale;

            return toBeScaled;
        }

        public ZcbRiskOutputContainer ZcbRiskProductOutputContainer(LinearRateInstrument product, DateTime asOf)
        {
            double[] risk = ZcbRiskProductAD(product);

            ZcbRiskOutputContainer output = new ZcbRiskOutputContainer();
            ZcbRiskOutput discRisk = new MasterThesis.ZcbRiskOutput(asOf);

            int j = 0;
            for (int i = 0; i < DiscCurve.Dimension; i++)
            {
                discRisk.AddRiskCalculation(CurveTenor.DiscOis, DiscCurve.Dates[i], risk[j]);
                j++;
            }

            output.AddDiscRisk(discRisk);

            CurveTenor[] tenors = new CurveTenor[] { CurveTenor.Fwd1M, CurveTenor.Fwd3M, CurveTenor.Fwd6M, CurveTenor.Fwd1Y };

            foreach (CurveTenor tenor in tenors.ToList())
            {
                ZcbRiskOutput tempFwdRisk = new ZcbRiskOutput(asOf);

                for (int i = 0; i < FwdCurveCollection.GetCurve(tenor).Dimension; i++)
                {
                    DateTime curvePoint = FwdCurveCollection.GetCurve(tenor).Dates[i];
                    tempFwdRisk.AddRiskCalculation(tenor, curvePoint, risk[j]);
                    j++;
                }

                output.AddForwardRisk(tenor, tempFwdRisk);
            }

            return output;

        }

        public ADouble[] CreateAdVariableArray()
        {
            List<ADouble> output = new List<ADouble>();

            output.AddRange(ADDiscCurve.Values);

            foreach (CurveTenor tenor in ADFwdCurveCollection.Curves.Keys)
                output.AddRange(ADFwdCurveCollection.Curves[tenor].Values);

            return output.ToArray();
        }

        public ADouble[] CreateAdVariableArrayFwdCurvesOnly()
        {
            List<ADouble> output = new List<ADouble>();

            foreach (CurveTenor tenor in ADFwdCurveCollection.Curves.Keys)
                output.AddRange(ADFwdCurveCollection.Curves[tenor].Values);

            return output.ToArray();
        }

        public void SetAdCurvesFromOrdinaryCurve()
        {
            List<ADouble> discValues = new List<ADouble>();
            for (int i = 0; i < DiscCurve.Values.Count; i++)
                discValues.Add(new MasterThesis.ADouble(DiscCurve.Values[i]));

            ADDiscCurve = new MasterThesis.Curve_AD(DiscCurve.Dates, discValues);
            ADFwdCurveCollection = new ADFwdCurveContainer();

            foreach (CurveTenor tenor in FwdCurveCollection.Curves.Keys)
            {
                List<ADouble> tempValues = new List<ADouble>();

                for (int i = 0; i < FwdCurveCollection.GetCurve(tenor).Values.Count; i++)
                    tempValues.Add(new ADouble(FwdCurveCollection.GetCurve(tenor).Values[i]));

                Curve_AD tempCurve = new Curve_AD(FwdCurveCollection.GetCurve(tenor).Dates, tempValues);
                ADFwdCurveCollection.AddCurve(tempCurve, tenor);
            }

            ADCurvesHasBeenSet = true;
        }

        public void SetAdDiscCurveFromOrdinaryCurve()
        {
            List<ADouble> discValues = new List<ADouble>();
            for (int i = 0; i < DiscCurve.Values.Count; i++)
                discValues.Add(new MasterThesis.ADouble(DiscCurve.Values[i]));
            ADDiscCurve = new MasterThesis.Curve_AD(DiscCurve.Dates, discValues);
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
        public ADouble AnnuityAD(DateTime AsOf, DateTime StartDate, DateTime EndDate, CurveTenor Tenor, DayCount DayCount, DayRule DayRule, InterpMethod Method)
        {
            SwapSchedule AnnuitySchedule = new SwapSchedule(AsOf, StartDate, EndDate, DayCount, DayRule, Tenor);
            ADouble Out = 0.0;
            DateTime PayDate;
            for (int i = 0; i < AnnuitySchedule.AdjEndDates.Count; i++)
            {
                PayDate = AnnuitySchedule.AdjEndDates[i];
                Out += AnnuitySchedule.Coverages[i] * ADDiscCurve.DiscFactor(AsOf, PayDate, Method);
            }
            return Out;
        }

        /// <summary>
        /// Calculate value from annuity from a swap schedule
        /// </summary>
        /// <param name="Schedule"></param>
        /// <param name="Method"></param>
        /// <returns></returns>
        public ADouble AnnuityAD(SwapSchedule Schedule, InterpMethod Method)
        {
            return AnnuityAD(Schedule.AsOf, Schedule.StartDate, Schedule.EndDate, Schedule.Frequency, Schedule.DayCount, Schedule.DayRule, Method);
        }

        /// <summary>
        /// Calculate the par fra rate (used for curve calibration)
        /// </summary>
        /// <param name="fra"></param>
        /// <returns></returns>
        public ADouble ParFraRateAD(Fra fra)
        {
            Curve_AD fwdCurve = this.ADFwdCurveCollection.GetCurve(fra.ReferenceIndex);
            ADouble rate = fwdCurve.FwdRate(fra.AsOf, fra.StartDate, fra.EndDate, fra.DayRule, fra.DayCount, Interpolation);
            return rate;
        }

        /// <summary>
        /// NPV of a FRA 
        /// </summary>
        /// <param name="fra"></param>
        /// <returns></returns>
        public ADouble FraNpvAD(Fra fra)
        {
            ADouble fraRate = ADFwdCurveCollection.GetCurve(fra.ReferenceIndex).FwdRate(fra.AsOf, fra.StartDate, fra.EndDate, fra.DayRule, fra.DayCount, Interpolation);
            double notional = fra.Notional;
            ADouble discFactor = ADDiscCurve.DiscFactor(fra.AsOf, fra.EndDate, Interpolation);
            double coverage = DateHandling.Cvg(fra.StartDate, fra.EndDate, fra.DayCount);
            return fra.TradeSign * notional * discFactor * (fraRate - fra.FixedRate) * coverage;
        }

        /// <summary>
        /// Calculate par futures rate (used for curve calibration). Here, we value
        /// futures as the par value of a fra + a convexity adjustment.
        /// </summary>
        /// <param name="future"></param>
        /// <returns></returns>
        public ADouble ParFutureRateAD(Futures future)
        {
            return ParFraRateAD(future.FraSameSpec) + future.Convexity;
        }

        public ADouble FuturesNpvAD(Futures future)
        {
            Fra fra = future.FraSameSpec;
            ADouble fraRate = ParFraRateAD(fra);
            ADouble notional = fra.Notional;
            ADouble convexity = future.Convexity;
            ADouble futuresRate = fraRate + convexity;
            return notional * (1.0 - futuresRate);
        }

        public ADouble ValueFloatLegAD(FloatLeg floatLeg)
        {
            ADouble floatValue = 0.0;
            ADouble spread = floatLeg.Spread;

            for (int i = 0; i < floatLeg.Schedule.AdjStartDates.Count; i++)
            {
                DateTime startDate = floatLeg.Schedule.AdjStartDates[i];
                DateTime endDate = floatLeg.Schedule.AdjEndDates[i];
                ADouble cvg = floatLeg.Schedule.Coverages[i];
                ADouble fwdRate = ADFwdCurveCollection.GetCurve(floatLeg.Tenor).FwdRate(floatLeg.AsOf, startDate, endDate, floatLeg.Schedule.DayRule, floatLeg.Schedule.DayCount, Interpolation);
                ADouble discFactor = ADDiscCurve.DiscFactor(floatLeg.AsOf, endDate, Interpolation);
                floatValue += (fwdRate + spread) * cvg * discFactor;
            }

            return floatValue * floatLeg.Notional;
        }

        public ADouble ValueFloatLegNoSpreadAD(FloatLeg floatLeg)
        {
            ADouble floatValue = 0.0;
            ADouble spread = 0.0;

            for (int i = 0; i < floatLeg.Schedule.AdjStartDates.Count; i++)
            {
                DateTime startDate = floatLeg.Schedule.AdjStartDates[i];
                DateTime endDate = floatLeg.Schedule.AdjEndDates[i];
                ADouble cvg = floatLeg.Schedule.Coverages[i];
                ADouble fwdRate = ADFwdCurveCollection.GetCurve(floatLeg.Tenor).FwdRate(floatLeg.AsOf, startDate, endDate, floatLeg.Schedule.DayRule, floatLeg.Schedule.DayCount, Interpolation);
                ADouble discFactor = ADDiscCurve.DiscFactor(floatLeg.AsOf, endDate, Interpolation);
                floatValue += (fwdRate + spread) * cvg * discFactor;
            }
            return floatValue * floatLeg.Notional;
        }

        public ADouble ValueFixedLegAD(FixedLeg FixedLeg)
        {
            ADouble FixedAnnuity = AnnuityAD(FixedLeg.Schedule, Interpolation);
            return FixedLeg.FixedRate * FixedAnnuity * FixedLeg.Notional;
        }

        public ADouble IrSwapNpvAD(IrSwap swap)
        {
            return (double)swap.TradeSign*(ValueFixedLegAD(swap.FixedLeg) - ValueFloatLegAD(swap.FloatLeg));
        }

        public ADouble IrParSwapRateAD(IrSwap swap)
        {
            ADouble floatPv = ValueFloatLegAD(swap.FloatLeg) / swap.FloatLeg.Notional;
            ADouble fixedAnnuity = AnnuityAD(swap.FixedLeg.Schedule, Interpolation);
            return floatPv / fixedAnnuity;
        }

        public ADouble BasisSwapNpvAD(BasisSwap swap)
        {
            return (double)swap.TradeSign*(ValueFloatLegAD(swap.FloatLegNoSpread) - ValueFloatLegAD(swap.FloatLegSpread));
        }

        public ADouble ParBasisSpreadAD(BasisSwap swap)
        {
            ADouble pvNoSpread = ValueFloatLegNoSpreadAD(swap.FloatLegNoSpread) / swap.FloatLegNoSpread.Notional;
            ADouble pvSpread = ValueFloatLegNoSpreadAD(swap.FloatLegSpread) / swap.FloatLegNoSpread.Notional;
            ADouble annuityNoSpread = AnnuityAD(swap.FloatLegSpread.Schedule, Interpolation);
            return (pvNoSpread - pvSpread) / annuityNoSpread;
        }

        public ADouble OisRateSimpleAD(OisSwap swap)
        {
            return ADDiscCurve.OisRateSimpleAD(swap, Interpolation);
        }
    }
}
