using System.Linq;
using Bot.Clockify;
using Bot.Clockify.Fill;
using Luis;

namespace Bot.Utils
{
    public static class EntityExtractorUtil
    {
        public static string GetWorkedEntity(TimeSurveyBotLuis._Entities._Instance entities)
        {
            var workedEntityInstances = entities.WorkedEntity;
            if (
                workedEntityInstances == null ||
                workedEntityInstances.Length == 0 ||
                workedEntityInstances.First().Text == null
            )
            {
                throw new InvalidWorkedEntityException(
                    "I can see you want to report some hours, but I really can't understand on what 😕");
            }

            return workedEntityInstances.First().Text;
        }

        public static string GetWorkerPeriodInstance(TimeSurveyBotLuis._Entities._Instance entities)
        {
            var workedPeriodInstances = entities.datetime;
            if (
                workedPeriodInstances == null ||
                workedPeriodInstances.Length == 0 ||
                workedPeriodInstances.First().Text == null)
            {
                throw new InvalidWorkedPeriodInstanceException(
                    "I can see you want to report some hours, but I really can't understand how many 😕");
            }

            return workedPeriodInstances.First().Text;
        }
    }
}