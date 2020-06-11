let CreateRestrictionPremisesAssoc = function () {
    let restrictions = getRestrictions();
    let restrictionPremisesAssoc = [];
    restrictions.forEach(function (item, i, arr) {
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
    restrictions.forEach(function (item, i, arr) {
        restrictionsBuildingsAssoc.push({
            IdBuilding: 0,
            IdRestriction: 0,
            RestrictionNavigation: item
        });
    });
    return restrictionsBuildingsAssoc;
};

let getRestrictions = function () {
    let trs = $('#restrictions>tr');
    let restrictions = [];
    trs.each(function () {
        restrictions.push(getRestriction($(this)));
    });
    return restrictions;
};

let getRestriction = function (tr) {
    let fields = tr.find('.field-restriction');
    let restriction = {
        IdRestriction: tr.data('idrestriction') === "" ? "0" : tr.data('idrestriction'),
        Number: fields[0].value,
        Date: fields[1].value,
        Description: fields[2].value,
        IdRestrictionType: fields[3].value
    };
    return restriction;
};

let getCurrentAddressRestrictions = function () {
    let address = {
        addressType: $('#restrictions').data('addresstype'),
        id: $('#restrictions').data('id')
    };
    return address;
};

let getErrorSpanRestrictions = function (dataValmsgFor) {
    return "<span class=\"text-danger field-validation-valid\" data-valmsg-for=\"" + dataValmsgFor +
        "\" data-valmsg-replace=\"true\"></span>";
};

let initializeVilidationTrRestriction = function (tr) {
    let fields = tr.find('.field-restriction');
    let idRestriction = tr.data('idrestriction');
    //Дата
    let date = 'Date_' + idRestriction;
    $(fields[1]).addClass('valid');
    $(fields[1]).attr('data-val', 'true');
    $(fields[1]).attr('data-val-required', 'Поле "Дата" является обязательным');
    $(fields[1]).attr('id', date);
    $(fields[1]).attr('name', date);
    $(fields[1]).attr('aria-describedby', date + '-error');
    $(fields[1]).after(getErrorSpanRestrictions(date));
    //Тип реквизита права собственности
    let restrictionType = 'IdRestriction_' + idRestriction;
    $(fields[3]).addClass('valid');
    $(fields[3]).attr('data-val', 'true');
    $(fields[3]).attr('data-val-required', 'Поле "Тип" является обязательным');
    $(fields[3]).attr('id', restrictionType);
    $(fields[3]).attr('name', restrictionType);
    $(fields[3]).attr('aria-describedby', restrictionType + '-error');
    $(fields[3]).after(getErrorSpanRestrictions(restrictionType));

    refreshValidationRestrictionsForm();
};

let refreshValidationRestrictionsForm = function () {
    var form = $("#restrictionsForm")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
};

let initializeVilidationTrsRestriciton = function () {
    let trs = $('#restrictions>tr');
    trs.each(function () {
        initializeVilidationTrRestriction($(this));
    });
};

let refreshRestriction = function (tr, restriction) {
    let fields = tr.find('.field-restriction');
    //Номер
    $(fields[0]).prop('value', restriction.number);
    $(fields[0]).prop('title', restriction.number);
    //Дата
    $(fields[1]).prop('value', restriction.date);
    $(fields[1]).prop('title', restriction.date);
    $(fields[1])
        .removeClass('input-validation-error')
        .addClass('valid');
    $(fields[1]).next()
        .removeClass('field-validation-error')
        .addClass('field-validation-valid')
        .text('');
    //Наименование
    $(fields[2]).prop('value', restriction.description);
    $(fields[2]).prop('title', restriction.description);
    //Тип ограничения
    $(fields[3]).prop('value', restriction.idRestrictionType);
    $(fields[3]).prop('title', restriction.idRestrictionType);
    $(fields[3])
        .removeClass('input-validation-error')
        .addClass('valid');
    $(fields[3]).next()
        .removeClass('field-validation-error')
        .addClass('field-validation-valid')
        .text('');
};

let showEditDelPanelRetriction = function (tr) {
    let fields = tr.find('.field-restriction');
    let editDelPanel = tr.find('.edit-del-panel');
    let yesNoPanel = tr.find('.yes-no-panel');
    fields.prop('disabled', true);
    fields.each(function (idx, field) {
        if (field.tagName === "INPUT" || field.tagName === "TEXTAREA") {
            $(field).prop("title", $(field).val());
        } else
            if (field.tagName === "SELECT") {
                $(field).prop("title", $(field).find("option[value='" + $(field).val() + "']").text());
            } else {
                $(field).prop("title", "");
            }
    });
    yesNoPanel.hide();
    editDelPanel.show();
};

let showYesNoPanelRetriction = function (tr) {
    let fields = tr.find('.field-restriction');
    let yesNoPanel = tr.find('.yes-no-panel');
    let editDelPanel = tr.find('.edit-del-panel');
    fields.prop('disabled', false);
    editDelPanel.hide();
    yesNoPanel.show();
};

let restrictionAddClick = function (event) {
    let addressType = $('#restrictions').data('addresstype');
    let action = $('#restrictions').data('action');
    $.ajax({
        async: false,
        type: 'POST',
        url: window.location.origin + '/Restrictions/AddRestriction',
        data: { addressType, action },
        success: function (tr) {
            let trs = $('#restrictions');
            let restrictionsToggle = $('#restrictionsToggle');
            if (!isExpandElemntArrow(restrictionsToggle)) // развернуть при добавлении, если было свернуто 
                restrictionsToggle.click();
            trs.append(tr);
            initializeVilidationTrRestriction(trs.find('tr').last());
            event.preventDefault();
        }
    });
};

let restrictionDeleteClick = function (tr) {
    let isOk = confirm("Вы уверены что хотите удалить реквизит?");
    if (isOk) {
        let idRestriction = tr.data('idrestriction');
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/Restrictions/DeleteRestriction',
            data: { idRestriction: idRestriction },
            success: function (ind) {
                if (ind == 1) {
                    tr.remove();
                }
                else {
                    alert("Ошибка удаления!");
                }
            }
        });
    }
};

let restrictionNoClick = function (tr) {
    let idRestriction = tr.data('idrestriction');
    //Отменить изменения внесенные в реквизит
    if (Number.isInteger(idRestriction)) {
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/Restrictions/GetRestriction',
            data: { idRestriction: idRestriction },
            success: function (restriction) {
                refreshRestriction(tr, restriction);
                showEditDelPanelRetriction(tr);
            }
        });
    }
    //Отменить вставку нового реквизита
    else {
        tr.remove();
    }
};

let restrictionYesClick = function (tr) {
    //Запуск ручной валидации, тк отсутсвует submit
    if ($('#restrictionsForm').valid()) {
        let restriction = getRestriction(tr);
        let address = getCurrentAddressRestrictions();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/Restrictions/YesRestriction',
            data: { restriction, address },
            success: function (idRestriction) {
                if (idRestriction > 0) {
                    tr.data('idrestriction', idRestriction);
                }
                showEditDelPanelRetriction(tr);
            }
        });
    }
};

let restrictionsClick = function (event) {
    let el = $(event.target);
    let tr = el.parents('tr');
    if (el.hasClass('oi-x')) {
        if (el.hasClass('delete')) {
            restrictionDeleteClick(tr);
        }
        else {
            restrictionNoClick(tr);
        }
    }
    if (el.hasClass('oi-check')) {
        restrictionYesClick(tr);
    }
    if (el.hasClass('oi-pencil')) {
        showYesNoPanelRetriction(tr);
    }
};

$(function () {
    $('#restrictionsTable').hide();
    $('.yes-no-panel').hide();
    initializeVilidationTrsRestriciton();
    $('#restrictionsToggle').on('click', $('#restrictionsTable'), elementToogle);
    $('#restrictionAdd').click(restrictionAddClick);
    $('#restrictions').click(restrictionsClick);
});