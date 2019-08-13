var ownerDelete = function (owner) {
    if ($('.ownerBlock').length == 1)
        return;
    var id = +owner.attr('data-id');//Преобразование id к числу
    owner.remove();
    recalculationOwnerId(id);
}

//id обязательно должно быть числом. Иначе некорректная работа
var recalculationOwnerId = function (id) {
    var list = $('.ownerBlock');
    if (id == list.length)
        return;
    var idOwnerType = $('#ownerType').val();
    for (var i = id; i < list.length; i++) {
        var oldId = i + 1;
        list[i].setAttribute('data-id', i);
        if (idOwnerType == 1) {
            $('#OwnerPersons_' + oldId + '__IdProcess')
                .attr('name', 'OwnerPersons[' + i + '].IdProcess')
                .attr('id', 'OwnerPersons_' + i + '__IdProcess');

            $('#OwnerPersons_' + oldId + '__IdPerson')
                .attr('name', 'OwnerPersons[' + i + '].IdPerson')
                .attr('id', 'OwnerPersons_' + i + '__IdPerson');

            $('#OwnerPersons_' + oldId + '__Surname')
                .attr('id', 'OwnerPersons_' + i + '__Surname')
                .attr('name', 'OwnerPersons[' + i + '].Surname')
                .attr('aria-describedby', 'OwnerPersons_' + i + '__Surname-error');
            $('span[data-valmsg-for="OwnerPersons[' + oldId + '].Surname"]')
                .attr('data-valmsg-for', 'OwnerPersons[' + i + '].Surname');

            $('#OwnerPersons_' + oldId + '__Name')
                .attr('name', 'OwnerPersons[' + i + '].Name')
                .attr('id', 'OwnerPersons_' + i + '__Name')
                .attr('aria-describedby', 'OwnerPersons_' + i + '__Name-error');
            $('span[data-valmsg-for="OwnerPersons[' + oldId + '].Name"]')
                .attr('data-valmsg-for', 'OwnerPersons[' + i + '].Name');

            $('#OwnerPersons_' + oldId + '__Patronymic')
                .attr('name', 'OwnerPersons[' + i + '].Patronymic')
                .attr('id', 'OwnerPersons_' + i + '__Patronymic')
                .attr('aria-describedby', 'OwnerPersons_' + i + '__Patronymic-error');
            $('span[data-valmsg-for="OwnerPersons[' + oldId + '].Patronymic"]')
                .attr('data-valmsg-for', 'OwnerPersons[' + i + '].Patronymic');

            $('#OwnerPersons_' + oldId + '__NumeratorShare')
                .attr('name', 'OwnerPersons[' + i + '].NumeratorShare')
                .attr('id', 'OwnerPersons_' + i + '__NumeratorShare')
                .attr('aria-describedby', 'OwnerPersons_' + i + '__NumeratorShare-error');
            $('span[data-valmsg-for="OwnerPersons[' + oldId + '].NumeratorShare"]')
                .attr('data-valmsg-for', 'OwnerPersons[' + i + '].NumeratorShare');

            $('#OwnerPersons_' + oldId + '__DenominatorShare')
                .attr('name', 'OwnerPersons[' + i + '].DenominatorShare')
                .attr('id', 'OwnerPersons_' + i + '__DenominatorShare')
                .attr('aria-describedby', 'OwnerPersons_' + i + '__DenominatorShare-error');
            $('span[data-valmsg-for="OwnerPersons[' + oldId + '].DenominatorShare"]')
                .attr('data-valmsg-for', 'OwnerPersons[' + i + '].DenominatorShare');
            continue;
        }
        $('#OwnerOrginfos_' + oldId + '__IdProcess')
            .attr('name', 'OwnerOrginfos[' + i + '].IdProcess')
            .attr('id', 'OwnerOrginfos_' + i + '__IdProcess');
        $('#OwnerOrginfos_' + oldId + '__IdOrginfo')
            .attr('name', 'OwnerOrginfos[' + i + '].IdOrginfo')
            .attr('id', 'OwnerOrginfos_' + i + '__IdOrginfo');
        $('#OwnerOrginfos_' + oldId + '__OrgName')
            .attr('name', 'OwnerOrginfos[' + i + '].OrgName')
            .attr('id', 'OwnerOrginfos_' + i + '__OrgName')
            .attr('aria-describedby', 'OwnerOrginfos_' + oldId + '__OrgName-error');
        $('span[data-valmsg-for="OwnerOrginfos[' + oldId + '].OrgName"]')
            .attr('data-valmsg-for', 'OwnerOrginfos[' + i + '].OrgName');
        $('#OwnerOrginfos_' + oldId + '__NumeratorShare')
            .attr('name', 'OwnerOrginfos[' + i + '].NumeratorShare')
            .attr('id', 'OwnerOrginfos_' + i + '__NumeratorShare')
            .attr('aria-describedby', 'OwnerOrginfos_' + oldId + '__NumeratorShare-error');
        $('span[data-valmsg-for="OwnerOrginfos[' + oldId + '].NumeratorShare"]')
            .attr('data-valmsg-for', 'OwnerOrginfos[' + i + '].NumeratorShare');
        $('#OwnerOrginfos_' + oldId + '__DenominatorShare')
            .attr('name', 'OwnerOrginfos[' + i + '].DenominatorShare')
            .attr('id', 'OwnerOrginfos_' + i + '__DenominatorShare')
            .attr('aria-describedby', 'OwnerOrginfos_' + oldId + '__DenominatorShare-error');
        $('span[data-valmsg-for="OwnerOrginfos[' + oldId + '].DenominatorShare"]')
            .attr('data-valmsg-for', 'OwnerOrginfos[' + i + '].DenominatorShare');
    }
}

$(function () {
    $('#owners').click(function (event) {
        var owner = $(event.target).parents().filter('.ownerBlock')
        if ($(event.target).hasClass('close'))
            ownerDelete(owner);
    });
});