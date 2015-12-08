using System;

namespace HelloWorldSample 
{
	public static class Program 
	{
		public static void Main() 
		{
			var t = Type.GetType("System.Console");
			Console.WriteLine(t.Name);
            Console.WriteLine(Type.GetType("SmartQuant.Configuration, FastQuant.Config").Name);
            Console.WriteLine("Hello World!");
		}
	}
}