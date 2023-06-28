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
        if (isEmptyPurpose(purposeInfo) && paymentRows.length > 0) {
            notFoundDistribObjectByPurpose(paymentRow, "Назначение платежа не определено", "Выберите лицевой счет или ПИР для распределния платежа");
            getPaymentDistributionInfo(paymentRows.shift());
        } else {
            var idStreetElem = $("#KladrStreetsForSearchIdByName");
            var idStreet = null;
            if (purposeInfo.address !== null && purposeInfo.address.street !== null) {
                var foundedOptions = idStreetElem.find("option").filter(function (idx, elem) {
                    return $(elem).text() === purposeInfo.address.street;
                });
                if (foundedOptions.length > 0) {
                    idStreet = foundedOptions[0].attr("value");
                }
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
                        $(paymentRow).find(".rr-distribution-tenancy-sum").val((distributionTenancy + "").replace(".", ","));
                        $(paymentRow).find(".rr-distribution-penalty-sum").val((distributionPenalty + "").replace(".", ","));
                    }

                    if (paymentRows.length > 0) {
                        getPaymentDistributionInfo(paymentRows.shift());
                    }
                }
            });
        }
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

        var html = "<div><u>ЛС №" + account.account + ":</u>" +
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

        var html = "<div><u>ИР №" + claim.idClaim + "</u> " +
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
        // TODO отображение модального окна поиска ЛС\ПИР
        e.preventDefault();
    });

    $(".rr-payment-for-distribution").on("click", ".rr-control-change-object", function (e) {
        // TODO отображение модального окна поиска ЛС\ПИР
        e.preventDefault();
    });

    $(".rr-payment-for-distribution").on("click", ".rr-control-delete-object", function (e) {
        // TODO отображение окна подтверждения удаления связки с ЛС\ПИР
        e.preventDefault();
    });

    $(".rr-payment-for-distribution").on("click", ".rr-remove-payment-from-master", function (e) {
        // TODO отображение окна подтверждения удаления платежа из мастера
        e.preventDefault();
    });
});