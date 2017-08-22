using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSTimesheetReport
{
    public static class ExtensionMethods
    {
        public static decimal ToDecimal(this string str)
        {
            int value = 0;

            int.TryParse(str, out value);

            return value;
        }
    }

    public static class ConsoleHelper
    {
        public static void WriteLine(string value, ConsoleColor color)
        {
            var _originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ForegroundColor = _originalColor;
        }
    }

}
