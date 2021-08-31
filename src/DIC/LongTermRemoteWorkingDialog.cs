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
    public class LongTermRemoteWorkingDialog : ComponentDialog
    {
        private const string RemoteWaterfall = "RemoteWaterfall";
        private const string AskForRemoteDays = "AskForRemoteDays";
        private const string Feedback = "Ok, that's nice. Thanks for planning in advance, much appreciated!";

        private const string NeedForPermissionFeedback = "Ok, got it. {0} days a week from home is a lot, " +
                                                         "so make sure your team is ok with that";

        private readonly IDipendentiInCloudService _dicService;
        private readonly UserState _userState;
        private readonly ITokenRepository _tokenRepository;

        public LongTermRemoteWorkingDialog(UserState userState, IDipendentiInCloudService clockifyService,
            ITokenRepository tokenRepository) : base(nameof(LongTermRemoteWorkingDialog))
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
                
                Dictionary<string, string> remotePlan = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    JsonConvert.SerializeObject(context.Activity.Value));
                remotePlan.Remove("start", out string? start);
                remotePlan.Remove("end", out string? end);
                var startingDay = DateTime.Parse(start!);
                var totalDays = (int) (DateTime.Parse(end!) - startingDay).TotalDays;
                Parallel.For(0, totalDays, async i =>
                {
                    var day = startingDay.AddDays(i);
                    if (!remotePlan.ContainsKey(((int) day.DayOfWeek).ToString())) return;
                    bool remote = bool.Parse(remotePlan[((int) day.DayOfWeek).ToString()]!);
                    if (remote)
                    {
                        await _dicService.SetRemoteWorkday(day, dicToken, userProfile.EmployeeId!.Value);
                    }
                    else
                    {
                        await _dicService.DeleteRemoteWorkday(day, dicToken, userProfile.EmployeeId!.Value);
                    }
                });

                int remoteFrequency = remotePlan.Count(kvp => bool.Parse(kvp.Value));
                if (remoteFrequency <= 3)
                {
                    await context.SendActivityAsync(MessageFactory.Text(string.Format(Feedback)),
                        cancellationToken);
                }
                else
                {
                    await context.SendActivityAsync(
                        MessageFactory.Text(string.Format(NeedForPermissionFeedback, remoteFrequency)),
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

        private static DateTime NextMonday()
        {
            return DateTime.Today.AddDays(((int) DayOfWeek.Sunday - (int) DateTime.Today.DayOfWeek + 7) % 7 + 1);
        }

        private static Task<bool> AlwaysValid(PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        private static Attachment CreateCard()
        {
            var toggles = new[] {1, 2, 3, 4, 5}.ToList()
                .Select(i => CultureInfo.CurrentCulture.DateTimeFormat.DayNames[i])
                .Select((day, index) => (AdaptiveElement) new AdaptiveToggleInput
                {
                    Id = (index + 1).ToString(),
                    Title = day.First().ToString().ToUpper() + day.Substring(1),
                    Value = "false",
                    ValueOn = "true",
                    ValueOff = "false"
                });
            var daysContainer = new AdaptiveContainer
            {
                Items = toggles.ToList(),
                Spacing = AdaptiveSpacing.Large,
                Separator = true
            };
            var cardContainer = new AdaptiveContainer
            {
                Items =
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Set up your remote working schedule",
                        Color = AdaptiveTextColor.Accent,
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Medium,
                        Wrap = true
                    },
                    new AdaptiveDateInput
                    {
                        Id = "start",
                        Label = "Start",
                        Min = NextMonday().ToString("yyyy-MM-dd"),
                    },
                    new AdaptiveDateInput
                    {
                        Id = "end",
                        Label = "End",
                        Min = NextMonday().AddDays(1).ToString("yyyy-MM-dd")
                    },
                    daysContainer
                }
            };
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