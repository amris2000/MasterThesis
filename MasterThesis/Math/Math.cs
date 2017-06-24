using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
    public struct MyMathConstants
    {
        public static double SqrtTwoPi = 2.506628274631;
        public static double Pi = 3.14159265359;
    }

    public static class MyMath
    {
        #region Interpolation
        // Main interpolation method. All other methods calls this
        public static double Interpolate(List<double> xArr, double x, List<double> yArr, InterpMethod Method)
        {
            if (xArr.Count() != yArr.Count())
                throw new ArgumentException("Number of dates has to correspond to number of values");

            int n = xArr.Count();

            // Extrapolation (flat)
            if (xArr[0] > x)
                return yArr[0];
            else if (xArr[n - 1] <= x)
                return yArr[n - 1];
            else
            {
                // No extrapolation - find relevant index in arrays
                int i = 0;
                int j = 0;

                for (i = 0; i < n; i++)
                {
                    if (xArr[i] <= x)
                        j = j + 1;
                    else
                        break;
                }

                j = j - 1;

                switch (Method)
                {
                    case InterpMethod.Constant:

                        return yArr[j];

                    case InterpMethod.Linear:

                        double tempLinear = (x - xArr[j]) * yArr[j + 1] + (xArr[j + 1] - x) * yArr[j];
                        return tempLinear / (xArr[j + 1] - xArr[j]);

                    case InterpMethod.LogLinear:

                        // Log-Linear interpolation is only valid for positive stuff
                        if (yArr[j + 1] < 0 || yArr[j] < 0)
                            return yArr[j];
                        else
                        {
                            double tempLogLinear1 = (x - xArr[j]) / (xArr[j + 1] - xArr[j]);
                            double tempLogLinear2 = (xArr[j + 1] - x) / (xArr[j + 1] - xArr[j]);
                            return Math.Pow(yArr[j + 1], tempLogLinear1) * Math.Pow(yArr[j], tempLogLinear2);
                        }

                    case InterpMethod.Hermite:

                        double bi, bk, hi, mi, ci, di;
                        int k = j + 1;
                        if (j == 0)
                        {
                            bi = (xArr[2] + xArr[1] - 2 * xArr[0]) * (yArr[1] - yArr[0]) / (xArr[1] - xArr[0]);
                            bi += -1 * (xArr[1] - xArr[0]) * (yArr[2] - yArr[1]) / (xArr[2] - xArr[1]);
                            bi *= Math.Pow(xArr[2] - xArr[0], -1.0);

                            bk = (xArr[k + 1] - xArr[k]) * (yArr[k] - yArr[k - 1]) / (xArr[k] - xArr[k - 1]);
                            bk += (xArr[k] - xArr[k - 1]) * (yArr[k + 1] - yArr[k]) / (xArr[k + 1] - xArr[k]);
                            bk *= Math.Pow(xArr[k + 1] - xArr[k - 1], -1);

                        }
                        else if (j == n - 2)
                        {
                            bi = (xArr[j + 1] - xArr[j]) * (yArr[j] - yArr[k - 1]) / (xArr[j] - xArr[j - 1]);
                            bi += (xArr[j] - xArr[j - 1]) * (yArr[j + 1] - yArr[j]) / (xArr[j + 1] - xArr[j]);
                            bi *= Math.Pow(xArr[j + 1] - xArr[j - 1], -1.0);

                            bk = -1 * (xArr[n - 1] - xArr[n - 2]) * (yArr[n - 2] - yArr[n - 3]) / (xArr[n - 2] - xArr[n - 3]);
                            bk += -1 * (xArr[n - 1] - xArr[n - 2] - (xArr[n - 1] - xArr[n - 3])) * (yArr[n - 1] - yArr[n - 2]) / (xArr[n - 1] - xArr[n - 2]);
                            bk *= Math.Pow(xArr[n - 1] - xArr[n - 3], -1.0);
                        }
                        else
                        {
                            bi = (xArr[j + 1] - xArr[j]) * (yArr[j] - yArr[j - 1]) / (xArr[j] - xArr[j - 1]);
                            bi += (xArr[j] - xArr[j - 1]) * (yArr[j + 1] - yArr[j]) / (xArr[j + 1] - xArr[j]);
                            bi *= Math.Pow(xArr[j + 1] - xArr[j - 1], -1.0);

                            bk = (xArr[k + 1] - xArr[k]) * (yArr[k] - yArr[k - 1]) / (xArr[k] - xArr[k - 1]);
                            bk += (xArr[k] - xArr[k - 1]) * (yArr[k + 1] - yArr[k]) / (xArr[k + 1] - xArr[k]);
                            bk *= Math.Pow(xArr[k + 1] - xArr[k - 1], -1.0);
                        }

                        hi = xArr[j + 1] - xArr[j];
                        mi = (yArr[j + 1] - yArr[j]) / hi;
                        ci = (3.0 * mi - bk - 2.0 * bi) / hi;
                        di = (bk + bi - 2.0 * mi) * Math.Pow(hi, -2.0);

                        return yArr[j] + bi * (x - xArr[j]) + ci * Math.Pow(x - xArr[j], 2.0) + di * Math.Pow(x - xArr[j], 3.0);
                    default:
                        throw new InvalidOperationException("Interpolation method is not valid");
                }
            }
        }

        public static double Interpolate(double[] xArr, double x, double[] yArr, InterpMethod method)
        {
            return Interpolate(xArr.ToList<double>(), x, yArr.ToList<double>(), method);
        }

        public static ADouble InterpolateCurve(List<DateTime> xDates, DateTime date, List<ADouble> yArr, InterpMethod method)
        {
            double[] xArr = xDates.Select(i => i.ToOADate()).ToArray();
            double x = date.ToOADate();
            return InterpolateCurve(xArr, x, yArr, method);
        }

        public static ADouble InterpolateCurve(double[] xArr, double x, List<ADouble> yArr, InterpMethod method)
        {
            // To-do: CATROM interpolation
            if (xArr.Count() != yArr.Count())
                throw new ArgumentException("Number of dates has to correspond to number of values");

            int n = xArr.Count();

            // Extrapolation (flat)
            if (xArr[0] > x)
                return yArr[0];
            else if (xArr[n - 1] <= x)
                return yArr[n - 1];
            else
            {
                // No extrapolation - find relevant index in arrays
                int i = 0;
                int j = 0;

                for (i = 0; i < n; i++)
                {
                    if (xArr[i] <= x)
                        j = j + 1;
                    else
                        break;
                }

                j = j - 1;

                switch (method)
                {
                    case InterpMethod.Constant:

                        return yArr[j];

                    case InterpMethod.Linear:

                        ADouble tempLinear = (x - xArr[j]) * yArr[j + 1] + (xArr[j + 1] - x) * yArr[j];
                        return tempLinear / (xArr[j + 1] - xArr[j]);

                    case InterpMethod.LogLinear:

                        // Log-Linear interpolation is only valid for positive stuff
                        if (yArr[j + 1] < 0.0 || yArr[j] < 0.0)
                            return yArr[j];
                        else
                        {
                            ADouble tempLogLinear1 = (x - xArr[j]) / (xArr[j + 1] - xArr[j]);
                            ADouble tempLogLinear2 = (xArr[j + 1] - x) / (xArr[j + 1] - xArr[j]);
                            return ADouble.Pow(yArr[j + 1], tempLogLinear1) * ADouble.Pow(yArr[j], tempLogLinear2);
                        }

                    case InterpMethod.Hermite:

                        ADouble bi, bk, hi, mi, ci, di;
                        int k = j + 1;
                        if (j == 0)
                        {
                            bi = (xArr[2] + xArr[1] - 2 * xArr[0]) * (yArr[1] - yArr[0]) / (xArr[1] - xArr[0]);
                            bi += -1 * (xArr[1] - xArr[0]) * (yArr[2] - yArr[1]) / (xArr[2] - xArr[1]);
                            bi *= ADouble.Pow(xArr[2] - xArr[0], -1.0);

                            bk = (xArr[k + 1] - xArr[k]) * (yArr[k] - yArr[k - 1]) / (xArr[k] - xArr[k - 1]);
                            bk += (xArr[k] - xArr[k - 1]) * (yArr[k + 1] - yArr[k]) / (xArr[k + 1] - xArr[k]);
                            bk *= ADouble.Pow(xArr[k + 1] - xArr[k - 1], -1.0);

                        }
                        else if (j == n - 2)
                        {
                            bi = (xArr[j + 1] - xArr[j]) * (yArr[j] - yArr[k - 1]) / (xArr[j] - xArr[j - 1]);
                            bi += (xArr[j] - xArr[j - 1]) * (yArr[j + 1] - yArr[j]) / (xArr[j + 1] - xArr[j]);
                            bi *= ADouble.Pow(xArr[j + 1] - xArr[j - 1], -1.0);

                            bk = -1 * (xArr[n - 1] - xArr[n - 2]) * (yArr[n - 2] - yArr[n - 3]) / (xArr[n - 2] - xArr[n - 3]);
                            bk += -1 * (xArr[n - 1] - xArr[n - 2] - (xArr[n - 1] - xArr[n - 3])) * (yArr[n - 1] - yArr[n - 2]) / (xArr[n - 1] - xArr[n - 2]);
                            bk *= ADouble.Pow(xArr[n - 1] - xArr[n - 3], -1.0);
                        }
                        else
                        {
                            bi = (xArr[j + 1] - xArr[j]) * (yArr[j] - yArr[j - 1]) / (xArr[j] - xArr[j - 1]);
                            bi += (xArr[j] - xArr[j - 1]) * (yArr[j + 1] - yArr[j]) / (xArr[j + 1] - xArr[j]);
                            bi *= ADouble.Pow(xArr[j + 1] - xArr[j - 1], -1.0);

                            bk = (xArr[k + 1] - xArr[k]) * (yArr[k] - yArr[k - 1]) / (xArr[k] - xArr[k - 1]);
                            bk += (xArr[k] - xArr[k - 1]) * (yArr[k + 1] - yArr[k]) / (xArr[k + 1] - xArr[k]);
                            bk *= ADouble.Pow(xArr[k + 1] - xArr[k - 1], -1.0);
                        }

                        hi = xArr[j + 1] - xArr[j];
                        mi = (yArr[j + 1] - yArr[j]) / hi;
                        ci = (3.0 * mi - bk - 2.0 * bi) / hi;
                        di = (bk + bi - 2.0 * mi) * ADouble.Pow(hi, -2.0);

                        return yArr[j] + bi * (x - xArr[j]) + ci * ADouble.Pow(x - xArr[j], 2.0) + di * ADouble.Pow(x - xArr[j], 3.0);
                    default:
                        throw new InvalidOperationException("Interpolation method is not valid");
                }
            }
        }

        public static double InterpolateCurve(List<DateTime> dates, DateTime inputDate, List<double> values, InterpMethod method)
        {
            double[] xArr = dates.Select(i=>i.ToOADate()).ToArray();
            double x = inputDate.ToOADate();
            return Interpolate(xArr, x, values.ToArray(), method);
        }

        public static double InterpolateCurve(Curve curve, DateTime inputDate, InterpMethod method)
        {
            return InterpolateCurve(curve.Dates, inputDate, curve.Values, method);
        }
        #endregion

        #region Only used for non linear stuff. Not used in thesis.
        public static double NormalCdf(double x)
        {
            return NormalCdf(x);
        }

        public static ADouble NormalCdf(ADouble x) 
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
            ADouble t = 1.0 / (1.0 + p * x);
            ADouble pol = t * (b1 + t * (b2 + t * (b3 + t * (b4 + t * b5))));
            ADouble pdf = 0.0;

            if (x < -10.0 || 10.0 < x)
                pdf = 0.0;
            else
                pdf = ADouble.Exp(-0.5 * x * x) / MyMathConstants.SqrtTwoPi;

            return 1.0 - pdf * pol;
        }

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
        #endregion

    }
}
