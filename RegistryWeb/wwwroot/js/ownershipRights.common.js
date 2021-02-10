let CreateOwnershipPremisesAssoc = function () {
    let ownershipRights = getOwnershipRights();
    let ownershipPremisesAssoc = [];
    ownershipRights.each(function (idx, item) {
        ownershipPremisesAssoc.push({
            IdPremises: 0,
            IdOwnershipRight: 0,
            OwnershipRightNavigation: item
        });
    });
    return ownershipPremisesAssoc;
}
let CreateOwnershipBuildingsAssoc = function () {
    let ownershipRights = getOwnershipRights();
    let ownershipBuildingsAssoc = [];
    ownershipRights.each(function (idx, item) {
        ownershipBuildingsAssoc.push({
            IdBuilding: 0,
            IdOwnershipRight: 0,
            OwnershipRightNavigation: item
        });
    });
    return ownershipBuildingsAssoc;
}

function getOwnershipRights() {
    return $("#ownershipRightsList .list-group-item").map(function (idx, elem) {
        return getOwnershipRight($(elem));
    });
}

function getOwnershipRight(owElem) {
    return {
        IdOwnershipRight: owElem.find("[name^='IdOwnershipRight']").val(),
        Number: owElem.find("[name^='OwnershipRightNum']").val(),
        Date: owElem.find("[name^='OwnershipRightDate']").val(),
        Description: owElem.find("[name^='OwnershipRightDescription']").val(),
        IdOwnershipRightType: owElem.find("[name^='IdOwnershipRightType']").val(),
        ResettlePlanDate: owElem.find("[name^='ResettlePlanDate']").val(),
        DemolishPlanDate: owElem.find("[name^='DemolishPlanDate']").val(),
        OwnershipRightFile: owElem.find("[name^='OwnershipRightFile']")[0],
        OwnershipRightFileRemove: owElem.find("[name^='OwnershipRightFileRemove']").val()
    };
}

function ownershipRightToFormData(owr, address) {
    var formData = new FormData();
    formData.append("OwnershipRight.IdOwnershipRight", owr.IdOwnershipRight);
    formData.append("OwnershipRight.Number", owr.Number);
    formData.append("OwnershipRight.Date", owr.Date);
    formData.append("OwnershipRight.Description", owr.Description);
    formData.append("OwnershipRight.IdOwnershipRightType", owr.IdOwnershipRightType);
    formData.append("OwnershipRight.ResettlePlanDate", owr.ResettlePlanDate);
    formData.append("OwnershipRight.DemolishPlanDate", owr.DemolishPlanDate);
    formData.append("OwnershipRightFile", owr.OwnershipRightFile.files[0]);
    formData.append("OwnershipRightFileRemove", owr.OwnershipRightFileRemove);
    formData.append("Address.AddressType", address.addressType);
    formData.append("Address.Id", address.id);
    return formData;
}

let getCurrentAddressOwnershipRights = function () {
    let address = {
        addressType: $('#ownershipRightsList').data('addresstype'),
        id: $('#ownershipRightsList').data('id')
    };
    return address;
};

let getErrorSpanOwnershipRights = function (dataValmsgFor) {
    return "<span class=\"text-danger field-validation-valid\" data-valmsg-for=\"" + dataValmsgFor +
        "\" data-valmsg-replace=\"true\"></span>";
};

let initializeVilidationOwnershipRight = function (ownershipRightElem) {

    let idOwnershipRight = ownershipRightElem.find("input[name^='IdOwnershipRight']").val();
    if (idOwnershipRight === "0") idOwnershipRight = uuidv4();
    //Дата документа
    let date = 'OwnershipRightDate_' + idOwnershipRight;
    ownershipRightElem.find("[name^='OwnershipRightDate']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Дата документа" является обязательным')
        .attr('id', date)
        .attr('name', date)
        .attr('aria-describedby', date + '-error')
        .after(getErrorSpanOwnershipRights(date));
    // Тип документа
    let idOwnershipRightTypeName = 'IdOwnershipRightType_' + idOwnershipRight;
    var ownershipRightTypeElem = ownershipRightElem.find("[name^='IdOwnershipRightType']");
    ownershipRightTypeElem.addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Тип документа" является обязательным')
        .attr('id', idOwnershipRightTypeName)
        .attr('name', idOwnershipRightTypeName)
        .attr('aria-describedby', idOwnershipRightTypeName + '-error').parent()
        .after(getErrorSpanOwnershipRights(idOwnershipRightTypeName));
    ownershipRightTypeElem.next().attr("data-id", idOwnershipRightTypeName);

    refreshValidationOwnershipRightsForm();
};

let refreshValidationOwnershipRightsForm = function () {
    var form = $("#ownershipRightsForm")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
};

let initializeVilidationOwnershipRights = function () {
    let owrs = $('#ownershipRightsList .list-group-item');
    owrs.each(function () {
        initializeVilidationOwnershipRight($(this));
    });
};

function deleteOwnershipRight(e) {
    let isOk = confirm("Вы уверены что хотите удалить ограничение?");
    if (isOk) {
        let owrElem = $(this).closest(".list-group-item");
        let idOwnershipRight = owrElem.find("input[name^='IdOwnershipRight']").val();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/OwnershipRights/DeleteOwnershipRight',
            data: { idOwnershipRight: idOwnershipRight },
            success: function (ind) {
                if (ind === 1) {
                    owrElem.remove();

                    if ($("#ownershipRightsList").find('.list-group-item').length - 1 > 0)
                        $(".ownershipRightsbadge").text($("#ownershipRightsList").find('.list-group-item').length - 1);
                    else
                        $(".ownershipRightsbadge").css("display", "none");
                }
                else {
                    alert("Ошибка удаления!");
                }
            }
        });
    }
    e.preventDefault();
}

function addOwnershipRight(e) {
    let action = $('#ownershipRightsList').data('action');
    let addressType = $('#ownershipRightsList').data('addresstype');

    $.ajax({
        type: 'POST',
        url: window.location.origin + '/OwnershipRights/AddOwnershipRight',
        data: { addressType, action },
        success: function (elem) {
            let list = $('#ownershipRightsList');
            let ownershipRightsToggle = $('#ownershipRightsToggle');
            if (!isExpandElemntArrow(ownershipRightsToggle)) // развернуть при добавлении, если было свернуто 
                ownershipRightsToggle.click();
            list.append(elem);
            elem = list.find(".list-group-item").last();
            elem.find("select").selectpicker("refresh");
            elem.find(".ownership-right-edit-btn").click();
            $([document.documentElement, document.body]).animate({
                scrollTop: $(elem).offset().top
            }, 1000);
            initializeVilidationOwnershipRight(elem);
        }
    });
    e.preventDefault();
}

function editOwnershipRight(e) {
    let owr = $(this).closest(".list-group-item");
    let fields = owr.find('input, select, textarea');
    let yesNoPanel = owr.find('.yes-no-panel');
    let editDelPanel = owr.find('.edit-del-panel');
    fields.filter(function (idx, val) { return !$(val).prop("name").startsWith("OwnershipRightAttachment"); }).prop('disabled', false);
    owr.find("select").selectpicker('refresh');
    editDelPanel.hide();
    yesNoPanel.show();
    showOwnershipRightEditFileBtns(owr,
        owr.find(".rr-ownership-right-file-download").length > 0 &&
        !owr.find(".rr-ownership-right-file-download").hasClass("disabled"));
    e.preventDefault();
}

function cancelEditOwnershipRight(e) {
    let owrElem = $(this).closest(".list-group-item");
    let idOwnershipRight = owrElem.find("input[name^='IdOwnershipRight']").val();
    //Отменить изменения внесенные в документ
    if (idOwnershipRight !== "0") {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/OwnershipRights/GetOwnershipRight',
            data: { idOwnershipRight: idOwnershipRight },
            success: function (owr) {
                refreshOwnershipRight(owrElem, owr);
                showEditDelPanelOwnershipRight(owrElem);
                clearValidationsOwnershipRights(owrElem);
                showOwnershipRightDownloadFileBtn(owrElem, owr.fileOriginName !== null);
            }
        });
    }
    //Отменить вставку нового документа
    else {
        owrElem.remove();
    }
    e.preventDefault();
}

function showOwnershipRightEditFileBtns(owrElem, fileExists) {
    let owrFileBtns = owrElem.find(".rr-ownership-right-file-buttons");
    owrElem.find(".rr-ownership-right-file-download").hide();
    if (fileExists) {
        owrFileBtns.append(owrElem.find(".rr-ownership-right-file-remove").show());
        owrElem.find(".rr-ownership-right-file-attach").hide();
    } else {
        owrElem.find(".rr-ownership-right-file-remove").hide();
        owrFileBtns.append(owrElem.find(".rr-ownership-right-file-attach").show());
    }
}

function showOwnershipRightDownloadFileBtn(owrElem, fileExists) {
    let owrFileBtns = owrElem.find(".rr-ownership-right-file-buttons");
    owrFileBtns.append(owrElem.find(".rr-ownership-right-file-download").show());
    if (fileExists) {
        owrElem.find(".rr-ownership-right-file-download").removeClass("disabled");
    } else {
        owrElem.find(".rr-ownership-right-file-download").addClass("disabled");
    }
    owrElem.find(".rr-ownership-right-file-remove").hide();
    owrElem.find(".rr-ownership-right-file-attach").hide();
}

function clearValidationsOwnershipRights(owrElem) {
    $(owrElem).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
    $(owrElem).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");
}

function showEditDelPanelOwnershipRight(owrElem) {
    let fields = owrElem.find('input, select, textarea');
    fields.prop('disabled', true).selectpicker('refresh');
    let editDelPanel = owrElem.find('.edit-del-panel');
    let yesNoPanel = owrElem.find('.yes-no-panel');
    yesNoPanel.hide();
    owrElem.removeClass("list-group-item-warning");
    editDelPanel.show();
}

function refreshOwnershipRight(owrElem, owr) {
    owrElem.find("[name^='OwnershipRightNum']").val(owr.number);
    owrElem.find("[name^='OwnershipRightDate']").val(owr.date);
    owrElem.find("[name^='OwnershipRightDescription']").val(owr.description);
    owrElem.find("[name^='IdOwnershipRightType']").val(owr.idOwnershipRightType).selectpicker('refresh');
    owrElem.find("[name^='DemolishPlanDate']").val(owr.demolishPlanDate);
    owrElem.find("[name^='ResettlePlanDate']").val(owr.resettlePlanDate);
    owrElem.find("[name^='OwnershipRightFile']").val("");
    owrElem.find("[name^='OwnershipRightFileRemove']").val(false);
}

function saveOwnershipRight(e) {
    let owrElem = $(this).closest(".list-group-item");
    owrElem.find("button[data-id]").removeClass("input-validation-error");
    if (owrElem.find("input, textarea, select").valid()) {
        let owr = ownershipRightToFormData(getOwnershipRight(owrElem), getCurrentAddressOwnershipRights());
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/OwnershipRights/SaveOwnershipRight',
            data: owr,
            processData: false,
            contentType: false,
            success: function (owr) {
                if (owr.idOwnershipRight > 0) {
                    owrElem.find("input[name^='IdOwnershipRight']").val(owr.idOwnershipRight);
                    owrElem.find(".rr-ownership-right-file-download").prop("href", "/OwnershipRights/DownloadFile/?idOwnershipRight=" + owr.idOwnershipRight);
                    showOwnershipRightDownloadFileBtn(owrElem, owr.fileOriginName !== null);

                    if ($("#ownershipRightsList").find('.list-group-item').length - 1 > 0) {
                        $(".ownershipRightsbadge").text($("#ownershipRightsList").find('.list-group-item').length + 1);
                        $(".ownershipRightsbadge").css("display", "inline-block");
                    }
                }
                showEditDelPanelOwnershipRight(owrElem);
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
        $([document.documentElement, document.body]).animate({
            scrollTop: owrElem.find(".input-validation-error").first().offset().top - 35
        }, 1000);
    }
    e.preventDefault();
}

function attachOwnershipRightFile(e) {
    var owrElem = $(this).closest(".list-group-item");
    owrElem.find("input[name^='OwnershipRightFile']").click();
    owrElem.find("input[name^='OwnershipRightFileRemove']").val(false);
    e.preventDefault();
}

function changeOwnershipRightFileAttachment() {
    var owrElem = $(this).closest(".list-group-item");
    if ($(this).val() !== "") {
        let owrFileBtns = owrElem.find(".rr-ownership-right-file-buttons");
        owrElem.find(".rr-ownership-right-file-attach").hide();
        owrFileBtns.append(owrElem.find(".rr-ownership-right-file-remove").show());
        var descriptionElem = owrElem.find("input[name^='OwnershipRightDescription']");
        if (descriptionElem.val() === "") {
            descriptionElem.val($(this)[0].files[0].name);
        }
    }
}

function removeOwnershipRightFile(e) {
    var owrElem = $(this).closest(".list-group-item");
    owrElem.find("input[name^='OwnershipRightFile']").val("");
    owrElem.find("input[name^='OwnershipRightFileRemove']").val(true);
    let owrFileBtns = owrElem.find(".rr-ownership-right-file-buttons");
    owrElem.find(".rr-ownership-right-file-remove").hide();
    owrFileBtns.append(owrElem.find(".rr-ownership-right-file-attach").show());
    e.preventDefault();
}

$(function () {
    $('.yes-no-panel').hide();
    initializeVilidationOwnershipRights();
    $('#ownershipRightAdd').click(addOwnershipRight);
    $('#ownershipRightsToggle').on('click', $('#ownershipRightsList'), elementToogleHide);
    $('#ownershipRightsList').on('click', '.ownership-right-edit-btn', editOwnershipRight);
    $('#ownershipRightsList').on('click', '.ownership-right-cancel-btn', cancelEditOwnershipRight);
    $('#ownershipRightsList').on('click', '.ownership-right-save-btn', saveOwnershipRight);
    $('#ownershipRightsList').on('click', '.ownership-right-delete-btn', deleteOwnershipRight);
    $('#ownershipRightsList').on('click', '.rr-ownership-right-file-attach', attachOwnershipRightFile);
    $('#ownershipRightsList').on('click', '.rr-ownership-right-file-remove', removeOwnershipRightFile);
    $('#ownershipRightsList').on('change', "input[name^='OwnershipRightFile']", changeOwnershipRightFileAttachment);

    if ($("#ownershipRightsList").find('.list-group-item').length == 0)
        $(".ownershipRightsbadge").css("display", "none");
});