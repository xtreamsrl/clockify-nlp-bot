namespace Bot.Clockify
{
    public interface IClockifyMessageSource
    {
        string SetupRequest { get; }
        string SetupFeedback { get; }
        string SetupReject { get; }

        string TaskCreationError { get; }
        string TaskUnrecognized { get; }
        string TaskCreation { get; }
        string TaskAbort { get; }
        string AddEntryFeedback { get; }
        string EntryFillUnderstandingError { get; }
        string AmbiguousProjectError { get; }
        string ProjectUnrecognized { get; }
        string TaskUnrecognizedRetry { get; }
        string TaskSelectionQuestion { get; }
        string NewTask { get; }
        string No { get; }
        
        string ReportDateRangeError { get; }
        string ReportWorkedPeriodUnrecognized { get; }
        string ReportTotalHours { get; }
        string ReportNoWork { get; }
        string ReportWork { get; }
        string ReportDateRangeExceedOneYear { get; }

        string RemindStoppedAlready { get; }
        string RemindStopAnswer { get; }
        string RemindEntryFill { get; }
        
        string RemindEntryFillYesterday { get; }

        string FollowUp { get; }
    }
}