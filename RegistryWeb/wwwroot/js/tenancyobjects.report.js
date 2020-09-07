$(document).ready(function () {
    $(".report").on("click", function (e) {
        var idReportType = +$(this).data("idreporttype");
        switch (idReportType) {
            case 0:
            case 1:
                $("#statisticModal").modal("show");
                $("#statisticModal").data("idreporttype", idReportType);
                break;
            case 2:
                $("#tenancyOrderModal").modal("show");
                break;
            case 3:
                $("#tenancyNotifiesListModal").modal("show");
                break;
        }

        e.preventDefault();
    });

    var resetModalForm = function (jQElem) {
        var form = jQElem.closest("form")[0];
        form.reset();
        var selectpickers = $(form).find(".selectpicker");
        selectpickers.selectpicker("deselectAll");
        selectpickers.selectpicker('refresh');
        $(form).find("fieldset").hide();
    }

    $(".close, .cancel").on("click", function (e) {
        resetModalForm($(this));
    });

    $("#statisticModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var idReportType = +$("#statisticModal").data("idreporttype");
        var idRegion = $("#statisticModal").find("[name='Statistic.Regions']").val();
        var idStreet = $("#statisticModal").find("[name='Statistic.Streets']").val();
        var house = $("#statisticModal").find("[name='Statistic.Houses']").val();
        var premisesNum = $("#statisticModal").find("[name='Statistic.PremisesNum']").val();
        var idRentType = $("#statisticModal").find("[name='Statistic.RentTypes']").val();
        var idTenancyReasonType = $("#statisticModal").find("[name='Statistic.TenancyReasonTypes']").val();
        var dateRegistrationFrom = $("#statisticModal").find("[name='Statistic.DateRegistrationFrom']").val();
        var dateRegistrationTo = $("#statisticModal").find("[name='Statistic.DateRegistrationTo']").val();
        var beginDateFrom = $("#statisticModal").find("[name='Statistic.BeginDateFrom']").val();
        var beginDateTo = $("#statisticModal").find("[name='Statistic.BeginDateTo']").val();
        var endDateFrom = $("#statisticModal").find("[name='Statistic.EndDateFrom']").val();
        var endDateTo = $("#statisticModal").find("[name='Statistic.EndDateTo']").val();
        var url = "/TenancyObjectsReports/GetTenancyStatistic?IdReportType=" + idReportType + "&IdRegion=" + idRegion + "&IdStreet=" + idStreet +
            "&House=" + house + "&PremisesNum=" + premisesNum + "&IdRentType=" + idRentType + "&IdTenancyReasonType=" + idTenancyReasonType +
            "&DateRegistrationFrom=" + dateRegistrationFrom + "&DateRegistrationTo=" + dateRegistrationTo + 
            "&BeginDateFrom=" + beginDateFrom + "&BeginDateTo=" + beginDateTo + 
            "&EndDateFrom=" + endDateFrom + "&EndDateTo=" + endDateTo;
        downloadFile(url);
        $("#statisticModal").modal("hide");
        resetModalForm($(this));
    });

    $("#tenancyOrderModal fieldset").hide();

    $("#TenancyOrder__OrderTypes").on("change", function (e) {
        var valueSelected = this.value;
        $("#tenancyOrderModal fieldset").hide();
        var showFieldsets = $("#tenancyOrderModal fieldset").filter(function (ind, el) {
            return $(el).data("selected-id") == valueSelected;
        });
        showFieldsets.show();
    });

    $("#tenancyOrderModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $("#tenancyOrderForm");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return false;
        }
        var idPreparer = $("#tenancyOrderModal").find("[name='TenancyOrder.Preparer']").val();
        var idLawyer = $("#tenancyOrderModal").find("[name='TenancyOrder.Lawyer']").val();
        var idStreet = $("#tenancyOrderModal").find("[name='TenancyOrder.Streets']").val();
        var house = $("#tenancyOrderModal").find("[name='TenancyOrder.Houses']").val();
        var premiseNum = $("#tenancyOrderModal").find("[name='TenancyOrder.PremisesNum']").val();
        var subPremiseNum = $("#tenancyOrderModal").find("[name='TenancyOrder.SubPremisesNum']").val();
        var orderDateFrom = $("#tenancyOrderModal").find("[name='TenancyOrder.OrderDateFrom']").val();
        var registrationDateFrom = $("#tenancyOrderModal").find("[name='TenancyOrder.RegistrationDateFrom']").val();
        var registrationDateTo = $("#tenancyOrderModal").find("[name='TenancyOrder.RegistrationDateTo']").val();
        var idRentType = $("#tenancyOrderModal").find("[name='TenancyOrder.RentTypes']").val();
        var idOrderType = $("#tenancyOrderModal").find("[name='TenancyOrder.OrderTypes']").val();
        var orphansNum = $("#tenancyOrderModal").find("[name='TenancyOrder.OrphansNum']").val();
        var orphansDate = $("#tenancyOrderModal").find("[name='TenancyOrder.OrphansDate']").val();
        var courtNum = $("#tenancyOrderModal").find("[name='TenancyOrder.CourtNum']").val();
        var courtDate = $("#tenancyOrderModal").find("[name='TenancyOrder.CourtDate']").val();
        var idCourt = $("#tenancyOrderModal").find("[name='TenancyOrder.CourtType']").val();
        var resettleNum = $("#tenancyOrderModal").find("[name='TenancyOrder.ResettleNum']").val();
        var resettleDate = $("#tenancyOrderModal").find("[name='TenancyOrder.ResettleDate']").val();
        var idResettleType = $("#tenancyOrderModal").find("[name='TenancyOrder.ResettleType']").val();
        var url = "/TenancyObjectsReports/GetTenancyOrder?IdPreparer=" + idPreparer + "&IdLawyer=" + idLawyer + "&IdStreet=" + idStreet +
            "&House=" + house + "&PremiseNum=" + premiseNum + "&SubPremiseNum=" + subPremiseNum + "&OrderDateFrom=" + orderDateFrom +
            "&RegistrationDateFrom=" + registrationDateFrom + "&RegistrationDateTo=" + registrationDateTo +
            "&IdRentType=" + idRentType + "&IdOrderType=" + idOrderType + "&OrphansNum=" + orphansNum + "&OrphansDate=" + orphansDate +
            "&CourtNum=" + courtNum + "&CourtDate=" + courtDate + "&IdCourt=" + idCourt + "&ResettleNum=" + resettleNum +
            "&ResettleDate=" + resettleDate + "&IdResettleType=" + idResettleType;
        downloadFile(url);
        $("#tenancyOrderModal").modal("hide");
        resetModalForm($(this));
    });

    $("#tenancyNotifiesListModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $("#tenancyNotifiesListForm");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return false;
        }
        var dateFrom = $("#tenancyNotifiesListModal").find("[name='TenancyNotifiesList.DateFrom']").val();
        var dateTo = $("#tenancyNotifiesListModal").find("[name='TenancyNotifiesList.DateTo']").val();
        var url = "/TenancyObjectsReports/GetTenancyNotifiesList?dateFrom=" + dateFrom + "&dateTo=" + dateTo;
        downloadFile(url);
        $("#tenancyNotifiesListModal").modal("hide");
        resetModalForm($(this));
    });

    $(".modal").on("hide.bs.modal", function () {
        $(this).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
        $(this).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");
    });
});