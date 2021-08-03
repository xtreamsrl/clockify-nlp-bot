using Bot.Clockify;
using Bot.Clockify.Client;
using Bot.Clockify.Fill;
using Bot.Clockify.Reports;
using Bot.Common;
using Bot.Data;
using Bot.Dialogs;
using Bot.DIC;
using Bot.Remind;
using Bot.Security;
using Bot.States;
using Bot.Supports;
using F23.StringSimilarity;
using F23.StringSimilarity.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bot
{
    public class Startup
    {
        private readonly string _dataConnectionString;
        private readonly string _containerName;
        public Startup(IConfiguration configuration)
        {
            _dataConnectionString = configuration.GetSection("BlobStorage")["DataConnectionString"];
            _containerName = configuration.GetSection("BlobStorage")["ContainerName"];
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            var clockifyService = new ClockifyService(new ClockifyClientFactory());
            var dicService = new DipendentiInCloudService(new DipendentiInCloudClient());

            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddSingleton<TextToDateRangeService>();
            services.AddSingleton<ReportDialog>();
            services.AddSingleton<IReportSummaryService, ReportSummaryService>();
            services.AddSingleton<IReportExtractor, ReportExtractor>();
            services.AddSingleton<EntryFillDialog>();
            services.AddSingleton<StopReminderDialog>();
            services.AddSingleton<WorthAskingForTaskService>();
            services.AddSingleton<ITimeEntryStoreService, TimeEntryStoreService>();
            services.AddSingleton<IStringDistance>(new ClockifyEntityDistance(new MetricLCS()));
            services.AddSingleton<ClockifyEntityRecognizer, ClockifyEntityRecognizer>();
            services.AddSingleton<UserProfilesProvider>();
            services.AddSingleton<IClockifyService>(clockifyService);
            services.AddSingleton<NextWeekRemoteWorkingDialog, NextWeekRemoteWorkingDialog>();
            services.AddSingleton<LongTermRemoteWorkingDialog, LongTermRemoteWorkingDialog>();
            services.AddSingleton<TeamAvailabilityService, TeamAvailabilityService>();
            services.AddSingleton<NotifyUsersDialog, NotifyUsersDialog>();
            
            // DIC
            services.AddSingleton<DicSetupDialog, DicSetupDialog>();
            services.AddSingleton<IDipendentiInCloudService>(dicService);

            services.AddSingleton<INeedRemindService, TimeSheetNotFullEnough>();
            services.AddSingleton<INeedRemindService, UserDidNotSayStop>();
            services.AddSingleton<INeedRemindService, NotOnLeave>();
            services.AddSingleton<ICompositeNeedReminderService, CompositeNeedReminderService>();
            services.AddSingleton<IRemindService, EntryFillRemindService>();
            services.AddSingleton<IRemindService, SmartWorkingRemindService>();
            services.AddSingleton<IRemindServiceResolver, RemindServiceResolver>();

            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.AddSingleton<ClockifySetupDialog, ClockifySetupDialog>();

            IStorage storage = new MemoryStorage();
            if (_dataConnectionString != null && _containerName != null)
            {
                storage = new AzureBlobStorage(_dataConnectionString, _containerName);
            }

            services.AddSingleton(storage);
            services.AddSingleton<ConversationState>();
            services.AddSingleton<UserState>();
            services.AddSingleton<LuisRecognizerProxy>();
            services.AddSingleton<IBot, Supports.Bot>();
            services.AddSingleton<IAzureBlobReader, AzureBlobReader>();
            services.AddSingleton<IUserProfileStorageReader, UserProfileStorageReader>();
            services.AddSingleton<IUserProfilesProvider, UserProfilesProvider>();

            // Bot supports
            services.AddSingleton<IBotHandler, DicHandler>();
            services.AddSingleton<IBotHandler, ClockifyHandler>();
            services.AddSingleton<BotHandlerChain>();

            // Security
            services.AddSingleton<IProactiveApiKeyProvider, ProactiveApiKeyProvider>();
            services.AddSingleton<IProactiveBotApiKeyValidator, ProactiveBotApiKeyValidator>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}