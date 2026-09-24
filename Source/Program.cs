using System;
using System.Windows.Forms;
using Phanmemwar3.Forms;

namespace Phanmemwar3
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
