$(document).ready(function () {

    $("body").on('click', ".rr-report-calc-debt", function (e) {
        var idAccount = $(this).data("id-account");
        var modal = $("#accountKumiCalcDeptModal");
        modal.find("[name='AccountKumi.IdAccount']").val(idAccount);
        modal.find("input, textarea, select").prop("disabled", false);

        refreshValidationForm($("#accountKumiCalcDeptForm"));

        modal.modal("show");
        e.preventDefault();
    });


    $("#accountKumiCalcDeptModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var isValid = $(this).closest("#accountKumiCalcDeptForm").valid();
        if (!isValid) {
            fixBootstrapSelectHighlight($(this).closest("#accountKumiCalcDeptForm"));
            return false;
        }
        var idAccount = $("#accountKumiCalcDeptModal").find("[name='AccountKumi.IdAccount']").val();
        var dateFrom = $("#accountKumiCalcDeptModal").find("[name='AccountKumi.DateFrom']").val();
        var dateTo = $("#accountKumiCalcDeptModal").find("[name='AccountKumi.DateTo']").val();
        var fileFormat = 0;
        if ($("#accountKumiCalcDeptModal #AccountKumi_FormatFile1").is(":checked")) {
            fileFormat = 1;
        }
        if ($("#accountKumiCalcDeptModal #AccountKumi_FormatFile2").is(":checked")) {
            fileFormat = 2;
        }
        if ($("#accountKumiCalcDeptModal").find(".input-validation-error").length > 0) {
            return false;
        }
        var url = "/KumiAccountReports/GetCalDept?idAccount=" + idAccount + "&dateFrom=" + dateFrom + "&dateTo=" + dateTo + "&fileFormat=" + fileFormat;

        if (url !== undefined) {
            downloadFile(url);
        }

        $("#accountKumiCalcDeptModal").modal("hide");
    });


    $("body").on('click', ".rr-report-rig-send, .rr-report-rig-export", function (e) {
        var idAccount = $(this).data("id-account");
        var modal = $("#accountKumiRegInGenModal");
        modal.find("[name='AccountKumi.IdAccount']").val(idAccount);
        modal.find("input, textarea, select").prop("disabled", false);

        if ($(this).hasClass("rr-report-rig-send")) {
            modal.find(".modal-title").text("Отправить счет-извещение");
            modal.find(".rr-report-submit").text("Отправить");
            modal.find("[name='AccountKumi.Action']").val("Send");
            modal.find("[name='AccountKumi.TextMessage']").closest(".form-group").css("display", "block");
        }
        else {
            modal.find(".modal-title").text("Сформировать счет-извещение");
            modal.find(".rr-report-submit").text("Сформировать");
            modal.find("[name='AccountKumi.Action']").val("Export");
            modal.find("[name='AccountKumi.TextMessage']").closest(".form-group").css("display", "none");
        }

        refreshValidationForm($("#accountKumiRegInGenForm"));
        modal.modal("show");
        e.preventDefault();
    });


});
