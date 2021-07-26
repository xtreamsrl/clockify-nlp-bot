using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Exceptions;
using Bot.Utils;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot.Services.Reports
{
    public class ReportDialog: ComponentDialog
    {
        private readonly IReportSummaryService _reportSummaryService;
        private readonly IReportExtractor _reportExtractor;
        private const string ReportWaterfall = "ReportWaterfall";
        private readonly UserState _userState;

        public ReportDialog(
            IReportSummaryService reportSummaryService, IReportExtractor reportExtractor, UserState userState)
        {
            _reportSummaryService = reportSummaryService;
            _reportExtractor = reportExtractor;
            _userState = userState;
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
            string clockifyToken = userProfile.ClockifyToken ?? throw new ArgumentNullException(nameof(userProfile.ClockifyToken));
            stepContext.Values["Token"] = clockifyToken;
            var entities = (TimeSurveyBotLuis._Entities._Instance) stepContext.Options;
            
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
            catch (Exception e) when (e is InvalidWorkedPeriodInstanceException ||
                                      e is InvalidDateRangeException)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(e.Message), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }
    }
}