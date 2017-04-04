
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{

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
            Count = AADTape._tapeCounter;
            //AADTape.AddEntry((int)AADType, Count, 0, 0, Value);
        }

        public ADouble(double MyDouble)
        {
            AADType = (int)Constants.AADType.Const;
            Value = MyDouble;
            Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADType, Count, 0, 0, Value);
        }

        public static implicit operator ADouble(double Value)
        {
            // In C# the assignment operator cannot be overloaded
            // as classes are by reference. This effectively does it.

            ADouble Temp = new MasterThesis.ADouble(Value);
            AADTape.AddEntry((int)Constants.AADType.Asg, AADTape._tapeCounter, 0, 0, Value);
            return Temp;
        }

        public void Assign()
        {
            Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Const, AADTape._tapeCounter, 0, 0, Value);
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
            Temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Mul, x1.Count, x2.Count, 0, Temp.Value);
            return Temp;
        }

        public static ADouble operator *(double K, ADouble x)
        {
            ADouble temp = new MasterThesis.ADouble();
            temp.Value = x.Value * K;
            temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsMul, x.Count, 0, 0, temp.Value, K);
            return temp;
        }

        public static ADouble operator *(ADouble x, double K)
        {
            ADouble temp = new MasterThesis.ADouble();
            temp.Value = x.Value * K;
            temp.Count = AADTape._tapeCounter;
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
            Temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Add, x1.Count, x2.Count, 0, Temp.Value);
            return Temp;
        }

        public static ADouble operator +(ADouble x, double K)
        {
            ADouble Temp = new ADouble();
            Temp.Value = x.Value + K;
            Temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsAdd, x.Count, 0, 0, Temp.Value, K);
            return Temp;
        }

        public static ADouble operator +(double K, ADouble x)
        {
            ADouble Temp = new ADouble();
            Temp.Value = x.Value + K;
            Temp.Count = AADTape._tapeCounter;
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
            Temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Sub, x1.Count, x2.Count, 0, Temp.Value);
            return Temp;
        }

        public static ADouble operator -(ADouble x, double K)
        {
            ADouble Temp = new ADouble();
            Temp.Value = x.Value - K;
            Temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsSub, x.Count, 0, 0, Temp.Value, K);
            return Temp;
        }

        public static ADouble operator -(double K, ADouble x)
        {
            ADouble Temp = new ADouble();
            Temp.Value = K - x.Value;
            Temp.Count = AADTape._tapeCounter;
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
            Temp.Count = AADTape._tapeCounter;
            //AADTape.AddEntry((int)Constants.AADType.Pow, x1.Count, 0, 0, Temp.Value, -1);
            AADTape.AddEntry((int)Constants.AADType.Div, x1.Count, x2.Count, 0, Temp.Value);
            return Temp;
        }

        public static ADouble operator /(double K, ADouble x)
        {
            ADouble Temp = new MasterThesis.ADouble();
            Temp.Value = K / x.Value;
            Temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsDiv, x.Count, 0, 0, Temp.Value, K);
            return Temp;
        }

        // Note ConsMul here
        public static ADouble operator /(ADouble x, double K)
        {
            ADouble Temp = new MasterThesis.ADouble();
            Temp.Value = x.Value / K;
            Temp.Count = AADTape._tapeCounter;
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
            Temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Exp, x1.Count, 0, 0, Temp.Value);
            return Temp;
        }

        public static ADouble Log(ADouble x1)
        {
            ADouble Temp = new ADouble();
            Temp.Value = Math.Log(x1.Value);
            Temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Log, x1.Count, 0, 0, Temp.Value);
            return Temp;
        }

        public static ADouble Pow(ADouble x1, double K)
        {
            ADouble Temp = new ADouble();
            Temp.Value = Math.Pow(x1.Value, K);
            Temp.Count = AADTape._tapeCounter;
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
