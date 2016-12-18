using System;
using System.Diagnostics;
using System.IO;

namespace OptigemLdapSync
{
    internal class TempFilemanager : IDisposable
    {
        private const string Prefix = "OptigemLdapSync-";

        private DirectoryInfo baseDir = null;

        public void Init(bool cleanup = true)
        {
            string temp = Path.GetTempPath();

            foreach (string directory in Directory.EnumerateDirectories(temp, Prefix + "*"))
            {
                this.SafeDelete(directory);
            }

            this.baseDir = new DirectoryInfo(Path.Combine(temp, Prefix + Guid.NewGuid().ToString().Replace("-", string.Empty)));
            this.baseDir.Create();
        }

        public bool IsInitialized => this.baseDir != null;

        public string GetTempFile(string filename)
        {
            return Path.Combine(this.baseDir.FullName, filename);
        }

        public void Dispose()
        {
            if (this.baseDir != null)
            {
                this.SafeDelete(this.baseDir.FullName);
            }
        }

        private void SafeDelete(string directory)
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch (Exception exception)
            {
                Trace.TraceError(exception.ToString());
            }
        }
    }
}
