var timeline = undefined;
$(function () {
    var container = $('#PaymentsTimeline');
    var idAccount = container.data("id-account");
    var accountCount = container.data("account-count");
    var minDate = container.data("min-date");
    var maxDate = container.data("max-date");
 
    var items = $(".pa-item").map(function (idx, elem) {
        return {
            id: $(elem).data("id"),
            content: $(elem).data("date-display") +
                (accountCount > 1 ? "<br>ЛС " + $(elem).data("account") : ""),
            start: $(elem).data("date"),
            style: $(elem).data("id-account") === idAccount && accountCount > 1 ? "background-color: #BBFEE8; cursor: pointer" : "background-color: white; cursor: pointer",
            title:
                "<b>Сальдо (вх. → исх.):</b><br>" +
                "Найм: " + $(elem).data("input-tenancy") + " → " + $(elem).data("output-tenancy") + "руб.<br>" + 
                ($(elem).data("input-penalties") !== "0,00" || $(elem).data("output-penalties") !== "0,00" ? 
                    "Пени: " + $(elem).data("input-penalties") + " → " + $(elem).data("output-penalties") + "руб.<br>" : "") +
                ($(elem).data("input-dgi") !== "0,00" || $(elem).data("output-dgi") !== "0,00" ?
                    "ДГИ: " + $(elem).data("input-dgi") + " → " + $(elem).data("output-dgi") + "руб.<br>" : "") +
                ($(elem).data("input-padun") !== "0,00" || $(elem).data("output-padun") !== "0,00" ?
                    "Падун: " + $(elem).data("input-padun") + " → " + $(elem).data("output-padun") + "руб.<br>" : "") +
                ($(elem).data("input-pkk") !== "0,00" || $(elem).data("output-pkk") !== "0,00" ?
                    "ПКК: " + $(elem).data("input-pkk") + " → " + $(elem).data("output-pkk") + "руб.<br>" : "") +
            "<b>Итого</b>: " + $(elem).data("input-total") + " → " + $(elem).data("output-total") + "руб.",
            account: $(elem).data("account"),
            date: $(elem).data("date")
        };
    }).toArray();

    itemsDS = new vis.DataSet(items);
    var options = {
        locale: 'ru',
        zoomMin: 1000 * 60 * 60 * 24 * 30,
        zoomable: false,
        order: function (a, b) {
            if (a.date > b.date) return 1;
            if (a.date < b.date) return -1;
            if (a.account > b.account) return 1;
            if (a.account < b.account) return -1;
            return 0;
        },
        min: minDate,
        max: maxDate
    };
    timeline = new vis.Timeline(container[0], itemsDS, options);

    function selectPaymentItem(id) {
        $(".pa-item").hide();
        currentPaItem = $(".pa-item[data-id='" + id + "']");
        $("#PaymentAccount").val(currentPaItem.data("account"));
        $("#PaymentAccountGisZkh").val(currentPaItem.data("account-gis-zkh"));
        $("#PaymentCrn").val(currentPaItem.data("crn"));
        $("#PaymentRawAddress").val(currentPaItem.data("raw-address"));
        $("#PaymentTenant").val(currentPaItem.data("tenant"));
        $("#PaymentAddress").val(currentPaItem.data("address"));
        $("#PaymentTotalArea").val(currentPaItem.data("total-area"));
        $("#PaymentLivingArea").val(currentPaItem.data("living-area"));
        $("#PaymentPrescribed").val(currentPaItem.data("prescribed"));
        $("#PaymentDate").val(currentPaItem.data("date-display"));

        $("#PaymentBalanceInputTotal").val(currentPaItem.data("input-total"));
        $("#PaymentBalanceInputTenancy").val(currentPaItem.data("input-tenancy"));
        $("#PaymentBalanceInputPenalties").val(currentPaItem.data("input-penalties"));
        $("#PaymentBalanceInputDgi").val(currentPaItem.data("input-dgi"));
        $("#PaymentBalanceInputPadun").val(currentPaItem.data("input-padun"));
        $("#PaymentBalanceInputPkk").val(currentPaItem.data("input-pkk"));

        $("#PaymentChargingTotal").val(currentPaItem.data("charging-total"));
        $("#PaymentChargingTenancy").val(currentPaItem.data("charging-tenancy"));
        $("#PaymentChargingPenalties").val(currentPaItem.data("charging-penalties"));
        $("#PaymentChargingDgi").val(currentPaItem.data("charging-dgi"));
        $("#PaymentChargingPadun").val(currentPaItem.data("charging-padun"));
        $("#PaymentChargingPkk").val(currentPaItem.data("charging-pkk"));

        $("#PaymentTransferBalance").val(currentPaItem.data("transfer-balance"));
        $("#PaymentRecalcTenancy").val(currentPaItem.data("recalc-tenancy"));
        $("#PaymentRecalcPenalties").val(currentPaItem.data("recalc-penalties"));
        $("#PaymentRecalcDgi").val(currentPaItem.data("recalc-dgi"));
        $("#PaymentRecalcPadun").val(currentPaItem.data("recalc-padun"));
        $("#PaymentRecalcPkk").val(currentPaItem.data("recalc-pkk"));

        $("#PaymentTenancy").val(currentPaItem.data("payment-tenancy"));
        $("#PaymentPenalties").val(currentPaItem.data("payment-penalties"));
        $("#PaymentDgi").val(currentPaItem.data("payment-dgi"));
        $("#PaymentPadun").val(currentPaItem.data("payment-padun"));
        $("#PaymentPkk").val(currentPaItem.data("payment-pkk"));

        $("#PaymentBalanceOutputTotal").val(currentPaItem.data("output-total"));
        $("#PaymentBalanceOutputTenancy").val(currentPaItem.data("output-tenancy"));
        $("#PaymentBalanceOutputPenalties").val(currentPaItem.data("output-penalties"));
        $("#PaymentBalanceOutputDgi").val(currentPaItem.data("output-dgi"));
        $("#PaymentBalanceOutputPadun").val(currentPaItem.data("output-padun"));
        $("#PaymentBalanceOutputPkk").val(currentPaItem.data("output-pkk"));

        currentPaItem.show();
    }

    timeline.on("select", function (properties) {
        if (properties.items.length > 0) {
            selectPaymentItem(properties.items[0]);
        }
    });

    timeline.on("changed", function () {
        if (scale) {
            container.height(container.find(".vis-timeline").height());
            $("#PaymentsTimelineLoader").remove();
        }
    });

    var scale = false;
    var now = new Date();

    $(window).on('resize', function (idx, elem) {
        var coef = container.width() / 1110; // Коэффициент ширины контейнера относительно номинала
        timeline.setWindow(new Date(now.getTime() - 1000 * 60 * 60 * 24 * 210 * coef), now, {
            animation: { duration: 0.001 }
        }, function () {
            scale = true;
            var ids = timeline.getSelection();
            if (ids.length > 0) {
                timeline.focus(ids[0], { animation: false });
            } else {
                timeline.setSelection(items[0].id, { focus: true, animation: false });
                selectPaymentItem(items[0].id);
            }
        });
    });
    $(window).resize();

    $('.pa-toggler').each(function (idx, e) {
        $(e).on('click', $('#' + $(e).data("for")), elementToogleHide);
    });

});