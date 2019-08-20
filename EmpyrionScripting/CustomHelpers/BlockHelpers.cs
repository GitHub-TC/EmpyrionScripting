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
        public static void DevicesBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{devices structure names}} helper must have exactly two argument: (structure) (name;name*;*;name)");

            var structure   = arguments[0] as IStructureData;
            var namesSearch = arguments[1].ToString();

            try
            {
                var uniqueNames = structure.AllCustomDeviceNames.GetUniqueNames(namesSearch);

                var blocks = uniqueNames
                    .SelectMany(N => structure.GetCurrent().GetDevicePositions(N)
                        .Select(V => new BlockData(structure.GetCurrent(), V))).ToArray();
                if (blocks != null && blocks.Length > 0) options.Template(output, blocks);
                else                                     options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{devices}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("devicesoftype")]
        public static void DevicesOfTypeBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{devicesoftype structure type}} helper must have exactly two argument: (structure) (type)");

            var structure  = arguments[0] as IStructureData;
            var typeSearch = arguments[1].ToString();

            try
            {
                var blocks = structure?.GetCurrent()
                                .GetDevices(typeSearch)?
                                .Values()
                                .Select(V => new BlockData(structure.GetCurrent(), V))
                                .ToArray();
                if (blocks != null && blocks.Length > 0) options.Template(output, blocks);
                else                                     options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{devicesoftype}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("block")]
        public static void ObjectBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 4) throw new HandlebarsException("{{block structure x y z}} helper must have exactly four argument: (structure) (x) (y) (z)");

            var structure = arguments[0] as IStructureData;
            int.TryParse(arguments[1].ToString(), out var x);
            int.TryParse(arguments[2].ToString(), out var y);
            int.TryParse(arguments[3].ToString(), out var z);

            try
            {
                var block = new BlockData(structure.GetCurrent(), new Eleon.Modding.VectorInt3(x, y, z));

                if (block != null) options.Template(output, block);
                else               options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{block}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("setactive")]
        public static void SetBlockActiveHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{setactive block|device active}} helper must have exactly two argument: (block|device) (name;name*;*;name)");

            var block = arguments[0] as BlockData;
            bool.TryParse(arguments[1]?.ToString(), out bool active);

            try
            {
                block.Active = active;
            }
            catch (Exception error)
            {
                output.Write("{{setactive}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("gettexture")]
        public static void GetTextureBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{gettexture block pos}} helper must have exactly three argument: (block) '(T|B|N|S|W|E)'");

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
                output.Write("{{block}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("settexture")]
        public static void SetTextureBlockHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 3) throw new HandlebarsException("{{settexture block pos textureid}} helper must have exactly three argument: (block) (T,B,N,S,W,E) (texture)");

            var block = arguments[0] as BlockData;
            var pos   = arguments[1].ToString();
            int.TryParse(arguments[2].ToString(), out var textureId);

            try
            {
                int? top = null, bottom = null, north = null, south = null, west = null, east = null;

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

                block.GetBlock().SetTextures(top, bottom, north, south, west, east);
            }
            catch (Exception error)
            {
                output.Write("{{block}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
