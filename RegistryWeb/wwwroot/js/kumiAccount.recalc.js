$(document).ready(function () {

    var recalcAccountForm = $("#accountRecalcForm");
    var recalcAccountFormValidator = recalcAccountForm.validate();
    recalcAccountFormValidator.settings.ignore = '.rr-valid-ignore';
    recalcAccountFormValidator.settings.ignoreTitle = true;

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

    $("#accountRecalcForm .rr-report-submit").on("click", function (e) {
        if ($(this).is)
        $(".rr-recalc-account-error").addClass("d-none");
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
                    $(".rr-recalc-account-error").removeClass("d-none").text(data.error);
                    recalcAccountForm.find(".progress").addClass("d-none");
                    $(this).removeAttr("disabled").text("Перерасчет");
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
});
