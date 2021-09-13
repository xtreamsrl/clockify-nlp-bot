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

        public string TimePeriod()
        {
            var workedPeriodInstances = Entities._instance.datetime;
            if (
                workedPeriodInstances == null ||
                workedPeriodInstances.Length == 0 ||
                Enumerable.First<InstanceData>(workedPeriodInstances).Text == null)
            {
                throw new InvalidWorkedPeriodInstanceException("No worked period has been recognized");
            }

            return Enumerable.First<InstanceData>(workedPeriodInstances).Text;
        }

        public double TimePeriodInMinutes(string culture = Culture.English)
        {
            string timePeriod = TimePeriod();
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
                throw new InvalidWorkedPeriodInstanceException(
                    $"No worked period has been recognized. Type {dateTimeType} not supported.");
            }
            catch (Exception ex) when (
                ex is FormatException ||
                ex is InvalidOperationException
            )
            {
                throw new InvalidWorkedPeriodInstanceException("No worked period has been recognized");
            }
        }
    }
}