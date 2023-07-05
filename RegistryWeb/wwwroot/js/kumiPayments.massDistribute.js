$(function () {
    var paymentRows = $(".rr-payment-for-distribution[data-id-payment]").filter(function (idx, elem) {
        return !$(elem).find(".rr-payment-distribution-details-loader").hasClass("d-none");
    }).toArray();
    if (paymentRows.length > 0) {
        var paymentRow = paymentRows.shift();
        getPaymentDistributionInfo(paymentRow);
    }

    function getPaymentDistributionInfo(paymentRow) {
        var purpose = $(paymentRow).find(".rr-payment-purpose").text();
        var purposeInfo = parsePurpose(purpose);
        if (isEmptyPurpose(purposeInfo)) {
            notFoundDistribObjectByPurpose(paymentRow, "Назначение платежа не определено", "Выберите лицевой счет или ПИР для распределния платежа");
            if (paymentRows.length > 0) {
                getPaymentDistributionInfo(paymentRows.shift());
            } else {
                $("#RunMassPaymentDistribution").removeClass("disabled");
            }
        } else {
            var idStreetElem = $("#KladrStreetsForSearchIdByName");
            var idStreet = null;
            if (purposeInfo.address !== null && purposeInfo.address.street !== null) {
                idStreet = getIdStreetForStreetName(purposeInfo.address.street,
                    idStreetElem.find("option").map(function (idx, opt) {
                        var street = $(opt).text();
                        var idStreet = $(opt).attr("value");
                        return { street, idStreet };
                    }));
            }
            $.ajax({
                async: true,
                type: 'POST',
                url: window.location.origin + '/KumiPayments/GetDistributePaymentToObjects',
                data: {
                    "FilterOptions.DistributeTo": purposeInfo.court_order !== null ? 1 : 0,

                    "FilterOptions.ClaimCourtOrderNum": purposeInfo.court_order,
                    "FilterOptions.Account": purposeInfo.account,

                    "FilterOptions.IdStreet": idStreet, 
                    "FilterOptions.House": purposeInfo.address !== null ? purposeInfo.address.house : null,
                    "FilterOptions.PremisesNum": purposeInfo.address !== null ? purposeInfo.address.premise : null
                },
                success: function (result) {
                    if (result.count === 0) {
                        notFoundDistribObjectByPurpose(paymentRow, "Назначение платежа не определено", "Выберите лицевой счет или ПИР для распределния платежа");
                    }
                    if (result.count > 1) {
                        notFoundDistribObjectByPurpose(paymentRow, "Найдено более одного совпадения", "Выберите лицевой счет или ПИР для распределния платежа");
                    }
                    if (result.count === 1) {
                        buildMassDistributionFormInfo(paymentRow, result);
                    }

                    if (paymentRows.length > 0) {
                        getPaymentDistributionInfo(paymentRows.shift());
                    } else {
                        $("#RunMassPaymentDistribution").removeClass("disabled");
                    }
                }
            });
        }
    }

    function buildMassDistributionFormInfo(paymentRow, result, forceTenancySum, forcePenaltySum, hideResultInfo) {
        if (hideResultInfo)
            $(paymentRow).find(".rr-payment-distribution-result-info").addClass("d-none");
        $(paymentRow).find(".rr-payment-distribution-details-loader").addClass("d-none");
        $(paymentRow).find(".rr-payment-distribution-object-info").removeClass("text-danger").removeClass("d-none");
        $(paymentRow).find(".rr-payment-distribution-object-info").find(".rr-control-change-object, .rr-control-delete-object").removeClass("d-none");
        $(paymentRow).find(".rr-payment-distribution-object-info").find(".rr-control-add-object").addClass("d-none");
        $(paymentRow).find(".rr-payment-distribution-sums-wrapper").removeClass("d-none");

        var sum = $(paymentRow).data("paymentSum");
        sum = (sum + "").replace(",", ".");
        sum = parseFloat(sum);

        var sumPosted = $(paymentRow).data("paymentSumPosted");
        sumPosted = (sumPosted + "").replace(",", ".");
        sumPosted = parseFloat(sumPosted);

        var sumForDistribution = sum - sumPosted;
        var currentPenalty = 0;
        var currentTenancy = 0;

        if (result.claims !== undefined) {
            var claim = result.claims[0];

            var html = buildClaimForMassDistributionFormInfo(claim);
            $(paymentRow).find(".rr-payment-distribution-object-info .rr-payment-distribution-object-caption").html(html);

            currentPenalty = claim.amountPenalties === undefined ? 0 : claim.amountPenalties;
            currentPenalty -= claim.amountPenaltiesRecovered === null ? 0 : claim.amountPenaltiesRecovered;
            currentTenancy = claim.amountTenancy === undefined ? 0 : claim.amountTenancy;
            currentTenancy -= claim.amountTenancyRecovered === null ? 0 : claim.amountTenancyRecovered;

            $(paymentRow).data("idClaim", claim.idClaim);
            $(paymentRow).data("state", "selected");
        }
        if (result.accounts !== undefined) {
            var account = result.accounts[0];
            html = buildAccountForMassDistributionFormInfo(account);
            $(paymentRow).find(".rr-payment-distribution-object-info .rr-payment-distribution-object-caption").html(html);

            currentPenalty = account.currentBalancePenalty === null ? 0 : account.currentBalancePenalty;
            currentTenancy = account.currentBalanceTenancy === null ? 0 : account.currentBalanceTenancy;

            $(paymentRow).data("idAccount", account.idAccount);
            $(paymentRow).data("state", "selected");
        }

        var distributionTenancy = Math.round(Math.max(Math.min(sumForDistribution, Math.max(currentTenancy, 0)), 0) * 100) / 100;
        var distributionPenalty = Math.max(Math.min(Math.round((sumForDistribution - distributionTenancy) * 100) / 100, currentPenalty), 0);
        distributionTenancy = Math.round((sumForDistribution - distributionPenalty) * 100) / 100;

        $(paymentRow).find(".rr-distribution-tenancy-sum").val((((forceTenancySum === undefined || forceTenancySum === false) ? distributionTenancy.toString() : forceTenancySum.toString())).replace(".", ","));
        $(paymentRow).find(".rr-distribution-penalty-sum").val((((forcePenaltySum === undefined || forcePenaltySum === false) ? distributionPenalty.toString() : forcePenaltySum.toString())).replace(".", ","));
    }

    function buildAccountForMassDistributionFormInfo(account) {
        stateClass = "";
        switch (account.idState) {
            case 1:
                stateClass = "text-success";
                break;
            case 2:
                stateClass = "text-danger";
                break;
            case 3:
            case 4:
                stateClass = "text-warning";
                break;
        }

        var currentBalanceTenancy = account.currentBalanceTenancy;
        if (currentBalanceTenancy === null) currentBalanceTenancy = 0;
        var currentBalancePenalty = account.currentBalancePenalty;
        if (currentBalancePenalty === null) currentBalancePenalty = 0;

        var lastChargeDateStr = null;
        if (account.lastChargeDate !== null) {
            var lastChargeDate = new Date(account.lastChargeDate);
            var year = lastChargeDate.getFullYear();
            var month = lastChargeDate.getMonth() + 1;
            var day = lastChargeDate.getDate();
            lastChargeDateStr = (day < 10 ? "0" + day : day) + "." + (month < 10 ? "0" + month : month) + "." + year;
        } else {
            lastChargeDateStr = "н/а";
        }

        var html = "<div class='rr-payment-distribution-object-caption-wrapper' data-id-account='" + account.idAccount + "' data-account='" + account.account + "' data-account-state='" + account.state
            + "' data-account-id-state='" + account.idState + "' data-current-balance-tenancy='" + currentBalanceTenancy + "' data-current-balance-penalty='" + currentBalancePenalty
            + "' data-last-charge-date='" + account.lastChargeDate +"'><u>ЛС №" + account.account + ":</u>" +
            " <sup><span title='" + account.state + "' class='" + stateClass + "'><b>" + account.state.substr(0, 1) + "</b></span></sup> " +
            "<a class=\"btn oi oi-eye p-0 text-primary rr-payment-list-eye-btn\" target=\"_blank\" href=\"/KumiAccounts/Details?idAccount=" + account.idAccount + "\"></a>" +
            "</div>" +
            "<div class=\"text-left\"><b>Текущее сальдо:</b><br class=\"d-lg-none\"/> найм " + distributePaymentFormatSum(currentBalanceTenancy) + " руб., пени " + distributePaymentFormatSum(currentBalancePenalty) + " руб.</div>" +
            "<div class=\"text-left\"><b>Посл. начисление:</b><br class=\"d-lg-none\"/> " + lastChargeDateStr + "</div>";
        return html;
    }

    function buildClaimForMassDistributionFormInfo(claim) {
        stateClass = "";
        switch (claim.idAccountState) {
            case 1:
                stateClass = "text-success";
                break;
            case 2:
                stateClass = "text-danger";
                break;
            case 3:
            case 4:
                stateClass = "text-warning";
                break;
        }

        var amountTenancy = claim.amountTenancy;
        if (amountTenancy === null) amountTenancy = 0;
        var amountPenalties = claim.amountPenalties;
        if (amountPenalties === null) amountPenalties = 0;
        var amountTenancyRecovered = claim.amountTenancyRecovered;
        if (amountTenancyRecovered === null) amountTenancyRecovered = 0;
        var amountPenaltiesRecovered = claim.amountPenaltiesRecovered;
        if (amountPenaltiesRecovered === null) amountPenaltiesRecovered = 0;

        var html = "<div class='rr-payment-distribution-object-caption-wrapper' data-id-claim='" + claim.idClaim + "' data-id-account='" + claim.idAccount
            + "' data-account='" + claim.account + "' data-account-id-state='" + claim.idAccountState + "' data-account-state='" + claim.accountState
            + "' data-account-current-balance-tenancy='" + claim.accountCurrentBalanceTenancy + "' data-account-current-balance-penalty='" + claim.accountCurrentBalancePenalty
            + "' data-claim-amount-tenancy='" + amountTenancy + "' data-claim-amount-penalty='" + amountPenalties + "' data-claim-amount-tenancy-recovered='" + amountTenancyRecovered
            + "' data-claim-amount-penalty-recovered='" + amountPenaltiesRecovered + "' data-claim-start-dept-period='" + claim.startDeptPeriod
            + "' data-claim-end-dept-period='"+claim.endDeptPeriod+"'><u>ИР №" + claim.idClaim + "</u> " +
            "<a class=\"btn oi oi-eye p-0 text-primary rr-payment-list-eye-btn\" target=\"_blank\" href=\"/Claims/Details?idClaim=" + claim.idClaim + "\"></a> <br class=\"d-lg-none\"/>" +
            "<u>ЛС №" + claim.account + ":</u>" +
            " <sup><span title='" + claim.accountState + "' class='" + stateClass + "'><b>" + claim.accountState.substr(0, 1) + "</b></span></sup> " +
            "<a class=\"btn oi oi-eye p-0 text-primary rr-payment-list-eye-btn\" target=\"_blank\" href=\"/KumiAccounts/Details?idAccount=" + claim.idAccount + "\"></a>" +
            "</div>" +
            "<div class=\"text-left\"><b>Текущее сальдо:</b><br class=\"d-lg-none\"/> найм " + distributePaymentFormatSum(claim.accountCurrentBalanceTenancy) + " руб., пени " + distributePaymentFormatSum(claim.accountCurrentBalancePenalty)+" руб.</div>" +
            "<div class=\"text-left\"><b>Период взыскания:</b><br class=\"d-lg-none\"/> " + claimDeptPeriodsToStr(claim) +"</div>" +
            "<div class=\"text-left\"><b>Взыскиваемая сумма:</b><br class=\"d-lg-none\"/> найм " + distributePaymentFormatSum(amountTenancy) + " руб., пени " + distributePaymentFormatSum(amountPenalties)+" руб.</div>" +
            "<div class=\"text-left\"><b>Взысканная сумма:</b><br class=\"d-lg-none\"/> найм " + distributePaymentFormatSum(amountTenancyRecovered) + " руб., пени " + distributePaymentFormatSum(amountPenaltiesRecovered)+" руб.</div>";
        return html;
    }

    function notFoundDistribObjectByPurpose(paymentRow, resultInfo, objectInfo) {
        $(paymentRow).find(".rr-payment-distribution-details-loader").addClass("d-none");
        $(paymentRow).find(".rr-payment-distribution-result-info").text(resultInfo).addClass("text-danger").removeClass("d-none");
        $(paymentRow).find(".rr-payment-distribution-object-info .rr-payment-distribution-object-caption").html("<span class='text-danger'>" + objectInfo + "</span>");
        $(paymentRow).find(".rr-payment-distribution-object-info").find(".rr-control-change-object, .rr-control-delete-object").addClass("d-none");
        $(paymentRow).find(".rr-payment-distribution-object-info").find(".rr-control-add-object").removeClass("d-none");
        $(paymentRow).find(".rr-payment-distribution-object-info").removeClass("d-none");
    }

    $(".rr-payment-for-distribution").on("click", ".rr-control-add-object", function (e) {
        var paymentRow = $(this).closest("tr");
        var idPayment = paymentRow.data("idPayment");
        var paymentSum = paymentRow.data("paymentSum");
        var paymentSumPosted = paymentRow.data("paymentSumPosted");
        var purpose = paymentRow.find(".rr-payment-purpose.rr-distribute-form-payment-purpose").text();

        distributionModalInitiate(purpose, null, paymentSum, paymentSumPosted, idPayment, true, attachPaymentToObjectOnSelectCallback);

        e.preventDefault();
    });

    $(".rr-payment-for-distribution").on("click", ".rr-control-change-object", function (e) {
        var paymentRow = $(this).closest("tr");
        var idPayment = paymentRow.data("idPayment");
        var paymentSum = paymentRow.data("paymentSum");
        var paymentSumPosted = paymentRow.data("paymentSumPosted");
        var purpose = paymentRow.find(".rr-payment-purpose.rr-distribute-form-payment-purpose").text();
        var account = paymentRow.find(".rr-payment-distribution-object-caption-wrapper").data("account");
        distributionModalInitiate(purpose, account, paymentSum, paymentSumPosted, idPayment, true, attachPaymentToObjectOnSelectCallback);

        e.preventDefault();
    });

    function attachPaymentToObjectOnSelectCallback(distribInfo) {
        var paymentRow = $(".rr-payment-for-distribution[data-id-payment='" + distribInfo.IdPayment + "']");
        var result = { count: 1 };
        if (distribInfo.DistributeTo === 0) {
            result.accounts = [distribInfo.Description];
        } else {
            result.claims = [distribInfo.Description];
        }
        buildMassDistributionFormInfo(paymentRow, result, distribInfo.TenancySum, distribInfo.PenaltySum, true);

        $("#DistributePaymentToAccountModal").modal('hide');

    }

    $(".rr-payment-for-distribution").on("click", ".rr-control-delete-object", function (e) {
        var paymentRow = $(this).closest("tr");
        var paymentSumText = paymentRow.find(".rr-payment-sum").text();
        var paymentDocRequisitsHtml = paymentRow.find(".rr-payment-doc-requisits").html();
        var idPayment = paymentRow.data("idPayment");
        var distribObjectCaptionHtml = paymentRow.find(".rr-payment-distribution-object-caption").html();
        var modal = $("#ConfirmDeletePaymentLinkToDistirbutionObjectModal");
        modal.find(".rr-payment-doc-requisits").html(paymentDocRequisitsHtml);
        modal.find(".rr-payment-sum").text(paymentSumText);
        modal.find("#ConfirmDeletePaymentLinkToDistirbutionObject_IdPayment").val(idPayment);
        modal.find(".rr-distrib-object-body").html(distribObjectCaptionHtml);
        modal.modal('show');
        e.preventDefault();
    });

    $("#confirmDeletePaymentLinkToDistirbutionObjectModalBtn").on("click", function (e) {
        var modal = $("#ConfirmDeletePaymentLinkToDistirbutionObjectModal");
        var idPayment = modal.find("#ConfirmDeletePaymentLinkToDistirbutionObject_IdPayment").val();
        var paymentRow = $(".rr-payment-for-distribution[data-id-payment='" + idPayment + "']");
        paymentRow.find(".rr-payment-distribution-result-info").removeClass("d-none").addClass("text-danger").removeClass("alert")
            .removeClass("alert-danger").removeClass("alert-warning").text("Назначение платежа не выбрано");
        paymentRow.find(".rr-payment-distribution-object-caption").html("<span class='text-danger'>Выберите лицевой счет или ПИР для распределния платежа</span>");
        paymentRow.find(".rr-payment-distribution-object-controls").find(".rr-control-delete-object, .rr-control-change-object").addClass("d-none");
        paymentRow.find(".rr-payment-distribution-object-controls .rr-control-add-object").removeClass("d-none");
        paymentRow.find(".rr-payment-distribution-sums-wrapper").addClass("d-none").find("input").val("");
        $(paymentRow).removeData("idAccount");
        $(paymentRow).removeData("idClaim");
        $(paymentRow).data("state", "notselected");
        modal.modal('hide');
    });

    $(".rr-payment-for-distribution").on("click", ".rr-remove-payment-from-master", function (e) {
        var paymentRow = $(this).closest("tr");
        var paymentSumText = paymentRow.find(".rr-payment-sum").text();
        var paymentDocRequisitsHtml = paymentRow.find(".rr-payment-doc-requisits").html();
        var idPayment = paymentRow.data("idPayment");
        var modal = $("#ConfirmDeletePaymentFromMassDistributeFormModal");
        modal.find(".rr-payment-doc-requisits").html(paymentDocRequisitsHtml);
        modal.find(".rr-payment-sum").text(paymentSumText);
        modal.find("#ConfirmDeletePaymentFromMassDistributeForm_IdPayment").val(idPayment);
        modal.modal('show');
        e.preventDefault();
    });

    $("#confirmDeletePaymentFromMassDistributeFormModalBtn").on("click", function (e) {
        var modal = $("#ConfirmDeletePaymentFromMassDistributeFormModal");
        var idPayment = modal.find("#ConfirmDeletePaymentFromMassDistributeForm_IdPayment").val();
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/KumiPayments/CheckIdToSession',
            data: { id: idPayment, isCheck: false },
            success: function () {
                $(".rr-payment-for-distribution[data-id-payment='" + idPayment+"']").remove();
                modal.modal('hide');
            }
        });
        e.preventDefault();
    });

    $(".rr-payment-for-distribution").on("click", ".rr-distrib-to-tenancy-sum-lefovers", function (e) {
        var paymentRow = $(this).closest("tr");
        var sum = parseFloat((paymentRow.data("paymentSum") + "").replace(",", "."));
        var sumPosted = parseFloat((paymentRow.data("paymentSumPosted") + "").replace(",", "."));
        var sumForDistribute = sum - sumPosted;
        var penaltySum = parseFloat(paymentRow.find(".rr-distribution-penalty-sum").val().replace(",", "."));
        var tenancySum = Math.round(Math.max(0, sumForDistribute - penaltySum) * 100) / 100;
        paymentRow.find(".rr-distribution-tenancy-sum").val((tenancySum + "").replace(".", ","));
        e.preventDefault();
    });

    $(".rr-payment-for-distribution").on("click", ".rr-distrib-to-penalty-sum-lefovers", function (e) {
        var paymentRow = $(this).closest("tr");
        var sum = parseFloat((paymentRow.data("paymentSum") + "").replace(",", "."));
        var sumPosted = parseFloat((paymentRow.data("paymentSumPosted") + "").replace(",", "."));
        var sumForDistribute = sum - sumPosted;
        var tenancySum = parseFloat(paymentRow.find(".rr-distribution-tenancy-sum").val().replace(",", "."));
        var penaltySum = Math.round(Math.max(0, sumForDistribute - tenancySum) * 100) / 100;
        paymentRow.find(".rr-distribution-penalty-sum").val((penaltySum + "").replace(".", ","));
        e.preventDefault();
    });

    $(".rr-payment-for-distribution").on("change", ".rr-distribution-tenancy-sum, .rr-distribution-penalty-sum", function () {
        var paymentRow = $(this).closest("tr");
        var sum = parseFloat((paymentRow.data("paymentSum") + "").replace(",", "."));
        var sumPosted = parseFloat((paymentRow.data("paymentSumPosted") + "").replace(",", "."));
        var sumForDistribution = sum - sumPosted;

        var value = parseFloat($(this).val().replace(",", "."));
        if (isNaN(value)) value = 0;
        value = Math.min(sumForDistribution, value);
        $(this).val((value + "").replace(".", ","));
        var diffSum = Math.round((sumForDistribution - value) * 100) / 100;
        if ($(this).hasClass("rr-distribution-tenancy-sum")) {
            var penalty = parseFloat(paymentRow.find(".rr-distribution-penalty-sum").val().replace(",", "."));
            if (penalty > diffSum)
                paymentRow.find(".rr-distribution-penalty-sum").val((diffSum + "").replace(".", ","));
        }
        if ($(this).hasClass("rr-distribution-penalty-sum")) {
            var tenancy = parseFloat(paymentRow.find(".rr-distribution-tenancy-sum").val().replace(",", "."));
            if (tenancy > diffSum)
                paymentRow.find(".rr-distribution-tenancy-sum").val((diffSum + "").replace(".", ","));
        }
    });

    $("#RunMassPaymentDistribution").on("click", function () {
        $("#ConfirmRunPaymentDistirbutionModal").modal('show');
    });

    var paymentsForDistribution = [];

    $("#confirmRunPaymentDistirbutionBtn").on("click", function () {
        stopingDistribution = false;
        $("#MassPaymentDistributionBackBtn, #MassPaymentDistributionClearAllBtn, #RunMassPaymentDistribution, .rr-remove-payment-from-master").addClass("disabled");
        $(".rr-payment-distribution-object-controls").addClass("d-none");
        $(".rr-payment-distribution-sums-wrapper").find("input, button").attr("disabled", "disabled");
        $("#StopMassPaymentDistribution").removeClass("disabled");
        $("#ConfirmRunPaymentDistirbutionModal").modal('hide');

        var paymentsForDistributionRows = $(".rr-payment-for-distribution").filter(function (idx, elem) {
            return $(elem).data("state") === "selected" && $(elem).find(".rr-payment-checked-for-distrib").is(":checked");
        });

        paymentsForDistributionRows.find(".rr-payment-distribution-result-info").addClass("d-none").removeClass("alert").removeClass("alert-primary").removeClass("alert-danger").removeClass("alert-warning").removeClass("text-danger").removeClass("text-warning");
        paymentsForDistributionRows.find(".rr-payment-distribution-details-loader").addClass("d-none");
        paymentsForDistribution = paymentsForDistributionRows.map(function (idx, elem) {
            return {
                "IdPayment": $(elem).data("idPayment"),
                "IdObject": $(elem).data("idClaim") === undefined ? $(elem).data("idAccount") : $(elem).data("idClaim"),
                "DistributeTo": $(elem).data("idClaim") === undefined ? 0 : 1,
                "TenancySum": $(elem).find(".rr-distribution-tenancy-sum").val().replace(".", ","),
                "PenaltySum": $(elem).find(".rr-distribution-penalty-sum").val().replace(".", ",")
                };
            }).toArray();

        if (paymentsForDistribution.length > 0) {
            var paymentInfo = paymentsForDistribution.shift();
            distributePayment(paymentInfo);
        } else {
            cancelPaymentDistribution();
        }

    });

    $("#StopMassPaymentDistribution").on("click", function () {
        $("#ConfirmStopPaymentDistirbutionModal").modal('show');
    });

    var stopingDistribution = false;

    $("#confirmStopPaymentDistirbutionBtn").on("click", function () {
        stopingDistribution = true;
        $("#ConfirmStopPaymentDistirbutionModal").modal('hide');
    });

    function cancelPaymentDistribution() {
        $("#MassPaymentDistributionBackBtn, #MassPaymentDistributionClearAllBtn, #RunMassPaymentDistribution, .rr-remove-payment-from-master").removeClass("disabled");
        $(".rr-payment-distribution-object-controls").removeClass("d-none");
        $(".rr-payment-distribution-sums-wrapper").find("input, button").removeAttr("disabled");
        $("#StopMassPaymentDistribution").addClass("disabled");
    }

    function distributePayment(paymentInfo) {
        var paymentRow = $(".rr-payment-for-distribution[data-id-payment='" + paymentInfo.IdPayment + "']");
        paymentRow.find(".rr-payment-distribution-details-loader .rr-loader-title").text("Распределение платежа");
        paymentRow.find(".rr-payment-distribution-details-loader").addClass("alert").addClass("alert-primary").removeClass("d-none");

        var url = window.location.origin + '/KumiPayments/DistributePaymentToAccount';
        $.ajax({
            async: true,
            type: 'POST',
            url: url,
            data: paymentInfo,
            success: function (result) {
                paymentRow.find(".rr-payment-distribution-details-loader").addClass("d-none");
                if (result.state === "Error") {
                    paymentRow.find(".rr-payment-distribution-result-info").text(result.error).addClass("text-danger").addClass("alert").addClass("alert-danger").removeClass("d-none");
                } else {
                    //var oldPostedSum = parseFloat((paymentRow.data("paymentSumPosted") + "").replace(",", "."));
                    var sum = parseFloat((paymentRow.data("paymentSum") + "").replace(",", "."));
                    var postedSum = result.distrubutedToTenancySum + result.distrubutedToPenaltySum;
                    var distribTenancySum = parseFloat(paymentRow.find(".rr-distribution-tenancy-sum").val().replace(",", "."));
                    var distribPenaltySum = parseFloat(paymentRow.find(".rr-distribution-penalty-sum").val().replace(",", "."));
                    postedSum = Math.round(postedSum * 100) / 100;
                    paymentRow.data("paymentSumPosted", postedSum.toString().replace(".", ","));
                    var distribObject = getDistributionObjectForPaymentRow(paymentRow);
                    if (postedSum === sum) {
                        paymentRow.find(".rr-payment-distribution-result-info").text("Платеж распределен ").addClass("text-success").removeClass("d-none");
                        paymentRow.find(".rr-payment-distribution-object-info, .rr-payment-distribution-sums-wrapper").addClass("d-none");
                        paymentRow.data("state", "distributed");
                    } else {
                        paymentRow.find(".rr-payment-distribution-result-info").text("Платеж распределен не полностью ").addClass("alert").addClass("alert-warning").addClass("text-warning").removeClass("d-none");

                        if (distribObject.claims === undefined) {
                            var account = distribObject.accounts[0];
                            account.currentBalanceTenancy = Math.round((account.currentBalanceTenancy - distribTenancySum)*100)/100;
                            account.currentBalancePenalty = Math.round((account.currentBalancePenalty - distribPenaltySum) * 100) / 100;
                        } else {
                            var claim = distribObject.claims[0];
                            claim.amountTenancyRecovered = Math.round((claim.amountTenancyRecovered + distribTenancySum) * 100) / 100;
                            claim.amountPenaltiesRecovered = Math.round((claim.amountPenaltiesRecovered + distribPenaltySum) * 100) / 100;
                        }
                        buildMassDistributionFormInfo(paymentRow, distribObject, false);
                    }
                    buildPaymentInfo(paymentRow, distribObject, distribTenancySum, distribPenaltySum);
                }
                if (paymentsForDistribution.length > 0 && !stopingDistribution) {
                    var paymentInfo = paymentsForDistribution.shift();
                    distributePayment(paymentInfo);
                } else {
                    cancelPaymentDistribution();
                }
            }
        });
    }

    function getDistributionObjectForPaymentRow(paymentRow) {
        var data = {
            "Count": 1
        };
        var distributionObjectWrapper = paymentRow.find(".rr-payment-distribution-object-caption-wrapper");


        if (distributionObjectWrapper.data("idClaim") === undefined) {
            data.accounts = [
                {
                    idAccount: distributionObjectWrapper.data("idAccount"),
                    account: distributionObjectWrapper.data("account"),
                    idState: distributionObjectWrapper.data("accountIdState"),
                    state: distributionObjectWrapper.data("accountState"),
                    currentBalanceTenancy: distributionObjectWrapper.data("currentBalanceTenancy"),
                    currentBalancePenalty: distributionObjectWrapper.data("currentBalancePenalty"),
                    lastChargeDate: distributionObjectWrapper.data("lastChargeDate")
                }
            ];
        } else {
            data.claims = [
                {
                    idClaim: distributionObjectWrapper.data("idClaim"),
                    idAccount: distributionObjectWrapper.data("idAccount"),
                    account: distributionObjectWrapper.data("account"),
                    idAccountState: distributionObjectWrapper.data("accountIdState"),
                    accountState: distributionObjectWrapper.data("accountState"),
                    accountCurrentBalanceTenancy: distributionObjectWrapper.data("accountCurrentBalanceTenancy"),
                    accountCurrentBalancePenalty: distributionObjectWrapper.data("accountCurrentBalancePenalty"),
                    amountTenancy: distributionObjectWrapper.data("claimAmountTenancy"),
                    amountPenalties: distributionObjectWrapper.data("claimAmountPenalty"),
                    amountTenancyRecovered: distributionObjectWrapper.data("claimAmountTenancyRecovered"),
                    amountPenaltiesRecovered: distributionObjectWrapper.data("claimAmountPenaltyRecovered"),
                    startDeptPeriod: distributionObjectWrapper.data("claimStartDeptPeriod"),
                    endDeptPeriod: distributionObjectWrapper.data("claimEndDeptPeriod")
                }
            ];
        }
        return data;
    }

    function buildPaymentInfo(paymentRow, distribObject, distribTenancySum, distribPenaltySum) {
        var sum = parseFloat((paymentRow.data("paymentSum") + "").replace(",", "."));
        var postedSum = parseFloat((paymentRow.data("paymentSumPosted") + "").replace(",", "."));
        paymentRow.find(".rr-payment-sum").text(distributePaymentFormatSum(sum) + " руб., расп.: " + distributePaymentFormatSum(postedSum)+" руб.");

        var paymentObjectDetailsRows = paymentRow.find(".rr-payment-distribution-sum-details .rr-payment-object-detail");
        var html = "";
        var elem = null;
        if (distribObject.claims === undefined) {
            var account = distribObject.accounts[0];
            var elems = paymentObjectDetailsRows.filter(function (idx, elem) { return $(elem).data("idAccount") === account.idAccount; });
            if (elems.length !== 0) elem = elems[0];
            else {
                var elemOuter = $('<div class="text-danger rr-payment-object-detail" data-id-account="' + account.idAccount + '" data-payment-distrib-tenancy="0" data-payment-distrib-penalty="0">');
                paymentRow.find(".rr-payment-distribution-sum-details").append(elemOuter);
                elem = paymentRow.find(".rr-payment-distribution-sum-details .rr-payment-object-detail").last();
            }
            var distributedTenancy = Math.round((parseFloat(($(elem).data("paymentDistribTenancy") + "").replace(",", ".")) + distribTenancySum) * 100) / 100;
            var distributedPenalty = Math.round((parseFloat(($(elem).data("paymentDistribPenalty") + "").replace(",", ".")) + distribPenaltySum) * 100) / 100;
            $(elem).data("paymentDistribTenancy", distributedTenancy);
            $(elem).data("paymentDistribPenalty", distributedPenalty);

            html = "<u>ЛС №" + account.account + ":</u> <a class=\"btn oi oi-eye p-0 text-primary rr-payment-list-eye-btn\" target=\"_blank\" href=\"/KumiAccounts/Details?idAccount=" + account.idAccount
                + "\"></a><br>найм " + distributePaymentFormatSum(distributedTenancy) + " руб., пени " + distributePaymentFormatSum(distributedPenalty) + " руб.";
        } else {
            var claim = distribObject.claims[0];
            elems = paymentObjectDetailsRows.filter(function (idx, elem) { return $(elem).data("idClaim") === claim.idClaim; });
            if (elems.length !== 0) elem = elems[0];
            else {
                elemOuter = $('<div class="text-danger rr-payment-object-detail" data-id-claim="' + claim.idClaim + '" data-payment-distrib-tenancy="0" data-payment-distrib-penalty="0">');
                paymentRow.find(".rr-payment-distribution-sum-details").append(elemOuter);
                elem = paymentRow.find(".rr-payment-distribution-sum-details .rr-payment-object-detail").last();
            }

            distributedTenancy = Math.round((parseFloat(($(elem).data("paymentDistribTenancy") + "").replace(",", ".")) + distribTenancySum) * 100) / 100;
            distributedPenalty = Math.round((parseFloat(($(elem).data("paymentDistribPenalty") + "").replace(",", ".")) + distribPenaltySum) * 100) / 100;
            $(elem).data("paymentDistribTenancy", distributedTenancy);
            $(elem).data("paymentDistribPenalty", distributedPenalty);

            html = "<u>ИР №" + claim.idClaim + "</u> <a class=\"btn oi oi-eye p-0 text-primary rr-payment-list-eye-btn\" target=\"_blank\" href=\"/Claims/Details?idClaim=" + claim.idClaim
                + "\"></a> <u>ЛС №" + claim.account + ":</u> <a class=\"btn oi oi-eye p-0 text-primary rr-payment-list-eye-btn\" target=\"_blank\" href=\"/KumiAccounts/Details?idAccount=" + claim.idAccount
                + "\"></a><br>найм " + distributePaymentFormatSum(distributedTenancy) + " руб., пени " + distributePaymentFormatSum(distributedPenalty) + " руб.";
        }
        $(elem).html(html);
    }
});