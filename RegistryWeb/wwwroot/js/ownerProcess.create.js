$(function () {
    $('#addressAdd').on("click", function () {
        var id = $('.addressBlock').length;
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/OwnerProcesses/AddressAdd',
            data: { id: id },
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
            data: { id: id },
            success: function (data) {
                $('#ownerReasons').append(data);
            }
        });
    });
    $('#ownerType').change(function () {
        $('.ownerBlock').remove();
        $('#ownerAdd').click();
    });
    $('#ownerAdd').on("click", function () {
        var id = $('.ownerBlock').length;
        var idOwnerType = $('#ownerType').val();
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/OwnerProcesses/OwnerAdd',
            data: { idOwnerType: idOwnerType, id: id },
            success: function (data) {
                $('#owners').append(data);
            }
        });
    });
});