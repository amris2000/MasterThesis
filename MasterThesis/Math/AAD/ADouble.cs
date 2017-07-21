
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
    /* --- General information
     * This file contains the actual implementation of the
     * ADouble number class. The class itselfs contains a number
     * of operator overloadings that defines what happens, when
     * operations are performed between two ADoubles or an ADouble
     * and a double. This includes storing the operation on the
     * AADTape. Also, methods for handling the exp, log and power
     * operations has been implemented.
     */

    public class ADouble : IComparable, IComparable<Double>, IEquatable<Double>
    {
        public int AADType { get; private set; }
        public double Value { get; private set; }
        public int PlacementInTape { get; private set; }

        #region Constructors and constructor-related.

        // Default constructor. C# sometimes creates intermediary variables. These does not go on the tape.
        public ADouble()
        {
            AADType = (int)AADUtility.AADCalculationType.Undef;
            Value = 0;
            PlacementInTape = AADTape._tapeCounter;
            //AADTape.AddEntry((int)AADType, Count, 0, 0, Value);
        }

        // Initialize ADouble variable from double
        public ADouble(double doubleValue)
        {
            AADType = (int)AADUtility.AADCalculationType.Const;
            Value = doubleValue;
            PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADType, PlacementInTape, 0, 0, Value);
        }

        // This allows us to say "ADouble x = double"
        public static implicit operator ADouble(double Value)
        {
            ADouble temp = new MasterThesis.ADouble(Value);
            AADTape.AddEntry((int)AADUtility.AADCalculationType.Asg, AADTape._tapeCounter, 0, 0, Value);
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
            PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.Const, AADTape._tapeCounter, 0, 0, Value);
        }

        #endregion

        #region Implementation of IComparable dependables for comparison

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

        // Needed for IComparable
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        // Needed for IComparable
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region Equalization operators
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
        #endregion

        #region Comparison operators
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
        #endregion

        #region Operator overloading for multiplication
        public static ADouble operator *(ADouble x1, ADouble x2)
        {
            ADouble temp = new ADouble();
            temp.Value = x1.Value * x2.Value;
            temp.PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.Mul, x1.PlacementInTape, x2.PlacementInTape, 0, temp.Value);
            return temp;
        }

        public static ADouble operator *(double K, ADouble x)
        {
            ADouble temp = new MasterThesis.ADouble();
            temp.Value = x.Value * K;
            temp.PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.ConsMul, x.PlacementInTape, 0, 0, temp.Value, K);
            return temp;
        }

        public static ADouble operator *(ADouble x, double K)
        {
            ADouble temp = new MasterThesis.ADouble();
            temp.Value = x.Value * K;
            temp.PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.ConsMul, x.PlacementInTape, 0, 0, temp.Value, K);
            return temp;
        }
        #endregion

        #region Operator overloading for addition
        public static ADouble operator +(ADouble x1, ADouble x2)
        {
            ADouble temp = new ADouble();
            temp.Value = x1.Value + x2.Value;
            temp.PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.Add, x1.PlacementInTape, x2.PlacementInTape, 0, temp.Value);
            return temp;
        }

        public static ADouble operator +(ADouble x, double K)
        {
            ADouble temp = new ADouble();
            temp.Value = x.Value + K;
            temp.PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.ConsAdd, x.PlacementInTape, 0, 0, temp.Value, K);
            return temp;
        }

        public static ADouble operator +(double K, ADouble x)
        {
            ADouble temp = new ADouble();
            temp.Value = x.Value + K;
            temp.PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.ConsAdd, x.PlacementInTape, 0, 0, temp.Value, K);
            return temp;
        }
        #endregion

        #region Operator overloading for subtraction
        public static ADouble operator -(ADouble x1, ADouble x2)
        {
            ADouble temp = new ADouble();
            temp.Value = x1.Value - x2.Value;
            temp.PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.Sub, x1.PlacementInTape, x2.PlacementInTape, 0, temp.Value);
            return temp;
        }

        public static ADouble operator -(ADouble x, double K)
        {
            ADouble temp = new ADouble();
            temp.Value = x.Value - K;
            temp.PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.ConsSub, x.PlacementInTape, 0, 0, temp.Value, K);
            return temp;
        }

        public static ADouble operator -(double K, ADouble x)
        {
            ADouble temp = new ADouble();
            temp.Value = K - x.Value;
            temp.PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.ConsSubInverse, x.PlacementInTape, 0, 0, temp.Value, K);
            return temp;
        }
        #endregion

        #region Operator overloading for division
        public static ADouble operator /(ADouble x1, ADouble x2)
        {
            ADouble temp = new ADouble();
            temp.Value = x1.Value / x2.Value;
            temp.PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.Div, x1.PlacementInTape, x2.PlacementInTape, 0, temp.Value);
            return temp;
        }

        public static ADouble operator /(double K, ADouble x)
        {
            ADouble temp = new MasterThesis.ADouble();
            temp.Value = K / x.Value;
            temp.PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.ConsDiv, x.PlacementInTape, 0, 0, temp.Value, K);
            return temp;
        }

        public static ADouble operator /(ADouble x, double K)
        {
            // Note that we store this as a "ConsMul" on the tape
            ADouble temp = new MasterThesis.ADouble();
            temp.Value = x.Value / K;
            temp.PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.ConsMul, x.PlacementInTape, 0, 0, temp.Value, 1/K);
            return temp;
        }
        #endregion

        #region Implementation of Unary operators. 

        // Allows "x = -y" instead of "x = -1.0*y"
        public static ADouble operator -(ADouble x1)
        {
            return -1.0 * x1;
        }

        public static ADouble Exp(ADouble x1)
        {
            ADouble Temp = new ADouble();
            Temp.Value = Math.Exp(x1.Value);
            Temp.PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.Exp, x1.PlacementInTape, 0, 0, Temp.Value);
            return Temp;
        }

        public static ADouble Log(ADouble x1)
        {
            ADouble temp = new ADouble();
            temp.Value = Math.Log(x1.Value);
            temp.PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.Log, x1.PlacementInTape, 0, 0, temp.Value);
            return temp;
        }

        public static ADouble Pow(ADouble x1, double exponent)
        {
            ADouble temp = new ADouble();
            temp.Value = Math.Pow(x1.Value, exponent);
            temp.PlacementInTape = AADTape._tapeCounter;
            AADTape.AddEntry((int)AADUtility.AADCalculationType.Pow, x1.PlacementInTape, 0, 0, temp.Value, exponent);
            return temp;
        }

        public static ADouble Sqrt(ADouble x1)
        {
            return Pow(x1,0.5);
        }
        #endregion
    }
}
