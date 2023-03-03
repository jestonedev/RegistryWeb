$(function () {
    function initAutocomplete(elem) {
        var elem2 = $("[data-processing|='edit']")[0];
        var accountElem = $(elem).find("[name|='Person.PaymentAccount']")[0];
        var idAccountElem = $(elem2).find("[name^='PaymentAccount_']")[0];

        accountElem.on("input", function () {
            idAccountElem.val("0");
        });

        accountElem.autocomplete({
            source: function (request, response) {
                $.ajax({
                    type: 'POST',
                    url: window.location.origin + '/Claims/GetAccounts',
                    dataType: 'json',
                    data: { text: request.term, type: "BKS" },
                    success: function (data) {
                        response($.map(data, function (item) {
                            let account = { label: item.account, value: item.account, idAccount: item.idAccount };
                            if (data.length === 1) {
                                idAccountElem.val(account.idAccount);
                                console.log(account.idAccount);
                                accountElem.val(account.value);
                                console.log(account.value);
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

    var accounts = $(".rr-payment-account");
    for (var i = 0; i < accounts.length; i++) {        
        initAutocomplete(accounts[i]);
    }

});