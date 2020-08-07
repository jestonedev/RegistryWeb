//Ф-ции для "Документы права собственности"
let getErrorSpanRestrictions = function (dataValmsgFor) {
    return "<span class=\"text-danger field-validation-valid\" data-valmsg-for=\"" + dataValmsgFor +
        "\" data-valmsg-replace=\"true\"></span>";
};

function getRestriction(restrictionElem) {
    return {
        //IdRestriction: restrictionElem.find("[name$='IdRestriction']").val(),
        Number: restrictionElem.find("input[name$='Number']").val(),
        Date: restrictionElem.find("input[name$='ResDate']").val(),
        DateStateReg: restrictionElem.find("input[name$='DateStateReg']").val(),
        Description: restrictionElem.find("input[name$='Description']").val(),
        IdRestrictionType: restrictionElem.find("select[name$='RestrictionType']").val(),
        RestrictionFile: restrictionElem.find("input[name$='RestrictionFile']")[0],
        RestrictionFileRemove: restrictionElem.find("input[name$='RestrictionFileRemove']").val()
    };
}

let refreshValidationRestrictionsForm = function () {
    var form = $("#restrictionsForm")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
};

function attachRestrictionFileforPaste(e) {
    var restriction = $(this).closest(".restrictionblock");
    restriction.find('input[name$="RestrictionFile"]').click();
    restriction.find('input[name$="RestrictionFileRemove"]').val(false);
    e.preventDefault();
}

function changeRestrictionFileAttachmentforPaste() {
    var restriction = $(this).closest(".restrictionblock");
    var name = restriction.find("[name$='RestrictionFile']").val().split('\\');
    if ($(this).val() !== "") {
        var elem = restriction.find("[name$='Description']");
        elem.val($("[name$='Description']").val() + " " + name[name.length - 1]);
        let restrictionFileBtns = restriction.find(".rr-restriction-file-buttons");
        restriction.find(".rr-restriction-file-attach").hide();
        restrictionFileBtns.append(restriction.find(".rr-restriction-file-remove").show());
    }
}

function removeRestrictionFileforPaste(e) {
    var restrictionElem = $(this).closest(".restrictionblock");
    restrictionElem.find("input[name$='RestrictionFile']").val("");
    restrictionElem.find("input[name$='Description']").val("");
    restrictionElem.find("input[name$='RestrictionFileRemove']").val(true);
    let restrictionFileBtns = restrictionElem.find(".rr-restriction-file-buttons");
    restrictionElem.find(".rr-restriction-file-remove").hide();
    restrictionFileBtns.append(restrictionElem.find(".rr-restriction-file-attach").show());
    e.preventDefault();
}

function restrictionToFormDataforPaste(restriction, address) {
    var formData = new FormData();
    //formData.append("Restriction.IdRestriction", restriction.IdRestriction);
    formData.append("Restriction.Number", restriction.Number);
    formData.append("Restriction.Date", restriction.Date);
    formData.append("Restriction.DateStateReg", restriction.DateStateReg);
    formData.append("Restriction.Description", restriction.Description);
    formData.append("Restriction.IdRestrictionType", restriction.IdRestrictionType);
    formData.append("RestrictionFile", restriction.RestrictionFile.files[0]);
    formData.append("RestrictionFileRemove", restriction.RestrictionFileRemove);
    formData.append("Address.AddressType", address.addressType);
    //formData.append("Address.Id", address.id);
    return formData;
}

function showRestrictionDownloadFileBtn(restrictionElem, fileExists) {
    let restrictionFileBtns = restrictionElem.find(".rr-restriction-file-buttons");
    restrictionFileBtns.append(restrictionElem.find(".rr-restriction-file-download").show());
    if (fileExists) {
        restrictionElem.find(".rr-restriction-file-download").removeClass("disabled");
    } else {
        restrictionElem.find(".rr-restriction-file-download").addClass("disabled");
    }
    restrictionElem.find(".rr-restriction-file-remove").hide();
    restrictionElem.find(".rr-restriction-file-attach").hide();
}

function showEditDelPanelRestriction(restrictionElem) {
    let fields = restrictionElem.find('input, select, textarea');
    fields.prop('disabled', true).selectpicker('refresh');
    let editDelPanel = restrictionElem.find('.edit-del-panel');
    let yesNoPanel = restrictionElem.find('.yes-no-panel');
    yesNoPanel.hide();
    editDelPanel.show();
}

function saveRestrictionforPaste(e) {
    $(".status").html(""); $(".status").removeClass("alert alert-success");
    let restrictionElem = $(".restrictionblock");
    restrictionElem.find("button[data-id]").removeClass("input-validation-error");
    if (restrictionElem.find("input, textarea, select").valid()) {
        let restriction = restrictionToFormDataforPaste(getRestriction(restrictionElem), { addressType: "Premise"});
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Premises/AddRestrictionInPremises',
            data: restriction,
            processData: false,
            contentType: false,
            success: function (status) {
                if (status == 0) {
                    $(".status").html("Реквизиты успешно проставлены!");//alert("Помещения успешно добавлены в мастер массовых операций");
                    $(".status").addClass("alert alert-success");
                    $("#restrictionModal").modal("toggle");
                }
                /*if (status == -1) {
                    $(".status").html("Отсутствуют помещения для добавления в мастер массовых операций");//alert("Отсутствуют помещения для добавления в мастер массовых операций");
                    $(".status").addClass("alert alert-danger");
                }*/
            }
        });
    } else {
        restrictionElem.find("select").each(function (idx, elem) {
            var id = $(elem).prop("id");
            var name = $(elem).prop("name");
            var errorSpan = $("span[data-valmsg-for='" + name + "']");
            if (errorSpan.hasClass("field-validation-error")) {
                $("button[data-id='" + id + "']").addClass("input-validation-error");
            }
        });
        $([document.documentElement, document.body]).animate({
            scrollTop: restrictionElem.find(".input-validation-error").first().offset().top - 35
        }, 1000);
    }
    e.preventDefault();
}
//______________________________________________________________________________________________________________

//Ф-ции для "Документы права собственности"
let getErrorSpanOwnershipRights = function (dataValmsgFor) {
    return "<span class=\"text-danger field-validation-valid\" data-valmsg-for=\"" + dataValmsgFor +
        "\" data-valmsg-replace=\"true\"></span>";
};

function getOwnershipRight(owElem) {
    return {
        //IdOwnershipRight: owElem.find("[name$='IdOwnershipRight']").val(),
        Number: owElem.find("[name$='Number']").val(),
        Date: owElem.find("[name$='OwrDate']").val(),
        Description: owElem.find("[name$='Description']").val(),
        IdOwnershipRightType: owElem.find("select[name$='OwrType']").val(),
        /*ResettlePlanDate: owElem.find("[name$='ResettlePlanDate']").val(),
        DemolishPlanDate: owElem.find("[name$='DemolishPlanDate']").val(),*/
        OwnershipRightFile: owElem.find("[name$='OwnershipRightFile']")[0],
        OwnershipRightFileRemove: owElem.find("[name$='OwnershipRightFileRemove']").val()
    };
}

let refreshValidationOwnershipRightsForm = function () {
    var form = $("#ownershiprightForm")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
};

function attachOwnershipRightFileforPaste(e) {
    var ownershipRight = $(this).closest(".ownershiprightblock");
    ownershipRight.find('input[name$="OwnershipRightFile"]').click();
    ownershipRight.find('input[name$="OwnershipRightFileRemove"]').val(false);
    e.preventDefault();
}

function changeOwnershipRightFileAttachmentforPaste() {
    var ownershipRight = $(this).closest(".ownershiprightblock");
    var name = ownershipRight.find("[name$='OwnershipRightFile']").val().split('\\');
    if ($(this).val() !== "") {
        var elem = ownershipRight.find("[name$='Description']");
        elem.val($("[name$='Description']").val() + " " + name[name.length - 1]);
        let ownershipRightFileBtns = ownershipRight.find(".rr-ownershipright-file-buttons");
        ownershipRight.find(".rr-ownershipright-file-attach").hide();
        ownershipRightFileBtns.append(ownershipRight.find(".rr-ownershipright-file-remove").show());
    }
}

function removeOwnershipRightFileforPaste(e) {
    var ownershipRightElem = $(this).closest(".ownershiprightblock");
    ownershipRightElem.find("input[name$='OwnershipRightFile']").val("");
    ownershipRightElem.find("input[name$='Description']").val("");
    ownershipRightElem.find("input[name$='OwnershipRightFileRemove']").val(true);
    let ownershipRightFileBtns = ownershipRightElem.find(".rr-ownershipright-file-buttons");
    ownershipRightElem.find(".rr-ownershipright-file-remove").hide();
    ownershipRightFileBtns.append(ownershipRightElem.find(".rr-ownershipright-file-attach").show());
    e.preventDefault();
}

function ownershipRightToFormDataforPaste(ownershipright, address) {
    var formData = new FormData();
    //formData.append("ownershipright.IdRestriction", ownershipright.IdRestriction);
    formData.append("OwnershipRight.Number", ownershipright.Number);
    formData.append("OwnershipRight.Date", ownershipright.Date);
    formData.append("OwnershipRight.Description", ownershipright.Description);
    formData.append("OwnershipRight.IdOwnershipRightType", ownershipright.IdOwnershipRightType);
    formData.append("OwnershipRightFile", ownershipright.OwnershipRightFile.files[0]);
    formData.append("OwnershipRightFileRemove", ownershipright.OwnershipRightFileRemove);
    formData.append("Address.AddressType", address.addressType);
    //formData.append("Address.Id", address.id);
    return formData;
}

function showOwnershipRightDownloadFileBtn(owrElem, fileExists) {
    let ownershiprightFileBtns = owrElem.find(".rr-ownershipright-file-buttons");
    ownershiprightFileBtns.append(owrElem.find(".rr-ownershipright-file-download").show());
    if (fileExists) {
        owrElem.find(".rr-ownershipright-file-download").removeClass("disabled");
    } else {
        owrElem.find(".rr-ownershipright-file-download").addClass("disabled");
    }
    owrElem.find(".rr-ownershipright-file-remove").hide();
    owrElem.find(".rr-ownershipright-file-attach").hide();
}

function showEditDelPanelOwnershipRight(owrElem) {
    let fields = owrElem.find('input, select, textarea');
    fields.prop('disabled', true).selectpicker('refresh');
    let editDelPanel = owrElem.find('.edit-del-panel');
    let yesNoPanel = owrElem.find('.yes-no-panel');
    yesNoPanel.hide();
    editDelPanel.show();
}

function saveOwnershipRightforPaste(e) {
    $(".status").html(""); $(".status").removeClass("alert alert-success");
    let owrElem = $(".ownershiprightblock");
    owrElem.find("button[data-id]").removeClass("input-validation-error");
    if (owrElem.find("input, textarea, select").valid()) {
        let owr = ownershipRightToFormDataforPaste(getOwnershipRight(owrElem), { addressType: "Premise" });
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Premises/AddOwnershipInPremises',
            data: owr,
            processData: false,
            contentType: false,
            success: function (status) {
                if (status == 0) {
                    $(".status").html("Ограничения успешно проставлены!");
                    $(".status").addClass("alert alert-success");
                    $("#ownershiprightModal").modal("toggle");
                }
            }
        });
    } else {
        owrElem.find("select").each(function (idx, elem) {
            var id = $(elem).prop("id");
            var name = $(elem).prop("name");
            var errorSpan = $("span[data-valmsg-for='" + name + "']");
            if (errorSpan.hasClass("field-validation-error")) {
                $("button[data-id='" + id + "']").addClass("input-validation-error");
            }
        });
        owrElem.find("input").each(function (idx, elem) {
            var id = $(elem).prop("id");
            var name = $(elem).prop("name");
            var errorSpan = $("span[data-valmsg-for='" + name + "']");
            if (errorSpan.hasClass("field-validation-error")) {
                $("button[data-id='" + id + "']").addClass("input-validation-error");
            }
        });
        $([document.documentElement, document.body]).animate({
            scrollTop: owrElem.find(".input-validation-error").first().offset().top - 35
        }, 1000);
    }
    e.preventDefault();
}
//______________________________________________________________________________________________________________

//Общая инициализирующая ф-ция
$(function () {
    $('.yes-no-panel').hide();

    $('.restrictionblock').on('click', '.restriction-save-btn', saveRestrictionforPaste);
    $(".restrictionblock").on('click', '.rr-restriction-file-attach', attachRestrictionFileforPaste);
    $('.restrictionblock').on('click', '.rr-restriction-file-remove', removeRestrictionFileforPaste);
    $('.restrictionblock').on('change', "input[name$='RestrictionFile']", changeRestrictionFileAttachmentforPaste);

    $('.ownershiprightblock').on('click', '.ownershipright-save-btn', saveOwnershipRightforPaste);
    $('.ownershiprightblock').on('click', '.rr-ownershipright-file-attach', attachOwnershipRightFileforPaste);
    $('.ownershiprightblock').on('click', '.rr-ownershipright-file-remove', removeOwnershipRightFileforPaste);
    $('.ownershiprightblock').on('change', "input[name$='OwnershipRightFile']", changeOwnershipRightFileAttachmentforPaste);
});