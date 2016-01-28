using System;
using System.Windows.Forms;
using SmartQuant;

namespace Demo
{
    public class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            //var sm = new StreamerManager();
            //var df = new DataFile(Configuration.DefaultConfiguaration().DataFileName, sm);
            //df.Open();
            //df.Dump();
            //df.Close();
            //var f = Framework.Current;
            //Console.WriteLine(f.Name);
            //Console.WriteLine(f.Configuration.DataFileName);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            Framework.Current.IsDisposable = true;
            Framework.Current.Dispose();
        }
    }
}
