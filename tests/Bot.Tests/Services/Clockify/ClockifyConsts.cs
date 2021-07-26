using System;

namespace Bot.Tests.Services.Clockify
{
    public static class ClockifyConsts
    {
        private const string ApiKey = "CLOCKIFY_API_KEY";
        private const string WorkspaceId = "CLOCKIFY_WS_ID";

        public static readonly string ClockifyApiKey = Environment.GetEnvironmentVariable(ApiKey);
        public static readonly string ClockifyWorkspaceId = Environment.GetEnvironmentVariable(WorkspaceId);

        public const string InvalidApiKey = "invalid-api-key";
        public const string NotExistingWorkspaceId = "invalid-workspace-id";
    }
}