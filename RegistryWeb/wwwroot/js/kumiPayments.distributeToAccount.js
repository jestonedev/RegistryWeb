$(function () {
    $("#DistributePaymentToAccount_IdRegion").on('change', distributePaymentToAccountModalChange);

    function distributePaymentToAccountModalChange(e) {
        var idRegion = $('#DistributePaymentToAccount_IdRegion').selectpicker('val');
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Address/GetKladrStreets',
            dataType: 'json',
            data: { idRegion },
            success: function (data) {
                var select = $('#DistributePaymentToAccount_IdStreet');
                select.selectpicker('destroy');
                select.find('option[value]').remove();
                $.each(data, function (i, d) {
                    select.append('<option value="' + d.idStreet + '">' + d.streetName + '</option>');
                });
                select.selectpicker();
            }
        });
        e.preventDefault();
    }

    $("#clearDistributePaymentToAccountModalBtn").on('click', distributePaymentToAccountModalClear);

    function distributePaymentToAccountModalClear(e) {
        distributePaymentToAccountModalClearResult();
        var idPayment = $("#DistributePaymentToAccount_IdPayment").val();
        var sumForDistribution = $("#DistributePaymentToAccount_SumForDistribution").val();
        resetModalForm($("#DistributePaymentToAccountModalForm"));
        $("#DistributePaymentToAccount_IdPayment").val(idPayment);
        $("#DistributePaymentToAccount_SumForDistribution").val(sumForDistribution);
        $("#DistributePaymentToAccount_DistributeTo").val(0).change().selectpicker('refresh');
        $("#DistributePaymentToAccount_IdRegion").change();
    }

    function distributePaymentToAccountModalClearResult() {
        $('#resultDistributePaymentToAccountModal').html('').closest('.form-row').addClass("d-none");
        $('#errorDistributePaymentToAccountModal').html('').closest('.form-row').addClass("d-none");
        $("#DistributePaymentToAccountModalForm .rr-payment-distribute-sum").addClass("d-none");
        $('#setDistributePaymentToAccountModalBtn').attr('disabled', true);
    }


    $("#searchDistributePaymentToAccountModalBtn").on('click', distributePaymentToAccountModalSearch);

    function distributePaymentToAccountModalSearch(e) {
        var div = $('#resultDistributePaymentToAccountModal');
        div.html("").closest('.form-row').addClass("d-none");
        $('#errorDistributePaymentToAccountModal').html('').closest('.form-row').addClass("d-none");
        $("#DistributePaymentToAccountModalForm .rr-payment-distribute-sum").addClass("d-none");
        $('#setDistributePaymentToAccountModalBtn').attr('disabled', true);
        $('#searchDistributePaymentToAccountModalBtn').text('Ищем...').attr('disabled', true);
        $.ajax({
            async: true,
            type: 'POST',
            url: window.location.origin + '/KumiPayments/GetDistributePaymentToObjects',
            data: {
                "FilterOptions.DistributeTo": $('#DistributePaymentToAccount_DistributeTo').selectpicker('val'),

                "FilterOptions.ClaimCourtOrderNum": $('#DistributePaymentToAccount_ClaimCourtOrderNum').val(),
                "FilterOptions.ClaimAtDate": $('#DistributePaymentToAccount_ClaimAtDate').val(),
                "FilterOptions.ClaimIdStateType": $('#DistributePaymentToAccount_ClaimIdStateType').selectpicker('val'),

                "FilterOptions.AccountGisZkh": $('#DistributePaymentToAccount_AccountGisZkh').val(),
                "FilterOptions.Account": $('#DistributePaymentToAccount_Account').val(),
                "FilterOptions.IdAccountState": $('#DistributePaymentToAccount_IdAccountState').selectpicker('val'),

                "FilterOptions.IdRegion": $('#DistributePaymentToAccount_IdRegion').selectpicker('val'),
                "FilterOptions.IdStreet": $('#DistributePaymentToAccount_IdStreet').selectpicker('val'),
                "FilterOptions.House": $('#DistributePaymentToAccount_House').val(),
                "FilterOptions.PremisesNum": $('#DistributePaymentToAccount_PremisesNum').val(),
                "FilterOptions.SubPremisesNum": $('#DistributePaymentToAccount_SubPremisesNum').val(),
                "FilterOptions.IdProcess": $('#DistributePaymentToAccount_IdProcess').val(),
                "FilterOptions.RegistrationNum": $('#DistributePaymentToAccount_RegNumber').val(),
                "FilterOptions.RegistrationDate": $('#DistributePaymentToAccount_RegDate').val(),
                "FilterOptions.IssuedDate": $('#DistributePaymentToAccount_IssueDate').val()
            },
            success: function (result) {
                
                var table = "<table class='table table-bordered mb-0 text-center'>";
                if (result.accounts !== undefined) {
                    table += "<thead><tr><th rowspan='2'></th><th rowspan='2'>ЛС</th><th rowspan='2'>Посл. начисление</th><th colspan='2'>Текущее сальдо</th></tr>" +
                        "<tr><th>Найм</th><th>Пени</th></tr></thead><tbody>";
                    for (var i = 0; i < result.accounts.length; i++) {
                        var account = result.accounts[i];
                        var idObject = account.idAccount;
                        var accountNum = account.account;
                        var state = account.state;
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
                        var stateClass = "";
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
                        var tenancy = account.currentBalanceTenancy;
                        var penalty = account.currentBalancePenalty;

                        var radioButton = "<div class='form-check'><input style='margin-top: -7px' name='DistributePaymentToAccount_IdObject' data-object-type='0' value='" + idObject + "' type='radio' class='form-check-input'></div>";


                        table += "<tr>";

                        table += "<td style='vertical-align: middle'>" + radioButton + "</td>";
                        table += "<td>" + accountNum
                            + " <sup><span title='" + state + "' class='" + stateClass + "'><b>" + state.substr(0, 1) + "</b></span></sup>"
                            + "<a class='btn oi oi-eye p-0 ml-1 text-primary rr-payment-list-eye-btn' href='/KumiAccounts/Details?idAccount=" + account.idAccount + "' target='_blank'></a>"
                            + "</td><td>" + lastChargeDateStr + "</td><td>" + distributePaymentFormatSum(tenancy) + "</td><td>" + distributePaymentFormatSum(penalty) + "</td>";
                        table += "</tr>";
                    }
                    if (result.accounts.length < result.count) {
                        table += "<tr><td colspan='5' class='text-center'><i class='text-danger'>Всего найдено " + result.count + " совпадений. Уточните запрос</i></td></tr>";
                    }
                    if (result.count === 0) {
                        table += "<tr><td colspan='5' class='text-center'><i class='text-danger'>Лицевые счета не найдены</i></td></tr>";
                    }
                    table += "</tbody></table>";
                } else
                if (result.claims !== undefined)
                {
                    table += "<thead><tr><th rowspan='2'></th><th rowspan='2'>ЛС</th><th rowspan='2'>Период взыскания</th><th colspan='2'>Взыскиваемая сумма</th><th colspan='2'>Взысканная сумма</th></tr>" +
                        "<tr><th>Найм</th><th>Пени</th><th>Найм</th><th>Пени</th></tr></thead><tbody>";
                    for (var j = 0; j < result.claims.length; j++) {
                        var claim = result.claims[j];
                        idObject = claim.idClaim;
                        accountNum = claim.account;
                        var amountTenancy = claim.amountTenancy;
                        if (amountTenancy === null) amountTenancy = 0;
                        var amountPenalties = claim.amountPenalties;
                        if (amountPenalties === null) amountPenalties = 0;
                        var amountTenancyRecovered = claim.amountTenancyRecovered;
                        if (amountTenancyRecovered === null) amountTenancyRecovered = 0;
                        var amountPenaltiesRecovered = claim.amountPenaltiesRecovered;
                        if (amountPenaltiesRecovered === null) amountPenaltiesRecovered = 0;
                        var startDeptPeriodStr = null;
                        if (claim.startDeptPeriod !== null) {
                            var startDeptPeriod = new Date(claim.startDeptPeriod);
                            year = startDeptPeriod.getFullYear();
                            month = startDeptPeriod.getMonth() + 1;
                            day = startDeptPeriod.getDate();
                            startDeptPeriodStr = (day < 10 ? "0" + day : day) + "." + (month < 10 ? "0" + month : month) + "." + year;
                        }

                        var endDeptPeriodStr = null;
                        if (claim.endDeptPeriod !== null) {
                            var endDeptPeriod = new Date(claim.endDeptPeriod);
                            year = endDeptPeriod.getFullYear();
                            month = endDeptPeriod.getMonth() + 1;
                            day = endDeptPeriod.getDate();
                            endDeptPeriodStr = (day < 10 ? "0" + day : day) + "." + (month < 10 ? "0" + month : month) + "." + year;
                        }

                        var deptPeriod = "";
                        if (startDeptPeriodStr !== null) {
                            deptPeriod = "с " + startDeptPeriodStr;
                        }
                        if (startDeptPeriodStr !== null && endDeptPeriodStr !== null) {
                            deptPeriod += " ";
                        }
                        if (endDeptPeriodStr !== null) {
                            deptPeriod += "по " + endDeptPeriodStr;
                        }

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

                        radioButton = "<div class='form-check'><input style='margin-top: -7px' name='DistributePaymentToAccount_IdObject' data-object-type='1' value='" + idObject + "' type='radio' class='form-check-input'></div>";

                        table += "<tr>";

                        table += "<td style='vertical-align: middle'>" + radioButton + "</td>";
                        table += "<td>" + accountNum
                            + " <sup><span title='" + claim.accountState + "' class='" + stateClass + "'><b>" + claim.accountState.substr(0, 1) + "</b></span></sup>"
                            + "<a class='btn oi oi-eye p-0 ml-1 text-primary rr-payment-list-eye-btn' href='/KumiAccounts/Details?idAccount=" + claim.idAccount + "' target='_blank'></a>"
                            + "</td><td>" + deptPeriod
                            + "<a class='btn oi oi-eye p-0 ml-1 text-primary rr-payment-list-eye-btn' href='/Claims/Details?idClaim=" + claim.idClaim + "' target='_blank'></a>"
                            + "</td><td>" + distributePaymentFormatSum(amountTenancy) + "</td><td>" + distributePaymentFormatSum(amountPenalties)
                            + "</td><td>" + distributePaymentFormatSum(amountTenancyRecovered) + "</td><td>" + distributePaymentFormatSum(amountPenaltiesRecovered) + "</td>";
                        table += "</tr>";
                    }
                    if (result.claims.length < result.count) {
                        table += "<tr><td colspan='7' class='text-center'><i class='text-danger'>Всего найдено " + result.count + " совпадений. Уточните запрос</i></td></tr>";
                    }
                    if (result.count === 0) {
                        table += "<tr><td colspan='7' class='text-center'><i class='text-danger'>Исковые работы не найдены</i></td></tr>";
                    }
                    table += "</tbody></table>";
                }
                div.html(table).closest('.form-row').removeClass("d-none");
                $('#searchDistributePaymentToAccountModalBtn').text('Найти').attr('disabled', false);
            }
        });
        e.preventDefault();
    }


    $("#resultDistributePaymentToAccountModal").on('click', "[name='DistributePaymentToAccount_IdObject'][type='radio']", function (e) {
        if ($(this).is(":checked")) {
            $("#setDistributePaymentToAccountModalBtn").attr('disabled', false);
            $("#DistributePaymentToAccountModalForm .rr-payment-distribute-sum").removeClass("d-none");
            var sumForDistribution = $("#DistributePaymentToAccountModalForm").find("#DistributePaymentToAccount_SumForDistribution").val();
            sumForDistribution = parseFloat(sumForDistribution);
            var objectType = $(this).data("objectType");
            var currentPenalty = 0;
            var row = $(this).closest("tr");
            switch (objectType) {
                case 0:
                    currentPenalty = parseFloat($(row.find("td")[4]).text().replace(",", "."));
                    break;
                case 1:
                    currentPenalty = parseFloat($(row.find("td")[4]).text().replace(",", "."));
                    currentPenalty -= parseFloat($(row.find("td")[6]).text().replace(",", "."));
                    break;
            }
            var distributionPenalty = Math.round(Math.max(Math.min(sumForDistribution, currentPenalty), 0)*100)/100;
            var distributionTenancy = Math.round((sumForDistribution - distributionPenalty)*100)/100;
            $("#DistributePaymentToAccount_TenancySum").val((distributionTenancy + "").replace(".", ","));
            $("#DistributePaymentToAccount_PenaltySum").val((distributionPenalty + "").replace(".", ","));
        }
    });


    $("#setDistributePaymentToAccountModalBtn").on('click', distributePaymentToAccountModalSet);

    var action = $("#paymentsForm").attr('data-action');

    function distributePaymentToAccountModalSet(e) {
        var modal = $("#DistributePaymentToAccountModal");
        $('#setDistributePaymentToAccountModalBtn').text('Сохраняем...').attr('disabled', true);
        var data = {
            "IdPayment": modal.find("#DistributePaymentToAccount_IdPayment").val(),
            "IdObject": $("[name='DistributePaymentToAccount_IdObject']:checked").val(),
            "DistributeTo": $("[name='DistributePaymentToAccount_IdObject']:checked").data("objectType"),
            "TenancySum": modal.find("#DistributePaymentToAccount_TenancySum").val().replace(".", ","),
            "PenaltySum": modal.find("#DistributePaymentToAccount_PenaltySum").val().replace(".", ",")
        };
        var url = window.location.origin + '/KumiPayments/DistributePaymentToAccount';
        
        $.ajax({
            async: true,
            type: 'POST',
            url: url,
            data: data,
            success: function (result) {
                if (result.state === "Error") {
                    var errorElem = $("#errorDistributePaymentToAccountModal");
                    errorElem.closest(".form-row").removeClass("d-none");
                    errorElem.html("<span class='text-danger'>" + result.error + "</span>");
                } else {
                    if (action !== undefined || $("#FilterOptions_IdAccount").val() !== "" || $("#FilterOptions_IdClaim").val() !== "")
                        location.reload();
                    else {
                        var index = $("#DistributePaymentToAccountModal").data("index");
                        var tr = $($(".rr-payments-table tbody tr")[index]);
                        var bell = tr.find(".rr-payment-bell");
                        var sumPosted = result.distrubutedToTenancySum + result.distrubutedToPenaltySum;

                        updatePaymentBell(bell, sumPosted, result.sum);
                        updatePaymentTrState(tr, sumPosted, result.sum);
                    }
                    $("#DistributePaymentToAccountModal").modal('hide');
                }
                $('#setDistributePaymentToAccountModalBtn').text("Распределить").attr('disabled', false);
            }
        });

        e.preventDefault();
    }

    function distributePaymentFormatSum(sum) {
        var sumParts = sum.toString().replace(".", ",").split(',');
        if (sumParts.length === 1) return sumParts[0] + ",00";
        return sumParts[0] + "," + sumParts[1].padEnd(2, '0');
    }

    $("#DistributePaymentToAccount_TenancySum, #DistributePaymentToAccount_PenaltySum").on("change", function () {
        var sumForDistribution = $("#DistributePaymentToAccountModalForm").find("#DistributePaymentToAccount_SumForDistribution").val();
        sumForDistribution = parseFloat(sumForDistribution);
        var value = parseFloat($(this).val().replace(",", "."));
        if (isNaN(value)) value = 0;
        value = Math.min(sumForDistribution, value);
        $(this).val((value + "").replace(".", ","));
        var diffSum = Math.round((sumForDistribution - value)*100) / 100;
        if ($(this).attr("id") === "DistributePaymentToAccount_TenancySum") {
            var penalty = parseFloat($("#DistributePaymentToAccount_PenaltySum").val().replace(",", "."));
            if (penalty > diffSum)
                $("#DistributePaymentToAccount_PenaltySum").val((diffSum + "").replace(".", ","));
        }
        if ($(this).attr("id") === "DistributePaymentToAccount_PenaltySum") {
            var tenancy = parseFloat($("#DistributePaymentToAccount_TenancySum").val().replace(",", "."));
            if (tenancy > diffSum)
                $("#DistributePaymentToAccount_TenancySum").val((diffSum + "").replace(".", ","));
        }
    });

    $("#DistributePaymentToAccount_DistributeTo").on("change", function (e) {
        distributePaymentToAccountModalClearResult();
        var val = $(this).val() | 0;
        var claimFilterBlock = $("#DistributePaymentToAccountModalForm .rr-distrib-modal-claims-filter");
        var titleElem = $("#DistributePaymentToAccountModal .modal-title");
        if (val === 0) {
            claimFilterBlock.find("input, select").val("");
            claimFilterBlock.find("select").selectpicker("refresh");
            claimFilterBlock.hide();
            titleElem.text("Поиск лицевого счета");
        } else {
            claimFilterBlock.show();
            titleElem.text("Поиск исковой работы");
        }
    });

    $("#DistributePaymentToAccount_DistributeTo").change();

    $(".rr-distribute-payment").on("click", function (e) {
        if (action === undefined) {
            $("#DistributePaymentToAccountModal").data("index", $(this).closest("tr").index());
        }
        var idPayment = $(this).data("idPayment");
        $("#DistributePaymentToAccountModalForm").find("#DistributePaymentToAccount_IdPayment").val(idPayment);

        var paymentSum = $(this).data("paymentSum") + "";
        var paymentSumPosted = $(this).data("paymentSumPosted") + "";
        var sumForDistribution = parseFloat(paymentSum.replace(",", ".")) - parseFloat(paymentSumPosted.replace(",", "."));
        $("#DistributePaymentToAccountModalForm").find("#DistributePaymentToAccount_SumForDistribution").val(sumForDistribution);

        var modal = $("#DistributePaymentToAccountModal");
        modal.find("input[type='text'], input[type='date'], select").prop("disabled", false);
        modal.modal('show');
        e.preventDefault();
    });

    $("#DistributePaymentToAccountModal").on("hide.bs.modal", function (e) {
        distributePaymentToAccountModalClear(e);
    });

    $(".rr-cancel-distribute-payment").on("click", function (e) {
        var modal = $("#CancelDistributePaymentToAccountModal");

        if (action === undefined) {
            modal.data("index", $(this).closest("tr").index());
        }
        var idPayment = $(this).data("idPayment");
        $("#CancelDistributePaymentToAccountModalForm").find("#CancelDistributePaymentToAccount_IdPayment").val(idPayment);

        var paymentSum = $(this).data("paymentSum");
        var paymentSumPosted = $(this).data("paymentSumPosted");
        var paymentTitle = $(this).data("paymentTitle");

        modal.find(".rr-payment-sum").text(distributePaymentFormatSum(paymentSum));
        modal.find(".rr-payment-sum-posted").text(distributePaymentFormatSum(paymentSumPosted));
        modal.find(".rr-payment-requisits").text(paymentTitle);

        modal.modal('show');
        e.preventDefault();
    });

    $("#CancelDistributePaymentToAccountModal").on("shown.bs.modal", function (e) {
        var modal = $("#CancelDistributePaymentToAccountModal");

        var data = {
            "IdPayment": modal.find("#CancelDistributePaymentToAccount_IdPayment").val()
        };
        var url = window.location.origin + '/KumiPayments/DistributePaymentDetails';

        $.ajax({
            async: true,
            type: 'POST',
            url: url,
            data: data,
            success: function (result) {
                modal.find(".rr-payment-distribution-details").html(result).removeClass("d-none");
                modal.find(".rr-payment-distribution-details-loader").addClass("d-none");
                modal.find(".rr-distribute-check-td").removeClass("d-none");
            }
        });
    });

    $("#CancelDistributePaymentToAccountModal").on("hidden.bs.modal", function (e) {
        var modal = $("#CancelDistributePaymentToAccountModal");
        modal.find(".rr-payment-distribution-details").html("").addClass("d-none");
        modal.find(".rr-payment-distribution-details-loader").removeClass("d-none");
    });

    $("#cancelDistributePaymentToAccountModalBtn").on('click', cancelDistributePaymentToAccountModal);

    function cancelDistributePaymentToAccountModal(e) {
        var modal = $("#CancelDistributePaymentToAccountModal");
        $('#cancelDistributePaymentToAccountModalBtn').text('Отменяем...').attr('disabled', true);
        var data = {
            "IdPayment": modal.find("#CancelDistributePaymentToAccount_IdPayment").val(),
            "IdClaims": modal.find("[name='Distrubution.IdClaim']:checked").map(function (idx, elem) { return $(elem).val() | 0; }).toArray(),
            "IdAccounts": modal.find("[name='Distrubution.IdAccount']:checked").map(function (idx, elem) { return $(elem).val() | 0; }).toArray()
        };
        var url = window.location.origin + '/KumiPayments/CancelDistributePaymentToAccount';

        $.ajax({
            async: true,
            type: 'POST',
            url: url,
            data: data,
            success: function (result) {
                if (result.state === "Error") {
                    var errorElem = $("#errorCanceDistributePaymentToAccountModal");
                    errorElem.closest(".form-row").removeClass("d-none");
                    errorElem.html("<span class='text-danger'>" + result.error + "</span>");
                } else {
                    location.reload();
                    modal.modal('hide');
                }
                $('#cancelDistributePaymentToAccountModalBtn').text("Отменить распределение").attr('disabled', false);
            }
        });

        e.preventDefault();
    }

    function updatePaymentBell(bellElem, sumPosted, sumPayment) {
        bellElem.removeClass("text-danger").removeClass("text-warning").addClass("d-none");
        if (sumPosted > sumPayment) {
            bellElem.removeClass("d-none")
                .addClass("text-danger").attr("title", "Распределеная сумма превышает фактическую по платежу");
        } else
            if (sumPosted > 0 && sumPayment > sumPosted) {
                bellElem.removeClass("d-none")
                    .addClass("text-warning").attr("title", "Платеж распределен не полностью");
            } else
                if (sumPosted === 0 && sumPayment !== 0) {
                    bellElem.removeClass("d-none")
                        .addClass("text-warning").attr("title", "Платеж не распределен");
                }
    }

    function updatePaymentTrState(tr, sumPosted, sumPayment) {
        tr.find(".rr-distribute-payment").data("paymentSumPosted", distributePaymentFormatSum(sumPosted));
        if (sumPayment <= sumPosted) {
            tr.find(".rr-distribute-payment").addClass("d-none");
        } else {
            tr.find(".rr-distribute-payment").removeClass("d-none");
        }
        if (sumPosted > 0) {
            tr.find(".rr-cancel-distribute-payment").removeClass("d-none");
            tr.find(".rr-apply-memorial-order").addClass("d-none");
        } else {
            tr.find(".rr-cancel-distribute-payment").addClass("d-none");
            tr.find(".rr-apply-memorial-order").removeClass("d-none");
        }

        var paymentSumElem = tr.find(".rr-payment-sum");
        paymentSumElem.text(distributePaymentFormatSum(sumPayment) + " руб." + (sumPosted > 0 ? ", расп.: " + distributePaymentFormatSum(sumPosted) + " руб": ""));
    }

    $("#DistributePaymentToAccountTenancyLeftovers").on("click", function (e) {
        var sumForDistribute = parseFloat($("#DistributePaymentToAccount_SumForDistribution").val().replace(",", "."));
        var penaltySum = parseFloat($("#DistributePaymentToAccount_PenaltySum").val().replace(",", "."));
        var tenancySum = Math.round(Math.max(0, sumForDistribute - penaltySum)*100)/100;
        $("#DistributePaymentToAccount_TenancySum").val((tenancySum + "").replace(".", ","));
        e.preventDefault();
    });

    $("#DistributePaymentToAccountPenaltyLeftovers").on("click", function (e) {
        var sumForDistribute = parseFloat($("#DistributePaymentToAccount_SumForDistribution").val().replace(",", "."));
        var tenancySum = parseFloat($("#DistributePaymentToAccount_TenancySum").val().replace(",", "."));
        var penaltySum = Math.round(Math.max(0, sumForDistribute - tenancySum) * 100) / 100;
        $("#DistributePaymentToAccount_PenaltySum").val((penaltySum + "").replace(".", ","));
        e.preventDefault();
    });
});