var refreshValidation = function () {
    var form = $("#r-ownerprocess-form")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
}
var ownersToggle = function () {
    if ($(this).html() === '∧') {
        $(this).html('∨');
    }
    else {
        $(this).html('∧');
    }
    $('#ownersTable').toggle();
}
$(function () {
    var action = $('form').data('action');
    $('#addressAdd').click(function () {
        var id = $('.buildingBlock').length;
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/OwnerProcesses/AddressAdd',
            data: { id: id, action: action },
            success: function (data) {
                $('#addresses').append(data);
                var newAddress = $('.addressBlock').last();
                addressListsDisplay(newAddress);
                autocompleteStreet(newAddress);
            }
        });
    });
    $('.ownerAdd').click(function () {
        var id = $('.ownerBlock').length;
        var idOwnerType = $(this).data('idOwnerType');
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/OwnerProcesses/OwnerAdd',
            data: { idOwnerType: idOwnerType, id: id, action: action },
            success: function (data) {
                $('#owners').append(data);
                refreshValidation();
            }
        });
    });
    $('#ownersToggle').click(ownersToggle);
});