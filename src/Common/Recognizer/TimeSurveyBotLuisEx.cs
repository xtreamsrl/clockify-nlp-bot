using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;

namespace Bot.Common.Recognizer
{
    public partial class TimeSurveyBotLuis
    {
        public Intent TopIntentWithMinScore(double minScore = 0.75)
        {
            (var topIntent, double score) = TopIntent();
            return score < minScore ? Intent.None : topIntent;
        }

        public string ProjectName()
        {
            var workedEntityInstances = Entities._instance.WorkedEntity;
            if (
                workedEntityInstances == null ||
                workedEntityInstances.Length == 0 ||
                Enumerable.First<InstanceData>(workedEntityInstances).Text == null
            )
            {
                throw new InvalidWorkedEntityException("No worked entity has been recognized");
            }

            return Enumerable.First<InstanceData>(workedEntityInstances).Text;
        }

        public string WorkedDuration()
        {
            var workedPeriodInstances = Entities._instance.datetime;
            if (
                workedPeriodInstances == null ||
                workedPeriodInstances.Length == 0 ||
                workedPeriodInstances.First<InstanceData>().Text == null)
            {
                throw new InvalidWorkedDurationException("No worked duration has been recognized");
            }

            return workedPeriodInstances.First<InstanceData>().Text;
        }

        public double WorkedDurationInMinutes(string culture = Culture.English)
        {
            string timePeriod = WorkedDuration();
            try
            {
                var recognizedDateTime = DateTimeRecognizer.RecognizeDateTime(timePeriod, culture).First();
                var resolvedDateTime = ((List<Dictionary<string, string>>)recognizedDateTime.Resolution["values"])[0];
                string dateTimeType = resolvedDateTime["type"];
                if (dateTimeType.Equals("duration"))
                {
                    string recognizedSeconds = resolvedDateTime["value"];
                    return double.Parse(recognizedSeconds) / 60;
                }
                throw new InvalidWorkedDurationException(
                    $"No worked duration has been recognized. Type {dateTimeType} not supported.");
            }
            catch (Exception ex) when (
                ex is FormatException ||
                ex is InvalidOperationException
            )
            {
                throw new InvalidWorkedEntityException("No worked duration has been recognized");
            }
        }

        public (DateTime start, DateTime end) WorkedPeriod(IDateTimeProvider dateTimeProvider, double minutes,
            string culture = Culture.English)
        {
            var workedPeriodInstances = Entities._instance.datetime;
            if (workedPeriodInstances.Length > 1)
            {
                string? instance = workedPeriodInstances[1].Text;
                var refTime = dateTimeProvider.DateTimeUtcNow();
                var recognizedDateTime =
                    DateTimeRecognizer.RecognizeDateTime(instance, culture, refTime: refTime).First();
                var resolvedPeriod = ((List<Dictionary<string, string>>)recognizedDateTime.Resolution["values"])[0];
                return RecognizedWorkedPeriod(refTime, resolvedPeriod, minutes);
            }

            var thisMorning = dateTimeProvider.DateTimeUtcNow().Date.AddHours(9);
            return (thisMorning, thisMorning.AddMinutes(minutes));
        }

        private static (DateTime start, DateTime end) RecognizedWorkedPeriod(DateTime refTime,
            IReadOnlyDictionary<string, string> periodData, double minutes)
        {
            string dateTimeType = periodData["type"];
            if (dateTimeType.Equals("date"))
            {
                var date = DateTime.Parse(periodData["value"]);
                var start = new DateTime(date.Year, date.Month, date.Day, 9, 0, 0);
                return (start, start.AddMinutes(minutes));
            }

            if (dateTimeType.Equals("datetime"))
            {
                var datetime = DateTime.Parse(periodData["value"]);
                return (datetime, datetime.AddMinutes(minutes));
            }

            if (dateTimeType.Equals("timerange") && periodData.ContainsKey("Mod") && periodData["Mod"].Equals("before"))
            {
                var time = DateTime.Parse(periodData["end"]);
                var datetime = new DateTime(refTime.Year, refTime.Month, refTime.Day, time.Hour, time.Minute,
                    time.Second);
                return (datetime.Subtract(TimeSpan.FromMinutes(minutes)), datetime);
            }

            if (dateTimeType.Equals("timerange") && periodData.ContainsKey("Mod") && periodData["Mod"].Equals("since"))
            {
                var time = DateTime.Parse(periodData["start"]);
                var datetime = new DateTime(refTime.Year, refTime.Month, refTime.Day, time.Hour, time.Minute,
                    time.Second);
                return (datetime, datetime.AddMinutes(minutes));
            }

            if (dateTimeType.Equals("timerange") && !periodData.ContainsKey("Mod"))
            {
                var timeStart = DateTime.Parse(periodData["start"]);
                var datetimeStart = new DateTime(refTime.Year, refTime.Month, refTime.Day, timeStart.Hour,
                    timeStart.Minute, timeStart.Second);
                var timeEnd = DateTime.Parse(periodData["end"]);
                var datetimeEnd = new DateTime(refTime.Year, refTime.Month, refTime.Day, timeEnd.Hour,
                    timeEnd.Minute, timeEnd.Second);

                double minutesBetweenDates = datetimeEnd.Subtract(datetimeStart).TotalMinutes;
                // Floating point comparison, we check that the difference is greater than one minute.
                if (Math.Abs(minutesBetweenDates - minutes) > 1)
                {
                    throw new InvalidWorkedPeriodException(
                        $"Worked period time span differs from the duration provided. Expected {minutes} but got {minutesBetweenDates}");
                }

                return (datetimeStart, datetimeEnd);
            }

            throw new InvalidWorkedPeriodException($"Date time type {dateTimeType} is not allowed");
        }
    }
}