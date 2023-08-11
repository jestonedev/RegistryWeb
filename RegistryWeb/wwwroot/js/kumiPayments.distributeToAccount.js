function parsePurpose(purpose) {
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
    var accountRegex = /(ЛИЦ(\.?|ЕВОЙ)?[ ]+СЧЕТ|ЛС)[ ]*[:]?[ ]*([0-9]{6,})/gmiu;
    var accountMatches = purpose.matchAll(accountRegex);
    while ((match = accountMatches.next()).done !== true) {
        purposeInfo.account = match.value[3];
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
    var addressRegex2 = /^[а-яА-Я ]+;([^;]+)$/gmiu;
    if (addressRegex2.test(purpose)) {
        // ВТБ
        purposeParts = purpose.split(';');
        if (purposeParts.length === 2) {
            address = purposeParts[1];
            purposeInfo.address = extractAddressFromString(address);
        }
    }
    return purposeInfo;
}

function extractAddressFromString(address) {
    var addressParts = $.trim(address.replace(", ул. ", ".").replace(", д. ", ".").replace(", кв. ", ".")).split('.');
    if (addressParts.length < 3) addressParts = $.trim(address).split('.');
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
        street = $.trim(addressParts[addressParts.length - 3]);
    }
    if (street.endsWith(" ПЕР"))
        street = street.replace(" ПЕР", "");
    if (street.endsWith(" ПР-КТ"))
        street = street.replace(" ПР-КТ", "");
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
        if (optStreetName === street.toUpperCase()) return optStreet.idStreet;
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
    if (sum === null || sum === undefined) return "0,00";
    var sumParts = sum.toString().replace(".", ",").split(',');
    if (sumParts.length === 1) return sumParts[0] + ",00";
    return sumParts[0] + "," + sumParts[1].padEnd(2, '0');
}

var distributionModalOnSelectCallback = undefined;

function distributionModalInitiate(purpose, account, paymentSum, paymentSumPosted, idPayment, attachInsteadOfDistribute, onSelectCallback) {
    distributionModalOnSelectCallback = onSelectCallback;

    var modalForm = $("#DistributePaymentToAccountModalForm");
    modalForm.find("#DistributePaymentToAccount_IdPayment").val(idPayment);

    paymentSum = paymentSum + "";
    paymentSumPosted = paymentSumPosted + "";
    var sumForDistribution = parseFloat(paymentSum.replace(",", ".")) - parseFloat(paymentSumPosted.replace(",", "."));
    modalForm.find("#DistributePaymentToAccount_SumForDistribution").val(sumForDistribution);
    
    var purposeInfo = parsePurpose(purpose);

    if (purposeInfo.account === null)
        purposeInfo.account = account;

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

function redistributePayments(sums, distributeToIndex, sumForDistrubution) {
    do {
        var totalSum = 0;
        for (var i = 0; i < sums.length; i++) {
            if (isNaN(sums[i])) {
                sums[i] = 0;
            }
            totalSum += sums[i];
        }
        if (totalSum > sumForDistrubution) {
            var notNullIndex = sums.length - 1;
            while (sums[notNullIndex] <= 0 || notNullIndex === distributeToIndex) {
                notNullIndex -= 1;
                if (notNullIndex === 0) break;
            }
            var diffSum = totalSum - sumForDistrubution;
            var subSum = Math.min(diffSum, sums[notNullIndex]);
            sums[notNullIndex] = Math.round((sums[notNullIndex] - subSum) * 100) / 100;
            totalSum -= subSum;
        }
    } while (totalSum > sumForDistrubution && notNullIndex !== 0);

    if (totalSum > sumForDistrubution) {
        sums[distributeToIndex] = sumForDistrubution;
    }
    return sums;
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
        var hasDgi = $(accounts).filter(function (idx, elem) { return elem.currentBalanceDgi > 0; }).length > 0;
        var hasPkk = $(accounts).filter(function (idx, elem) { return elem.currentBalancePkk > 0; }).length > 0;
        var hasPadun = $(accounts).filter(function (idx, elem) { return elem.currentBalancePadun > 0; }).length > 0;
        var additionalCounts = 0;
        additionalCounts += hasDgi ? 1 : 0;
        additionalCounts += hasPkk ? 1 : 0;
        additionalCounts += hasPadun ? 1 : 0;
        var head = "<thead><tr><th rowspan='2'></th><th rowspan='2'>ЛС</th><th rowspan='2'>Посл. начисление</th>";
        head += "<th colspan='" + (2 + additionalCounts) + "'>Текущее сальдо</th></tr><tr><th>Найм</th><th>Пени</th>";
        if (hasDgi) head += "<th>ДГИ</th>";
        if (hasPkk) head += "<th>ПКК</th>";
        if (hasPadun) head += "<th>Падун</th>";
        head += "</tr></thead>";
        table += head + "<tbody>";
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
            var dgi = account.currentBalanceDgi;
            var pkk = account.currentBalancePkk;
            var padun = account.currentBalancePadun;

            var radioButton = "<div class='form-check'><input style='margin-top: -7px' name='DistributePaymentToAccount_IdObject' data-object-type='0' value='" + idObject + "' type='radio' class='form-check-input'></div>";


            table += "<tr data-account='" + accountNum + "' data-account-id-state='" + account.idState+"' data-account-state='" + state + "' data-last-charge-date='"
                + account.lastChargeDate + "' data-current-balance-tenancy='" + tenancy + "' data-current-balance-penalty='" + penalty
                + "' data-account-tenant='" + account.tenant + "' data-current-balance-exclude-charge='" + account.currentBalanceExcludeCharge
                + "' data-current-balance-dgi='" + dgi + "' data-current-balance-pkk='" + pkk + "' data-current-balance-padun='" + padun + "'>";

            table += "<td style='vertical-align: middle'>" + radioButton + "</td>";
            table += "<td>" + accountNum
                + " <sup><span title='" + state + "' class='" + stateClass + "'><b>" + state.substr(0, 1) + "</b></span></sup>"
                + "<a class='btn oi oi-eye p-0 ml-1 text-primary rr-payment-list-eye-btn' href='/KumiAccounts/Details?idAccount=" + account.idAccount + "' target='_blank'></a>"
                + "</td><td>" + lastChargeDateStr + "</td><td>"
                + (account.currentBalanceExcludeCharge ? " <sup style='visibility: hidden'>*</sup> " : "") + distributePaymentFormatSum(tenancy) + (account.currentBalanceExcludeCharge ? " <sup class='text-danger' title='За вычетом начисления текущего периода'>*</sup>" : "") + "</td><td>"
                + (account.currentBalanceExcludeCharge ? " <sup style='visibility: hidden'>*</sup> " : "") + distributePaymentFormatSum(penalty) + (account.currentBalanceExcludeCharge ? " <sup class='text-danger' title='За вычетом начисления текущего периода'>*</sup>" : "") + "</td>";
            if (hasDgi) table += "<td>" + (account.currentBalanceExcludeCharge ? " <sup style='visibility: hidden'>*</sup> " : "") + distributePaymentFormatSum(dgi) + (account.currentBalanceExcludeCharge ? " <sup class='text-danger' title='За вычетом начисления текущего периода'>*</sup>" : "") + "</td>";
            if (hasPkk) table += "<td>" + (account.currentBalanceExcludeCharge ? " <sup style='visibility: hidden'>*</sup> " : "") + distributePaymentFormatSum(pkk) + (account.currentBalanceExcludeCharge ? " <sup class='text-danger' title='За вычетом начисления текущего периода'>*</sup>" : "") + "</td>";
            if (hasPadun) table += "<td>" + (account.currentBalanceExcludeCharge ? " <sup style='visibility: hidden'>*</sup> " : "") + distributePaymentFormatSum(padun) + (account.currentBalanceExcludeCharge ? " <sup class='text-danger' title='За вычетом начисления текущего периода'>*</sup>" : "") + "</td>";
            table += "</tr>";
        }
        if (accounts.length < factCount) {

            table += "<tr><td colspan='" + (5 + additionalCounts) + "' class='text-center'><i class='text-danger'>Всего найдено " + factCount + " совпадений. Уточните запрос</i></td></tr>";
        }
        if (factCount === 0) {
            table += "<tr><td colspan='" + (5 + additionalCounts) + "' class='text-center'><i class='text-danger'>Лицевые счета не найдены</i></td></tr>";
        }
        table += "</tbody></table>";
        return table;
    }

    function buildClaimForDistribTable(claims, factCount) {
        var hasDgi = $(claims).filter(function (idx, elem) { return elem.accountCurrentBalanceDgi > 0 || elem.amountDgi > 0 || elem.amountDgiRecovered > 0; }).length > 0;
        var hasPkk = $(claims).filter(function (idx, elem) { return elem.accountCurrentBalancePkk > 0 || elem.amountPkk > 0 || elem.amountPkkRecovered > 0; }).length > 0;
        var hasPadun = $(claims).filter(function (idx, elem) { return elem.accountCurrentBalancePadun || elem.amountPadun > 0 || elem.amountPadunRecovered > 0 > 0; }).length > 0;
        var additionalCounts = 0;
        additionalCounts += hasDgi ? 1 : 0;
        additionalCounts += hasPkk ? 1 : 0;
        additionalCounts += hasPadun ? 1 : 0;

        var table = "<table class='table table-bordered mb-0 text-center'>";
        var head = "<thead><tr><th rowspan='2'></th><th rowspan='2'>ЛС</th><th colspan='" + (2 + additionalCounts) + "'>Текущее сальдо</th><th rowspan='2'>ПИР</th><th rowspan='2'>Сумма</th><th rowspan='2'>Найм</th><th rowspan='2'>Пени</th>";
        if (hasDgi) head += "<th rowspan='2'>ДГИ</th>";
        if (hasPkk) head += "<th rowspan='2'>ПКК</th>";
        if (hasPadun) head += "<th rowspan='2'>Падун</th>";
        head += "</tr><tr>";
        head += "<th>Найм</th><th>Пени</th>";
        if (hasDgi) head += "<th>ДГИ</th>";
        if (hasPkk) head += "<th>ПКК</th>";
        if (hasPadun) head += "<th>Падун</th>";
        head += "</tr></thead>";
        table += head + "<tbody>";

        for (var j = 0; j < claims.length; j++) {
            var claim = claims[j];
            idObject = claim.idClaim;
            accountNum = claim.account;
            var amountTenancy = claim.amountTenancy;
            if (amountTenancy === null) amountTenancy = 0;
            var amountPenalties = claim.amountPenalties;
            if (amountPenalties === null) amountPenalties = 0;
            var amountDgi = claim.amountDgi;
            if (amountDgi === null) amountDgi = 0;
            var amountPkk = claim.amountPkk;
            if (amountPkk === null) amountPkk = 0;
            var amountPadun = claim.amountPadun;
            if (amountPadun === null) amountPadun = 0;

            var amountTenancyRecovered = claim.amountTenancyRecovered;
            if (amountTenancyRecovered === null) amountTenancyRecovered = 0;
            var amountPenaltiesRecovered = claim.amountPenaltiesRecovered;
            if (amountPenaltiesRecovered === null) amountPenaltiesRecovered = 0;
            var amountDgiRecovered = claim.amountDgiRecovered;
            if (amountDgiRecovered === null) amountDgiRecovered = 0;
            var amountPkkRecovered = claim.amountPkkRecovered;
            if (amountPkkRecovered === null) amountPkkRecovered = 0;
            var amountPadunRecovered = claim.amountPadunRecovered;
            if (amountPadunRecovered === null) amountPadunRecovered = 0;
            var accountCurrentBalanceTenancy = claim.accountCurrentBalanceTenancy;
            if (accountCurrentBalanceTenancy === null) accountCurrentBalanceTenancy = 0;
            var accountCurrentBalancePenalty = claim.accountCurrentBalancePenalty;
            if (accountCurrentBalancePenalty === null) accountCurrentBalancePenalty = 0;
            var accountCurrentBalanceDgi = claim.accountCurrentBalanceDgi;
            if (accountCurrentBalanceDgi === null) accountCurrentBalanceDgi = 0;
            var accountCurrentBalancePkk = claim.accountCurrentBalancePkk;
            if (accountCurrentBalancePkk === null) accountCurrentBalancePkk = 0;
            var accountCurrentBalancePadun = claim.accountCurrentBalancePadun;
            if (accountCurrentBalancePadun === null) accountCurrentBalancePadun = 0;

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
                + "' data-id-account='" + claim.idAccount + "' data-account-current-balance-tenancy='" + accountCurrentBalanceTenancy
                + "' data-account-current-balance-penalty='" + accountCurrentBalancePenalty + "' data-account-current-balance-dgi='" + accountCurrentBalanceDgi
                + "' data-account-current-balance-pkk='" + accountCurrentBalancePkk + "' data-account-current-balance-padun='" + accountCurrentBalancePadun
                + "' data-account-current-balance-exclude-charge='" + claim.accountCurrentBalanceExcludeCharge
                + "' data-claim-court-order-num='" + claim.courtOrderNum + "' data-claim-tenant='" + claim.tenant
                + "' data-claim-start-dept-period='" + claim.startDeptPeriod
                + "' data-claim-end-dept-period='" + claim.endDeptPeriod
                + "' data-claim-amount-tenancy='" + amountTenancy + "' data-claim-amount-penalty='" + amountPenalties
                + "' data-claim-amount-dgi='" + amountDgi + "' data-claim-amount-pkk='" + amountPkk
                + "' data-claim-amount-padun='" + amountPadun
                + "' data-claim-amount-tenancy-recovered='" + amountTenancyRecovered + "'  data-claim-amount-penalty-recovered='" + amountPenaltiesRecovered
                + "' data-claim-amount-dgi-recovered='" + amountDgiRecovered + "'  data-claim-amount-pkk-recovered='" + amountPkkRecovered
                + "' data-claim-amount-padun-recovered='" + amountPadunRecovered+ "'>";

            table += "<td rowspan='2' style='vertical-align: middle'>" + radioButton + "</td>";
            table += "<td rowspan='2' style='vertical-align: middle'>" + accountNum
                + " <sup><span title='" + claim.accountState + "' class='" + stateClass + "'><b>" + claim.accountState.substr(0, 1) + "</b></span></sup>"
                + "<a class='btn oi oi-eye p-0 ml-1 text-primary rr-payment-list-eye-btn' href='/KumiAccounts/Details?idAccount=" + claim.idAccount + "' target='_blank'></a>"
                + "</td>"
                + "<td rowspan='2' style='vertical-align: middle'>" + (claim.accountCurrentBalanceExcludeCharge ? " <sup style='visibility: hidden'>*</sup> " : "") + distributePaymentFormatSum(claim.accountCurrentBalanceTenancy) + (claim.accountCurrentBalanceExcludeCharge ? " <sup class='text-danger' title='За вычетом начисления текущего периода'>*</sup>" : "")
                + "</td><td rowspan='2' style='vertical-align: middle'>" + (claim.accountCurrentBalanceExcludeCharge ? " <sup style='visibility: hidden'>*</sup> " : "") + distributePaymentFormatSum(claim.accountCurrentBalancePenalty) + (claim.accountCurrentBalanceExcludeCharge ? " <sup class='text-danger' title='За вычетом начисления текущего периода'>*</sup>" : "") + "</td>"
                + (hasDgi ? "<td rowspan='2' style='vertical-align: middle'>" + (claim.accountCurrentBalanceExcludeCharge ? " <sup style='visibility: hidden'>*</sup> " : "") + distributePaymentFormatSum(claim.accountCurrentBalanceDgi) + (claim.accountCurrentBalanceExcludeCharge ? " <sup class='text-danger' title='За вычетом начисления текущего периода'>*</sup>" : "") + "</td>" : "")
                + (hasPkk ? "<td rowspan='2' style='vertical-align: middle'>" + (claim.accountCurrentBalanceExcludeCharge ? " <sup style='visibility: hidden'>*</sup> " : "") + distributePaymentFormatSum(claim.accountCurrentBalancePkk) + (claim.accountCurrentBalanceExcludeCharge ? " <sup class='text-danger' title='За вычетом начисления текущего периода'>*</sup>" : "") + "</td>" : "")
                + (hasPadun ? "<td rowspan='2' style='vertical-align: middle'>" + (claim.accountCurrentBalanceExcludeCharge ? " <sup style='visibility: hidden'>*</sup> " : "") + distributePaymentFormatSum(claim.accountCurrentBalancePadun) + (claim.accountCurrentBalanceExcludeCharge ? " <sup class='text-danger' title='За вычетом начисления текущего периода'>*</sup>" : "") + "</td>" : "")
                + "<td rowspan='2' style='vertical-align: middle'>" + (claim.courtOrderNum != null ? "с\\п " + claim.courtOrderNum + "<br/>" : "") + deptPeriod
                + "<a class='btn oi oi-eye p-0 ml-1 text-primary rr-payment-list-eye-btn' href='/Claims/Details?idClaim=" + claim.idClaim + "' target='_blank'></a>"
                + "</td><td>Взыскиваемая</td><td>" + distributePaymentFormatSum(amountTenancy) + "</td><td>" + distributePaymentFormatSum(amountPenalties) + "</td>"
                + (hasDgi ? "<td>" + distributePaymentFormatSum(amountDgi) + "</td>" : "")
                + (hasPkk ? "<td>" + distributePaymentFormatSum(amountPkk) + "</td>" : "")
                + (hasPadun ? "<td>" + distributePaymentFormatSum(amountPadun) + "</td>" : "") + "</tr>";
            table += "<tr><td>Взысканная</td><td>" + distributePaymentFormatSum(amountTenancyRecovered) + "</td><td>" + distributePaymentFormatSum(amountPenaltiesRecovered) + "</td>"
                + (hasDgi ? "<td>" + distributePaymentFormatSum(amountDgiRecovered) + "</td>" : "")
                + (hasPkk ? "<td>" + distributePaymentFormatSum(amountPkkRecovered) + "</td>" : "")
                + (hasPadun ? "<td>" + distributePaymentFormatSum(amountPadunRecovered) + "</td>" : "");
            table += "</tr>";
        }
        if (claims.length < factCount) {
            table += "<tr><td colspan='" + (8+additionalCounts) + "' class='text-center'><i class='text-danger'>Всего найдено " + factCount + " совпадений. Уточните запрос</i></td></tr>";
        }
        if (factCount === 0) {
            table += "<tr><td colspan='" + (8 + additionalCounts) + "' class='text-center'><i class='text-danger'>Исковые работы не найдены</i></td></tr>";
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
            var hasDgi = false;
            var hasPkk = false;
            var hasPadun = false;
            var row = $(this).closest("tr");
            var tenancyDistribElemWrapper = $("#DistributePaymentToAccountModalForm .rr-payment-distribute-sum #DistributePaymentToAccount_TenancySum").closest(".form-group");
            var penaltyDistribElemWrapper = $("#DistributePaymentToAccountModalForm .rr-payment-distribute-sum #DistributePaymentToAccount_PenaltySum").closest(".form-group");
            var dgiDistribElemWrapper = $("#DistributePaymentToAccountModalForm .rr-payment-distribute-sum #DistributePaymentToAccount_DgiSum").closest(".form-group");
            var pkkDistribElemWrapper = $("#DistributePaymentToAccountModalForm .rr-payment-distribute-sum #DistributePaymentToAccount_PkkSum").closest(".form-group");
            var padunDistribElemWrapper = $("#DistributePaymentToAccountModalForm .rr-payment-distribute-sum #DistributePaymentToAccount_PadunSum").closest(".form-group");
            switch (objectType) {
                case 0:
                    hasDgi = $(row).data("currentBalanceDgi") > 0;
                    hasPkk = $(row).data("currentBalancePkk") > 0;
                    hasPadun = $(row).data("currentBalancePadun") > 0;
                  
                    currentPenalty = parseFloat($(row).data("currentBalancePenalty").toString().replace(",", "."));
                    currentTenancy = parseFloat($(row).data("currentBalanceTenancy").toString().replace(",", "."));
                    break;
                case 1:
                    hasDgi = $(row).data("claimAmountDgi") > 0;
                    hasPkk = $(row).data("claimAmountPkk") > 0;
                    hasPadun = $(row).data("claimAmountPadun") > 0;

                    currentPenalty = parseFloat($(row).data("claimAmountPenalty").toString().replace(",", "."));
                    currentPenalty -= parseFloat($(row).data("claimAmountPenaltyRecovered").toString().replace(",", "."));
                    currentTenancy = parseFloat($(row).data("claimAmountTenancyRecovered").toString().replace(",", "."));
                    currentTenancy -= parseFloat($(row).data("claimAmountTenancyRecovered").toString().replace(",", "."));
                    break;
            }

            var additionalCounts = 0;
            additionalCounts += hasDgi ? 1 : 0;
            additionalCounts += hasPkk ? 1 : 0;
            additionalCounts += hasPadun ? 1 : 0;
            if (hasDgi)
                dgiDistribElemWrapper.removeClass("d-none");
            else
                dgiDistribElemWrapper.addClass("d-none");
            if (hasPkk)
                pkkDistribElemWrapper.removeClass("d-none");
            else
                pkkDistribElemWrapper.addClass("d-none");
            if (hasPadun)
                padunDistribElemWrapper.removeClass("d-none");
            else
                padunDistribElemWrapper.addClass("d-none");

            tenancyDistribElemWrapper.addClass("col-3").removeClass("col-2").removeClass("offset-1");
            penaltyDistribElemWrapper.addClass("col-3").removeClass("col-2");
            dgiDistribElemWrapper.addClass("col-3").removeClass("col-2");
            pkkDistribElemWrapper.addClass("col-3").removeClass("col-2");
            padunDistribElemWrapper.addClass("col-3").removeClass("col-2");

            switch (additionalCounts) {
                case 0:
                    $("#DistributePaymentToAccountModalForm .rr-payment-distribute-title").removeClass("d-none").removeClass("col-3").addClass("col-6");
                    break;
                case 1:
                    $("#DistributePaymentToAccountModalForm .rr-payment-distribute-title").removeClass("d-none").removeClass("col-6").addClass("col-3");
                    break;
                case 2:
                    $("#DistributePaymentToAccountModalForm .rr-payment-distribute-title").addClass("d-none");
                    break;
                case 3:
                    $("#DistributePaymentToAccountModalForm .rr-payment-distribute-title").addClass("d-none");
                    tenancyDistribElemWrapper.removeClass("col-3").addClass("col-2").addClass("offset-1");
                    penaltyDistribElemWrapper.removeClass("col-3").addClass("col-2");
                    dgiDistribElemWrapper.removeClass("col-3").addClass("col-2");
                    pkkDistribElemWrapper.removeClass("col-3").addClass("col-2");
                    padunDistribElemWrapper.removeClass("col-3").addClass("col-2");
                    break;
            }

            var distributionTenancy = Math.round(Math.max(Math.min(sumForDistribution, Math.max(currentTenancy, 0)), 0) * 100) / 100;
            var distributionPenalty = Math.round(Math.max(Math.min((sumForDistribution - distributionTenancy), currentPenalty), 0) * 100) / 100;
            distributionTenancy = Math.round((sumForDistribution - distributionPenalty)*100)/100;
            $("#DistributePaymentToAccount_TenancySum").val(((distributionTenancy === 0 ? "0,00" : distributionTenancy) + "").replace(".", ","));
            $("#DistributePaymentToAccount_PenaltySum").val(((distributionPenalty === 0 ? "0,00" : distributionPenalty) + "").replace(".", ","));
            $("#DistributePaymentToAccount_DgiSum").val("0,00");
            $("#DistributePaymentToAccount_PkkSum").val("0,00");
            $("#DistributePaymentToAccount_PadunSum").val("0,00");
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
            "DgiSum": modal.find("#DistributePaymentToAccount_DgiSum").val().replace(".", ","),
            "PkkSum": modal.find("#DistributePaymentToAccount_PkkSum").val().replace(".", ","),
            "PadunSum": modal.find("#DistributePaymentToAccount_PadunSum").val().replace(".", ","),
            "Description": {}
        };
        var row = $("[name='DistributePaymentToAccount_IdObject']:checked").closest("tr");
        if (data.DistributeTo === 0) {
            data.Description.idAccount = data.IdObject;
            data.Description.account = row.data("account");
            data.Description.idState = row.data("accountIdState");
            data.Description.state = row.data("accountState");
            data.Description.tenant = row.data("accountTenant");
            data.Description.currentBalanceTenancy = row.data("currentBalanceTenancy");
            data.Description.currentBalancePenalty = row.data("currentBalancePenalty");
            data.Description.currentBalanceDgi = row.data("currentBalanceDgi");
            data.Description.currentBalancePkk = row.data("currentBalancePkk");
            data.Description.currentBalancePadun = row.data("currentBalancePadun");
            data.Description.currentBalanceExcludeCharge = row.data("currentBalanceExcludeCharge");
            data.Description.lastChargeDate = row.data("lastChargeDate");
        } else {
            data.Description.idClaim = data.IdObject;
            data.Description.idAccount = row.data("idAccount");
            data.Description.account = row.data("account");
            data.Description.idAccountState = row.data("accountIdState");
            data.Description.accountState = row.data("accountState");
            data.Description.courtOrderNum = row.data("claimCourtOrderNum");
            data.Description.tenant = row.data("claimTenant");
            data.Description.amountTenancy = row.data("claimAmountTenancy");
            data.Description.amountPenalties = row.data("claimAmountPenalty");
            data.Description.amountDgi = row.data("claimAmountDgi");
            data.Description.amountPkk = row.data("claimAmountPkk");
            data.Description.amountPadun = row.data("claimAmountPadun");
            data.Description.amountTenancyRecovered = row.data("claimAmountTenancyRecovered");
            data.Description.amountPenaltiesRecovered = row.data("claimAmountPenaltyRecovered");
            data.Description.amountDgiRecovered = row.data("claimAmountDgiRecovered");
            data.Description.amountPkkRecovered = row.data("claimAmountPkkRecovered");
            data.Description.amountPadunRecovered = row.data("claimAmountPadunRecovered");
            data.Description.accountCurrentBalanceTenancy = row.data("accountCurrentBalanceTenancy");
            data.Description.accountCurrentBalancePenalty = row.data("accountCurrentBalancePenalty");
            data.Description.accountCurrentBalanceDgi = row.data("accountCurrentBalanceDgi");
            data.Description.accountCurrentBalancePkk = row.data("accountCurrentBalancePkk");
            data.Description.accountCurrentBalancePadun = row.data("accountCurrentBalancePadun");
            data.Description.accountCurrentBalanceExcludeCharge = row.data("accountCurrentBalanceExcludeCharge");
            data.Description.startDeptPeriod = row.data("claimStartDeptPeriod");
            data.Description.endDeptPeriod = row.data("claimEndDeptPeriod");
        }
        $('#setDistributePaymentToAccountModalBtn').text('Сохраняем...').attr('disabled', true);

        if (distributionModalOnSelectCallback !== undefined) {
            distributionModalOnSelectCallback(data);
        }

        e.preventDefault();
    }

    $("#DistributePaymentToAccount_TenancySum, #DistributePaymentToAccount_PenaltySum, #DistributePaymentToAccount_DgiSum, #DistributePaymentToAccount_PkkSum, #DistributePaymentToAccount_PadunSum").on("change", function () {
        var sumForDistribution = $("#DistributePaymentToAccountModalForm").find("#DistributePaymentToAccount_SumForDistribution").val();
        sumForDistribution = parseFloat(sumForDistribution);   
        var tenancy = parseFloat($("#DistributePaymentToAccount_TenancySum").val().replace(",", "."));
        var penalty = parseFloat($("#DistributePaymentToAccount_PenaltySum").val().replace(",", "."));
        var dgi = parseFloat($("#DistributePaymentToAccount_DgiSum").val().replace(",", "."));
        var pkk = parseFloat($("#DistributePaymentToAccount_PkkSum").val().replace(",", "."));
        var padun = parseFloat($("#DistributePaymentToAccount_PadunSum").val().replace(",", "."));
        switch ($(this).attr("id")) {
            case "DistributePaymentToAccount_TenancySum":
                var sums = redistributePayments([tenancy, penalty, dgi, pkk, padun], 0, sumForDistribution);
                break;
            case "DistributePaymentToAccount_PenaltySum":
                sums = redistributePayments([tenancy, penalty, dgi, pkk, padun], 1, sumForDistribution);
                break;
            case "DistributePaymentToAccount_DgiSum":
                sums = redistributePayments([tenancy, penalty, dgi, pkk, padun], 2, sumForDistribution);
                break;
            case "DistributePaymentToAccount_PkkSum":
                sums = redistributePayments([tenancy, penalty, dgi, pkk, padun], 3, sumForDistribution);
                break;
            case "DistributePaymentToAccount_PadunSum":
                sums = redistributePayments([tenancy, penalty, dgi, pkk, padun], 4, sumForDistribution);
                break;
        }
        $("#DistributePaymentToAccount_TenancySum").val((sums[0] === 0 ? "0,00" : sums[0] + "").replace(".", ","));
        $("#DistributePaymentToAccount_PenaltySum").val((sums[1] === 0 ? "0,00" : sums[1] + "").replace(".", ","));
        $("#DistributePaymentToAccount_DgiSum").val((sums[2] === 0 ? "0,00" : sums[2] + "").replace(".", ","));
        $("#DistributePaymentToAccount_PkkSum").val((sums[3] === 0 ? "0,00" : sums[3] + "").replace(".", ","));
        $("#DistributePaymentToAccount_PadunSum").val(sums[4] === 0 ? "0,00" : (sums[4] + "").replace(".", ","));
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

        distributionModalInitiate(purpose, null, paymentSum, paymentSumPosted, idPayment, false, distributePaymentToObjectOnSelectCallback);

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
        var dgiSum = parseFloat($("#DistributePaymentToAccount_DgiSum").val().replace(",", "."));
        var pkkSum = parseFloat($("#DistributePaymentToAccount_PkkSum").val().replace(",", "."));
        var padunSum = parseFloat($("#DistributePaymentToAccount_PadunSum").val().replace(",", "."));
        var tenancySum = Math.round(Math.max(0, sumForDistribute - penaltySum - dgiSum - pkkSum - padunSum) * 100) / 100;
        if (tenancySum === 0) tenancySum = "0,00";
        $("#DistributePaymentToAccount_TenancySum").val((tenancySum + "").replace(".", ","));
        e.preventDefault();
    });

    $("#DistributePaymentToAccountPenaltyLeftovers").on("click", function (e) {
        var sumForDistribute = parseFloat($("#DistributePaymentToAccount_SumForDistribution").val().replace(",", "."));
        var tenancySum = parseFloat($("#DistributePaymentToAccount_TenancySum").val().replace(",", "."));
        var dgiSum = parseFloat($("#DistributePaymentToAccount_DgiSum").val().replace(",", "."));
        var pkkSum = parseFloat($("#DistributePaymentToAccount_PkkSum").val().replace(",", "."));
        var padunSum = parseFloat($("#DistributePaymentToAccount_PadunSum").val().replace(",", "."));
        var penaltySum = Math.round(Math.max(0, sumForDistribute - tenancySum - dgiSum - pkkSum - padunSum) * 100) / 100;
        if (penaltySum === 0) penaltySum = "0,00";
        $("#DistributePaymentToAccount_PenaltySum").val((penaltySum + "").replace(".", ","));
        e.preventDefault();
    });

    $("#DistributePaymentToAccountDgiLeftovers").on("click", function (e) {
        var sumForDistribute = parseFloat($("#DistributePaymentToAccount_SumForDistribution").val().replace(",", "."));
        var tenancySum = parseFloat($("#DistributePaymentToAccount_TenancySum").val().replace(",", "."));
        var penaltySum = parseFloat($("#DistributePaymentToAccount_PenaltySum").val().replace(",", "."));
        var pkkSum = parseFloat($("#DistributePaymentToAccount_PkkSum").val().replace(",", "."));
        var padunSum = parseFloat($("#DistributePaymentToAccount_PadunSum").val().replace(",", "."));
        var dgiSum = Math.round(Math.max(0, sumForDistribute - tenancySum - penaltySum - pkkSum - padunSum) * 100) / 100;
        if (dgiSum === 0) dgiSum = "0,00";
        $("#DistributePaymentToAccount_DgiSum").val((dgiSum + "").replace(".", ","));
        e.preventDefault();
    });

    $("#DistributePaymentToAccountPkkLeftovers").on("click", function (e) {
        var sumForDistribute = parseFloat($("#DistributePaymentToAccount_SumForDistribution").val().replace(",", "."));
        var tenancySum = parseFloat($("#DistributePaymentToAccount_TenancySum").val().replace(",", "."));
        var penaltySum = parseFloat($("#DistributePaymentToAccount_PenaltySum").val().replace(",", "."));
        var dgiSum = parseFloat($("#DistributePaymentToAccount_DgiSum").val().replace(",", "."));
        var padunSum = parseFloat($("#DistributePaymentToAccount_PadunSum").val().replace(",", "."));
        var pkkSum = Math.round(Math.max(0, sumForDistribute - tenancySum - penaltySum - dgiSum - padunSum) * 100) / 100;
        if (pkkSum === 0) pkkSum = "0,00";
        $("#DistributePaymentToAccount_PkkSum").val((pkkSum + "").replace(".", ","));
        e.preventDefault();
    });

    $("#DistributePaymentToAccountPadunLeftovers").on("click", function (e) {
        var sumForDistribute = parseFloat($("#DistributePaymentToAccount_SumForDistribution").val().replace(",", "."));
        var tenancySum = parseFloat($("#DistributePaymentToAccount_TenancySum").val().replace(",", "."));
        var penaltySum = parseFloat($("#DistributePaymentToAccount_PenaltySum").val().replace(",", "."));
        var dgiSum = parseFloat($("#DistributePaymentToAccount_DgiSum").val().replace(",", "."));
        var pkkSum = parseFloat($("#DistributePaymentToAccount_PkkSum").val().replace(",", "."));
        var padunSum = Math.round(Math.max(0, sumForDistribute - tenancySum - penaltySum - dgiSum - pkkSum) * 100) / 100;
        if (padunSum === 0) padunSum = "0,00";
        $("#DistributePaymentToAccount_PadunSum").val((padunSum + "").replace(".", ","));
        e.preventDefault();
    });
});