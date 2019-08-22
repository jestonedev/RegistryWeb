var ownerDelete = function (owner) {
    if ($('.ownerBlock').length == 1)
        return;
    var iOwner = +owner.attr('data-i-owner');//Преобразование iOwner к числу
    $('.reasonBlock[data-i-owner="' + iOwner + '"]').remove();
    owner.remove();
    recalculationIdOwner(iOwner);
}
var reasonDelete = function (reason) {
    var iOwner = +reason.attr('data-i-owner');
    var iReason = +reason.attr('data-i-reason');
    var reasons = $('.reasonBlock[data-i-owner="' + iOwner + '"]');
    if (reasons.length == 1)
        return;
    reason.remove();
    recalculationIdReason(iOwner, iReason, 2);
}

var getIOwner = function (iOwner, type) {
    if (type == 1)
        return iOwner + 1;
    return iOwner;
}
var getIReason = function (iReason, type) {
    if (type == 1)
        return iReason;
    return iReason + 1;
}
//iOwner и iReason обязательно должны быть числом. Иначе некорректная работа
//type - тип переименования:
// 1 - переименование в случае удаления собственника
// 2 - переименование в случае удаление документа-основания
var recalculationIdReason = function (iOwner, iReason, type) {
    var oldIOwner = getIOwner(iOwner, type);
    var list = $('.reasonBlock[data-i-owner="' + oldIOwner + '"]');
    if (iReason == list.length)
        return;
    for (var i = iReason; i < list.length; i++) {
        var oldIReason = getIReason(i, type);
        list[i].setAttribute('data-i-owner', iOwner);
        list[i].setAttribute('data-i-reason', i);
        $('#Owners_' + oldIOwner + '__OwnerReasons_' + oldIReason + '__IdReason')
            .attr('name', 'Owners[' + iOwner + '].OwnerReasons[' + i + '].IdReason')
            .attr('id', 'Owners_' + iOwner + '__OwnerReasons_' + i + '__IdReason');
        $('#Owners_' + oldIOwner + '__OwnerReasons_' + oldIReason + '__IdOwner')
            .attr('name', 'Owners[' + iOwner + '].OwnerReasons[' + i + '].IdOwner')
            .attr('id', 'Owners_' + iOwner + '__OwnerReasons_' + i + '__IdOwner');
        $('#Owners_' + oldIOwner + '__OwnerReasons_' + oldIReason + '__NumeratorShare')
            .attr('name', 'Owners[' + iOwner + '].OwnerReasons[' + i + '].NumeratorShare')
            .attr('id', 'Owners_' + iOwner + '__OwnerReasons_' + i + '__NumeratorShare')
            .attr('aria-describedby', 'Owners_' + iOwner + '__OwnerReasons_' + i + '__NumeratorShare-error');
        $('span[data-valmsg-for="Owners[' + oldIOwner + '].OwnerReasons[' + oldIReason + '].NumeratorShare"]')
            .attr('data-valmsg-for', 'Owners[' + iOwner + '].OwnerReasons[' + i + '].NumeratorShare');
        $('#Owners_' + oldIOwner + '__OwnerReasons_' + oldIReason + '__DenominatorShare')
            .attr('name', 'Owners[' + iOwner + '].OwnerReasons[' + i + '].DenominatorShare')
            .attr('id', 'Owners_' + iOwner + '__OwnerReasons_' + i + '__DenominatorShare')
            .attr('aria-describedby', 'Owners_' + iOwner + '__OwnerReasons_' + i + '__DenominatorShare-error');
        $('span[data-valmsg-for="Owners[' + oldIOwner + '].OwnerReasons[' + oldIReason + '].DenominatorShare"]')
            .attr('data-valmsg-for', 'Owners[' + iOwner + '].OwnerReasons[' + i + '].DenominatorShare');
        $('#Owners_' + oldIOwner + '__OwnerReasons_' + oldIReason + '__IdReasonType')
            .attr('name', 'Owners[' + iOwner + '].OwnerReasons[' + i + '].IdReasonType')
            .attr('id', 'Owners_' + iOwner + '__OwnerReasons_' + i + '__IdReasonType')
            .attr('aria-describedby', 'Owners_' + iOwner + '__OwnerReasons_' + i + '__IdReasonType-error');
        $('#Owners_' + oldIOwner + '__OwnerReasons_' + oldIReason + '__ReasonNumber')
            .attr('name', 'Owners[' + iOwner + '].OwnerReasons[' + i + '].ReasonNumber')
            .attr('id', 'Owners_' + iOwner + '__OwnerReasons_' + i + '__ReasonNumber')
            .attr('aria-describedby', 'Owners_' + iOwner + '__OwnerReasons_' + i + '__ReasonNumber-error');
        $('span[data-valmsg-for="Owners[' + oldIOwner + '].OwnerReasons[' + oldIReason + '].ReasonNumber"]')
            .attr('data-valmsg-for', 'Owners[' + iOwner + '].OwnerReasons[' + i + '].ReasonNumber');
        $('#Owners_' + oldIOwner + '__OwnerReasons_' + oldIReason + '__ReasonDate')
            .attr('name', 'Owners[' + iOwner + '].OwnerReasons[' + i + '].ReasonDate')
            .attr('id', 'Owners_' + iOwner + '__OwnerReasons_' + i + '__ReasonDate')
            .attr('aria-describedby', 'Owners_' + iOwner + '__OwnerReasons_' + i + '__ReasonDate-error');
        $('span[data-valmsg-for="Owners[' + oldIOwner + '].OwnerReasons[' + oldIReason + '].ReasonDate"]')
            .attr('data-valmsg-for', 'Owners[' + iOwner + '].OwnerReasons[' + i + '].ReasonDate');
    }
}
//iOwner обязательно должно быть числом. Иначе некорректная работа
var recalculationIdOwner = function (iOwner) {
    var list = $('.ownerBlock');
    if (iOwner == list.length)
        return;
    for (var i = iOwner; i < list.length; i++) {
        var oldI = i + 1;
        list[i].setAttribute('data-i-owner', i);
        recalculationIdReason(i, 0, 1);
        $('#Owners_' + oldI + '__IdOwner')
            .attr('name', 'Owners[' + i + '].IdOwner')
            .attr('id', 'Owners_' + i + '__IdOwner');
        $('#Owners_' + oldI + '__IdProcess')
            .attr('name', 'Owners[' + i + '].IdProcess')
            .attr('id', 'Owners_' + i + '__IdProcess');
        $('#Owners_' + oldI + '__IdOwnerType')
            .attr('name', 'Owners[' + i + '].IdOwnerType')
            .attr('id', 'Owners_' + i + '__IdOwnerType');
        var idOwnerType = $('#Owners_' + i + '__IdOwnerType').val();
        if (idOwnerType == 1) {
            $('#Owners_' + oldI + '__OwnerPerson_IdOwner')
                .attr('name', 'Owners[' + i + '].OwnerPerson.IdOwner')
                .attr('id', 'Owners_' + i + '__OwnerPerson_IdOwner');
            $('#Owners_' + oldI + '__OwnerPerson_Name')
                .attr('name', 'Owners[' + i + '].OwnerPerson.Name')
                .attr('id', 'Owners_' + i + '__OwnerPerson_Name')
                .attr('aria-describedby', 'Owners_' + i + '__OwnerPerson_Name-error');
            $('span[data-valmsg-for="Owners[' + oldI + '].OwnerPerson.Name"]')
                .attr('data-valmsg-for', 'Owners[' + i + '].OwnerPerson.Name');
            $('#Owners_' + oldI + '__OwnerPerson_Surname')
                .attr('id', 'Owners_' + i + '__OwnerPerson_Surname')
                .attr('name', 'Owners[' + i + '].OwnerPerson.Surname')
                .attr('aria-describedby', 'Owners_' + i + '__OwnerPerson_Surname-error');
            $('span[data-valmsg-for="Owners[' + oldI + '].OwnerPerson.Surname"]')
                .attr('data-valmsg-for', 'Owners[' + i + '].OwnerPerson.Surname');
            $('#Owners_' + oldI + '__OwnerPerson_Patronymic')
                .attr('name', 'Owners[' + i + '].OwnerPerson.Patronymic')
                .attr('id', 'Owners_' + i + '__OwnerPerson_Patronymic')
                .attr('aria-describedby', 'Owners_' + i + '__OwnerPerson_Patronymic-error');
            $('span[data-valmsg-for="Owners[' + oldI + '].OwnerPerson.Patronymic"]')
                .attr('data-valmsg-for', 'Owners[' + i + '].OwnerPerson.Patronymic');
            continue;
        }
        $('#Owners_' + oldI + '__OwnerOrginfo_IdOwner')
            .attr('name', 'Owners[' + i + '].OwnerOrginfo.IdOwner')
            .attr('id', 'Owners_' + i + '__OwnerOrginfo_IdOwner');
        $('#Owners_' + oldI + '__OwnerOrginfo_OrgName')
            .attr('name', 'Owners[' + i + '].OwnerOrginfo.OrgName')
            .attr('id', 'Owners_' + i + '__OwnerOrginfo_OrgName')
            .attr('aria-describedby', 'Owners_' + i + '__OwnerOrginfo_OrgName-error');
        $('span[data-valmsg-for="Owners[' + oldI + '].OwnerOrginfo.OrgName"]')
            .attr('data-valmsg-for', 'Owners[' + i + '].OwnerOrginfo.OrgName');
    }
}

var reasonsToggle = function (owner) {
    var iOwner = +owner.attr('data-i-owner');
    var reasonsToggleBtn = owner.find('.reasonsToggle');
    if (reasonsToggleBtn.html() === '∧') {
        reasonsToggleBtn.html('∨');
    }
    else {
        reasonsToggleBtn.html('∧');
    }
    $('.reasonBlock[data-i-owner="' + iOwner + '"]').toggle();
}

var reasonAdd = function (owner) {
    var iOwner = +owner.attr('data-i-owner');
    var reasons = $('.reasonBlock[data-i-owner="' + iOwner + '"]');
    var iReason = reasons.length;
    var action = $('form').attr('data-action');
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/OwnerProcesses/OwnerReasonAdd',
        data: { iOwner: iOwner, iReason: iReason, action: action },
        success: function (data) {
            if (iReason == 0) {
                owner.after(data);
            }
            else {
                $(reasons[iReason - 1]).after(data);
            }
            if (owner.find('.reasonsToggle').html() === '∨') {
                $('.reasonBlock[data-i-owner="' + iOwner + '"]').last().hide();
            }
            refreshValidation();
        }
    });
}

$(function () {
    $('#owners').click(function (event) {
        if ($(event.target).hasClass('ownerDelete')) {
            var owner = $(event.target).parents().filter('.ownerBlock')
            ownerDelete(owner);
        } else
        if ($(event.target).hasClass('reasonDelete')) {
            var reason = $(event.target).parents().filter('.reasonBlock');
            reasonDelete(reason);
        } else
        if ($(event.target).hasClass('reasonsToggle')) {
            var owner = $(event.target).parents().filter('.ownerBlock')
            reasonsToggle(owner);
        } else
        if ($(event.target).hasClass('reasonAdd')) {
            var owner = $(event.target).parents().filter('.ownerBlock')
            reasonAdd(owner);
        }
        
    });
});