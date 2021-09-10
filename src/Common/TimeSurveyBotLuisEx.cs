using System.Linq;
using Bot.Clockify;
using Bot.Clockify.Fill;

namespace Luis
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
                workedEntityInstances.First().Text == null
            )
            {
                throw new InvalidWorkedEntityException("No worked entity has been recognized");
            }
            return workedEntityInstances.First().Text;
        }

        public string TimePeriod()
        {
            var workedPeriodInstances = Entities._instance.datetime;
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