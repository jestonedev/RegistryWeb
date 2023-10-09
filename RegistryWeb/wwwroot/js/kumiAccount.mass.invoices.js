$(document).ready(function () {
    var modal = $("#invoicesMassCreatingModal");
    var errorsWrapper = $(".rr-invoice-errors");

    var accounts = $(".rr-invoice-wrapper").toArray();

    if (accounts.length > 0) {
        modal.modal("show");
    } else {
        errorsWrapper.append($("<div>Отсутствуют ЛС для формирования счет-квитанций</div>"));
    }

    modal.on("shown.bs.modal", function () {
        initInvoiceGenerationProgress(accounts.length);
        generateInvoice(accounts.shift());
    });

    modal.on("hidden.bs.modal", function () {
        window.print();
    });

    var stopMassCreating = false;

    $("#stopInvoiceMassCreating").on("click", function () {
        stopMassCreating = true;
    });

    function invoiceWrappertToObject(invoiceWrapper) {
        return {
            idAccount: $(invoiceWrapper).data("idAccount"),
            account: $(invoiceWrapper).data("account"),
            accountGisZkh: $(invoiceWrapper).data("accountGisZkh"),
            address: $(invoiceWrapper).data("address"),
            postIndex: $(invoiceWrapper).data("postIndex"),
            tenant: $(invoiceWrapper).data("tenant"),
            totalArea: $(invoiceWrapper).data("totalArea").toString().replace(".", ","),
            tariff: $(invoiceWrapper).data("tariff").toString().replace(".", ","),
            prescribed: $(invoiceWrapper).data("prescribed"),
            balanceInput: $(invoiceWrapper).data("balanceInput").toString().replace(".", ","),
            balanceOutput: $(invoiceWrapper).data("balanceOutput").toString().replace(".", ","),
            chargingTenancy: $(invoiceWrapper).data("chargingTenancy").toString().replace(".", ","),
            chargingPenalty: $(invoiceWrapper).data("chargingPenalty").toString().replace(".", ","),
            payed: $(invoiceWrapper).data("payed").toString().replace(".", ","),
            recalcTenancy: $(invoiceWrapper).data("recalcTenancy").toString().replace(".", ","),
            recalcPenalty: $(invoiceWrapper).data("recalcPenalty").toString().replace(".", ","),
            onDate: $(invoiceWrapper).data("onDate")
        };
    }

    function generateInvoice(invoiceWrapper) {
        if (stopMassCreating) {
            modal.off("hidden.bs.modal");
            modal.modal('hide');
            errorsWrapper.append($("<div>Не сформировано квитанций: " + (accounts.length + 1) + "</div>"));
            return;
        }
        var invoiceInfo = invoiceWrappertToObject(invoiceWrapper);
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/KumiAccountReports/GenerateInvoice',
            data: invoiceInfo,
            dataType: 'json',
            success: function (data) {
                if (data.state === "Success") {
                    var wrapper = $(".rr-invoice-wrapper[data-id-account='" + data.idAccount + "']");
                    wrapper.html(data.html);
                } else {
                    errorsWrapper.append(data.error);
                }
                setInvoiceGenerationProgress(progressCurrentValue + 1);
                if (accounts.length > 0) {
                    generateInvoice(accounts.shift());
                } else {
                    modal.modal("hide");
                }
            }
        });
    }

    var progressMaxValue = 0;
    var progressCurrentValue = 0;

    function initInvoiceGenerationProgress(maxValue) {
        var progress = modal.find(".progress");
        progress.removeClass("d-none");
        progressMaxValue = maxValue;
        progressCurrentValue = 0;
        var progressBar = progress.find(".progress-bar");
        progressBar.css("width", "0%");
        progressBar.attr("aria-valuenow", "0");
    }

    function setInvoiceGenerationProgress(value) {
        progressCurrentValue = value;
        var currentPosition = progressMaxValue;
        if (progressMaxValue > 0) {
            currentPosition = Math.round(progressCurrentValue / progressMaxValue * 100);
        }

        var progress = modal.find(".progress");
        var progressBar = progress.find(".progress-bar");
        progressBar.css("width", currentPosition + "%");
        progressBar.attr("aria-valuenow", currentPosition);
    }
});
