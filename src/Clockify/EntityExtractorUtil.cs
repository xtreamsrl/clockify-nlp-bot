using System.Linq;
using Bot.Clockify.Fill;
using Luis;

namespace Bot.Clockify
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
                throw new InvalidWorkedEntityException("No worked entity has been recognized");
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
                throw new InvalidWorkedPeriodInstanceException("No worked period has been recognized");
            }

            return workedPeriodInstances.First().Text;
        }
    }
}