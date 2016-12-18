using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;

namespace OptigemLdapSync
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += ApplicationThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            Trace.AutoFlush = true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e != null)
            {
                MessageBox.Show("Es ist ein Fehler aufgetreten:" + Environment.NewLine + Environment.NewLine + e.ExceptionObject.ToString(), "Fehler...", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
        {
            if (e != null)
            {
                MessageBox.Show("Es ist ein Fehler aufgetreten:" + Environment.NewLine + Environment.NewLine + e.Exception.ToString(), "Fehler...", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
