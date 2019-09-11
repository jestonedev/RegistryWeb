using System;
using System.Collections.Generic;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Log
{
    public class LogOwnerProcessHelper
    {
        private RegistryContext registryContext;
        private SecurityService securityService;

        public LogOwnerProcessHelper(RegistryContext registryContext, SecurityService securityService)
        {
            this.registryContext = registryContext;
            this.securityService = securityService;
        }

        public void CreateLog(LogTypes logType, OwnerProcess newOwnerProcess, OwnerProcess oldOwnerProcess = null)
        {
            var time = DateTime.Now;
            var user = securityService.User;
            var idProcess = newOwnerProcess.IdProcess;
            if (logType == LogTypes.Create)
            {
                CreateLogOwnerProcess(idProcess, time, user, LogTypes.Create, LogObjects.OwnerProcess, newOwnerProcess);
                foreach (var oba in newOwnerProcess.OwnerBuildingsAssoc)
                {
                    CreateLogOwnerProcess(idProcess, time, user, LogTypes.Add, LogObjects.Address, oba);
                }
                foreach (var opa in newOwnerProcess.OwnerPremisesAssoc)
                {
                    CreateLogOwnerProcess(idProcess, time, user, LogTypes.Add, LogObjects.Address, opa);
                }
                foreach (var ospa in newOwnerProcess.OwnerSubPremisesAssoc)
                {
                    CreateLogOwnerProcess(idProcess, time, user, LogTypes.Add, LogObjects.Address, ospa);
                }
                foreach (var owner in newOwnerProcess.Owners)
                {
                    CreateLogOwnerProcess(idProcess, time, user, LogTypes.Create, LogObjects.Owner, owner);
                    foreach (var reason in owner.OwnerReasons)
                    {
                        CreateLogOwnerProcess(idProcess, time, user, LogTypes.Create, LogObjects.Reason, reason);
                    }
                }
            }
        }

        private void CreateLogOwnerProcess(int idProcess, DateTime time, AclUser user, LogTypes logType, LogObjects logObject, object newObject, object oldObject = null)
        {
            var log = new LogOwnerProcess();
            log.IdProcess = idProcess;
            log.Date = time;
            log.IdUser = user.IdUser;
            log.IdUserNavigation = user;
            log.IdLogObject = (int)logObject;
            log.IdLogType = (int)logType;
            log.LogOwnerProcessesValue = CreateLogOwnerProcessesValues(logObject, newObject, oldObject);
            registryContext.LogOwnerProcesses.Add(log);
            registryContext.SaveChanges();
        }

        private List<LogOwnerProcessValue> CreateLogOwnerProcessesValues(LogObjects logObject, object newObject, object oldObject)
        {
            var logValues = new List<LogOwnerProcessValue>();
            var logValue = new LogOwnerProcessValue();
            if (logObject == LogObjects.OwnerProcess)
            {
                var ownerProcess = newObject as OwnerProcess;
                logValue = new LogOwnerProcessValue();
                logValue.Talble = "owner_processes";
                logValue.IdKey = ownerProcess.IdProcess;
                logValue.Field = "annul_date";
                logValue.NewValue = ownerProcess.AnnulDate.ToString();
                logValue.OldValue = null;
                logValues.Add(logValue);
            }
            return logValues;
        }
    }
}
