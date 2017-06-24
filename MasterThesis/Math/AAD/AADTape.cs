using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
    /* --- General information
     * This file contains the implementation of the automatic
     * differention tape discussed in the thesis. Also, a class "AADInputVariable"
     * has been implemented, which is an abstraction of the "active variables" in the
     * AD calculation. This class contains information on where the actual variable
     * is stored in the tape which is used to obtain the gradient from the tape
     * once this has been interpreted.
     */

    public class AADInputVariable
    {
        public double Input { get; private set; }
        private double _derivative;
        public int PositionInTape { get; private set; }
        public string Identifier { get; private set; }

        public AADInputVariable(double input, int positionInTape, string identifier = null)
        {
            Input = input;
            PositionInTape = positionInTape;

            if (identifier == null)
                Identifier = "ARG_" + positionInTape.ToString();
            else
                Identifier = identifier;
        }

        public void SetDerivative()
        {
            _derivative = AADTape.Adjoint[PositionInTape];
        }

        public double GetDerivative()
        {
            return _derivative;
        }
    }

    public static class AADTape
    {
        // Obtainable from outside the class
        public static int _tapeCounter { get; private set; }
        public static double[] Adjoint { get; private set; }

        private static int[] _argument1;
        private static int[] _argument2;
        private static int[] _operationIdentifier;
        private static double[] _value;
        private static double[] _constants;
        private static bool _tapeIsRunning;
        private static bool _tapeHasBeenInterpreted;

        // Identified by position in Arg1
        private static IDictionary<int, AADInputVariable> _AADResultSets = new Dictionary<int, AADInputVariable>();

        // Static constructor.
        static AADTape()
        {
            // Initialize once application is started
            _argument1 = new int[AADUtility.MAX_TAPE_SIZE];
            _argument2 = new int[AADUtility.MAX_TAPE_SIZE];
            _operationIdentifier = new int[AADUtility.MAX_TAPE_SIZE];
            _value = new double[AADUtility.MAX_TAPE_SIZE];
            Adjoint = new double[AADUtility.MAX_TAPE_SIZE];
            _constants = new double[AADUtility.MAX_TAPE_SIZE];


            _tapeIsRunning = false;
            _tapeHasBeenInterpreted = false;
            _tapeCounter = 0;
        }

        #region Related to initialization and reset of the tape
        public static void ResetTape()
        {
            for (int i = _tapeCounter - 1; i >= 0; i--)
                Adjoint[i] = 0;

            _tapeCounter = 0;
            _tapeIsRunning = false;
            _tapeHasBeenInterpreted = false;
            _AADResultSets.Clear();
        }

        public static void Initialize()
        {
            _tapeIsRunning = true;
            _tapeHasBeenInterpreted = false;
        }

        public static void Initialize(ADouble[] inputs, string[] identifiers = null)
        {
            _tapeIsRunning = true;
            _tapeHasBeenInterpreted = false;

            int inputCount = 0;

            if (identifiers != null)
            {
                if (identifiers.Length != inputs.Length)
                    throw new InvalidOperationException("AADTape -> Initialize: inputs and identifiers does not have same size (identifiers not null)");
            }

            // Initialize tape by assigning inputs and create inputVariables-objects.
            for (int i = 0; i<inputs.Length; i++)
            {
                if (identifiers == null)
                    _AADResultSets[_tapeCounter] = new MasterThesis.AADInputVariable(inputs[i].Value, _tapeCounter);
                else
                    _AADResultSets[_tapeCounter] = new MasterThesis.AADInputVariable(inputs[i].Value, _tapeCounter, identifiers[i]);

                inputs[i].Assign();
                inputCount += 1;
            }
        }

        public static void Initialize(List<ADouble> inputs, string[] identifiers)
        {
            Initialize(inputs.ToArray(), identifiers);
        }
        #endregion

        public static void SetGradient()
        {
            if (_tapeHasBeenInterpreted == false)
                throw new InvalidOperationException("Tape has not been intepreted. Cannot extract derivatives.");

            int inputCount = _AADResultSets.Count;

            if (_AADResultSets.Count == 0)
                return;

            for (int i = 0; i<inputCount; i++)
            {
                // Wonder if this works?
                _AADResultSets[i].SetDerivative();
            }
        }

        public static double[] GetGradient()
        {
            List<double> output = new List<double>();
            for (int i = 0; i < _AADResultSets.Count; i++)
                output.Add(_AADResultSets[i].GetDerivative());

            return output.ToArray();
        }
        
        private static void IncrementTape()
        {
            _tapeCounter += 1;
        }

        // Adds entry to the tape. This method is called whenever an AD operation. See the "ADouble" class
        public static void AddEntry(int operationIdentifier, int arg1Index, int arg2Index, double adjoint, double value, double? constant = null)
        {
            // Ensures that we can run AD stuff without filling up the memory.
            // The tape has to be initialized first
            if (_tapeIsRunning)
            {
                _argument1[_tapeCounter] = arg1Index;
                _argument2[_tapeCounter] = arg2Index;
                _operationIdentifier[_tapeCounter] = operationIdentifier;
                _value[_tapeCounter] = value;
                if (constant.HasValue)
                    _constants[_tapeCounter] = (double)constant;
                IncrementTape();
            }
        }

        public static void InterpretTape()
        {
            // Once the function has been calculated
            // and the tape recorded, run this function
            // to propagate the adjoints backwards.
            // the switch implements how the adjoint is treated
            // for each of the operators

            if (_tapeCounter == 0)
                throw new InvalidOperationException("Tape is empty. Nothing to interpret.");

            // Seed the tape by setting final adjoint to 1
            Adjoint[_tapeCounter - 1] = 1;

            // Interpret tape by backwards propagation
            for (int i = _tapeCounter - 1; i >= 1; i--)
            {
                switch (_operationIdentifier[i])
                {
                    case 1: // Assign
                        Adjoint[_argument1[i]] += Adjoint[i];
                        break;

                    // --- Arithmetic
                    case 2: // Add
                        Adjoint[_argument1[i]] += Adjoint[i];
                        Adjoint[_argument2[i]] += Adjoint[i];
                        break;
                    case 3: // Subtract
                        Adjoint[_argument1[i]] += Adjoint[i];
                        Adjoint[_argument2[i]] -= Adjoint[i];
                        break;
                    case 4: // Multiply
                        Adjoint[_argument1[i]] += _value[_argument2[i]] * Adjoint[i];
                        Adjoint[_argument2[i]] += _value[_argument1[i]] * Adjoint[i];
                        break;

                    case 5: // Division
                        Adjoint[_argument1[i]] += Adjoint[i] / _value[_argument2[i]];
                        Adjoint[_argument2[i]] -= Adjoint[i] * _value[_argument1[i]] / (Math.Pow(_value[_argument2[i]], 2));
                        break;

                    // -- Unary operations
                    case 6: // Exponentiate
                        Adjoint[_argument1[i]] += Adjoint[i] * _value[i]; // Value[i] = Exp(x). Could also say Math.Exp(Value[Arg1[i]])
                        break;
                    case 7: // Natural Logarithm
                        Adjoint[_argument1[i]] += Adjoint[i] / _value[_argument1[i]];
                        break;
                    case 8: // Power
                        Adjoint[_argument1[i]] += Adjoint[i] * _constants[i] * Math.Pow(_value[_argument1[i]], _constants[i] - 1);
                        break;

                    // --- Arithmetic with constants
                    case 11: // Const multiply
                        Adjoint[_argument1[i]] += Adjoint[i] * _constants[i];
                        break;
                    case 12: // Const Divide
                        Adjoint[_argument1[i]] -= Adjoint[i] * _constants[i] / (Math.Pow(_value[_argument1[i]], 2));
                        break;
                    case 13: // Const  add
                        Adjoint[_argument1[i]] += Adjoint[i];
                        break;

                        // Const sub. This is needed since 
                        //      f = x - K => f' = 1
                        //      f = K - x => f' = -1 ... Learned this the hard way.
                    case 14: // Const sub
                        Adjoint[_argument1[i]] += Adjoint[i];
                        break;
                    case 15: // Const sub (inverse)
                        Adjoint[_argument1[i]] -= Adjoint[i];
                        break;
                    default:
                        break;
                }
            }

            _tapeHasBeenInterpreted = true;
            _tapeIsRunning = false;
            SetGradient();
        }

        #region Print functionality for console
        public static void PrintTape()
        {
            Console.WriteLine("");
            Console.WriteLine("----------- Printing AAD tape");
            List<string[]> Out = new List<string[]>();

            Out.Add(new string[] { "    ", "#", "OPER", "ARG1", "ARG2", "VAL", "ADJ", "CONS" });
            for (int i = 0; i < _tapeCounter; i++)
            {

                Out.Add(new string[] {
                    "     ",
                    i.ToString(),
                    AADUtility.GetTypeName(AADTape._operationIdentifier[i]),
                    AADTape._argument1[i].ToString(),
                    AADTape._argument2[i].ToString(),
                    Math.Round(AADTape._value[i], 3).ToString(),
                    Math.Round(AADTape.Adjoint[i], 3).ToString(),
                    Math.Round(AADTape._constants[i],3).ToString()
                    });
            }

            var Output = PrintUtility.PrintListNicely(Out, 5);
            Console.WriteLine(Output);
        }

        public static void PrintResultSet()
        {
            Console.WriteLine("-------------------------");
            Console.WriteLine("  AAD ResultSet");
            Console.WriteLine("-------------------------");
            Console.WriteLine("");
            List<string[]> output = new List<string[]>();

            output.Add(new string[] { "Ident", "Input", "Derivative" });

            for (int i = 0; i < _AADResultSets.Count; i++)
                output.Add(new string[] { _AADResultSets[i].Identifier, _AADResultSets[i].Input.ToString(), _AADResultSets[i].GetDerivative().ToString() });

            var actualOutput = PrintUtility.PrintListNicely(output);
            Console.WriteLine(actualOutput);
        }
        #endregion
    }
}
