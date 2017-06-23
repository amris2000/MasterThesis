﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis
{
    public class AADInputVariable
    {
        public double Input;
        private double _derivative;
        public int PositionInTape;
        public string Identifier;

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
        // Could consider making these privat.
        // Only way to obtain derivatives is through
        // the result set.
        public static int _tapeCounter { get; private set; }
        public static int[] Arg1 = new int[Constants.TAPE_SIZE];
        public static int[] Arg2 = new int[Constants.TAPE_SIZE];
        public static int[] OperationIdentifier = new int[Constants.TAPE_SIZE];
        public static double[] Value = new double[Constants.TAPE_SIZE];
        public static double[] Adjoint = new double[Constants.TAPE_SIZE];
        public static double[] Consts = new double[Constants.TAPE_SIZE];
        public static bool IsRunning { get; private set; }
        public static bool TapeHasBeenInterpreted { get; private set; }

        // Identified by position in Arg1
        private static IDictionary<int, AADInputVariable> _AADResultSets = new Dictionary<int, AADInputVariable>();

        public static void ResetTape()
        {
            for (int i = _tapeCounter - 1; i >= 0; i--)
                Adjoint[i] = 0;

            _tapeCounter = 0;
            IsRunning = false;
            TapeHasBeenInterpreted = false;
            _AADResultSets.Clear();
        }

        // Static constructor.
        static AADTape()
        {
            IsRunning = false;
            TapeHasBeenInterpreted = false;
            _tapeCounter = 0;
        }

        public static void Initialize()
        {
            //if (IsRunning)
            //    throw new InvalidOperationException("Cannot initialize. Tape is already running.");

            IsRunning = true;
            TapeHasBeenInterpreted = false;
        }

        //public static void Initialize(List<Ref<ADouble>> inputs, string[] identifiers = null)
        public static void Initialize(ADouble[] inputs, string[] identifiers = null)
        {
            //if (IsRunning)
            //    throw new InvalidOperationException("Cannot initialize. Tape is already running.");

            IsRunning = true;
            TapeHasBeenInterpreted = false;

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

        //// Overloads of initialize for convenience.
        //public static void Initialize(List<ADouble> inputs, List<string> identifiers = null)
        //{
        //    Initialize(inputs.ToArray(), identifiers.ToArray());
        //}

        public static void Initialize(List<ADouble> inputs, string[] identifiers)
        {
            Initialize(inputs.ToArray(), identifiers);
        }

        //public static void Initialize(ADouble[] inputs, List<string> identifiers = null)
        //{
        //    Initialize(inputs, identifiers.ToArray());
        //}

        public static void SetGradient()
        {
            if (TapeHasBeenInterpreted == false)
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
        
        // Tape can only be incremented internally.
        private static void IncrementTape()
        {
            _tapeCounter += 1;
        }

        // Adds entry to the tape. This method is called whenever an AD operation 
        // is being made (see ADouble class)
        public static void AddEntry(int operationIdentifier, int arg1Index, int arg2Index, double adjoint, double value, double? constant = null)
        {
            // Ensures that we can run AD stuff without filling up the memory.
            // The tape has to be initialized first
            if (IsRunning)
            {
                Arg1[_tapeCounter] = arg1Index;
                Arg2[_tapeCounter] = arg2Index;
                OperationIdentifier[_tapeCounter] = operationIdentifier;
                Value[_tapeCounter] = value;
                if (constant.HasValue)
                    Consts[_tapeCounter] = (double)constant;
                IncrementTape();
            }
        }

        // Once the function has been calculated
        // and the tape recorded, run this function
        // to propagate the adjoints backwards.
        // the switch implements how the adjoint is treated
        // for each of the operators
        public static void InterpretTape()
        {
            // Consider if exception should be thrown if
            // tape is not running (has to be running to interpret)
            // or the other way around. Nice to have

            if (_tapeCounter == 0)
                throw new InvalidOperationException("Tape is empty. Nothing to interpret.");

            // Set the last adjoint equal to 1
            Adjoint[_tapeCounter - 1] = 1;

            // Interpret tape by backwards propagation
            for (int i = _tapeCounter - 1; i >= 1; i--)
            {
                switch (OperationIdentifier[i])
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

                    case 5: // Division
                        Adjoint[Arg1[i]] += Adjoint[i] / Value[Arg2[i]];
                        Adjoint[Arg2[i]] -= Adjoint[i] * Value[Arg1[i]] / (Math.Pow(Value[Arg2[i]], 2));
                        break;

                    // -- UNARY OPERATORS
                    case 6: // Exponentiate
                        Adjoint[Arg1[i]] += Adjoint[i] * Value[i]; // Value[i] = Exp(x). Could also say Math.Exp(Value[Arg1[i]])
                        break;
                    case 7: // Natural Logarithm
                        Adjoint[Arg1[i]] += Adjoint[i] / Value[Arg1[i]];
                        break;
                    case 8: // Power
                        Adjoint[Arg1[i]] += Adjoint[i] * Consts[i] * Math.Pow(Value[Arg1[i]], Consts[i] - 1);
                        break;

                    // -- CONSTANT OPERATORS
                    case 11: // Const multiply
                        Adjoint[Arg1[i]] += Adjoint[i] * Consts[i];
                        break;
                    case 12: // Const Divide
                        Adjoint[Arg1[i]] -= Adjoint[i] * Consts[i] / (Math.Pow(Value[Arg1[i]], 2));
                        break;
                    case 13: // Const  add
                        Adjoint[Arg1[i]] += Adjoint[i];
                        break;

                        // Const sub. This is needed since 
                        //      f = x - K => f' = 1
                        //      f = K - x => f' = -1 ... Learned this the hard way.
                    case 14: // Const sub
                        Adjoint[Arg1[i]] += Adjoint[i];
                        break;
                    case 15: // Const sub (inverse)
                        Adjoint[Arg1[i]] -= Adjoint[i];
                        break;
                    default:
                        break;
                }
            }

            TapeHasBeenInterpreted = true;
            IsRunning = false;
            SetGradient();
        }

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
                    Constants.GetTypeName(AADTape.OperationIdentifier[i]),
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
}
