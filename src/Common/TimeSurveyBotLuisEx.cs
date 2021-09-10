namespace Luis
{
    public partial class TimeSurveyBotLuis
    {
        public Intent TopIntentWithMinScore(double minScore = 0.75)
        {
            (var topIntent, double score) = TopIntent();
            return score < minScore ? Intent.None : topIntent;
        }
    }
}