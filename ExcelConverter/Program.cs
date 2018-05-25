using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinancialAccountTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (CanCreate())
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FinancialAccountTool());
            }
            else
            {
                MessageBox.Show("Can only run an instance.");
                Environment.Exit(-1);
            }
        }

        private static bool CanCreate() {
            bool canCreate;
            Mutex mutex = new Mutex(false, "FinancialAccountTool", out canCreate);

            return canCreate;
        }
    }
}
