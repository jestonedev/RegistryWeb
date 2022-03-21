using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RegistryWeb.DataHelpers;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class ClaimStatesController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        private readonly IConfiguration config;

        public ClaimStatesController(SecurityService securityService, RegistryContext registryContext, IConfiguration config)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.config = config;
        }

        [HttpPost]
        public int DeleteClaimState(int? idState)
        {
            if (idState == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return -2;
            try
            {
                var claimState = registryContext.ClaimStates
                    .FirstOrDefault(op => op.IdState == idState);

                var claimStates = registryContext.ClaimStates.Where(cs => cs.IdClaim == claimState.IdClaim).AsNoTracking().ToList();
                if(claimStates[claimStates.Count - 1].IdState != claimState.IdState)
                {
                    return -4;
                }
                claimState.Deleted = 1;

                if (claimState.IdStateType == 4)
                {
                    var claimCourtOrders = registryContext.ClaimCourtOrders.Where(r => r.IdClaim == claimState.IdClaim);
                    foreach (var courtOrder in claimCourtOrders)
                    {
                        courtOrder.Deleted = 1;
                    }
                }

                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult GetClaimState(int? idState)
        {
            if (idState == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return Json(-2);
            var claimState = registryContext.ClaimStates
                .FirstOrDefault(op => op.IdState == idState);
            if (claimState == null)
                return Json(-3);
            var courtOrders = claimState.IdStateType == 4 ? registryContext.ClaimCourtOrders.Where(r => r.IdClaim == claimState.IdClaim).ToList()
                : new List<ClaimCourtOrder>();
            return Json(new {
                idStateType = claimState.IdStateType,
                dateStartState = claimState.DateStartState.HasValue ? claimState.DateStartState.Value.ToString("yyyy-MM-dd") : null,
                description = claimState.Description,
                executor = claimState.Executor,
                bksRequester = claimState.BksRequester,
                transferToLegalDepartmentDate = claimState.TransferToLegalDepartmentDate.HasValue ? claimState.TransferToLegalDepartmentDate.Value.ToString("yyyy-MM-dd") : null,
                transferToLegalDepartmentWho = claimState.TransferToLegalDepartmentWho,
                acceptedByLegalDepartmentDate = claimState.AcceptedByLegalDepartmentDate.HasValue ? claimState.AcceptedByLegalDepartmentDate.Value.ToString("yyyy-MM-dd") : null,
                acceptedByLegalDepartmentWho = claimState.AcceptedByLegalDepartmentWho,
                claimDirectionDate = claimState.ClaimDirectionDate.HasValue ? claimState.ClaimDirectionDate.Value.ToString("yyyy-MM-dd") : null,
                claimDirectionDescription = claimState.ClaimDirectionDescription,
                courtOrderDate = claimState.CourtOrderDate.HasValue ? claimState.CourtOrderDate.Value.ToString("yyyy-MM-dd") : null,
                courtOrderNum = claimState.CourtOrderNum,
                obtainingCourtOrderDate = claimState.ObtainingCourtOrderDate.HasValue ? claimState.ObtainingCourtOrderDate.Value.ToString("yyyy-MM-dd") : null,
                obtainingCourtOrderDescription = claimState.ObtainingCourtOrderDescription,
                directionCourtOrderBailiffsDate = claimState.DirectionCourtOrderBailiffsDate.HasValue ? claimState.DirectionCourtOrderBailiffsDate.Value.ToString("yyyy-MM-dd") : null,
                directionCourtOrderBailiffsDescription = claimState.DirectionCourtOrderBailiffsDescription,
                enforcementProceedingStartDate = claimState.EnforcementProceedingStartDate.HasValue ? claimState.EnforcementProceedingStartDate.Value.ToString("yyyy-MM-dd") : null,
                enforcementProceedingStartDescription = claimState.EnforcementProceedingStartDescription,
                enforcementProceedingEndDate = claimState.EnforcementProceedingEndDate.HasValue ? claimState.EnforcementProceedingEndDate.Value.ToString("yyyy-MM-dd") : null,
                enforcementProceedingEndDescription = claimState.EnforcementProceedingEndDescription,
                enforcementProceedingTerminateDate = claimState.EnforcementProceedingTerminateDate.HasValue ? claimState.EnforcementProceedingTerminateDate.Value.ToString("yyyy-MM-dd") : null,
                enforcementProceedingTerminateDescription = claimState.EnforcementProceedingTerminateDescription,
                repeatedDirectionCourtOrderBailiffsDate = claimState.RepeatedDirectionCourtOrderBailiffsDate.HasValue ? claimState.RepeatedDirectionCourtOrderBailiffsDate.Value.ToString("yyyy-MM-dd") : null,
                repeatedDirectionCourtOrderBailiffsDescription = claimState.RepeatedDirectionCourtOrderBailiffsDescription,
                repeatedEnforcementProceedingStartDate = claimState.RepeatedEnforcementProceedingStartDate.HasValue ? claimState.RepeatedEnforcementProceedingStartDate.Value.ToString("yyyy-MM-dd") : null,
                repeatedEnforcementProceedingStartDescription = claimState.RepeatedEnforcementProceedingStartDescription,
                repeatedEnforcementProceedingEndDate = claimState.RepeatedEnforcementProceedingEndDate.HasValue ? claimState.RepeatedEnforcementProceedingEndDate.Value.ToString("yyyy-MM-dd") : null,
                repeatedEnforcementProceedingEndDescription = claimState.RepeatedEnforcementProceedingEndDescription,
                courtOrderCompleteDate = claimState.CourtOrderCompleteDate.HasValue ? claimState.CourtOrderCompleteDate.Value.ToString("yyyy-MM-dd") : null,
                courtOrderCompleteReason = claimState.CourtOrderCompleteReason,
                courtOrderCompleteDescription = claimState.CourtOrderCompleteDescription,
                courtOrders = courtOrders.Select(r => new {
                    idOrder = r.IdOrder,
                    idSigner = r.IdSigner,
                    idJudge = r.IdJudge,
                    idExecutor = r.IdExecutor,
                    openAccountDate = r.OpenAccountDate.ToString("yyyy-MM-dd"),
                    amountTenancy = r.AmountTenancy,
                    amountPenalties = r.AmountPenalties,
                    amountDgi = r.AmountDgi,
                    amountPadun = r.AmountPadun,
                    amountPkk = r.AmountPkk,
                    startDeptPeriod = r.StartDeptPeriod.HasValue ? r.StartDeptPeriod.Value.ToString("yyyy-MM-dd") : null,
                    endDeptPeriod = r.EndDeptPeriod.HasValue ? r.EndDeptPeriod.Value.ToString("yyyy-MM-dd") : null,
                    createDate = r.CreateDate.HasValue ? r.CreateDate.Value.ToString("yyyy-MM-dd") : null,
                    orderDate = r.OrderDate.ToString("yyyy-MM-dd")
                })
            });
        }

        [HttpPost]
        public IActionResult SaveClaimState(ClaimState claimState, List<ClaimCourtOrder> courtOrders)
        {
            if (claimState == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return Json(new { Error = -2 });

            var claimStates = registryContext.ClaimStates.Where(cs => cs.IdClaim == claimState.IdClaim).AsNoTracking().ToList();
            var claimStateTypeRelations = registryContext.ClaimStateTypeRelations.AsNoTracking().ToList();
            var claimStateTypes = registryContext.ClaimStateTypes.AsNoTracking().ToList();
            ClaimState prevClaimState = null;
            ClaimState nextClaimState = null;
            //Создать
            if (claimState.IdState == 0)
            {
                //Проверяем целостность графа переходов при создании
                if (claimStates.Count > 0)
                {
                    prevClaimState = claimStates[claimStates.Count - 1];
                }
                if ((prevClaimState == null && 
                    !claimStateTypes.Any(r => r.IsStartStateType && r.IdStateType == claimState.IdStateType)) ||
                    (prevClaimState != null &&
                    !claimStateTypeRelations.Any(r => r.IdStateFrom == prevClaimState.IdStateType && r.IdStateTo == claimState.IdStateType)))
                {
                    return Json(new { Error = -3 });
                }

                registryContext.ClaimStates.Add(claimState);
                if (claimState.IdStateType == 4)
                    UpdateCourtOrders(courtOrders, claimState.IdClaim);

                registryContext.SaveChanges();

                return Json(new { claimState });
            }
            //Обновить    
            //Проверяем целостность графа переходов при обновлении
            for (var i = 0; i < claimStates.Count; i++)
            {
                if (claimStates[i].IdState == claimState.IdState)
                {
                    if (i > 0)
                    {
                        prevClaimState = claimStates[i - 1];
                    }
                    if (i < claimStates.Count - 1)
                    {
                        nextClaimState = claimStates[i + 1];
                    }
                    break;
                }
            }

            if ((prevClaimState == null &&
                !claimStateTypes.Any(r => r.IsStartStateType && r.IdStateType == claimState.IdStateType)) ||
                (prevClaimState != null &&
                !claimStateTypeRelations.Any(r => r.IdStateFrom == prevClaimState.IdStateType && r.IdStateTo == claimState.IdStateType)) ||
                (nextClaimState != null &&
                !claimStateTypeRelations.Any(r => r.IdStateTo == nextClaimState.IdStateType && r.IdStateFrom == claimState.IdStateType)))
            {
                return Json(new { Error = -3 });
            }

            registryContext.ClaimStates.Update(claimState);
            if (claimState.IdStateType == 4)
                UpdateCourtOrders(courtOrders, claimState.IdClaim);

            registryContext.SaveChanges();
            return Json(new { claimState });
        }

        private void UpdateCourtOrders(List<ClaimCourtOrder> newCourtOrders, int idClaim)
        {
            var oldCourtOrders = registryContext.ClaimCourtOrders.Where(r => r.IdClaim == idClaim);
            foreach(var oldCourtOrder in oldCourtOrders)
            {
                if (!newCourtOrders.Any(r => r.IdOrder == oldCourtOrder.IdOrder))
                {
                    oldCourtOrder.Deleted = 1;
                }
            }
            foreach(var newCourtOrder in newCourtOrders)
            {
                var oldCourtOrder = oldCourtOrders.FirstOrDefault(r => r.IdOrder == newCourtOrder.IdOrder);
                if (oldCourtOrder != null)
                {
                    oldCourtOrder.IdExecutor = newCourtOrder.IdExecutor;
                    oldCourtOrder.CreateDate = newCourtOrder.CreateDate;
                    oldCourtOrder.IdSigner = newCourtOrder.IdSigner;
                    oldCourtOrder.IdJudge = newCourtOrder.IdJudge;
                    oldCourtOrder.OrderDate = newCourtOrder.OrderDate;
                    oldCourtOrder.OpenAccountDate = newCourtOrder.OpenAccountDate;
                    oldCourtOrder.AmountTenancy = newCourtOrder.AmountTenancy;
                    oldCourtOrder.AmountPenalties = newCourtOrder.AmountPenalties;
                    oldCourtOrder.AmountDgi = newCourtOrder.AmountDgi;
                    oldCourtOrder.AmountPadun = newCourtOrder.AmountPadun;
                    oldCourtOrder.AmountPkk = newCourtOrder.AmountPkk;
                    oldCourtOrder.StartDeptPeriod = newCourtOrder.StartDeptPeriod;
                    oldCourtOrder.EndDeptPeriod = newCourtOrder.EndDeptPeriod;
                    registryContext.ClaimCourtOrders.Update(oldCourtOrder);
                } else
                {
                    registryContext.ClaimCourtOrders.Add(newCourtOrder);
                }
            }
        }
    }
}