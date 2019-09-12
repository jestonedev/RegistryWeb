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

        public void CreateLog(LogTypes logType, OwnerProcess ownerProcess, OwnerProcess oldOwnerProcess = null)
        {
            var time = DateTime.Now;
            var user = securityService.User;
            var idProcess = ownerProcess.IdProcess;

            var addressLogType = (logType == LogTypes.Create) ? LogTypes.Add : LogTypes.Edit;
            if (logType == LogTypes.Create)
            {
                
                CreateLogOwnerProcess(idProcess, time, user, LogTypes.Create, ownerProcess);
                foreach (var oba in ownerProcess.OwnerBuildingsAssoc)
                {
                    CreateLogOwnerProcess(idProcess, time, user, LogTypes.Add, oba);
                }
                foreach (var opa in ownerProcess.OwnerPremisesAssoc)
                {
                    CreateLogOwnerProcess(idProcess, time, user, LogTypes.Add, opa);
                }
                foreach (var ospa in ownerProcess.OwnerSubPremisesAssoc)
                {
                    CreateLogOwnerProcess(idProcess, time, user, LogTypes.Add, ospa);
                }
                //foreach (var owner in ownerProcess.Owners)
                //{
                //    CreateLogOwnerProcess(idProcess, time, user, LogTypes.Create, LogObjects.Owner, owner);
                //    foreach (var reason in owner.OwnerReasons)
                //    {
                //        CreateLogOwnerProcess(idProcess, time, user, LogTypes.Create, LogObjects.Reason, reason);
                //    }
                //}
            }
            if (logType == LogTypes.Edit)
            {

            }
            if (logType == LogTypes.Delete)
            {
                CreateLogOwnerProcess(idProcess, time, user, LogTypes.Delete, ownerProcess);
                foreach (var oba in ownerProcess.OwnerBuildingsAssoc)
                {
                    CreateLogOwnerProcess(idProcess, time, user, LogTypes.Delete, oba);
                }
                foreach (var opa in ownerProcess.OwnerPremisesAssoc)
                {
                    CreateLogOwnerProcess(idProcess, time, user, LogTypes.Delete, opa);
                }
                foreach (var ospa in ownerProcess.OwnerSubPremisesAssoc)
                {
                    CreateLogOwnerProcess(idProcess, time, user, LogTypes.Delete, ospa);
                }
            }
        }

        //OwnerProcess
        private void CreateLogOwnerProcess(int idProcess, DateTime time, AclUser user, LogTypes logType, OwnerProcess ownerProcess)
        {
            var log = new LogOwnerProcess();
            log.IdProcess = idProcess;
            log.Date = time;
            log.IdUser = user.IdUser;
            log.IdUserNavigation = user;
            log.IdLogObject = (int)LogObjects.OwnerProcess;
            log.IdLogType = (int)logType;
            log.Talble = "owner_processes";
            log.IdKey = idProcess;
            if (logType == LogTypes.Create || logType == LogTypes.Edit || logType == LogTypes.Annul)
                log.LogOwnerProcessesValue = CreateLogOwnerProcessesValues(ownerProcess);
            registryContext.LogOwnerProcesses.Add(log);
            registryContext.SaveChanges();
        }

        //Address
        private void CreateLogOwnerProcess(int idProcess, DateTime time, AclUser user, LogTypes logType, IAddressAssoc addressAssoc)
        {
            var log = new LogOwnerProcess();
            log.IdProcess = idProcess;
            log.Date = time;
            log.IdUser = user.IdUser;
            log.IdUserNavigation = user;
            log.IdLogObject = (int)LogObjects.Address;
            log.IdLogType = (int)logType;
            log.Talble = addressAssoc.GetTable();
            log.IdKey = addressAssoc.IdAssoc;
            if (logType == LogTypes.Add || logType == LogTypes.Edit)
                log.LogOwnerProcessesValue = CreateLogOwnerProcessesValues(addressAssoc);
            registryContext.LogOwnerProcesses.Add(log);
            registryContext.SaveChanges();
        }


        //OwnerProcess
        private List<LogOwnerProcessValue> CreateLogOwnerProcessesValues(OwnerProcess ownerProcess)
        {
            var logValues = new List<LogOwnerProcessValue>();

            var logValue = new LogOwnerProcessValue();
            logValue.Field = "annul_date";
            logValue.Value = ownerProcess.AnnulDate.ToString();
            logValues.Add(logValue);

            logValue = new LogOwnerProcessValue();
            logValue.Field = "annul_comment";
            logValue.Value = ownerProcess.AnnulComment;
            logValues.Add(logValue);

            logValue = new LogOwnerProcessValue();
            logValue.Field = "comment";
            logValue.Value = ownerProcess.Comment;
            logValues.Add(logValue);

            return logValues;
        }

        //Address
        private List<LogOwnerProcessValue> CreateLogOwnerProcessesValues(IAddressAssoc addressAssoc)
        {
            var logValues = new List<LogOwnerProcessValue>();

            var logValue = new LogOwnerProcessValue();
            logValue.Field = addressAssoc.GetFieldAdress();
            logValue.Value = addressAssoc.GetValueAddress().ToString();
            logValues.Add(logValue);

            logValue = new LogOwnerProcessValue();
            logValue.Field = "id_process";
            logValue.Value = addressAssoc.IdProcess.ToString();
            logValues.Add(logValue);

            return logValues;
        }
    }
}
