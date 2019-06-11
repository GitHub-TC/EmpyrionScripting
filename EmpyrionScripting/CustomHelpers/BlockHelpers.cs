using Eleon.Modding;
using HandlebarsDotNet;
using System;
using System.IO;
using System.Linq;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public class BlockHelpers
    {

        public class BlockData
        {
            private IBlock _block;
            private int blockShape;
            private int blockType;
            private int blockRotation;
            private bool? blockActive;

            public BlockData(IBlock block)
            {
                _block = block;
            }

            private BlockData GetData() {
                if (blockActive.HasValue) return this;
                _block.Get(out blockType, out blockShape, out blockRotation, out bool active);
                blockActive = active;
                return this; 
            }

            public bool Active { get => GetData().blockActive.Value; set => _block.Set(null, null, null, value); }
            public int Id => GetData().blockType;
            public int Shape => GetData().blockShape;
            public int Rotation => GetData().blockRotation;
            public int Damage => _block.GetDamage();
            public int HitPoints => _block.GetHitPoints();
            public string CustomName => _block.CustomName;
            public int? LockCode => _block.LockCode;
        }

        [HandlebarTag("blocks")]
        public static void DeviceBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{blocks structure names}} helper must have exactly two argument: (structure) (name;name*;*;name)");

            var structure   = arguments[0] as StructureData;
            var namesSearch = arguments[1] as string;

            try
            {
                var uniqueNames = structure.GetUniqueNames(namesSearch);

                var blocks = uniqueNames.Values
                    .SelectMany(N => structure.GetCurrent().GetDevicePositions(N)
                        .Select(V => structure.GetCurrent().GetBlock(V.x, V.y, V.z))).ToArray();
                if (blocks.Length > 0) blocks.ForEach(B => options.Template(output, new BlockData(B)));
                else                                       options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{blocks}} error " + error.Message);
            }
        }

        [HandlebarTag("setblockactive")]
        public static void SetBlockActiveHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{setblockactive block active}} helper must have exactly two argument: (structure) (name;name*;*;name)");

            var block = arguments[0] as BlockData;
            bool.TryParse(arguments[1]?.ToString(), out bool active);

            try
            {
                block.Active = active;
            }
            catch (Exception error)
            {
                output.Write("{{setblockactive}} error " + error.Message);
            }
        }

    }
}
