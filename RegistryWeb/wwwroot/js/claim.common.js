﻿
$(function () {
    $('.claim-toggler').each(function (idx, e) {
        $(e).on('click', $('#' + $(e).data("for")), elementToogleHide);
    });

    function claimCustomValidations(validator) {
        var isValid = true;

        return isValid;
    }

    $("#ClaimForm").on("submit", function (e) {
        var action = $("#ClaimForm").data("action");
        $("button[data-id], .bootstrap-select").removeClass("input-validation-error");
        $("input.decimal").each(function (idx, elem) {
            $(elem).val($(elem).val().replace(".", ","));
        });

        var validator = $(this).validate();
        var isFormValid = $(this).valid();
        var isStateFormValid = $("#ClaimStatesForm").valid();
        if (!claimCustomValidations(validator)) {
            isFormValid = false;
        }

        if (!isFormValid || !isStateFormValid) {
            $("select").each(function (idx, elem) {
                var id = $(elem).prop("id");
                var name = $(elem).prop("name");
                var errorSpan = $("span[data-valmsg-for='" + name + "']");
                if (errorSpan.hasClass("field-validation-error")) {
                    $("button[data-id='" + id + "']").addClass("input-validation-error");
                }
            });

            $(".toggle-hide").each(function (idx, elem) {
                if ($(elem).find(".field-validation-error").length > 0) {
                    var toggler = $(elem).closest(".card").find(".card-header .claim-toggler").first();
                    if (!isExpandElemntArrow(toggler)) {
                        toggler.click();
                    }
                }
            });
            $([document.documentElement, document.body]).animate({
                scrollTop: $(".input-validation-error").first().offset().top - 35
            }, 1000);

            e.preventDefault();
        } else {
            if (action !== "Create") return true;
            var inputTemplate = "<input type='hidden' name='{0}' value='{1}'>";
            let claimStates = getClaimStates();
            for (let i = 0; i < claimStates.length; i++) {
                let tr = "Claim.ClaimStates[" + i + "].";
                $(this).append(inputTemplate.replace('{0}', tr + "IdState").replace('{1}', claimStates[i].IdState));
                $(this).append(inputTemplate.replace('{0}', tr + "IdClaim").replace('{1}', claimStates[i].IdClaim));
                $(this).append(inputTemplate.replace('{0}', tr + "IdStateType").replace('{1}', claimStates[i].IdStateType));
                $(this).append(inputTemplate.replace('{0}', tr + "DateStartState").replace('{1}', claimStates[i].DateStartState));
                $(this).append(inputTemplate.replace('{0}', tr + "Description").replace('{1}', claimStates[i].Description));
                $(this).append(inputTemplate.replace('{0}', tr + "Executor").replace('{1}', claimStates[i].Executor));
                // При создании исковой работы разрешено добавлять только первый этап "Запрос в БКС"
                $(this).append(inputTemplate.replace('{0}', tr + "BksRequester").replace('{1}', claimStates[i].BksRequester));
            }

            let claimFiles = getClaimFiles();
            for (let i = 0; i < claimFiles.length; i++) {
                let tf = "Claim.ClaimFiles[" + i + "].";
                $(this).append(inputTemplate.replace('{0}', tf + "IdFile").replace('{1}', claimFiles[i].IdFile));
                $(this).append(inputTemplate.replace('{0}', tf + "IdClaim").replace('{1}', claimFiles[i].IdClaim));
                $(this).append(inputTemplate.replace('{0}', tf + "Description").replace('{1}', claimFiles[i].Description));
                let file = $(claimFiles[i].AttachmentFile).clone();
                file.attr("name", "ClaimFile[" + i + "]");
                $(this).append(file);
            }
        }
    });

    $("form").on("change", "select", function () {
        var isValid = $(this).valid();
        var id = $(this).prop("id");
        if (!isValid) {
            $("button[data-id='" + id + "']").addClass("input-validation-error");
        } else {

            $("button[data-id='" + id + "']").removeClass("input-validation-error");
        }
    });

    $("#createBtn, #editBtn, #deleteBtn").on("click", function () {
        $("#ClaimForm").submit();
    });

    $("#ClaimAmount .decimal").on("focusout", function () {
        var totalSum = 0;
        var tenancyElem = $("#Claim_AmountTenancy");
        if (tenancyElem.valid() && tenancyElem.val() !== "" && tenancyElem.val() !== null) {
            let sum = parseFloat(tenancyElem.val().replace(",", "."));
            tenancyElem.val(sum.toFixed(2).replace(".", ","));
            totalSum += sum;
        }
        var penaltiesElem = $("#Claim_AmountPenalties");
        if (penaltiesElem.valid() && penaltiesElem.val() !== "" && penaltiesElem.val() !== null) {
            let sum = parseFloat(penaltiesElem.val().replace(",", "."));
            penaltiesElem.val(sum.toFixed(2).replace(".", ","));
            totalSum += sum;
        }
        var dgiElem = $("#Claim_AmountDgi");
        if (dgiElem.valid() && dgiElem.val() !== "" && dgiElem.val() !== null) {
            let sum = parseFloat(dgiElem.val().replace(",", "."));
            dgiElem.val(sum.toFixed(2).replace(".", ","));
            totalSum += sum;
        }
        var padunElem = $("#Claim_AmountPadun");
        if (padunElem.valid() && padunElem.val() !== "" && padunElem.val() !== null) {
            let sum = parseFloat(padunElem.val().replace(",", "."));
            padunElem.val(sum.toFixed(2).replace(".", ","));
            totalSum += sum;
        }
        var pkkElem = $("#Claim_AmountPkk");
        if (pkkElem.valid() && pkkElem.val() !== "" && pkkElem.val() !== null) {
            let sum = parseFloat(pkkElem.val().replace(",", "."));
            pkkElem.val(sum.toFixed(2).replace(".", ","));
            totalSum += sum;
        }
        $("#Claim_AmountTotal").val(totalSum.toFixed(2).replace(".", ","));
    });


    $("#Claim_IdAccountNavigation_Account").on("input", function () {
        $("#Claim_IdAccount").val("");
    });

    function selectAccount(account) {
        $("#Claim_IdAccount").val(account.idAccount);
        $("#Claim_IdAccountNavigation_Account").val(account.value);
        if (account.idAccount !== "" && account.idAccount !== null) {
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/Claims/GetRentAddress',
                dataType: 'json',
                data: { idAccount: account.idAccount },
                success: function (address) {
                    $("input[name='Claim.BksAddress']").val(address.bksAddress);
                    $("input[name='Claim.RegistryAddress']").val(address.registryAddress);
                }
            });
        }
    }

    $("#Claim_IdAccountNavigation_Account").autocomplete({
        source: function (request, response) {
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/Claims/GetAccounts',
                dataType: 'json',
                data: { text: request.term },
                success: function (data) {
                    response($.map(data, function (item) {
                        let account = { label: item.account, value: item.account, idAccount: item.idAccount };
                        if (data.length === 1) {
                            selectAccount(account);
                        }
                        return account;
                    }));
                }
            });
        },
        select: function (event, ui) {
            selectAccount(ui.item);
        },
        minLength: 3
    });

    $("#Claim_IdAccountNavigation_Account").on("focusout", function () {
        if ($('#Claim_IdAccount').val() === "") {
            $('#Claim_IdAccountNavigation_Account').val("");
            $("input[name='Claim.BksAddress']").val("");
            $("input[name='Claim.RegistryAddress']").val("");
        }
    });
});