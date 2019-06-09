using Eleon.Modding;
using HandlebarsDotNet;
using System;
using System.IO;
using System.Linq;

namespace EmpyrionScripting.CustomHelpers
{
    public class BlockHelpers
    {
        public static readonly HandlebarsBlockHelper DeviceBlockHelper = (TextWriter output, HelperOptions options, dynamic context, object[] arguments) =>
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{device structure names}} helper must have exactly two argument: (structure) (name;name*;*;name)");

            var structure   = arguments[0] as StructureData;
            var namesSearch = arguments[1] as string;

            try
            {
                var uniqueNames = structure.GetUniqueNames(namesSearch);

                var devices = uniqueNames.Values.Select(N => structure.GetCurrent().GetDevice<IDevice>(N)).ToArray();
                if(devices.Length > 0)  options.Template(output, devices);
                else                    options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{device}} error " + error.Message);
            }
        };
    }
}
