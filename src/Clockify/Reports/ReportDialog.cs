using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Data;
using Bot.States;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot.Clockify.Reports
{
    public class ReportDialog : ComponentDialog
    {
        private readonly IReportSummaryService _reportSummaryService;
        private readonly IReportExtractor _reportExtractor;
        private const string ReportWaterfall = "ReportWaterfall";
        private readonly UserState _userState;
        private readonly ITokenRepository _tokenRepository;
        private readonly IClockifyMessageSource _messageSource;

        public ReportDialog(
            IReportSummaryService reportSummaryService, IReportExtractor reportExtractor, UserState userState,
            ITokenRepository tokenRepository, IClockifyMessageSource messageSource)
        {
            _reportSummaryService = reportSummaryService;
            _reportExtractor = reportExtractor;
            _userState = userState;
            _tokenRepository = tokenRepository;
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
            var tokenData = await _tokenRepository.ReadAsync(userProfile.ClockifyTokenId!);
            string clockifyToken = tokenData.Value;
            stepContext.Values["Token"] = clockifyToken;
            var entities = (TimeSurveyBotLuis._Entities._Instance)stepContext.Options;

            try
            {
                string timePeriodInstance = _reportExtractor.GetDateTimeInstance(entities);
                var dateRange = _reportExtractor.GetDateRangeFromTimePeriod(timePeriodInstance);

                // TODO refactor Summary to manage error response exception
                string summary = await _reportSummaryService.Summary(
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