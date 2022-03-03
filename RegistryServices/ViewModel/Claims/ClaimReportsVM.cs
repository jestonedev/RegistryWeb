﻿using RegistryDb.Models.Entities;
using System;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.Claims
{
    public class ClaimReportsVM
    {
        public List<Executor> Executors { get; set; }
        public List<ClaimStateType> StateTypes { get; set; }
        public Executor CurrentExecutor { get; set; }
        public Dictionary<int, DateTime> MonthsList { get; set; }
    }
}
