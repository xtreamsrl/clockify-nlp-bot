namespace Bot.Clockify.Reports
{
    public class ReportEntry
    {
        public ReportEntry(string project, string task, float hours)
        {
            Project = project;
            Task = task;
            Hours = hours;
        }

        public string Project { get; }
        public string Task { get; }
        public float Hours { get; }
    }
}