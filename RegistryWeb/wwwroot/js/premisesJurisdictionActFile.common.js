/*let CreateOwnershipPremisesAssoc = function () {
    let PremisesJurisdictions = getPremisesJurisdictions();
    let ownershipPremisesAssoc = [];
    PremisesJurisdictions.forEach(function (item, i, arr) {
        ownershipPremisesAssoc.push({
            IdPremises: 0,
            IdPremisesJurisdiction: 0,
            PremisesJurisdictionNavigation: item
        });
    });
    return ownershipPremisesAssoc;
}
let CreateOwnershipBuildingsAssoc = function () {
    let PremisesJurisdictions = getPremisesJurisdictions();
    let ownershipBuildingsAssoc = [];
    PremisesJurisdictions.forEach(function (item, i, arr) {
        ownershipBuildingsAssoc.push({
            IdBuilding: 0,
            IdPremisesJurisdiction: 0,
            PremisesJurisdictionNavigation: item
        });
    });
    return ownershipBuildingsAssoc;
}*/
let getPremisesJurisdictions = function () {
    let trs = $('#PremisesJurisdictions>tr');
    let owrs = [];
    trs.each(function () {
        owrs.push(getPremisesJurisdiction($(this)));
    });
    return owrs;
}
let getPremisesJurisdiction = function (tr) {
    let fields = tr.find('.field-premises-jurisdiction');
    let PremisesJurisdiction = {
        IdPremisesJurisdiction: tr.data('idPremisesJurisdiction') == "" ? "0" : tr.data('idPremisesJurisdiction'),
        Number: fields[0].value,
        Date: fields[1].value,
        Description: fields[2].value,
        IdPremisesJurisdictionType: fields[3].value,
        ResettlePlanDate: fields[4].value,
        DemolishPlanDate: fields[5].value
    };
    return PremisesJurisdiction;
}
let getCurrentAddressJurisdictions = function () {
    let address = {
        addressType: $('#PremisesJurisdictions').data('addresstype'),
        id: $('#PremisesJurisdictions').data('id')
    };
    return address;
}
let getErrorSpanJurisdictions = function (dataValmsgFor) {
    return "<span class=\"text-danger field-validation-valid\" data-valmsg-for=\"" + dataValmsgFor +
        "\" data-valmsg-replace=\"true\"></span>";
}
let initializeVilidationJurisdictionTr = function (tr) {
    let fields = tr.find('.field-premises-jurisdiction');
    let idPremisesJurisdiction = tr.data('idownershipright');
    //Дата
    let Date = 'Date_' + idPremisesJurisdiction;
    $(fields[1]).addClass('valid');
    $(fields[1]).attr('data-val', 'true');
    $(fields[1]).attr('data-val-required', 'Поле "Дата" является обязательным');
    $(fields[1]).attr('id', Date);
    $(fields[1]).attr('name', Date);
    $(fields[1]).attr('aria-describedby', Date + '-error');
    $(fields[1]).after(getErrorSpanJurisdictions(Date));
    //Тип ограничения
    let PremisesJurisdictionType = 'IdPremisesJurisdictionType_' + idPremisesJurisdiction;
    $(fields[3]).addClass('valid');
    $(fields[3]).attr('data-val', 'true');
    $(fields[3]).attr('data-val-required', 'Поле "Тип" является обязательным');
    $(fields[3]).attr('id', PremisesJurisdictionType);
    $(fields[3]).attr('name', PremisesJurisdictionType);
    $(fields[3]).attr('aria-describedby', PremisesJurisdictionType + '-error');
    $(fields[3]).after(getErrorSpanJurisdictions(PremisesJurisdictionType));

    refreshValidationPremisesJurisdictionsForm();
}
let refreshValidationPremisesJurisdictionsForm = function () {
    var form = $("#PremisesJurisdiction")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
}
let initializeVilidationJurisdictionTrs = function () {
    let trs = $('#PremisesJurisdictions>tr');
    trs.each(function () {
        initializeVilidationJurisdictionTr($(this));
    });
}
let refreshPremisesJurisdiction = function (tr, PremisesJurisdiction) {
    let fields = tr.find('.field-premises-jurisdiction');
    //Номер
    $(fields[0]).prop('value', PremisesJurisdiction.number);
    $(fields[0]).prop('title', PremisesJurisdiction.number);
    //Дата
    $(fields[1]).prop('value', PremisesJurisdiction.date);
    $(fields[1]).prop('title', PremisesJurisdiction.date);
    $(fields[1])
        .removeClass('input-validation-error')
        .addClass('valid');
    $(fields[1]).next()
        .removeClass('field-validation-error')
        .addClass('field-validation-valid')
        .text('');
    //Наименование
    $(fields[2]).prop('value', PremisesJurisdiction.description);
    $(fields[2]).prop('title', PremisesJurisdiction.description);
    //Тип ограничения
    $(fields[3]).prop('value', PremisesJurisdiction.idPremisesJurisdictionType);
    $(fields[3]).prop('title', PremisesJurisdiction.idPremisesJurisdictionType);
    $(fields[3])
        .removeClass('input-validation-error')
        .addClass('valid');
    $(fields[3]).next()
        .removeClass('field-validation-error')
        .addClass('field-validation-valid')
        .text('');
    //Планируемая дата переселения
    $(fields[4]).prop('value', PremisesJurisdiction.resettlePlanDate);
    $(fields[4]).prop('title', PremisesJurisdiction.resettlePlanDate);
    //Планируемая дата сноса
    $(fields[5]).prop('value', PremisesJurisdiction.demolishPlanDate);
    $(fields[5]).prop('title', PremisesJurisdiction.demolishPlanDate);
}
let showEditDelPanelJurisdiction = function (tr) {
    let fields = tr.find('.field-premises-jurisdiction');
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
}
let showYesNoPanelJurisdiction = function (tr) {
    let fields = tr.find('.field-premises-jurisdiction');
    let yesNoPanel = tr.find('.yes-no-panel');
    let editDelPanel = tr.find('.edit-del-panel');
    fields.prop('disabled', false);
    editDelPanel.hide();
    yesNoPanel.show();
}
let PremisesJurisdictionAddClick = function (event) {
    let addressType = $('#PremisesJurisdictions').data('addresstype');
    let action = $('#PremisesJurisdictions').data('action');
    $.ajax({
        async: false,
        type: 'POST',
        url: window.location.origin + '/PremisesJurisdictions/AddPremisesJurisdiction',
        data: { addressType, action },
        success: function (tr) {
            let trs = $('#PremisesJurisdictions');
            let PremisesJurisdictionsToggle = $('#PremisesJurisdictionsToggle');
            if (!isExpandElemntArrow(PremisesJurisdictionsToggle)) // развернуть при добавлении, если было свернуто 
                PremisesJurisdictionsToggle.click();
            trs.append(tr);
            initializeVilidationJurisdictionTr(trs.find('tr').last());
            event.preventDefault();
        }
    });
}
let PremisesJurisdictionDeleteClick = function (tr) {
    let isOk = confirm("Вы уверены что хотите удалить ограничение?");
    if (isOk) {
        let idPremisesJurisdiction = tr.data('idPremisesJurisdiction');
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/PremisesJurisdictions/DeletePremisesJurisdiction',
            data: { idPremisesJurisdiction: idPremisesJurisdiction },
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
}
let PremisesJurisdictionNoClick = function (tr) {
    let idPremisesJurisdiction = tr.data('idownershipright');
    //Отменить изменения внесенные в ограничение
    if (Number.isInteger(idPremisesJurisdiction)) {
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/PremisesJurisdictions/GetPremisesJurisdiction',
            data: { idPremisesJurisdiction: idPremisesJurisdiction },
            success: function (PremisesJurisdiction) {
                refreshPremisesJurisdiction(tr, PremisesJurisdiction);
                showEditDelPanelJurisdiction(tr);
            }
        });
    }
    //Отменить вставку нового ограничения
    else {
        tr.remove();
    }
}
let PremisesJurisdictionYesClick = function (tr) {
    //Запуск ручной валидации, тк отсутсвует submit
    if ($('#PremisesJurisdiction').valid()) {
        let owr = getPremisesJurisdiction(tr);
        let address = getCurrentAddressJurisdictions();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/PremisesJurisdictions/YesPremisesJurisdiction',
            data: { owr, address },
            success: function (idPremisesJurisdiction) {
                if (idPremisesJurisdiction > 0) {
                    tr.data('idownershipright', idPremisesJurisdiction);
                }
                showEditDelPanelJurisdiction(tr);
            }
        });
    }
}
let PremisesJurisdictionsClick = function (event) {
    let el = $(event.target);
    let tr = el.parents('tr');
    if (el.hasClass('oi-x')) {
        if (el.hasClass('delete')) {
            PremisesJurisdictionDeleteClick(tr);
        }
        else {
            PremisesJurisdictionNoClick(tr);
        }
    }
    if (el.hasClass('oi-check')) {
        PremisesJurisdictionYesClick(tr);
    }
    if (el.hasClass('oi-pencil')) {
        showYesNoPanelJurisdiction(tr);
    }
}

$(function () {
    $('#PremisesJurisdictionsTable').hide();
    $('.yes-no-panel').hide();
    initializeVilidationJurisdictionTrs();
    $('#PremisesJurisdictionsToggle').on('click', $('#PremisesJurisdictionsTable'), elementToogle);
    $('#PremisesJurisdictionAdd').click(PremisesJurisdictionAddClick);
    $('#PremisesJurisdictions').click(PremisesJurisdictionsClick);
});