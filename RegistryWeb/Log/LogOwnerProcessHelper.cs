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

        public void CreateLog(LogTypes logType, OwnerProcess ownerProcess)
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
                foreach (var owner in ownerProcess.Owners)
                {
                    CreateLogOwnerProcess(idProcess, time, user, LogTypes.Create, owner);
                    foreach (var reason in owner.OwnerReasons)
                    {
                        CreateLogOwnerProcess(idProcess, time, user, LogTypes.Create, reason);
                    }
                }
            }
            if (logType == LogTypes.Edit)
            {
                CreateLogOwnerProcess(idProcess, time, user, LogTypes.Edit, ownerProcess);
                foreach (var oba in ownerProcess.OwnerBuildingsAssoc)
                {
                    if (oba.Deleted == 1)
                    {
                        CreateLogOwnerProcess(idProcess, time, user, LogTypes.Delete, oba);
                    }
                    else
                    {
                        CreateLogOwnerProcess(idProcess, time, user, LogTypes.Add, oba);
                    }
                }
                foreach (var opa in ownerProcess.OwnerPremisesAssoc)
                {
                    if (opa.Deleted == 1)
                    {
                        CreateLogOwnerProcess(idProcess, time, user, LogTypes.Delete, opa);
                    }
                    else
                    {
                        CreateLogOwnerProcess(idProcess, time, user, LogTypes.Add, opa);
                    }
                }
                foreach (var ospa in ownerProcess.OwnerSubPremisesAssoc)
                {
                    if (ospa.Deleted == 1)
                    {
                        CreateLogOwnerProcess(idProcess, time, user, LogTypes.Delete, ospa);
                    }
                    else
                    {
                        CreateLogOwnerProcess(idProcess, time, user, LogTypes.Add, ospa);
                    }
                }
                foreach (var owner in ownerProcess.Owners)
                {
                    if (owner.Deleted == 1)
                    {
                        CreateLogOwnerProcess(idProcess, time, user, LogTypes.Delete, owner);
                    }
                    else
                    {
                        CreateLogOwnerProcess(idProcess, time, user, LogTypes.Add, owner);
                    }
                }
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
                foreach (var owner in ownerProcess.Owners)
                {
                    CreateLogOwnerProcess(idProcess, time, user, LogTypes.Delete, owner);
                    foreach (var reason in owner.OwnerReasons)
                    {
                        CreateLogOwnerProcess(idProcess, time, user, LogTypes.Delete, reason);
                    }
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
            if (logType == LogTypes.Add || logType == LogTypes.Edit)
                log.LogOwnerProcessesValue = CreateLogOwnerProcessesValues(addressAssoc);
            registryContext.LogOwnerProcesses.Add(log);
            registryContext.SaveChanges();
        }

        //Owner
        private void CreateLogOwnerProcess(int idProcess, DateTime time, AclUser user, LogTypes logType, Owner owner)
        {
            var log = new LogOwnerProcess();
            log.IdProcess = idProcess;
            log.Date = time;
            log.IdUser = user.IdUser;
            log.IdUserNavigation = user;
            log.IdLogObject = (int)LogObjects.Owner;
            log.IdLogType = (int)logType;
            if (logType == LogTypes.Create || logType == LogTypes.Edit)
                log.LogOwnerProcessesValue = CreateLogOwnerProcessesValues(owner);
            registryContext.LogOwnerProcesses.Add(log);
            registryContext.SaveChanges();
        }

        //OwnerReason
        private void CreateLogOwnerProcess(int idProcess, DateTime time, AclUser user, LogTypes logType, OwnerReason reason)
        {
            var log = new LogOwnerProcess();
            log.IdProcess = idProcess;
            log.Date = time;
            log.IdUser = user.IdUser;
            log.IdUserNavigation = user;
            log.IdLogObject = (int)LogObjects.Reason;
            log.IdLogType = (int)logType;
            if (logType == LogTypes.Create || logType == LogTypes.Edit)
                log.LogOwnerProcessesValue = CreateLogOwnerProcessesValues(reason);
            registryContext.LogOwnerProcesses.Add(log);
            registryContext.SaveChanges();
        }


        //OwnerProcess
        private List<LogOwnerProcessValue> CreateLogOwnerProcessesValues(OwnerProcess ownerProcess)
        {
            var logValues = new List<LogOwnerProcessValue>();

            var logValue = new LogOwnerProcessValue();
            logValue.Table = "owner_processes";
            logValue.IdKey = ownerProcess.IdProcess;
            logValue.Field = "annul_date";
            logValue.Value = ownerProcess.AnnulDate.ToString();
            logValues.Add(logValue);

            logValue = new LogOwnerProcessValue();
            logValue.Table = "owner_processes";
            logValue.IdKey = ownerProcess.IdProcess;
            logValue.Field = "annul_comment";
            logValue.Value = ownerProcess.AnnulComment;
            logValues.Add(logValue);

            logValue = new LogOwnerProcessValue();
            logValue.Table = "owner_processes";
            logValue.IdKey = ownerProcess.IdProcess;
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
            logValue.Table = addressAssoc.GetTable();
            logValue.IdKey = addressAssoc.IdAssoc;
            logValue.Field = addressAssoc.GetFieldAdress();
            logValue.Value = addressAssoc.GetValueAddress().ToString();
            logValues.Add(logValue);

            logValue = new LogOwnerProcessValue();
            logValue.Table = addressAssoc.GetTable();
            logValue.IdKey = addressAssoc.IdAssoc;
            logValue.Field = "id_process";
            logValue.Value = addressAssoc.IdProcess.ToString();
            logValues.Add(logValue);

            return logValues;
        }

        //Owner
        private List<LogOwnerProcessValue> CreateLogOwnerProcessesValues(Owner owner)
        {
            var logValues = new List<LogOwnerProcessValue>();

            var logValue = new LogOwnerProcessValue();
            logValue.Table = "owners";
            logValue.IdKey = owner.IdOwner;
            logValue.Field = "id_owner_type";
            logValue.Value = owner.IdOwnerType.ToString();
            logValues.Add(logValue);

            if (owner.OwnerOrginfo != null)
            {
                logValue = new LogOwnerProcessValue();
                logValue.Table = "owner_orginfo";
                logValue.IdKey = owner.IdOwner;
                logValue.Field = "org_name";
                logValue.Value = owner.OwnerOrginfo.OrgName;
                logValues.Add(logValue);
            }

            if (owner.OwnerPerson != null)
            {
                logValue = new LogOwnerProcessValue();
                logValue.Table = "owner_persons";
                logValue.IdKey = owner.IdOwner;
                logValue.Field = "surnanme";
                logValue.Value = owner.OwnerPerson.Surname;
                logValues.Add(logValue);

                logValue = new LogOwnerProcessValue();
                logValue.Table = "owner_persons";
                logValue.IdKey = owner.IdOwner;
                logValue.Field = "name";
                logValue.Value = owner.OwnerPerson.Name;
                logValues.Add(logValue);

                logValue = new LogOwnerProcessValue();
                logValue.Table = "owner_persons";
                logValue.IdKey = owner.IdOwner;
                logValue.Field = "patronymic";
                logValue.Value = owner.OwnerPerson.Patronymic;
                logValues.Add(logValue);
            }

            return logValues;
        }

        //OwnerReason
        private List<LogOwnerProcessValue> CreateLogOwnerProcessesValues(OwnerReason reason)
        {
            var logValues = new List<LogOwnerProcessValue>();

            var logValue = new LogOwnerProcessValue();
            logValue.Table = "owner_reasons";
            logValue.IdKey = reason.IdReason;
            logValue.Field = "id_owner";
            logValue.Value = reason.IdOwner.ToString();
            logValues.Add(logValue);

            logValue = new LogOwnerProcessValue();
            logValue.Table = "owner_reasons";
            logValue.IdKey = reason.IdReason;
            logValue.Field = "numerator_share";
            logValue.Value = reason.NumeratorShare.ToString();
            logValues.Add(logValue);

            logValue = new LogOwnerProcessValue();
            logValue.Table = "owner_reasons";
            logValue.IdKey = reason.IdReason;
            logValue.Field = "denominator_share";
            logValue.Value = reason.DenominatorShare.ToString();
            logValues.Add(logValue);

            logValue = new LogOwnerProcessValue();
            logValue.Table = "owner_reasons";
            logValue.IdKey = reason.IdReason;
            logValue.Field = "id_reason_type";
            logValue.Value = reason.IdReasonType.ToString();
            logValues.Add(logValue);

            logValue = new LogOwnerProcessValue();
            logValue.Table = "owner_reasons";
            logValue.IdKey = reason.IdReason;
            logValue.Field = "reason_number";
            logValue.Value = reason.ReasonNumber;
            logValues.Add(logValue);

            logValue = new LogOwnerProcessValue();
            logValue.Table = "owner_reasons";
            logValue.IdKey = reason.IdReason;
            logValue.Field = "reason_date";
            logValue.Value = reason.ReasonDate.ToString();
            logValues.Add(logValue);

            return logValues;
        }
    }
}
