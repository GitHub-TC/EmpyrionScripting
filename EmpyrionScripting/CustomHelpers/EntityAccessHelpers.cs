using Eleon.Modding;
using EmpyrionScripting.DataWrapper;
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
            var isElevatedScript    = rootObject is ScriptSaveGameRootData || root.E.GetCurrent().Faction.Group == FactionGroup.Admin;
            var namesSearch         = arguments[0]?.ToString();

            if (int.TryParse(arguments.Get(1)?.ToString(), out var distance)) distance = Math.Min((int)EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance, distance);
            else                                                              distance = isElevatedScript ? int.MaxValue : (int)EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance;

            try
            {
                var found = root.GetCurrentEntites()
                    .Where(SafeIsNoProxyCheck)
                    .Where(E => isElevatedScript || E.Faction.Id == root.E.GetCurrent().Faction.Id)
                    .Where(E => E.Name == namesSearch)
                    .FirstOrDefault(E => Vector3.Distance(E.Position, root.E.Pos) <= distance);

                if (found == null) options.Inverse(output, context as object);
                else options.Template(output, new EntityData(root.GetCurrentPlayfield(), found));
            }
            catch (Exception error)
            {
                output.Write("{{entitybyname}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        private static bool SafeIsNoProxyCheck(IEntity entity)
        {
            try   { return entity != null && entity.Type != EntityType.Proxy; }
            catch { return false; }
        }

        [HandlebarTag("entitiesbyname")]
        public static void EntitiesByNameBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1 && arguments.Length != 2) throw new HandlebarsException("{{entitiesbyname (name;name*;*) [maxdistance]}} helper must have one or two argument: (name;name*;*) [maxdistance]");
            if (EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance == 0) return;

            var root                = rootObject as IScriptRootData;
            var isElevatedScript    = rootObject is ScriptSaveGameRootData || root.E.GetCurrent().Faction.Group == FactionGroup.Admin;
            var namesSearch         = arguments[0]?.ToString();

            if (int.TryParse(arguments.Get(1)?.ToString(), out var distance)) distance = Math.Min((int)EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance, distance);
            else                                                              distance = isElevatedScript ? int.MaxValue : (int)EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance;

            try
            {
                var found = root.GetCurrentEntites()
                    .Where(SafeIsNoProxyCheck)
                    .Where(E => isElevatedScript || E.Faction.Id == root.E.GetCurrent().Faction.Id)
                    .Where(E => new[] { E.Name }.GetUniqueNames(namesSearch).Any())
                    .Where(E => Vector3.Distance(E.Position, root.E.Pos) <= distance);

                if (found == null || !found.Any()) options.Inverse(output, context as object);
                else                               options.Template(output, found.Select(E => new EntityData(root.GetCurrentPlayfield(), E)).ToArray());
            }
            catch (Exception error)
            {
                output.Write("{{entitiesbyname}} error " + EmpyrionScripting.ErrorFilter(error));
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
                var found = root.GetCurrentEntites()
                    .Where(SafeIsNoProxyCheck)
                    .Where(E => E.Faction.Id == root.E.GetCurrent().Faction.Id)
                    .Where(E => new[] { E.Id.ToString() }.GetUniqueNames(idsSearch).Any())
                    .Where(E => Vector3.Distance(E.Position, root.E.Pos) <= EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance);

                if (found == null || !found.Any()) options.Inverse(output, context as object);
                else                               options.Template(output, found.Select(E => new EntityData(root.GetCurrentPlayfield(), E)).ToArray());
            }
            catch (Exception error)
            {
                output.Write("{{entitiesbyid}} error " + EmpyrionScripting.ErrorFilter(error));
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
                var found = root.GetCurrentEntites()
                    .Where(SafeIsNoProxyCheck)
                    .Where(E => E.Faction.Id == root.E.GetCurrent().Faction.Id)
                    .Where(E => E.Id == id)
                    .FirstOrDefault(E => Vector3.Distance(E.Position, root.E.Pos) <= EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance);

                if (found == null) options.Inverse(output, context as object);
                else               options.Template(output, new EntityData(root.GetCurrentPlayfield(), found));
            }
            catch (Exception error)
            {
                output.Write("{{entitybyid}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
