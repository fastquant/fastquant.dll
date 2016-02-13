using System;
using SmartQuant;
using System.IO;
using System.Diagnostics;

namespace HelloWorld 
{
	public static class Program 
	{
		public static void Main() 
		{
            Test1();
            //var f = Framework.Current;
            //var ds = f.DataManager.GetDataSeries("AAPL.0.Bid");
            //ds.Dump();
            //for (long i = 0; i < ds.Count; i++)
            //    Console.WriteLine(ds.Get(i));

            // DumpInstrument();
            Console.ReadLine();
		}
        static void DumpDataFile()
        {
            var f = Framework.Current;
            var df = new DataFile("d:\\data.quant", f.StreamerManager);
            df.Open();
            df.Dump();
            ObjectKey key;
            var kname = "AAPL.0.Bid";
            df.Keys.TryGetValue(kname, out key);
            if (key != null)
            {
                Console.WriteLine(key.DateTime);
                Console.WriteLine(key.CompressionLevel);
                Console.WriteLine(key.CompressionMethod);
                var obj = (DataSeries)key.GetObject();
                obj.Dump();
                for (long i = 0; i < obj.Count; i++)
                    Console.WriteLine(obj.Get(i));
            }
            df.Close();
        }
        static void DumpInstrument()
        {
            var f = Framework.Current;
            foreach (var i in f.InstrumentManager.Instruments)
                Console.WriteLine(i.Symbol);
        }

        static void Test1()
        {
            var during = new Stopwatch();
            var count = 300000000;
            object b = null;
            during.Start();
            for(int i=0;i<count;i++ )
            {
                b = b ?? new object();
            }
            during.Stop();
            Console.WriteLine(during.Elapsed);

            during.Restart();
            for (int i = 0; i < count; i++)
            {
                if (b == null)
                    b = new object();
               // b = b ?? new object();
            }
            during.Stop();
            Console.WriteLine(during.Elapsed);
        }
	}
}