using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Common;
using Bot.Data;
using Bot.States;

namespace Bot.Clockify.Reports
{
    public class ReportSummaryService : IReportSummaryService
    {
        private readonly IClockifyService _clockifyService;
        private readonly ITokenRepository _tokenRepository;

        public ReportSummaryService(IClockifyService clockifyService, ITokenRepository tokenRepository)
        {
            _clockifyService = clockifyService;
            _tokenRepository = tokenRepository;
        }

        public async Task<string> Summary(UserProfile userProfile, DateRange dateRange)
        {
            var tokenData = await _tokenRepository.ReadAsync(userProfile.ClockifyTokenId) ??
                            throw new Exception("Token not found");
            string clockifyToken = tokenData.Value;
            var workspaces = await _clockifyService.GetWorkspacesAsync(clockifyToken);

            var fullSummary = new StringBuilder();
            int numOfWorkspaces = workspaces.Count;
            float totalHours = 0;

            foreach (var workspace in workspaces)
            {
                var workspaceBuilder = new StringBuilder();
                var hydratedTimeEntries = await _clockifyService.GetHydratedTimeEntriesAsync(
                    clockifyToken,
                    workspace.Id,
                    userProfile.UserId ?? throw new ArgumentNullException(),
                    dateRange.Start,
                    dateRange.End
                );
                var reportEntries = ReportUtil.ConvertToReportEntries(hydratedTimeEntries).ToList();
                totalHours += reportEntries.Sum(e => e.Hours);

                if (numOfWorkspaces > 1)
                {
                    workspaceBuilder.Append($"\n\nWork reported on workspace **{workspace.Name}**:");
                }

                if (reportEntries.Count > 0)
                {
                    workspaceBuilder.Append(ReportUtil.SummaryForReportEntries(reportEntries));
                    fullSummary.Append(workspaceBuilder);
                }
                else
                {
                    fullSummary.Append($"\n\nNo work to report on workspace **{workspace.Name}**\n\n");
                }
            }

            if (totalHours > 0)
            {
                string intro = $"You worked **{ReportUtil.FormatDuration(totalHours)}** in {dateRange.ToString()}\n\n";
                return intro + fullSummary;
            }

            return fullSummary.ToString();
        }
    }
}