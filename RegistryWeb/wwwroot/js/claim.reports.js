$(document).ready(function () {
    $("body").on('click', ".rr-report-request-to-bsk, .rr-report-memo-to-lawyers", function (e) {
        var idClaim = $(this).data("id-claim");
        var modal = $("#claimBksAndTransToLegalModal");
        modal.find("[name='Claim.IdClaim']").val(idClaim);
        modal.find("input, textarea, select").prop("disabled", false);

        var title = "Передача в юридический отдел";
        var dateTitle = "Дата передачи";
        var dateRequire = "Укажите дату передачи";
        modal.find("[name='Claim.IdReport']").val(1);
        if ($(this).hasClass("rr-report-request-to-bsk")) {
            var idReportBKSType = $(this).data("id-reportbkstype");
            switch (idReportBKSType)
            {
                case 1:
                    title = "Запрос в БКС";
                    modal.find("[name='Claim.IdReportBKSType']").val(1);
                    break;
                case 2:
                    title = "Запрос в БКС (с периодом расчета)";
                    modal.find("[name='Claim.IdReportBKSType']").val(2);
                    break;
            }
            dateTitle = "Дата запроса";
            dateRequire = "Укажите дату запроса";
            modal.find("[name='Claim.IdReport']").val(2);
        }
        modal.find(".modal-title").text(title);
        modal.find("label[for='Claim.Date']").text(dateTitle);
        modal.find("input[name='Claim.Date']").attr("data-val-required", dateRequire).attr("title", dateTitle);

        refreshValidationForm($("#claimBksAndTransToLegalForm"));

        modal.modal("show");
        e.preventDefault();
    });

    $("#claimBksAndTransToLegalModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var isValid = $(this).closest("#claimBksAndTransToLegalForm").valid();
        if (!isValid) {
            fixBootstrapSelectHighlight($(this).closest("#claimBksAndTransToLegalForm"));
            return false;
        }

        var idClaim = $("#claimBksAndTransToLegalModal").find("[name='Claim.IdClaim']").val();
        var idReport = $("#claimBksAndTransToLegalModal").find("[name='Claim.IdReport']").val();
        var idReportBKSType = $("#claimBksAndTransToLegalModal").find("[name='Claim.IdReportBKSType']").val();
        var idSigner = $("#claimBksAndTransToLegalModal").find("[name='Claim.IdSigner']").val();
        var date = $("#claimBksAndTransToLegalModal").find("[name='Claim.Date']").val();
        if ($("#claimBksAndTransToLegalModal").find(".input-validation-error").length > 0) {
            return false;
        }
        var url = undefined;
        switch (idReport) {
            case "1":
                url = "/ClaimReports/GetTransferToLegal?idClaim=" + idClaim + "&idSigner=" + idSigner + "&dateValue=" + date;
                break;
            case "2":
                url = "/ClaimReports/GetRequestToBks?idClaim=" + idClaim + "&idSigner=" + idSigner + "&dateValue=" + date + "&idReportBKSType=" + idReportBKSType;
                break;
        }

        if (url !== undefined) {
            downloadFile(url);
        }

        $("#claimBksAndTransToLegalModal").modal("hide");
    });

    $("body").on('click', ".rr-report-court-order-statement", function (e) {
        var idOrder = $(this).data("id-order");
        var idClaim = $("#Claim_IdClaim").val();
        url = "/ClaimReports/GetCourtOrderStatement?idOrder=" + idOrder + "&idClaim=" + idClaim;
        downloadFile(url);
        e.preventDefault();
    });

    $("body").on('click', ".rr-report-court-osp-statement", function (e) {
        var idClaim = $("#Claim_IdClaim").val();
        var createDate = $("#CreateDate").val();
        var idSigner = $("#IdSigner").val();
        url = "/ClaimReports/GetCourtOspStatement?idClaim=" + idClaim + "&createDate=" + createDate + "&idSigner=" + idSigner;
        downloadFile(url);
        $("#OspModal").modal('hide');
        e.preventDefault();
    });


    $("body").on('click', ".rr-claim-spi-btn", function (e) {
        var modal = $("#courtSpiStatementModal");
        modal.find("input, textarea, select").prop("disabled", false);
        modal.modal("show");
        e.preventDefault();
    });


    $("body").on('click', ".rr-spi-report-submit", function (e) {
        var idClaim = $("#Claim_IdClaim").val();
        var idCourtType = $("#courtSpiStatementModal").find("[name='ClaimState.CourtType']").val();
        url = "/ClaimReports/GetCourtSpiStatement?idClaim=" + idClaim + "&idCourtType=" +idCourtType;
        downloadFile(url);
        e.preventDefault();
    });


    $("#claimBksAndTransToLegalModal, #claimAddStateModal, #ClaimCommonReportModal").on("change", "select", function () {
        fixBootstrapSelectHighlightOnChange($(this));
    });

    /*      МУЛЬТИМАСТЕР        */

    $(".pagination .page-link").on("click", function (e) {
        var path = location.pathname;
        var page = $(this).data("page");
        location.href = path + "?PageOptions.CurrentPage=" + page;
        e.preventDefault();
    });

    $("#setDeptPeriod").on('click', function (e) {
        var modal = $("#claimDeptPeriodModal");
        modal.find("input, textarea, select").prop("disabled", false);
        modal.modal("show");
        e.preventDefault();
    });

    $("#claimDeptPeriodModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $(this).closest("form");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return false;
        }
        form.submit();
    });

    $("#exportBtn").on('click', function (e) {
        url = "/ClaimReports/GetClaimsExport";
        downloadFile(url);
        e.preventDefault();
    });


    $("#doverieBtn").on('click', function (e) {
        var modal = $("#courtSspStatementModal");
        modal.modal("show");
        e.preventDefault();
    });

    $("#courtSspStatementModal .rr-report-submit").on("click", function (e) {
        var statusSending = $(this).val();
        url = "/ClaimReports/GetClaimsForDoverie?statusSending=" + statusSending;
        downloadFile(url);
        $("#courtSspStatementModal").modal("hide");
    });

    $("#addClaimState").on('click', function (e) {
        var modal = $("#claimAddStateModal");
        modal.find("input, textarea, select").filter(function (idx, elem) {
            return $(elem).prop("name") !== "ClaimState.Executor";
        }).prop("disabled", false);
        modal.modal("show");
        e.preventDefault();
    });

    $("#claimAddStateModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $(this).closest("form");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return false;
        }
        
        $("#claimAddStateModal .rr-claim-ext-info").each(function (idx, elem) {
            if ($(elem).hasClass("d-none")) {
                $(elem).find("input, textarea, select").val("");
            }
        });

        form.submit();
    });

    $("#claimAddStateModal #ClaimState_IdStateType").on("change", function (e) {
        var idStateType = $(this).val() | 0;
        $("#claimAddStateModal .rr-claim-ext-info").each(function (idx, elem) {
            if ($(elem).data("id-state-type") === idStateType) {
                $(elem).removeClass("d-none");
            } else
                if (!$(elem).hasClass("d-none")) {
                    $(elem).addClass("d-none");
                }
        });
    });

    $("#claimAddStateModal #ClaimState_IdStateType").change();

    /* Отчеты ClaimReports/Index */

    $(".rr-claim-common-modal-report").on("click", function (e) {
        var modal = $("#ClaimCommonReportModal");
        var title = $(this).find(".media-body").text();
        modal.find(".modal-title").text(title);
        var action = $(this).data("action");
        modal.find("input[name='ActionUrl']").val(action);
        modal.find("input, textarea, select").prop("disabled", false);

        var standartWrapper = modal.find("input[name='StartDate']").closest(".form-row");
        var executorWrapper = modal.find("select[name='Executor']").closest(".form-row");
        var claimStateTypeWrapper = modal.find("select[name='IdStateType']").closest(".form-row");
        var isCurrentStateWrapper = modal.find("input[name='IsCurrentState']").closest(".rr-claim-common-report-checkbox");

        var factMailingWrapper = modal.find("select[name='Flag']").closest(".form-row");

        standartWrapper.hide();
        executorWrapper.hide();
        claimStateTypeWrapper.hide();
        isCurrentStateWrapper.hide();
        factMailingWrapper.hide();

        switch (action) {
            case "ClaimStatesReport":
            case "ClaimStatesAllDatesReport":
                standartWrapper.show();
                claimStateTypeWrapper.show();
                isCurrentStateWrapper.show();
                break;
            case "ClaimExecutorsReport":
                standartWrapper.show();
                executorWrapper.show();
                break;
            case "ClaimCourtReport":
                standartWrapper.show();
                break;
            case "ClaimEmergencyTariffReport":
                standartWrapper.show();
                break;
            case "ClaimFactMailing":
                factMailingWrapper.show();
                break;
            case "ClaimDateReferralCcoBailiffs":
                standartWrapper.show();
                break;
            case "ClaimExecutedWork":
                standartWrapper.show();
                break;        }

        modal.modal("show");
        e.preventDefault();
    });

    $("#ClaimCommonReportModal  .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $(this).closest("form");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return false;
        }

        var action = form.find("input[name='ActionUrl']").val();
        var startDate = form.find("input[name='StartDate']").val();
        var endDate = form.find("input[name='EndDate']").val();

        var url = "/ClaimReports/Get" + action + "?startDate=" + startDate + "&endDate=" + endDate;

        switch (action) {
            case "ClaimStatesReport":
            case "ClaimStatesAllDatesReport":
                var idStateType = form.find("select[name='IdStateType']").val();
                var isCurrentState = form.find("input[name='IsCurrentState']").is(":checked");
                url += "&idStateType=" + idStateType + "&isCurrentState=" + isCurrentState;
                break;
            case "ClaimExecutorsReport":
                var idExecutor = form.find("select[name='Executor']").val();
                url += "&idExecutor=" + idExecutor;
                break;
            case "ClaimFactMailing":
                startDate = form.find("[name='StDate_Year']").val() + "-" + form.find("[name='StDate_Month']").val() + "-01";
                endDate = form.find("[name='EndDate_Year']").val() + "-" + form.find("[name='EndDate_Month']").val() + "-01";
                
                url = "/ClaimReports/Get" + action + "?startDate=" + startDate + "&endDate=" + endDate;
                var flag = form.find("select[name='Flag']").val();
                url += "&flag=" + flag;
                break;
        }

        if (url !== undefined) {
            downloadFile(url);
        }

        $("#ClaimCommonReportModal").modal("hide");

    });

    $("#ClaimSplitAccountsReport").on("click", function (e) {
        url = "/ClaimReports/GetSplitAccountsReport";
        downloadFile(url);
        e.preventDefault();
    });

    $(".rr-payment-uk-invoice-modal-report").on("click", function (e) {
        var modal = $("#PaymentUkInvoiceReportModal");
        var title = $(this).find(".media-body").text();
        modal.find(".modal-title").text(title);
        var action = $(this).data("action");
        modal.find("input[name='ActionUrl']").val(action);
        modal.find("input, textarea, select").prop("disabled", false);

        var standartWrapper = modal.find("input[name='StartDate']").closest(".form-row");
        var ukWrapper = modal.find("select[name='Uk']").closest(".form-row");
        var balanceWrapper = modal.find("select[name='StDate_Month']").closest(".form-row");

        standartWrapper.hide();
        ukWrapper.hide();
        balanceWrapper.hide();

        switch (action) {
            case "UkInvoiceAgg":
            case "UkInvoiceDetails":
                ukWrapper.show();
                break;
            case "PaymentsForPeriod":
                standartWrapper.show();
                break;
            case "BalanceForPeriod":
                balanceWrapper.show();
                break;
        }

        modal.modal("show");
        e.preventDefault();
    });

    $("#PaymentUkInvoiceReportModal  .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $(this).closest("form");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return false;
        }

        var action = form.find("input[name='ActionUrl']").val();

        var url = "/ClaimReports/Get" + action + "?";
        switch (action) {
            case "UkInvoiceAgg":
            case "UkInvoiceDetails":
                var ids = form.find("[name='Uk']").val();
                var idsStr = "";
                for (var i = 0; i < ids.length; i++) {
                    idsStr += "idsOrganization="+ids[i];
                    if (i < ids.length - 1) {
                        idsStr += "&";
                    }
                }
                url += idsStr;
                break;
            case "PaymentsForPeriod":
                var startDate = form.find("input[name='StartDate']").val();
                var endDate = form.find("input[name='EndDate']").val();
                url += "startDate=" + startDate + "&endDate=" + endDate;
                break;
            case "BalanceForPeriod":
                startDate = form.find("[name='StDate_Year']").val() + "-" + form.find("[name='StDate_Month']").val() + "-01";
                url += "startDate=" + startDate;
                break;
        }

        if (url !== undefined) {
            downloadFile(url);
        }

        $("#PaymentUkInvoiceReportModal").modal("hide");

    });

    $("#UkInvoiceAggKumi").on("click", function (e) {
        url = "/ClaimReports/GetUkInvoiceAggKumi";
        downloadFile(url);
        e.preventDefault();
    });

    $("#UkInvoiceDetailsKumi").on("click", function (e) {
        url = "/ClaimReports/GetUkInvoiceDetailsKumi";
        downloadFile(url);
        e.preventDefault();
    });
});
