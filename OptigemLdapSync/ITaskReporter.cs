namespace OptigemLdapSync
{
    public interface ITaskReporter
    {
        void Init(int taskCount);

        void StartTask(string name, int total);

        void Progress(string text);

        void Log(string text);
    }
}
