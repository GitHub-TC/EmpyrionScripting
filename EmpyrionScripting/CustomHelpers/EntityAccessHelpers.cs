using EmpyrionScripting.CsHelper;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Internal.Interface;
using HandlebarsDotNet;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public class EntityAccessHelpers
    {
        [HandlebarTag("entitybyname")]
        public static void EntityByNameBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1 && arguments.Length != 2) throw new HandlebarsException("{{entitybyname name [maxdistance]}} helper must have one or two argument: (name) [maxdistance]");
            if (EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance == 0) return;

            var root                = rootObject as IScriptRootData;
            var namesSearch         = arguments[0]?.ToString();

            if (int.TryParse(arguments.Get(1)?.ToString(), out var distance)) distance = Math.Min((int)EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance, distance);
            else                                                              distance = root.IsElevatedScript ? int.MaxValue : (int)EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance;

            try
            {
                var found = root.GetEntities().Where(E => E.Name == namesSearch)
                    .FirstOrDefault(E => Vector3.Distance(E.Position, root.E.Pos) <= distance);

                if (found == null)  options.Inverse(output, context as object);
                else                options.Template(output, new EntityData(root.GetCurrentPlayfield(), found) { Distance = Vector3.Distance(found.Position, root.E.Pos) });
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{entitybyname}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("entitiesbyname")]
        public static void EntitiesByNameBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length < 1 && arguments.Length > 3) throw new HandlebarsException("{{entitiesbyname (name;name*;*) [maxdistance] [types]}} helper must have one or two argument: (name;name*;*) [maxdistance] [types]");
            if (EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance == 0) return;

            var root                = rootObject as IScriptRootData;
            var namesSearch         = arguments[0]?.ToString();

            var selectedTypes = arguments.Get(2)?.ToString();
            if (!string.IsNullOrEmpty(selectedTypes) && !root.IsElevatedScript) throw new HandlebarsException("'selectedTypes' only allowed in elevated scripts");

            if (int.TryParse(arguments.Get(1)?.ToString(), out var distance)) distance = root.IsElevatedScript ? distance     : Math.Min((int)EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance, distance);
            else                                                              distance = root.IsElevatedScript ? int.MaxValue : (int)EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance;

            try
            {
                var entities = string.IsNullOrEmpty(selectedTypes)
                    ? root.GetEntities() : root.GetAllEntities().Where(E =>
                        {
                            try  { return new[] { E.Type.ToString() }.GetUniqueNames(selectedTypes).Any(); }
                            catch{ return false; }
                        });

                var found = entities
                    .Where(E => new[] { E.Name }.GetUniqueNames(namesSearch).Any())
                    .Where(E => Vector3.Distance(E.Position, root.E.Pos) <= distance);

                if (found == null || !found.Any()) options.Inverse(output, context as object);
                else                               options.Template(output, found.Select(E => new EntityData(root.GetCurrentPlayfield(), E) { Distance = Vector3.Distance(E.Position, root.E.Pos) }).ToArray());
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{entitiesbyname}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("entitiesbyid")]
        public static void EntitiesByIdBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{entitiesbyid ids}} helper must have exactly one argument: (id1;id2;id3)");
            if (EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance == 0) return;

            var root      = rootObject as IScriptRootData;
            var idsSearch = arguments[0]?.ToString();

            try
            {
                var found = root.GetEntities().Where(E => new[] { E.Id.ToString() }.GetUniqueNames(idsSearch).Any());

                if (found == null || !found.Any()) options.Inverse(output, context as object);
                else                               options.Template(output, found.Select(E => new EntityData(root.GetCurrentPlayfield(), E) { Distance = Vector3.Distance(E.Position, root.E.Pos) }).ToArray());
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{entitiesbyid}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("entitybyid")]
        public static void EntityByIdBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{entitybyid id}} helper must have exactly one argument: (id)");
            if (EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance == 0) return;

            var root = rootObject as IScriptRootData;
            if (!int.TryParse(arguments[0]?.ToString(), out int id)) return;

            try
            {
                var found = root.GetEntities().FirstOrDefault(E => E.Id == id);

                if (found == null) options.Inverse(output, context as object);
                else               options.Template(output, new EntityData(root.GetCurrentPlayfield(), found) { Distance = Vector3.Distance(found.Position, root.E.Pos) });
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{entitybyid}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
