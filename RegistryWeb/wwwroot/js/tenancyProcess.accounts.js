$(function () {

    function initAutocomplete(elem) {
        var accountElem = $(elem).find("[name$='AccountNavigation.Account']");
        var idAccountElem = $(elem).find("[name$='IdAccount']");

        accountElem.on("input", function () {
            idAccountElem.val("0");
        });

        accountElem.autocomplete({
            source: function (request, response) {
                $.ajax({
                    type: 'POST',
                    url: window.location.origin + '/Claims/GetAccounts',
                    dataType: 'json',
                    data: { text: request.term, type: "KUMI" },
                    success: function (data) {
                        response($.map(data, function (item) {
                            let account = { label: item.account, value: item.account, idAccount: item.idAccount };
                            if (data.length === 1) {
                                idAccountElem.val(account.idAccount);
                                accountElem.val(account.value);
                            }
                            return account;
                        }));
                    }
                });
            },
            select: function (event, ui) {
                idAccountElem.val(ui.item.idAccount);
                accountElem.val(ui.item.value);
            },
            minLength: 3
        });
    } 

    function updateAccountIndexes() {
        var namePropRegex = /(AccountsTenancyProcessesAssoc)\[\d+\]/;
        var idPropRegex = /(AccountsTenancyProcessesAssoc)_\d+__/;
        var accounts = $('.rr-account');
        accounts.each(function (idx, elem) {
            $(elem).find("[name]")
                .filter(function (fieldIdx, field) {
                    return $(field).prop("name").match(namePropRegex) != null;
                })
                .each(function (fieldIdx, field) {
                    var name = $(field).prop("name").replace(namePropRegex, "$1[" + idx + "]");
                    $(field).prop("name", name);
                    var id = $(field).prop("id").replace(idPropRegex, "$1_" + idx + "__");
                    $(field).prop("id", id);
                    $(field).closest(".form-group").find("label").prop("for", id);
                    $(field).closest(".form-group").find("span[data-valmsg-for]").attr("data-valmsg-for", name);
                });
        });
    }

    function addAccount(e) {
        template.clone(true).insertAfter($(".rr-account").last());
        var account = $(".rr-account").last();
        account.removeClass("d-none");
        updateAccountIndexes();
        initAutocomplete(account);
    }

    function deleteAccount(e) {
        var account = $(this).closest(".rr-account");
        if (account.index(".rr-account") > 0) {
            account.remove();
        } else {
            var accounts = $(".rr-account");
            if (accounts.length > 1) {
                var newAccount = account.next();
                newAccount.removeClass("offset-md-6");
                newAccount.find(".rr-account-append-span").remove();
                newAccount.find(".rr-tenancy-account-add-btn").removeClass("d-none");
                newAccount.find("label").removeClass("d-none");
                account.remove();
            } else {
                account.find("input[name$='AccountNavigation.Account']").val("");
                account.find("input[name$='IdAccount']").val("0");
                account.find("input[name$='IdAssoc']").val("0");
                account.find("input[name$='Fraction']").val("1,0000");
            }
        }
        updateAccountIndexes();
        e.preventDefault();
    }

    $("#TenancyProcess").on("change", "input[name$='Fraction']", function () {
        var val = $(this).val();
        if (val === "" || val === undefined) {
            $(this).val("0,0000");
            return;
        }
        var valDecimal = parseFloat(val.replace(",", "."));
        if (valDecimal > 1) {
            $(this).val("1,0000");
            return;
        }
        val = valDecimal.toString().replace(".", ",");
        if (val.indexOf(",") === -1) val += ",";
        while (val.length < 6) {
            val += "0";
        }
        $(this).val(val);
    });

    var accounts = $(".rr-account");
    var template = accounts.last().clone(true);
    accounts.last().remove();
    for (var i = 0; i < accounts.length; i++)
    {
        initAutocomplete(accounts[i]);
    }

    $('#TenancyProcess').on('click', '.rr-tenancy-account-delete-btn', deleteAccount);
    $("#TenancyProcess").on("click", ".rr-tenancy-account-add-btn", addAccount);
});