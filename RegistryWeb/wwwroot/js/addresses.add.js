$(function () {
    $('#addressAdd').on("click", function () {
        var id = $('.address').length;
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/OwnerProcesses/AddressAdd',
            data: { id: id },
            success: function (data) {
                $('#addresses').append(data);
                var newAddress = $('.address').last();
                $(newAddress).find('.typeAddress').checkboxradio({ icon: false });
                typeAddressRadiobuttonChange(newAddress);
                autocompleteStreet(newAddress);
            }
        });
    });
});