using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Bot.Models.DIC;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NuGet.Packaging;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Bot.Services
{
    public class DipendentiInCloudService : IDipendentiInCloudService
    {
        public DipendentiInCloudService(DipendentiInCloudClient dicClient)
        {
            DicClient = dicClient;
        }

        private DipendentiInCloudClient DicClient { get; }

        public async Task<Employee> GetCurrentEmployeeAsync(string apiKey)
        {
            var response = await DicClient.GetCompanyInfo(apiKey);
            if (!response.IsSuccessful) throw new ErrorResponseException("Unable to get current employee");
            return response.Data.data.access.employee;
        }

        public async Task<List<Employee>> GetAllEmployeesAsync(string apiKey)
        {
            var response = await DicClient.GetEmployees(apiKey);
            if (!response.IsSuccessful) throw new ErrorResponseException("Unable to get employees");
            return response.Data.data;
        }

        public async Task SetRemoteWorkday(DateTime day, string apiKey, int employeeId)
        {
            await DicClient.AddOrUpdateReason(day, day, apiKey, employeeId, 34);
        }

        public async Task DeleteRemoteWorkday(DateTime day, string apiKey, int employeeId)
        {
            await DicClient.DeleteReasonIfPresent(day, day, apiKey, employeeId, 34);
        }

        public async Task<DicDay> GetTimesheetForDay(DateTime date, string apiKey, int employeeId)
        {
            var timesheetResponse =
                await DicClient.GetTimesheet(date.Year, date.Month, date.Month, new List<int> {employeeId}, apiKey);
            return timesheetResponse.data.timesheet[employeeId.ToString()][date.ToString("yyyy-MM-dd")];
        }
        
        public async Task<IReadOnlyDictionary<string,Dictionary<string, DicDay>>> GetTimesheetBetweenDates
            (DateTime start, DateTime end, string apiKey, List<int> employees)
        {
            int yearDelta = end.Year - start.Year;
            if (yearDelta == 0)
            {
                var timesheetResponse =
                    await DicClient.GetTimesheet(start.Year, start.Month, end.Month, employees, apiKey);
                return timesheetResponse.data.timesheet;
            }

            if (yearDelta < 0 || yearDelta > 1)
            {
                throw new Exception("Range not handled");
            }
            
            var timesheetResponse1 =
                await DicClient.GetTimesheet(start.Year, start.Month, 12, employees, apiKey);
            var timesheetResponse2 =
                await DicClient.GetTimesheet(end.Year, 1, end.Month, employees, apiKey);
            var result = timesheetResponse1.data.timesheet;
            foreach (var k in timesheetResponse2.data.timesheet.Keys)
            {
                result[k].AddRange(timesheetResponse2.data.timesheet[k]);
            }

            return result;
        }
    }

    public class DipendentiInCloudClient
    {
        private IRestClient _client = null!;

        public DipendentiInCloudClient() => InitClient();

        private void InitClient()
        {
            _client = new RestClient("https://secure.dipendentincloud.it/backend_apiV2");
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter(),
                    new IsoDateTimeConverter
                    {
                        DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"
                    }
                },
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            _client.UseNewtonsoftJson(settings);
        }

        public async Task<IRestResponse<DipendentiInCloudDTO<CompanyInfoData>>> GetCompanyInfo(string apiKey) =>
            await _client.ExecuteGetAsync<DipendentiInCloudDTO<CompanyInfoData>>(
                Authorized(new RestRequest("company/info"), apiKey));

        private IRestRequest Authorized(IRestRequest request, string apiKey)
        {
            return request.AddHeader("authorization", apiKey);
        }

        public async Task AddOrUpdateReason(DateTime dateFrom, DateTime dateTo, string apiKey, int employeeId,
            int reasonId)
        {
            DipendentiInCloudDTO<TimesheetRequest> dto = GetTimeSheetDto(dateFrom, dateTo, employeeId, reasonId);
            RestRequest addRequest = new RestRequest("timesheet", Method.POST);
            addRequest.AddJsonBody(dto);
            var response = await _client.ExecuteAsync(Authorized(addRequest, apiKey), Method.POST);
            if (!response.IsSuccessful) throw new Exception("Error while adding a reason to DIC");
        }

        public async Task DeleteReasonIfPresent(DateTime dateFrom, DateTime dateTo, string apiKey, int employeeId,
            int reasonId)
        {
            var dto = GetTimeSheetDto(dateFrom, dateTo, employeeId, reasonId);
            RestRequest deleteRequest = new RestRequest("timesheet/delete", Method.POST);
            deleteRequest.AddJsonBody(dto);
            var response = await _client.ExecuteAsync(Authorized(deleteRequest, apiKey), Method.POST);
            if (!response.IsSuccessful) throw new Exception("Error while removing a reason from DIC");
        }

        private static DipendentiInCloudDTO<TimesheetRequest> GetTimeSheetDto(DateTime dateFrom, DateTime dateTo,
            int employeeId, int reasonId)
        {
            DipendentiInCloudDTO<TimesheetRequest> dto = new DipendentiInCloudDTO<TimesheetRequest>()
            {
                data = new TimesheetRequest
                {
                    all_day = true,
                    date_from = dateFrom,
                    date_to = dateTo,
                    employees = new List<int> {employeeId},
                    reason_id = reasonId,
                    view = new TimesheetView
                    {
                        employees = new List<int> {employeeId},
                        year = dateFrom.Year,
                        month_from = dateFrom.Month,
                        month_to = dateTo.Month
                    }
                }
            };
            return dto;
        }

        public async Task<DipendentiInCloudDTO<TimesheetResponse>> GetTimesheet(int year, int monthFrom, int monthTo,
            IEnumerable<int> employees, string apiKey)
        {
            var request = new RestRequest("timesheet", Method.GET)
                .AddParameter("year", year)
                .AddParameter("month_from", monthFrom)
                .AddParameter("month_to", monthTo)
                .AddParameter("employees", string.Join(",", employees));
            request = Authorized(request, apiKey);
            var response = await _client.ExecuteGetAsync<DipendentiInCloudDTO<TimesheetResponse>>(request);
            if (response.IsSuccessful)
            {
                return response.Data;
            }

            throw new Exception("Error while fetching timesheet from DIC");
        }

        public async Task<IRestResponse<DipendentiInCloudDTO<List<Employee>>>> GetEmployees(string apiKey) =>
            await _client.ExecuteGetAsync<DipendentiInCloudDTO<List<Employee>>>(Authorized(new RestRequest("employees"),
                apiKey));
    }
}