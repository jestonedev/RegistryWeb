﻿using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Claims;
using RegistryDb.Models.Entities.Common;
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
