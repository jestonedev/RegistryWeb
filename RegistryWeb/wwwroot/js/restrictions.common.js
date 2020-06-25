let CreateRestrictionPremisesAssoc = function () {
    let restrictions = getRestrictions();
    let restrictionPremisesAssoc = [];
    restrictions.each(function (idx, item) {
        restrictionPremisesAssoc.push({
            IdPremises: 0,
            IdRestriction: 0,
            RestrictionNavigation: item
        });
    });
    return restrictionPremisesAssoc;
};

let CreateRestrictionBuildingsAssoc = function () {
    let restrictions = getRestrictions();
    let restrictionsBuildingsAssoc = [];
    restrictions.each(function (idx, item) {
        restrictionsBuildingsAssoc.push({
            IdBuilding: 0,
            IdRestriction: 0,
            RestrictionNavigation: item
        });
    });
    return restrictionsBuildingsAssoc;
};

function getRestrictions() {
    return $("#restrictionsList .list-group-item").map(function (idx, elem) {
        return getRestriction($(elem));
    });
}

function getRestriction(restrictionElem) {
    return {
        IdRestriction: restrictionElem.find("[name^='IdRestriction']").val(),
        Number: restrictionElem.find("[name^='RestrictionNum']").val(),
        Date: restrictionElem.find("[name^='RestrictionDate']").val(),
        Description: restrictionElem.find("[name^='RestrictionDescription']").val(),
        IdRestrictionType: restrictionElem.find("[name^='IdRestrictionType']").val(),
        RestrictionFile: restrictionElem.find("[name^='RestrictionFile']")[0],
        RestrictionFileRemove: restrictionElem.find("[name^='RestrictionFileRemove']").val()
    };
}

function restrictionToFormData(restriction, address) {
    var formData = new FormData();
    formData.append("Restriction.IdRestriction", restriction.IdRestriction);
    formData.append("Restriction.Number", restriction.Number);
    formData.append("Restriction.Date", restriction.Date);
    formData.append("Restriction.Description", restriction.Description);
    formData.append("Restriction.IdRestrictionType", restriction.IdRestrictionType);
    formData.append("RestrictionFile", restriction.RestrictionFile.files[0]);
    formData.append("RestrictionFileRemove", restriction.RestrictionFileRemove);
    formData.append("Address.AddressType", address.addressType);
    formData.append("Address.Id", address.id);
    return formData;
}

let getCurrentAddressRestrictions = function () {
    let address = {
        addressType: $('#restrictionsList').data('addresstype'),
        id: $('#restrictionsList').data('id')
    };
    return address;
};

let getErrorSpanRestrictions = function (dataValmsgFor) {
    return "<span class=\"text-danger field-validation-valid\" data-valmsg-for=\"" + dataValmsgFor +
        "\" data-valmsg-replace=\"true\"></span>";
};

let initializeVilidationRestriction = function (restrictionElem) {

    let idRestriction = restrictionElem.find("input[name^='IdRestriction']").val();
    if (idRestriction === "0") idRestriction = uuidv4();
    //Дата документа
    let date = 'RestrictionDate_' + idRestriction;
    restrictionElem.find("[name^='RestrictionDate']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Дата документа" является обязательным')
        .attr('id', date)
        .attr('name', date)
        .attr('aria-describedby', date + '-error')
        .after(getErrorSpanRestrictions(date));
    // Тип документа
    let idRestrictionTypeName = 'IdRestrictionType_' + idRestriction;
    var restrictionTypeElem = restrictionElem.find("[name^='IdRestrictionType']");
    restrictionTypeElem.addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Тип документа" является обязательным')
        .attr('id', idRestrictionTypeName)
        .attr('name', idRestrictionTypeName)
        .attr('aria-describedby', idRestrictionTypeName + '-error').parent()
        .after(getErrorSpanRestrictions(idRestrictionTypeName));
    restrictionTypeElem.next().attr("data-id", idRestrictionTypeName);

    refreshValidationRestrictionsForm();
};

let refreshValidationRestrictionsForm = function () {
    var form = $("#restrictionsForm")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
};

let initializeVilidationRestricitons = function () {
    let restrictions = $('#restrictionsList .list-group-item');
    restrictions.each(function () {
        initializeVilidationRestriction($(this));
    });
};

function deleteRestriction(e) {
    let isOk = confirm("Вы уверены что хотите удалить документ права собственности?");
    if (isOk) {
        let restrictionElem = $(this).closest(".list-group-item");
        let idRestriction = restrictionElem.find("input[name^='IdRestriction']").val();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/Restrictions/DeleteRestriction',
            data: { idRestriction: idRestriction },
            success: function (ind) {
                if (ind === 1) {
                    restrictionElem.remove();
                }
                else {
                    alert("Ошибка удаления!");
                }
            }
        });
    }
    e.preventDefault();
}

function addRestriction(e) {
    let action = $('#restrictionsList').data('action');
    let addressType = $('#restrictionsList').data('addresstype');

    $.ajax({
        type: 'POST',
        url: window.location.origin + '/Restrictions/AddRestriction',
        data: { addressType, action },
        success: function (elem) {
            let list = $('#restrictionsList');
            let restrictionsToggle = $('#restrictionsToggle');
            if (!isExpandElemntArrow(restrictionsToggle)) // развернуть при добавлении, если было свернуто 
                restrictionsToggle.click();
            list.append(elem);
            elem = list.find(".list-group-item").last();
            elem.find("select").selectpicker("refresh");
            elem.find(".restriction-edit-btn").click();
            $([document.documentElement, document.body]).animate({
                scrollTop: $(elem).offset().top
            }, 1000);
            initializeVilidationRestriction(elem);
        }
    });
    e.preventDefault();
}

function editRestriction(e) {
    let restriction = $(this).closest(".list-group-item");
    let fields = restriction.find('input, select, textarea');
    let yesNoPanel = restriction.find('.yes-no-panel');
    let editDelPanel = restriction.find('.edit-del-panel');
    fields.filter(function (idx, val) { return !$(val).prop("name").startsWith("RestrictionAttachment"); }).prop('disabled', false);
    restriction.find("select").selectpicker('refresh');
    editDelPanel.hide();
    yesNoPanel.show();
    showRestrictionEditFileBtns(restriction,
        restriction.find(".rr-restriction-file-download").length > 0 &&
        !restriction.find(".rr-restriction-file-download").hasClass("disabled"));
    e.preventDefault();
}

function cancelEditRestriction(e) {
    let restrictionElem = $(this).closest(".list-group-item");
    let idRestriction = restrictionElem.find("input[name^='IdRestriction']").val();
    //Отменить изменения внесенные в документ
    if (idRestriction !== "0") {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Restrictions/GetRestriction',
            data: { idRestriction: idRestriction },
            success: function (restriction) {
                refreshRestriction(restrictionElem, restriction);
                showEditDelPanelRestriction(restrictionElem);
                clearValidationsRestrictions(restrictionElem);
                showRestrictionDownloadFileBtn(restrictionElem, restriction.fileOriginName !== null);
            }
        });
    }
    //Отменить вставку нового документа
    else {
        restrictionElem.remove();
    }
    e.preventDefault();
}

function showRestrictionEditFileBtns(restrictionElem, fileExists) {
    let restrictionFileBtns = restrictionElem.find(".rr-restriction-file-buttons");
    restrictionElem.find(".rr-restriction-file-download").hide();
    if (fileExists) {
        restrictionFileBtns.append(restrictionElem.find(".rr-restriction-file-remove").show());
        restrictionElem.find(".rr-restriction-file-attach").hide();
    } else {
        restrictionElem.find(".rr-restriction-file-remove").hide();
        restrictionFileBtns.append(restrictionElem.find(".rr-restriction-file-attach").show());
    }
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

function clearValidationsRestrictions(restrictionElem) {
    $(restrictionElem).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
    $(restrictionElem).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");
}

function showEditDelPanelRestriction(restrictionElem) {
    let fields = restrictionElem.find('input, select, textarea');
    fields.prop('disabled', true).selectpicker('refresh');
    let editDelPanel = restrictionElem.find('.edit-del-panel');
    let yesNoPanel = restrictionElem.find('.yes-no-panel');
    yesNoPanel.hide();
    editDelPanel.show();
}

function refreshRestriction(restrictionElem, restriction) {
    restrictionElem.find("[name^='RestrictionNum']").val(restriction.number);
    restrictionElem.find("[name^='RestrictionDate']").val(restriction.date);
    restrictionElem.find("[name^='RestrictionDescription']").val(restriction.description);
    restrictionElem.find("[name^='IdRestrictionType']").val(restriction.idRestrictionType).selectpicker('refresh');
    restrictionElem.find("[name^='RestrictionFile']").val("");
    restrictionElem.find("[name^='RestrictionFileRemove']").val(false);
}

function saveRestriction(e) {
    let restrictionElem = $(this).closest(".list-group-item");
    restrictionElem.find("button[data-id]").removeClass("input-validation-error");
    if (restrictionElem.find("input, textarea, select").valid()) {
        let restriction = restrictionToFormData(getRestriction(restrictionElem), getCurrentAddressRestrictions());
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Restrictions/SaveRestriction',
            data: restriction,
            processData: false,
            contentType: false,
            success: function (restriction) {
                if (restriction.idRestriction > 0) {
                    restrictionElem.find("input[name^='IdRestriction']").val(restriction.idRestriction);
                    restrictionElem.find(".rr-restriction-file-download")
                        .prop("href", "/Restrictions/DownloadFile/?idRestriction=" + restriction.idRestriction);
                    showRestrictionDownloadFileBtn(restrictionElem, restriction.fileOriginName !== null);
                }
                showEditDelPanelRestriction(restrictionElem);
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

function attachRestrictionFile(e) {
    var restrictionElem = $(this).closest(".list-group-item");
    restrictionElem.find("input[name^='RestrictionFile']").click();
    restrictionElem.find("input[name^='RestrictionFileRemove']").val(false);
    e.preventDefault();
}

function changeRestrictionFileAttachment() {
    var restrictionElem = $(this).closest(".list-group-item");
    if ($(this).val() !== "") {
        let restrictionFileBtns = restrictionElem.find(".rr-restriction-file-buttons");
        restrictionElem.find(".rr-restriction-file-attach").hide();
        restrictionFileBtns.append(restrictionElem.find(".rr-restriction-file-remove").show());
    }
}

function removeRestrictionFile(e) {
    var restrictionElem = $(this).closest(".list-group-item");
    restrictionElem.find("input[name^='RestrictionFile']").val("");
    restrictionElem.find("input[name^='RestrictionFileRemove']").val(true);
    let restrictionFileBtns = restrictionElem.find(".rr-restriction-file-buttons");
    restrictionElem.find(".rr-restriction-file-remove").hide();
    restrictionFileBtns.append(restrictionElem.find(".rr-restriction-file-attach").show());
    e.preventDefault();
}

$(function () {
    $('.yes-no-panel').hide();
    initializeVilidationRestricitons();
    $('#restrictionAdd').click(addRestriction);
    $('#restrictionsToggle').on('click', $('#restrictionsList'), elementToogleHide);
    $('#restrictionsList').on('click', '.restriction-edit-btn', editRestriction);
    $('#restrictionsList').on('click', '.restriction-cancel-btn', cancelEditRestriction);
    $('#restrictionsList').on('click', '.restriction-save-btn', saveRestriction);
    $('#restrictionsList').on('click', '.restriction-delete-btn', deleteRestriction);
    $('#restrictionsList').on('click', '.rr-restriction-file-attach', attachRestrictionFile);
    $('#restrictionsList').on('click', '.rr-restriction-file-remove', removeRestrictionFile);
    $('#restrictionsList').on('change', "input[name^='RestrictionFile']", changeRestrictionFileAttachment);
});