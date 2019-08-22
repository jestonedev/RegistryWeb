var ownerDelete = function (owner) {
    if ($('.ownerBlock').length == 1)
        return;
    var iOwner = +owner.attr('data-i-owner');//Преобразование iOwner к числу
    owner.remove();
    recalculationOwnerId(iOwner);
}

//id обязательно должно быть числом. Иначе некорректная работа
var recalculationOwnerId = function (id) {
    var list = $('.ownerBlock');
    if (id == list.length)
        return;
    for (var i = id; i < list.length; i++) {
        var oldId = i + 1;
        list[i].setAttribute('data-i-owner', i);
        $('#Owners_' + oldId + '__IdOwner')
            .attr('name', 'Owners[' + i + '].IdOwner')
            .attr('id', 'Owners_' + i + '__IdOwner');
        $('#Owners_' + oldId + '__IdProcess')
            .attr('name', 'Owners[' + i + '].IdProcess')
            .attr('id', 'Owners_' + i + '__IdProcess');
        $('#Owners_' + oldId + '__IdOwnerType')
            .attr('name', 'Owners[' + i + '].IdOwnerType')
            .attr('id', 'Owners_' + i + '__IdOwnerType');
        var idOwnerType = $('#Owners_' + i + '__IdOwnerType').val();
        console.log(idOwnerType);
        if (idOwnerType == 1) {
            $('#Owners_' + oldId + '__OwnerPerson_IdOwner')
                .attr('name', 'Owners[' + i + '].OwnerPerson.IdOwner')
                .attr('id', 'Owners_' + i + '__OwnerPerson_IdOwner');
            $('#Owners_' + oldId + '__OwnerPerson_Name')
                .attr('name', 'Owners[' + i + '].OwnerPerson.Name')
                .attr('id', 'Owners_' + i + '__OwnerPerson_Name')
                .attr('aria-describedby', 'Owners_' + i + '__OwnerPerson_Name-error');
            $('span[data-valmsg-for="Owners[' + oldId + '].OwnerPerson.Name"]')
                .attr('data-valmsg-for', 'Owners[' + i + '].OwnerPerson.Name');
            $('#Owners_' + oldId + '__OwnerPerson_Surname')
                .attr('id', 'Owners_' + i + '__OwnerPerson_Surname')
                .attr('name', 'Owners[' + i + '].OwnerPerson.Surname')
                .attr('aria-describedby', 'Owners_' + i + '__OwnerPerson_Surname-error');
            $('span[data-valmsg-for="Owners[' + oldId + '].OwnerPerson.Surname"]')
                .attr('data-valmsg-for', 'Owners[' + i + '].OwnerPerson.Surname');
            $('#Owners_' + oldId + '__OwnerPerson_Patronymic')
                .attr('name', 'Owners[' + i + '].OwnerPerson.Patronymic')
                .attr('id', 'Owners_' + i + '__OwnerPerson_Patronymic')
                .attr('aria-describedby', 'Owners_' + i + '__OwnerPerson_Patronymic-error');
            $('span[data-valmsg-for="Owners[' + oldId + '].OwnerPerson.Patronymic"]')
                .attr('data-valmsg-for', 'Owners[' + i + '].OwnerPerson.Patronymic');
            continue;
        }
        $('#Owners_' + oldId + '__OwnerOrginfo_IdOwner')
            .attr('name', 'Owners[' + i + '].OwnerOrginfo.IdOwner')
            .attr('id', 'Owners_' + i + '__OwnerOrginfo_IdOwner');
        $('#Owners_' + oldId + '__OwnerOrginfo_OrgName')
            .attr('name', 'Owners[' + i + '].OwnerOrginfo.OrgName')
            .attr('id', 'Owners_' + i + '__OwnerOrginfo_OrgName')
            .attr('aria-describedby', 'Owners_' + i + '__OwnerOrginfo_OrgName-error');
        $('span[data-valmsg-for="Owners[' + oldId + '].OwnerOrginfo.OrgName"]')
            .attr('data-valmsg-for', 'Owners[' + i + '].OwnerOrginfo.OrgName');
    }
}

var reasonToggle = function (owner) {
    var iOwner = +owner.attr('data-i-owner');
    var reasonToggle = owner.find('.reasonToggle');
    if (reasonToggle.html() === '∨') {
        reasonToggle.html('∧');
    }
    else {
        reasonToggle.html('∨');
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
            if (owner.find('.reasonToggle').html() === '∧') {
                $('.reasonBlock[data-i-owner="' + iOwner + '"]').last().hide();
            }
            refreshValidation();
        }
    });
}

$(function () {
    $('#owners').click(function (event) {
        var owner = $(event.target).parents().filter('.ownerBlock')
        if ($(event.target).hasClass('close'))
            ownerDelete(owner);
        if ($(event.target).hasClass('reasonToggle'))
            reasonToggle(owner);
        if ($(event.target).hasClass('reasonAdd'))
            reasonAdd(owner);
    });
});