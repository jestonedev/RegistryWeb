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

    $("body").on('click', ".rr-report-rig", function (e) {
        var idAccount = $(this).data("id-account");
        var modal = $("#accountRegInGenModal");
        modal.find("[name='Account.IdAccount']").val(idAccount);
        modal.find("input, textarea, select").prop("disabled", false);

        refreshValidationForm($("#accountRegInGenForm"));

        modal.modal("show");
        e.preventDefault();
    });    
        
    if (navigator.userAgent.indexOf('YaBrowser') == -1) {
        $("#dontforFirefox").css("display", "none");
        $("#forFirefox").css("display", "block");
    }
    else {
        $("#forFirefox").css("display", "none");
        $("#dontforFirefox").css("display", "block");
    }

    $("#accountRegInGenModal .rr-report-submit").on("click", function (e)
    {
        $("#accountRegInGenModal .rr-report-submit").prop("disabled", true);
        $("#gifforrig").css("display", "block");

        e.preventDefault();
        var isValid = $(this).closest("#accountRegInGenForm").valid();
        if (!isValid) {
            fixBootstrapSelectHighlight($(this).closest("#accountRegInGenForm"));
            return false;
        }

        var OnDate = null;
        if (navigator.userAgent.indexOf('YaBrowser') == -1)
            OnDate = $("#accountRegInGenForm").find("[name='Account.OnDate_Year']").val() + "-" + $("#accountRegInGenForm").find("[name='Account.OnDate_Month']").val();
        else
            OnDate = $("#accountRegInGenForm").find("[name='Account.OnDate']").val();
        

        if (OnDate==null)
            onDate = new Date().getFullYear()+"-"+new Date().getMonth()

        var idAccount = $("#accountRegInGenModal").find("[name='Account.IdAccount']").val();
        if (idAccount != undefined && idAccount != 0)
        {
            var codeError = null;
            $.ajax({
                type: 'POST',
                url: window.location.origin + "/PaymentAccountReports/InvoiceGenerator?idAccount=" + idAccount + "&OnDate=" + OnDate,
                dataType: 'json',
                data: { errorCode: codeError },
                success: function (data)
                {
                    $("#accountRegInGenModal .rr-report-submit").prop("disabled", false);
                    $("#gifforrig").css("display", "none");
                    switch (data.errorCode)
                    {
                        case 0: {
                            alert("Успешное выполнение");
                            break;
                        }
                        case -1: {
                            alert("Ошибка при сохранении qr - кода");
                            break;
                        }
                        case -2: {
                            alert("Ошибка сохранения html - файла");
                            break;
                        }
                        case -3: {
                            alert("Ошибка конвертации html в pdf");
                            break;
                        }
                        case -4: {
                            alert("Ошибка отправки сообщения");
                            break;
                        }
                        case -5: {
                            alert("Ошибка удаления временных файлов");
                            break;
                        }
                        case -6: {
                            alert("По данному ЛС отсутствует платеж на указанную дату");
                            break;
                        }
                        case -7: {
                            alert("Отсутствует электронная почта для отправки");
                            break;
                        }
                        case -8: {
                            alert("Недостаточно прав на данную операцию");
                            break;
                        }
                        case -9: {
                            alert("Требуется отладка для определения ошибки");
                            break;
                        }
                    }
                }
            });
        }
        else
        {
            $.ajax({
                type: 'POST',
                url: window.location.origin + "/PaymentAccountReports/InvoicesGenerator?OnDate=" + OnDate,
                dataType: 'json',
                success: function (data)
                {
                    $("#accountRegInGenModal .rr-report-submit").prop("disabled", false);
                    $("#gifforrig").css("display", "none");
                    //console.log(data);
                    var str = "<ul class='text-left'>";
                    var mas = []
                    for (let d in data)
                    {
                        if (data[d].length <= 8)
                            mas = data[d];
                        else
                        {
                            var dd=0;
                            for (dd=0; dd<8; dd++)                            
                                mas.push(data[d][dd]);                            
                        }

                        switch (d) {
                            case "0": {
                                str += "<li>Успешная отправка для ЛС: " + mas + ", ...<br>Всего: " + data[d].length + "</li>";
                                mas.length = 0;
                                break;
                            }
                            case "-1": {
                                str += "<li>Ошибка при сохранении qr - кода для ЛС: "+mas+", ...<br>Всего: "+data[d].length+"</li>";
                                mas.length = 0;
                                break;
                            }
                            case "-2": {
                                str += "<li>Ошибка сохранения html - файла для ЛС: "+mas+", ...<br>Всего: "+data[d].length+"</li>";
                                mas.length = 0;
                                break;
                            }
                            case "-3": {
                                str += "<li>Ошибка конвертации html в pdf для ЛС: "+mas+", ...<br>Всего: "+data[d].length+"</li>";
                                mas.length = 0;
                                break;
                            }
                            case "-4": {
                                str += "<li>Ошибка отправки сообщения для ЛС: "+mas+", ...<br>Всего: "+data[d].length+"</li>";
                                mas.length = 0;
                                break;
                            }
                            case "-5": {
                                str += "<li>Ошибка удаления временных файлов для ЛС: "+mas+", ...<br>Всего: "+data[d].length+"</li>";
                                mas.length = 0;
                                break;
                            }
                            case "-6": {
                                str += "<li>Отсутствие платежа на указанную дату для ЛС: "+mas+", ...<br>Всего: "+data[d].length+"</li>";
                                mas.length = 0;
                                break;
                            }
                            case "-7": {
                                str += "<li>Отсутствует электронная почта для ЛС: "+mas+", ...<br>Всего: "+data[d].length+"</li>";
                                mas.length = 0;
                                break;
                            }
                            case "-8": {
                                alert("Недостаточно прав на данную операцию");
                                break;
                            }
                            case "-9": {
                                alert("Требуется отладка для определения ошибки");
                                break;
                            }
                        }
                    }

                    str += "</ul>";
                    if (str != "<ul class='text-left'>")
                    {
                        $(".rr-errorsinv").css("display", "block");
                        $(".rr-errorsinv-item").html(str);
                    }
                    else
                        $("rr-errorsinv").css("display", "none");

                    $("#accountRegInGenModal").modal("hide");
                }
            });
        }
    });



});
