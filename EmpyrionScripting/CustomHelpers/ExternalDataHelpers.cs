using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpyrionScripting.CustomHelpers
{
    public static class ExternalDataHelpers
    {
        public static void DateTimeHelper(TextWriter output, dynamic context, object[] arguments)
        {
            var format = arguments.Length > 0 ? arguments[0] as string : null;
            var add    = arguments.Length > 1 ? arguments[1] as string : null;

            try
            {
                var current = string.IsNullOrEmpty(add)
                        ? DateTime.Now
                        : DateTime.UtcNow.AddHours(int.Parse(add));

                if (string.IsNullOrEmpty(format)) output.Write(current);
                else                              output.Write(current.ToString(format));
            }
            catch (Exception error)
            {
                output.Write("{{datetime}} error " + error.Message);
            }
        }

    }
}
