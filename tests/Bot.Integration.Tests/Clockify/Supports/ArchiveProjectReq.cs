namespace Bot.Integration.Tests.Clockify.Supports
{
    public class ArchiveProjectReq
    {
        public ArchiveProjectReq(bool archived)
        {
            Archived = archived;
        }

        public bool Archived { get; }
    }
}