var refreshValidation = function () {
    var form = $("#r-ownerprocess-form")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
}
var arrowAnimation = function (arrow) {
    if (arrow.html() === '∧') {
        arrow.html('∨');
    }
    else {
        arrow.html('∧');
    }
}
var ownersToggle = function () {
    arrowAnimation($(this));
    $('#ownersTable').toggle();
}
var commentToggle = function () {
    arrowAnimation($(this));
    $('#Comment').toggle();
}
var addressesToggle = function () {
    arrowAnimation($(this));
    $('#addresses').toggle();
}
var ownerProcessToggle = function () {
    arrowAnimation($(this));
    $('#ownerProcess').toggle();
}
var annulOwnerProcessToggle = function () {
    arrowAnimation($(this));
    $('#annulOwnerProcess').toggle();
}
var annulOwnerProcessCheckboxChange = function () {
    $('#annulOwnerProcessCard').toggle();
    $('#annulBadge').toggle();
    $('#AnnulDate').val("");
    $('#AnnulComment').val("");
    
}
$(function () {
    var action = $('form').data('action');
    if ($('#AnnulDate').val() === "") {
        $('#annulOwnerProcessCard').hide();
        $('#annulBadge').hide();
    }
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
    $('#commentToggle').click(commentToggle);
    $('#addressesToggle').click(addressesToggle);
    $('#ownerProcessToggle').click(ownerProcessToggle);
    $('#annulOwnerProcessToggle').click(annulOwnerProcessToggle);
    $('#annulOwnerProcessCheckbox').change(annulOwnerProcessCheckboxChange);
    
});