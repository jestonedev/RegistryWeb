function addTenancyReason(e) {
    let action = $('#TenancyProcessReasons').data('action');

    $.ajax({
        type: 'POST',
        url: window.location.origin + '/TenancyProcesses/AddTenancyReason',
        data: { action },
        success: function (elem) {
            let list = $('#TenancyProcessReasons');
            list.find(".rr-list-group-item-empty").hide();
            let tenancyReasonToggle = $('#TenancyProcessReasonsForm .tenancy-process-toggler');
            if (!isExpandElemntArrow(tenancyReasonToggle)) // развернуть при добавлении, если было свернуто 
                tenancyReasonToggle.click();
            list.append(elem);
            elem = list.find(".list-group-item").last();
            elem.find("select").selectpicker("refresh");
            elem.find(".tenancy-reason-edit-btn").first().click();
            $([document.documentElement, document.body]).animate({
                scrollTop: $(elem).offset().top
            }, 1000);
            refreshValidationTenancyReasonsForm();
        }
    });
    e.preventDefault();
}

function editTenancyReason(e) {
    let tenancyReason = $(this).closest(".list-group-item");
    let fields = tenancyReason.find('input, select, textarea');
    let yesNoPanel = tenancyReason.find('.yes-no-panel');
    let editDelPanel = tenancyReason.find('.edit-del-panel');
    fields.prop('disabled', false);
    tenancyReason.find("select").selectpicker('refresh');
    editDelPanel.hide();
    yesNoPanel.show();
    e.preventDefault();
}

function cancelEditTenancyReason(e) {
    let tenancyReasonElem = $(this).closest(".list-group-item");
    let idReason = tenancyReasonElem.find("input[name^='IdReason']").val();
    //Отменить изменения внесенные в документ
    if (idReason !== "0") {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyReasons/GetReason',
            data: { idReason: idReason },
            success: function (tenancyReason) {
                refreshTenancyReason(tenancyReasonElem, tenancyReason);
                showEditDelPanelTenancyReason(tenancyReasonElem);
                clearValidationsTenancyReason(tenancyReasonElem);
            }
        });
    }
    //Отменить вставку нового документа
    else {
        tenancyReasonElem.remove();
        if ($("#TenancyProcessReasons .list-group-item").length === 1) {
            $("#TenancyProcessReasons .rr-list-group-item-empty").show();
        }
    }
    e.preventDefault();
}

function clearValidationsTenancyReason(tenancyReasonElem) {
    $(tenancyReasonElem).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
    $(tenancyReasonElem).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");
}

function refreshTenancyReason(tenancyReasonElem, tenancyReason) {
    tenancyReasonElem.find("[name^='ReasonNumber']").val(tenancyReason.reasonNumber);
    tenancyReasonElem.find("[name^='ReasonDate']").val(tenancyReason.reasonDate);
    tenancyReasonElem.find("[name^='IdReasonType']").val(tenancyReason.idReasonType).selectpicker('refresh');
}

function showEditDelPanelTenancyReason(tenancyReasonElem) {
    let fields = tenancyReasonElem.find('input, select, textarea');
    fields.prop('disabled', true).selectpicker('refresh');
    let editDelPanel = tenancyReasonElem.find('.edit-del-panel');
    let yesNoPanel = tenancyReasonElem.find('.yes-no-panel');
    yesNoPanel.hide();
    editDelPanel.show();
}

function saveTenancyReason(e) {
    let tenancyReasonElem = $(this).closest(".list-group-item");
    if (tenancyReasonElem.find("input, textarea, select").valid()) {
        let tenancyReason = tenancyReasonToFormData(getTenancyReason(tenancyReasonElem));
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyReasons/SaveReason',
            data: tenancyReason,
            processData: false,
            contentType: false,
            success: function (tenancyReason) {
                if (tenancyReason.idReason > 0) {
                    tenancyReasonElem.find("input[name^='IdReason']").val(tenancyReason.idReason);
                }
                showEditDelPanelTenancyReason(tenancyReasonElem);
            }
        });
    } else {
        tenancyReasonElem.find("select").each(function (idx, elem) {
            var id = $(elem).prop("id");
            var name = $(elem).prop("name");
            var errorSpan = $("span[data-valmsg-for='" + name + "']");
            if (errorSpan.hasClass("field-validation-error")) {
                $("button[data-id='" + id + "']").addClass("input-validation-error");
            }
        });
        $([document.documentElement, document.body]).animate({
            scrollTop: tenancyReasonElem.find(".input-validation-error").first().offset().top - 35
        }, 1000);
    }
    e.preventDefault();
}

function getTenancyReason(tenancyReasonElem) {
    return {
        IdReason: tenancyReasonElem.find("[name^='IdReason']").val(),
        IdProcess: tenancyReasonElem.closest("#TenancyProcessReasons").data("id"),
        IdReasonType: tenancyReasonElem.find("[name^='IdReasonType']").val(),
        ReasonNumber: tenancyReasonElem.find("[name^='ReasonNumber']").val(),
        ReasonDate: tenancyReasonElem.find("[name^='ReasonDate']").val()
    };
}

function getTenancyReasons() {
    var items = $("#TenancyProcessReasons .list-group-item").filter(function (idx, elem) {
        return !$(elem).hasClass("rr-list-group-item-empty");
    });
    return items.map(function (idx, elem) { return getTenancyReason($(elem)); });
}

function tenancyReasonToFormData(tenancyReason) {
    var formData = new FormData();
    formData.append("TenancyReason.IdReason", tenancyReason.IdReason);
    formData.append("TenancyReason.IdProcess", tenancyReason.IdProcess);
    formData.append("TenancyReason.IdReasonType", tenancyReason.IdReasonType);
    formData.append("TenancyReason.ReasonNumber", tenancyReason.ReasonNumber);
    formData.append("TenancyReason.ReasonDate", tenancyReason.ReasonDate);
    return formData;
}

function deleteTenancyReason(e) {
    let isOk = confirm("Вы уверены что хотите удалить основание найма?");
    if (isOk) {
        let tenancyReasonElem = $(this).closest(".list-group-item");
        let idReason = tenancyReasonElem.find("input[name^='IdReason']").val();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/TenancyReasons/DeleteReason',
            data: { idReason: idReason },
            success: function (ind) {
                if (ind === 1) {
                    tenancyReasonElem.remove();
                    if ($("#TenancyProcessReasons .list-group-item").length === 1) {
                        $("#TenancyProcessReasons .rr-list-group-item-empty").show();
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

let refreshValidationTenancyReasonsForm = function () {
    var form = $("#TenancyProcessReasonsForm")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
};

$(function () {
    $("#TenancyProcessReasonsForm").on("click", "#tenancyReasonAdd", addTenancyReason);
    $('#TenancyProcessReasonsForm').on('click', '.tenancy-reason-edit-btn', editTenancyReason);
    $('#TenancyProcessReasonsForm').on('click', '.tenancy-reason-cancel-btn', cancelEditTenancyReason);
    $('#TenancyProcessReasonsForm').on('click', '.tenancy-reason-save-btn', saveTenancyReason);
    $('#TenancyProcessReasonsForm').on('click', '.tenancy-reason-delete-btn', deleteTenancyReason);
});