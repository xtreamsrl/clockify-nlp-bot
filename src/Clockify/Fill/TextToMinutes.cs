﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;

namespace Bot.Clockify.Fill
{
    public static class TextToMinutes
    {
        public static double ToMinutes(string text)
        {
            try
            {
                var recognizedDateTime = DateTimeRecognizer.RecognizeDateTime(text, Culture.English).First();
                string recognizedSeconds =
                    ((List<Dictionary<string, string>>) recognizedDateTime.Resolution["values"])[0]["value"];
                return double.Parse(recognizedSeconds) / 60;
            }
            catch (Exception ex) when (
                ex is FormatException ||
                ex is InvalidOperationException
            )
            {
                throw new InvalidWorkedPeriodInstanceException(
                    "I can see you want to report some hours, but I really can't understand how many 😕");
            }
        }
    }
}