using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        var path = @"C:\Users\wlean\.nuget\packages\altwalker.executor\0.3.0\lib\netstandard2.0\AltwalkerExecutor.dll";
        Console.WriteLine("Inspecting: " + path);
        try
        {
            var asm = Assembly.LoadFrom(path);
            Console.WriteLine("Loaded assembly: " + asm.FullName);
            foreach (var t in asm.GetTypes())
            {
                Console.WriteLine(t.FullName);
                foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    Console.WriteLine("  M: " + m.Name);
                }
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            Console.WriteLine("ReflectionTypeLoadException:");
            foreach (var le in ex.LoaderExceptions)
            {
                Console.WriteLine("  -> " + le.Message);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
