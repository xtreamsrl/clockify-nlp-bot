using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;

namespace Bot.DIC
{
    public interface IDipendentiInCloudClient
    {
        Task<IRestResponse<DipendentiInCloudDTO<CompanyInfoData>>> GetCompanyInfo(string apiKey);

        Task AddOrUpdateReason(DateTime dateFrom, DateTime dateTo, string apiKey, int employeeId, int reasonId);

        Task DeleteReasonIfPresent(DateTime dateFrom, DateTime dateTo, string apiKey, int employeeId, int reasonId);

        Task<DipendentiInCloudDTO<TimesheetResponse>> GetTimesheet(int year, int monthFrom, int monthTo,
            IEnumerable<int> employees, string apiKey);

        Task<IRestResponse<DipendentiInCloudDTO<List<Employee>>>> GetEmployees(string apiKey);
    }
}