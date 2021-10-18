using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Common.Recognizer;
using Bot.Data;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Bot.Clockify.Reports
{
    public class ReportDialog : ComponentDialog
    {
        private readonly IReportSummaryService _reportSummaryService;
        private readonly IReportExtractor _reportExtractor;
        private const string ReportWaterfall = "ReportWaterfall";
        private readonly UserState _userState;
        private readonly IClockifyMessageSource _messageSource;

        public ReportDialog(IReportSummaryService reportSummaryService, IReportExtractor reportExtractor,
            UserState userState, IClockifyMessageSource messageSource)
        {
            _reportSummaryService = reportSummaryService;
            _reportExtractor = reportExtractor;
            _userState = userState;
            _messageSource = messageSource;
            AddDialog(new WaterfallDialog(ReportWaterfall, new List<WaterfallStep>
            {
                HandleReportRequestAsync,
            }));
            Id = nameof(ReportDialog);
        }

        private async Task<DialogTurnResult> HandleReportRequestAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var userProfile =
                await StaticUserProfileHelper.GetUserProfileAsync(_userState, stepContext.Context, cancellationToken);
            var luisResult = (TimeSurveyBotLuis)stepContext.Options;
            try
            {
                string timePeriodInstance = luisResult.TimePeriod();
                var dateRange = _reportExtractor.GetDateRangeFromTimePeriod(timePeriodInstance);

                if (dateRange.End.Subtract(dateRange.Start).Days > 366)
                {
                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text(string.Format(_messageSource.ReportDateRangeExceedOneYear, "\n")),
                        cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }

                string channel = stepContext.Context.Activity.ChannelId;
                string summary = await _reportSummaryService.Summary(
                    channel,
                    userProfile,
                    dateRange
                );
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(summary), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            catch (InvalidWorkedPeriodInstanceException)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(_messageSource.ReportWorkedPeriodUnrecognized), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            catch (InvalidDateRangeException)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(_messageSource.ReportDateRangeError),
                    cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }
    }
}