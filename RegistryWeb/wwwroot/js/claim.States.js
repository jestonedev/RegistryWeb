function addClaimState(e) {
    let action = $('#ClaimStates').data('action');
    let idClaim = $('#ClaimStates').data('id');

    $.ajax({
        type: 'POST',
        url: window.location.origin + '/Claims/AddClaimState',
        data: { action, idClaim },
        success: function (elem) {
            let list = $('#ClaimStates');
            list.children(".rr-list-group-item-empty").hide();
            let claimToggle = $('#ClaimStatesForm .claim-toggler');
            if (!isExpandElemntArrow(claimToggle)) // развернуть при добавлении, если было свернуто 
                claimToggle.click();
            list.append(elem);
            elem = list.children(".list-group-item").last();
            elem.find("select").selectpicker("refresh");
            elem.find(".claim-state-edit-btn").first().click();

            var idStateTypeElem = elem.find("select[name^='IdStateType']");
            idStateTypeElem.change();

            $([document.documentElement, document.body]).animate({
                scrollTop: $(elem).offset().top
            }, 1000);
            refreshValidationForm($("#ClaimStatesForm"));
        }
    });
    e.preventDefault();
}

var claimStateDetailsBtnClosed = true;
let courtOrderTemplateForCancelation = undefined;

function editClaimState(e) {
    disableEditingMultiyClaimStates(true);
    let claimStateElem = $(this).closest(".list-group-item");
    let fields = claimStateElem.find('input, select, textarea').filter(function (idx, elem) {
        return !/^Executor_/.test($(elem).prop("name")) && $(elem).closest("[id^='ClaimCourtOrders']").length === 0;
    });
    let yesNoPanel = claimStateElem.find('.yes-no-panel');
    let editDelPanel = claimStateElem.find('.edit-del-panel');
    fields.prop('disabled', false);

    if (claimStateElem.next().length !== 0) {
        let claimStateTypeElem = claimStateElem.find("select[name^='IdStateType']");
        claimStateTypeElem.prop('disabled', true);
    }

    claimStateElem.find("select").selectpicker('refresh');
    editDelPanel.hide();
    yesNoPanel.show();

    var claimStateDetailsBtn = claimStateElem.find(".rr-claim-state-details");
    var icon = $(claimStateDetailsBtn).find(".oi");
    if (icon.hasClass("oi-chevron-bottom")) {
        claimStateDetailsBtn.click();
        claimStateDetailsBtnClosed = true;
    } else {
        claimStateDetailsBtnClosed = false;
    }

    claimStateElem.find("[id^='claimCourtOrderAdd']").removeClass("disabled");
    claimStateElem.find(".edit-del-court-order-panel a").removeClass("disabled");

    let courtOrderElems = claimStateElem.find("[id^='ClaimCourtOrders_'] > .list-group-item").filter(function (idx, elem) {
        return !$(elem).hasClass("rr-list-group-item-empty");
    });
    if (courtOrderElems.length > 0) {
        courtOrderTemplateForCancelation = courtOrderElems.first().clone();
    } else {
        courtOrderTemplateForCancelation = undefined;
    }

    e.preventDefault();
}

function cancelEditClaimState(e) {
    let claimStateElem = $(this).closest(".list-group-item");
    let idState = claimStateElem.find("input[name^='IdState']").val();
    //Отменить изменения внесенные в документ
    if (idState !== "0") {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/ClaimStates/GetClaimState',
            data: { idState: idState },
            success: function (claimState) {
                refreshClaimState(claimStateElem, claimState);
                showEditDelPanelClaimState(claimStateElem);
                clearValidationError(claimStateElem);
                claimStateElem.find("[id^='claimCourtOrderAdd']").addClass("disabled");
                claimStateElem.find(".edit-del-court-order-panel a").addClass("disabled");
                if (claimStateDetailsBtnClosed) {
                    var claimStateDetailsBtn = claimStateElem.find(".rr-claim-state-details");
                    var icon = $(claimStateDetailsBtn).find(".oi");
                    if (icon.hasClass("oi-chevron-top")) {
                        claimStateDetailsBtn.click();
                    }
                }
            }
        });
    }
    //Отменить вставку нового документа
    else {
        claimStateElem.remove();
        if ($("#ClaimStates > .list-group-item").length === 1) {
            $("#ClaimStates > .rr-list-group-item-empty").show();
        }
        disableEditingMultiyClaimStates(false);
    }
    e.preventDefault();
}

function disableEditingMultiyClaimStates(disable) {
    let addClaimStateBtn = $("#claimStateAdd");
    let claimStatesEditBtns = $("#ClaimStates > .list-group-item").find(".claim-state-edit-btn");
    let claimStatesDelBtns = $("#ClaimStates > .list-group-item").find(".claim-state-delete-btn");
    if (disable) {
        addClaimStateBtn.addClass("disabled");
        claimStatesEditBtns.addClass("disabled");
        claimStatesDelBtns.addClass("disabled");
    } else {
        addClaimStateBtn.removeClass("disabled");
        claimStatesEditBtns.removeClass("disabled");
        $(claimStatesDelBtns[claimStatesDelBtns.length - 1]).removeClass("disabled");
        $(claimStatesDelBtns[claimStatesDelBtns.length - 2]).removeClass("disabled");
    }
}

function refreshClaimState(claimStateElem, claimState) {
    claimStateElem.find("[name^='IdStateType']").val(claimState.idStateType).selectpicker('refresh').change();
    claimStateElem.find("[name^='DateStartState']").val(claimState.dateStartState);
    claimStateElem.find("[name^='Description']").val(claimState.description);
    claimStateElem.find("[name^='Executor']").val(claimState.executor);
    claimStateElem.find("[name^='BksRequester']").val(claimState.bksRequester);
    claimStateElem.find("[name^='TransferToLegalDepartmentDate']").val(claimState.transferToLegalDepartmentDate);
    claimStateElem.find("[name^='TransferToLegalDepartmentWho']").val(claimState.transferToLegalDepartmentWho);
    claimStateElem.find("[name^='AcceptedByLegalDepartmentDate']").val(claimState.acceptedByLegalDepartmentDate);
    claimStateElem.find("[name^='AcceptedByLegalDepartmentWho']").val(claimState.acceptedByLegalDepartmentWho);
    claimStateElem.find("[name^='ClaimDirectionDate']").val(claimState.claimDirectionDate);
    claimStateElem.find("[name^='ClaimDirectionDescription']").val(claimState.claimDirectionDescription);
    claimStateElem.find("[name^='CourtOrderDate']").val(claimState.courtOrderDate);
    claimStateElem.find("[name^='CourtOrderNum']").val(claimState.courtOrderNum);
    claimStateElem.find("[name^='ObtainingCourtOrderDate']").val(claimState.obtainingCourtOrderDate);
    claimStateElem.find("[name^='ObtainingCourtOrderDescription']").val(claimState.obtainingCourtOrderDescription);
    claimStateElem.find("[name^='DirectionCourtOrderBailiffsDate']").val(claimState.directionCourtOrderBailiffsDate);
    claimStateElem.find("[name^='DirectionCourtOrderBailiffsDescription']").val(claimState.directionCourtOrderBailiffsDescription);
    claimStateElem.find("[name^='EnforcementProceedingStartDate']").val(claimState.enforcementProceedingStartDate);
    claimStateElem.find("[name^='EnforcementProceedingStartDescription']").val(claimState.enforcementProceedingStartDescription);
    claimStateElem.find("[name^='EnforcementProceedingEndDate']").val(claimState.enforcementProceedingEndDate);
    claimStateElem.find("[name^='EnforcementProceedingEndDescription']").val(claimState.enforcementProceedingEndDescription);
    claimStateElem.find("[name^='EnforcementProceedingTerminateDate']").val(claimState.enforcementProceedingTerminateDate);
    claimStateElem.find("[name^='EnforcementProceedingTerminateDescription']").val(claimState.enforcementProceedingTerminateDescription);
    claimStateElem.find("[name^='RepeatedDirectionCourtOrderBailiffsDate']").val(claimState.repeatedDirectionCourtOrderBailiffsDate);
    claimStateElem.find("[name^='RepeatedDirectionCourtOrderBailiffsDescription']").val(claimState.repeatedDirectionCourtOrderBailiffsDescription);
    claimStateElem.find("[name^='RepeatedEnforcementProceedingStartDate']").val(claimState.repeatedEnforcementProceedingStartDate);
    claimStateElem.find("[name^='RepeatedEnforcementProceedingStartDescription']").val(claimState.repeatedEnforcementProceedingStartDescription);
    claimStateElem.find("[name^='RepeatedEnforcementProceedingEndDate']").val(claimState.repeatedEnforcementProceedingEndDate);
    claimStateElem.find("[name^='RepeatedEnforcementProceedingEndDescription']").val(claimState.repeatedEnforcementProceedingEndDescription);
    claimStateElem.find("[name^='CourtOrderCompleteDate']").val(claimState.courtOrderCompleteDate);
    claimStateElem.find("[name^='CourtOrderCompleteReason']").val(claimState.courtOrderCompleteReason);
    claimStateElem.find("[name^='CourtOrderCompleteDescription']").val(claimState.courtOrderCompleteDescription);

    var courtOrderListElem = claimStateElem.find("[id^='ClaimCourtOrders_']");
    courtOrderListElem.empty();
    if (courtOrderTemplateForCancelation !== undefined) {
        for (let i = 0; i < claimState.courtOrders.length; i++) {
            let courtOrder = claimState.courtOrders[i];
            let courtOrderElem = courtOrderTemplateForCancelation.clone();
            courtOrderListElem.append(courtOrderElem);
            courtOrderElem = courtOrderListElem.find(".list-group-item").last();
            courtOrderElem.find("[name^='IdOrder']").val(courtOrder.idOrder);
            courtOrderElem.find("[name^='IdSigner']").val(courtOrder.idSigner);
            courtOrderElem.find("[name^='IdJudge']").val(courtOrder.idJudge);
            courtOrderElem.find("[name^='OpenAccountDate']").val(courtOrder.openAccountDate);
            courtOrderElem.find("[name^='AmountTenancy']").val(courtOrder.amountTenancy);
            courtOrderElem.find("[name^='AmountPenalties']").val(courtOrder.amountPenalties);
            courtOrderElem.find("[name^='AmountDgi']").val(courtOrder.amountDgi);
            courtOrderElem.find("[name^='AmountPadun']").val(courtOrder.amountPadun);
            courtOrderElem.find("[name^='AmountPkk']").val(courtOrder.amountPkk);
            courtOrderElem.find("[name^='StartDeptPeriod']").val(courtOrder.startDeptPeriod);
            courtOrderElem.find("[name^='EndDeptPeriod']").val(courtOrder.endDeptPeriod);
            courtOrderElem.find("[name^='CreateDate']").val(courtOrder.createDate);
            courtOrderElem.find("[name^='OrderDate']").val(courtOrder.orderDate);

            var idExecutorElem = courtOrderElem.find('select[name^="IdExecutor"]');
            var formGroup = idExecutorElem.closest('.form-group');
            idExecutorElem.closest('.bootstrap-select').remove();
            $(idExecutorElem).insertAfter(formGroup.find("label"));
            idExecutorElem.find("option[class='bs-title-option']").remove();
            idExecutorElem.selectpicker().val(courtOrder.idExecutor).selectpicker('refresh');
        }
    }
}

function showEditDelPanelClaimState(claimStateElem) {
    let fields = claimStateElem.find('input, select, textarea');
    fields.prop('disabled', true).selectpicker('refresh');
    let editDelPanel = claimStateElem.find('.edit-del-panel');
    let yesNoPanel = claimStateElem.find('.yes-no-panel');
    yesNoPanel.hide();
    claimStateElem.removeClass("list-group-item-warning");
    editDelPanel.show();
    disableEditingMultiyClaimStates(false);
}

function saveClaimState(e) {
    let claimStateElem = $(this).closest(".list-group-item");
    if (claimStateElem.find("input, textarea, select")
        .filter(function (idx, elem) {
            return $(elem).closest(".modal").length === 0 && $(elem).closest("[id^='ClaimCourtOrdersForm']").length === 0;
        }).valid()) {
        let claimState = claimStateToFormData(getClaimState(claimStateElem), getCourtOrders(claimStateElem));
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/ClaimStates/SaveClaimState',
            data: claimState,
            processData: false,
            contentType: false,
            success: function (claimState) {
                if (claimState.idState > 0) {
                    showEditDelPanelClaimState(claimStateElem);
                    claimStateElem.find("input[name^='IdState']").val(claimState.idState);
                    claimStateElem.find("[id^='claimCourtOrderAdd']").addClass("disabled");
                    claimStateElem.find(".edit-del-court-order-panel a").addClass("disabled");
                } else {
                    alert("Произошла ошибка при изменении этапа исковой работы");
                }
            }
        });
    } else {
        claimStateElem.find("select").each(function (idx, elem) {
            var id = $(elem).prop("id");
            var name = $(elem).prop("name");
            var errorSpan = $("span[data-valmsg-for='" + name + "']");
            if (errorSpan.hasClass("field-validation-error")) {
                $("button[data-id='" + id + "']").addClass("input-validation-error");
            }
        });
        $([document.documentElement, document.body]).animate({
            scrollTop: claimStateElem.find(".input-validation-error").first().offset().top - 35
        }, 1000);
    }
    e.preventDefault();
}

function getCourtOrders(claimStateElem) {
    var claimStateTypeId = claimStateElem.find("[name^='IdStateType']").val() | 0;
    var courtOrders = [];
    if (claimStateTypeId === 4) {
        var courtOrderElems = $(claimStateElem).find("[id^=ClaimCourtOrders_] > .list-group-item").filter(function (idx, elem) {
            return !$(elem).hasClass("rr-list-group-item-empty");
        });
        var idClaim = claimStateElem.closest("#ClaimStates").data("id");
        courtOrders = courtOrderElems.map(function (idx, elem) {
            return {
                IdOrder: $(elem).find("input[id^='IdOrder']").val(),
                IdClaim: idClaim,
                IdSigner: $(elem).find("input[id^='IdSigner']").val(),
                IdJudge: $(elem).find("input[id^='IdJudge']").val(),
                OpenAccountDate: $(elem).find("input[id^='OpenAccountDate']").val(),
                AmountTenancy: $(elem).find("input[id^='AmountTenancy']").val(),
                AmountPenalties: $(elem).find("input[id^='AmountPenalties']").val(),
                AmountDgi: $(elem).find("input[id^='AmountDgi']").val(),
                AmountPadun: $(elem).find("input[id^='AmountPadun']").val(),
                AmountPkk: $(elem).find("input[id^='AmountPkk']").val(),
                StartDeptPeriod: $(elem).find("input[id^='StartDeptPeriod']").val(),
                EndDeptPeriod: $(elem).find("input[id^='EndDeptPeriod']").val(),
                CreateDate: $(elem).find("input[id^='CreateDate']").val(),
                OrderDate: $(elem).find("input[id^='OrderDate']").val(),
                IdExecutor: $(elem).find("select[id^='IdExecutor']").val()
            };
        });
    }
    return courtOrders;
}

function getClaimState(claimStateElem) {
    var claimStateTypeId = claimStateElem.find("[name^='IdStateType']").val() | 0;
    var claimState = {
        IdState: claimStateElem.find("[name^='IdState']").val(),
        IdClaim: claimStateElem.closest("#ClaimStates").data("id"),
        IdStateType: claimStateTypeId,
        DateStartState: claimStateElem.find("[name^='DateStartState']").val(),
        Description: claimStateElem.find("[name^='Description']").val(),
        Executor: claimStateElem.find("[name^='Executor']").val()
    };
    if (claimStateTypeId === 1) {
        claimState.BksRequester = claimStateElem.find("[name^='BksRequester']").val();
    }
    if (claimStateTypeId === 2) {
        claimState.TransferToLegalDepartmentDate = claimStateElem.find("[name^='TransferToLegalDepartmentDate']").val();
        claimState.TransferToLegalDepartmentWho = claimStateElem.find("[name^='TransferToLegalDepartmentWho']").val();
    }
    if (claimStateTypeId === 3) {
        claimState.AcceptedByLegalDepartmentDate = claimStateElem.find("[name^='AcceptedByLegalDepartmentDate']").val();
        claimState.AcceptedByLegalDepartmentWho = claimStateElem.find("[name^='AcceptedByLegalDepartmentWho']").val();
    }
    if (claimStateTypeId === 4) {
        claimState.ClaimDirectionDate = claimStateElem.find("[name^='ClaimDirectionDate']").val();
        claimState.ClaimDirectionDescription = claimStateElem.find("[name^='ClaimDirectionDescription']").val();
        claimState.CourtOrderDate = claimStateElem.find("[name^='CourtOrderDate']").val();
        claimState.CourtOrderNum = claimStateElem.find("[name^='CourtOrderNum']").val();
        claimState.ObtainingCourtOrderDate = claimStateElem.find("[name^='ObtainingCourtOrderDate']").val();
        claimState.ObtainingCourtOrderDescription = claimStateElem.find("[name^='ObtainingCourtOrderDescription']").val();
    }
    if (claimStateTypeId === 5) {
        claimState.DirectionCourtOrderBailiffsDate = claimStateElem.find("[name^='DirectionCourtOrderBailiffsDate']").val();
        claimState.DirectionCourtOrderBailiffsDescription = claimStateElem.find("[name^='DirectionCourtOrderBailiffsDescription']").val();
        claimState.EnforcementProceedingStartDate = claimStateElem.find("[name^='EnforcementProceedingStartDate']").val();
        claimState.EnforcementProceedingStartDescription = claimStateElem.find("[name^='EnforcementProceedingStartDescription']").val();
        claimState.EnforcementProceedingEndDate = claimStateElem.find("[name^='EnforcementProceedingEndDate']").val();
        claimState.EnforcementProceedingEndDescription = claimStateElem.find("[name^='EnforcementProceedingEndDescription']").val();
        claimState.EnforcementProceedingTerminateDate = claimStateElem.find("[name^='EnforcementProceedingTerminateDate']").val();
        claimState.EnforcementProceedingTerminateDescription = claimStateElem.find("[name^='EnforcementProceedingTerminateDescription']").val();
        claimState.RepeatedDirectionCourtOrderBailiffsDate = claimStateElem.find("[name^='RepeatedDirectionCourtOrderBailiffsDate']").val();
        claimState.RepeatedDirectionCourtOrderBailiffsDescription = claimStateElem.find("[name^='RepeatedDirectionCourtOrderBailiffsDescription']").val();
        claimState.RepeatedEnforcementProceedingStartDate = claimStateElem.find("[name^='RepeatedEnforcementProceedingStartDate']").val();
        claimState.RepeatedEnforcementProceedingStartDescription = claimStateElem.find("[name^='RepeatedEnforcementProceedingStartDescription']").val();
        claimState.RepeatedEnforcementProceedingEndDate = claimStateElem.find("[name^='RepeatedEnforcementProceedingEndDate']").val();
        claimState.RepeatedEnforcementProceedingEndDescription = claimStateElem.find("[name^='RepeatedEnforcementProceedingEndDescription']").val();
    }
    if (claimStateTypeId === 6) {
        claimState.CourtOrderCompleteDate = claimStateElem.find("[name^='CourtOrderCompleteDate']").val();
        claimState.CourtOrderCompleteReason = claimStateElem.find("[name^='CourtOrderCompleteReason']").val();
        claimState.CourtOrderCompleteDescription = claimStateElem.find("[name^='CourtOrderCompleteDescription']").val();
    }

    return claimState;
}

function getClaimStates() {
    var items = $("#ClaimStates .list-group-item").filter(function (idx, elem) {
        return !$(elem).hasClass("rr-list-group-item-empty");
    });
    return items.map(function (idx, elem) { return getClaimState($(elem)); });
}

function claimStateToFormData(claimState, courtOrders) {
    var formData = new FormData();
    for (let field in claimState) {
        formData.append("ClaimState." + field, claimState[field]);
    }
    for (let i = 0; i < courtOrders.length; i++) {
        for (let field in courtOrders[i]) {
            formData.append("CourtOrders[" + i + "]." + field, courtOrders[i][field]);
        }
    }
    return formData;
}

function deleteClaimState(e) {
    let isOk = confirm("Вы уверены что хотите удалить этап исковой работы?");
    if (isOk) {
        let claimStateElem = $(this).closest(".list-group-item");
        let idState = claimStateElem.find("input[name^='IdState']").val();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/ClaimStates/DeleteClaimState',
            data: { idState: idState },
            success: function (ind) {
                if (ind === 1) {
                    claimStateElem.remove();
                    var claimStates = $("#ClaimStates > .list-group-item");
                    if (claimStates.length === 1) {
                        $("#ClaimStates > .rr-list-group-item-empty").show();
                    } else {
                        $(claimStates[claimStates.length - 1]).find(".claim-state-delete-btn").removeClass("disabled");
                    }
                }
                else {
                    alert("Ошибка удаления!");
                }
            }
        });
    }
    e.preventDefault();
}

function showClaimStateDetails(e) {
    var claimStateDetails = $(this).closest(".list-group-item").find(".rr-claim-ext-info");
    var icon = $(this).find(".oi");
    if (icon.hasClass("oi-chevron-bottom")) {
        icon.removeClass("oi-chevron-bottom");
        icon.addClass("oi-chevron-top");
        claimStateDetails.css("display", "flex");
        $(this).closest(".list-group-item").addClass("rr-claim-state-active");
    } else {
        icon.addClass("oi-chevron-bottom");
        icon.removeClass("oi-chevron-top");
        claimStateDetails.css("display", "none");
        $(this).closest(".list-group-item").removeClass("rr-claim-state-active");
    }
    e.preventDefault();
}

function changeClaimStateType(e) {
    var idStateType = $(this).val() | 0;
    var extInfo = $(this).closest(".list-group-item").find(".rr-claim-ext-info");
    extInfo.each(function (idx, elem) {
        var elemIdStateType = $(elem).data("id-state-type");
        if (idStateType === elemIdStateType) {
            $(elem).removeClass("d-none");
        } else if (!$(elem).hasClass("d-none")) {
            $(elem).addClass("d-none");
        }
    });
}

function openModalForOspStatement(e)
{
    $("#OspModal").find("input, textarea, select").prop("disabled", false);
    $("#OspModal").modal('show');
    e.preventDefault();
}

$(function (){
    $("#ClaimStatesForm").on("click", "#claimStateAdd", addClaimState);
    $('#ClaimStatesForm').on('click', '.claim-state-edit-btn', editClaimState);
    $('#ClaimStatesForm').on('click', '.claim-state-cancel-btn', cancelEditClaimState);
    $('#ClaimStatesForm').on('click', '.claim-state-save-btn', saveClaimState);
    $('#ClaimStatesForm').on('click', '.claim-state-delete-btn', deleteClaimState);
    $('#ClaimStatesForm').on('click', '.rr-claim-state-details', showClaimStateDetails);
    $('#ClaimStatesForm').on('change', 'select[name^="IdStateType"]', changeClaimStateType);

    $("#ClaimStatesForm").on("click", "#osp-btn", openModalForOspStatement);
});
