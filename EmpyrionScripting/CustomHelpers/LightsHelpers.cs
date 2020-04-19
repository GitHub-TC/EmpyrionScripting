using Eleon.Modding;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using HandlebarsDotNet;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public static class LightsHelpers
    {
        [HandlebarTag("lights")]
        public static void LightsBlockHelper(TextWriter output, object root, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{lights structure names}} helper must have exactly two argument: (structure) (name;name*;*;name)");

            var structure   = arguments[0] as IStructureData;
            var namesSearch = arguments[1]?.ToString();

            try
            {
                var uniqueNames = structure.AllCustomDeviceNames.GetUniqueNames(namesSearch);

                var devices = uniqueNames.Select(N => structure.GetCurrent().GetDevice<ILight>(N)).ToArray();
                if (devices.Length > 0) devices.ForEach(L => options.Template(output, L));
                else                    options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{lights}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("lightcolor")]
        public static void LightsColorHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{lightcolor light color}} helper must have exactly two argument: (light) (rgb hex)");

            var light = arguments[0] as ILight;

            try
            {
                int.TryParse(arguments[1] as string, NumberStyles.HexNumber, null, out int color);
                light?.SetColor(new Color((color & 0xff0000) >> 16, (color & 0x00ff00) >> 8, color & 0x0000ff));
            }
            catch (Exception error)
            {
                output.Write("{{lightcolor}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("lightblink")]
        public static void LightsBlickHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 4) throw new HandlebarsException("{{lightblink light interval length offset}} helper must have exactly four argument: (light) (interval) (length) (offset)");

            var light = arguments[0] as ILight;

            try
            {
                light?.SetBlinkData(GetFloat(arguments[1]), GetFloat(arguments[2]), GetFloat(arguments[3]));
            }
            catch (Exception error)
            {
                output.Write("{{lightblink}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("lightintensity")]
        public static void LightsIntensityHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{lightintensity light intensity}} helper must have exactly two argument: (light) (intensity)");

            var light = arguments[0] as ILight;

            try
            {
                light?.SetIntensity(GetFloat(arguments[1]));
            }
            catch (Exception error)
            {
                output.Write("{{lightintensity}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("lightrange")]
        public static void LightsRangeHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{lightrange light range}} helper must have exactly two argument: (light) (range)");

            var light = arguments[0] as ILight;

            try
            {
                light?.SetRange(GetFloat(arguments[1]));
            }
            catch (Exception error)
            {
                output.Write("{{lightrange}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("lightspotangle")]
        public static void LightsSpotAngleHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{lightspotangle light spotangle}} helper must have exactly two argument: (light) (spotangle)");

            var light = arguments[0] as ILight;

            try
            {
                light?.SetSpotAngle(GetFloat(arguments[1]));
            }
            catch (Exception error)
            {
                output.Write("{{lightspotangle}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("lighttype")]
        public static void LightsTypeHelper(TextWriter output, object root, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{lighttype light type}} helper must have exactly two argument: (light) (type)");

            var light = arguments[0] as ILight;

            try
            {
                Enum.TryParse<LightType>(arguments[1]?.ToString(), true, out LightType type);
                light?.SetLightType(type);
            }
            catch (Exception error)
            {
                output.Write("{{lighttype}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }


        private static float GetFloat(object data)
        {
            return float.TryParse(data?.ToString(), out float f) ? f : 0;
        }
    }
}
