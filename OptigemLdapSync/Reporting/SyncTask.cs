namespace OptigemLdapSync.Reporting
{
    public class SyncTask
    {
        public string Label { get; set; }

        public int Total { get; set; } = 0;

        public int Progress { get; set; } = 0;

        public string ProgressLabel { get; set; }
    }
}