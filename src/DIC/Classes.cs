using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.DIC
{
    public class Access
    {
        public Employee employee { get; set; }
    }

    public class Employee : Id
    {
        public string? full_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }

        public string? number { get; set; }
        
        [JsonProperty(PropertyName = "main_role")]
        public MainRole role { get; set; }
        public List<TeamEmployeeAccess> teams { get; set; }
    }

    public class MainRole
    {
        public Team team { get; set; }
    }

    public class TeamEmployeeAccess
    {
        public Team team { get; set; }
    }

    public class Team : Id
    {
        public string name { get; set; }
    }

    public class CompanyInfoData
    {
        public Access access { get; set; }
    }

    public class TimesheetRequest
    {
        public bool all_day { get; set; }
        public DateTime date_from { get; set; }
        public DateTime date_to { get; set; }
        public List<int> employees { get; set; }
        public int reason_id { get; set; }
        public TimesheetView view { get; set; }
    }

    public class TimesheetView
    {
        public List<int> employees { get; set; }
        public int month_from { get; set; }
        public int month_to { get; set; }
        public int year { get; set; }
    }

    public class TimesheetResponse
    {
        public Dictionary<string, Dictionary<string, DicDay>> timesheet { get; set; }
    }

    public class DicDay
    {
        public bool closed { get; set; }
        public List<InsertedReason> reasons { get; set; }
    }

    public class InsertedReason
    {
        public Id reason { get; set; }
        public int? duration { get; set; }
    }

    public class Id
    {
        public int id { get; set; }
        public bool active { get; set; }
    }

    public class DipendentiInCloudDTO<T>
    {
        public T data { get; set; }
    }
}