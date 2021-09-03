using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Clockify.Models;
using Clockify.Net.Models.Tasks;
using Clockify.Net.Models.Workspaces;
using F23.StringSimilarity.Interfaces;

namespace Bot.Clockify.Fill
{
    public class ClockifyEntityRecognizer
    {
        private readonly IStringDistance _stringDistanceEngine;
        private readonly IClockifyService _clockifyService;

        public ClockifyEntityRecognizer(IStringDistance stringDistanceEngine, IClockifyService clockifyService)
        {
            _stringDistanceEngine = stringDistanceEngine;
            _clockifyService = clockifyService;
        }

        public async Task<ProjectDo> RecognizeProject(string workedEntity, string apiKey)
        {
            var possibleProjects = (await GetAllPossibleProjects(apiKey)).ToList();

            var exactMatches = possibleProjects.Where(t => t.Name.ToLower().Equals(workedEntity.ToLower())).ToList();
            if (exactMatches.Count == 1)
            {
                return exactMatches.First();
            }

            var scoredProjects = possibleProjects
                .Where(p => !p.Archived.GetValueOrDefault())
                .Select(p => new
                {
                    Project = p,
                    Distance = _stringDistanceEngine.Distance(workedEntity.ToLower(), p.Name.ToLower())
                })
                .OrderBy(x => x.Distance);
            var scoredProjectsList = scoredProjects.ToList();
            if (!scoredProjectsList.Any())
            {
                throw new CannotRecognizeProjectException(workedEntity);
            }

            var best = scoredProjectsList.First();
            var secondBest = scoredProjectsList.Take(2).Last();
            bool moreThanOneTopScore = !best.Project.Equals(secondBest.Project) && Math.Abs(best.Distance - secondBest.Distance) < .01;
            bool scoreTooLow = best.Distance > 0.3;

            if (scoreTooLow)
            {
                throw new CannotRecognizeProjectException(workedEntity);
            }

            if (moreThanOneTopScore)
            {
                throw new AmbiguousRecognizableProjectException(best.Project, secondBest.Project);
            }

            return best.Project;
        }

        private async Task<IEnumerable<ProjectDo>> GetAllPossibleProjects(string apiKey)
        {
            var workspaces = await _clockifyService.GetWorkspacesAsync(apiKey);

            async Task<List<ProjectDo>> ProjectsFromWs(WorkspaceDo w) =>
                await _clockifyService.GetProjectsAsync(apiKey, w.Id);

            var possibleProjects = (await Task.WhenAll(workspaces.Select(ProjectsFromWs))).SelectMany(p => p);
            return possibleProjects;
        }

        private async Task<IEnumerable<TaskDo>> GetAllPossibleTasks(string apiKey, ProjectDo project)
        {
            return await _clockifyService.GetTasksAsync(apiKey, project.WorkspaceId, project.Id);
        }

        public async Task<TaskDo> RecognizeTask(string? workedEntity, string apiKey, ProjectDo project)
        {
            var possibleTasks = (await GetAllPossibleTasks(apiKey, project)).ToList();

            var exactMatches = possibleTasks.Where(t => t.Name.ToLower().Equals(workedEntity?.ToLower())).ToList();
            if (exactMatches.Count == 1)
            {
                return exactMatches.First();
            }

            var scoredTasks = possibleTasks
                .Select(p => new
                {
                    Entity = p, 
                    Distance = _stringDistanceEngine.Distance(workedEntity?.ToLower(), p.Name.ToLower())
                })
                .OrderBy(x => x.Distance);
            var scoredTaskList = scoredTasks.ToList();
            if (!scoredTaskList.Any())
            {
                throw new CannotRecognizeProjectException(
                    $"Project {project.Name} does not seem to have tasks defined, hence I can't really get " +
                    "a match. There was my fault to ask you in the first place, sorry for that 😟");
            }

            var best = scoredTaskList.First();
            bool uniqueAndValidTask = best.Distance < 0.3;
            if (scoredTaskList.Count > 1)
            {
                var secondBest = scoredTaskList.Take(2).Last();
                bool ambiguous = Math.Abs(best.Distance - secondBest.Distance) < .01;
                uniqueAndValidTask = uniqueAndValidTask && !ambiguous;
            }

            if (!uniqueAndValidTask)
            {
                throw new CannotRecognizeProjectException($"I can't get a task matching {workedEntity} 😟");
            }

            return best.Entity;
        }
    }

    public class AmbiguousRecognizableProjectException : Exception
    {
        public readonly ProjectDo Option1;
        public readonly ProjectDo Option2;

        public AmbiguousRecognizableProjectException(ProjectDo option1, ProjectDo option2) :
            base(option1 + " - " + option2)
        {
            Option1 = option1;
            Option2 = option2;
        }
    }
}