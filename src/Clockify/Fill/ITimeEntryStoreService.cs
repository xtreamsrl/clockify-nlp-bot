﻿using System;
using System.Threading.Tasks;
using Bot.Clockify.Models;

namespace Bot.Clockify.Fill
{
    public interface ITimeEntryStoreService
    {
        public Task<double> AddTimeEntries(string clockifyToken, ProjectDo project, TaskDo? task, DateTime start, DateTime end);
    }
}