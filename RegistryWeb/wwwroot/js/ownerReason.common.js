var ownerReasonDelete = function (id) {
    if ($('.ownerReasonBlock').length == 1)
        return;
    $('.ownerReasonBlock').filter(function (index) {
        return $(this).attr('data-id') === id;
    }).remove();
    //Преобразование id к числу
    recalculationOwnerReasonId(+id);
}

//id обязательно должно быть числом. Иначе некорректная работа
var recalculationOwnerReasonId = function (id) {
    if (id == $('.ownerReasonBlock').length)
        return;
    for (var i = id; i < $('.ownerReasonBlock').length; i++) {
        var oldId = i + 1;
        $('.ownerReasonBlock').get(i).setAttribute('data-id', i);
        $('input[name="OwnerReasons[' + oldId + '].IdProcess"]').attr('name', 'OwnerReasons[' + i + '].IdProcess');
        $('input[name="OwnerReasons[' + oldId + '].IdReason"]').attr('name', 'OwnerReasons[' + i + '].IdReason');
        $('select[name="OwnerReasons[' + oldId + '].IdReasonType"]').attr('name', 'OwnerReasons[' + i + '].IdReasonType');
        $('input[name="OwnerReasons[' + oldId + '].ReasonNumber"]').attr('name', 'OwnerReasons[' + i + '].ReasonNumber');
        $('input[name="OwnerReasons[' + oldId + '].ReasonDate"]').attr('name', 'OwnerReasons[' + i + '].ReasonDate');
    }
}

$(function () {
    $('#ownerReasons').click(function (event) {
        var id = $(event.target).parents().filter('.ownerReasonBlock').attr('data-id');
        if ($(event.target).hasClass('close'))
            ownerReasonDelete(id);
    });
});