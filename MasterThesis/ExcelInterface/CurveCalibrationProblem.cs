using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis.ExcelInterface
{

    public class CurveCalibration
    {
        private CurveCalibrationProblem _problem;

        public CurveCalibration(CurveCalibrationProblem problem)
        {
            _problem = problem;
        }

        public void ConstructOisDiscCurve() { }
        public void ConstructLiborDiscCurve() { }
        public void ConstructFwdCurves() { }

    }

    // Class should hold the instruments, their quotes and preferably set
    // up the order of the instruments in the calibration problem.
    // Could create list based on InstrumentQuotes curvePoint (as in old method)
    public class CurveCalibrationProblem
    {
        private IDictionary<string, InstrumentQuote> _oisDiscInstruments;
        private IDictionary<string, InstrumentQuote> _liborDiscInstruments;
        private IDictionary<string, InstrumentQuote> _fwd1MInstruments;
        private IDictionary<string, InstrumentQuote> _fwd3MInstruments;
        private IDictionary<string, InstrumentQuote> _fwd6MInstruments;
        private IDictionary<string, InstrumentQuote> _fwd1YInstruments;

        private InstrumentFactory Factory;

        public CurveCalibrationProblem(InstrumentFactory instrumentFactory)
        {
            this.Factory = instrumentFactory;
        }

        public void SetOisDiscCurveInstruments() { }
        public void SetLiborDiscInstruments() { }
        public void SetFwd1MInstruments() { }
        public void SetFwd3MInstruments() { }
        public void SetFwd6MInstruments() { }
        public void SetFwd1YInstruments() { }

    }
}
