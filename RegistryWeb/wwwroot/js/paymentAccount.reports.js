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

    $("#accountBksAndTransToLegalModal, #accountCalcDeptModal, #accountRegInGenForm").on("change", "select", function () {
        fixBootstrapSelectHighlightOnChange($(this));
    });

    /*      МУЛЬТИМАСТЕР        */

    $(".pagination .page-link").on("click", function (e) {
        var path = location.pathname;
        var page = $(this).data("page");
        location.href = path + "?PageOptions.CurrentPage=" + page;
        e.preventDefault();
    });

    $("#exportBtn").on('click', function (e) {
        url = "/PaymentAccountReports/GetPaymentsExport";
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


//______________________________________________

    $("body").on('click', ".rr-report-rig-send, .rr-report-rig-export", function (e) {
        var idAccount = $(this).data("id-account");
        var modal = $("#accountRegInGenModal");
        modal.find("[name='Account.IdAccount']").val(idAccount);
        modal.find("input, textarea, select").prop("disabled", false);

        if ($(this).hasClass("rr-report-rig-send")) {
            modal.find(".modal-title").text("Отправить счет-извещение");
            modal.find(".rr-report-submit").text("Отправить");
            modal.find("[name='Account.Action']").val("Send");
        }
        else {
            modal.find(".modal-title").text("Сформировать счет-извещение");
            modal.find(".rr-report-submit").text("Сформировать");
            modal.find("[name='Account.Action']").val("Export");
        }

        refreshValidationForm($("#accountRegInGenForm"));
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

    $("#accountRegInGenModal .rr-report-submit").on("click", function (e)
    {
        e.preventDefault();

        var form = $(this).closest("#accountRegInGenForm");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return false;
        }

        $("#accountRegInGenModal .rr-report-submit").prop("disabled", true);
        $("#gifforrig").css("display", "block");

        var onDate = form.find("[name='Account.OnDate_Year']").val() + "-" + $("#accountRegInGenForm").find("[name='Account.OnDate_Month']").val() + "-01";
        var idAccount = form.find("[name='Account.IdAccount']").val();
        var action = form.find("[name='Account.Action']").val();
        var url = window.location.origin + "/PaymentAccountReports/InvoiceGenerator?onDate=" + onDate + "&invoiceAction=" + action;
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
                        $("#accountRegInGenModal .rr-report-submit").prop("disabled", false);
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
                        $("#accountRegInGenModal .rr-report-submit").prop("disabled", false);
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

                        $("#accountRegInGenModal").modal("hide");
                    }
                });
            }
        } else {
            $("#accountRegInGenModal .rr-report-submit").prop("disabled", false);
            $("#gifforrig").css("display", "none");
            $("#accountRegInGenModal").modal("hide");
            downloadFile(url);
        }
    });



});
