
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
        if (!claimCustomValidations(validator)) {
            isFormValid = false;
        }

        if (!isFormValid) {
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
            // TODO: перенос компонентных данных в hidden поля
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

    $("#Claim_IdAccountNavigation_Account").autocomplete({
        source: function (request, response) {
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/Claims/GetAccounts',
                dataType: 'json',
                data: { text: request.term },
                success: function (data) {
                    response($.map(data, function (item) {
                        return { label: item.account, value: item.account, idAccount: item.idAccount };
                    }));
                }
            });
        },
        select: function (event, ui) {
            $("#Claim_IdAccount").val(ui.item.idAccount);
            $("#Claim_IdAccountNavigation_Account").val(ui.item.value);
        },
        minLength: 3
    });

    $("#Claim_IdAccountNavigation_Account").on("focusout", function () {
        if ($('#Claim_IdAccount').val() === "") {
            $('#Claim_IdAccountNavigation_Account').val("");
        }
    });
});