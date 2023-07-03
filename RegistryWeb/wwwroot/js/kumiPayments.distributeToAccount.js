﻿function parsePurpose(purpose) {
    var purposeInfo = {
        account: null,
        contract_num: null,
        court_order: null,
        address: null
    };
    var match = null;

    var courtOrderRegex = /(ИД)[ ]*([0-9][-][0-9]{1,6}[ ]?[\/][ ]?([0-9]{4}|[0-9]{2}))/gmiu;
    var courtOrderMatches = purpose.matchAll(courtOrderRegex);
    while ((match = courtOrderMatches.next()).done !== true) {
        purposeInfo.court_order = match.value[2];
    }
    var accountRegex = /ЛИЦ(\.?|ЕВОЙ)?[ ]+СЧЕТ[ ]*[:]?[ ]*([0-9]{6,})/gmiu;
    var accountMatches = purpose.matchAll(accountRegex);
    while ((match = accountMatches.next()).done !== true) {
        purposeInfo.account = match.value[2];
    }

    var addressRegex1 = /\/\/без НДС$/gmiu;
    if (addressRegex1.test(purpose)) {
        // Почта РФ
        var purposeParts = purpose.split('//');
        if (purposeParts.length >= 2) {
            var address = purposeParts[purposeParts.length - 2];
            purposeInfo.address = extractAddressFromString(address);
        }
    }

    return purposeInfo;
}

function extractAddressFromString(address) {
    var addressParts = $.trim(address).split('.');
    if (addressParts.length < 3) addressParts = $.trim(address).split(',');
    if (addressParts.length < 3) return null;
    var premise = addressParts[addressParts.length - 1];
    var house = addressParts[addressParts.length - 2];
    var street = null;
    var exclusion1 = /^\(ПУСТО\)\.(.+) Ж\.Р\.[ ]?СТЕНИХА/gmiu;
    var exclusion1Matches = address.matchAll(exclusion1);
    while ((match = exclusion1Matches.next()).done !== true) {
        street = $.trim(match.value[1]);
    }
    if (address.indexOf("К.МАРКСА") !== -1) {
        street = "К.МАРКСА";
    }
    if (street === null) {
        street = addressParts[addressParts.length - 3];
    }
    return {
        street: street.replace('XX ПАРТСЪЕЗДА', 'ХХ ПАРТСЪЕЗДА'),
        house: house,
        premise: premise
    };
}

function getIdStreetForStreetName(street, array) {
    for (var i = 0; i < array.length; i++) {
        optStreet = array[i];
        var optStreetParts = optStreet.street.split(',');
        var optStreetName = optStreetParts[optStreetParts.length - 1];
        optStreetName = optStreetName.replace('ул.', '').replace('пер.', '').replace('пр-кт.', '').replace('б-р.', '').replace('гск.', '').replace('туп.', '');
        optStreetName = $.trim(optStreetName).toUpperCase();
        if (optStreetName === street) return optStreet.idStreet;
    }
}

function isEmptyPurpose(purpose) {
    return purpose.account === null && purpose.contract_num === null && purpose.court_order === null && purpose.address === null;
}

function claimDeptPeriodsToStr(claim) {
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
    return deptPeriod;
}

function distributePaymentFormatSum(sum) {
    var sumParts = sum.toString().replace(".", ",").split(',');
    if (sumParts.length === 1) return sumParts[0] + ",00";
    return sumParts[0] + "," + sumParts[1].padEnd(2, '0');
}

var distributionModalOnSelectCallback = undefined;

function distributionModalInitiate(purpose, paymentSum, paymentSumPosted, idPayment, attachInsteadOfDistribute, onSelectCallback) {
    distributionModalOnSelectCallback = onSelectCallback;

    var modalForm = $("#DistributePaymentToAccountModalForm");
    modalForm.find("#DistributePaymentToAccount_IdPayment").val(idPayment);

    paymentSum = paymentSum + "";
    paymentSumPosted = paymentSumPosted + "";
    var sumForDistribution = parseFloat(paymentSum.replace(",", ".")) - parseFloat(paymentSumPosted.replace(",", "."));
    modalForm.find("#DistributePaymentToAccount_SumForDistribution").val(sumForDistribution);
    
    var purposeInfo = parsePurpose(purpose);

    var modal = $("#DistributePaymentToAccountModal");
    modal.find("input[type='text'], input[type='date'], select").prop("disabled", false);
    var purposeElem = modal.find("#DistrubutePaymentModalPurpose");
    purposeElem.text(purpose);
    purposeElem.attr("title", purpose);
    if (purposeInfo.account !== null) {
        modal.find("#DistributePaymentToAccount_Account").val(purposeInfo.account);
    }
    if (purposeInfo.contract_num !== null) {
        modal.find("#DistributePaymentToAccount_RegNumber").val(purposeInfo.contract_num);
    }
    if (purposeInfo.court_order !== null) {
        modal.find("#DistributePaymentToAccount_DistributeTo").val(1).change();
        modal.find("#DistributePaymentToAccount_ClaimCourtOrderNum").val(purposeInfo.court_order);
    }
    if (purposeInfo.address !== null) {
        var idStreet = getIdStreetForStreetName(purposeInfo.address.street,
            modal.find("#DistributePaymentToAccount_IdStreet option").map(function (idx, opt) {
                var street = $(opt).text();
                var idStreet = $(opt).attr("value");
                return { street, idStreet };
            }));
        modal.find("#DistributePaymentToAccount_IdStreet").val(idStreet).selectpicker('refresh');
        modal.find("#DistributePaymentToAccount_House").val(purposeInfo.address.house);
        modal.find("#DistributePaymentToAccount_PremisesNum").val(purposeInfo.address.premise);
    }

    if (attachInsteadOfDistribute) {
        modalForm.data("action", "attach");
        modalForm.find("#setDistributePaymentToAccountModalBtn").text("Выбрать");
    } else {
        modalForm.data("action", "distribute");
        modalForm.find("#setDistributePaymentToAccountModalBtn").text("Распределить");
    }

    modal.modal('show');
    if (!isEmptyPurpose(purposeInfo)) {
        modal.find("#searchDistributePaymentToAccountModalBtn").click();
    }
}

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
                
                var table = "";
                if (result.accounts !== undefined) {
                    table = buildAccountForDistribTable(result.accounts, result.count);
                } else
                if (result.claims !== undefined)
                {
                    table = buildClaimForDistribTable(result.claims, result.count);
                }
                div.html(table).closest('.form-row').removeClass("d-none");

                if (result.count === 1) {
                    var radio = $("#resultDistributePaymentToAccountModal table tbody td input[type='radio']");
                    radio.prop("checked", true);
                    radio.click();
                }

                $('#searchDistributePaymentToAccountModalBtn').text('Найти').attr('disabled', false);
            }
        });
        e.preventDefault();
    }

    function buildAccountForDistribTable(accounts, factCount) {
        var table = "<table class='table table-bordered mb-0 text-center'>";
        table += "<thead><tr><th rowspan='2'></th><th rowspan='2'>ЛС</th><th rowspan='2'>Посл. начисление</th><th colspan='2'>Текущее сальдо</th></tr>" +
            "<tr><th>Найм</th><th>Пени</th></tr></thead><tbody>";
        for (var i = 0; i < accounts.length; i++) {
            var account = accounts[i];
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


            table += "<tr data-account='" + accountNum + "' data-account-id-state='" + account.idState+"' data-account-state='" + state + "' data-last-charge-date='"
                + account.lastChargeDate + "' data-current-balance-tenancy='" + tenancy + "' data-current-balance-penalty='" + penalty + "'>";

            table += "<td style='vertical-align: middle'>" + radioButton + "</td>";
            table += "<td>" + accountNum
                + " <sup><span title='" + state + "' class='" + stateClass + "'><b>" + state.substr(0, 1) + "</b></span></sup>"
                + "<a class='btn oi oi-eye p-0 ml-1 text-primary rr-payment-list-eye-btn' href='/KumiAccounts/Details?idAccount=" + account.idAccount + "' target='_blank'></a>"
                + "</td><td>" + lastChargeDateStr + "</td><td>" + distributePaymentFormatSum(tenancy) + "</td><td>" + distributePaymentFormatSum(penalty) + "</td>";
            table += "</tr>";
        }
        if (accounts.length < factCount) {
            table += "<tr><td colspan='5' class='text-center'><i class='text-danger'>Всего найдено " + factCount + " совпадений. Уточните запрос</i></td></tr>";
        }
        if (factCount === 0) {
            table += "<tr><td colspan='5' class='text-center'><i class='text-danger'>Лицевые счета не найдены</i></td></tr>";
        }
        table += "</tbody></table>";
        return table;
    }

    function buildClaimForDistribTable(claims, factCount) {
        var table = "<table class='table table-bordered mb-0 text-center'>";
        table += "<thead><tr><th rowspan='2'></th><th rowspan='2'>ЛС</th><th colspan='2'>Текущее сальдо</th><th rowspan='2'>Период взыскания</th><th colspan='2'>Взыскиваемая сумма</th><th colspan='2'>Взысканная сумма</th></tr>" +
            "<tr><th>Найм</th><th>Пени</th><th>Найм</th><th>Пени</th><th>Найм</th><th>Пени</th></tr></thead><tbody>";
        for (var j = 0; j < claims.length; j++) {
            var claim = claims[j];
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

            var deptPeriod = claimDeptPeriodsToStr(claim);

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

            table += "<tr data-account='" + accountNum + "' data-account-state='" + claim.accountState + "' data-account-id-state='" + claim.idAccountState
                + "' data-id-account='" + claim.idAccount + "' data-account-current-balance-tenancy='" + claim.accountCurrentBalanceTenancy
                + "' data-account-current-balance-penalty='" + claim.accountCurrentBalancePenalty + "' data-claim-start-dept-period='" + claim.startDeptPeriod
                + "' data-claim-end-dept-period='" + claim.endDeptPeriod + "' data-claim-amount-tenancy='" + amountTenancy + "' data-claim-amount-penalty='" + amountPenalties
                + "' data-claim-amount-tenancy-recovered='" + amountTenancyRecovered + "'  data-claim-amount-penalty-recovered='" + amountPenaltiesRecovered+"'>";

            table += "<td style='vertical-align: middle'>" + radioButton + "</td>";
            table += "<td>" + accountNum
                + " <sup><span title='" + claim.accountState + "' class='" + stateClass + "'><b>" + claim.accountState.substr(0, 1) + "</b></span></sup>"
                + "<a class='btn oi oi-eye p-0 ml-1 text-primary rr-payment-list-eye-btn' href='/KumiAccounts/Details?idAccount=" + claim.idAccount + "' target='_blank'></a>"
                + "</td>"
                + "<td>" + distributePaymentFormatSum(claim.accountCurrentBalanceTenancy) + "</td><td>" + distributePaymentFormatSum(claim.accountCurrentBalancePenalty)
                + "</td><td>" + deptPeriod
                + "<a class='btn oi oi-eye p-0 ml-1 text-primary rr-payment-list-eye-btn' href='/Claims/Details?idClaim=" + claim.idClaim + "' target='_blank'></a>"
                + "</td><td>" + distributePaymentFormatSum(amountTenancy) + "</td><td>" + distributePaymentFormatSum(amountPenalties)
                + "</td><td>" + distributePaymentFormatSum(amountTenancyRecovered) + "</td><td>" + distributePaymentFormatSum(amountPenaltiesRecovered) + "</td>";
            table += "</tr>";
        }
        if (claims.length < factCount) {
            table += "<tr><td colspan='9' class='text-center'><i class='text-danger'>Всего найдено " + factCount + " совпадений. Уточните запрос</i></td></tr>";
        }
        if (factCount === 0) {
            table += "<tr><td colspan='9' class='text-center'><i class='text-danger'>Исковые работы не найдены</i></td></tr>";
        }
        table += "</tbody></table>";
        return table;
    }

    $("#resultDistributePaymentToAccountModal").on('click', "[name='DistributePaymentToAccount_IdObject'][type='radio']", function (e) {
        if ($(this).is(":checked")) {
            $("#setDistributePaymentToAccountModalBtn").attr('disabled', false);
            $("#DistributePaymentToAccountModalForm .rr-payment-distribute-sum").removeClass("d-none");
            var sumForDistribution = $("#DistributePaymentToAccountModalForm").find("#DistributePaymentToAccount_SumForDistribution").val();
            sumForDistribution = parseFloat(sumForDistribution);
            var objectType = $(this).data("objectType");
            var currentPenalty = 0;
            var currentTenancy = 0;
            var row = $(this).closest("tr");
            switch (objectType) {
                case 0:
                    currentPenalty = parseFloat($(row.find("td")[4]).text().replace(",", "."));
                    currentTenancy = parseFloat($(row.find("td")[3]).text().replace(",", "."));
                    break;
                case 1:
                    currentPenalty = parseFloat($(row.find("td")[6]).text().replace(",", "."));
                    currentPenalty -= parseFloat($(row.find("td")[8]).text().replace(",", "."));
                    currentTenancy = parseFloat($(row.find("td")[5]).text().replace(",", "."));
                    currentTenancy -= parseFloat($(row.find("td")[7]).text().replace(",", "."));
                    break;
            }
            var distributionTenancy = Math.round(Math.max(Math.min(sumForDistribution, Math.max(currentTenancy, 0)), 0) * 100) / 100;
            var distributionPenalty = Math.max(Math.min(Math.round((sumForDistribution - distributionTenancy) * 100) / 100, currentPenalty), 0);
            distributionTenancy = Math.round((sumForDistribution - distributionPenalty)*100)/100;
            $("#DistributePaymentToAccount_TenancySum").val((distributionTenancy + "").replace(".", ","));
            $("#DistributePaymentToAccount_PenaltySum").val((distributionPenalty + "").replace(".", ","));
        }
    });


    $("#setDistributePaymentToAccountModalBtn").on('click', distributePaymentToAccountModalSet);

    var action = $("#paymentsForm").attr('data-action');

    function distributePaymentToAccountModalSet(e) {
        var modal = $("#DistributePaymentToAccountModal");
        var data = {
            "IdPayment": modal.find("#DistributePaymentToAccount_IdPayment").val(),
            "IdObject": $("[name='DistributePaymentToAccount_IdObject']:checked").val(),
            "DistributeTo": $("[name='DistributePaymentToAccount_IdObject']:checked").data("objectType"),
            "TenancySum": modal.find("#DistributePaymentToAccount_TenancySum").val().replace(".", ","),
            "PenaltySum": modal.find("#DistributePaymentToAccount_PenaltySum").val().replace(".", ","),
            "Description": {}
        };
        var row = $("[name='DistributePaymentToAccount_IdObject']:checked").closest("tr");
        if (data.DistributeTo === 0) {
            data.Description.idAccount = data.IdObject;
            data.Description.account = row.data("account");
            data.Description.idState = row.data("accountIdState");
            data.Description.state = row.data("accountState");
            data.Description.currentBalanceTenancy = row.data("currentBalanceTenancy");
            data.Description.currentBalancePenalty = row.data("currentBalancePenalty");
            data.Description.lastChargeDate = row.data("lastChargeDate");
        } else {
            data.Description.idClaim = data.IdObject;
            data.Description.idAccount = row.data("idAccount");
            data.Description.account = row.data("account");
            data.Description.idAccountState = row.data("accountIdState");
            data.Description.accountState = row.data("accountState");
            data.Description.amountTenancy = row.data("claimAmountTenancy");
            data.Description.amountPenalties = row.data("claimAmountPenalty");
            data.Description.amountTenancyRecovered = row.data("claimAmountTenancyRecovered");
            data.Description.amountPenaltiesRecovered = row.data("claimAmountPenaltyRecovered");
            data.Description.accountCurrentBalanceTenancy = row.data("accountCurrentBalanceTenancy");
            data.Description.accountCurrentBalancePenalty = row.data("accountCurrentBalancePenalty");
            data.Description.startDeptPeriod = row.data("claimStartDeptPeriod");
            data.Description.endDeptPeriod = row.data("claimEndDeptPeriod");
        }
        $('#setDistributePaymentToAccountModalBtn').text('Сохраняем...').attr('disabled', true);

        if (distributionModalOnSelectCallback !== undefined) {
            distributionModalOnSelectCallback(data);
        }

        e.preventDefault();
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
        var paymentSum = $(this).data("paymentSum");
        var paymentSumPosted = $(this).data("paymentSumPosted");
        var purpose = undefined;
        if (action === undefined)
            purpose = $(this).closest("tr").find(".rr-payment-purpose").text();
        else
            purpose = $("#Purpose").text();

        distributionModalInitiate(purpose, paymentSum, paymentSumPosted, idPayment, false, distributePaymentToObjectOnSelectCallback);

        e.preventDefault();
    });

    function distributePaymentToObjectOnSelectCallback(distributeInfo) {
        var url = window.location.origin + '/KumiPayments/DistributePaymentToAccount';

        $.ajax({
            async: true,
            type: 'POST',
            url: url,
            data: distributeInfo,
            success: function (result) {
                if (result.state === "Error") {
                    var errorElem = $("#errorDistributePaymentToAccountModal");
                    errorElem.closest(".form-row").removeClass("d-none");
                    errorElem.html("<span class='text-danger'>" + result.error + "</span>");
                    $('#setDistributePaymentToAccountModalBtn').text("Распределить").attr('disabled', false);
                } else {
                    location.reload();
                }
            }
        });
    }

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