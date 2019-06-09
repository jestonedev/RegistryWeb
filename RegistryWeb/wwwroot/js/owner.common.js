var ownerDelete = function (id) {
    if ($('.ownerBlock').length == 1)
        return;
    $('.ownerBlock').filter(function (index) {
        return $(this).attr('data-id') === id;
    }).remove();
    //Преобразование id к числу
    recalculationOwnerId(+id);
}

//id обязательно должно быть числом. Иначе некорректная работа
var recalculationOwnerId = function (id) {
    if (id == $('.ownerBlock').length)
        return;
    for (var i = id; i < $('.ownerBlock').length; i++) {
        var oldId = i + 1;
        $('.ownerBlock').get(i).setAttribute('data-id', i);
        if ($('#ownerType').val() == 1) {
            $('input[name="OwnerPersons[' + oldId + '].IdOwnerProcess"]').attr('name', 'OwnerPersons[' + i + '].IdOwnerProcess');
            $('input[name="OwnerPersons[' + oldId + '].IdOwnerPersons"]').attr('name', 'OwnerPersons[' + i + '].IdOwnerPersons');
            $('input[name="OwnerPersons[' + oldId + '].Surname"]').attr('name', 'OwnerPersons[' + i + '].Surname');
            $('input[name="OwnerPersons[' + oldId + '].Name"]').attr('name', 'OwnerPersons[' + i + '].Name');
            $('input[name="OwnerPersons[' + oldId + '].Patronymic"]').attr('name', 'OwnerPersons[' + i + '].Patronymic');
            $('input[name="OwnerPersons[' + oldId + '].NumeratorShare"]').attr('name', 'OwnerPersons[' + i + '].NumeratorShare');
            $('input[name="OwnerPersons[' + oldId + '].DenominatorShare"]').attr('name', 'OwnerPersons[' + i + '].DenominatorShare');
            continue;
        }
        $('input[name="OwnerOrginfos[' + oldId + '].IdProcess"]').attr('name', 'OwnerOrginfos[' + i + '].IdProcess');
        $('input[name="OwnerOrginfos[' + oldId + '].IdOrginfo"]').attr('name', 'OwnerOrginfos[' + i + '].IdOrginfo');
        $('input[name="OwnerOrginfos[' + oldId + '].OrgName"]').attr('name', 'OwnerOrginfos[' + i + '].OrgName');
    }
}

$(function () {
    $('#owners').click(function (event) {
        var id = $(event.target).parents().filter('.ownerBlock').attr('data-id');
        if ($(event.target).hasClass('close'))
            ownerDelete(id);
    });
});