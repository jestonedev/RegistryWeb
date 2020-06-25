/*let CreateJurisdictionPremisesAssoc = function () {
    let jurisdictions = getJurisdictions();
    let jurisdictionPremisesAssoc = [];
    jurisdictions.each(function (idx, item) {
        jurisdictionPremisesAssoc.push({
            IdPremises: 0,
            IdJurisdiction: 0,
            JurisdictionNavigation: item
        });
    });
    return jurisdictionPremisesAssoc;
};

let CreateJurisdictionBuildingsAssoc = function () {
    let jurisdictions = getJurisdictions();
    let jurisdictionsBuildingsAssoc = [];
    jurisdictions.each(function (idx, item) {
        jurisdictionsBuildingsAssoc.push({
            IdBuilding: 0,
            IdJurisdiction: 0,
            JurisdictionNavigation: item
        });
    });
    return jurisdictionsBuildingsAssoc;
};
*/
function getJurisdictions() {
    return $("#jurisdictionsList .list-group-item").map(function (idx, elem) {
        return getJurisdiction($(elem));
    });
}

function getJurisdiction(jurisdictionElem) {
    return {
        IdJurisdiction: jurisdictionElem.find("[name^='IdJurisdiction']").val(),
        IdPremises: jurisdictionElem.find("[name^='IdPremises']").val(),
        IdActFile: jurisdictionElem.find("[name^='IdActFile']").val(),
        IdActFileTypeDocument: jurisdictionElem.find("[name^='IdJurisdictionType']").val(),
        Number: jurisdictionElem.find("[name^='JurisdictionNum']").val(),
        Date: jurisdictionElem.find("[name^='JurisdictionDate']").val(),
        Name: jurisdictionElem.find("[name^='JurisdictionName']").val(),
        JurisdictionFile: jurisdictionElem.find("[name^='JurisdictionFile']")[0],
        JurisdictionFileRemove: jurisdictionElem.find("[name^='JurisdictionFileRemove']").val(),
        
    };
}

function jurisdictionToFormData(jurisdiction, address) {
    var formData = new FormData();
    formData.append("Jurisdiction.IdJurisdiction", jurisdiction.IdJurisdiction);
    formData.append("Jurisdiction.Number", jurisdiction.Number);
    formData.append("Jurisdiction.Date", jurisdiction.Date);
    formData.append("Jurisdiction.Name", jurisdiction.Name);
    formData.append("JurisdictionFile", jurisdiction.JurisdictionFile.files[0]);
    formData.append("JurisdictionFileRemove", jurisdiction.JurisdictionFileRemove);
    formData.append("Address.AddressType", address.addressType);
    formData.append("Address.Id", address.id);
    formData.append("Jurisdiction.IdPremises", jurisdiction.IdPremises);
    formData.append("Jurisdiction.IdActFile", jurisdiction.IdActFile);
    formData.append("Jurisdiction.IdActFileTypeDocument", jurisdiction.IdActFileTypeDocument);
    return formData;
}

let getCurrentAddressJurisdictions = function () {
    let address = {
        addressType: $('#jurisdictionsList').data('addresstype'),
        id: $('#jurisdictionsList').data('id')
    };
    return address;
};

let getErrorSpanJurisdictions = function (dataValmsgFor) {
    return "<span class=\"text-danger field-validation-valid\" data-valmsg-for=\"" + dataValmsgFor +
        "\" data-valmsg-replace=\"true\"></span>";
};

let initializeVilidationJurisdiction = function (jurisdictionElem) {

    let idJurisdiction = jurisdictionElem.find("input[name^='IdJurisdiction']").val();
    if (idJurisdiction === "0") idJurisdiction = uuidv4();
    //Дата документа
    let date = 'JurisdictionDate_' + idJurisdiction;
    jurisdictionElem.find("[name^='JurisdictionDate']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Дата документа" является обязательным')
        .attr('id', date)
        .attr('name', date)
        .attr('aria-describedby', date + '-error')
        .after(getErrorSpanJurisdictions(date));
    // Тип документа
    let idJurisdictionTypeName = 'IdJurisdictionType_' + idJurisdiction;
    var jurisdictionTypeElem = jurisdictionElem.find("[name^='IdJurisdictionType']");
    jurisdictionTypeElem.addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Тип документа" является обязательным')
        .attr('id', idJurisdictionTypeName)
        .attr('name', idJurisdictionTypeName)
        .attr('aria-describedby', idJurisdictionTypeName + '-error').parent()
        .after(getErrorSpanJurisdictions(idJurisdictionTypeName));
    jurisdictionTypeElem.next().attr("data-id", idJurisdictionTypeName);

    refreshValidationJurisdictionsForm();
};

let refreshValidationJurisdictionsForm = function () {
    var form = $("#jurisdictionsForm")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
};

let initializeVilidationJurisdictions = function () {
    let jurisdictions = $('#jurisdictionsList .list-group-item');
    jurisdictions.each(function () {
        initializeVilidationJurisdiction($(this));
    });
};

function deleteJurisdiction(e) {
    let isOk = confirm("Вы уверены что хотите удалить документ судебного разбирательства?");
    if (isOk) {
        let jurisdictionElem = $(this).closest(".list-group-item");
        let idJurisdiction = jurisdictionElem.find("input[name^='IdJurisdiction']").val();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/PremisesJurisdictionActFiles/DeleteJurisdiction',
            data: { idJurisdiction: idJurisdiction },
            success: function (ind) {
                if (ind === 1) {
                    jurisdictionElem.remove();
                }
                else {
                    alert("Ошибка удаления!");
                }
            }
        });
    }
    e.preventDefault();
}

function addJurisdiction(e) {
    let action = $('#jurisdictionsList').data('action');
    let addressType = $('#jurisdictionsList').data('addresstype');

    $.ajax({
        type: 'POST',
        url: window.location.origin + '/PremisesJurisdictionActFiles/AddJurisdiction',
        data: { addressType, action },
        success: function (elem) {
            let list = $('#jurisdictionsList');
            let jurisdictionsToggle = $('#jurisdictionsToggle');
            if (!isExpandElemntArrow(jurisdictionsToggle)) // развернуть при добавлении, если было свернуто 
                jurisdictionsToggle.click();
            list.append(elem);
            elem = list.find(".list-group-item").last();
            elem.find("select").selectpicker("refresh");
            elem.find(".jurisdiction-edit-btn").click();
            $([document.documentElement, document.body]).animate({
                scrollTop: $(elem).offset().top
            }, 1000);
            initializeVilidationJurisdiction(elem);
        }
    });
    e.preventDefault();
}

function editJurisdiction(e) {
    let jurisdiction = $(this).closest(".list-group-item");
    let fields = jurisdiction.find('input, select, textarea');
    let yesNoPanel = jurisdiction.find('.yes-no-panel');
    let editDelPanel = jurisdiction.find('.edit-del-panel');
    fields.filter(function (idx, val) { return !$(val).prop("name").startsWith("JurisdictionAttachment"); }).prop('disabled', false);
    jurisdiction.find("select").selectpicker('refresh');
    editDelPanel.hide();
    yesNoPanel.show();
    showJurisdictionEditFileBtns(jurisdiction,
        jurisdiction.find(".rr-jurisdiction-file-download").length > 0 &&
        !jurisdiction.find(".rr-jurisdiction-file-download").hasClass("disabled"));
    e.preventDefault();
}

function cancelEditJurisdiction(e) {
    let jurisdictionElem = $(this).closest(".list-group-item");
    let idJurisdiction = jurisdictionElem.find("input[name^='IdJurisdiction']").val();
    //Отменить изменения внесенные в документ
    if (idJurisdiction !== "0") {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/PremisesJurisdictionActFiles/GetJurisdiction',
            data: { idJurisdiction: idJurisdiction },
            success: function (jurisdiction) {
                refreshJurisdiction(jurisdictionElem, jurisdiction);
                showEditDelPanelJurisdiction(jurisdictionElem);
                clearValidationsJurisdictions(jurisdictionElem);
                showJurisdictionDownloadFileBtn(jurisdictionElem, jurisdiction.fileOriginName !== null);
            }
        });
    }
    //Отменить вставку нового документа
    else {
        jurisdictionElem.remove();
    }
    e.preventDefault();
}

function showJurisdictionEditFileBtns(jurisdictionElem, fileExists) {
    let jurisdictionFileBtns = jurisdictionElem.find(".rr-jurisdiction-file-buttons");
    jurisdictionElem.find(".rr-jurisdiction-file-download").hide();
    if (fileExists) {
        jurisdictionFileBtns.append(jurisdictionElem.find(".rr-jurisdiction-file-remove").show());
        jurisdictionElem.find(".rr-jurisdiction-file-attach").hide();
    } else {
        jurisdictionElem.find(".rr-jurisdiction-file-remove").hide();
        jurisdictionFileBtns.append(jurisdictionElem.find(".rr-jurisdiction-file-attach").show());
    }
}

function showJurisdictionDownloadFileBtn(jurisdictionElem, fileExists) {
    let jurisdictionFileBtns = jurisdictionElem.find(".rr-jurisdiction-file-buttons");
    jurisdictionFileBtns.append(jurisdictionElem.find(".rr-jurisdiction-file-download").show());
    if (fileExists) {
        jurisdictionElem.find(".rr-jurisdiction-file-download").removeClass("disabled");
    } else {
        jurisdictionElem.find(".rr-jurisdiction-file-download").addClass("disabled");
    }
    jurisdictionElem.find(".rr-jurisdiction-file-remove").hide();
    jurisdictionElem.find(".rr-jurisdiction-file-attach").hide();
}

function clearValidationsJurisdictions(jurisdictionElem) {
    $(jurisdictionElem).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
    $(jurisdictionElem).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");
}

function showEditDelPanelJurisdiction(jurisdictionElem) {
    let fields = jurisdictionElem.find('input, select, textarea');
    fields.prop('disabled', true).selectpicker('refresh');
    let editDelPanel = jurisdictionElem.find('.edit-del-panel');
    let yesNoPanel = jurisdictionElem.find('.yes-no-panel');
    yesNoPanel.hide();
    editDelPanel.show();
}

function refreshJurisdiction(jurisdictionElem, jurisdiction) {
    jurisdictionElem.find("[name^='JurisdictionNum']").val(jurisdiction.number);
    jurisdictionElem.find("[name^='JurisdictionDate']").val(jurisdiction.date);
    jurisdictionElem.find("[name^='JurisdictionName']").val(jurisdiction.name);
    jurisdictionElem.find("[name^='IdJurisdictionType']").val(jurisdiction.idJurisdictionType).selectpicker('refresh');
    jurisdictionElem.find("[name^='JurisdictionFile']").val("");
    jurisdictionElem.find("[name^='JurisdictionFileRemove']").val(false);
}

function saveJurisdiction(e) {
    let jurisdictionElem = $(this).closest(".list-group-item");
    jurisdictionElem.find("button[data-id]").removeClass("input-validation-error");
    if (jurisdictionElem.find("input, textarea, select").valid()) {
        let jurisdiction = jurisdictionToFormData(getJurisdiction(jurisdictionElem), getCurrentAddressJurisdictions());
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/PremisesJurisdictionActFiles/SaveJurisdiction',
            data: jurisdiction,
            processData: false,
            contentType: false,
            success: function (jurisdiction) {
                if (jurisdiction.idJurisdiction > 0) {
                    jurisdictionElem.find("input[name^='IdJurisdiction']").val(jurisdiction.idJurisdiction);
                    jurisdictionElem.find(".rr-jurisdiction-file-download")
                        .prop("href", "/PremisesJurisdictionActFiles/DownloadFile/?idJurisdiction=" + jurisdiction.idJurisdiction);
                    showJurisdictionDownloadFileBtn(jurisdictionElem, jurisdiction.fileOriginName !== null);
                }
                showEditDelPanelJurisdiction(jurisdictionElem);
            }
        });
    } else {
        jurisdictionElem.find("select").each(function (idx, elem) {
            var id = $(elem).prop("id");
            var name = $(elem).prop("name");
            var errorSpan = $("span[data-valmsg-for='" + name + "']");
            if (errorSpan.hasClass("field-validation-error")) {
                $("button[data-id='" + id + "']").addClass("input-validation-error");
            }
        });
    }
    e.preventDefault();
}

function attachJurisdictionFile(e) {
    var jurisdictionElem = $(this).closest(".list-group-item");
    jurisdictionElem.find("input[name^='JurisdictionFile']").click();
    jurisdictionElem.find("input[name^='JurisdictionFileRemove']").val(false);
    e.preventDefault();
}

function changeJurisdictionFileAttachment() {
    var jurisdictionElem = $(this).closest(".list-group-item");
    if ($(this).val() !== "") {
        let jurisdictionFileBtns = jurisdictionElem.find(".rr-jurisdiction-file-buttons");
        jurisdictionElem.find(".rr-jurisdiction-file-attach").hide();
        jurisdictionFileBtns.append(jurisdictionElem.find(".rr-jurisdiction-file-remove").show());
    }
}

function removeJurisdictionFile(e) {
    var jurisdictionElem = $(this).closest(".list-group-item");
    jurisdictionElem.find("input[name^='JurisdictionFile']").val("");
    jurisdictionElem.find("input[name^='JurisdictionFileRemove']").val(true);
    let jurisdictionFileBtns = jurisdictionElem.find(".rr-jurisdiction-file-buttons");
    jurisdictionElem.find(".rr-jurisdiction-file-remove").hide();
    jurisdictionFileBtns.append(jurisdictionElem.find(".rr-jurisdiction-file-attach").show());
    e.preventDefault();
}

$(function () {
    $('#jurisdictionsList').hide();
    $('.yes-no-panel').hide();
    initializeVilidationJurisdictions();
    $('#jurisdictionAdd').click(addJurisdiction);
    $('#jurisdictionsToggle').on('click', $('#jurisdictionsList'), elementToogle);
    $('#jurisdictionsList').on('click', '.jurisdiction-edit-btn', editJurisdiction);
    $('#jurisdictionsList').on('click', '.jurisdiction-cancel-btn', cancelEditJurisdiction);
    $('#jurisdictionsList').on('click', '.jurisdiction-save-btn', saveJurisdiction);
    $('#jurisdictionsList').on('click', '.jurisdiction-delete-btn', deleteJurisdiction);
    $('#jurisdictionsList').on('click', '.rr-jurisdiction-file-attach', attachJurisdictionFile);
    $('#jurisdictionsList').on('click', '.rr-jurisdiction-file-remove', removeJurisdictionFile);
    $('#jurisdictionsList').on('change', "input[name^='JurisdictionFile']", changeJurisdictionFileAttachment);
});