using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Clockify.Fill;
using Bot.Clockify.Models;
using Bot.Common;
using Bot.Common.ChannelData.Telegram;
using Bot.Common.Recognizer;
using Bot.Data;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bot.Clockify.User
{
    public class UserSettingsDialog : ComponentDialog
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly ITimeEntryStoreService _timeEntryStoreService;
        private readonly UserState _userState;
        private readonly IClockifyMessageSource _messageSource;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<UserSettingsDialog> _logger;

        private const string TaskWaterfall = "TaskWaterfall";

        private const string Telegram = "telegram";

        public UserSettingsDialog(ITimeEntryStoreService timeEntryStoreService,
            UserState userState, ITokenRepository tokenRepository,
            IClockifyMessageSource messageSource, IDateTimeProvider dateTimeProvider,
            ILogger<UserSettingsDialog> logger)
        {
            _timeEntryStoreService = timeEntryStoreService;
            _userState = userState;
            _tokenRepository = tokenRepository;
            _messageSource = messageSource;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            AddDialog(new WaterfallDialog(TaskWaterfall, new List<WaterfallStep>
            {
                PromptForTaskAsync
            }));
            Id = nameof(UserSettingsDialog);
        }

        private async Task<DialogTurnResult> PromptForTaskAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            string messageText = "";

            var userProfile =
                await StaticUserProfileHelper.GetUserProfileAsync(_userState, stepContext.Context, cancellationToken);
            var tokenData = await _tokenRepository.ReadAsync(userProfile.ClockifyTokenId!);
            string clockifyToken = tokenData.Value;
            stepContext.Values["ClockifyTokenId"] = userProfile.ClockifyTokenId;
            var luisResult = (TimeSurveyBotLuis)stepContext.Options;

            var workingMinutes = luisResult.WorkedDurationInMinutes();
            var workingHours = workingMinutes / 60;

            //Default messageText
            messageText = string.Format(_messageSource.SetWorkingHoursFeedback, workingHours);
            
            //Check if there is a need for a change
            if (userProfile.WorkingHours != null)
            {
                if (userProfile.WorkingHours == workingHours)
                    messageText = string.Format(_messageSource.SetWorkingHoursUnchangedFeedback, workingHours);
            }

            //Store the working hours within the userProfile
            userProfile.WorkingHours = workingHours;
            
            //Inform user and exit the conversation.
            return await InformAndExit(stepContext, cancellationToken, messageText);
        }


        private async Task<DialogTurnResult> InformAndExit(DialogContext stepContext,
            CancellationToken cancellationToken, string messageText)
        {
            string platform = stepContext.Context.Activity.ChannelId;
            var ma = GetExitMessageActivity(messageText, platform);
            await stepContext.Context.SendActivityAsync(ma, cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }


        private static IMessageActivity GetExitMessageActivity(string messageText, string platform)
        {
            IMessageActivity ma;
            switch (platform.ToLower())
            {
                case Telegram:
                    ma = Activity.CreateMessageActivity();
                    var sendMessageParams = new SendMessageParameters(messageText, new ReplyKeyboardRemove());
                    var channelData = new SendMessage(sendMessageParams);
                    ma.ChannelData = JsonConvert.SerializeObject(channelData);
                    return ma;
                default:
                    ma = MessageFactory.Text(messageText);
                    ma.SuggestedActions = new SuggestedActions { Actions = new List<CardAction>() };
                    return ma;
            }

            ;
        }
    }
}