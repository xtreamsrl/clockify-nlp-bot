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
        private readonly IClockifyMessageSource _messageSource;


        private const string TaskWaterfall = "TaskWaterfall";
        private const string AskForTaskStep = "AskForTask";
        private const string AskForNewTaskNameStep = "AskForNewTaskNameStep";

        private const string No = "no";
        private const string NewTask = "new task";
        private const string Abort = "abort";

        public EntryFillDialog(ClockifyEntityRecognizer clockifyWorkableRecognizer,
            ITimeEntryStoreService timeEntryStoreService, WorthAskingForTaskService worthAskingForTask,
            UserState userState, IClockifyService clockifyService, ITokenRepository tokenRepository,
            IClockifyMessageSource messageSource)
        {
            _clockifyWorkableRecognizer = clockifyWorkableRecognizer;
            _timeEntryStoreService = timeEntryStoreService;
            _worthAskingForTask = worthAskingForTask;
            _userState = userState;
            _clockifyService = clockifyService;
            _tokenRepository = tokenRepository;
            _messageSource = messageSource;
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
            var entities = (TimeSurveyBotLuis._Entities._Instance)stepContext.Options;

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
                            Title = _messageSource.No, Type = ActionTypes.MessageBack, Text = No, Value = No,
                            DisplayText = _messageSource.No
                        });
                    suggestions.Add(
                        new CardAction
                        {
                            Title = _messageSource.NewTask, Type = ActionTypes.MessageBack, Text = NewTask,
                            Value = NewTask,
                            DisplayText = _messageSource.NewTask
                        });
                    var activity = MessageFactory.Text(_messageSource.TaskSelectionQuestion);
                    activity.SuggestedActions = new SuggestedActions { Actions = suggestions };
                    return await stepContext.PromptAsync(AskForTaskStep, new PromptOptions
                    {
                        Prompt = activity,
                        RetryPrompt = MessageFactory.Text(_messageSource.TaskUnrecognizedRetry),
                        Validations = new ClockifyTaskValidatorOptions(recognizedProject, clockifyToken)
                    }, cancellationToken);
                }

                return await AddEntryAndExit(stepContext, cancellationToken, clockifyToken,
                    recognizedProject, minutes, fullEntity, null);
            }
            catch (CannotRecognizeProjectException e)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(
                    string.Format(_messageSource.ProjectUnrecognized, e.Unmatchable)), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            catch (AmbiguousRecognizableProjectException e)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(string.Format(_messageSource.AmbiguousProjectError, e.Option1.Name,
                        e.Option2.Name)), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            catch (Exception e) when (e is InvalidWorkedPeriodInstanceException ||
                                      e is InvalidWorkedEntityException)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(_messageSource.EntryFillUnderstandingError),
                    cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> CreateWithTaskOrAskForNewTaskAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var token = (string)stepContext.Values["Token"];
            var project = (ProjectDtoImpl)stepContext.Values["Project"];
            var minutes = (double)stepContext.Values["Minutes"];
            TaskDto? recognizedTask = null;
            var requestedTask = stepContext.Result.ToString();
            var fullEntity = (string)stepContext.Values["FullEntity"];
            switch (requestedTask?.ToLower())
            {
                case NewTask:
                    return await stepContext.PromptAsync(AskForNewTaskNameStep, new PromptOptions
                    {
                        Prompt = MessageFactory.Text(_messageSource.TaskCreation)
                    }, cancellationToken);
                case No:
                    return await AddEntryAndExit(stepContext, cancellationToken, token,
                        project, minutes, fullEntity, recognizedTask);
                case Abort:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(_messageSource.TaskAbort),
                        cancellationToken);
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
                            MessageFactory.Text(_messageSource.TaskUnrecognized), cancellationToken);
                        return await stepContext.EndDialogAsync(null, cancellationToken);
                    }

                    return await AddEntryAndExit(stepContext, cancellationToken, token,
                        project, minutes, fullEntity, recognizedTask);
            }
        }

        private async Task<DialogTurnResult> FeedbackAndExit(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var token = (string)stepContext.Values["Token"];
            var project = (ProjectDtoImpl)stepContext.Values["Project"];
            var minutes = (double)stepContext.Values["Minutes"];
            var newTaskName = stepContext.Result.ToString();
            var fullEntity = (string)stepContext.Values["FullEntity"];
            try
            {
                var createdTask =
                    await _clockifyService.CreateTaskAsync(token, newTaskName!, project.Id, project.WorkspaceId);
                fullEntity += " - " + createdTask.Name;
                return await AddEntryAndExit(stepContext, cancellationToken, token, project, minutes, fullEntity,
                    createdTask);
            }
            catch (Exception)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(_messageSource.TaskCreationError), cancellationToken);
                return await AddEntryAndExit(stepContext, cancellationToken, token, project, minutes, fullEntity, null);
            }
        }

        private async Task<bool> ClockifyTaskValidatorAsync(PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken)
        {
            string? requestedTask = promptContext.Recognized.Value;
            var options = (ClockifyTaskValidatorOptions)promptContext.Options.Validations;
            var specialAnswers = new[] { No, NewTask, Abort };
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

            var feedback =
                MessageFactory.Text(string.Format(_messageSource.AddEntryFeedback, minutes, fullEntity, current));
            feedback.SuggestedActions = new SuggestedActions { Actions = new List<CardAction>() };
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

        public ProjectDtoImpl Project { get; }
        public string Token { get; }
    }
}