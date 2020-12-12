function ownerRename() {
    $('.rr-owner').each(function (i) {
        var oldI = $(this).data('i');
        $(this).attr('data-i', i);
        var inputs = $(this).find('input');
        inputs[0].name = 'Owners[' + i + '].IdOwner';
        inputs[1].name = 'Owners[' + i + '].IdProcess';
        inputs[2].name = 'Owners[' + i + '].IdOwnerType';
        if (inputs[2].value == 1) {
            inputs[5].name = 'Owners[' + i + '].OwnerPerson.IdOwner';
            inputs[6].name = 'Owners[' + i + '].OwnerPerson.Surname';
            inputs[7].name = 'Owners[' + i + '].OwnerPerson.Name';
            inputs[8].name = 'Owners[' + i + '].OwnerPerson.Patronymic';
        }
        else {
            inputs[5].name = 'Owners[' + i + '].OwnerOrginfo.IdOwner';
            inputs[6].name = 'Owners[' + i + '].OwnerOrginfo.OrgName';
        }
        ownerReasonRename(oldI, i);
    });
}
function ownerReasonRename(oldI, i) {
    $('.rr-owner-reason[data-i="' + oldI + '"]').each(function (j) {
        $(this).attr('data-i', i);
        var inputs = $(this).find('input');
        inputs[0].name = 'Owners[' + i + '].OwnerFilesAssoc[' + j + '].Id';
        inputs[1].name = 'Owners[' + i + '].OwnerFilesAssoc[' + j + '].IdOwner';
        inputs[2].name = 'Owners[' + i + '].OwnerFilesAssoc[' + j + '].IdFile';
        inputs[3].name = 'Owners[' + i + '].OwnerFilesAssoc[' + j + '].NumeratorShare';
        inputs[4].name = 'Owners[' + i + '].OwnerFilesAssoc[' + j + '].DenominatorShare';
    });
}
function ownerDelete(e) {
    var owner = $(this).closest('li');
    var i = owner.data('i');
    owner.remove();
    $('.rr-owner-reason[data-i="' + i + '"]').remove();
    ownerRename();
    e.preventDefault();
}
function ownerReasonDelete(e) {
    var ownerReason = $(this).closest('li');
    var i = ownerReason.data('i');
    ownerReason.remove();
    ownerReasonRename(i, i);
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
            reason.find('input[name$="IdOwner"]').val(owner.find('input[name$="IdOwner"]').val());
            $('#ownerFiles .list-group-item').each(function () {
                var idFile = $(this).find('input[name$="Id"]').val();
                var doc = getDocumentString($(this));                
                reason.find('select').append('<option value="' + idFile + '">' + doc + '</option>');
                reason.find('.rr-fraction').inputFilter(function (value) {
                    return /^$/.test(value) || /^[1-9]?\d{0,2}\/$/.test(value) || /^\/[1-9]?\d{0,2}$/.test(value) ||
                        /^\/$/.test(value) || /^[1-9]?\d{0,2}$/.test(value) || /^[1-9]\d{0,2}\/[1-9]\d{0,2}$/.test(value);
                });
            });
            reason.find('input[name$="IdFile"]').val(reason.find('option:selected').val());
        }
    });
    e.preventDefault();
}
function selectDocumentChange(e) {
    var reason = $(this).closest('li');
    reason.find('input[name$="IdFile"]').val(reason.find('option:selected').val());
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
$(function () {
    $('.rr-fraction').inputFilter(function (value) {
        return /^$/.test(value) || /^[1-9]?\d{0,2}\/$/.test(value) || /^\/[1-9]?\d{0,2}$/.test(value) ||
            /^\/$/.test(value) || /^[1-9]?\d{0,2}$/.test(value) || /^[1-9]\d{0,2}\/[1-9]\d{0,2}$/.test(value);
    });
    $('.rr-owner-add').on('click', ownerAdd);
    $('#owners').on('click', '.rr-owner-delete', ownerDelete);
    $('#owners').on('click', '.rr-owner-reason-add', ownerReasonAdd);
    $('#owners').on('click', '.rr-owner-reason-delete', ownerReasonDelete);
    $('#owners').on('change', 'select', selectDocumentChange);
    $('#owners').on('change', '.rr-fraction', fractionDocumentChange);
});