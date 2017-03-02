using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MasterThesis.Extensions;

namespace Sandbox
{
    public static class MultiThreadingTests
    {
        public static void SimpleTest()
        {
            int workerThreads;
            int portThreads;

            ThreadPool.GetAvailableThreads(out workerThreads, out portThreads);
            Logging.WriteLine("Number of worker threads: " + workerThreads);
            Logging.WriteLine("Number of port threads: " + portThreads);
            Logging.WriteLine("Number of threads: ");
        }

    }
}
