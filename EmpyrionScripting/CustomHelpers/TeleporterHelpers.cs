using EmpyrionScripting.CsHelper;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
using HandlebarsDotNet;
using System;
using System.IO;
using System.Linq;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    class TeleporterHelpers
    {
        [HandlebarTag("teleporters")]
        public static void TeleporterHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{teleporters structure names}} helper must have exactly two argument: (structure) (names)");

            var root                = rootObject as IScriptRootData;
            var structure           = arguments[0] as IStructureData;
            var namesSearch         = arguments[1].ToString();

            try
            {
                var uniqueNames = structure.AllCustomDeviceNames.GetUniqueNames(namesSearch).ToDictionary(N => N);

                var blocks = structure.GetCurrent()
                    .GetDevices(DeviceTypeName.Teleporter).Values()
                    .Select(V => new TeleporterData(structure.GetCurrent(), V))
                    .Where(T => uniqueNames.ContainsKey(T.CustomName))
                    .ToArray();

                if (blocks != null && blocks.Length > 0) options.Template(output, blocks);
                else                                     options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{teleporters}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setteleporter")]
        public static void SetTeleporterHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{setteleporter structure name destination}} helper must have exactly three argument: (structure) (name) (destination)");

            var root = rootObject as IScriptRootData;
            try
            {
                var destination = arguments[2].ToString();
                ChangeTeleporterData(rootObject, arguments, T => T.Destination = destination);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{setteleporter}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setteleporterdevicename")]
        public static void SetTeleporterDeviceNameHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{setteleporterdevicename structure name devicename}} helper must have exactly three argument: (structure) (name) (devicename)");

            var root = rootObject as IScriptRootData;
            try
            {
                var deviceName = arguments[2].ToString();
                ChangeTeleporterData(rootObject, arguments, T => T.DeviceName = deviceName);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{setteleporterdevicename}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setteleportertarget")]
        public static void SetTeleporterTargetHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{setteleportertarget structure name devicename}} helper must have exactly three argument: (structure) (name) (target)");

            var root = rootObject as IScriptRootData;
            try
            {
                var target = arguments[2].ToString();
                ChangeTeleporterData(rootObject, arguments, T => T.Target = target);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{setteleportertarget}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setteleporterplayfield")]
        public static void SetTeleporterPlayfieldHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{setteleporterplayfield structure name playfield}} helper must have exactly three argument: (structure) (name) (playfield)");

            var root = rootObject as IScriptRootData;
            try
            {
                var playfield = arguments[2].ToString();
                ChangeTeleporterData(rootObject, arguments, T => T.Playfield = playfield);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{setteleporterplayfield}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setteleporterorigin")]
        public static void SetTeleporterOriginHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{setteleporterorigin structure name origin}} helper must have exactly three argument: (structure) (name) (origin)");

            var root = rootObject as IScriptRootData;
            try
            {
                byte.TryParse(arguments[2].ToString(), out var origin);
                ChangeTeleporterData(rootObject, arguments, T => T.Origin = origin);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{setteleporterorigin}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        private static void ChangeTeleporterData(object rootObject, object[] arguments, Action<TeleporterData> change)
        {
            var root                = rootObject as IScriptRootData;
            var structure           = arguments[0] as IStructureData;
            var namesSearch         = arguments[1].ToString();

            if (!root.IsElevatedScript) throw new HandlebarsException("only allowed in elevated scripts");

            var uniqueNames = structure.AllCustomDeviceNames.GetUniqueNames(namesSearch).ToDictionary(N => N);

            var blocks = structure.GetCurrent()
                .GetDevices(DeviceTypeName.Teleporter).Values()
                .Select(V => new TeleporterData(structure.GetCurrent(), V))
                .Where(T => uniqueNames.ContainsKey(T.CustomName))
                .ForEach(change);
        }
    }
}
