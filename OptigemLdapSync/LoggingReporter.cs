using System;
using System.IO;

namespace OptigemLdapSync
{
    internal class LoggingReporter : ITaskReporter, IDisposable
    {
        private readonly ITaskReporter wrappedReporter;

        private StreamWriter writer;

        private string pendingProgress = null;

        public LoggingReporter(string logFile, ITaskReporter wrappedReporter)
        {
            this.wrappedReporter = wrappedReporter;
            this.writer = File.AppendText(logFile);
        }

        public void Dispose()
        {
            this.writer?.WriteLine($"{DateTime.Now.ToString()} sync done.");
            this.writer?.Flush();
            this.writer?.Dispose();
            this.writer = null;
        }

        public void Init(int taskCount)
        {
            this.writer.WriteLine($"{DateTime.Now.ToString()} Starting sync.");
            this.writer.WriteLine($"Current user: {Environment.UserDomainName}\\{Environment.UserName}");
            this.writer.WriteLine($"Current machine: {Environment.MachineName}");
            this.wrappedReporter?.Init(taskCount);
        }

        public void StartTask(string name, int total)
        {
            this.writer.WriteLine($"{DateTime.Now.ToString()} Starting '{name}' with {total} steps.");
            this.wrappedReporter?.StartTask(name, total);
        }

        public void Progress(string text)
        {
            this.pendingProgress = text;
            this.wrappedReporter?.Progress(text);
        }

        public void Log(string text)
        {
            if (this.pendingProgress != null)
            {
                this.writer.WriteLine("  " + this.pendingProgress);
                this.pendingProgress = null;
            }

            this.writer.WriteLine("    " + text);
            this.wrappedReporter?.Log(text);
        }
    }
}
