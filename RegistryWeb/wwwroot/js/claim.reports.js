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

    $("#claimBksAndTransToLegalModal").on("change", "select", function () {
        fixBootstrapSelectHighlightOnChange($(this));
    });

    /*      МУЛЬТИМАСТЕР        */

    $(".pagination .page-link").on("click", function (e) {
        var path = location.pathname;
        var page = $(this).data("page");
        location.href = path + "?PageOptions.CurrentPage=" + page;
        e.preventDefault();
    });
});
