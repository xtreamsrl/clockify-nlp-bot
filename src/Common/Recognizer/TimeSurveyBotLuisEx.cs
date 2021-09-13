using System.Linq;
using Bot.Clockify;
using Bot.Clockify.Fill;
using Microsoft.Bot.Builder.AI.Luis;

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
    }
}