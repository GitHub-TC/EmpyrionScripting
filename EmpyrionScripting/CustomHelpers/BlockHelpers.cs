using EmpyrionScripting.CsHelper;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using HandlebarsDotNet;
using System;
using System.IO;
using System.Linq;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public class BlockHelpers
    {

        [HandlebarTag("devices")]
        public static void DevicesBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{devices structure names}} helper must have exactly two argument: (structure) (name;name*;*;name)");

            var root        = rootObject as IScriptModData;
            var structure   = arguments[0] as IStructureData;
            var namesSearch = arguments[1].ToString();

            try
            {
                var blocks = Devices(structure, namesSearch);

                if (blocks != null && blocks.Length > 0) options.Template(output, blocks);
                else                                     options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                if(!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{devices}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        public static IBlockData[] Devices(IStructureData structure, string names)
        {
            var uniqueNames = structure.AllCustomDeviceNames.GetUniqueNames(names);

            return uniqueNames
                .SelectMany(N => structure.GetCurrent().GetDevicePositions(N)
                .Select(V => new BlockData(structure.E, V)))
                .ToArray();
        }

        [HandlebarTag("devicesoftype")]
        public static void DevicesOfTypeBlockHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{devicesoftype structure type}} helper must have exactly two argument: (structure) (type)");

            var structure  = arguments[0] as IStructureData;
            var typeSearch = arguments[1].ToString();

            try
            {
                if (!Enum.TryParse<DeviceTypeName>(typeSearch, true, out var deviceType))
                {
                    output.Write("{{devicesoftype}} error unknown device " + typeSearch);
                }

                var blocks = DevicesOfType(structure, deviceType);

                if (blocks != null && blocks.Length > 0) options.Template(output, blocks);
                else                                     options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{devicesoftype}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        public static IBlockData[] DevicesOfType(IStructureData structure, DeviceTypeName deviceType)
        {
            return structure?.GetCurrent()
                            .GetDevices(deviceType)?
                            .Values()
                            .Select(V => new BlockData(structure.E, V))
                            .ToArray();
        }

        [HandlebarTag("block")]
        public static void ObjectBlockHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 4) throw new HandlebarsException("{{block structure x y z}} helper must have exactly four argument: (structure) (x) (y) (z)");

            var structure = arguments[0] as IStructureData;
            int.TryParse(arguments[1].ToString(), out var x);
            int.TryParse(arguments[2].ToString(), out var y);
            int.TryParse(arguments[3].ToString(), out var z);

            try
            {
                var block = new BlockData(structure.E, new Eleon.Modding.VectorInt3(x, y, z));

                if (block != null) options.Template(output, block);
                else               options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{block}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setactive")]
        public static void SetBlockActiveHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{setactive block|device active}} helper must have exactly two argument: (block|device) (true|false)");

            var block = arguments[0] as BlockData;
            bool.TryParse(arguments[1]?.ToString(), out bool active);

            try
            {
                block.Active = active;
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{setactive}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setlockcode")]
        public static void SetLockCodeHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{setlockcode block|device code}} helper must have exactly two argument: (block|device) (code)");

            var block = arguments[0] as BlockData;
            int.TryParse(arguments[1]?.ToString(), out var code);

            try
            {
                block.LockCode = code;
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{setlockcode}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("gettexture")]
        public static void GetTextureBlockHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{gettexture block pos}} helper must have exactly two argument: (block) '(T|B|N|S|W|E)'");

            var block = arguments[0] as BlockData;
            var pos   = arguments[1].ToString();

            try
            {
                var textureId = -1;
                switch (pos.ToUpper())
                {
                    case "T": textureId = block.Top;    break;
                    case "B": textureId = block.Bottom; break;
                    case "N": textureId = block.North;  break;
                    case "S": textureId = block.South;  break;
                    case "W": textureId = block.West;   break;
                    case "E": textureId = block.East;   break;
                }

                if (textureId != -1) options.Template(output, textureId);
                else                 options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{block}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("settexture")]
        public static void SetTextureBlockHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 3) throw new HandlebarsException("{{settexture block textureid [pos]}} helper must have at least two argument: (block) (texture) [T,B,N,S,W,E]");

            var block = arguments[0] as BlockData;
            int.TryParse(arguments[1].ToString(), out var textureId);
            var pos   = arguments.Get(2)?.ToString();

            try
            {
                int? top = null, bottom = null, north = null, south = null, west = null, east = null;

                if (string.IsNullOrEmpty(pos))
                {
                    block.SetTextureForWholeBlock(textureId);
                    return;
                }

                pos.ToUpper()
                    .Split(',')
                    .Select(P => P.Trim())
                    .ForEach(P => { 
                            switch (P)
                            {
                                case "T": top       = textureId; break;
                                case "B": bottom    = textureId; break;
                                case "N": north     = textureId; break;
                                case "S": south     = textureId; break;
                                case "W": west      = textureId; break;
                                case "E": east      = textureId; break;
                            }
                        }
                    );

                block.SetTextures(top, bottom, north, south, west, east);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{block}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("getcolor")]
        public static void GetColorBlockHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{getcolor block pos}} helper must have exactly two argument: (block) '(T|B|N|S|W|E)'");

            var block = arguments[0] as BlockData;
            var pos   = arguments[1].ToString();

            try
            {
                var colorId = -1;
                switch (pos.ToUpper())
                {
                    case "T": colorId = block.TopColor;    break;
                    case "B": colorId = block.BottomColor; break;
                    case "N": colorId = block.NorthColor;  break;
                    case "S": colorId = block.SouthColor;  break;
                    case "W": colorId = block.WestColor;   break;
                    case "E": colorId = block.EastColor;   break;
                }

                if (colorId != -1) options.Template(output, colorId);
                else               options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{block}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setcolor")]
        public static void SetColorBlockHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 3) throw new HandlebarsException("{{setcolor block colorid [pos]}} helper must have at least two argument: (block) (color) [T,B,N,S,W,E]");

            var block = arguments[0] as BlockData;
            int.TryParse(arguments[1].ToString(), out var colorId);
            var pos   = arguments.Get(2)?.ToString();

            try
            {
                int? top = null, bottom = null, north = null, south = null, west = null, east = null;

                if (string.IsNullOrEmpty(pos))
                {
                    block.SetColorForWholeBlock(colorId);
                    return;
                }

                pos.ToUpper()
                    .Split(',')
                    .Select(P => P.Trim())
                    .ForEach(P => { 
                            switch (P)
                            {
                                case "T": top       = colorId; break;
                                case "B": bottom    = colorId; break;
                                case "N": north     = colorId; break;
                                case "S": south     = colorId; break;
                                case "W": west      = colorId; break;
                                case "E": east      = colorId; break;
                            }
                        }
                    );

                block.SetColors(top, bottom, north, south, west, east);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{block}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
