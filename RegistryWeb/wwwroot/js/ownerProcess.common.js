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
    $('#ownersToggle').click(ownersToggle);
    $('#commentToggle').click(commentToggle);
    $('#addressesToggle').click(addressesToggle);
    $('#ownerProcessToggle').click(ownerProcessToggle);
    $('#annulOwnerProcessToggle').click(annulOwnerProcessToggle);
    $('#annulOwnerProcessCheckbox').change(annulOwnerProcessCheckboxChange);
    $('#logToggle').click(logToggle);
    $('.logValueToggle').click(logValueToggle);

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