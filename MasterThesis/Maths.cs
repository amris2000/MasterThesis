using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
    public struct MathConstants
    {
        public static double SqrtTwoPi = 2.506628274631;
        public static double Pi = 3.14159265359;
    }

    public static class Maths
    {
        


        public static double InterpolateCurve(List<DateTime> Dates, DateTime Date, List<double> Values, InterpMethod Method = InterpMethod.Linear)
        {
            // To-do: CATROM interpolation
            double Output = 0.0;

            if (Dates.Count() != Values.Count())
                throw new ArgumentException("Number of dates has to correspond to number of values");

            int n = Dates.Count();

            // Extrapolation (flat)
            if (Dates[0] > Date)
                return Values[0];
            else if (Dates[n - 1] <= Date)
                return Values[n - 1];
            else
            {
                // No extrapolation - find relevant index in arrays
                int j = 0;
                while (Dates[j] <= Date)
                    j = j + 1;

                j = j - 1;
                n = n - 1;
                switch (Method)
                {
                    case InterpMethod.Constant:
                        return Values[j];
                    case InterpMethod.Linear:
                        double HelpLin = Date.Subtract(Dates[j]).TotalDays * Values[j + 1] + Dates[j + 1].Subtract(Date).TotalDays * Values[j];
                        return HelpLin / (Dates[j + 1].Subtract(Dates[j]).TotalDays);
                    case InterpMethod.LogLinear:
                        double HelpLogLin1 = Date.Subtract(Dates[j]).TotalDays / Dates[j + 1].Subtract(Dates[j]).TotalDays;
                        double HelpLogLin2 = Dates[j + 1].Subtract(Date).TotalDays / Dates[j + 1].Subtract(Dates[j]).TotalDays;
                        return Math.Pow(Values[j + 1], HelpLogLin1) * Math.Pow(Values[j], HelpLogLin2);
                    case InterpMethod.Hermite:
                        double bi, bk, hi, mi, ci, di;
                        double D11, D01, D10, Dn11, Dn01, Dn10, Dk11, Dk10, Dk01;
                        int K = j + 1;
                        if (j == 0)
                        {
                            D11 = Dates[2].Subtract(Dates[0]).TotalDays;
                            D10 = Dates[2].Subtract(Dates[1]).TotalDays;
                            D01 = Dates[1].Subtract(Dates[0]).TotalDays;
                            Dk11 = Dates[K + 1].Subtract(Dates[K - 1]).TotalDays;
                            Dk10 = Dates[K + 1].Subtract(Dates[K]).TotalDays;
                            Dk01 = Dates[K].Subtract(Dates[K - 1]).TotalDays;

                            bi = Math.Pow((D11 + D01) * (Values[1] - Values[0]) / D01 - D01 * (Values[2] - Values[1]) / D10 * D11, -1);
                            bk = Math.Pow(Dk10 * (Values[K] - Values[K - 1]) / Dk01 + Dk01 * (Values[K + 1] - Values[K]) / Dk10 * Dk11, -1);
                        }
                        else if (j == n - 1)
                        {
                            D11 = Dates[j + 1].Subtract(Dates[j - 1]).TotalDays;
                            D10 = Dates[j + 1].Subtract(Dates[j]).TotalDays;
                            D01 = Dates[j].Subtract(Dates[j - 1]).TotalDays;
                            Dn11 = Dates[n].Subtract(Dates[n - 2]).TotalDays;
                            Dn10 = Dates[n].Subtract(Dates[n - 1]).TotalDays;
                            Dn01 = Dates[n - 1].Subtract(Dates[n - 2]).TotalDays;

                            bi = Math.Pow(D10 * (Values[j] - Values[j - 1]) / Dn01 + D01 * (Values[j + 1] - Values[j]) / D10 * D11, -1);
                            bk = Math.Pow(-Dn10 * (Values[n - 1] - Values[n - 2]) / Dn10 - (Dn10 - Dn11) * (Values[n] - Values[n - 1]) / Dn10 * Dn11, -1);
                        }
                        else
                        {
                            D11 = Dates[j + 1].Subtract(Dates[j - 1]).TotalDays;
                            D10 = Dates[j + 1].Subtract(Dates[j]).TotalDays;
                            D01 = Dates[j].Subtract(Dates[j - 1]).TotalDays;
                            Dk11 = Dates[K + 1].Subtract(Dates[K - 1]).TotalDays;
                            Dk10 = Dates[K + 1].Subtract(Dates[K]).TotalDays;
                            Dk01 = Dates[K].Subtract(Dates[K - 1]).TotalDays;

                            bi = Math.Pow(D10 * (Values[j] - Values[j - 1]) / D01 + D01 * (Values[j + 1] - Values[j]) / D10 * D11, -1);
                            bk = Math.Pow(Dk10 * (Values[K] - Values[K - 1]) / Dk01 + Dk01 * (Values[K + 1] - Values[K]) / Dk10 * Dk11, -1);
                        }

                        hi = Dates[j + 1].Subtract(Dates[j]).TotalDays;
                        mi = (Values[j + 1] - Values[j]) / hi;
                        ci = (3 * mi - bk - 2 * bi) / hi;
                        di = (bk + bi - 2 * mi) * Math.Pow(hi, -2.0);
                        return Values[j] + bi * Date.Subtract(Dates[j]).TotalDays +
                                            ci * Math.Pow(Date.Subtract(Dates[j]).TotalDays, 2) +
                                            di * Math.Pow(Date.Subtract(Dates[j]).TotalDays, 3);
                }


            }

            return Output;

        }

        public static double InterpolateCurve(DateTime[] Dates, DateTime Date, double[] Values, InterpMethod Method = InterpMethod.Linear)
        {
            // To-do: CATROM interpolation
            double Output = 0.0;

            if (Dates.Length != Values.Length)
                throw new ArgumentException("Number of dates has to correspond to number of values");

            int n = Dates.Length;

            // Extrapolation (flat)
            if (Dates[0] > Date)
                return Values[0];
            else if (Dates[n - 1] <= Date)
                return Values[n - 1];
            else
            {
                // No extrapolation - find relevant index in arrays
                int j = 0;
                while (Dates[j] <= Date)
                        j = j + 1;

                j = j - 1;
                n = n - 1;
                switch (Method)
                {
                    case InterpMethod.Constant:
                        return Values[j];
                    case InterpMethod.Linear:
                        double HelpLin = Date.Subtract(Dates[j]).TotalDays * Values[j + 1] + Dates[j + 1].Subtract(Date).TotalDays * Values[j];
                        return HelpLin / (Dates[j + 1].Subtract(Dates[j]).TotalDays);
                    case InterpMethod.LogLinear:
                        double HelpLogLin1 = Date.Subtract(Dates[j]).TotalDays / Dates[j + 1].Subtract(Dates[j]).TotalDays;
                        double HelpLogLin2 = Dates[j + 1].Subtract(Date).TotalDays / Dates[j + 1].Subtract(Dates[j]).TotalDays;
                        return Math.Pow(Values[j + 1], HelpLogLin1) * Math.Pow(Values[j], HelpLogLin2);
                    case InterpMethod.Hermite:
                        double bi, bk, hi, mi, ci, di;
                        double D11, D01, D10, Dn11, Dn01, Dn10, Dk11, Dk10, Dk01;
                        int K = j+1;
                        if (j == 0)
                        {
                            D11 = Dates[2].Subtract(Dates[0]).TotalDays;
                            D10 = Dates[2].Subtract(Dates[1]).TotalDays;
                            D01 = Dates[1].Subtract(Dates[0]).TotalDays;
                            Dk11 = Dates[K + 1].Subtract(Dates[K - 1]).TotalDays;
                            Dk10 = Dates[K + 1].Subtract(Dates[K]).TotalDays;
                            Dk01 = Dates[K].Subtract(Dates[K - 1]).TotalDays;

                            bi = Math.Pow((D11 + D01) * (Values[1] - Values[0]) / D01 - D01 * (Values[2] - Values[1]) / D10 * D11, -1);
                            bk = Math.Pow(Dk10 * (Values[K] - Values[K - 1]) / Dk01 + Dk01 * (Values[K + 1] - Values[K]) / Dk10 * Dk11, -1);
                        }
                        else if (j == n-1)
                        {
                            D11 = Dates[j + 1].Subtract(Dates[j-1]).TotalDays;
                            D10 = Dates[j + 1].Subtract(Dates[j]).TotalDays;
                            D01 = Dates[j].Subtract(Dates[j - 1]).TotalDays;
                            Dn11 = Dates[n].Subtract(Dates[n - 2]).TotalDays;
                            Dn10 = Dates[n].Subtract(Dates[n-1]).TotalDays;
                            Dn01 = Dates[n-1].Subtract(Dates[n - 2]).TotalDays;

                            bi = Math.Pow(D10 * (Values[j] - Values[j - 1]) / Dn01 + D01 * (Values[j + 1] - Values[j]) / D10 * D11, -1);
                            bk = Math.Pow(-Dn10 * (Values[n - 1] - Values[n - 2]) / Dn10 - (Dn10 - Dn11) * (Values[n] - Values[n - 1]) / Dn10 * Dn11, -1);
                        }
                        else
                        {
                            D11 = Dates[j + 1].Subtract(Dates[j - 1]).TotalDays;
                            D10 = Dates[j + 1].Subtract(Dates[j]).TotalDays;
                            D01 = Dates[j].Subtract(Dates[j - 1]).TotalDays;
                            Dk11 = Dates[K + 1].Subtract(Dates[K - 1]).TotalDays;
                            Dk10 = Dates[K + 1].Subtract(Dates[K]).TotalDays;
                            Dk01 = Dates[K].Subtract(Dates[K - 1]).TotalDays;

                            bi = Math.Pow(D10 * (Values[j] - Values[j - 1]) / D01 + D01 * (Values[j + 1] - Values[j]) / D10 * D11, -1);
                            bk = Math.Pow(Dk10 * (Values[K] - Values[K - 1]) / Dk01 + Dk01 * (Values[K + 1] - Values[K]) / Dk10 * Dk11, -1);
                        }

                        hi = Dates[j + 1].Subtract(Dates[j]).TotalDays;
                        mi = (Values[j + 1] - Values[j]) / hi;
                        ci = (3 * mi - bk - 2 * bi) / hi;
                        di = (bk + bi - 2 * mi) * Math.Pow(hi, -2.0);
                        return Values[j] + bi * Date.Subtract(Dates[j]).TotalDays +
                                            ci* Math.Pow(Date.Subtract(Dates[j]).TotalDays, 2) +
                                            di * Math.Pow(Date.Subtract(Dates[j]).TotalDays, 3);
                }
                              

            }

            return Output;

        }
        
        public static ADouble NormalCdf(ADouble x) 
        {
            // Courtesy of Antoine Savine (Danske Bank)
            if (x < -10.0)
                return new ADouble(0.0);
            else if (x > 10.0)
                return new ADouble(1.0);
            if (x < 0.0)
                return 1.0 - NormalCdf(-x);

            // Constants
            double p = 0.2316419;
            double b1 = 0.319381530;
            double b2 = -0.356563782;
            double b3 = 1.781477937;
            double b4 = -1.821255978;
            double b5 = 1.330274429;

            // Transform
            ADouble t = 1.0 / (1.0 + p * x);
            ADouble pol = t * (b1 + t * (b2 + t * (b3 + t * (b4 + t * b5))));
            ADouble pdf = new ADouble();

            if (x < -10.0 || 10.0 < x)
                pdf = new ADouble(0.0);
            else
                pdf = ADouble.Exp(-0.5 * x * x) / MathConstants.SqrtTwoPi;

            return 1.0 - pdf * pol;
        }

        public static ADouble InvNormalCdf(ADouble p)
        {
            // Courtesy to Antoine Savine (Danske Bank)
            if (p > 0.5)
                return -InvNormalCdf(1.0 - p);

            ADouble Up = new ADouble();

            if (p < 1.0e-15)
                Up = new ADouble(1.0e-15);
            else
                Up = p;

            //	constants
            double a0 = 2.50662823884;
            double a1 = -18.61500062529;
            double a2 = 41.39119773534;
            double a3 = -25.44106049637;

            double b0 = -8.47351093090;
            double b1 = 23.08336743743;
            double b2 = -21.06224101826;
            double b3 = 3.13082909833;

            double c0 = 0.3374754822726147;
            double c1 = 0.9761690190917186;
            double c2 = 0.1607979714918209;
            double c3 = 0.0276438810333863;
            double c4 = 0.0038405729373609;
            double c5 = 0.0003951896511919;
            double c6 = 0.0000321767881768;
            double c7 = 0.0000002888167364;
            double c8 = 0.0000003960315187;

            ADouble x = Up - 0.5;
            ADouble r = new ADouble();

            if (Math.Abs(x.Value)<0.42)
            {
                r = x * x;
                r = x * (((a3 * r + a2) * r + a1) * r + a0) / ((((b3 * r + b2) * r + b1) * r + b0) * r + 1.0);
                return r;
            }

            // Log-Log approximation
            r = Up;
            r = ADouble.Log(-ADouble.Log(r));
            r = c0 + r * (c1 + r * (c2 + r * (c3 + r * (c4 + r * (c5 + r * (c6 + r * (c7 + r * c8)))))));

            r = -r;
            return r;
        }

    }
}
