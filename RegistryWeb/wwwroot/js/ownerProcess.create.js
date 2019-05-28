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
                typeAddressRadiobuttonChange(newAddress);
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
});