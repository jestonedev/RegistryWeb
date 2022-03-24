$(function () {
    var form = $('#paymentsForm');
    var action = form.attr('data-action');

    if (action === 'Details' || action === 'Delete') {
        $('select').prop('disabled', true);
        $('input').prop('disabled', true);
        $('textarea').prop('disabled', true);
        $('input[type="hidden"]').prop('disabled', false);
        $('input[type="submit"]').prop('disabled', false);
    }

    $('#paymentCreate, #paymentEdit').click(function (e) {
        e.preventDefault();

        $(".input-decimal").each(function (idx, elem) {
            $(elem).val($(elem).val().replace(".", ","));
        });

        var isValid = form.valid();

        if (isValid) {
            form.find('input, select, textarea').attr('disabled', false);
            form.submit();
        } else {
            fixBootstrapSelectHighlight(form);
        }
    });

    $('#paymentDelete').click(function (e) {
        e.preventDefault();
        form.submit();
    });

    $('#PayerToggler').on("click", $("#Payer"), elementToogleHide);
    $('#RecipientToggler').on("click", $("#Recipient"), elementToogleHide);
    $('#PaymentToggler').on("click", $("#Payment"), elementToogleHide);
    $('#PaymentsUfsToggler').on("click", $("#PaymentsUfs"), elementToogleHide);
    $('#MemoryOrdersToggler').on("click", $("#MemoryOrders"), elementToogleHide);
    

    $(".rr-payment-revision").on("click", function () {
        $(".rr-payment-revision").removeClass("active");
        var actualRevision = $(".rr-payment-revision").filter(function (idx, elem) {
            return $(elem).text() === "Текущая ревизия";
        });
        $(this).addClass("active");

        var sum = $(this).data('sum');
        var sumElem = $("#Sum");
        sumElem.val(sum);
        if (actualRevision.data('sum') !== sum) {
            sumElem.addClass("text-danger");
        } else {
            sumElem.removeClass("text-danger");
        }

        var idKbkType = $(this).data('idKbkType');
        var idKbkTypeElem = $("#IdKbkType");
        var idKbkTypeElemBtn = $("[data-id='IdKbkType']");
        idKbkTypeElem.val(idKbkType);
        if (actualRevision.data('idKbkType') !== idKbkType) {
            idKbkTypeElemBtn.addClass("text-danger");
        } else {
            idKbkTypeElemBtn.removeClass("text-danger");
        }
        idKbkTypeElem.selectpicker('render');

        var kbk = $(this).data('kbk');
        var kbkElem = $("#Kbk");
        kbkElem.val(kbk);
        if (actualRevision.data('kbk') !== kbk) {
            kbkElem.addClass("text-danger");
        } else {
            kbkElem.removeClass("text-danger");
        }

        var targetCode = $(this).data('targetCode');
        var targetCodeElem = $("#TargetCode");
        targetCodeElem.val(targetCode);
        if (actualRevision.data('targetCode') !== targetCode) {
            targetCodeElem.addClass("text-danger");
        } else {
            targetCodeElem.removeClass("text-danger");
        }

        var okato = $(this).data('okato');
        var okatoElem = $("#Okato");
        okatoElem.val(okato);
        if (actualRevision.data('okato') !== okato) {
            okatoElem.addClass("text-danger");
        } else {
            okatoElem.removeClass("text-danger");
        }

        var recipientInn = $(this).data('recipientInn');
        var recipientInnElem = $("#RecipientInn");
        recipientInnElem.val(recipientInn);
        if (actualRevision.data('recipientInn') !== recipientInn) {
            recipientInnElem.addClass("text-danger");
        } else {
            recipientInnElem.removeClass("text-danger");
        }

        var recipientKpp = $(this).data('recipientKpp');
        var recipientKppElem = $("#RecipientKpp");
        recipientKppElem.val(recipientKpp);
        if (actualRevision.data('recipientKpp') !== recipientKpp) {
            recipientKppElem.addClass("text-danger");
        } else {
            recipientKppElem.removeClass("text-danger");
        }
    });

    $(".rr-payment-distribution-details-eye-btn").on("click", function (e) {
        var modal = $("#PaymentDistributionDetailsModal");
        modal.modal('show');
        e.preventDefault();
    });
});