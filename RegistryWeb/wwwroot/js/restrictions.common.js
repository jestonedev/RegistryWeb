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
        IdRestrictionType: restrictionElem.find("[name^='IdRestrictionType']").val()
    };
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
    var restrictionTypeElem = restrictionElem.find("[name^='IdRetrctionType']");
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

let showEditDelPanelRetriction = function (restrictionElem) {
    let fields = restrictionElem.find('input, select, textarea');
    fields.prop('disabled', true).selectpicker('refresh');
    let editDelPanel = restrictionElem.find('.edit-del-panel');
    let yesNoPanel = restrictionElem.find('.yes-no-panel');
    yesNoPanel.hide();
    editDelPanel.show();
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
            }
        });
    }
    //Отменить вставку нового документа
    else {
        restrictionElem.remove();
    }
    e.preventDefault();
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
    restrictionElem.find("[name^='IdRetrctionType']").val(restriction.idRestrictionType).selectpicker('refresh');
}

function saveRestriction(e) {
    let restrictionElem = $(this).closest(".list-group-item");
    restrictionElem.find("button[data-id]").removeClass("input-validation-error");
    if (restrictionElem.find("input, textarea, select").valid()) {
        let restriction = getRestriction(restrictionElem);
        let address = getCurrentAddressRestrictions();
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Restrictions/SaveRestriction',
            data: { restriction, address },
            success: function (idRestriction) {
                if (idRestriction > 0) {
                    restrictionElem.find("input[name^='IdRestriction']").val(idRestriction);
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
    }
    e.preventDefault();
}

$(function () {
    $('#restrictionsList').hide();
    $('.yes-no-panel').hide();
    initializeVilidationRestricitons();
    $('#restrictionAdd').click(addRestriction);
    $('#restrictionsToggle').on('click', $('#restrictionsList'), elementToogle);
    $('#restrictionsList').on('click', '.restriction-edit-btn', editRestriction);
    $('#restrictionsList').on('click', '.restriction-cancel-btn', cancelEditRestriction);
    $('#restrictionsList').on('click', '.restriction-save-btn', saveRestriction);
    $('#restrictionsList').on('click', '.restriction-delete-btn', deleteRestriction);
});