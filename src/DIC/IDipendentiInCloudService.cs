﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Models.DIC;

namespace Bot.DIC
{
    public interface IDipendentiInCloudService
    {
        public Task<Employee> GetCurrentEmployeeAsync(string apiKey);

        public Task SetRemoteWorkday(DateTime day, string apiKey, int employeeId);

        public Task DeleteRemoteWorkday(DateTime day, string apiKey, int employeeId);
        public Task<List<Employee>> GetAllEmployeesAsync(string apiKey);

        Task<DicDay> GetTimesheetForDay(DateTime date, string apiKey, int employeeId);
        
        public Task<IReadOnlyDictionary<string,Dictionary<string,DicDay>>> GetTimesheetBetweenDates(DateTime startDate, DateTime endDate, string apiKey, List<int> employees);
    }
}