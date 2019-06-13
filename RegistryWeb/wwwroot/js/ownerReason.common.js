var ownerReasonDelete = function (reason) {
    if ($('.ownerReasonBlock').length == 1)
        return;
    var id = +reason.attr('data-id');//Преобразование id к числу
    reason.remove();
    recalculationOwnerReasonId(id);
}

//id обязательно должно быть числом. Иначе некорректная работа
var recalculationOwnerReasonId = function (id) {
    var list = $('.ownerReasonBlock');
    if (id == list.length)
        return;
    for (var i = id; i < list.length; i++) {
        var oldId = i + 1;
        list[i].setAttribute('data-id', i);
        $('#OwnerReasons_' + oldId + '__IdProcess')
            .attr('name', 'OwnerReasons[' + i + '].IdProcess')
            .attr('id', 'OwnerReasons' + i + '__IdProcess');
        $('#OwnerReasons_' + oldId + '__IdReason')
            .attr('name', 'OwnerReasons[' + i + '].IdReason')
            .attr('id', 'OwnerReasons' + i + '__IdReason');
        $('#OwnerReasons_' + oldId + '__IdReasonType')
            .attr('name', 'OwnerReasons[' + i + '].IdReasonType')
            .attr('id', 'OwnerReasons' + i + '__IdReasonType')
            .attr('aria-describedby', 'OwnerReasons_' + i + '__IdReasonType-error');
        $('span[data-valmsg-for="OwnerReasons[' + oldId + '].IdReasonType"]')
            .attr('data-valmsg-for', 'OwnerReasons[' + i + '].IdReasonType');
        $('#OwnerReasons_' + oldId + '__ReasonNumber')
            .attr('name', 'OwnerReasons[' + i + '].ReasonNumber')
            .attr('id', 'OwnerReasons' + i + '__ReasonNumber')
            .attr('aria-describedby', 'OwnerReasons_' + i + '__ReasonNumber-error');
        $('span[data-valmsg-for="OwnerReasons[' + oldId + '].ReasonNumber"]')
            .attr('data-valmsg-for', 'OwnerReasons[' + i + '].ReasonNumber');
        $('#OwnerReasons_' + oldId + '__ReasonDate')
            .attr('name', 'OwnerReasons[' + i + '].ReasonDate')
            .attr('id', 'OwnerReasons' + i + '__ReasonDate')
            .attr('aria-describedby', 'OwnerReasons_' + i + '__ReasonDate-error');
        $('span[data-valmsg-for="OwnerReasons[' + oldId + '].ReasonDate"]')
            .attr('data-valmsg-for', 'OwnerReasons[' + i + '].ReasonDate');
    }
}

$(function () {
    $('#ownerReasons').click(function (event) {
        var reason = $(event.target).parents().filter('.ownerReasonBlock');
        if ($(event.target).hasClass('close'))
            ownerReasonDelete(reason);
    });
});