let getOwnershipRight = function (tr) {
    let fields = tr.find('.field-ownership-right');
    let ownershipRightVM = {
        idOwnershipRight: tr.data('idownershipright'),
        number: fields[0].value,
        date: fields[1].value,
        description: fields[2].value,
        idOwnershipRightType: fields[3].value,
        resettlePlanDate: fields[4].value,
        demolishPlanDate: fields[5].value
    };
    return ownershipRightVM;
}
let getCurrentAddress = function () {
    let address = {
        addressType: $('#ownershipRights').data('addresstype'),
        id: $('#ownershipRights').data('id')
    };
    return address;
}
let refreshOwnershipRight = function (tr, ownershipRight) {
    let fields = tr.find('.field-ownership-right');
    //Номер
    $(fields[0]).prop('value', ownershipRight.number);
    $(fields[0]).prop('title', ownershipRight.number);
    //Дата
    $(fields[1]).prop('value', ownershipRight.date);
    $(fields[1]).prop('title', ownershipRight.date);
    //Наименование
    $(fields[2]).prop('value', ownershipRight.description);
    $(fields[2]).prop('title', ownershipRight.description);
    //Тип ограничения
    $(fields[3]).prop('value', ownershipRight.idOwnershipRightType);
    $(fields[3]).prop('title', ownershipRight.idOwnershipRightType);
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
let ownershipRightsToggle = function (e) {
    arrowAnimation($(this));
    $('#ownershipRightsTable').toggle();
    e.preventDefault();
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
            $('#ownershipRights').append(tr);
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
    //Отменить вставку нового ограничения
    if (idOwnershipRight === "") {
        tr.remove();
    }
    //Отменить изменения внесенные в ограничение
    else {
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
}
let ownershipRightYesClick = function (tr) {
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
    $('#ownershipRightsToggle').click(ownershipRightsToggle);
    $('#ownershipRightAdd').click(ownershipRightAddClick);
    $('#ownershipRights').click(ownershipRightsClick);
});