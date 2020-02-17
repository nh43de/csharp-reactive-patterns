using System;
using System.Linq;

namespace Bootstrapping
{
    /// <summary>
    /// Use by calling typeof(Program).Bootstrap(); in your Main() routine
    /// </summary>
    public static class BootstrappingExtensions
    {
        public static void Bootstrap(this Type hostType)
        {
            while (true)
            {
                Console.WriteLine("Program modes:");
                Console.WriteLine("");
                Console.WriteLine("");

                //get parameterless static methods in me
                var methods = hostType.GetMethods().Where(m => m.IsPublic && m.IsStatic && m.GetParameters().Length == 0).ToArray();

                var i = 1;
                foreach (var methodInfo in methods)
                {
                    Console.WriteLine($"{i}. {methodInfo.Name}");
                    i++;
                }

                Console.WriteLine("");
                Console.WriteLine("");

                Console.WriteLine("Make a selection: ");

                var a = Console.ReadLine();

                int selectedInt;

                Console.WriteLine("");
                Console.WriteLine("");

                if (int.TryParse(a, out selectedInt) && selectedInt > 0 && selectedInt <= methods.Length)
                {
                    try
                    {
                        methods[selectedInt - 1].Invoke(null, null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("Press any key to continue");
                        Console.Read();
                    }
                }
                else
                {
                    continue;
                }
                break;
            }
        }
    }
}