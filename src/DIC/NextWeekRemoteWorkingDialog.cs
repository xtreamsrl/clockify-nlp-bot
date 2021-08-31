using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Bot.Data;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.DIC
{
    public class NextWeekRemoteWorkingDialog : ComponentDialog
    {
        private const string RemoteWaterfall = "RemoteWaterfall";
        private const string AskForRemoteDays = "AskForRemoteDays";
        private const string Feedback = "Ok, that's nice. Enjoy your {0} days home then!";

        private const string NeedForPermissionFeedback = "Ok, got it. {0} days from home is a lot, " +
                                                         "so make sure to ask explicitely for consent";

        private readonly IDipendentiInCloudService _dicService;
        private readonly UserState _userState;
        private readonly ITokenRepository _tokenRepository;

        public NextWeekRemoteWorkingDialog(UserState userState, IDipendentiInCloudService clockifyService,
            ITokenRepository tokenRepository) : base(nameof(NextWeekRemoteWorkingDialog))
        {
            _userState = userState;
            _dicService = clockifyService;
            _tokenRepository = tokenRepository;

            AddDialog(new WaterfallDialog(RemoteWaterfall, new List<WaterfallStep>
            {
                PromptForDaysAsync,
                FeedbackAndExitAsync
            }));
            AddDialog(new TextPrompt(AskForRemoteDays, AlwaysValid));
            InitialDialogId = RemoteWaterfall;
        }

        private static async Task<DialogTurnResult> PromptForDaysAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var pickerId = Guid.NewGuid().ToString();
            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Id = pickerId,
                    Attachments = new List<Attachment> {CreateCard()},
                    Type = ActivityTypes.Message,
                },
                RetryPrompt = MessageFactory.Text("Can you retry?"),
            };
            stepContext.Values.Add("PickerID", pickerId);
            return await stepContext.PromptAsync(AskForRemoteDays, opts, cancellationToken);
        }

        private async Task<DialogTurnResult> FeedbackAndExitAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var context = stepContext.Context;
            var userProfile =
                await StaticUserProfileHelper.GetUserProfileAsync(_userState, stepContext.Context, cancellationToken);
            try
            {
                var tokenData = await _tokenRepository.ReadAsync(userProfile.DicTokenId!);
                string dicToken = tokenData.Value;
                
                Dictionary<int, bool> remotePlan = JsonConvert.DeserializeObject<Dictionary<int, bool>>(
                    JsonConvert.SerializeObject(context.Activity.Value));
                var nextMonday =
                    DateTime.Today.AddDays(((int) DayOfWeek.Sunday - (int) DateTime.Today.DayOfWeek + 7) % 7 + 1);
                foreach ((int dayOfWeekIndex, bool remote) in remotePlan.ToList())
                {
                    var day = nextMonday.AddDays(dayOfWeekIndex);
                    if (remote)
                    {
                        await _dicService.SetRemoteWorkday(day, dicToken, userProfile.EmployeeId!.Value);
                    }
                    else
                    {
                        await _dicService.DeleteRemoteWorkday(day, dicToken, userProfile.EmployeeId!.Value);
                    }
                }

                int remoteDays = remotePlan.Count(kvp => kvp.Value);
                if (remoteDays <= 3)
                {
                    await context.SendActivityAsync(MessageFactory.Text(string.Format(Feedback, remoteDays)),
                        cancellationToken);
                }
                else
                {
                    await context.SendActivityAsync(
                        MessageFactory.Text(string.Format(NeedForPermissionFeedback, remoteDays)),
                        cancellationToken);
                }

                try
                {
                    await stepContext.Context.DeleteActivityAsync(stepContext.Values["PickerID"].ToString(),
                        cancellationToken);
                }
                catch
                {
                    // ignored in case channel does not support activity deletion
                }

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            catch (Exception e)
            {
                await context.SendActivityAsync(MessageFactory.Text(string.Format(e.Message)), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private Task<bool> AlwaysValid(PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        private static Attachment CreateCard()
        {
            var toggles = new[] {1, 2, 3, 4, 5}.ToList()
                .Select(i => CultureInfo.CurrentCulture.DateTimeFormat.DayNames[i])
                .Select((day, index) => new AdaptiveToggleInput
                {
                    Id = index.ToString(),
                    Title = day.First().ToString().ToUpper() + day.Substring(1),
                    Value = "false",
                    ValueOn = "true",
                    ValueOff = "false"
                });
            var cardContainer = new AdaptiveContainer
            {
                Items =
                {
                    new AdaptiveTextBlock
                    {
                        Text = "When are you working from home next week?",
                        Color = AdaptiveTextColor.Accent,
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Medium,
                        Wrap = true
                    },
                }
            };
            cardContainer.Items.AddRange(toggles);
            var card = new AdaptiveCard("1.2")
            {
                Body =
                {
                    cardContainer
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Confirm",
                        Data = null,
                        Type = "Action.Submit"
                    }
                }
            };
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(card))
            };
        }
    }
}