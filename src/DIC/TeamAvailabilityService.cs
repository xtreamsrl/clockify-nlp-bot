using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveCards;
using Bot.Models.DIC;
using Bot.States;
using FluentDateTime;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.DIC
{
    public class TeamAvailabilityService
    {
        private readonly IDipendentiInCloudService _dipendentiInCloudService;

        public TeamAvailabilityService(IDipendentiInCloudService dipendentiInCloudService)
        {
            _dipendentiInCloudService = dipendentiInCloudService;
        }

        public async Task<Attachment> CreateAvailabilityReportAsync(UserProfile profile)
        {
            var startDate = DateTime.Today;
            var employees = await _dipendentiInCloudService.GetAllEmployeesAsync(profile.DicToken!);
            var timesheet = await _dipendentiInCloudService.GetTimesheetBetweenDates(startDate, startDate.AddDays(7),
                profile.DicToken!, employees.Select(e => e.id).ToList());
            var interestingEmployees =
                employees.Where(e => timesheet.Keys.Contains(e.id.ToString()) && e.id != profile.EmployeeId)
                    .OrderBy(e => e.number ?? e.full_name).ToList();
            var leftColumn = new AdaptiveColumn
            {
                Width = AdaptiveColumnWidth.Auto,
                Height = AdaptiveHeight.Stretch,
                VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center,
                Items =
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = "Name"
                    }
                }
            };
            leftColumn.Items.AddRange(interestingEmployees
                .Select(e => e.number ?? e.full_name)
                .Select(name => new AdaptiveTextBlock
                {
                    Text = name,
                    Separator = true
                }));
            var otherColumns = new List<int> {0, 1, 2, 3, 4}
                .Select(i => startDate.AddBusinessDays(i))
                .Select(day => CreateDayColumn(day, timesheet, interestingEmployees));
            var allColumns = new List<AdaptiveColumn> {leftColumn};
            allColumns.AddRange(otherColumns);
            var card = new AdaptiveCard("1.2")
            {
                Body =
                {
                    new AdaptiveContainer
                    {
                        Items =
                        {
                            new AdaptiveTextBlock
                            {
                                Text = "Team availability - next week",
                                Color = AdaptiveTextColor.Accent,
                                Weight = AdaptiveTextWeight.Bolder,
                                Size = AdaptiveTextSize.Medium,
                                Wrap = true
                            },
                            new AdaptiveColumnSet
                            {
                                Columns = allColumns
                            }
                        }
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Url = new Uri("https://secure.dipendentincloud.it/app/timesheet"),
                        Title = "See more on Dipendenti in Cloud"
                    }
                }
            };
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(card))
            };
        }

        private static AdaptiveColumn CreateDayColumn(DateTime day,
            IReadOnlyDictionary<string, Dictionary<string, DicDay>> timesheet,
            IEnumerable<Employee> interestingEmployees)
        {
            var column = new AdaptiveColumn
            {
                Width = AdaptiveColumnWidth.Stretch,
                Height = AdaptiveHeight.Stretch,
                VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center,
                Items =
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = day.ToString("dd/MM")
                    }
                }
            };
            column.Items.AddRange(interestingEmployees
                .Select(e => e.id)
                .Select(id =>
                {
                    var dayString = day.ToString("yyyy-MM-dd");
                    var dicDay = timesheet[id.ToString()][dayString];
                    int onLeave = dicDay.reasons
                        .Where(r => r.reason.id != 34)
                        .Select(r => r.duration ?? 0)
                        .Sum();
                    bool remote = dicDay.reasons.Any(r => r.reason.id == 34);
                    string summary = "";
                    if (dicDay.closed)
                    {
                        summary = "❌";
                    }
                    else if (onLeave >= 8 * 60)
                    {
                        summary = "🌴";
                    }
                    else
                    {
                        summary = remote ? "🏠" : "💻";
                        if (onLeave > 0)
                        {
                            summary += $"+{onLeave / 60}h❌";
                        }
                    }

                    return new AdaptiveTextBlock
                    {
                        Text = summary,
                        Separator = true
                    };
                }));
            return column;
        }
    }
}