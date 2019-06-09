using Eleon.Modding;
using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public static class LightsHelpers
    {
        [HandlebarTag("lights")]
        public static void LightsBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{lights structure names}} helper must have exactly two argument: (structure) (name;name*;*;name)");

            var structure   = arguments[0] as StructureData;
            var namesSearch = arguments[1] as string;

            try
            {
                var uniqueNames = structure.GetUniqueNames(namesSearch);

                var devices = uniqueNames.Values.Select(N => structure.GetCurrent().GetDevice<ILight>(N)).ToArray();
                if (devices.Length > 0) devices.ForEach(L => options.Template(output, L));
                else                    options.Inverse(output, context as object);
            }
            catch (Exception error)
            {
                output.Write("{{lights}} error " + error.Message);
            }
        }

        [HandlebarTag("lightcolor")]
        public static void LightsColorHelper(TextWriter output, dynamic context, object[] arguments)
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
                output.Write("{{lightcolor}} error " + error.Message);
            }
        }

        [HandlebarTag("lightblink")]
        public static void LightsBlickHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 4) throw new HandlebarsException("{{lightblink light interval length offset}} helper must have exactly three argument: (light) (interval) (length) (offset)");

            var light = arguments[0] as ILight;

            try
            {
                light?.SetBlinkData(GetFloat(arguments[1]), GetFloat(arguments[2]), GetFloat(arguments[3]));
            }
            catch (Exception error)
            {
                output.Write("{{lightblink}} error " + error.Message);
            }
        }

        [HandlebarTag("lightintensity")]
        public static void LightsIntensityHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{lightintensity light intensity}} helper must have exactly three argument: (light) (intensity)");

            var light = arguments[0] as ILight;

            try
            {
                light?.SetIntensity(GetFloat(arguments[1]));
            }
            catch (Exception error)
            {
                output.Write("{{lightintensity}} error " + error.Message);
            }
        }

        [HandlebarTag("lightrange")]
        public static void LightsRangeHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{lightrange light range}} helper must have exactly three argument: (light) (range)");

            var light = arguments[0] as ILight;

            try
            {
                light?.SetRange(GetFloat(arguments[1]));
            }
            catch (Exception error)
            {
                output.Write("{{lightrange}} error " + error.Message);
            }
        }

        [HandlebarTag("lightspotangle")]
        public static void LightsSpotAngleHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{lightspotangle light spotangle}} helper must have exactly three argument: (light) (spotangle)");

            var light = arguments[0] as ILight;

            try
            {
                light?.SetSpotAngle(GetFloat(arguments[1]));
            }
            catch (Exception error)
            {
                output.Write("{{lightspotangle}} error " + error.Message);
            }
        }

        [HandlebarTag("lighttype")]
        public static void LightsTypeHelper(TextWriter output, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{lighttype light type}} helper must have exactly three argument: (light) (type)");

            var light = arguments[0] as ILight;

            try
            {
                Enum.TryParse<LightType>(arguments[1]?.ToString(), true, out LightType type);
                light?.SetLightType(type);
            }
            catch (Exception error)
            {
                output.Write("{{lighttype}} error " + error.Message);
            }
        }


        private static float GetFloat(object data)
        {
            return float.TryParse(data?.ToString(), out float f) ? f : 0;
        }
    }
}
