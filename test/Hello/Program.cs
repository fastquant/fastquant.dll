using System;
using System.Reflection;

namespace HelloWorldSample 
{
	public static class Program 
	{
		public static void Main() 
		{
			var t = Type.GetType("System.Console");
			Console.WriteLine(t.Name);
		    var a = Assembly.LoadFrom("bin\\Debug\\dnx45\\FastQuant.Config.dll");
            Console.WriteLine(a.FullName);
            Console.WriteLine(a.GetType("SmartQuant.Configuration").Name);
            Console.WriteLine("Hello World!");
		}
	}
}