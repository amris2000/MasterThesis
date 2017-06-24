using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
    /* --- General information
     * The contents of this file is not used within 
     * the masters thesis.
     */

    public interface INonLinearRateModel
    {
        void IncrementByTimeStep();
        void ImpliedVolatility();
        void SimulatePath();
        void IncrementUnderlying();
        void ValueEuropeanPayoff();
    }

    public class BlackScholes : INonLinearRateModel
    {
        public double vol;
        public double rate;
        public double t;
        public double mat;
        public double spot;

        public void IncrementByTimeStep() { }
        public void ImpliedVolatility() { }
        public void SimulatePath() { }
        public void ValueEuropeanPayoff() { }
        public void IncrementUnderlying() { }
    }

    public class Heston
    {
        public double Spot;
        public double Strike;
        public double Mat;
        public double Lambda;
        public double Eps;
        public double MeanRv;
        public double Rho;

        public Heston(double spot, double mat, double lambda, double eps, double meanRv, double rho)
        {
            this.Spot = spot;
            this.Mat = mat;
            this.Lambda = lambda;
            this.Eps = eps;
            this.MeanRv = meanRv;
            this.Rho = rho;
        }

        private double CondM(double vt, double k, double dt)
        {
            return (vt - 1) * Math.Exp(-k * dt) + 1;
        }

        private double CondV(double vt, double k, double dt)
        {
            return vt*Eps*Eps / k * (Math.Exp(-k*dt) - Math.Exp(-2.0*k*dt)) + Eps * Eps / (2.0 * k) * (1 - Math.Exp(-k * dt)) * (1 - Math.Exp(-k * dt)); 
        }

        private double LocalLnY(double vt, double k, double dt)
        {
            return Math.Sqrt(Math.Log(CondV(vt, k, dt) / (Math.Pow(CondM(vt, k, dt), 2.0)) + 1));
        }
    }

    public static class ClosedForm
    {
        public static double BsCallPrice(double spot, double vol, double mat, double strike, double rate)
        {
            double std = Math.Sqrt(mat) * vol;
            double halfVar = (rate + vol * vol * 0.5) * mat;
            double d1 = (Math.Log(spot / strike) + halfVar) / std;
            double d2 = d1 - std;
            return spot * MyMath.NormalCdf(d1) - strike * MyMath.NormalCdf(d2) * Math.Exp(-rate * mat);
        }

        public static double BachelierCallPrice(double spot, double lambda, double mat, double strike)
        {
            double d = (spot - strike) / (lambda * Math.Sqrt(mat));
            double NormalPdf = 1.0; // calculate this
            return MyMath.NormalCdf(d) * (spot - strike) + lambda * Math.Sqrt(mat) * NormalPdf;
        }
    }

    public static class Sampler
    {
        public static double[] GenerateCorrelatedNormals(Random rand, double corr)
        {
            double A = corr;
            double B = Math.Sqrt(1 - corr * corr);
            double out1 = rand.NextDouble();
            double temp = rand.NextDouble();
            double out2 = A * out1 + B * temp;
            return new double[2] { MyMath.U2G(out1), MyMath.U2G(out2) };
        }
    }

    public class Sabr
    {
        public double Sigma0;
        public double Mat;
        public double Alpha;
        public double Beta;
        public double Rho;
        private Random rand;

        public Sabr(double sigma0, double mat, double alpha, double beta, double rho)
        {
            Sigma0 = sigma0;
            Mat = mat;
            Alpha = alpha;
            Beta = beta;
            Rho = rho;
            rand = new Random(1234);
        }

        private double IncrementUnderlying(double value, double vol, double dt, double random)
        {
            return value + Math.Exp(vol) * Math.Pow(value, Beta) * Math.Sqrt(dt) * random;
        }

        private double IncrementLogVol(double logVol, double dt, double random)
        {
            // The logarithm of v
            return logVol - 0.5*Alpha*Alpha*dt + Alpha*Math.Sqrt(dt) * random;
        }

        public double GeneratePath(double initialSpot, double mat, double timeSteps)
        {
            double dt = mat / timeSteps;
            double spot = initialSpot;
            double logVol = Math.Log(Sigma0);

            for (int i = 0; i<timeSteps; i++)
            {
                double[] randoms = Sampler.GenerateCorrelatedNormals(rand, Rho);
                spot = IncrementUnderlying(spot, logVol, dt, randoms[0]);
                logVol = IncrementLogVol(logVol, dt, randoms[1]);
            }
            return spot;
        }

        public double CallPrice(double spot, double strike, double mat, double rate, int paths, int timeSteps)
        {
            double sum = 0.0;
            double spotT;
            for (int i = 0; i<paths; i++)
            {
                spotT = GeneratePath(spot, mat, timeSteps);
                sum += Math.Max(spotT - strike, 0);
            }
            return Math.Exp(-rate*mat)* sum / paths;
        }

        public double ImpliedVolatility(double spot, double strike, double mat, double rate, double price)
        {
            double tol = 0.0000001;
            int n = 0;
            int nMax = 100;
            double volQuess = Sigma0;
            double b = 1.0;
            double a = 0.0001;
            double c = 0.0;

            double fc = 0.0;
            double fa = 0.0;

            double signFc, signFa;

            while (n<nMax)
            {
                n = n + 1;
                c = (a + b) * 0.5;
                fc = ClosedForm.BsCallPrice(spot, c, mat, strike, rate) - price;
                fa = ClosedForm.BsCallPrice(spot, a, mat, strike, rate) - price;

                if ((b - a) * 0.5 < tol)
                {
                    return c;
                }

                signFc = -1;
                signFa = -1;

                if (fc > 0)
                    signFc = 1; 
                if (fa > 0)
                    signFa = 1; 

                if (signFa == signFc)
                    a = c;
                else
                    b = c;
            }

            throw new InvalidOperationException("Bi-section algorithm did not converge in " + nMax + " iterations.");
        }
    }

    public class Freia
    {
        private double _lambda;
        private double _level;
        private double _rho;
        private double _s0;
        private double _z0;
        private double _epsilon;
        private double _alpha;
        private double _backbone;
        private double _mix;
        private double _beta;

        public Freia(double lambda, double level, double rho, double s0, double z0, double epsilon,
                        double alpha, double backbone, double mix, double beta)
        {
            // Still missing shift parameter

            _lambda = lambda;
            _level = level;
            _rho = rho;
            _s0 = s0;
            _z0 = z0; // Usually = 1
            _epsilon = epsilon;
            _alpha = alpha; // Usually = 1
            _backbone = backbone;
            _mix = mix; // CEV parameter
            _beta = beta;
        }

        public double ImpliedVol(double impliedVol, double strike)
        {
            double testVal = 0.0;
            return testVal;
        }

        private double incrementSt(double st, double zt, double timeStep, Random random)
        {
            double w = random.NextDouble();
            double help1 = Math.Sqrt(zt) * _lambda * Math.Pow((_s0 / _level), _backbone - 1);
            double help2 = _mix * st + (1 - _mix) * _s0;
            double ds = help1 * help2 * Math.Sqrt(timeStep) * w;
            return st + ds;
        }

        private double incrementPt(double zt, double timeStep, Random random)
        {
            double w = random.NextDouble();
            double dz = _beta * (_alpha - zt) * timeStep + _epsilon * Math.Sqrt(zt) * Math.Sqrt(timeStep) * w;
            return zt + dz;
        }

        private double simulatePath(double maturity, int timeSteps, Random random)
        {
            double timeStep = maturity / timeSteps;
            double valueSpot = _s0;
            double valueZ = _z0;

            for (int i = 0; i < timeSteps; i++)
            {
                valueZ = incrementPt(valueZ, timeStep, random);
                valueSpot = incrementSt(valueSpot, valueZ, timeStep, random);
            }
            return valueSpot;
        }

        public double callValue(double maturity, double strike, int paths, int timeSteps)
        {
            double[] values = new double[paths];
            Random random = new Random(1234);

            for (int i = 0; i < paths; i++)
                values[i] = Math.Max(simulatePath(maturity, timeSteps, random) - strike, 0);

            return values.Average();
        }
    }
}
