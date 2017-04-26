
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{

    public class ADouble : IComparable, IComparable<Double>, IEquatable<Double>
    {
        // To-do: 
        //  problem when doing arithmetic with ints, i.e. x*1 instead of x*1.0
        //  What happens when we say x = y, where x,y is ADoubles? Need tapeEntry?
        //  or is this handled since ADouble is a reference type?

        public int AADType;
        public double Value;
        public int Count;

        // Not sure when this is used, but it is used (as can be seen by the tape).
        // Seems like some hidden variables are being created. Need more investigation
        public ADouble()
        {
            AADType = (int)Constants.AADType.Undef;
            Value = 0;
            Count = AADTape._tapeCounter;
            //AADTape.AddEntry((int)AADType, Count, 0, 0, Value);
        }

        public ADouble(double doubleValue)
        {
            AADType = (int)Constants.AADType.Const;
            Value = doubleValue;
            Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADType, Count, 0, 0, Value);
        }

        // This allows us to say "ADouble x = double"
        public static implicit operator ADouble(double Value)
        {
            ADouble temp = new MasterThesis.ADouble(Value);
            AADTape.AddEntry((int)Constants.AADType.Asg, AADTape._tapeCounter, 0, 0, Value);
            return temp;
        }

        // Allows us to say i.e. "double x = aDouble"
        public static implicit operator double(ADouble aDouble)
        {
            // Should a tapeEntry be added here?
            return aDouble.Value;
        }

        // Assign ADouble to the tape. This is used when instantiating the tape.
        public void Assign()
        {
            Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Const, AADTape._tapeCounter, 0, 0, Value);
        }

        // ------- INTERFACE IMPLEMENTATIONS

        // for IComparable (sorting)
        public int CompareTo(Object obj)
        {
            ADouble otherAdouble = obj as ADouble;

            if (this.Value > otherAdouble.Value)
                return -1;
            if (this.Value == otherAdouble.Value)
                return 0;

            return 1;
        }

        // for IComparable (sorting with doubles)
        public int CompareTo(double x)
        {
            if (this.Value > x)
                return -1;
            if (this.Value == x)
                return 0;

            return 1;
        }

        // for equatable with doubles.
        public bool Equals(double x)
        {
            return (x == Value);
        }

        // Equalization operators
        public static bool operator ==(ADouble x, ADouble y)
        {

            if (System.Object.ReferenceEquals(x, y))
                return true;

            if (((object)x == null || ((object)y == null)))
                return false;

            return x.Value == y.Value && (int)x.AADType == (int)y.AADType;
        }

        public static bool operator ==(ADouble x, double y)
        {
            if (((object)x == null || ((object)y == null)))
                return false;

            return x.Value == y;
        }

        public static bool operator !=(ADouble x, double y)
        {
            if (((object)x == null || ((object)y == null)))
                return false;

            return x.Value != y;
        }

        public static Boolean operator !=(ADouble x, ADouble y)
        {
            if (System.Object.ReferenceEquals(x, y))
                return false;

            if (((object)x == null || ((object)y == null)))
                return true;

            return x.Value != y.Value && (int)x.AADType != (int)y.AADType;
        }

        // ------- MULTIPLICATIVE OPERATOR OVERLOADING
        public static ADouble operator *(ADouble x1, ADouble x2)
        {
            ADouble temp = new ADouble();
            temp.Value = x1.Value * x2.Value;
            temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Mul, x1.Count, x2.Count, 0, temp.Value);
            return temp;
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

        // ------- ADDITIVE OPERATOR OVERLOADING
        public static ADouble operator +(ADouble x1, ADouble x2)
        {
            ADouble temp = new ADouble();
            temp.Value = x1.Value + x2.Value;
            temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Add, x1.Count, x2.Count, 0, temp.Value);
            return temp;
        }

        public static ADouble operator +(ADouble x, double K)
        {
            ADouble temp = new ADouble();
            temp.Value = x.Value + K;
            temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsAdd, x.Count, 0, 0, temp.Value, K);
            return temp;
        }

        public static ADouble operator +(double K, ADouble x)
        {
            ADouble temp = new ADouble();
            temp.Value = x.Value + K;
            temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsAdd, x.Count, 0, 0, temp.Value, K);
            return temp;
        }


        // ------- SUBTRACTIVE OPERATOR OVERLOADING
        public static ADouble operator -(ADouble x1, ADouble x2)
        {
            ADouble temp = new ADouble();
            temp.Value = x1.Value - x2.Value;
            temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Sub, x1.Count, x2.Count, 0, temp.Value);
            return temp;
        }

        public static ADouble operator -(ADouble x, double K)
        {
            ADouble temp = new ADouble();
            temp.Value = x.Value - K;
            temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsSub, x.Count, 0, 0, temp.Value, K);
            return temp;
        }

        public static ADouble operator -(double K, ADouble x)
        {
            ADouble temp = new ADouble();
            temp.Value = K - x.Value;
            temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsSub, x.Count, 0, 0, temp.Value, K);
            return temp;
        }

        // ------- DIVISION OPERATOR OVERLOADING
        public static ADouble operator /(ADouble x1, ADouble x2)
        {
            ADouble temp = new ADouble();
            temp.Value = x1.Value / x2.Value;
            temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.Div, x1.Count, x2.Count, 0, temp.Value);
            return temp;
        }

        public static ADouble operator /(double K, ADouble x)
        {
            ADouble temp = new MasterThesis.ADouble();
            temp.Value = K / x.Value;
            temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsDiv, x.Count, 0, 0, temp.Value, K);
            return temp;
        }

        // Note ConsMul here
        public static ADouble operator /(ADouble x, double K)
        {
            ADouble temp = new MasterThesis.ADouble();
            temp.Value = x.Value / K;
            temp.Count = AADTape._tapeCounter;
            AADTape.AddEntry((int)Constants.AADType.ConsMul, x.Count, 0, 0, temp.Value, 1/K);
            return temp;
        }

        ////////////////////
        // UNARY OPERATORS
        ////////////////////

        // Allows "x = -y" instead of "x = -1.0*y"
        public static ADouble operator -(ADouble x1)
        {
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

        // ------- COMPARISON OPERATORS
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

        // To make the compiler happy
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        // To make the compiler happy
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
