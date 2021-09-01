using Microsoft.Extensions.Localization;

namespace Bot.Clockify
{
    public class ClockifyMessageSource : IClockifyMessageSource
    {
        private readonly IStringLocalizer<ClockifyMessageSource> _localizer;

        public ClockifyMessageSource(IStringLocalizer<ClockifyMessageSource> localizer)
        {
            _localizer = localizer;
        }

        public string SetupRequest => GetString(nameof(SetupRequest));
        public string SetupFeedback => GetString(nameof(SetupFeedback));
        public string SetupReject => GetString(nameof(SetupReject));
        public string TaskCreationError => GetString(nameof(TaskCreationError));
        public string TaskUnrecognized => GetString(nameof(TaskUnrecognized));
        public string TaskCreation => GetString(nameof(TaskCreation));
        public string TaskAbort => GetString(nameof(TaskAbort));
        public string AddEntryFeedback => GetString(nameof(AddEntryFeedback));
        public string EntryFillUnderstandingError => GetString(nameof(EntryFillUnderstandingError));
        public string AmbiguousProjectError => GetString(nameof(AmbiguousProjectError));
        public string ProjectUnrecognized => GetString(nameof(ProjectUnrecognized));
        public string TaskUnrecognizedRetry => GetString(nameof(TaskUnrecognizedRetry));
        public string TaskSelectionQuestion => GetString(nameof(TaskSelectionQuestion));
        public string NewTask => GetString(nameof(NewTask));
        public string No => GetString(nameof(No));

        private string GetString(string name) => _localizer[name];
    }
}