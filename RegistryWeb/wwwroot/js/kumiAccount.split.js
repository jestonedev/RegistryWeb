$(function () {
    $("#KumiSplitAccount").on("click", function (e) {
        e.preventDefault();
        var modal = $("#SplitAccountModal");
        modal.find("input, select").prop("disabled", "");
        modal.find("#SplitAccount_TotalFraction").prop("disabled", "disabled");
        var idAccount = $(this).data("idAccount");
        modal.find("input[name='IdAccount']").val(idAccount);
        modal.modal("show");
        addSplitAccountString();
    });

    function addSplitAccountString() {
        var modal = $("#SplitAccountModal");
        $.get("/KumiAccounts/GetSplitAccountString?v=" + (new Date()).getTime(), function (result) {
            modal.find(".rr-split-accounts-wrapper").append(result);
            modal.find(".rr-split-accounts-wrapper .rr-split-account-loading").addClass("d-none");
            var row = modal.find(".rr-split-accounts-wrapper .rr-split-account-string").last();
            if ($(row).index() === 1) {
                $(row).find(".rr-add-split-account-string").removeClass("d-none");
                $(row).find(".rr-remove-split-account-string").remove();
            } else {
                $(row).find(".rr-remove-split-account-string").removeClass("d-none");
                $(row).find(".rr-add-split-account-string").remove();
            }
            refreshValidationForm($("#SplitAccountForm"));
        });
    }

    $(".rr-split-accounts-wrapper").on("click", ".rr-add-split-account-string", function (e) {
        e.preventDefault();
        addSplitAccountString();
    });

    $(".rr-split-accounts-wrapper").on("click", ".rr-remove-split-account-string", function (e) {
        e.preventDefault();
        $(this).closest(".rr-split-account-string").remove();
        refreshValidationForm($("#SplitAccountForm"));
    });

    $(".rr-split-accounts-wrapper").on("change", ".input-fraction", function () {
        var val = $.trim($(this).val());
        if (val === "" || val === undefined) {
            $(this).val("0,0000");
            calcTotalFraction();
            return;
        }
        var valDecimal = parseFloat(val.replace(",", "."));
        if (valDecimal > 1) {
            $(this).val("1,0000");
            calcTotalFraction();
            return;
        }
        val = valDecimal.toString().replace(".", ",");
        if (val.indexOf(",") === -1) val += ",";
        while (val.length < 6) {
            val += "0";
        }
        $(this).val(val);
        calcTotalFraction();
    });

    function calcTotalFraction() {
        var allFractions = $(".rr-split-accounts-wrapper .input-fraction").map(function (idx, elem) {
            return parseFloat($(elem).val().replace(",", "."));
        }).toArray();
        var totalFraction = allFractions.reduce(function (acc, v) { return acc + v; });
        totalFraction = Math.round(totalFraction * 10000) / 10000;
        var totalFractionStr = totalFraction.toString().replace(".", ",");
        if (totalFractionStr.indexOf(",") === -1) totalFractionStr += ",";
        while (totalFractionStr.length < 6) {
            totalFractionStr += "0";
        }
        $("#SplitAccount_TotalFraction").val(totalFractionStr);
    }

    $("#SplitAccountForm .rr-split-submit").on("click", function (e) {
        e.preventDefault();
        var form = $(this).closest("form");
        var self = this;
        form.find(".rr-split-state-wrapper").addClass("d-none");
        if (form.valid()) {
            $(this).addClass("disabled").text("Разделить...");
            var onDate = form.find("#SplitAccount_OnDate").val();
            var idAccount = form.find("input[name='IdAccount']").val();
            var rows = form.find(".rr-split-account-string");
            var formData = new FormData();
            formData.append("IdAccount", idAccount);
            formData.append("OnDate", onDate);
            for (var i = 0; i < rows.length; i++) {
                formData.append("SplitAccounts[" + i + "].Account", $(rows[i]).find("input[id^='SplitAccount_Account_']").val());
                formData.append("SplitAccounts[" + i + "].Fraction", $(rows[i]).find("input[id^='SplitAccount_Fraction_']").val());
                formData.append("SplitAccounts[" + i + "].Owner", $(rows[i]).find("input[id^='SplitAccount_Owner_']").val());
            }
            form.find(".rr-split-state-wrapper").removeClass("d-none");
            form.find(".rr-split-state-wrapper .rr-split-account-loading").removeClass("d-none");
            form.find(".rr-split-state-wrapper .rr-split-state").addClass("d-none").text("");
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/KumiAccounts/SplitAccount',
                data: formData,
                processData: false,
                contentType: false,
                success: function (result) {
                    form.find(".rr-split-state .rr-split-account-loading").addClass("d-none");
                    if (result.state !== "Success") {
                        $(self).removeClass("disabled").text("Разделить");
                        form.find(".rr-split-state-wrapper .rr-split-account-loading").addClass("d-none");
                        form.find(".rr-split-state-wrapper .rr-split-state").removeClass("d-none").text(result.error);
                    } else {
                        $("#SplitAccountModal").modal("hide");
                        var url = "/KumiAccounts/?";
                        for (var i = 0; i < result.accounts.length; i++) {
                            url += "FilterOptions.IdsAccount=" + result.accounts[i];
                            if (i < result.accounts.length - 1)
                                url += "&";
                        }
                        downloadFile(url);
                    }
                }
            });
        }
    });
});