using System;

namespace Bot.Clockify.Reports
{
    public readonly struct DateRange
    {
        public DateTime Start { get; }
        public DateTime End { get; }

        public DateRange(DateTime? start, DateTime? end)
        {
            Start = start ?? DateTime.UnixEpoch;
            End = end ?? DateTime.Today;
        }

        public static DateRange FromString(string? startS, string? endS, TimeSpan granularity)
        {
            var start = ParseDateTime(startS);
            var end = ParseDateTime(endS);
            if (end - start < granularity) end = start + granularity;
            return new DateRange(start, end);
        }

        public static DateRange FromString(string start, string end) => 
            FromString(start, end, TimeSpan.FromSeconds(0));

        private static DateTime? ParseDateTime(string? datetimeS)
        {
            DateTime? dateTime;
            try
            {
                dateTime = DateTime.Parse(datetimeS ?? throw new ArgumentNullException(nameof(datetimeS)));
            }
            catch (Exception ex) when (
                ex is ArgumentNullException
                || ex is FormatException
            )
            {
                dateTime = null;
            }

            return dateTime;
        }

        public override string ToString()
        {
            const string format = "dd MMMM yyyy";
            return $"{Start.ToString(format)} - {End.ToString(format)}";
        }

        public TimeSpan Length()
        {
            return End - Start;
        }
    }
}