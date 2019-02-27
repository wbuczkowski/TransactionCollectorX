using System;
using System.Reflection;
using System.Windows.Forms;

namespace TransactionCollectorX
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static Form1 F1 = null;

        [STAThread]
        static void Main()
        {
            // Use the assembly GUID as the name of the mutex which we use to detect if an application instance is already running
            bool createdNew = false;
            string mutexName = System.Reflection.Assembly.GetExecutingAssembly().GetType().GUID.ToString();
            using (System.Threading.Mutex mutex = new System.Threading.Mutex(false, mutexName, out createdNew))
            {
                if (!createdNew)
                {
                    // Only allow one instance
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                F1 = new Form1
                {
                    Text = typeof(Program).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title
                };
                // Make sure the application runs!
                Application.Run(F1);
            }
        }
    }
}
