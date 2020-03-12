var refreshValidation = function () {
    var form = $("#r-ownerprocess-form")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
}
var ownersToggle = function (e) {
    arrowAnimation($(this));
    $('#ownersTable').toggle();
    e.preventDefault();
}
var commentToggle = function (e) {
    arrowAnimation($(this));
    $('#Comment').toggle();
    e.preventDefault();
}
var addressesToggle = function (e) {
    arrowAnimation($(this));
    $('#addresses').toggle();
    e.preventDefault();
}
var ownerProcessToggle = function (e) {
    arrowAnimation($(this));
    $('#ownerProcess').toggle();
    e.preventDefault();
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
var logToggle = function (e) {
    arrowAnimation($(this));
    $('#logTable').toggle();
    e.preventDefault();
}
var logValueToggle = function (e) {
    arrowAnimation($(this));
    var ind = $(this).data('ind');
    console.log(ind);
    $('.logValue')
        .filter(function () { return $(this).data('ind') === ind; })
        .toggle();
    e.preventDefault();
}
$(function () {
    var action = $('form').data('action');
    if ($('#AnnulDate').val() === "") {
        $('#annulOwnerProcessCard').hide();
        $('#annulBadge').hide();
    }
    $('#addressAdd').click(function (e) {
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
        e.preventDefault();
    });
    $('.ownerAdd').click(function (e) {
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
        e.preventDefault();
    });
    $('.logValue').hide();
    $('#logTable').hide();
    $('#ownersToggle').click(ownersToggle);
    $('#commentToggle').click(commentToggle);
    $('#addressesToggle').click(addressesToggle);
    $('#ownerProcessToggle').click(ownerProcessToggle);
    $('#annulOwnerProcessToggle').click(annulOwnerProcessToggle);
    $('#annulOwnerProcessCheckbox').change(annulOwnerProcessCheckboxChange);
    $('#logToggle').click(logToggle);
    $('.logValueToggle').click(logValueToggle);
});