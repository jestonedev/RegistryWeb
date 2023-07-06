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

        if (addChargeCorrectionForm.valid()) {
            $(this).attr("disabled", "disabled").text("Сохранение...");
            var atDate = $("#AccountKumiChargeCorrection_AtDate").val();
            var description = $("#AccountKumiChargeCorrection_Description").val();
            var tenancyValue = $("#AccountKumiChargeCorrection_TenancyValue").val().replace('.', ',');
            var penaltyValue = $("#AccountKumiChargeCorrection_PenaltyValue").val().replace('.', ',');
            var idAccount = addChargeCorrectionForm.find("input[name='AccountKumiChargeCorrection.IdAccount']").val();

            $.ajax({
                type: 'POST',
                url: window.location.origin + '/KumiAccounts/AddChargeCorrection',
                data: { idAccount, atDate, tenancyValue, penaltyValue, description },
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
        var cellClasses = [".rr-charge-period-input-tenancy", ".rr-charge-period-input-penalty",
            ".rr-charge-period-payment-tenancy", ".rr-charge-period-payment-penalty",
            ".rr-charge-period-tenancy", ".rr-charge-period-penalty",
            ".rr-charge-period-recalc-tenancy", ".rr-charge-period-recalc-penalty",
            ".rr-charge-period-output-tenancy", ".rr-charge-period-output-penalty"];
        var values = [];
        var updatedValues = [];
        for (var i = 0; i < cellClasses.length; i++) {
            var elem = chargeCurrentMonthElem.find(cellClasses[i]);
            values[i] = parseFloat(elem.text().replace(",", "."));
            updatedValues[i] = values[i];
            if (i >= 4)
                elem.html("<img width=\"15\" height=\"15\" style=\"margin-top: -2px;\" src=\"/image/spinner.gif\" />");
        }
        $.post("/KumiAccounts/CalcForecastPeriod", { idAccount: $("#IdAccount").val(), calcToDate: endDate }).done(function (resultCharge) {
            var props = ["chargeTenancy", "chargePenalty", "recalcTenancy", "recalcPenalty"];
            var chargeTotalElem = $(".rr-charge-total-row");
            for (var i = 0; i < props.length; i++) {
                var value = resultCharge[props[i]];
                var elem = chargeCurrentMonthElem.find(cellClasses[i+4]);
                elem.html("").text(value.toFixed(2).replace(".", ","));
                elem.css("color", "#ff5400").attr("title", "Значение предварительно расчитано и может измениться по окончании незавершенного периода");
                updatedValues[i + 4] = value;
                var totalElem = chargeTotalElem.find(cellClasses[i+4].replace("-period", "-total")+" b");
                if (totalElem.length === 1) {
                    var totalValue = parseFloat(totalElem.text().replace(",", "."));
                    totalElem.text((Math.round((totalValue + updatedValues[i + 4] - values[i + 4]) * 100) / 100).toFixed(2).replace(".", ","))
                        .css("color", "#ff5400")
                        .attr("title", "Значение с учетом незавершенного периода.\r\nБез учета незавершенного периода - " +
                            totalValue.toFixed(2).replace(".", ",") + " руб.");
                }
            }

            for (var j = 0; j <= 1; j++) {
                chargeCurrentMonthElem.find(cellClasses[j+8]).html("")
                    .text((Math.round((updatedValues[j + 0] - updatedValues[j + 2] + updatedValues[j + 4] + updatedValues[j +6]) * 100) / 100).toFixed(2).replace(".", ","))
                    .css("color", "#ff5400")
                    .attr("title", "Значение расчитано с учетом начисления за незавершенный период и может измениться по окончании периода." +
                        "\r\nБез учета начисления за незавершенный период - " + values[j +8].toFixed(2).replace(".", ",") + " руб.");
            }
        });
    }
});
