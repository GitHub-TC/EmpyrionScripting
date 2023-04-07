using Eleon.Modding;
using EmpyrionScripting.CsHelper;
using EmpyrionScripting.DataWrapper;
using EmpyrionScripting.Interface;
using EmpyrionScripting.Internal.Interface;
using HandlebarsDotNet;
using System;
using System.Globalization;
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

        [HandlebarTag("globaltostructpos")]
        public static void GlobalToStructPosHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 4) throw new HandlebarsException("{{globaltostructpos structure (vector | x y z)}} helper must have two or four argument: (structure) (vector | (x) (y) (z))");
            if (EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance == 0) return;

            var root      = rootObject   as IScriptRootData;
            var structure = arguments[0] as IStructureData;

            try
            {
                if(arguments.Length == 2)
                {
                    if      (arguments[1] is Vector3    vector3)    options.Template(output, structure.GlobalToStructPos(vector3));
                    else if (arguments[1] is VectorInt3 vectorint3) options.Template(output, structure.GlobalToStructPos(new Vector3(vectorint3.x, vectorint3.y, vectorint3.z)));
                }
                else {
                    float.TryParse(arguments.Get(1)?.ToString(), out var x);
                    float.TryParse(arguments.Get(2)?.ToString(), out var y);
                    float.TryParse(arguments.Get(3)?.ToString(), out var z);
                    options.Template(output, structure.GlobalToStructPos(new Vector3((int)x, (int)y, (int)z)));
                }
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{globaltostructpos}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("structtoglobalpos")]
        public static void StructToGlobalPosHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length != 2 && arguments.Length != 4) throw new HandlebarsException("{{structtoglobalpos structure (vector | x y z)}} helper must have two or four argument: (structure) (vector | (x) (y) (z))");
            if (EmpyrionScripting.Configuration.Current.EntityAccessMaxDistance == 0) return;

            var root      = rootObject   as IScriptRootData;
            var structure = arguments[0] as IStructureData;

            try
            {
                if(arguments.Length == 2)
                {
                    if      (arguments[1] is Vector3    vector3)    options.Template(output, structure.StructToGlobalPos(new VectorInt3(vector3)));
                    else if (arguments[1] is VectorInt3 vectorint3) options.Template(output, structure.StructToGlobalPos(vectorint3));
                }
                else { 
                    float.TryParse(arguments.Get(1)?.ToString(), out var x);
                    float.TryParse(arguments.Get(2)?.ToString(), out var y);
                    float.TryParse(arguments.Get(3)?.ToString(), out var z);
                    options.Template(output, structure.StructToGlobalPos(new VectorInt3((int)x, (int)y, (int)z)));
                }
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{structtoglobalpos}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("movestop")]
        public static void MoveStopHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 0) throw new HandlebarsException("{{movestop}} have no arguments)");

            try
            {
                var root = rootObject as IScriptRootData;
                root.E.Move(Vector3.zero);
                root.E.MoveForward(0);
                root.E.MoveStop();
                output.Write("movestop");
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, rootObject)) output.Write("{{movestop}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("move")]
        public static void MoveHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1 && arguments.Length != 3) throw new HandlebarsException("{{move (vector3direction) | x y z}}");

            try
            {
                Vector3 direction;

                var root = rootObject as IScriptRootData;
                if      (arguments[0] is Vector3    vector    ) direction = vector;
                else if (arguments[0] is VectorInt3 vectorint3) direction = new Vector3(vectorint3.x, vectorint3.y, vectorint3.z);
                else { 
                    if (!float.TryParse(arguments[0]?.ToString(), out var x)) throw new HandlebarsException($"move argument (x) as float found {arguments[0]}");
                    if (!float.TryParse(arguments[1]?.ToString(), out var y)) throw new HandlebarsException($"move argument (y) as float found {arguments[1]}");
                    if (!float.TryParse(arguments[2]?.ToString(), out var z)) throw new HandlebarsException($"move argument (z) as float found {arguments[2]}");
                    direction = new Vector3(x, y, z);
                }

                root.E.Move(direction);
                output.Write($"move to {direction}");

            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, rootObject)) output.Write("{{movestop}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

        [HandlebarTag("moveforward")]
        public static void MoveForwardHelper(TextWriter output, object rootObject, dynamic context, object[] arguments)
        {
            if (arguments.Length != 1) throw new HandlebarsException("{{moveforward have one argument (speed)");

            try
            {
                var root = rootObject as IScriptRootData;
                if(!float.TryParse(arguments[0]?.ToString(), out var speed)) throw new HandlebarsException($"moveforward have one argument (speed) as float found {arguments[0]}");
                
                root.E.MoveForward(speed);
                output.Write($"moveforward with {speed}");
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, rootObject)) output.Write("{{moveforward}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }

    }
}
