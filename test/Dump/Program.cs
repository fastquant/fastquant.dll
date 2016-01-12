using System;
using SmartQuant;
using System.IO;


namespace HelloWorldSample 
{
	public static class Program 
	{
		public static void Main() 
		{
            var m = new MemoryStream();
            var writer = new BinaryWriter(m);
            writer.Write("SmartQuant");
            writer.Flush();
            Console.WriteLine(m.ToArray().Length);
            var f = Framework.Current;
            var df = f.DataFileManager.GetFile(@"C:\Users\alex\AppData\Roaming\SmartQuant Ltd\OpenQuant 2014\data\data.quant");
            Console.WriteLine($"Hello World!, {f.Name}");
		}
	}
}