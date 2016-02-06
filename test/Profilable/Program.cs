using SmartQuant;
using System;
using System.Threading;

namespace Profilable
{
	class Program
	{
		static void Main(string[] args)
		{
			new Demo.Backtest(Framework.Current).Run();
		    Thread.Sleep(10);
		}
	}
}
