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
    var idOwner = $(this).closest('li').find('input')[0].value;
    $('.ownerReason[data-idowner="' + idOwner + '"]').toggle();
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
        annulDate.addClass('ignore');
        annulComment.val("");
        annulComment.addClass('ignore');
    }
    else {
        annulDate.removeClass('ignore');
        annulComment.removeClass('ignore');
    }
}

$.validator.addMethod('addressRequired', function (value, element) {
    return $('.address').length != 0;
});
$.validator.addMethod('ownerSurnameRequired', $.validator.methods.required,
    'Поле «Фамилия» является обязательным для заполнения');
$.validator.addMethod('ownerNameRequired', $.validator.methods.required,
    'Поле «Имя» является обязательным для заполнения');
$.validator.addMethod('ownerOrgNameRequired', $.validator.methods.required,
    'Поле «Наименование» является обязательным для заполнения');
$.validator.addMethod('reasonNumeratorShareRequired', $.validator.methods.required,
    'Поле «Числитель» является обязательным для заполнения');
$.validator.addMethod('reasonNumeratorShareRange', $.validator.methods.range,
    'Поле «Числитель» должно лежать между 1 и 1024');
$.validator.addMethod('reasonDenominatorShareRequired', $.validator.methods.required,
    'Поле «Знаменатель» является обязательным для заполнения');
$.validator.addMethod('reasonDenominatorShareRange', $.validator.methods.range,
    'Поле «Знаменатель» должно лежать между 1 и 1024');
$.validator.addMethod('reasonNumRequired', $.validator.methods.required,
    'Поле «Номер» является обязательным для заполнения');
$.validator.addMethod('reasonDateRequired', $.validator.methods.required,
    'Поле «Дата» является обязательным для заполнения');
$.validator.addClassRules('ownerSurname', {
    ownerSurnameRequired: true
});
$.validator.addClassRules('ownerName', {
    ownerNameRequired: true
});
$.validator.addClassRules('ownerOrgName', {
    ownerOrgNameRequired: true
});
$.validator.addClassRules('reasonNumeratorShare', {
    reasonNumeratorShareRequired: true,
    reasonNumeratorShareRange: [1, 1024]
});
$.validator.addClassRules('reasonDenominatorShare', {
    reasonDenominatorShareRequired: true,
    reasonDenominatorShareRange: [1, 1024]
});
$.validator.addClassRules('reasonNum', {
    reasonNumRequired: true
});
$.validator.addClassRules('reasonDate', {
    reasonDateRequired: true
});
$(function () {
    var form = $('#ownerProcessForm');
    var validator = form.validate();
    validator.settings.ignore = '.ignore';
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
        },
        AddressSearch: {
            addressRequired: 'Блок «Адрес» является обязательным для заполнения'
        }
    };    
    if ($('input[name="AnnulDate"]').val() === "") {
        $('input[name="AnnulDate"]').addClass('ignore');
        $('input[name="AnnulComment"]').addClass('ignore');
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
    $('.ownerReasonsToggle').on('click', ownerReasonsToggle);
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