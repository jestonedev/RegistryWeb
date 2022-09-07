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
        var lastChargeYear = $(this).data("last-charge-year");
        var lastChargeMonth = $(this).data("last-charge-month");
        var modal = $("#accountKumiRegInGenModal");
        modal.find("[name='AccountKumi.IdAccount']").val(idAccount);
        modal.find("input, textarea, select").prop("disabled", false);
        modal.find("[name='AccountKumi.OnDate_Year']").val(lastChargeYear).selectpicker('refresh');
        modal.find("[name='AccountKumi.OnDate_Month']").val(lastChargeMonth).selectpicker('refresh');

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

    function invoiceGeneratorCodeToErrorText(errorCode) {
        switch (errorCode) {
            case 0:
                return "Отправка выполнена успешно";
            case -1:
                return "Ошибка при сохранении qr - кода";
            case -2:
                return "Ошибка сохранения html - файла";
            case -3:
                return "Ошибка конвертации html в pdf";
            case -4:
                return "Ошибка отправки сообщения";
            case -5:
                return "Ошибка удаления временных файлов";
            case -6:
                return "Отсутствует платеж на указанную дату";
            case -7:
                return "Отсутствует электронная почта для отправки";
            case -8:
                return "Недостаточно прав на данную операцию";
            default:
                return "Неизвестная ошибка";
        }
    }

    $("#accountKumiRegInGenModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();

        var form = $(this).closest("#accountKumiRegInGenForm");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return false;
        }

        $("#accountKumiRegInGenModal .rr-report-submit").prop("disabled", true);
        $("#gifforrig").css("display", "block");

        var onDate = form.find("[name='AccountKumi.OnDate_Year']").val() + "-" + $("#accountKumiRegInGenForm").find("[name='AccountKumi.OnDate_Month']").val() + "-01";
        var idAccount = form.find("[name='AccountKumi.IdAccount']").val();
        var textmessage = form.find("[name='AccountKumi.TextMessage']").val();
        var action = form.find("[name='AccountKumi.Action']").val();
        var url = window.location.origin + "/KumiAccountReports/InvoiceGenerator?onDate=" + onDate + "&invoiceAction=" + action + "&textmessage=" + textmessage;
        if (idAccount !== "" && idAccount !== "0") {
            url += "&idAccount=" + idAccount;
        }

        if (action === "Send") {
            if (idAccount !== "" && idAccount !== "0") {
                $.ajax({
                    type: 'POST',
                    url: url,
                    dataType: 'json',
                    success: function (data) {
                        $("#accountKumiRegInGenModal .rr-report-submit").prop("disabled", false);
                        $("#gifforrig").css("display", "none");
                        $("#accountRegInGenModal").modal("hide");
                        if (data.errorCode !== undefined)
                            alert(invoiceGeneratorCodeToErrorText(data.errorCode | 0));
                        for (let code in data.results) {
                            alert(invoiceGeneratorCodeToErrorText(code | 0));
                            return;
                        }
                    }
                });
            }
            else {
                $.ajax({
                    type: 'POST',
                    url: url,
                    dataType: 'json',
                    success: function (data) {
                        $("#accountKumiRegInGenModal .rr-report-submit").prop("disabled", false);
                        $("#gifforrig").css("display", "none");
                        var str = "<ul class='text-left'>";
                        if (data.errorCode !== undefined) {
                            str += invoiceGeneratorCodeToErrorText(data.errorCode);
                        } else {
                            for (let code in data.results) {
                                var accounts = [];
                                if (data.results[code].length <= 8)
                                    accounts = data.results[code];
                                else {
                                    for (var i = 0; i < 8; i++)
                                        accounts.push(data.results[accounts][i]);
                                }

                                switch (code) {
                                    case "0": case "-1": case "-2": case "-3":
                                    case "-4": case "-5": case "-6": case "-7":
                                        str += "<li>" + invoiceGeneratorCodeToErrorText(code | 0) + ": " + accounts +
                                            (data.results[code].length > 8 ? ", ..." : "") + "<br>Всего: " + data.results[code].length + "</li>";
                                        break;
                                    default:
                                        alert(invoiceGeneratorCodeToErrorText(code | 0));
                                        break;
                                }
                            }
                        }

                        str += "</ul>";
                        if (str !== "<ul class='text-left'></ul>") {
                            $(".rr-errorsinv").css("display", "block");
                            $(".rr-errorsinv-item").html(str);
                        }
                        else
                            $("rr-errorsinv").css("display", "none");

                        $("#accountKumiRegInGenModal").modal("hide");
                    }
                });
            }
        } else {
            $("#accountKumiRegInGenModal .rr-report-submit").prop("disabled", false);
            $("#gifforrig").css("display", "none");
            $("#accountKumiRegInGenModal").modal("hide");
            downloadFile(url);
        }
    });    $(".rr-report-recalc").on("click", function (e) {
        var modal = $("#accountRecalcModal");
        modal.find("input[name='AccountKumiRecalc.IdAccount']").val("");
        modal.find("select, input").prop('disabled', false);
        modal.modal('show');
        e.preventDefault();
    });

    $(".pagination .page-link").on("click", function (e) {
        var path = location.pathname;
        var page = $(this).data("page");
        location.href = path + "?PageOptions.CurrentPage=" + page;
        e.preventDefault();
    });

    $("#exportBtn").on('click', function (e) {
        url = "/KumiAccountReports/GetAccountsExport";
        downloadFile(url);
        e.preventDefault();
    });

    $("#addClaim").on('click', function (e) {
        var modal = $("#createClaimModal");
        modal.find("input, textarea, select").prop("disabled", false);
        modal.modal("show");
        e.preventDefault();
    });

    $("#createClaimModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $(this).closest("#createClaimForm");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return false;
        }

        form.submit();
    });

    $(".rr-get-act-charge").on('click', function (e) {

        var idAccount = $(this).data("id-account");
        var modal = $("#actChargeModal");
        modal.find("[name='IdAccount']").val(idAccount);

        modal.find("input, textarea, select").prop("disabled", false);
        modal.modal("show");
        e.preventDefault();
    });

    $("#actChargeModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $(this).closest("#actChargeForm");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return false;
        }

        form.submit();
        $("#actChargeModal").modal("hide");

    });
});
