function addRentPeriod(e) {
    let action = $('#TenancyProcessRentPeriods').data('action');

    $.ajax({
        type: 'POST',
        url: window.location.origin + '/TenancyProcesses/AddRentPeriod',
        data: { action },
        success: function (elem) {
            let list = $('#TenancyProcessRentPeriods');
            list.find(".rr-list-group-item-empty").hide();
            let rentPeriodToggle = $('.rr-rent-periods-card .tenancy-process-toggler');
            if (!isExpandElemntArrow(rentPeriodToggle)) // развернуть при добавлении, если было свернуто 
                rentPeriodToggle.click();
            list.append(elem);
            elem = list.find(".list-group-item").last();
            elem.find(".rent-period-edit-btn").first().click();
            $([document.documentElement, document.body]).animate({
                scrollTop: $(elem).offset().top
            }, 1000);
        }
    });
    e.preventDefault();
}

function editRentPeriod(e) {
    let rentPeriod = $(this).closest(".list-group-item");
    let self = this;
    let fields = rentPeriod.find('input, select, textarea');
    let yesNoPanel = rentPeriod.find('.yes-no-panel');
    let editDelPanel = rentPeriod.find('.edit-del-panel');
    fields.each(function (idx, elem) {
        if (/^EndDate_.*/.test($(elem).prop("name"))) {
            if (!rentPeriod.find("input[name^='UntilDismissal']").is(":checked")) {
                $(elem).prop('disabled', false);
            }
        } else {
            $(elem).prop('disabled', false);
        }
    });
    editDelPanel.hide();
    yesNoPanel.show();
    e.preventDefault();
}

function cancelEditRentPeriod(e) {
    let rentPeriodElem = $(this).closest(".list-group-item");
    let idRentPeriod = rentPeriodElem.find("input[name^='IdRentPeriod']").val();
    //Отменить изменения внесенные в документ
    if (idRentPeriod !== "0") {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyRentPeriods/GetRentPeriod',
            data: { idRentPeriod: idRentPeriod },
            success: function (rentPeriod) {
                refreshRentPeriod(rentPeriodElem, rentPeriod);
                showEditDelPanelRentPeriod(rentPeriodElem);
            }
        });
    }
    //Отменить вставку нового документа
    else {
        rentPeriodElem.remove();
        if ($("#TenancyProcessRentPeriods .list-group-item").length === 1) {
            $("#TenancyProcessRentPeriods .rr-list-group-item-empty").show();
        }
    }
    e.preventDefault();
}

function refreshRentPeriod(rentPeriodElem, rentPeriod) {
    rentPeriodElem.find("[name^='BeginDate']").val(rentPeriod.beginDate);
    rentPeriodElem.find("[name^='EndDate']").val(rentPeriod.endDate);
    rentPeriodElem.find("[name^='UntilDismissal']").prop("checked", rentPeriod.untilDismissal);
}

function showEditDelPanelRentPeriod(rentPeriodElem) {
    let fields = rentPeriodElem.find('input, select, textarea');
    fields.prop('disabled', true);
    let editDelPanel = rentPeriodElem.find('.edit-del-panel');
    let yesNoPanel = rentPeriodElem.find('.yes-no-panel');
    yesNoPanel.hide();
    rentPeriodElem.removeClass("list-group-item-warning");
    editDelPanel.show();
}

function saveRentPeriod(e) {
    let rentPeriodElem = $(this).closest(".list-group-item");
    if (rentPeriodElem.find("input, textarea, select").valid()) {
        let rentPeriod = rentPeriodToFormData(getRentPeriod(rentPeriodElem));
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyRentPeriods/SaveRentPeriod',
            data: rentPeriod,
            processData: false,
            contentType: false,
            success: function (rentPeriod) {
                if (rentPeriod.idRentPeriod > 0) {
                    rentPeriodElem.find("input[name^='IdRentPeriod']").val(rentPeriod.idRentPeriod);
                }
                showEditDelPanelRentPeriod(rentPeriodElem);
            }
        });
    } else {
        rentPeriodElem.find("select").each(function (idx, elem) {
            var id = $(elem).prop("id");
            var name = $(elem).prop("name");
            var errorSpan = $("span[data-valmsg-for='" + name + "']");
            if (errorSpan.hasClass("field-validation-error")) {
                $("button[data-id='" + id + "']").addClass("input-validation-error");
            }
        });
        $([document.documentElement, document.body]).animate({
            scrollTop: rentPeriodElem.find(".input-validation-error").first().offset().top - 35
        }, 1000);
    }
    e.preventDefault();
}

function getRentPeriod(rentPeriodElem) {
    return {
        IdRentPeriod: rentPeriodElem.find("[name^='IdRentPeriod']").val(),
        IdProcess: rentPeriodElem.closest("#TenancyProcessRentPeriods").data("id"),
        BeginDate: rentPeriodElem.find("[name^='BeginDate']").val(),
        EndDate: rentPeriodElem.find("[name^='EndDate']").val(),
        UntilDismissal: rentPeriodElem.find("[name^='UntilDismissal']").is(":checked")
    };
}

function rentPeriodToFormData(rentPeriod) {
    var formData = new FormData();
    formData.append("RentPeriod.IdRentPeriod", rentPeriod.IdRentPeriod);
    formData.append("RentPeriod.IdProcess", rentPeriod.IdProcess);
    formData.append("RentPeriod.BeginDate", rentPeriod.BeginDate);
    formData.append("RentPeriod.EndDate", rentPeriod.EndDate);
    formData.append("RentPeriod.UntilDismissal", rentPeriod.UntilDismissal);
    return formData;
}

function deleteRentPeriod(e) {
    let isOk = confirm("Вы уверены что хотите удалить период найма?");
    if (isOk) {
        let rentPeriodElem = $(this).closest(".list-group-item");
        let idRentPeriod = rentPeriodElem.find("input[name^='IdRentPeriod']").val();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/TenancyRentPeriods/DeleteRentPeriod',
            data: { idRentPeriod: idRentPeriod },
            success: function (ind) {
                if (ind === 1) {
                    rentPeriodElem.remove();
                    if ($("#TenancyProcessRentPeriods .list-group-item").length === 1) {
                        $("#TenancyProcessRentPeriods .rr-list-group-item-empty").show();
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

function changeUntilDismissal(e) {
    var rentPeriodElem = $(this).closest(".list-group-item");
    var endDateElem = rentPeriodElem.find("input[name^='EndDate']");
    if ($(this).is(":checked")) {
        endDateElem.data("last-end-date-before-dismissal", endDateElem.val());
        endDateElem.val("");
        endDateElem.prop("disabled", "disabled");
    } else {
        endDateElem.prop("disabled", "");
        if (endDateElem.data("last-end-date-before-dismissal") !== undefined) {
            endDateElem.val(endDateElem.data("last-end-date-before-dismissal"));
        }
    }
    e.preventDefault();
}

function rentPeriodsCorrectNaming() {
    var items = $("#TenancyProcessRentPeriods .list-group-item");
    var index = 0;
    items.each(function (idx, elem) {
        if ($(elem).hasClass("rr-list-group-item-empty")) {
            return;
        }
        $(elem).find("input, select, textarea").each(function (subIdx, subElem) {
            var name = $(subElem).attr("name");
            name = name.split("_")[0];
            $(subElem).attr("name", "TenancyProcess.TenancyRentPeriods[" + index + "]." + name);
        });
        index++;
    });
}

$(function () {
    $(".rr-rent-periods-card").on("click", "#rentPeriodAdd", addRentPeriod);
    $('.rr-rent-periods-card').on('click', '.rent-period-edit-btn', editRentPeriod);
    $('.rr-rent-periods-card').on('click', '.rent-period-cancel-btn', cancelEditRentPeriod);
    $('.rr-rent-periods-card').on('click', '.rent-period-save-btn', saveRentPeriod);
    $('.rr-rent-periods-card').on('click', '.rent-period-delete-btn', deleteRentPeriod);
    $('.rr-rent-periods-card').on('change', '[id^="UntilDismissal"]', changeUntilDismissal);
});