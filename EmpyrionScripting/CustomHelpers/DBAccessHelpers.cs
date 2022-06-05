using EmpyrionScripting.CsHelper;
using EmpyrionScripting.Interface;
using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.IO;

namespace EmpyrionScripting.CustomHelpers
{
    [HandlebarHelpers]
    public class DBAccessHelpers
    {

        [HandlebarTag("db")]
        public static void DBAccessBlockHelper(TextWriter output, object rootObject, HelperOptions options, dynamic context, object[] arguments)
        {
            if (arguments.Length == 0) throw new HandlebarsException("{{db queryname [top] [orderBy] [additionalWhereAnd] [parameters]}} helper must have least one argument: (queryname)");

            var root                = rootObject as IScriptModData;
            var queryName           = arguments.Get(0)?.ToString();
            if (!int.TryParse(arguments.Get(1)?.ToString(), out var top)) top = 100;
            var orderBy             = arguments.Get(2)?.ToString();
            var additionalWhereAnd  = arguments.Get(3)?.ToString();

            try
            {
                var parameter = new Dictionary<string, object>()
                {
                    { "@PlayerId",              root.E.S.Pilot.Id    },
                    { "@FactionId",             root.E.Faction.Id    },
                    { "@FactionGroup",  (int)   root.E.Faction.Group },
                    { "@EntityId",              root.E.Id            },
                };

                for (int i = 4; i < arguments.Length; i++) parameter.Add($"@{i-3}", arguments[i]);

                var result = EmpyrionScripting.SqlDbAccess.ReadData(queryName, root.IsElevatedScript, top, orderBy, additionalWhereAnd, parameter);

                if (result.Count > 0) options.Template(output, result);
                else                  options.Inverse (output, context as object);
            }
            catch (Exception error)
            {
                if (!CsScriptFunctions.FunctionNeedsMainThread(error, root)) output.Write("{{db}} error " + EmpyrionScripting.ErrorFilter(error));
            }
        }
    }
}
