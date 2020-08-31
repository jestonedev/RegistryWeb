function calculateAmountTotal(self) {
    var wrapper = self.hasClass("modal") ? self : self.closest(".modal");
    var totalSum = 0;
    var tenancyElem = wrapper.find("input[name^='CourtOrder.AmountTenancy']");
    if (tenancyElem.valid() && tenancyElem.val() !== "" && tenancyElem.val() !== null) {
        let sum = parseFloat(tenancyElem.val().replace(",", "."));
        tenancyElem.val(sum.toFixed(2).replace(".", ","));
        totalSum += sum;
    }
    var penaltiesElem = wrapper.find("input[name^='CourtOrder.AmountPenalties']");
    if (penaltiesElem.valid() && penaltiesElem.val() !== "" && penaltiesElem.val() !== null) {
        let sum = parseFloat(penaltiesElem.val().replace(",", "."));
        penaltiesElem.val(sum.toFixed(2).replace(".", ","));
        totalSum += sum;
    }
    var dgiElem = wrapper.find("input[name^='CourtOrder.AmountDgi']");
    if (dgiElem.valid() && dgiElem.val() !== "" && dgiElem.val() !== null) {
        let sum = parseFloat(dgiElem.val().replace(",", "."));
        dgiElem.val(sum.toFixed(2).replace(".", ","));
        totalSum += sum;
    }
    var padunElem = wrapper.find("input[name^='CourtOrder.AmountPadun']");
    if (padunElem.valid() && padunElem.val() !== "" && padunElem.val() !== null) {
        let sum = parseFloat(padunElem.val().replace(",", "."));
        padunElem.val(sum.toFixed(2).replace(".", ","));
        totalSum += sum;
    }
    var pkkElem = wrapper.find("input[name^='CourtOrder.AmountPkk']");
    if (pkkElem.valid() && pkkElem.val() !== "" && pkkElem.val() !== null) {
        let sum = parseFloat(pkkElem.val().replace(",", "."));
        pkkElem.val(sum.toFixed(2).replace(".", ","));
        totalSum += sum;
    }
    var totalElem = wrapper.find("input[name^='CourtOrder.AmountTotal']");
    totalElem.val(totalSum.toFixed(2).replace(".", ","));
}

$(function () {
    $("body").on("focusout", "input[name^='CourtOrder.Amount']", function () {
        calculateAmountTotal($(this));
    });

    function claimCourtOrderFillModal(claimCourtOrderElem, modal, action) {
        var fields = claimCourtOrderElem.find("input, select, textarea");
        var modalFields = modal.find("input, select, textarea");
        modalFields.prop("disabled", "");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name").split("_")[0];
            modal.find("[name^='CourtOrder." + name + "']").val($(elem).val());
        });
        if (action === "Details" || action === "Delete") {
            modalFields.prop("disabled", "disabled");
        }
        else {
            modalFields.prop("disabled", "");
            var amountTotal = modal.find("input[name^='CourtOrder.AmountTotal']");
            modal.find("input[name^='CourtOrder.CreateDate'], select[name^='CourtOrder.IdExecutor']").prop("disabled", "disabled");
            amountTotal.prop("disabled", "disabled");
        }
        calculateAmountTotal(modal);
    }

    function claimCourtOrderFillElem(claimCourtOrderElem, modal) {
        var fields = claimCourtOrderElem.find("input, select, textarea");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name").split("_")[0];
            var value = modal.find("[name^='CourtOrder." + name + "']").val();
            $(elem).val(value);
            if (elem.tagName === "SELECT") {
                $(elem).selectpicker("refresh");
            }
        });
    }

    $("body").on("show.bs.modal", "[id^='ClaimCourtOrderModal']", function () {
        $(this).find("select").selectpicker("refresh");
    });

    $("body").on("hide.bs.modal", "[id^='ClaimCourtOrderModal']", function () {
        $(this).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
        $(this).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");
        var claimCourtOrderElemsWrapper = $(this).closest("form").prev("form[id^='ClaimCourtOrdersForm']").find("[id^='ClaimCourtOrders']");
        var claimCourtOrderElem = claimCourtOrderElemsWrapper.find('.list-group-item[data-processing]');
        claimCourtOrderElem.removeAttr("data-processing");
        addingClaimCourtOrderElem = undefined;
    });

    function updateInsertClaimCourtOrderElem(modal) {
        var list = $(modal).closest("form").prev("form[id^='ClaimCourtOrdersForm']").find("[id^='ClaimCourtOrder']");
        let claimCourtOrderElem = list.find(".list-group-item[data-processing]");
        if (claimCourtOrderElem.length > 0) {
            claimCourtOrderFillElem(claimCourtOrderElem, modal);
            modal.modal('hide');
        } else {
            list.find(".rr-list-group-item-empty").hide();
            list.append(addingClaimCourtOrderElem);
            let claimCourtOrderElem = list.find('.list-group-item').last();
            claimCourtOrderElem.find("select").selectpicker("render");
            claimCourtOrderFillElem(claimCourtOrderElem, modal);
            claimCourtOrderElem.find(".edit-del-court-order-panel a").removeClass("disabled");
            modal.modal('hide');
            $([document.documentElement, document.body]).animate({
                scrollTop: $(claimCourtOrderElem).offset().top
            }, 1000);
        }
    }

    $("body").on("click", "[id^='saveClaimCourtOrderModalBtn']", function (e) {
        var form = $(this).closest("form");
        var isValid = form.valid();

        if (isValid) {
            updateInsertClaimCourtOrderElem($(this).closest("[id^='ClaimCourtOrderModal']"));
        } else {
            form.find("select").each(function (idx, elem) {
                var id = $(elem).prop("id");
                var name = $(elem).prop("name");
                var errorSpan = $("span[data-valmsg-for='" + name + "']");
                if (errorSpan.hasClass("field-validation-error")) {
                    $("button[data-id='" + id + "']").addClass("input-validation-error");
                }
            });
        }
    });

    function deleteClaimCourtOrder(e) {
        let isOk = confirm("Вы уверены что хотите удалить версию судебного приказа?");
        if (isOk) {
            let courtOrderElem = $(this).closest(".list-group-item");
            let courtOrderWrapper = courtOrderElem.closest("[id^='ClaimCourtOrders']");
            courtOrderElem.remove();

            if (courtOrderWrapper.find(".list-group-item").length === 1) {
                courtOrderWrapper.find(".rr-list-group-item-empty").show();
            }
        }
        e.preventDefault();
    }

    var addingClaimCourtOrderElem = undefined;

    function addClaimCourtOrder(e) {
        let claimCourtOrderElemsWrapper = $(this).closest("form").find('[id^="ClaimCourtOrders"]');
        let action = claimCourtOrderElemsWrapper.data('action');
        let idAccount = $("#Claim_IdAccount").val();
        let self = this;
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Claims/AddCourtOrder',
            data: { action, idAccount },
            success: function (elem) {
                addingClaimCourtOrderElem = elem;
                var modal = $(self).closest("form").next("[id^='ClaimCourtOrdersModalForm']").find("[id^='ClaimCourtOrderModal']");
                claimCourtOrderFillModal($(elem), modal, action);
                // Автоматическое проставление данных при добавлении нового судебного приказа
                if (claimCourtOrderElemsWrapper.find(".list-group-item").length > 1) {
                    var lastOrder = claimCourtOrderElemsWrapper.find(".list-group-item").last();
                    modal.find("input[id^='CourtOrder_AmountTenancy']").val($(lastOrder).find("input[id^='AmountTenancy']").val());
                    modal.find("input[id^='CourtOrder_AmountPenalties']").val($(lastOrder).find("input[id^='AmountPenalties']").val());
                    modal.find("input[id^='CourtOrder_AmountDgi']").val($(lastOrder).find("input[id^='AmountDgi']").val());
                    modal.find("input[id^='CourtOrder_AmountPadun']").val($(lastOrder).find("input[id^='AmountPadun']").val());
                    modal.find("input[id^='CourtOrder_AmountPkk']").val($(lastOrder).find("input[id^='AmountPkk']").val());
                    modal.find("input[id^='CourtOrder_StartDeptPeriod']").val($(lastOrder).find("input[id^='StartDeptPeriod']").val());
                    modal.find("input[id^='CourtOrder_EndDeptPeriod']").val($(lastOrder).find("input[id^='EndDeptPeriod']").val());
                    modal.find("input[id^='CourtOrder_OrderDate']").val($(lastOrder).find("input[id^='OrderDate']").val());
                    modal.find("input[id^='CourtOrder_OpenAccountDate']").val($(lastOrder).find("input[id^='OpenAccountDate']").val());
                    modal.find("select[id^='CourtOrder_IdSigner']").val($(lastOrder).find("input[id^='IdSigner']").val());
                    modal.find("select[id^='CourtOrder_IdJudge']").val($(lastOrder).find("input[id^='IdJudge']").val());
                } else {
                    modal.find("input[id^='CourtOrder_AmountTenancy']").val($("#Claim_AmountTenancy").val());
                    modal.find("input[id^='CourtOrder_AmountPenalties']").val($("#Claim_AmountPenalties").val());
                    modal.find("input[id^='CourtOrder_AmountDgi']").val($("#Claim_AmountDgi").val());
                    modal.find("input[id^='CourtOrder_AmountPadun']").val($("#Claim_AmountPadun").val());
                    modal.find("input[id^='CourtOrder_AmountPkk']").val($("#Claim_AmountPkk").val());
                    modal.find("input[id^='CourtOrder_StartDeptPeriod']").val($("#Claim_StartDeptPeriod").val());
                    modal.find("input[id^='CourtOrder_EndDeptPeriod']").val($("#Claim_EndDeptPeriod").val());
                }
                calculateAmountTotal(modal);
                modal.modal('show');
            }
        });
        e.preventDefault();
    }

    $('body').on('click', '.claim-court-order-open-btn, .claim-court-order-edit-btn', function (e) {
        var courtOrderElem = $(this).closest('.list-group-item');
        courtOrderElem.attr("data-processing", "create");
        var action = courtOrderElem.closest("[id^='ClaimCourtOrders']").data("action");
        var modal = courtOrderElem.closest("form").next("form[id^='ClaimCourtOrdersModalForm']").find("[id^='ClaimCourtOrderModal']");
        claimCourtOrderFillModal(courtOrderElem, modal, action);
        modal.modal('show');
        e.preventDefault();
    });

    $('body').on('click', '.claim-court-order-delete-btn', deleteClaimCourtOrder);
    $('body').on("click", "[id^='claimCourtOrderAdd']", addClaimCourtOrder);
});