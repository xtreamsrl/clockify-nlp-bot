using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Data;
using Bot.States;

namespace Bot.Clockify.Reports
{
    public class ReportSummaryService : IReportSummaryService
    {
        private readonly IClockifyService _clockifyService;
        private readonly ITokenRepository _tokenRepository;
        private readonly IClockifyMessageSource _messageSource;

        public ReportSummaryService(IClockifyService clockifyService, ITokenRepository tokenRepository,
            IClockifyMessageSource messageSource)
        {
            _clockifyService = clockifyService;
            _tokenRepository = tokenRepository;
            _messageSource = messageSource;
        }

        public async Task<string> Summary(string channel, UserProfile userProfile, DateRange dateRange)
        {
            var tokenData = await _tokenRepository.ReadAsync(userProfile.ClockifyTokenId!);
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
                    workspaceBuilder.Append("\n\n");
                    workspaceBuilder.Append(string.Format(_messageSource.ReportWork, workspace.Name));
                }

                if (reportEntries.Count > 0)
                {
                    workspaceBuilder.Append(ReportUtil.SummaryForReportEntries(channel, reportEntries));
                    fullSummary.Append(workspaceBuilder);
                }
                else
                {
                    fullSummary.Append("\n\n");
                    fullSummary.Append(string.Format(_messageSource.ReportNoWork, workspace.Name));
                    fullSummary.Append("\n\n");
                }
            }

            if (totalHours > 0)
            {
                string intro = string.Format(_messageSource.ReportTotalHours, ReportUtil.FormatDuration(totalHours),
                    dateRange.ToString());
                return intro + "\n\n" + fullSummary;
            }

            return fullSummary.ToString();
        }
    }
}