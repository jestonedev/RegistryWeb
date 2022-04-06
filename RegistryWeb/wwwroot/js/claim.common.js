
$(function () {
    var form = $("#ClaimForm");

    $('.claim-toggler').each(function (idx, e) {
        $(e).on('click', $('#' + $(e).data("for")), elementToogleHide);
    });

    function claimCustomValidations(validator) {
        var isValid = true;
        var accountErrorWrapper = $("input[name='AnyAccountRequire']");
        var idAccountBks = form.find("#Claim_IdAccountNavigation_Account");
        var idAccountKumi = form.find("#Claim_IdAccountKumiNavigation_Account");
        if (idAccountBks.val() === "" && idAccountKumi.val() === "") {
            let error = {};
            error["AnyAccountRequire"] = "Укажите лицевой счет";
            validator.showErrors(error);
            $(".rr-account-common-error").addClass("mb-2");
            idAccountBks.addClass("input-validation-error");
            idAccountKumi.addClass("input-validation-error");
            isValid = false;
        } else {
            clearValidationError(accountErrorWrapper);
            removeErrorFromValidator(validator, accountErrorWrapper);
            $(".rr-account-common-error").removeClass("mb-2");
            idAccountBks.removeClass("input-validation-error");
            idAccountKumi.removeClass("input-validation-error");
        }
        return isValid;
    }

    form.on("submit", function (e) {
        var action = form.data("action");
        $("button[data-id], .bootstrap-select").removeClass("input-validation-error");
        $("input.decimal").each(function (idx, elem) {
            $(elem).val($(elem).val().replace(".", ","));
        });

        var validator = $(this).validate();
        var isFormValid = $(this).valid();
        var isStateFormValid = $("#ClaimStatesForm").valid();
        var isPersonsFormValid = $("#ClaimPersonsForm").valid();
        if (!claimCustomValidations(validator)) {
            isFormValid = false;
        }

        var itemsInEditMode = $("ul.list-group .yes-no-panel").filter(function (idx, elem) {
            return $(elem).css("display") !== "none";
        });

        if (!isFormValid || !isStateFormValid || !isPersonsFormValid) {
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
        } else
        if (itemsInEditMode.length > 0 && action === "Edit")
        {
            itemsInEditMode.each(function (idx, elem) {
                if ($(elem).closest("ul.list-group").hasClass("toggle-hide")) {
                    var toggler = $(elem).closest(".card").find(".card-header .claim-toggler").first();
                    toggler.click();
                }
                var listGroupItem = $(elem).closest(".list-group-item");
                if (!listGroupItem.hasClass("list-group-item-warning")) {
                    listGroupItem.addClass("list-group-item-warning");
                }
            });
            $([document.documentElement, document.body]).animate({
                scrollTop: itemsInEditMode.first().closest(".list-group-item").offset().top
            }, 1000);

            e.preventDefault();
        }
        else {
            if (action !== "Create") {
                return true;
            }

            let claimPersons = getClaimPersons();

            if ($(this).data("personsSourceSelected") === 0 && claimPersons.length === 0) {
                var personsSourceModal = $("#PersonsSourceModal");
                personsSourceModal.find("#PeronsSource").val(0);
                personsSourceModal.modal('show');
                return false;
            }

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
            
            for (let i = 0; i < claimPersons.length; i++) {
                let tr = "Claim.ClaimPersons[" + i + "].";
                $(this).append(inputTemplate.replace('{0}', tr + "IdPerson").replace('{1}', claimPersons[i].IdPerson));
                $(this).append(inputTemplate.replace('{0}', tr + "IdClaim").replace('{1}', claimPersons[i].IdClaim));
                $(this).append(inputTemplate.replace('{0}', tr + "Surname").replace('{1}', claimPersons[i].Surname));
                $(this).append(inputTemplate.replace('{0}', tr + "Name").replace('{1}', claimPersons[i].Name));
                $(this).append(inputTemplate.replace('{0}', tr + "Patronymic").replace('{1}', claimPersons[i].Patronymic));
                $(this).append(inputTemplate.replace('{0}', tr + "DateOfBirth").replace('{1}', claimPersons[i].DateOfBirth));
                $(this).append(inputTemplate.replace('{0}', tr + "PlaceOfBirth").replace('{1}', claimPersons[i].PlaceOfBirth));
                $(this).append(inputTemplate.replace('{0}', tr + "WorkPlace").replace('{1}', claimPersons[i].WorkPlace));
                $(this).append(inputTemplate.replace('{0}', tr + "IsClaimer").replace('{1}', claimPersons[i].IsClaimer));
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
        form.submit();
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

    // IdAccount
    $("#Claim_IdAccountNavigation_Account").on("input", function () {
        $("#Claim_IdAccount").val("");
    });

    function selectAccount(account, type) {
        if (type == "BKS") {
            $("#Claim_IdAccount").val(account.idAccount);
            $("#Claim_IdAccountNavigation_Account").val(account.value);
        } else
        if (type == "KUMI") {
            $("#Claim_IdAccountKumi").val(account.idAccount);
            $("#Claim_IdAccountKumiNavigation_Account").val(account.value);
        }
        if (account.idAccount !== "" && account.idAccount !== null) {
            var amountHeader = $(".rr-claim-amount-sum-header");
            amountHeader.html("Сумма к взысканию <span class='text-danger'>(обновляется...)</span>");
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/Claims/GetAccountInfo',
                dataType: 'json',
                data: { idAccount: account.idAccount, type: type },
                success: function (address) {
                    if (type === "BKS") {
                        $("input[name='Claim.BksAddress']").val(address.bksAddress);
                        $("input[name='Claim.RegistryAddressBks']").val(address.registryAddress);
                        amountHeader.html("Сумма к взысканию <span class='text-danger'>(по данным из ЛС БКС)</span>");
                    }
                    if (type === "KUMI") {
                        $("input[name='Claim.RegistryAddressKumi']").val(address.registryAddress);
                        amountHeader.html("Сумма к взысканию <span class='text-danger'>(по данным из ЛС КУМИ)</span>");
                    }
                    $("input[id='Claim_AmountTenancy']").val(address.amountTenancy);
                    $("input[id='Claim_AmountPenalties']").val(address.amountPenalties);
                    $("input[id='Claim_AmountDgi']").val(address.amountDgi);
                    $("input[id='Claim_AmountPadun']").val(address.amountPadun);
                    $("input[id='Claim_AmountPkk']").val(address.amountPkk).focusout();
                }
            });
        }
        var accountErrorWrapper = $("input[name='AnyAccountRequire']");
        var idAccountBks = form.find("#Claim_IdAccountNavigation_Account");
        var idAccountKumi = form.find("#Claim_IdAccountKumiNavigation_Account");

        if (idAccountBks.val() !== "" || idAccountKumi !== "") {
            clearValidationError(accountErrorWrapper);
            removeErrorFromValidator(form.validate(), accountErrorWrapper);
            $(".rr-account-common-error").removeClass("mb-2");
            idAccountBks.removeClass("input-validation-error");
            idAccountKumi.removeClass("input-validation-error");
        }
    }

    $("#Claim_IdAccountNavigation_Account").autocomplete({
        source: function (request, response) {
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/Claims/GetAccounts',
                dataType: 'json',
                data: { text: request.term, type: "BKS" },
                success: function (data) {
                    response($.map(data, function (item) {
                        let account = { label: item.account, value: item.account, idAccount: item.idAccount };
                        if (data.length === 1) {
                            selectAccount(account, "BKS");
                        }
                        return account;
                    }));
                }
            });
        },
        select: function (event, ui) {
            selectAccount(ui.item, "BKS");
        },
        minLength: 3
    });

    $("#Claim_IdAccountNavigation_Account").on("focusout", function () {
        if ($('#Claim_IdAccount').val() === "") {
            $('#Claim_IdAccountNavigation_Account').val("");
            $("input[name='Claim.BksAddress']").val("");
            $("input[name='Claim.RegistryAddressBks']").val("");
        }
    });

    // IdAccountAddtional
    $("#Claim_IdAccountAdditionalNavigation_Account").on("input", function () {
        $("#Claim_IdAccountAdditional").val("");
    });

    function selectAdditionalAccount(account) {
        $("#Claim_IdAccountAdditional").val(account.idAccount);
        $("#Claim_IdAccountAdditionalNavigation_Account").val(account.value);
    }

    $("#Claim_IdAccountAdditionalNavigation_Account").autocomplete({
        source: function (request, response) {
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/Claims/GetAccounts',
                dataType: 'json',
                data: { text: request.term, type: 'BKS' },
                success: function (data) {
                    response($.map(data, function (item) {
                        let account = { label: item.account, value: item.account, idAccount: item.idAccount };
                        if (data.length === 1) {
                            selectAdditionalAccount(account);
                        }
                        return account;
                    }));
                }
            });
        },
        select: function (event, ui) {
            selectAdditionalAccount(ui.item);
        },
        minLength: 3
    });

    $("#Claim_IdAccountAdditionalNavigation_Account").on("focusout", function () {
        if ($('#Claim_IdAccountAdditional').val() === "") {
            $('#Claim_IdAccountAdditionalNavigation_Account').val("");
        }
    });

// IdAccountKumi
    $("#Claim_IdAccountKumiNavigation_Account").on("input", function () {
        $("#Claim_IdAccountKumi").val("");
    });

    $("#Claim_IdAccountKumiNavigation_Account").autocomplete({
        source: function (request, response) {
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/Claims/GetAccounts',
                dataType: 'json',
                data: { text: request.term, type: "KUMI" },
                success: function (data) {
                    response($.map(data, function (item) {
                        let account = { label: item.account, value: item.account, idAccount: item.idAccount };
                        if (data.length === 1) {
                            selectAccount(account, "KUMI");
                        }
                        return account;
                    }));
                }
            });
        },
        select: function (event, ui) {
            selectAccount(ui.item, "KUMI");
        },
        minLength: 3
    });

    $("#Claim_IdAccountKumiNavigation_Account").on("focusout", function () {
        if ($('#Claim_IdAccountKumi').val() === "") {
            $('#Claim_IdAccountKumiNavigation_Account').val("");
            $("input[name='Claim.RegistryAddressKumi']").val("");
        }
    });

    $("#PersonsSourceModal .rr-select-source").on('click', function (e) {
        var form = $("#ClaimForm");
        form.data("personsSourceSelected", 1);
        form.find("#LoadPersonsSource").val($("#PersonsSourceModal #PeronsSource").val());
        form.submit();
        e.preventDefault();
    });

    $("#PersonsSourceModal #PeronsSource").on('change', function (e) {
        var modal = $("#PersonsSourceModal");
        var source = $(this).val();
        var idAccountBks = $("#ClaimForm #Claim_IdAccount").val();
        if (idAccountBks === "0") idAccountBks = null;
        var idAccountKumi = $("#ClaimForm #Claim_IdAccountKumi").val();
        if (idAccountKumi === "0") idAccountKumi = null;
        var loader = modal.find(".rr-persons-loader");
        var resultWrapper = modal.find(".rr-persons-preview");
        if (source !== 0)
            loader.removeClass("d-none");
        resultWrapper.addClass("d-none").html("");

        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Claims/GetClaimPersonsBySource',
            data: { loadPersonsSource: source, idAccountBks: idAccountBks, idAccountKumi: idAccountKumi },
            success: function (data) {
                loader.addClass("d-none");
                resultWrapper.removeClass("d-none").html(data);
            }
        });

        e.preventDefault();
    });

    $("#PersonsSourceModal").on("hidden.bs.modal", function () {
        $(this).find(".rr-persons-loader").addClass("d-none");
        $(this).find(".rr-persons-preview").addClass("d-none").html("");
    });

    $(".rr-claim-account-main .dropdown-item").on("click", function (e) {
        var text = $(this).text();
        $(this).closest(".input-group-prepend").find("button").text(text);
        switch (text) {
            case "ЛС КУМИ":
                reformatClaimForKumiAccount();
                break;
            case "ЛС БКС":
                reformatClaimForBksAccount();
                break;
            case "ЛС БКС/КУМИ":
                reformatClaimForBksAndKumiAccount();
                break;
        }
        e.preventDefault();
    });

    var accountMainWrapper = $(".rr-claim-account-main");
    var accountKumi = $("#Claim_IdAccountKumiNavigation_Account");
    var accountBks = $("#Claim_IdAccountNavigation_Account");
    var accountBksAdditionaleWrapper = $(".rr-claim-account-additional");
    var claimAtDateWrapper = $("#Claim_AtDate").closest(".form-group");
    var claimStartDeptWrapper = $("#Claim_StartDeptPeriod").closest(".form-group");
    var claimEndDeptWrapper = $("#Claim_EndDeptPeriod").closest(".form-group");
    var bksAddresses = $(".rr-claim-bks-address");
    var kumiAddress = $(".rr-claim-kumi-address");
    var additionalAmountWrapper = $(".rr-claim-additional-amount");

    if (accountMainWrapper.find(".input-group-prepend button").text() === "ЛС БКС") {
        accountMainWrapper.append(accountBks);
    }

    function reformatClaimForKumiAccount() {
        $("#Claim_IdAccount").val("");
        $("#Claim_IdAccountAdditional").val("");
        $("#Claim_IdAccountAdditionalNavigation_Account").val("");
        accountBks.val("").addClass("d-none");
        accountKumi.removeClass("d-none");
        accountMainWrapper.append(accountKumi);
        accountMainWrapper.closest(".form-group").addClass("col-md-6 col-lg-3").removeClass("col-md-8 col-lg-6");
        accountBksAdditionaleWrapper.addClass("d-none");
        claimAtDateWrapper.addClass("col-md-6 col-lg-3").removeClass("col-md-4 col-lg-2");
        claimStartDeptWrapper.addClass("col-md-6 col-lg-3").removeClass("col-md-4 col-lg-2 col-lg-6");
        claimEndDeptWrapper.addClass("col-md-6 col-lg-3").removeClass("col-md-4 col-lg-2 col-lg-6");
        bksAddresses.addClass("d-none");
        bksAddresses.find("input").val("");
        $(bksAddresses[0]).find("label").text("Адрес арендуемого ЖП по БКС");
        $(bksAddresses[1]).find("label").text("Адрес арендуемого ЖП по ЖФ");
        kumiAddress.removeClass("d-none");
        kumiAddress.find("label").text("Адрес арендуемого ЖП");
        additionalAmountWrapper.addClass("d-none");
        additionalAmountWrapper.find("input").val("0,00");
    }

    function reformatClaimForBksAccount() {
        $("#Claim_IdAccountKumi").val("");
        accountKumi.val("").addClass("d-none");
        accountBks.removeClass("d-none");
        accountMainWrapper.append(accountBks);
        accountMainWrapper.closest(".form-group").addClass("col-md-6 col-lg-3").removeClass("col-md-8 col-lg-6");
        accountBksAdditionaleWrapper.removeClass("d-none");
        accountBksAdditionaleWrapper.addClass("col-md-6").removeClass("col-md-4");
        claimAtDateWrapper.addClass("col-md-4 col-lg-2").removeClass("col-md-6 col-lg-3");
        claimStartDeptWrapper.addClass("col-md-4 col-lg-2").removeClass("col-md-6 col-lg-3 col-lg-6");
        claimEndDeptWrapper.addClass("col-md-4 col-lg-2").removeClass("col-md-6 col-lg-3 col-lg-6");
        kumiAddress.addClass("d-none");
        kumiAddress.find("input").val("");
        bksAddresses.removeClass("d-none");
        additionalAmountWrapper.removeClass("d-none");
    }

    function reformatClaimForBksAndKumiAccount() {
        accountBks.removeClass("d-none");
        accountKumi.removeClass("d-none");
        accountMainWrapper.append(accountKumi);
        accountMainWrapper.closest(".form-group").addClass("col-md-8 col-lg-6").removeClass("col-md-6 col-lg-3");
        accountBksAdditionaleWrapper.removeClass("d-none");
        accountBksAdditionaleWrapper.addClass("col-md-4").removeClass("col-md-6");
        claimAtDateWrapper.addClass("col-md-4 col-lg-3").removeClass("col-md-6 col-lg-2");
        claimStartDeptWrapper.addClass("col-md-4 col-lg-6").removeClass("col-md-6 col-lg-2 col-lg-3");
        claimEndDeptWrapper.addClass("col-md-4 col-lg-6").removeClass("col-md-6 col-lg-2 col-lg-3");
        kumiAddress.removeClass("d-none");
        bksAddresses.removeClass("d-none");
        $(bksAddresses[0]).find("label").text("Адрес арендуемого ЖП по БКС (ЛС БКС)");
        $(bksAddresses[1]).find("label").text("Адрес арендуемого ЖП по ЖФ (ЛС БКС)");
        kumiAddress.find("label").text("Адрес арендуемого ЖП (ЛС КУМИ)");
        additionalAmountWrapper.removeClass("d-none");
    }
});