using System;
using System.Diagnostics;
using System.Linq;
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
        static void Main(string[] args)
        {
            Trace.AutoFlush = true;

            if ((args?.Contains("/headless") ?? false) || (args?.Contains("-headless") ?? false))
            {
                Trace.Listeners.Add(new EventLogTraceListener("OptigemLdapSync"));

                var configuration = new SyncConfiguration();
                Models.SyncResult result = null;

                try
                {
                    var kernel = new SyncEngine(configuration);

                    result = kernel.Do(null, false);
                }
                catch (Exception exception)
                {
                    Trace.TraceError(exception.ToString());

                    Console.WriteLine(exception);
                    throw;
                }
                finally
                {
                    Trace.TraceInformation(
                        "OPTIGEM LDAP Sync: {0} activated, {1} deactivated, {2} created, {3} updated.",
                        result == null ? 0 : result.Activated,
                        result == null ? 0 : result.Dectivated,
                        result == null ? 0 : result.Created,
                        result == null ? 0 : result.Updated);

                    Trace.Flush();
                }
            }
            else
            {
                Application.ThreadException += ApplicationThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
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
