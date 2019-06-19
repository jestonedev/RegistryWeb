var refreshValidation = function () {
    var form = $("#r-ownerprocess-form")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
}
var ownerHeaderChange = function () {
    if ($('#ownerType').val() == 1) {
        $('.ownerPersonHeader').show();
        $('.ownerOrginfoHeader').hide();
    }
    else { //2 или 3
        $('.ownerPersonHeader').hide();
        $('.ownerOrginfoHeader').show();
    }
}
$(function () {
    ownerHeaderChange();
    $('#ownerType').change(function () {
        $('.ownerBlock').remove();
        ownerHeaderChange();
        $('#ownerAdd').click();
    });
    var action = $('form').data('action');
    $('#addressAdd').on("click", function () {
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
    $('#ownerReasonAdd').on("click", function () {
        var id = $('.ownerReasonBlock').length;
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/OwnerProcesses/OwnerReasonAdd',
            data: { id: id, action: action },
            success: function (data) {
                $('#ownerReasons').append(data);
                refreshValidation();
            }
        });
    });
    $('#ownerAdd').on("click", function () {
        var id = $('.ownerBlock').length;
        var idOwnerType = $('#ownerType').val();
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
});