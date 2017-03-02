using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterThesis.Extensions
{
    static class Extensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }

    public static class Logging
    {
        public static void WriteLine(string message, ConsoleColor col = ConsoleColor.White)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = col;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }

        public static void WriteSectionHeader(string header, ConsoleColor col = ConsoleColor.Cyan)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = col;
            Console.WriteLine("");
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("   " + header);
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("");
            Console.ForegroundColor = oldColor;
        }
    }

}
