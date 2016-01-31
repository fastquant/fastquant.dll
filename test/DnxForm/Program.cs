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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            Framework.Current.IsDisposable = true;
            Framework.Current.Dispose();
        }
    }
}
