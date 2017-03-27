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
        // TO DO
        public static double Interpolation(List<double> xArr, double x, List<double> yArr, InterpMethod Method)
        {
            return 0.01;
        }

        public static double InterpolateCurve(List<DateTime> dates, DateTime inputDate, List<double> values, InterpMethod Method)
        {
            // To-do: CATROM interpolation
            double Output = 0.0;

            if (dates.Count() != values.Count())
                throw new ArgumentException("Number of dates has to correspond to number of values");

            // Convert to dates
            double[] xArr = dates.Select(i=>i.ToOADate()).ToArray();
            double x = inputDate.ToOADate();
            int n = dates.Count();

            // Extrapolation (flat)
            if (xArr[0] > x)
                return values[0];
            else if (xArr[n - 1] <= x)
                return values[n - 1];
            else
            {
                // No extrapolation - find relevant index in arrays
                int i = 0;
                int j = 0;
                //while (dates[j] <= inputDate)
                //    j = j + 1;

                for (i = 0; i < n; i++)
                {
                    if (xArr[i] <= x)
                        j = j + 1;
                    else
                        break;
                }

                j = j - 1;

                //j = j - 1;
                //n = n - 1;
                switch (Method)
                {
                    case InterpMethod.Constant:
                        return values[j];
                    case InterpMethod.Linear:
                        //double HelpLin = inputDate.Subtract(dates[j]).TotalDays * values[j + 1] + dates[j + 1].Subtract(inputDate).TotalDays * values[j];
                        //return HelpLin / (dates[j + 1].Subtract(dates[j]).TotalDays);
                        double tempLinear = (x - xArr[j]) * values[j + 1] + (xArr[j + 1] - x) * values[j];
                        return tempLinear/ (xArr[j+1] - xArr[j]);
                    case InterpMethod.LogLinear:

                        // Log-Linear interpolation is only valid for positive stuff
                        if (values[j + 1] < 0 || values[j] < 0)
                            return values[j];
                        else
                        {
                            // double HelpLogLin1 = inputDate.Subtract(dates[j]).TotalDays / dates[j + 1].Subtract(dates[j]).TotalDays;
                            // double HelpLogLin2 = dates[j + 1].Subtract(inputDate).TotalDays / dates[j + 1].Subtract(dates[j]).TotalDays;
                            // return Math.Pow(values[j + 1], HelpLogLin1) * Math.Pow(values[j], HelpLogLin2);

                            double tempLogLinear1 = (x - xArr[j]) / (xArr[j + 1] - xArr[j]);
                            double tempLogLinear2 = (xArr[j + 1] - x) / (xArr[j + 1] - xArr[j]);
                            return Math.Pow(values[j + 1], tempLogLinear1) * Math.Pow(values[j], tempLogLinear2);       
                        }

                    case InterpMethod.Hermite:
                        double bi, bk, hi, mi, ci, di;
                        //double D11, D01, D10, Dn11, Dn01, Dn10, Dk11, Dk10, Dk01;
                        int k = j + 1;
                        if (j == 0)
                        {
                            //D11 = dates[2].Subtract(dates[0]).TotalDays;
                            //D10 = dates[2].Subtract(dates[1]).TotalDays;
                            //D01 = dates[1].Subtract(dates[0]).TotalDays;
                            //Dk11 = dates[K + 1].Subtract(dates[K - 1]).TotalDays;
                            //Dk10 = dates[K + 1].Subtract(dates[K]).TotalDays;
                            //Dk01 = dates[K].Subtract(dates[K - 1]).TotalDays;

                            //bi = Math.Pow((D11 + D01) * (values[1] - values[0]) / D01 - D01 * (values[2] - values[1]) / D10 * D11, -1);
                            //bk = Math.Pow(Dk10 * (values[K] - values[K - 1]) / Dk01 + Dk01 * (values[K + 1] - values[K]) / Dk10 * Dk11, -1);

                            bi = (xArr[2] + xArr[1] - 2 * xArr[0]) * (values[1] - values[0]) / (xArr[1] - xArr[0]);
                            bi += -1 * (xArr[1] - xArr[0]) * (values[2] - values[1]) / (xArr[2] - xArr[1]);
                            bi *= Math.Pow(xArr[2] - xArr[0], -1.0);

                            bk = (xArr[k + 1] - xArr[k]) * (values[k] - values[k - 1]) / (xArr[k] - xArr[k - 1]);
                            bk += (xArr[k] - xArr[k - 1]) * (values[k + 1] - values[k]) / (xArr[k + 1] - xArr[k]);
                            bk *= Math.Pow(xArr[k + 1] - xArr[k - 1],-1);

                        }
                        else if (j == n - 2)
                        {
                            //D11 = dates[j + 1].Subtract(dates[j - 1]).TotalDays;
                            //D10 = dates[j + 1].Subtract(dates[j]).TotalDays;
                            //D01 = dates[j].Subtract(dates[j - 1]).TotalDays;
                            //Dn11 = dates[n].Subtract(dates[n - 2]).TotalDays;
                            //Dn10 = dates[n].Subtract(dates[n - 1]).TotalDays;
                            //Dn01 = dates[n - 1].Subtract(dates[n - 2]).TotalDays;

                            //bi = Math.Pow(D10 * (values[j] - values[j - 1]) / Dn01 + D01 * (values[j + 1] - values[j]) / D10 * D11, -1);
                            //bk = Math.Pow(-Dn10 * (values[n - 1] - values[n - 2]) / Dn10 - (Dn10 - Dn11) * (values[n] - values[n - 1]) / Dn10 * Dn11, -1);

                            bi = (xArr[j + 1] - xArr[j]) * (values[j] - values[k - 1]) / (xArr[j] - xArr[j - 1]);
                            bi += (xArr[j] - xArr[j - 1]) * (values[j + 1] - values[j]) / (xArr[j + 1] - xArr[j]);
                            bi *= Math.Pow(xArr[j + 1] - xArr[j - 1], -1.0);

                            bk = -1*(xArr[n-1] - xArr[n - 2]) * (values[n - 2] - values[n - 3]) / (xArr[n - 2] - xArr[n - 3]);
                            bk += -1 * (xArr[n - 1] - xArr[n - 2] - (xArr[n - 1] - xArr[n - 3])) * (values[n - 1] - values[n - 2]) / (xArr[n - 1] - xArr[n - 2]);
                            bk *= Math.Pow(xArr[n-1] - xArr[n - 3], -1.0);
                        }
                        else
                        {
                            //D11 = dates[j + 1].Subtract(dates[j - 1]).TotalDays;
                            //D10 = dates[j + 1].Subtract(dates[j]).TotalDays;
                            //D01 = dates[j].Subtract(dates[j - 1]).TotalDays;
                            //Dk11 = dates[K + 1].Subtract(dates[K - 1]).TotalDays;
                            //Dk10 = dates[K + 1].Subtract(dates[K]).TotalDays;
                            //Dk01 = dates[K].Subtract(dates[K - 1]).TotalDays;

                            //bi = Math.Pow(D10 * (values[j] - values[j - 1]) / D01 + D01 * (values[j + 1] - values[j]) / D10 * D11, -1);
                            //bk = Math.Pow(Dk10 * (values[K] - values[K - 1]) / Dk01 + Dk01 * (values[K + 1] - values[K]) / Dk10 * Dk11, -1);

                            bi = (xArr[j + 1] - xArr[j]) * (values[j] - values[j - 1]) / (xArr[j] - xArr[j - 1]);
                            bi += (xArr[j] - xArr[j - 1]) * (values[j + 1] - values[j]) / (xArr[j + 1] - xArr[j]);
                            bi *= Math.Pow(xArr[j + 1] - xArr[j - 1],-1.0);

                            bk = (xArr[k + 1] - xArr[k]) * (values[k] - values[k - 1]) / (xArr[k] - xArr[k-1]);
                            bk += (xArr[k] - xArr[k - 1]) * (values[k + 1] - values[k]) / (xArr[k + 1] - xArr[k]);
                            bk *= Math.Pow(xArr[k + 1] - xArr[k - 1],-1.0);
                        }

                        //hi = dates[j + 1].Subtract(dates[j]).TotalDays;
                        //return values[j] + bi * inputDate.Subtract(dates[j]).TotalDays +
                        //                    ci * Math.Pow(inputDate.Subtract(dates[j]).TotalDays, 2) +
                        //                    di * Math.Pow(inputDate.Subtract(dates[j]).TotalDays, 3);

                        hi = xArr[j + 1] - xArr[j];
                        mi = (values[j + 1] - values[j]) / hi;
                        ci = (3.0 * mi - bk - 2.0 * bi) / hi;
                        di = (bk + bi - 2.0 * mi) * Math.Pow(hi, -2.0);

                        return values[j] + bi*(x - xArr[j]) + ci * Math.Pow(x - xArr[j], 2.0) + di *Math.Pow(x - xArr[j], 3.0);
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

        public static double NormalCdf(double x)
        {
            // Courtesy of Antoine Savine (Danske Bank)
            if (x < -10.0)
                return 0.0;
            else if (x > 10.0)
                return 1.0;
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
            double t = 1.0 / (1.0 + p * x);
            double pol = t * (b1 + t * (b2 + t * (b3 + t * (b4 + t * b5))));
            double pdf;

            if (x < -10.0 || 10.0 < x)
                pdf = 0.0;
            else
                pdf = Math.Exp(-0.5 * x * x) / MathConstants.SqrtTwoPi;

            return 1.0 - pdf * pol;
        }

        // TEMPLATE THIS
        public static double U2G(double u)
        {
            return InvNormalCdf(u);
        }

        public static double InvNormalCdf(double p)
        {
            // Courtesy to Antoine Savine (Danske Bank)
            if (p > 0.5)
                return -InvNormalCdf(1.0 - p);

            double Up = new double();

            if (p < 1.0e-15)
                Up = 1.0e-15;
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

            double x = Up - 0.5;
            double r;

            if (Math.Abs(x) < 0.42)
            {
                r = x * x;
                r = x * (((a3 * r + a2) * r + a1) * r + a0) / ((((b3 * r + b2) * r + b1) * r + b0) * r + 1.0);
                return r;
            }

            // Log-Log approximation
            r = Up;
            r = Math.Log(-Math.Log(r));
            r = c0 + r * (c1 + r * (c2 + r * (c3 + r * (c4 + r * (c5 + r * (c6 + r * (c7 + r * c8)))))));

            r = -r;
            return r;
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
