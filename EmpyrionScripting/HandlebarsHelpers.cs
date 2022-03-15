using EmpyrionScripting.Interface;
using HandlebarsDotNet;
using System;
using System.Linq;
using System.Reflection;

namespace EmpyrionScripting
{
    public class HandlebarHelpersAttribute : Attribute { }

    public class HandlebarTagAttribute : Attribute
    {
        public HandlebarTagAttribute(string tag)
        {
            Tag = tag;
        }

        public string Tag { get; }
    }

    public static class HandlebarsHelpers
    {
        public static void ScanHandlebarHelpers()
        {
            var helperTypes = typeof(EmpyrionScripting).Assembly.GetTypes()
                .Where(T => T.GetCustomAttributes(typeof(HandlebarHelpersAttribute), true).Length > 0);

            helperTypes.ForEach(T =>
                T.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public)
                .ForEach(M =>
                {
                    try
                    {
                        if (Attribute.GetCustomAttribute(M, typeof(HandlebarTagAttribute)) is HandlebarTagAttribute A)
                            Handlebars.RegisterHelper(A.Tag, (HandlebarsHelper)Delegate.CreateDelegate(typeof(HandlebarsHelper), M));
                    }
                    catch { }

                    try
                    {
                        if (Attribute.GetCustomAttribute(M, typeof(HandlebarTagAttribute)) is HandlebarTagAttribute A)
                            Handlebars.RegisterHelper(A.Tag, (HandlebarsBlockHelper)Delegate.CreateDelegate(typeof(HandlebarsBlockHelper), M));
                    }
                    catch { }
                })
            );
        }


    }
}
