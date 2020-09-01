$(document).ready(function () {
    $("body").on('click', ".rr-report-request-to-bsk", function (e) {
        var idAccount = $(this).data("id-account");
        var modal = $("#accountBksAndTransToLegalModal");
        modal.find("[name='Account.IdAccount']").val(idAccount);
        modal.find("input, textarea, select").prop("disabled", false);

        refreshValidationForm($("#accountBksAndTransToLegalForm"));

        modal.modal("show");
        e.preventDefault();
    });

    $("#accountBksAndTransToLegalModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var isValid = $(this).closest("#accountBksAndTransToLegalForm").valid();
        if (!isValid) {
            fixBootstrapSelectHighlight($(this).closest("#accountBksAndTransToLegalForm"));
            return false;
        }

        var idAccount = $("#accountBksAndTransToLegalModal").find("[name='Account.IdAccount']").val();
        var idSigner = $("#accountBksAndTransToLegalModal").find("[name='Account.IdSigner']").val();
        var date = $("#accountBksAndTransToLegalModal").find("[name='Account.Date']").val();
        if ($("#accountBksAndTransToLegalModal").find(".input-validation-error").length > 0) {
            return false;
        }
        var url = "/PaymentAccountReports/GetRequestToBks?idAccount=" + idAccount + "&idSigner=" + idSigner + "&dateValue=" + date;

        if (url !== undefined) {
            downloadFile(url);
        }

        $("#accountBksAndTransToLegalModal").modal("hide");
    });

    $("body").on('click', ".rr-report-calc-debt", function (e) {
        var idAccount = $(this).data("id-account");
        var modal = $("#accountCalcDeptModal");
        modal.find("[name='Account.IdAccount']").val(idAccount);
        modal.find("input, textarea, select").prop("disabled", false);

        refreshValidationForm($("#accountCalcDeptForm"));

        modal.modal("show");
        e.preventDefault();
    });

    $("#accountCalcDeptModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var isValid = $(this).closest("#accountCalcDeptForm").valid();
        if (!isValid) {
            fixBootstrapSelectHighlight($(this).closest("#accountCalcDeptForm"));
            return false;
        }

        var idAccount = $("#accountCalcDeptModal").find("[name='Account.IdAccount']").val();
        var dateFrom = $("#accountCalcDeptModal").find("[name='Account.DateFrom']").val();
        var dateTo = $("#accountCalcDeptModal").find("[name='Account.DateTo']").val();
        var fileFormat = 0;
        if ($("#accountCalcDeptModal #Account_FormatFile1").is(":checked")) {
            fileFormat = 1;
        }
        if ($("#accountCalcDeptModal #Account_FormatFile2").is(":checked")) {
            fileFormat = 2;
        }
        if ($("#accountCalcDeptModal").find(".input-validation-error").length > 0) {
            return false;
        }
        var url = "/PaymentAccountReports/GetCalDept?idAccount=" + idAccount + "&dateFrom=" + dateFrom + "&dateTo=" + dateTo + "&fileFormat=" + fileFormat;

        if (url !== undefined) {
            downloadFile(url);
        }

        $("#accountCalcDeptModal").modal("hide");
    });

    $("#accountBksAndTransToLegalModal, #accountCalcDeptModal").on("change", "select", function () {
        fixBootstrapSelectHighlightOnChange($(this));
    });
});
