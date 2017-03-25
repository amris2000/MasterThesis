
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{

    public static class Constants
    {
        public static uint TAPE_SIZE = 10000;
        public enum AADType {   Undef = -1, Const = 0, Asg = 1, Add = 2, Sub = 3, Mul = 4,
                                Div = 5, Exp = 6, Log = 7, Pow = 8,
                                ConsAdd = 13, ConsSub = 14, ConsDiv = 12, ConsMul = 11};

        public static string GetTypeName(int n)
        {
            switch (n)
            {
                case -1:
                    return "UNDEF";
                case 0:
                    return "CONST";
                case 1:
                    return "ASSGN";
                case 2:
                    return "ADD";
                case 3:
                    return "SUBTR";
                case 4:
                    return "MULTP";
                case 5:
                    return "DIVIS";
                case 6:
                    return "EXPON";
                case 7:
                    return "LOGRM";
                case 8:
                    return "POWER";
                case 11:
                    return "CNMUL";
                case 13:
                    return "CNADD";
                case 14:
                    return "CNSUB";
                case 12:
                    return "CNDIV";
                default:
                    return "UNDEF";
            }
        }
    }

    public static class AADFunc
    {
        public static void BlackScholes(ADouble Vol, ADouble Spot, ADouble Rate, ADouble Time, ADouble Mat, ADouble Strike)
        {
            ADouble Help1 = Vol * ADouble.Sqrt(Mat-Time);
            ADouble d1 = 1 / Help1 * (ADouble.Log(Spot / Strike) + (Rate + 0.5 * ADouble.Pow(Vol, 2)) * (Mat-Time));
            ADouble d2 = d1 - Vol * ADouble.Sqrt(Mat-Time);
            ADouble Out = Maths.NormalCdf(d1) * Spot - Strike * ADouble.Exp(-Rate * (Mat-Time)) * Maths.NormalCdf(d2);

            Console.WriteLine("");
            Console.WriteLine("BLACK-SCHOLES TEST. Value: " + Out.Value);
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }


        public static void Func1(ADouble x, ADouble y, ADouble z)
        {
            // Derivative
            //      Fx(x,y,z) = y*z + 1 + y
            //      Fy(x,y,z) = x*z - 1 + x
            //      Fz(x,y,z) = x*y

            ADouble Out = x * y * z + x - y + x * y;
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        public static void FuncLog(ADouble x)
        {
            // Derivative: Fx(x) = 3 + 3/x

            ADouble Temp = 3.0 * x + 3.0 * ADouble.Log(x * 4.0) + 50.0;
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        public static void FuncExp(ADouble x)
        {
            // Derivative: Fx(x) = 3 + 3*exp(3*x)

            ADouble Temp = 3 * x + ADouble.Exp(3 * x) + 50;
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        public static void Func11(ADouble x, ADouble y, ADouble z)
        {
            ADouble Out1, Out2, Out3;
            Out1 = x * y * z;
            Out2 = x * y;
            Out3 = Out1 + x - y + Out2;
            Out3 = Out3 * 3;
            Out3 = 2* Out3;
            Out3 = 10.0 + Out3;
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        public static void FuncDiv(ADouble x)
        {
            ADouble Out = x * x * x / (3 + 2 * x);
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        public static void FuncDiv2(ADouble x)
        {
            ADouble Out = 1 / x;
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        public static void FuncDiv3(ADouble x1, ADouble x2, double K)
        {
            ADouble Out = x1 / x2 + K / x2 + K / x1 + x1 / K + x2 / K;
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }

        public static void FuncPow(ADouble x, double k)
        {
            ADouble Out = 3 * ADouble.Log(x) + 5 * ADouble.Pow(x, k);
            Console.WriteLine(" ");
            Console.WriteLine("Testing Adjoint differentiation of a function involving powers");
            AADTape.InterpretTape();
            AADTape.PrintTape();
            AADTape.ResetTape();
        }
    }

    public static class AADTape
    {
        public static int TapeCounter = 0;
        public static int[] Arg1 = new int[Constants.TAPE_SIZE];
        public static int[] Arg2 = new int[Constants.TAPE_SIZE];
        public static int[] Oc = new int[Constants.TAPE_SIZE];
        public static double[] Value = new double[Constants.TAPE_SIZE];
        public static double[] Adjoint = new double[Constants.TAPE_SIZE];
        public static double[] Consts = new double[Constants.TAPE_SIZE];

        public static void ResetTape()
        {
            for (int i = TapeCounter - 1; i >= 0; i--)
            {
                Adjoint[i] = 0;
            }
            TapeCounter = 0;
        }

        public static void IncrTape()
        {
            TapeCounter = TapeCounter + 1;
        }

        public static void AddEntry(int OcV, int Arg1V, int Arg2V, double A, double V, double? K = null)
        {
            Arg1[TapeCounter] = Arg1V;
            Arg2[TapeCounter] = Arg2V;
            Oc[TapeCounter] = OcV;
            Value[TapeCounter] = V;
            if (K.HasValue)
                Consts[TapeCounter] = (double) K;
            IncrTape();
        }

        public static void InterpretTape()
        {
            // Once the function has been calculated
            // and the tape recorded, run this function
            // to propagate the adjoints backwards.
            // the switch implements how the adjoint is treated
            // for each of the operators

            Adjoint[TapeCounter - 1] = 1;
            for (int i = TapeCounter - 1; i >= 1; i--)
            {
                switch (Oc[i])
                {
                    case 1: // Assign
                        Adjoint[Arg1[i]] += Adjoint[i];
                        break;

                    // -- ELEMENTARY OPERATIONS
                    case 2: // Add
                        Adjoint[Arg1[i]] += Adjoint[i];
                        Adjoint[Arg2[i]] += Adjoint[i];
                        break;
                    case 3: // Subtract
                        Adjoint[Arg1[i]] += Adjoint[i];
                        Adjoint[Arg2[i]] -= Adjoint[i];
                        break;
                    case 4: // Multiply
                        Adjoint[Arg1[i]] += Value[Arg2[i]] * Adjoint[i];
                        Adjoint[Arg2[i]] += Value[Arg1[i]] * Adjoint[i];
                        break;

                    case 5: // Division (Check that this is in fact correct...)
                        Adjoint[Arg1[i]] += Adjoint[i] / Value[Arg2[i]]; 
                        Adjoint[Arg2[i]] -= Adjoint[i] * Value[Arg1[i]]/ (Math.Pow(Value[Arg2[i]], 2));
                        break;

                    // -- UNARY OPERATORS
                    case 6: // Exponentiate
                        Adjoint[Arg1[i]] += Adjoint[i] * Value[i]; // Value[i] = Exp(x). Could also say Math.Exp(Value[Arg1[i]])
                        break;
                    case 7: // Natural Logarithm
                        Adjoint[Arg1[i]] += Adjoint[i] / Value[Arg1[i]];
                        break;
                    case 8: 
                        Adjoint[Arg1[i]] += Adjoint[i] * Consts[i] * Math.Pow(Value[Arg1[i]], Consts[i] - 1);
                        break;

                    // -- CONSTANT OPERATORS
                    case 11: // Const multiply
                        Adjoint[Arg1[i]] += Adjoint[i]*Consts[i];
                        break;
                    case 12: // Const Divide
                        Adjoint[Arg1[i]] -= Adjoint[i] * Consts[i] / (Math.Pow(Value[Arg1[i]], 2));
                        break;
                    case 13: // Const add - Should perhabs do nothing here ... For efficiency (or not.. seems to give 0)
                        Adjoint[Arg1[i]] += Adjoint[i];
                        break;
                    case 14: // Const sub
                        Adjoint[Arg1[i]] -= Adjoint[i];
                        break;

                    default:
                        break;
                }
            }
        }

        public static void PrintTape()
        {
            Console.WriteLine("");
            Console.WriteLine("----------- Printing AAD tape");
            List<string[]> Out = new List<string[]>();

            Out.Add(new string[] { "    ", "#", "OPER", "ARG1", "ARG2", "VAL", "ADJ", "CONS" });
            for (int i = 0; i < TapeCounter; i++)
            {

                Out.Add(new string[] {
                    "     ",
                    i.ToString(),
                    Constants.GetTypeName(AADTape.Oc[i]),
                    AADTape.Arg1[i].ToString(),
                    AADTape.Arg2[i].ToString(),
                    Math.Round(AADTape.Value[i], 3).ToString(),
                    Math.Round(AADTape.Adjoint[i], 3).ToString(),
                    Math.Round(AADTape.Consts[i],3).ToString()
                    });
            }

            var Output = PrintUtility.PrintListNicely(Out, 5);
            Console.WriteLine(Output);
        }
    }

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public class ADouble
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
    {
        public int AADType;
        public double Value;
        public int Count;

        public ADouble()
        {
            AADType = (int)Constants.AADType.Undef;
            Value = 0;
            //Count = AADTape.TapeCounter;
            //AADTape.AddEntry((int)AADType, Count, 0, 0, Value);
        }

        public ADouble(double MyDouble)
        {
            AADType = (int)Constants.AADType.Const;
            Value = MyDouble;
            Count = AADTape.TapeCounter;
            AADTape.AddEntry((int)AADType, Count, 0, 0, Value);
        }

        public static implicit operator ADouble(double Value)
        {
            // In C# the assignment operator cannot be overloaded
            // as classes are by reference. This effectively does it.

            ADouble Temp = new MasterThesis.ADouble(Value);
            AADTape.AddEntry((int)Constants.AADType.Asg, AADTape.TapeCounter, 0, 0, Value);
            return Temp;
        }

        public static bool operator ==(ADouble x, ADouble y)
        {

            if (System.Object.ReferenceEquals(x, y))
                return true;

            if (((object)x == null || ((object)y == null)))
                return false;

            return x.Value == y.Value && (int)x.AADType == (int)y.AADType;
        }

        public static Boolean operator !=(ADouble x, ADouble y)
        {
            if (System.Object.ReferenceEquals(x, y))
                return false;

            if (((object)x == null || ((object)y == null)))
                return true;

            return x.Value != y.Value && (int)x.AADType != (int)y.AADType;
        }

        //
        // Multiplicative operator overloading
        //
        public static ADouble operator *(ADouble x1, ADouble x2)
        {
            ADouble Temp = new ADouble();
            Temp.Value = x1.Value * x2.Value;
            Temp.Count = AADTape.TapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Mul, x1.Count, x2.Count, 0, Temp.Value);
            return Temp;
        }

        public static ADouble operator *(double K, ADouble x)
        {
            ADouble temp = new MasterThesis.ADouble();
            temp.Value = x.Value * K;
            temp.Count = AADTape.TapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsMul, x.Count, 0, 0, temp.Value, K);
            return temp;
        }

        public static ADouble operator *(ADouble x, double K)
        {
            ADouble temp = new MasterThesis.ADouble();
            temp.Value = x.Value * K;
            temp.Count = AADTape.TapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsMul, x.Count, 0, 0, temp.Value, K);
            return temp;
        }

        //
        // Addition operator overloading
        //
        public static ADouble operator +(ADouble x1, ADouble x2)
        {
            ADouble Temp = new ADouble();
            Temp.Value = x1.Value + x2.Value;
            Temp.Count = AADTape.TapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Add, x1.Count, x2.Count, 0, Temp.Value);
            return Temp;
        }

        public static ADouble operator +(ADouble x, double K)
        {
            ADouble Temp = new ADouble();
            Temp.Value = x.Value + K;
            Temp.Count = AADTape.TapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsAdd, x.Count, 0, 0, Temp.Value, K);
            return Temp;
        }

        public static ADouble operator +(double K, ADouble x)
        {
            ADouble Temp = new ADouble();
            Temp.Value = x.Value + K;
            Temp.Count = AADTape.TapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsAdd, x.Count, 0, 0, Temp.Value, K);
            return Temp;
        }


        //
        // Subtraction operator overloading
        //
        public static ADouble operator -(ADouble x1, ADouble x2)
        {
            ADouble Temp = new ADouble();
            Temp.Value = x1.Value - x2.Value;
            Temp.Count = AADTape.TapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Sub, x1.Count, x2.Count, 0, Temp.Value);
            return Temp;
        }

        public static ADouble operator -(ADouble x, double K)
        {
            ADouble Temp = new ADouble();
            Temp.Value = x.Value - K;
            Temp.Count = AADTape.TapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsSub, x.Count, 0, 0, Temp.Value, K);
            return Temp;
        }

        public static ADouble operator -(double K, ADouble x)
        {
            ADouble Temp = new ADouble();
            Temp.Value = K - x.Value;
            Temp.Count = AADTape.TapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsSub, x.Count, 0, 0, Temp.Value, K);
            return Temp;
        }

        //
        // Division operator overloading
        //
        public static ADouble operator /(ADouble x1, ADouble x2)
        {
            ADouble Temp = new ADouble();
            Temp.Value = x1.Value / x2.Value;
            Temp.Count = AADTape.TapeCounter;
            //AADTape.AddEntry((int)Constants.AADType.Pow, x1.Count, 0, 0, Temp.Value, -1);
            AADTape.AddEntry((int)Constants.AADType.Div, x1.Count, x2.Count, 0, Temp.Value);
            return Temp;
        }

        public static ADouble operator /(double K, ADouble x)
        {
            ADouble Temp = new MasterThesis.ADouble();
            Temp.Value = K / x.Value;
            Temp.Count = AADTape.TapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsDiv, x.Count, 0, 0, Temp.Value, K);
            return Temp;
        }

        // Note ConsMul here
        public static ADouble operator /(ADouble x, double K)
        {
            ADouble Temp = new MasterThesis.ADouble();
            Temp.Value = x.Value / K;
            Temp.Count = AADTape.TapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsMul, x.Count, 0, 0, Temp.Value, 1/K);
            return Temp;
        }

        ////////////////////
        // UNARY OPERATORS
        ////////////////////


        public static ADouble operator -(ADouble x1)
        {
            // Allows "x = -y" instead of "x = -1.0*y"
            return -1.0 * x1;
        }

        public static ADouble Exp(ADouble x1)
        {
            ADouble Temp = new ADouble();
            Temp.Value = Math.Exp(x1.Value);
            Temp.Count = AADTape.TapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Exp, x1.Count, 0, 0, Temp.Value);
            return Temp;
        }

        public static ADouble Log(ADouble x1)
        {
            ADouble Temp = new ADouble();
            Temp.Value = Math.Log(x1.Value);
            Temp.Count = AADTape.TapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Log, x1.Count, 0, 0, Temp.Value);
            return Temp;
        }

        public static ADouble Pow(ADouble x1, double K)
        {
            ADouble Temp = new ADouble();
            Temp.Value = Math.Pow(x1.Value, K);
            Temp.Count = AADTape.TapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Pow, x1.Count, 0, 0, Temp.Value, K);
            return Temp;
        }

        public static ADouble Sqrt(ADouble x1)
        {
            return Pow(x1,0.5);
        }

        //
        // Comparison operators
        //
        public static bool operator <(ADouble x1, ADouble x2)
        {
            if (x1.Value < x2.Value)
                return true;
            else
                return false;
        }

        public static bool operator >(ADouble x1, ADouble x2)
        {
            if (x1.Value > x2.Value)
                return true;
            else
                return false;
        }

        public static bool operator <(ADouble x1, double k)
        {
            if (x1.Value < k)
                return true;
            else
                return false;
        }

        public static bool operator >(ADouble x1, double k)
        {
            if (x1.Value > k)
                return true;
            else
                return false;
        }

        public static bool operator <(double k, ADouble x1)
        {
            if (x1.Value > k)
                return true;
            else
                return false;
        }

        public static bool operator >(double k, ADouble x1)
        {
            if (x1.Value < k)
                return true;
            else
                return false;
        }


    }

    public class AAD
    {

    }
}
