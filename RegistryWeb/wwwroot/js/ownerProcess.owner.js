function reasonDelete(reason) {
    var iOwner = +reason.attr('data-i-owner');
    var reasons = $('.reasonBlock[data-i-owner="' + iOwner + '"]');
    if (reasons.length == 1)
        return;
    reason.remove();
    var newReasons = $('.reasonBlock[data-i-owner="' + iOwner + '"]');
    for (var j = 0; j < newReasons.length; j++) {
        recalculationReasons($(newReasons[j]), iOwner, j);
    }
    if (newReasons.length == 1) {
        newReasons.find('.reasonDelete').addClass('disabled');
    }
}
function ownerDelete(owner) {
    if ($('.ownerBlock').length == 1)
        return;
    var iOwner = +owner.attr('data-i-owner');//Преобразование iOwner к числу
    $('.reasonBlock[data-i-owner="' + iOwner + '"]').remove();
    owner.remove();
    var trs = $('#owners tr');
    var i = -1;
    var j = 0;
    for (var k = 0; k < trs.length; k++) {
        var tr = $(trs[k]);
        var iOwnerTr = +tr.attr('data-i-owner')
        if (iOwner >= iOwnerTr)
            break;
        if (tr.hasClass('ownerBlock')) {
            i++;
            recalculationOwners(tr, i);
            j = 0;
        }
        if (tr.hasClass('reasonBlock')) {
            recalculationReasons(tr, i, j);
            j++;
        }
    }
    if ($('.ownerBlock').length == 1) {
        $('.ownerDelete').addClass('disabled');
    }
}
function recalculationOwners(owner, i) {
    var inputs = owner.find('input');
    owner.attr('data-i-owner', i);
    inputs[0].name = 'Owners[' + i + '].IdOwner';
    inputs[1].name = 'Owners[' + i + '].IdProcess';
    inputs[2].name = 'Owners[' + i + '].IdOwnerType';
    if (inputs[2].value == 1) {
        inputs[3].name = 'Owners[' + i + '].OwnerPerson.IdOwner';
        inputs[4].name = 'Owners[' + i + '].OwnerPerson.Surname';
        inputs[5].name = 'Owners[' + i + '].OwnerPerson.Name';
        inputs[6].name = 'Owners[' + i + '].OwnerPerson.Patronymic';
        return;
    }
    inputs[3].name = 'Owners[' + i + '].OwnerOrginfo.IdOwner';
    inputs[4].name = 'Owners[' + i + '].OwnerOrginfo.OrgName';
}
function recalculationReasons(reason, i, j) {
    var inputs = reason.find('input');
    reason.attr('data-i-owner', i);
    reason.attr('data-i-reason', j);
    inputs[0].name = 'Owners[' + i + '].OwnerReasons[' + j + '].IdReason';
    inputs[1].name = 'Owners[' + i + '].OwnerReasons[' + j + '].IdOwner';
    inputs[2].name = 'Owners[' + i + '].OwnerReasons[' + j + '].NumeratorShare';
    inputs[3].name = 'Owners[' + i + '].OwnerReasons[' + j + '].DenominatorShare';
    inputs[4].name = 'Owners[' + i + '].OwnerReasons[' + j + '].ReasonNumber';
    inputs[5].name = 'Owners[' + i + '].OwnerReasons[' + j + '].ReasonDate';
    reason.find('select')[0].name = 'Owners[' + i + '].OwnerReasons[' + j + '].IdReasonType';
}
function reasonsToggle(owner) {
    var iOwner = +owner.attr('data-i-owner');
    var reasonsToggleBtn = owner.find('.reasonsToggle');
    if (reasonsToggleBtn.html() === '∧') {
        reasonsToggleBtn.html('∨');
    }
    else {
        reasonsToggleBtn.html('∧');
    }
    $('.reasonBlock[data-i-owner="' + iOwner + '"]').toggle();
}

function reasonAdd(owner) {
    var iOwner = +owner.attr('data-i-owner');
    var reasons = $('.reasonBlock[data-i-owner="' + iOwner + '"]');
    var iReason = reasons.length;
    var action = $('form').attr('data-action');
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/OwnerProcesses/OwnerReasonAdd',
        data: { iOwner: iOwner, iReason: iReason, action: action },
        success: function (data) {
            if (iReason == 0) {
                owner.after(data);
            }
            else {
                $(reasons[iReason - 1]).after(data);
            }
            var newReasons = $('.reasonBlock[data-i-owner="' + iOwner + '"]');
            if (owner.find('.reasonsToggle').html() === '∨') {
                newReasons.last().hide();
            }
            newReasons.find('.reasonDelete').removeClass('disabled');
        }
    });
}
$(function () {
    if ($('.ownerBlock').length == 1) {
        $('.ownerDelete').addClass('disabled');
    }
    for (var i = 0; i < $('.ownerBlock').length; i++) {
        var reasons = $('.reasonBlock[data-i-owner="' + i + '"]');
        if (reasons.length == 1) {
            reasons.find('.reasonDelete').addClass('disabled');
        }
    }
    var action = $('form').attr('data-action');
    $('.ownerAdd').click(function (e) {
        e.preventDefault();
        var i = $('.ownerBlock').length;
        var idOwnerType = $(this).data('idOwnerType');
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/OwnerProcesses/OwnerAdd',
            data: { idOwnerType: idOwnerType, i: i, action: action },
            success: function (data) {
                $('#owners').append(data);
                if ($('.ownerBlock').length > 1) {
                    $('.ownerDelete').removeClass('disabled');
                }
                $('.reasonBlock[data-i-owner="' + i + '"]')
                    .find('.reasonDelete')
                    .addClass('disabled');
            }
        });        
    });
    $('#owners').click(function (event) {
        if ($(event.target).hasClass('ownerDelete')) {
            event.preventDefault();
            var owner = $(event.target).parents('.ownerBlock')
            ownerDelete(owner);
        } else if ($(event.target).hasClass('reasonDelete')) {
            event.preventDefault();
            var reason = $(event.target).parents('.reasonBlock');
            reasonDelete(reason);
        } else if ($(event.target).hasClass('reasonsToggle')) {
            event.preventDefault();
            var owner = $(event.target).parents('.ownerBlock')
            reasonsToggle(owner);
        } else if ($(event.target).hasClass('reasonAdd')) {
            event.preventDefault();
            var owner = $(event.target).parents('.ownerBlock')
            reasonAdd(owner);
        }
    });
});