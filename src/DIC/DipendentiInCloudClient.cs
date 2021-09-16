using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Bot.DIC
{
    public class DipendentiInCloudClient : IDipendentiInCloudClient
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

        public async Task<IRestResponse<DipendentiInCloudDTO<List<Employee>>>> GetEmployees(string apiKey)
        {
            var employeeReq = new RestRequest("employees").AddParameter("permission", "timesheet_read");
            return await _client.ExecuteGetAsync<DipendentiInCloudDTO<List<Employee>>>(Authorized(employeeReq, apiKey));
        }

        private IRestRequest Authorized(IRestRequest request, string apiKey)
        {
            return request.AddHeader("authorization", apiKey);
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
    }
    
}