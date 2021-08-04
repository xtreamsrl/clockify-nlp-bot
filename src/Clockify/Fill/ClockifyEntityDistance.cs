using System;
using System.Linq;
using F23.StringSimilarity.Interfaces;

namespace Bot.Clockify.Fill
{
    public class ClockifyEntityDistance : IStringDistance
    {
        private readonly IStringDistance _distance;

        public ClockifyEntityDistance(IStringDistance distance)
        {
            _distance = distance;
        }

        public double Distance(string s1, string s2)
        {
            var delimiters = new[] {'_', ' ', '-'};
            var s1Words = s1.Split(delimiters).ToList();
            var s2Words = s2.Split(delimiters).ToList();
            int n = Math.Min(s1Words.Count, s2Words.Count);
            var combinations =
                from s1W in s1Words
                from s2W in s2Words
                select new {x = s1W, y = s2W};
            var topScores = combinations.Select(tuple => _distance.Distance(tuple.x, tuple.y))
                .OrderBy(d => d).Take(n).ToList();
            return topScores.Average() * 0.9 + 0.1 / topScores.Count;
        }
    }
}