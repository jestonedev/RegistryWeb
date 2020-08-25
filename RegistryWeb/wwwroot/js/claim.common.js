
$(function () {
    $('.claim-toggler').each(function (idx, e) {
        $(e).on('click', $('#' + $(e).data("for")), elementToogleHide);
    });

    function claimCustomValidations(validator) {
        var isValid = true;
        // TODO: Валидация состояний при добавлении
        return isValid;
    }

    $("#ClaimForm").on("submit", function (e) {
        var action = $("#ClaimForm").data("action");
        $("button[data-id], .bootstrap-select").removeClass("input-validation-error");
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
});