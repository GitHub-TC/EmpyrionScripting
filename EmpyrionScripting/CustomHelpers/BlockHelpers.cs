using EmpyrionScripting.DataWrapper;
using HandlebarsDotNet;
using System;
using System.IO;
using System.Linq;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public partial class BlockHelpers
    {

        [HandlebarTag("devices")]
        public static void DeviceBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{devices structure names}} helper must have exactly two argument: (structure) (name;name*;*;name)");

            var structure   = arguments[0] as StructureData;
            var namesSearch = arguments[1].ToString();

            try
            {
                var uniqueNames = structure.GetUniqueNames(namesSearch);

                var blocks = uniqueNames.Values
                    .SelectMany(N => structure.GetCurrent().GetDevicePositions(N)
                        .Select(V => new BlockData(structure.GetCurrent(), structure.GetCurrent().GetBlock(V), V))).ToArray();
                if (blocks != null && blocks.Length > 0) options.Template(output, blocks);
                else                                     options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{devices}} error " + error.Message);
            }
        }

        [HandlebarTag("devicesoftype")]
        public static void DeviceTypeBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{devicesoftype structure type}} helper must have exactly two argument: (structure) (type)");

            var structure  = arguments[0] as StructureData;
            var typeSearch = arguments[1].ToString();

            try
            {
                var blocks = structure?.GetCurrent()
                                .GetDevices(typeSearch)?
                                .Values()
                                .Select(V => new BlockData(structure.GetCurrent(), structure.GetCurrent().GetBlock(V), V))
                                .ToArray();
                if (blocks != null && blocks.Length > 0) options.Template(output, blocks);
                else                                     options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{devicesoftype}} error " + error.Message);
            }
        }

        [HandlebarTag("setactive")]
        public static void SetBlockActiveHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{setactive block|device active}} helper must have exactly two argument: (structure) (name;name*;*;name)");

            var block = arguments[0] as BlockData;
            bool.TryParse(arguments[1]?.ToString(), out bool active);

            try
            {
                block.Active = active;
            }
            catch (Exception error)
            {
                output.Write("{{setactive}} error " + error.Message);
            }
        }

    }
}
