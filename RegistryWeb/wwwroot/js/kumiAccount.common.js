$(function () {
    var form = $('#accountForm');
    var action = form.attr('data-action');

    if (action === 'Details' || action === 'Delete') {
        $('select').prop('disabled', true);
        $('input').prop('disabled', true);
        $('textarea').prop('disabled', true);
        $('input[type="hidden"]').prop('disabled', false);
        $('input[type="submit"]').prop('disabled', false);
    }

    $('.account-toggler').each(function (idx, e) {
        $(e).on('click', $('#' + $(e).data("for")), elementToogleHide);
    });    

    function accountCustomValidations(validator) {
        var isValid = true;

        var account = $("#Account");
        if (account.valid()) {
            var accountExist = false;
            var idAccount = $('#IdAccount').val();
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/KumiAccounts/AccountExist',
                data: { account: account.val(), idAccount },
                async: false,
                success: function (data) {
                    accountExist = data;
                }
            });
            if (accountExist) {
                let error = {};
                error[account.attr("name")] = "Лицевой счет уже существует";
                validator.showErrors(error);
                isValid = false;
            } else {
                clearValidationError(account);
                removeErrorFromValidator(validator, account);
            }
        }
        
        return isValid;
    }

    $('#accountCreate, #accountEdit').click(function (e) {
        e.preventDefault();
        var isValid = form.valid();
        var validator = form.validate();
        if (!accountCustomValidations(validator)) {
            isValid = false;
        }

        if (isValid) {
            form.find('input, select, textarea').attr('disabled', false);
            form.submit();
        } else {
            fixBootstrapSelectHighlight(form);
        }
    });
    $('#accountDelete').click(function (e) {
        e.preventDefault();
        form.submit();
    });

    var firstIdStateChange = true;

    $('#IdState').on('change', function (e) {
        var val = $(this).val();
        if (val === '2') {

            if (action === 'Create' || action === 'Edit') {
                $('#AnnualDate').removeAttr('disabled');
                if (!firstIdStateChange) {
                    var now = new Date();
                    var month = now.getMonth() + 1;
                    var day = now.getDate();
                    var nowStr = now.getFullYear() + '-' + (month < 10 ? '0' : '') + month + '-' + (day < 10 ? '0' : '') + day;
                    $('#AnnualDate').val(nowStr);
                }
            }
        }
        else {
            $('#AnnualDate').val('').attr('disabled', 'disabled');
        }
        e.preventDefault();
    });

    $('#IdState').change();
    firstIdStateChange = false;
});