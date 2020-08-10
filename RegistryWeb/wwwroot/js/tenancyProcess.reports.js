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

    $("#preContractModal, #openDateModal").on("change", "select", function () {
        fixBootstrapSelectHighlightOnChange($(this));
    });
});
