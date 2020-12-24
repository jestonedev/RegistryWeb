function ownerProcessToggle(e) {
    $(this).children('span').toggleClass('oi-chevron-top').toggleClass('oi-chevron-bottom');
    $('#ownerProcess').toggle();
    e.preventDefault();
}
function annulOwnerProcessToggle(e) {
    $(this).children('span').toggleClass('oi-chevron-top').toggleClass('oi-chevron-bottom');
    $('#annulOwnerProcess').toggle();
    e.preventDefault();
}
function addressesToggle(e) {
    $(this).children('span').toggleClass('oi-chevron-top').toggleClass('oi-chevron-bottom');
    $('#addresses').toggle();
    e.preventDefault();
}
function ownerFilesToggle(e) {
    $(this).children('span').toggleClass('oi-chevron-top').toggleClass('oi-chevron-bottom');
    $('#ownerFiles').toggle();
    e.preventDefault();
}
function ownersToggle(e) {
    $(this).children('span').toggleClass('oi-chevron-top').toggleClass('oi-chevron-bottom');
    $('#owners').toggle();
    e.preventDefault();
}
function ownerReasonsToggle(e) {
    $(this).children('span').toggleClass('oi-chevron-top').toggleClass('oi-document');
    var i = $(this).closest('li').data('i');
    $('.rr-owner-reason[data-i="' + i + '"]').toggle();
    e.preventDefault();
}
function commentToggle(e) {
    $(this).children('span').toggleClass('oi-chevron-top').toggleClass('oi-chevron-bottom');
    $('#Comment').toggle();
    e.preventDefault();
}
function logToggle(e) {
    var logCard = $(this).parents('.card');
    var logCardBody = logCard.find('.card-body');
    var isHidden = logCardBody.length == 0;
    if (isHidden) {
        var idProcess = $('input[name="IdProcess"]').val();
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/OwnerProcesses/ProcessLogGet',
            data: { idProcess },
            success: function (processLog) {
                logCard.find('.card-header').after(processLog);
                $('.logValueToggle').on('click', logValueToggle);
            }
        });
    }
    else {
        logCardBody.remove();
    }
    $(this).children('span').toggleClass('oi-chevron-top').toggleClass('oi-chevron-bottom');
    e.preventDefault();
}
function logValueToggle(e) {
    $(this).children('span').toggleClass('oi-chevron-top').toggleClass('oi-chevron-bottom');
    var ind = $(this).data('ind');
    $('.logValue')
        .filter(function () { return $(this).data('ind') === ind; })
        .toggle();
    e.preventDefault();
}
var annulOwnerProcessCheckboxChange = function () {
    $('#annulOwnerProcessCard').toggle();
    $('#annulBadge').toggle();
    var annulDate = $('input[name="AnnulDate"]');
    var annulComment = $('input[name="AnnulComment"]');
    if ($('#annulOwnerProcessCheckbox').is(':checked')) {
        annulDate.val("");
        annulDate.addClass('rr-valid-ignore');
        annulComment.val("");
        annulComment.addClass('rr-valid-ignore');
    }
    else {
        annulDate.removeClass('rr-valid-ignore');
        annulComment.removeClass('rr-valid-ignore');
    }
}

$.validator.addMethod('documentRequired', function (value, element) {
    var btn = $(element).closest('li').find('button');
    if (value == '') {
        btn.addClass('input-validation-error');
    }
    else {
        btn.removeClass('input-validation-error');
    }
    return value != '';
}, 'Блок «Документ» у собственника не выбран');
$.validator.addMethod('addressRequired', function (value, element) {
    return $('.rr-address').length != 0;
}, 'Блок «Адрес» является обязательным для заполнения');
$.validator.addMethod('ownerSurnameRequired', $.validator.methods.required,
    'Поле «Фамилия» является обязательным для заполнения');
$.validator.addMethod('ownerNameRequired', $.validator.methods.required,
    'Поле «Имя» является обязательным для заполнения');
$.validator.addMethod('ownerOrgNameRequired', $.validator.methods.required,
    'Поле «Наименование» является обязательным для заполнения');
$.validator.addMethod('fractionSum', function (value, element) {
    return isValidFraction();
}, 'Сумма значений полей «Доля» должно быть равно 1');
$.validator.addMethod('fileNumRequired', $.validator.methods.required,
    'Поле «Номер» является обязательным для заполнения');
$.validator.addMethod('fileDateRequired', $.validator.methods.required,
    'Поле «Дата» является обязательным для заполнения');
$.validator.addClassRules('rr-owner-surname', {
    ownerSurnameRequired: true
});
$.validator.addClassRules('rr-owner-name', {
    ownerNameRequired: true
});
$.validator.addClassRules('rr-owner-org-name', {
    ownerOrgNameRequired: true
});
$.validator.addClassRules('rr-owner-reason-document', {
    documentRequired: true
});
$.validator.addClassRules('rr-fraction', {
    fractionSum: true
});
$.validator.addClassRules('rr-file-num', {
    fileNumRequired: true
});
$.validator.addClassRules('rr-file-date', {
    fileDateRequired: true
});
$(function () {
    var form = $('#ownerProcessForm');
    var validator = form.validate();
    validator.settings.ignore = '.rr-valid-ignore';
    validator.settings.ignoreTitle = true;
    validator.settings.rules = {
        AnnulDate: {
            required: true
        },
        AnnulComment: {
            required: true
        },
        AddressSearch: {
            addressRequired: true
        }
    };
    validator.settings.messages = {
        AnnulDate: {
            required: 'Поле «Дата аннулирования» является обязательным для заполнения при аннулировании процесса'
        },
        AnnulComment: {
            required: 'Поле «Причина аннулирования» является обязательным для заполнения при аннулировании процесса'
        }
    };    
    if ($('input[name="AnnulDate"]').val() === "") {
        $('input[name="AnnulDate"]').addClass('rr-valid-ignore');
        $('input[name="AnnulComment"]').addClass('rr-valid-ignore');
        $('#annulOwnerProcessCard').hide();
        $('#annulBadge').hide();
    }    
    $('.logValue').hide();
    $('#logTable').hide();
    $('#ownerProcessToggle').on('click', ownerProcessToggle);
    $('#annulOwnerProcessToggle').on('click', annulOwnerProcessToggle);
    $('#addressesToggle').on('click',addressesToggle);
    $('#ownerFilesToggle').on('click', ownerFilesToggle);
    $('#ownersToggle').on('click', ownersToggle);
    $('.rr-owner-reasons-toggle').on('click', ownerReasonsToggle);
    $('#commentToggle').on('click', commentToggle);
    $('#logToggle').on('click', logToggle);
    
    $('#annulOwnerProcessCheckbox').change(annulOwnerProcessCheckboxChange);

    $('#ownerProcessEdit, #ownerProcessCreate').click(function (e) {
        e.preventDefault();
        if (form.valid()) {
            form.submit();
        }
        $('input[data-val="true"]')
            .removeAttr('aria-describedby')
            .removeAttr('aria-invalid');
    });
    $('#ownerProcessDelete').click(function (e) {
        e.preventDefault();
        $('#ownerProcessForm').submit();
    });    
});