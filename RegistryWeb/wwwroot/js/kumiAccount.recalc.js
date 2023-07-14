$(document).ready(function () {

    var recalcAccountForm = $("#accountRecalcForm");
    var recalcAccountFormValidator = recalcAccountForm.validate();
    recalcAccountFormValidator.settings.ignore = '.rr-valid-ignore';
    recalcAccountFormValidator.settings.ignoreTitle = true;

    var addChargeCorrectionForm = $("#accountAddCorrectionForm");

    $("#AccountKumiRecalc_RecalcType").on("change", function (e) {
        var idRecalcType = $(this).val();
        var recalcPeriodElem = $("#AccountKumiRecalc_RecalcPeriod");
        if (idRecalcType === "0") {
            recalcPeriodElem.val(0);
            recalcPeriodElem.find("option[value='1']").attr("disabled", "disabled");
            recalcPeriodElem.selectpicker('refresh');
        } else {
            recalcPeriodElem.find("option[value='1']").removeAttr("disabled");
            recalcPeriodElem.selectpicker('refresh');
        }
        recalcPeriodElem.change();
        e.preventDefault();
    });

    $("#AccountKumiRecalc_RecalcPeriod").on("change", function (e) {
        var idRecalcPeriod = $(this).val();
        var recalcPeriodYearElem = $("#AccountKumiRecalc_RecalcPeriodYear");
        var recalcPeriodYearElemWrapper = recalcPeriodYearElem.closest(".form-group");
        var recalcPeriodMonthElem = $("#AccountKumiRecalc_RecalcPeriodMonth");
        var recalcPeriodMonthElemWrapper = recalcPeriodMonthElem.closest(".form-group");

        if (idRecalcPeriod === "1") {
            recalcPeriodYearElemWrapper.removeClass("d-none");
            recalcPeriodMonthElemWrapper.removeClass("d-none");
            recalcPeriodYearElem.removeClass("rr-valid-ignore");
            recalcPeriodMonthElem.removeClass("rr-valid-ignore");
        } else {
            recalcPeriodYearElemWrapper.addClass("d-none");
            recalcPeriodMonthElemWrapper.addClass("d-none");
            recalcPeriodYearElem.addClass("rr-valid-ignore");
            recalcPeriodMonthElem.addClass("rr-valid-ignore");
        }
        e.preventDefault();
    });

    $("#AccountRecalcBtn").on("click", function (e) {
        var idAccount = $("#accountForm #IdAccount").val();
        var modal = $("#accountRecalcModal");
        modal.find("input[name='AccountKumiRecalc.IdAccount']").val(idAccount);
        modal.find("select, input").prop('disabled', false);
        modal.modal('show');
        e.preventDefault();
    });

    $("#accountRecalcForm .rr-report-submit").on("click", function (e) {
        recalcAccountForm.find(".rr-recalc-account-error").addClass("d-none");
        if (recalcAccountForm.valid()) {
            $(this).attr("disabled", "disabled").text("Выполняется расчет...");
            var recalcType = $("#AccountKumiRecalc_RecalcType").val();
            var recalcPeriod = $("#AccountKumiRecalc_RecalcPeriod").val();
            var recalcStartYear = recalcPeriod === "0" ? null : $("#AccountKumiRecalc_RecalcPeriodYear").val();
            var recalcStartMonth = recalcPeriod === "0" ? null : $("#AccountKumiRecalc_RecalcPeriodMonth").val();
            var idAccount = recalcAccountForm.find("input[name='AccountKumiRecalc.IdAccount']").val();
            if (idAccount !== "") {
                initRecalcAccountsProgress(1);
                recalcAccounts([idAccount], [], recalcType, recalcStartYear, recalcStartMonth);
            } else {
                getAccountIds(function (ids) {
                    initRecalcAccountsProgress(ids.length);
                    var accountIdsForRecalc = ids.slice(0, 10);
                    var accountIdsOther = ids.slice(10);
                    recalcAccounts(accountIdsForRecalc, accountIdsOther, recalcType, recalcStartYear, recalcStartMonth);
                });
            }
        } else {
            fixBootstrapSelectHighlight(recalcAccountForm);
        }
        e.preventDefault();
    });

    $("#AddChargeCorrectionBtn").on("click", function (e) {
        var idAccount = $("#accountForm #IdAccount").val();
        var modal = $("#accountAddCorrectionModal");

        var chargeListHref = modal.find(".rr-charge-corrections-list-href");
        if (chargeListHref.data("idAccount") === undefined) {
            chargeListHref.data("idAccount", idAccount);
            chargeListHref.attr("href", chargeListHref.attr("href") + "?idAccount=" + idAccount);
        }
        modal.find("input[name='AccountKumiChargeCorrection.IdAccount']").val(idAccount);
        modal.find("textarea, input").prop('disabled', false);
        modal.modal('show');
        e.preventDefault();
    });

    $("#accountAddCorrectionForm .rr-report-submit").on("click", function (e) {
        addChargeCorrectionForm.find(".rr-recalc-account-error").addClass("d-none");
        var tenancyValueElem = addChargeCorrectionForm.find("#AccountKumiChargeCorrection_TenancyValue");
        $(tenancyValueElem).val($(tenancyValueElem).val().replace(',', '.'));
        var penaltyValueElem = addChargeCorrectionForm.find("#AccountKumiChargeCorrection_PenaltyValue");
        $(penaltyValueElem).val($(penaltyValueElem).val().replace(',', '.'));
        var dgiValueElem = addChargeCorrectionForm.find("#AccountKumiChargeCorrection_DgiValue");
        if (dgiValueElem.length > 0)
            $(dgiValueElem).val($(dgiValueElem).val().replace(',', '.'));
        var pkkValueElem = addChargeCorrectionForm.find("#AccountKumiChargeCorrection_PkkValue");
        if (pkkValueElem.length > 0)
            $(pkkValueElem).val($(pkkValueElem).val().replace(',', '.'));
        var padunValueElem = addChargeCorrectionForm.find("#AccountKumiChargeCorrection_PadunValue");
        if (padunValueElem.length > 0)
            $(padunValueElem).val($(padunValueElem).val().replace(',', '.'));

        var paymentTenancyValueElem = addChargeCorrectionForm.find("#AccountKumiChargeCorrection_PaymentTenancyValue");
        $(paymentTenancyValueElem).val($(tenancyValueElem).val().replace(',', '.'));
        var paymentPenaltyValueElem = addChargeCorrectionForm.find("#AccountKumiChargeCorrection_PaymentPenaltyValue");
        $(paymentPenaltyValueElem).val($(penaltyValueElem).val().replace(',', '.'));
        var paymentDgiValueElem = addChargeCorrectionForm.find("#AccountKumiChargeCorrection_PaymentDgiValue");
        if (paymentDgiValueElem.length > 0)
            $(paymentDgiValueElem).val($(paymentDgiValueElem).val().replace(',', '.'));
        var paymentPkkValueElem = addChargeCorrectionForm.find("#AccountKumiChargeCorrection_PaymentPkkValue");
        if (paymentPkkValueElem.length > 0)
            $(paymentPkkValueElem).val($(paymentPkkValueElem).val().replace(',', '.'));
        var paymentPadunValueElem = addChargeCorrectionForm.find("#AccountKumiChargeCorrection_PaymentPadunValue");
        if (paymentPadunValueElem.length > 0)
            $(paymentPadunValueElem).val($(paymentPadunValueElem).val().replace(',', '.'));

        if (addChargeCorrectionForm.valid()) {
            $(this).attr("disabled", "disabled").text("Сохранение...");
            var atDate = $("#AccountKumiChargeCorrection_AtDate").val();
            var description = $("#AccountKumiChargeCorrection_Description").val();
            var tenancyValue = tenancyValueElem.val().replace('.', ',');
            var penaltyValue = penaltyValueElem.val().replace('.', ',');
            var dgiValue = 0;
            if (dgiValueElem.length > 0)
                dgiValue = dgiValueElem.val().replace('.', ',');
            var pkkValue = 0;
            if (pkkValueElem.length > 0)
                pkkValue = pkkValueElem.val().replace('.', ',');
            var padunValue = 0;
            if (padunValueElem.length > 0)
                padunValue = padunValueElem.val().replace('.', ',');

            var paymentTenancyValue = $("#AccountKumiChargeCorrection_PaymentTenancyValue").val().replace('.', ',');
            var paymentPenaltyValue = $("#AccountKumiChargeCorrection_PaymentPenaltyValue").val().replace('.', ',');
            
            var paymentDgiValue = 0;
            if (paymentDgiValueElem.length > 0)
                paymentDgiValue = paymentDgiValueElem.val().replace('.', ',');
            var paymentPkkValue = 0;
            if (paymentPkkValueElem.length > 0)
                paymentPkkValue = paymentPkkValueElem.val().replace('.', ',');
            var paymentPadunValue = 0;
            if (paymentPadunValueElem.length > 0)
                paymentPadunValue = paymentPadunValueElem.val().replace('.', ',');
            var idAccount = addChargeCorrectionForm.find("input[name='AccountKumiChargeCorrection.IdAccount']").val();

            $.ajax({
                type: 'POST',
                url: window.location.origin + '/KumiAccounts/AddChargeCorrection',
                data: {
                    idAccount, atDate, tenancyValue, penaltyValue, dgiValue, pkkValue, padunValue,
                    paymentTenancyValue, paymentPenaltyValue, paymentDgiValue, paymentPkkValue, paymentPadunValue, description
                },
                dataType: 'json',
                success: function () {
                    recalcAccounts([idAccount], [], 0, null, null);
                }
            });
        } else {
            fixBootstrapSelectHighlight(addChargeCorrectionForm);
        }
        e.preventDefault();
    });

    function getAccountIds(success) {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/KumiAccounts/GetSessionIds',
            dataType: 'json',
            success: function (ids) {
                success(ids);
            }
        });
    }

    function recalcAccounts(accountIdsForRecalc, accountIdsOther, recalcType, recalcStartYear, recalcStartMonth) {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/KumiAccounts/RecalcAccounts',
            data: { idAccounts: accountIdsForRecalc, recalcType, recalcStartYear, recalcStartMonth },
            dataType: 'json',
            success: function (data) {
                if (data.state === "Success") {
                    setRecalcAccountsProgress(progressCurrentValue + accountIdsForRecalc.length);
                    if (accountIdsOther.length > 0) {
                        accountIdsForRecalc = accountIdsOther.slice(0, 10);
                        accountIdsOther = accountIdsOther.slice(10);
                        recalcAccounts(accountIdsForRecalc, accountIdsOther, recalcStartYear, recalcStartMonth);
                    } else {
                        location.reload();
                    }
                } else {
                    recalcAccountForm.find(".rr-recalc-account-error").removeClass("d-none").text(data.error);
                    recalcAccountForm.find(".progress").addClass("d-none");
                    $("#accountRecalcModal .rr-report-submit").removeAttr("disabled").text("Перерасчет");
                }
            }
        });
    }

    var progressMaxValue = 0;
    var progressCurrentValue = 0;

    function initRecalcAccountsProgress(maxValue) {
        var progress = recalcAccountForm.find(".progress");
        progress.removeClass("d-none");
        progressMaxValue = maxValue;
        progressCurrentValue = 0;
        var progressBar = progress.find(".progress-bar");
        progressBar.css("width", "0%");
        progressBar.attr("aria-valuenow", "0");
    }

    function setRecalcAccountsProgress(value) {
        progressCurrentValue = value;
        var currentPosition = progressMaxValue;
        if (progressMaxValue > 0) {
            currentPosition = Math.round(progressCurrentValue / progressMaxValue * 100);
        }

        var progress = recalcAccountForm.find(".progress");
        var progressBar = progress.find(".progress-bar");
        progressBar.css("width", currentPosition+"%");
        progressBar.attr("aria-valuenow", currentPosition);
    }

    $("#AccountKumiRecalc_RecalcPeriodMonth").on("change", function () {
        fixBootstrapSelectHighlightOnChange($(this));
    });

    $("#AccountKumiRecalc_RecalcType").change();

    var chargeCurrentMonthElem = $(".rr-charge-current-month-row");
    if (chargeCurrentMonthElem.length === 1) {
        var endDate = chargeCurrentMonthElem.data("chargeEndDate");
        var cellClasses = [
            {
                cssSelector: ".rr-charge-period-input-tenancy",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-input-penalty", 
                property: null
            },
            {
                cssSelector: ".rr-charge-period-payment-tenancy",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-payment-penalty",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-tenancy",
                property: "chargeTenancy"
            },
            {
                cssSelector: ".rr-charge-period-penalty",
                property: "chargePenalty"
            },
            {
                cssSelector: ".rr-charge-period-recalc-tenancy",
                property: ["recalcTenancy", "correctionTenancy"]
            },
            {
                cssSelector: ".rr-charge-period-recalc-penalty",
                property: ["recalcPenalty", "correctionPenalty"]
            },
            {
                cssSelector: ".rr-charge-period-output-tenancy",
                property: function (elem) {
                    return parseFloat(elem.closest("tr").find(".rr-charge-period-input-tenancy").text().replace(",", ".")) +
                        parseFloat(elem.closest("tr").find(".rr-charge-period-tenancy").text().replace(",", ".")) - 
                        parseFloat(elem.closest("tr").find(".rr-charge-period-payment-tenancy").text().replace(",", ".")) +
                        parseFloat(elem.closest("tr").find(".rr-charge-period-recalc-tenancy").text().replace(",", "."));
                }
            },
            {
                cssSelector: ".rr-charge-period-output-penalty",
                property: function (elem) {
                    return parseFloat(elem.closest("tr").find(".rr-charge-period-input-penalty").text().replace(",", ".")) +
                        parseFloat(elem.closest("tr").find(".rr-charge-period-penalty").text().replace(",", ".")) -
                        parseFloat(elem.closest("tr").find(".rr-charge-period-payment-penalty").text().replace(",", ".")) +
                        parseFloat(elem.closest("tr").find(".rr-charge-period-recalc-penalty").text().replace(",", "."));
                }
            },
            {
                cssSelector: ".rr-charge-period-input-dgi",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-payment-dgi",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-dgi",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-recalc-dgi",
                property: ["recalcDgi", "correctionDgi"]
            },
            {
                cssSelector: ".rr-charge-period-output-dgi",
                property: function (elem) {
                    return parseFloat(elem.closest("tr").find(".rr-charge-period-input-dgi").text().replace(",", ".")) +
                        parseFloat(elem.closest("tr").find(".rr-charge-period-dgi").text().replace(",", ".")) -
                        parseFloat(elem.closest("tr").find(".rr-charge-period-payment-dgi").text().replace(",", ".")) +
                        parseFloat(elem.closest("tr").find(".rr-charge-period-recalc-dgi").text().replace(",", "."));
                }
            },
            {
                cssSelector: ".rr-charge-period-input-pkk",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-payment-pkk",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-pkk",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-recalc-pkk",
                property: ["recalcPkk", "correctionPkk"]
            },
            {
                cssSelector: ".rr-charge-period-output-pkk",
                property: function (elem) {
                    return parseFloat(elem.closest("tr").find(".rr-charge-period-input-pkk").text().replace(",", ".")) +
                        parseFloat(elem.closest("tr").find(".rr-charge-period-pkk").text().replace(",", ".")) -
                        parseFloat(elem.closest("tr").find(".rr-charge-period-payment-pkk").text().replace(",", ".")) +
                        parseFloat(elem.closest("tr").find(".rr-charge-period-recalc-pkk").text().replace(",", "."));
                }
            },
            {
                cssSelector: ".rr-charge-period-input-padun",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-payment-padun",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-padun",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-recalc-padun",
                property: ["recalcPadun", "correctionPadun"]
            },
            {
                cssSelector: ".rr-charge-period-output-padun",
                property: function (elem) {
                    return parseFloat(elem.closest("tr").find(".rr-charge-period-input-padun").text().replace(",", ".")) +
                        parseFloat(elem.closest("tr").find(".rr-charge-period-padun").text().replace(",", ".")) -
                        parseFloat(elem.closest("tr").find(".rr-charge-period-payment-padun").text().replace(",", ".")) +
                        parseFloat(elem.closest("tr").find(".rr-charge-period-recalc-padun").text().replace(",", "."));
                }
            },
            {
                cssSelector: ".rr-charge-period-input-all",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-input-all-penalty",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-payment-all",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-payment-all-penalty",
                property: null
            },
            {
                cssSelector: ".rr-charge-period-all",
                property: ["chargeTenancy", "chargeDgi", "chargePkk", "chargePadun"]
            },
            {
                cssSelector: ".rr-charge-period-all-penalty",
                property: "chargePenalty"
            },
            {
                cssSelector: ".rr-charge-period-recalc-all",
                property: ["recalcTenancy", "correctionTenancy", "recalcDgi", "correctionDgi", "recalcPkk", "correctionPkk", "recalcPadun", "correctionPadun"]
            },
            {
                cssSelector: ".rr-charge-period-recalc-all-penalty",
                property: ["recalcPenalty", "correctionPenalty"]
            },
            {
                cssSelector: ".rr-charge-period-output-all",
                property: function (elem) {
                    var dgi = parseFloat(elem.closest("tr").find(".rr-charge-period-output-dgi").text().replace(",", "."));
                    if (isNaN(dgi)) dgi = 0;
                    var pkk = parseFloat(elem.closest("tr").find(".rr-charge-period-output-pkk").text().replace(",", "."));
                    if (isNaN(pkk)) pkk = 0;
                    var padun = parseFloat(elem.closest("tr").find(".rr-charge-period-output-padun").text().replace(",", "."));
                    if (isNaN(padun)) padun = 0;
                    return parseFloat(elem.closest("tr").find(".rr-charge-period-output-tenancy").text().replace(",", ".")) + dgi + pkk + padun;
                }
            },
            {
                cssSelector: ".rr-charge-period-output-all-penalty",
                property: function (elem) {
                    return parseFloat(elem.closest("tr").find(".rr-charge-period-output-penalty").text().replace(",", "."));
                }
            }
        ];

        for (var i = 0; i < cellClasses.length; i++) {
            var elem = chargeCurrentMonthElem.find(cellClasses[i].cssSelector);
            if (elem.find("a").length > 0)
                elem = elem.find("a");
            cellClasses[i].value = parseFloat(elem.text().replace(",", "."));
            elem.html("").text(cellClasses[i].value.toFixed(2).replace(".", ","));
            if (cellClasses[i].property !== null)
                elem.html("<img width=\"15\" height=\"15\" style=\"margin-top: -2px;\" src=\"/image/spinner.gif\" />");
        }
        $.post("/KumiAccounts/CalcForecastPeriod", { idAccount: $("#IdAccount").val(), calcToDate: endDate }).done(function (resultCharge) {
            var chargeTotalElem = $(".rr-charge-total-row");
            for (var i = 0; i < cellClasses.length; i++) {
                var elem = chargeCurrentMonthElem.find(cellClasses[i].cssSelector);
                if (elem.find("a").length > 0)
                    elem = elem.find("a");
                if (cellClasses[i].property !== null) {
                    value = 0;
                    if (Array.isArray(cellClasses[i].property)) {
                        for (var j = 0; j < cellClasses[i].property.length; j++)
                            value += resultCharge[cellClasses[i].property[j]];
                    } else
                    if ($.isFunction(cellClasses[i].property))
                        value = cellClasses[i].property(elem);
                    else
                        value = resultCharge[cellClasses[i].property];
                    elem.html("").text(value.toFixed(2).replace(".", ","));
                    elem.css("color", "#ff5400").attr("title", "Значение предварительно расчитано и может измениться по окончании незавершенного периода");
                    cellClasses[i].updatedValue = value;
                }

                var totalElem = chargeTotalElem.find(cellClasses[i].cssSelector.replace("-period", "-total") + " b");
                if (totalElem.length === 1) {
                    var totalValue = parseFloat(totalElem.text().replace(",", "."));
                    totalElem.text((Math.round((totalValue + cellClasses[i].updatedValue - cellClasses[i].value) * 100) / 100).toFixed(2).replace(".", ","))
                        .css("color", "#ff5400")
                        .attr("title", "Значение с учетом незавершенного периода.\r\nБез учета незавершенного периода - " +
                            totalValue.toFixed(2).replace(".", ",") + " руб.");
                }
            }
        });
    }
});
