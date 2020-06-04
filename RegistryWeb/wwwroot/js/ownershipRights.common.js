let CreateOwnershipPremisesAssoc = function () {
    let ownershipRights = getOwnershipRights();
    let ownershipPremisesAssoc = [];
    ownershipRights.forEach(function (item, i, arr) {
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
    ownershipRights.forEach(function (item, i, arr) {
        ownershipBuildingsAssoc.push({
            IdBuilding: 0,
            IdOwnershipRight: 0,
            OwnershipRightNavigation: item
        });
    });
    return ownershipBuildingsAssoc;
}
let getOwnershipRights = function () {
    let trs = $('#ownershipRights>tr');
    let owrs = [];
    trs.each(function () {
        owrs.push(getOwnershipRight($(this)));
    });
    return owrs;
}
let getOwnershipRight = function (tr) {
    let fields = tr.find('.field-ownership-right');
    let ownershipRight = {
        IdOwnershipRight: tr.data('idownershi pright') == "" ? "0" : tr.data('idownershipright'),
        Number: fields[0].value,
        Date: fields[1].value,
        Description: fields[2].value,
        IdOwnershipRightType: fields[3].value,
        ResettlePlanDate: fields[4].value,
        DemolishPlanDate: fields[5].value
    };
    return ownershipRight;
}
let getCurrentAddress = function () {
    let address = {
        addressType: $('#ownershipRights').data('addresstype'),
        id: $('#ownershipRights').data('id')
    };
    return address;
}
let getErrorSpan = function (dataValmsgFor) {
    return "<span class=\"text-danger field-validation-valid\" data-valmsg-for=\"" + dataValmsgFor +
        "\" data-valmsg-replace=\"true\"></span>";
}
let initializeVilidationTr = function (tr) {
    let fields = tr.find('.field-ownership-right');
    let idOwnershipRight = tr.data('idownershipright');
    //Дата
    let Date = 'Date_' + idOwnershipRight;
    $(fields[1]).addClass('valid');
    $(fields[1]).attr('data-val', 'true');
    $(fields[1]).attr('data-val-required', 'Поле "Дата" является обязательным');
    $(fields[1]).attr('id', Date);
    $(fields[1]).attr('name', Date);
    $(fields[1]).attr('aria-describedby', Date + '-error');
    $(fields[1]).after(getErrorSpan(Date));
    //Тип ограничения
    let OwnershipRightType = 'IdOwnershipRightType_' + idOwnershipRight;
    $(fields[3]).addClass('valid');
    $(fields[3]).attr('data-val', 'true');
    $(fields[3]).attr('data-val-required', 'Поле "Тип" является обязательным');
    $(fields[3]).attr('id', OwnershipRightType);
    $(fields[3]).attr('name', OwnershipRightType);
    $(fields[3]).attr('aria-describedby', OwnershipRightType + '-error');
    $(fields[3]).after(getErrorSpan(OwnershipRightType));

    refreshValidationOwnershipRightsForm();
}
let refreshValidationOwnershipRightsForm = function () {
    var form = $("#ownershipRightsForm")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
}
let initializeVilidationTrs = function () {
    let trs = $('#ownershipRights>tr');
    trs.each(function () {
        initializeVilidationTr($(this));
    });
}
let refreshOwnershipRight = function (tr, ownershipRight) {
    let fields = tr.find('.field-ownership-right');
    //Номер
    $(fields[0]).prop('value', ownershipRight.number);
    $(fields[0]).prop('title', ownershipRight.number);
    //Дата
    $(fields[1]).prop('value', ownershipRight.date);
    $(fields[1]).prop('title', ownershipRight.date);
    $(fields[1])
        .removeClass('input-validation-error')
        .addClass('valid');
    $(fields[1]).next()
        .removeClass('field-validation-error')
        .addClass('field-validation-valid')
        .text('');
    //Наименование
    $(fields[2]).prop('value', ownershipRight.description);
    $(fields[2]).prop('title', ownershipRight.description);
    //Тип ограничения
    $(fields[3]).prop('value', ownershipRight.idOwnershipRightType);
    $(fields[3]).prop('title', ownershipRight.idOwnershipRightType);
    $(fields[3])
        .removeClass('input-validation-error')
        .addClass('valid');
    $(fields[3]).next()
        .removeClass('field-validation-error')
        .addClass('field-validation-valid')
        .text('');
    //Планируемая дата переселения
    $(fields[4]).prop('value', ownershipRight.resettlePlanDate);
    $(fields[4]).prop('title', ownershipRight.resettlePlanDate);
    //Планируемая дата сноса
    $(fields[5]).prop('value', ownershipRight.demolishPlanDate);
    $(fields[5]).prop('title', ownershipRight.demolishPlanDate);
}
let showEditDelPanel = function (tr) {
    let fields = tr.find('.field-ownership-right');
    let editDelPanel = tr.find('.edit-del-panel');
    let yesNoPanel = tr.find('.yes-no-panel');
    fields.prop('disabled', true);
    yesNoPanel.hide();
    editDelPanel.show();
}
let showYesNoPanel = function (tr) {
    let fields = tr.find('.field-ownership-right');
    let yesNoPanel = tr.find('.yes-no-panel');
    let editDelPanel = tr.find('.edit-del-panel');
    fields.prop('disabled', false);
    editDelPanel.hide();
    yesNoPanel.show();
}
let ownershipRightAddClick = function (event) {
    let addressType = $('#ownershipRights').data('addresstype');
    let action = $('#ownershipRights').data('action');
    $.ajax({
        async: false,
        type: 'POST',
        url: window.location.origin + '/OwnershipRights/AddOwnershipRight',
        data: { addressType, action },
        success: function (tr) {
            let trs = $('#ownershipRights');
            let ownershipRightsToggle = $('#ownershipRightsToggle');
            if (!isExpandElemntArrow(ownershipRightsToggle)) // развернуть при добавлении, если было свернуто 
                ownershipRightsToggle.click();
            trs.append(tr);
            initializeVilidationTr(trs.find('tr').last());
            event.preventDefault();
        }
    });
}
let ownershipRightDeleteClick = function (tr) {
    let isOk = confirm("Вы уверены что хотите удалить ограничение?");
    if (isOk) {
        let idOwnershipRight = tr.data('idownershipright');
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/OwnershipRights/DeleteOwnershipRight',
            data: { idOwnershipRight: idOwnershipRight },
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
let ownershipRightNoClick = function (tr) {
    let idOwnershipRight = tr.data('idownershipright');
    //Отменить изменения внесенные в ограничение
    if (Number.isInteger(idOwnershipRight)) {
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/OwnershipRights/GetOwnershipRight',
            data: { idOwnershipRight: idOwnershipRight },
            success: function (ownershipRight) {
                refreshOwnershipRight(tr, ownershipRight);
                showEditDelPanel(tr);
            }
        });
    }
    //Отменить вставку нового ограничения
    else {
        tr.remove();
    }
}
let ownershipRightYesClick = function (tr) {
    //Запуск ручной валидации, тк отсутсвует submit
    if ($('#ownershipRightsForm').valid()) {
        let owr = getOwnershipRight(tr);
        let address = getCurrentAddress();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/OwnershipRights/YesOwnershipRight',
            data: { owr, address },
            success: function (idOwnershipRight) {
                if (idOwnershipRight > 0) {
                    tr.data('idownershipright', idOwnershipRight);
                }
                showEditDelPanel(tr);
            }
        });
    }
}
let ownershipRightsClick = function (event) {
    let el = $(event.target);
    let tr = el.parents('tr');
    if (el.hasClass('oi-x')) {
        if (el.hasClass('delete')) {
            ownershipRightDeleteClick(tr);
        }
        else {
            ownershipRightNoClick(tr);
        }
    }
    if (el.hasClass('oi-check')) {
        ownershipRightYesClick(tr);
    }
    if (el.hasClass('oi-pencil')) {
        showYesNoPanel(tr);
    }
}

$(function () {
    $('#ownershipRightsTable').hide();
    $('.yes-no-panel').hide();
    initializeVilidationTrs();
    $('#ownershipRightsToggle').on('click', $('#ownershipRightsTable'), elementToogle);
    $('#ownershipRightAdd').click(ownershipRightAddClick);
    $('#ownershipRights').click(ownershipRightsClick);
});