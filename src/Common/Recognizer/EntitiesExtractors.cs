using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;

namespace Bot.Common.Recognizer
{
    public static class EntitiesExtractors
    {
        public static TimeSurveyBotLuis._Entities FillEntitiesExtractor(IReadOnlyList<string> entities)
        {
            int numOfEntities = entities.Count;
            if (numOfEntities != 2)
            {
                throw new ArgumentException($"Fill intent require 2 entities but {numOfEntities} were found");
            }

            if (!EntityIsDuration(entities[0]))
            {
                throw new ArgumentException($"Entity [{entities[0]}] must be a duration");
            }

            var instances = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = entities[0],
                        Type = "builtin.datetimeV2.duration"
                    }
                },
                WorkedEntity = new[]
                {
                    new InstanceData
                    {
                        Text = entities[1]
                    }
                }
            };

            return new TimeSurveyBotLuis._Entities
            {
                _instance = instances
            };
        }

        public static TimeSurveyBotLuis._Entities ReportEntitiesExtractor(IReadOnlyList<string> entities)
        {
            int numOfEntities = entities.Count;
            if (numOfEntities != 1)
            {
                throw new ArgumentException($"Fill intent require 1 entity but {numOfEntities} were found");
            }

            if (!EntityIsDaterange(entities[0]))
            {
                throw new ArgumentException($"Entity [{entities[0]}] must be a daterange");
            }

            var instances = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = entities[0],
                        Type = "builtin.datetimeV2.daterange"
                    }
                }
            };

            return new TimeSurveyBotLuis._Entities
            {
                _instance = instances
            };
        }

        private static bool EntityIsDuration(string entity)
        {
            // TODO Find a way to make culture configurable.
            var recognizedDateTime = DateTimeRecognizer.RecognizeDateTime(entity, Culture.English).First();
            var resolvedDateTime = ((List<Dictionary<string, string>>)recognizedDateTime.Resolution["values"])[0];
            string dateTimeType = resolvedDateTime["type"];
            return dateTimeType.Equals("duration");
        }

        private static bool EntityIsDaterange(string entity)
        {
            // TODO Find a way to make culture configurable.
            var recognizedDateTime = DateTimeRecognizer.RecognizeDateTime(entity, Culture.English).First();
            var resolvedDateTime = ((List<Dictionary<string, string>>)recognizedDateTime.Resolution["values"])[0];
            string dateTimeType = resolvedDateTime["type"];
            return dateTimeType.Equals("daterange");
        }
    }
}