﻿$(document).ready(function () {
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
            title = "Запрос в БКС";
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
                url = "/ClaimReports/GetRequestToBks?idClaim=" + idClaim + "&idSigner=" + idSigner + "&dateValue=" + date;
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

    $(".rr-claim-common-modal-report").on("click", function (e) {
        var modal = $("#ClaimCommonReportModal");
        var title = $(this).find(".media-body").text();
        modal.find(".modal-title").text(title);
        var action = $(this).data("action");
        modal.find("input[name='ActionUrl']").val(action);
        modal.find("input, textarea, select").prop("disabled", false);

        var executorWrapper = modal.find("select[name='Executor']").closest(".form-row");
        var claimStateTypeWrapper = modal.find("select[name='IdStateType']").closest(".form-row");
        var isCurrentStateWrapper = modal.find("input[name='IsCurrentState']").closest(".rr-claim-common-report-checkbox");

        executorWrapper.hide();
        claimStateTypeWrapper.hide();
        isCurrentStateWrapper.hide();

        switch (action) {
            case "ClaimStatesReport":
                executorWrapper.show();
                break;
            case "ClaimExecutorsReport":
                claimStateTypeWrapper.show();
                isCurrentStateWrapper.show();
                break;
        }

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
                var idExecutor = form.find("select[name='Executor']").val();
                url += "&idExecutor=" + idExecutor;
                break;
            case "ClaimExecutorsReport":
                var idStateType = form.find("select[name='IdStateType']").val();
                var isCurrentState = form.find("input[name='IsCurrentState']").is(":checked");
                url += "&idStateType=" + idStateType + "&isCurrentState=" + isCurrentState;
                break;
        }

        if (url !== undefined) {
            downloadFile(url);
        }

        $("#ClaimCommonReportModal").modal("hide");

    });

    $("#ClaimSplitAccountsReport").on("click", function () {
        url = "/ClaimReports/GetSplitAccountsReport";
        downloadFile(url);
        e.preventDefault();
    });
});
