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
                return "Отсутствует адрес или начисление на указанную дату";
            case -7:
                return "Отсутствует электронная почта для отправки";
            case -8:
                return "Недостаточно прав на данную операцию";
            default:
                return "Неизвестная ошибка";
        }
    }

    var ids = [];
    var str = "";
    var accountsCount = 0;
    var output = [];
    $("#accountKumiRegInGenModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();

        var form = $(this).closest("#accountKumiRegInGenForm");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return false;
        }

        $("#accountKumiRegInGenModal .rr-report-submit").prop("disabled", true);
        $("#gifforrig").css("display", "inline");

        var onDate = form.find("[name='AccountKumi.OnDate_Year']").val() + "-" + $("#accountKumiRegInGenForm").find("[name='AccountKumi.OnDate_Month']").val() + "-01";
        var idAccount = form.find("[name='AccountKumi.IdAccount']").val();
        var textmessage = form.find("[name='AccountKumi.TextMessage']").val();
        var action = form.find("[name='AccountKumi.Action']").val();
        var controllerAction = "InvoiceToHtmlList";

        if (action === "Send" || (idAccount !== "" && idAccount !== "0"))
            controllerAction = "InvoiceGenerator";

        if (action === "Send") {
            if (idAccount !== "" && idAccount !== "0") {
                initInvoiceGeneratorsProgress(1);
                accountsCount = 1;
                invoiceGenerator([idAccount], [], controllerAction, onDate, action, textmessage);
            }
            else {
                ids = getAccountIds();
                initInvoiceGeneratorsProgress(ids.length);
                var accountIdsForInvoiceGenerator = ids.slice(0, 10);
                var accountIdsOther = ids.slice(10);
                accountsCount = ids.length;
                invoiceGenerator(accountIdsForInvoiceGenerator, accountIdsOther, controllerAction, onDate, action, textmessage);
            }
        } else {
            $("#accountKumiRegInGenModal .rr-report-submit").prop("disabled", false);
            $("#gifforrig").css("display", "none");
            $("#accountKumiRegInGenModal").modal("hide");
            downloadFile(url);
        }
    });

    function getAccountIds() {
        var result = [];
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/KumiAccounts/GetSessionIds',
            dataType: 'json',
            async: false,
            success: function (data) {
                result = data;
            }
        });
        return result;
    }

    $(".rr-report-recalc").on("click", function (e) {
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

    function invoiceGenerator(accountIdsForInvoiceGenerator, accountIdsOther, controllerAction, onDate, action, textmessage)
    {
        $.ajax({
            type: 'POST',
            url: window.location.origin + "/KumiAccountReports/" + controllerAction,
            data: { idAccounts: accountIdsForInvoiceGenerator, onDate: onDate, invoiceAction: action, textmessage:textmessage},
            dataType: 'json',
            success: function (data)
            {
                setinvoiceGeneratorsProgress(progressCurrentValue + accountIdsForInvoiceGenerator.length);
                $("#accountKumiRegInGenModal .rr-report-submit").prop("disabled", false);

                if (data.errorCode !== undefined)
                    alert(invoiceGeneratorCodeToErrorText(data.errorCode | 0));

                if (accountsCount == 1) {
                    for (let code in data.results) {
                        alert(invoiceGeneratorCodeToErrorText(code | 0));
                        break;
                    }
                    $("#accountKumiRegInGenModal").modal("hide");
                }
                else {
                    for (let code in data.results)
                    {
                        var accounts = [];
                        if (data.results[code].length <= 8)
                            accounts = data.results[code];
                        else
                            for (var i = 0; i < 8; i++)
                                accounts.push(data.results[code][i]);

                        switch (code)
                        {
                            case "0": case "-1": case "-2": case "-3":
                            case "-4": case "-5": case "-6": case "-7":
                                let codeOut = {
                                    code: code,
                                    accounts: data.results[code],
                                    count: data.results[code].length
                                };
                                output.push(codeOut);
                                break;
                            default:
                                alert(invoiceGeneratorCodeToErrorText(code | 0));
                                break;
                        }
                    }
                }

                if (accountIdsOther.length > 0)
                {
                    accountIdsForInvoiceGenerator = accountIdsOther.slice(0, 10);
                    accountIdsOther = accountIdsOther.slice(10);
                    invoiceGenerator(accountIdsForInvoiceGenerator, accountIdsOther, controllerAction, onDate, action, textmessage);
                }
                else
                {
                    if (accountsCount > 1) 
                        resultForIG(accountsCount);

                    output = [];
                    var form = $("#accountKumiRegInGenForm");
                    $("#gifforrig").css("display", "none");
                    form.find(".progress").addClass("d-none");
                    $("#accountKumiRegInGenModal").modal("hide");
                }
            }
        });
    }

    var progressMaxValue = 0;
    var progressCurrentValue = 0;
    function initInvoiceGeneratorsProgress(maxValue)
    {
        var form = $("#accountKumiRegInGenForm");
        var progress = form.find(".progress");
        progress.removeClass("d-none");
        progressMaxValue = maxValue;
        progressCurrentValue = 0;
        var progressBar = progress.find(".progress-bar");
        progressBar.css("width", "0%");
        progressBar.attr("aria-valuenow", "0");
    }

    function setinvoiceGeneratorsProgress(value)
    {
        progressCurrentValue = value;
        var currentPosition = progressMaxValue;
        if (progressMaxValue > 0)
            currentPosition = Math.round(progressCurrentValue / progressMaxValue * 100);

        var form = $("#accountKumiRegInGenForm");
        var progress = form.find(".progress");
        var progressBar = progress.find(".progress-bar");
        progressBar.css("width", currentPosition + "%");
        progressBar.attr("aria-valuenow", currentPosition);
    }

    function resultForIG(accountsLen)
    {
        str = "<ul class='text-left'>";
        var resultList = [];

        var difCodeList = output.filter((val, index, self) => index === self.findIndex((t) => (t.code === val.code)));

        var codeList = [];
        difCodeList.forEach((elem) => { codeList.push(elem.code); });

        codeList.forEach(function (elem) {
            var list = output.filter(v => v.code === elem);

            var sumAccounts = 0;
            var accountsList = [];
            $.each(list, function (key, val) {
                sumAccounts += parseInt(this.count);
                val.accounts.forEach(function (el) { accountsList.push(el); });
            });
            accountsList = accountsList.filter((val, index, self) => index < 8);

            resultList.push({ code: elem, accounts: accountsList, count: sumAccounts });
        });

        resultList.forEach(function (elem) {
            str += "<li>" + invoiceGeneratorCodeToErrorText(elem.code | 0) + ": " + elem.accounts +
                (elem.count > 8 ? ", ..." : "") + "<br>Всего: " + elem.count + "</li>";
        });

        if(resultList.length>1)
            str += "<b>Всего обработанных ЛС: " + accountsLen+"</b>";

        str += "</ul>";
        if (str !== "<ul class='text-left'></ul>") {
            $(".rr-errorsinv").css("display", "block");
            $(".rr-errorsinv-item").html(str);
        }
        else
            $("rr-errorsinv").css("display", "none");

        str = "";
    }

});
