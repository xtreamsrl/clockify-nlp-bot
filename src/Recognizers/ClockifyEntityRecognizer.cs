using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Exceptions;
using Bot.Services;
using Castle.Core.Internal;
using Clockify.Net.Models.Projects;
using Clockify.Net.Models.Tasks;
using Clockify.Net.Models.Workspaces;
using F23.StringSimilarity.Interfaces;

namespace Bot.Recognizers
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

        public async Task<ProjectDtoImpl> RecognizeProject(string workedEntity, string apiKey)
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
            if (scoredProjectsList.IsNullOrEmpty())
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

        private async Task<IEnumerable<ProjectDtoImpl>> GetAllPossibleProjects(string apiKey)
        {
            var workspaces = await _clockifyService.GetWorkspacesAsync(apiKey);

            async Task<List<ProjectDtoImpl>> ProjectsFromWs(WorkspaceDto w) =>
                await _clockifyService.GetProjectsAsync(apiKey, w.Id);

            var possibleProjects = (await Task.WhenAll(workspaces.Select(ProjectsFromWs))).SelectMany(p => p);
            return possibleProjects;
        }

        private async Task<IEnumerable<TaskDto>> GetAllPossibleTasks(string apiKey, ProjectDtoImpl project)
        {
            return await _clockifyService.GetTasksAsync(apiKey, project.WorkspaceId, project.Id);
        }

        public async Task<TaskDto> RecognizeTask(string? workedEntity, string apiKey, ProjectDtoImpl project)
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
            if (scoredTaskList.IsNullOrEmpty())
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
        public readonly ProjectDtoImpl Option1;
        public readonly ProjectDtoImpl Option2;

        public AmbiguousRecognizableProjectException(ProjectDtoImpl option1, ProjectDtoImpl option2) :
            base(option1 + " - " + option2)
        {
            Option1 = option1;
            Option2 = option2;
        }
    }
}