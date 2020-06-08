//let getErrorSpan = function (dataValmsgFor) {
//    return "<span class=\"text-danger field-validation-valid\" data-valmsg-for=\"" + dataValmsgFor +
//        "\" data-valmsg-replace=\"true\"></span>";
//}
//let initializeVilidationTr = function (tr) {
//    let fields = tr.find('.field-ownership-right');
//    let idOwnershipRight = tr.data('idownershipright');
//    //Номер
//    let Number = 'Number_' + idOwnershipRight;
//    $(fields[0]).addClass('valid');
//    $(fields[0]).attr('data-val', 'true');
//    $(fields[0]).attr('data-val-required', 'Поле "Номер" является обязательным');
//    $(fields[0]).attr('id', Number);
//    $(fields[0]).attr('name', Number);
//    $(fields[0]).attr('aria-describedby', Number + '-error');
//    $(fields[0]).after(getErrorSpan(Number));
//    //Дата
//    let Date = 'Date_' + idOwnershipRight;
//    $(fields[1]).addClass('valid');
//    $(fields[1]).attr('data-val', 'true');
//    $(fields[1]).attr('data-val-required', 'Поле "Дата" является обязательным');
//    $(fields[1]).attr('id', Date);
//    $(fields[1]).attr('name', Date);
//    $(fields[1]).attr('aria-describedby', Date + '-error');
//    $(fields[1]).after(getErrorSpan(Date));
//    //Тип ограничения
//    let OwnershipRightType = 'IdOwnershipRightType_' + idOwnershipRight;
//    $(fields[3]).addClass('valid');
//    $(fields[3]).attr('data-val', 'true');
//    $(fields[3]).attr('data-val-required', 'Поле "Тип" является обязательным');
//    $(fields[3]).attr('id', OwnershipRightType);
//    $(fields[3]).attr('name', OwnershipRightType);
//    $(fields[3]).attr('aria-describedby', OwnershipRightType + '-error');
//    $(fields[3]).after(getErrorSpan(OwnershipRightType));

//    refreshValidationOwnershipRightsForm();
//}
//let refreshValidationOwnershipRightsForm = function () {
//    var form = $("#ownershipRightsForm")
//        .removeData("validator")
//        .removeData("unobtrusiveValidation");
//    $.validator.unobtrusive.parse(form);
//    form.validate();
//}
//let initializeVilidationTrs = function () {
//    let trs = $('#ownershipRights>tr');
//    trs.each(function () {
//        initializeVilidationTr($(this));
//    });
//}
//let refreshOwnershipRight = function (tr, ownershipRight) {
//    let fields = tr.find('.field-ownership-right');
//    //Номер
//    $(fields[0]).prop('value', ownershipRight.number);
//    $(fields[0]).prop('title', ownershipRight.number);
//    $(fields[0])
//        .removeClass('input-validation-error')
//        .addClass('valid');
//    $(fields[0]).next()
//        .removeClass('field-validation-error')
//        .addClass('field-validation-valid')
//        .text('');
//    //Дата
//    $(fields[1]).prop('value', ownershipRight.date);
//    $(fields[1]).prop('title', ownershipRight.date);
//    $(fields[0])
//        .removeClass('input-validation-error')
//        .addClass('valid');
//    $(fields[1]).next()
//        .removeClass('field-validation-error')
//        .addClass('field-validation-valid')
//        .text('');
//    //Наименование
//    $(fields[2]).prop('value', ownershipRight.description);
//    $(fields[2]).prop('title', ownershipRight.description);
//    //Тип ограничения
//    $(fields[3]).prop('value', ownershipRight.idOwnershipRightType);
//    $(fields[3]).prop('title', ownershipRight.idOwnershipRightType);
//    $(fields[0])
//        .removeClass('input-validation-error')
//        .addClass('valid');
//    $(fields[3]).next()
//        .removeClass('field-validation-error')
//        .addClass('field-validation-valid')
//        .text('');
//    //Планируемая дата переселения
//    $(fields[4]).prop('value', ownershipRight.resettlePlanDate);
//    $(fields[4]).prop('title', ownershipRight.resettlePlanDate);
//    //Планируемая дата сноса
//    $(fields[5]).prop('value', ownershipRight.demolishPlanDate);
//    $(fields[5]).prop('title', ownershipRight.demolishPlanDate);
//}
let getBuildingDemolitionInfo = function (idBuilding) {
    let trs = $('#buildingDemolitionActFiles>tr');
    let actFiles = [];
    trs.each(function () {
        actFiles.push(getBuildingDemolitionActFile($(this), idBuilding));
    });
    let buildingDemolitionInfoVM = {
        demolishPlanDate: $('#demolishPlanDate').val(),
        buildingDemolitionActFiles: actFiles
    };
    return buildingDemolitionInfoVM;
}
let getBuildingDemolitionActFile = function (tr, idBuilding) {
    let fields = tr.find('.field-building-demolition-act-file');
    let ownershipRight = {
        Id: tr.data('idbuildingdemolitionactfile'),
        IdBuilding: idBuilding,
        IdActFile: 0,
        IdActTypeDocument: fields[3].value,
        Number: fields[0].value,
        Date: fields[1].value,
        Name: fields[2].value,
    };
    return ownershipRight;
}
let buildingDemolitionActFileAddClick = function (event) {
    //$.ajax({
    //    async: false,
    //    type: 'POST',
    //    url: window.location.origin + '/BuildingDemolitionInfo/AddBuildingDemolitionActFile',
    //    data: { addressType, action },
    //    success: function (tr) {
    //        let trs = $('#ownershipRights');
    //        trs.append(tr);
    //        initializeVilidationTr(trs.find('tr').last());
    //        event.preventDefault();
    //    }
    //});
}
let buildingDemolitionInfoCancelClick = function () {
    let idBuilding = $('#buildingDemolitionActFiles').data('id');
    $.ajax({
        async: false,
        type: 'POST',
        url: window.location.origin + '/BuildingDemolitionInfo/GetBuildingDemolitionInfo',
        data: { idBuilding },
        success: function (buildingDemolitionInfoVM) {
            console.log(buildingDemolitionInfoVM);
        }
    });
}
let buildingDemolitionInfoSaveClick = function () {
    if ($('#buildingDemolitionInfoForm').valid()) {
        let idBuilding = $('#buildingDemolitionActFiles').data('id');
        let buildingDemolitionInfoVM = getBuildingDemolitionInfo(idBuilding);
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/BuildingDemolitionInfo/SaveBuildingDemolitionInfo',
            data: { idBuilding, buildingDemolitionInfoVM },
            success: function (ind) {
                if (ind != 1) {
                    alert("Ошибка сохранения информации о сносе!");
                }
            }
        });
    }
}
let buildingDemolitionActFilesClick = function (event) {
    let el = $(event.target);
    let tr = el.parents('tr');
    if (el.hasClass('oi-x')) {
        tr.remove();
    }
}
$(function () {
    $('#buildingDemolitionInfoBlock').hide();
    initializeVilidationTrs();
    $('#buildingDemolitionInfoToggle').on('click', $('#buildingDemolitionInfoBlock'), elementToogle);
    $('#buildingDemolitionActFileAdd').click(buildingDemolitionActFileAddClick);
    $('#buildingDemolitionActFiles').click(buildingDemolitionActFilesClick);
    $('#buildingDemolitionInfoSave').click(buildingDemolitionInfoSaveClick);
    $('#buildingDemolitionInfoCancel').click(buildingDemolitionInfoCancelClick);
});