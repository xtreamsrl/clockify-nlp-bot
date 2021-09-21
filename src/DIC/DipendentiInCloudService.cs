using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using NuGet.Packaging;

namespace Bot.DIC
{
    public class DipendentiInCloudService : IDipendentiInCloudService
    {
        private const string MaintainerTeamId = nameof(MaintainerTeamId);
        private const string MaintainerCompanyId = nameof(MaintainerCompanyId);

        private readonly IDipendentiInCloudClient _dicClient;
        
        public DipendentiInCloudService(IDipendentiInCloudClient dicClient)
        {
            _dicClient = dicClient;
        }

        public bool IsMaintainer(Employee employee)
        {
            int teamId = int.Parse(Environment.GetEnvironmentVariable(MaintainerTeamId) ?? "0");
            int companyId = int.Parse(Environment.GetEnvironmentVariable(MaintainerCompanyId) ?? "0");
            return employee.teams.Any(t => t.team.id == teamId) && employee.role.team.id == companyId;
        }
        
        public async Task<Employee> GetCurrentEmployeeAsync(string apiKey)
        {
            var response = await _dicClient.GetCompanyInfo(apiKey);
            if (!response.IsSuccessful) throw new ErrorResponseException("Unable to get current employee");
            return response.Data.data.access.employee;
        }

        public async Task<List<Employee>> GetAllEmployeesAsync(string apiKey)
        {
            var response = await _dicClient.GetEmployees(apiKey);
            if (!response.IsSuccessful) throw new ErrorResponseException("Unable to get employees");
            return response.Data.data;
        }

        public async Task SetRemoteWorkday(DateTime day, string apiKey, int employeeId)
        {
            await _dicClient.AddOrUpdateReason(day, day, apiKey, employeeId, 34);
        }

        public async Task DeleteRemoteWorkday(DateTime day, string apiKey, int employeeId)
        {
            await _dicClient.DeleteReasonIfPresent(day, day, apiKey, employeeId, 34);
        }

        public async Task<DicDay> GetTimesheetForDay(DateTime date, string apiKey, int employeeId)
        {
            var timesheetResponse =
                await _dicClient.GetTimesheet(date.Year, date.Month, date.Month, new List<int> {employeeId}, apiKey);
            return timesheetResponse.data.timesheet[employeeId.ToString()][date.ToString("yyyy-MM-dd")];
        }
        
        public async Task<IReadOnlyDictionary<string,Dictionary<string, DicDay>>> GetTimesheetBetweenDates
            (DateTime start, DateTime end, string apiKey, List<int> employees)
        {
            int yearDelta = end.Year - start.Year;
            if (yearDelta == 0)
            {
                var timesheetResponse =
                    await _dicClient.GetTimesheet(start.Year, start.Month, end.Month, employees, apiKey);
                return timesheetResponse.data.timesheet;
            }

            if (yearDelta < 0 || yearDelta > 1)
            {
                throw new Exception("Range not handled");
            }
            
            var timesheetResponse1 =
                await _dicClient.GetTimesheet(start.Year, start.Month, 12, employees, apiKey);
            var timesheetResponse2 =
                await _dicClient.GetTimesheet(end.Year, 1, end.Month, employees, apiKey);
            var result = timesheetResponse1.data.timesheet;
            foreach (var k in timesheetResponse2.data.timesheet.Keys)
            {
                result[k].AddRange(timesheetResponse2.data.timesheet[k]);
            }

            return result;
        }
    }
    
}