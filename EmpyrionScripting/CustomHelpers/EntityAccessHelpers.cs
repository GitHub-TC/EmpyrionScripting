using EmpyrionScripting.DataWrapper;
using HandlebarsDotNet;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using UnityEngine;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public class EntityAccessHelpers
    {
        [HandlebarTag("entitybyname")]
        public static void EntityByNameBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{entitybyname @root name}} helper must have exactly two argument: @root (name)");
            if (EmpyrionScripting.Configuration.Current.EntityAccessMinDistance == 0) return;

            var root        = arguments[0] as ScriptRootData;
            var namesSearch = arguments[1]?.ToString();

            try
            {
                var found = root.GetCurrentEntites()
                    .Where(E => E.Faction.Id == root.E.GetCurrent().Faction.Id)
                    .Where(E => E.Name == namesSearch)
                    .FirstOrDefault(E => Vector3.Distance(E.Position, root.E.Pos) <= EmpyrionScripting.Configuration.Current.EntityAccessMinDistance);

                if (found == null) options.Inverse(output, context as object);
                else               options.Template(output, new EntityData(found));
            }
            catch (Exception error)
            {
                output.Write("{{entitybyname}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("entitiesbyname")]
        public static void EntitiesByNameBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{entitiesbyname @root name}} helper must have exactly two argument: @root (name;name*;*)");
            if (EmpyrionScripting.Configuration.Current.EntityAccessMinDistance == 0) return;

            var root        = arguments[0] as ScriptRootData;
            var namesSearch = arguments[1]?.ToString();

            try
            {
                var found = root.GetCurrentEntites()
                    .Where(E => E.Faction.Id == root.E.GetCurrent().Faction.Id)
                    .Where(E => new[] { E.Name }.GetUniqueNames(namesSearch).Any())
                    .Where(E => Vector3.Distance(E.Position, root.E.Pos) <= EmpyrionScripting.Configuration.Current.EntityAccessMinDistance);

                if (found == null || !found.Any()) options.Inverse(output, context as object);
                else                               options.Template(output, found.Select(E => new EntityData(E)).ToArray());
            }
            catch (Exception error)
            {
                output.Write("{{entitiesbyname}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("entitiesbyid")]
        public static void EntitiesByIdBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{entitiesbyid @root ids}} helper must have exactly two argument: @root (id1;id2;id3)");
            if (EmpyrionScripting.Configuration.Current.EntityAccessMinDistance == 0) return;

            var root      = arguments[0] as ScriptRootData;
            var idsSearch = arguments[1]?.ToString();

            try
            {
                var found = root.GetCurrentEntites()
                    .Where(E => E.Faction.Id == root.E.GetCurrent().Faction.Id)
                    .Where(E => new[] { E.Id.ToString() }.GetUniqueNames(idsSearch).Any())
                    .Where(E => Vector3.Distance(E.Position, root.E.Pos) <= EmpyrionScripting.Configuration.Current.EntityAccessMinDistance);

                if (found == null || !found.Any()) options.Inverse(output, context as object);
                else                               options.Template(output, found.Select(E => new EntityData(E)).ToArray());
            }
            catch (Exception error)
            {
                output.Write("{{entitiesbyid}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("entitybyid")]
        public static void EntityByIdBlockHelper(TextWriter output, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2) throw new HandlebarsException("{{entitybyid @root id}} helper must have exactly two argument: @root (id)");
            if (EmpyrionScripting.Configuration.Current.EntityAccessMinDistance == 0) return;

            var root = arguments[0] as ScriptRootData;
            if (!int.TryParse(arguments[1]?.ToString(), out int id)) return;

            try
            {
                var found = root.GetCurrentEntites()
                    .Where(E => E.Faction.Id == root.E.GetCurrent().Faction.Id)
                    .Where(E => E.Id == id)
                    .FirstOrDefault(E => Vector3.Distance(E.Position, root.E.Pos) <= EmpyrionScripting.Configuration.Current.EntityAccessMinDistance);

                if (found == null) options.Inverse(output, context as object);
                else               options.Template(output, new EntityData(found));
            }
            catch (Exception error)
            {
                output.Write("{{entitybyid}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
