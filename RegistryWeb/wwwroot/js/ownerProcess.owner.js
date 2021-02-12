function ownerRename() {
    $('.rr-owner').each(function (i) {
        var oldI = $(this).data('i');
        $(this).attr('data-i', i);
        var inputs = $(this).find('input');
        inputs[0].name = 'Owners[' + i + '].IdOwner';
        inputs[1].name = 'Owners[' + i + '].IdProcess';
        inputs[2].name = 'Owners[' + i + '].IdOwnerType';
        if (inputs[2].value == 1) {
            inputs[4].name = 'Owners[' + i + '].OwnerPerson.IdOwner';
            inputs[5].name = 'Owners[' + i + '].OwnerPerson.Surname';
            inputs[6].name = 'Owners[' + i + '].OwnerPerson.Name';
            inputs[7].name = 'Owners[' + i + '].OwnerPerson.Patronymic';
        }
        else {
            inputs[4].name = 'Owners[' + i + '].OwnerOrginfo.IdOwner';
            inputs[5].name = 'Owners[' + i + '].OwnerOrginfo.OrgName';
        }
        ownerReasonRename(oldI, i);
    });
}
function ownerReasonRename(oldI, i) {
    $('.rr-owner-reason[data-i="' + oldI + '"]').each(function (j) {
        $(this).attr('data-i', i);
        $(this).find('[name$=IdReason]').attr('name','Owners[' + i + '].OwnerReasons[' + j + '].IdReason');
        $(this).find('[name$=IdOwner]').attr('name','Owners[' + i + '].OwnerReasons[' + j + '].IdOwner');
        $(this).find('[name$=NumeratorShare]').attr('name','Owners[' + i + '].OwnerReasons[' + j + '].NumeratorShare');
        $(this).find('[name$=DenominatorShare]').attr('name','Owners[' + i + '].OwnerReasons[' + j + '].DenominatorShare');
        $(this).find('select').attr('name', 'Owners[' + i + '].OwnerReasons[' + j + '].IdReasonType');
        $(this).find('[name$=ReasonNumber]').attr('name','Owners[' + i + '].OwnerReasons[' + j + '].ReasonNumber');
        $(this).find('[name$=ReasonDate]').attr('name','Owners[' + i + '].OwnerReasons[' + j + '].ReasonDate');
    });
}
function ownerDelete(e) {
    var owner = $(this).closest('li');
    var i = owner.data('i');
    owner.remove();
    $('.rr-owner-reason[data-i="' + i + '"]').remove();
    ownerRename();
    isValidFraction();
    e.preventDefault();
}
function ownerReasonDelete(e) {
    var ownerReason = $(this).closest('li');
    var i = ownerReason.data('i');
    ownerReason.remove();
    ownerReasonRename(i, i);
    isValidFraction();
    e.preventDefault();
}
function ownerAdd(e) {
    var i = $('.rr-owner').length;
    var idOwnerType = $(this).data('idownertype');
    var action = $('#ownerProcessForm').data('action');
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/OwnerProcesses/OwnerAdd',
        data: { idOwnerType: idOwnerType, i: i, action: action },
        success: function (data) {
            $('#owners').append(data);
            var li = $('.rr-owner').last();
            li.find('input[name$="IdProcess"]').val($('#ownerProcess input[name="IdProcess"]').val());
        }
    });
    e.preventDefault();
}
function ownerReasonAdd(e) {
    var owner = $(this).closest('li');
    var i = owner.data('i');
    var ownerReasons = $('.rr-owner-reason[data-i="' + i + '"]');
    var j = ownerReasons.length;
    var action = $('#ownerProcessForm').data('action');
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/OwnerProcesses/OwnerReasonAdd',
        data: { i, j, action },
        success: function (data) {
            if (j == 0) {
                owner.after(data);
            }
            else {
                ownerReasons.last().after(data);
            }
            var reason = $('.rr-owner-reason[data-i="' + i + '"]').last();
            if (owner.find('.rr-owner-reasons-toggle span').hasClass('oi-document')) {
                reason.hide();
            }
            reason.find('input[name$="IdOwner"]').val(owner.find('input[name$="IdOwner"]').val());
            $('#ownerFiles .list-group-item').each(function () {
                reason.find('.rr-fraction').inputFilter(function (value) {
                    return /^$/.test(value) || /^[1-9]?\d{0,2}\/$/.test(value) || /^\/[1-9]?\d{0,2}$/.test(value) ||
                        /^\/$/.test(value) || /^[1-9]?\d{0,2}$/.test(value) || /^[1-9]\d{0,2}\/[1-9]\d{0,2}$/.test(value);
                });
            });
            reason.find('select').selectpicker("render");
            isValidFraction();
        }
    });
    e.preventDefault();
}
function fractionDocumentChange(e) {
    var fractionArray = $(this).val().split('/');
    if (fractionArray[0] == '') {
        fractionArray[0] = 1;
        if (fractionArray.length == 1) {
            $(this).val('1');
        }
        else {
            $(this).val('1' + $(this).val());
        }
    }
    if (fractionArray[1] == '' || fractionArray[1] == undefined) {
        fractionArray[1] = 1;
        $(this).val(fractionArray[0]);
    }
    var reason = $(this).closest('li');
    reason.find('input[name$="NumeratorShare"]').val(fractionArray[0]);
    reason.find('input[name$="DenominatorShare"]').val(fractionArray[1]);
    e.preventDefault();
}
function isValidFraction() {
    var result = 0.0;
    $('.rr-fraction').each(function () {
        var li = $(this).closest('li');
        var numeratorShare = li.find('input[name$="NumeratorShare"]').val();
        var denominatorShare = li.find('input[name$="DenominatorShare"]').val();
        result += numeratorShare / denominatorShare;
    });
    var isValid = result.toFixed(2) == '1.00';
    if (isValid) {
        $('.rr-fraction').removeClass('input-validation-error');
    }
    else {
        $('.rr-fraction').addClass('input-validation-error');
    }
    return isValid;
}
$(function () {
    $('.rr-fraction').inputFilter(function (value) {
        return /^$/.test(value) || /^[1-9]?\d{0,2}\/$/.test(value) || /^\/[1-9]?\d{0,2}$/.test(value) ||
            /^\/$/.test(value) || /^[1-9]?\d{0,2}$/.test(value) || /^[1-9]\d{0,2}\/[1-9]\d{0,2}$/.test(value);
    });
    $('.rr-owner-add').on('click', ownerAdd);
    $('#owners').on('click', '.rr-owner-delete', ownerDelete);
    $('#owners').on('click', '.rr-owner-reason-add', ownerReasonAdd);
    $('#owners').on('click', '.rr-owner-reason-delete', ownerReasonDelete);
    $('#owners').on('change', '.rr-fraction', fractionDocumentChange);
});