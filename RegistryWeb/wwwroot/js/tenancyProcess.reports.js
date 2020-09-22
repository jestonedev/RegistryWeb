$(document).ready(function () {
    $("body").on('click', ".rr-report-pre-contract", function (e) {
        var idProcess = $(this).data("id-process");
        $("#preContractModal").find("[name='PreContract.IdProcess']").val(idProcess);
        $("#preContractModal").find("input, textarea, select").prop("disabled", false);
        $("#preContractModal").modal("show");
        e.preventDefault();
    });

    $("#preContractModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var isValid = $(this).closest("#preContractForm").valid();
        if (!isValid) {
            fixBootstrapSelectHighlight($(this).closest("#preContractForm"));
            return false;
        }

        var idProcess = $("#preContractModal").find("[name='PreContract.IdProcess']").val();
        var idPreamble = $("#preContractModal").find("[name='PreContract.IdPreamble']").val();
        var idCommittee = $("#preContractModal").find("[name='PreContract.IdCommittee']").val();
        if ($("#preContractModal").find(".input-validation-error").length > 0) {
            return false;
        }

        var url = "/TenancyReports/GetPreContract?idProcess=" + idProcess + "&idPreamble=" + idPreamble
            + "&idCommittee=" + idCommittee;

        if (url !== undefined) {
            downloadFile(url);
        }

        $("#preContractModal").modal("hide");
    });

    $("body").on('click', ".rr-report-dksr-contract", function (e) {
        var idProcess = $(this).data("id-process");
        url = "/TenancyReports/GetDksrContract?idProcess=" + idProcess;
        downloadFile(url);
        e.preventDefault();
    });

    $("body").on('click', ".rr-report-contract", function (e) {
        var idProcess = $(this).data("id-process");
        var contractType = $(this).data("contract-type");
        var idRentType = $(this).data("id-rent-type");
        $("#openDateModal").find("[name='OpenDate.IdProcess']").val(idProcess);
        $("#openDateModal").find("[name='OpenDate.ReportType']").val("Contract");
        $("#openDateModal").find("[name='OpenDate.IdRentType']").val(idRentType);
        $("#openDateModal").find("[name='OpenDate.ContractType']").val(contractType);
        $("#openDateModal").find(".modal-title").text("Договор");
        $("#openDateModal").modal("show");
        e.preventDefault();
    });

    $("body").on('click', ".rr-report-act-to-tenant", function (e) {
        var idProcess = $(this).data("id-process");
        $("#openDateModal").find("[name='OpenDate.IdProcess']").val(idProcess);
        $("#openDateModal").find("[name='OpenDate.ReportType']").val("ActToTenant");
        $("#openDateModal").find("[name='OpenDate.IdRentType']").val("");
        $("#openDateModal").find("[name='OpenDate.ContractType']").val("");
        $("#openDateModal").find(".modal-title").text("Акт передачи в найм");
        $("#openDateModal").modal("show");
        e.preventDefault();
    });

    $("body").on('click', ".rr-report-act-from-tenant", function (e) {
        var idProcess = $(this).data("id-process");
        $("#openDateModal").find("[name='OpenDate.IdProcess']").val(idProcess);
        $("#openDateModal").find("[name='OpenDate.ReportType']").val("ActFromTenant");
        $("#openDateModal").find("[name='OpenDate.IdRentType']").val("");
        $("#openDateModal").find("[name='OpenDate.ContractType']").val("");
        $("#openDateModal").find(".modal-title").text("Акт приема из найма");
        $("#openDateModal").modal("show");
        e.preventDefault();
    });

    $("#openDateModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var idProcess = $("#openDateModal").find("[name='OpenDate.IdProcess']").val();
        var reportType = $("#openDateModal").find("[name='OpenDate.ReportType']").val();
        var idRentType = $("#openDateModal").find("[name='OpenDate.IdRentType']").val();
        var contractType = $("#openDateModal").find("[name='OpenDate.ContractType']").val();
        var openDate = $(this).data("val") === 1 ? true : false;

        var url = "/TenancyReports/Get" + reportType + "?idProcess=" + idProcess + "&openDate=" + openDate;
        if (reportType === "Contract") {
            url += "&contractType=" + contractType + "&idRentType="+idRentType;
        }

        if (url !== undefined) {
            downloadFile(url);
        }

        $("#openDateModal").modal("hide");
    });

    $("body").on('click', ".rr-report-agreement", function (e) {
        var idAgreement = $(this).data("id-agreement");
        url = "/TenancyReports/GetAgreement?idAgreement=" + idAgreement;
        downloadFile(url);
        e.preventDefault();
    });

    $("body").on('click', ".rr-report-excerpt", function (e) {
        var idProcess = $(this).data("id-process");
        var reportType = $(this).data("report-type");
        var reportTitle = $(this).text();
        url = "/TenancyReports/GetNotifySingleDocument?idProcess=" + idProcess + "&reportType=" + reportType + "&reportTitle="+reportTitle;
        downloadFile(url);
        e.preventDefault();
    });

    $("body").on('click', ".rr-report-mvd", function (e) {
        var idProcess = $(this).data("id-process");
        var requestType = $(this).data("request-type");
        url = "/TenancyReports/GetRequestToMvd?idProcess=" + idProcess + "&requestType=" + requestType;
        downloadFile(url);
        e.preventDefault();
    });

    $("#preContractModal, #openDateModal, #tenancyWarningModal, #tenancyReasonModal").on("change", "select", function () {
        fixBootstrapSelectHighlightOnChange($(this));
    });

    /*      МУЛЬТИМАСТЕР        */

    $(".pagination .page-link").on("click", function (e) {
        var path = location.pathname;
        var page = $(this).data("page");
        location.href = path + "?PageOptions.CurrentPage=" + page;
        e.preventDefault();
    });

    $("body").on('click', "#notifiesPrimaryBtn", function (e) {
        downloadFile("/TenancyReports/GetNotifiesPrimary");
        e.preventDefault();
    });

    $("body").on('click', "#notifiesSecondaryBtn", function (e) {
        downloadFile("/TenancyReports/GetNotifiesSecondary");
        e.preventDefault();
    });

    $("body").on('click', "#notifiesProlongContractBtn", function (e) {
        downloadFile("/TenancyReports/GetNotifiesProlongContract");
        e.preventDefault();
    });

    $("body").on('click', "#notifiesEvictionFromEmergencyFundBtn", function (e) {
        downloadFile("/TenancyReports/GetNotifiesEvictionFromEmergencyFund");
        e.preventDefault();
    });

    $("body").on('click', "#requestToMvdBtn, #requestToMvdNewBtn", function (e) {
        var requestType = $(this).data("request-type");
        downloadFile("/TenancyReports/GetRequestToMvd?requestType=" + requestType);
        e.preventDefault();
    });

    $("body").on('click', "#tenancyWarningBtn", function (e) {
        $('#tenancyWarningModal').modal('toggle');
        e.preventDefault();
    });

    $("body").on('click', "#tenancyContractRegDateBtn", function (e) {
        $('#tenancyContractRegDateModal').modal('toggle');
        e.preventDefault();
    });

    $("body").on('click', "#tenancyReasonBtn", function (e) {
        $('#tenancyReasonModal').modal('toggle');
        e.preventDefault();
    });

    $("body").on('click', "#exportReasonsForGisZkhBtn", function (e) {
        downloadFile("/TenancyReports/GetExportReasonsForGisZkh");
        e.preventDefault();
    });

    $("body").on('click', "#gisZkhExportBtn", function (e) {
        downloadFile("/TenancyReports/GetGisZkhExport");
        e.preventDefault();
    });

    $(".info").hide();

    $("#tenancyWarningModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $(this).closest("#tenancyWarningForm");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return false;
        }
        var idPreparer = $("#tenancyWarningModal").find("[name='TenancyWarning.IdPreparer']").val();
        var isMultipageDocument = null;
        if ($("#tenancyWarningModal").find("[name='TenancyWarning.IsMultipageDocument']").is(':checked')) {
            isMultipageDocument = "True";
        } else {
            isMultipageDocument = "False";
        }

        var url = "/TenancyReports/GetTenancyWarning?idPreparer=" + idPreparer + "&isMultipageDocument=" + isMultipageDocument;

        if (url !== undefined) {
            downloadFile(url);
        }

        $("#tenancyWarningModal").modal("hide");
    });

    $("#tenancyContractRegDateModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $(this).closest("#tenancyContractRegDateForm");
        var isValid = form.valid();
        if (!isValid) {
            return false;
        }
        var regDate = $("#tenancyContractRegDateModal").find("[name='TenancyContractRegDate.RegDate']").val();
        $.ajax({
            type: 'GET',
            url: window.location.origin + '/TenancyReports/SetTenancyContractRegDate?regDate=' + regDate,
            success: function (data) {
                console.log(data);
                $('.info').text(data);
                $("#tenancyContractRegDateModal").modal("hide");
            }
        });
    });

    $("#tenancyReasonModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $(this).closest("#tenancyReasonForm");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return false;
        }
        var reasonNumber = $("#tenancyReasonModal").find("[name='TenancyReason.ReasonNumber']").val();
        var reasonDate = $("#tenancyReasonModal").find("[name='TenancyReason.ReasonDate']").val();
        var idReasonType = $("#tenancyReasonModal").find("[name='TenancyReason.IdReasonType']").val();
        var isDeletePrevReasons = null;
        if ($("#tenancyReasonModal").find("[name='TenancyReason.IsDeletePrevReasons']").is(':checked')) {
            isDeletePrevReasons = "True";
        } else {
            isDeletePrevReasons = "False";
        }
        $.ajax({
            type: 'GET',
            url: window.location.origin + '/TenancyReports/SetTenancyReason?reasonNumber=' + reasonNumber +
                '&reasonDate=' + reasonDate + '&idReasonType=' + idReasonType + '&isDeletePrevReasons=' + isDeletePrevReasons,
            success: function (data) {
                console.log(data);
                $('.info').text(data);
                $("#tenancyReasonModal").modal("hide");
            }
        });
    });

    $("#tenanciesExportBtn").on('click', function (e) {
        url = "/TenancyReports/GetTenanciesExport";
        downloadFile(url);
        e.preventDefault();
    });
});
