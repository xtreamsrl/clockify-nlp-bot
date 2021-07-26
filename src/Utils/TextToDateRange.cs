using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Models;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;

namespace Bot.Utils
{
    // TODO Culture and DateRef should be configurable.
    public class TextToDateRangeService
    {
        public static DateRange? Convert(string text, DateTime? refTime = null)
        {
            // Check DateTimeRecognizer results
            // TODO: hardcoded culture english ?
            var results = DateTimeRecognizer.RecognizeDateTime(text, Culture.English, refTime: refTime);
            if (
                results.Count == 0
                || results.First().Resolution.Count == 0
                || !(results.First().Resolution.First().Value is List<Dictionary<string, string>> resolved)
                || resolved.Count == 0
            )
                return null;

            var firstResolved = resolved.First();
            if (firstResolved["type"] == "date")
            {
                // If type is date (for example "today") then replace it with a daterange with same start-end
                firstResolved["type"] = "daterange";
                firstResolved["start"] = firstResolved["value"];
                firstResolved["end"] = firstResolved["value"];
            }

            if (firstResolved["type"] != "daterange") return null;

            firstResolved.TryGetValue("start", out var start);
            firstResolved.TryGetValue("end", out var end);
            return DateRange.FromString(start, end, TimeSpan.FromDays(1));
        }
    }
}