using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Common;
using Bot.Data;
using Bot.States;
using Bot.Utils;
using Clockify.Net.Models.Projects;
using Clockify.Net.Models.Tasks;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using TaskStatus = Clockify.Net.Models.Tasks.TaskStatus;

namespace Bot.Clockify.Fill
{
    public class EntryFillDialog : ComponentDialog
    {
        private readonly ClockifyEntityRecognizer _clockifyWorkableRecognizer;
        private readonly IClockifyService _clockifyService;
        private readonly ITokenRepository _tokenRepository;
        private readonly ITimeEntryStoreService _timeEntryStoreService;
        private readonly WorthAskingForTaskService _worthAskingForTask;
        private readonly UserState _userState;


        private const string TaskWaterfall = "TaskWaterfall";
        private const string AskForTaskStep = "AskForTask";
        private const string AskForNewTaskNameStep = "AskForNewTaskNameStep";

        public EntryFillDialog(ClockifyEntityRecognizer clockifyWorkableRecognizer,
            ITimeEntryStoreService timeEntryStoreService, WorthAskingForTaskService worthAskingForTask,
            UserState userState, IClockifyService clockifyService, ITokenRepository tokenRepository)
        {
            _clockifyWorkableRecognizer = clockifyWorkableRecognizer;
            _timeEntryStoreService = timeEntryStoreService;
            _worthAskingForTask = worthAskingForTask;
            _userState = userState;
            _clockifyService = clockifyService;
            _tokenRepository = tokenRepository;
            AddDialog(new WaterfallDialog(TaskWaterfall, new List<WaterfallStep>
            {
                PromptForTaskAsync,
                CreateWithTaskOrAskForNewTaskAsync,
                FeedbackAndExit
            }));
            AddDialog(new TextPrompt(AskForTaskStep, ClockifyTaskValidatorAsync));
            AddDialog(new TextPrompt(AskForNewTaskNameStep));
            Id = nameof(EntryFillDialog);
        }

        private async Task<DialogTurnResult> PromptForTaskAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var userProfile =
                await StaticUserProfileHelper.GetUserProfileAsync(_userState, stepContext.Context, cancellationToken);
            var tokenData = await _tokenRepository.ReadAsync(userProfile.ClockifyTokenId!);
            string clockifyToken = tokenData.Value;
            stepContext.Values["Token"] = clockifyToken;
            var entities = (TimeSurveyBotLuis._Entities._Instance) stepContext.Options;

            try
            {
                string workedPeriod = EntityExtractorUtil.GetWorkerPeriodInstance(entities);
                string workedEntity = EntityExtractorUtil.GetWorkedEntity(entities);
                var recognizedProject = await _clockifyWorkableRecognizer.RecognizeProject(workedEntity, clockifyToken);
                stepContext.Values["Project"] = recognizedProject;
                double minutes = TextToMinutes.ToMinutes(workedPeriod);
                stepContext.Values["Minutes"] = minutes;
                string fullEntity = recognizedProject.Name;
                stepContext.Values["FullEntity"] = fullEntity;
                if (await _worthAskingForTask.IsWorthAskingForTask(recognizedProject, userProfile))
                {
                    var suggestedTasks =
                        await _clockifyService.GetTasksAsync(clockifyToken, recognizedProject.WorkspaceId,
                            recognizedProject.Id);
                    var suggestions = suggestedTasks
                        .Where(t => t.Status == TaskStatus.Active)
                        .Select(t => new CardAction
                        {
                            Title = t.Name, Type = ActionTypes.MessageBack, Value = t.Name, Text = t.Name,
                            DisplayText = t.Name
                        }).OrderBy(c => c.Title).ToList();
                    suggestions.Add(
                        new CardAction
                        {
                            Title = "no", Type = ActionTypes.MessageBack, Text = "no", Value = "no", DisplayText = "no"
                        });
                    suggestions.Add(
                        new CardAction
                        {
                            Title = "new task", Type = ActionTypes.MessageBack, Text = "new task", Value = "new task",
                            DisplayText = "new task"
                        });
                    var activity = MessageFactory.Text("Any task in particular?");
                    activity.SuggestedActions = new SuggestedActions {Actions = suggestions};
                    return await stepContext.PromptAsync(AskForTaskStep, new PromptOptions
                    {
                        Prompt = activity,
                        RetryPrompt = MessageFactory.Text("I can't get a matching task... can you be more specific?"),
                        Validations = new ClockifyTaskValidatorOptions(recognizedProject, clockifyToken)
                    }, cancellationToken);
                }

                return await AddEntryAndExit(stepContext, cancellationToken, clockifyToken,
                    recognizedProject, minutes, fullEntity, null);
            }
            catch (CannotRecognizeProjectException e)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(
                    $"Sorry, I don't find a project matching {e.Unmatchable}"), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            catch (AmbiguousRecognizableProjectException e)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(
                        "Sorry, but the project you mention is ambiguous, I can't choose between " +
                        $"{e.Option1.Name} and {e.Option2.Name}"),
                    cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            catch (Exception e) when (e is InvalidWorkedPeriodInstanceException ||
                                      e is InvalidWorkedEntityException)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(
                        "Sorry, I can see you want to fill some " +
                        "entries, but I have troubles understanding " +
                        "how many and on which project... " +
                        "Can you try and be more specific?"),
                    cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> CreateWithTaskOrAskForNewTaskAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var token = (string) stepContext.Values["Token"];
            var project = (ProjectDtoImpl) stepContext.Values["Project"];
            var minutes = (double) stepContext.Values["Minutes"];
            TaskDto? recognizedTask = null;
            var requestedTask = stepContext.Result.ToString();
            var fullEntity = (string) stepContext.Values["FullEntity"];
            switch (requestedTask?.ToLower())
            {
                case "new task":
                    return await stepContext.PromptAsync(AskForNewTaskNameStep, new PromptOptions
                    {
                        Prompt = MessageFactory.Text("What is the name of the task you want to create?")
                    }, cancellationToken);
                case "no":
                    return await AddEntryAndExit(stepContext, cancellationToken, token,
                        project, minutes, fullEntity, recognizedTask);
                case "abort":
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Ok, as you wish"), cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                default:
                    try
                    {
                        recognizedTask = await _clockifyWorkableRecognizer.RecognizeTask(requestedTask, token, project);
                        fullEntity += " - " + recognizedTask.Name;
                    }
                    catch (CannotRecognizeProjectException)
                    {
                        await stepContext.Context.SendActivityAsync(
                            MessageFactory.Text("I can't get a matching task..."),
                            cancellationToken);
                        return await stepContext.EndDialogAsync(null, cancellationToken);
                    }

                    return await AddEntryAndExit(stepContext, cancellationToken, token,
                        project, minutes, fullEntity, recognizedTask);
            }
        }

        private async Task<DialogTurnResult> FeedbackAndExit(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var token = (string) stepContext.Values["Token"];
            var project = (ProjectDtoImpl) stepContext.Values["Project"];
            var minutes = (double) stepContext.Values["Minutes"];
            var newTaskName = stepContext.Result.ToString();
            var fullEntity = (string) stepContext.Values["FullEntity"];
            try
            {
                var createdTask = await _clockifyService.CreateTaskAsync(token, newTaskName!, project.Id, project.WorkspaceId);
                fullEntity += " - " + createdTask.Name;
                return await AddEntryAndExit(stepContext, cancellationToken, token, project, minutes, fullEntity, createdTask);
            }
            catch (Exception e)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("Sorry, it appears you can't create tasks"), cancellationToken);
                return await AddEntryAndExit(stepContext, cancellationToken, token, project, minutes, fullEntity, null);
            }
            
        }

        private async Task<bool> ClockifyTaskValidatorAsync(PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken)
        {
            string? requestedTask = promptContext.Recognized.Value;
            var options = (ClockifyTaskValidatorOptions) promptContext.Options.Validations;
            var specialAnswers = new[] {"no", "new task", "abort"};
            if (specialAnswers.Contains(requestedTask?.ToLower())) return true;
            try
            {
                await _clockifyWorkableRecognizer.RecognizeTask(requestedTask, options.Token, options.Project);
                return true;
            }
            catch (CannotRecognizeProjectException)
            {
                return false;
            }
        }

        private async Task<DialogTurnResult> AddEntryAndExit(DialogContext stepContext,
            CancellationToken cancellationToken,
            string clockifyToken, ProjectDtoImpl recognizedProject, double minutes, string fullEntity, TaskDto? task)
        {
            double current =
                await _timeEntryStoreService.AddTimeEntries(clockifyToken, recognizedProject, task, minutes);
            var feedback = MessageFactory.Text(
                $"Ok, I added {minutes:0} minutes entry on {fullEntity}. " +
                $"In total you have now {current:0.00} hours filled");
            feedback.SuggestedActions = new SuggestedActions {Actions = new List<CardAction>()};
            await stepContext.Context.SendActivityAsync(feedback, cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }

    internal class ClockifyTaskValidatorOptions
    {
        public ClockifyTaskValidatorOptions(ProjectDtoImpl project, string token)
        {
            Project = project;
            Token = token;
        }

        public ProjectDtoImpl Project { get; set; }
        public string Token { get; set; }
    }
}