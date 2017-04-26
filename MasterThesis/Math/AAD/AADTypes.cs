using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
    public static class Constants
    {
        public static uint TAPE_SIZE = 100000;
        public enum AADType
        {
            Undef = -1,
            Const = 0,
            Asg = 1,
            Add = 2,
            Sub = 3,
            Mul = 4,
            Div = 5,
            Exp = 6,
            Log = 7,
            Pow = 8,
            ConsAdd = 13,
            ConsSub = 14,
            ConsDiv = 12,
            ConsMul = 11
        };

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
}
