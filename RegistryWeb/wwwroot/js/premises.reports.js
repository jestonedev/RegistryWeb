﻿$(document).ready(function () {
    function downloadFile(url) {
        var link = document.createElement('a');
        link.href = url;
        link.target = "_blank";
        link.style.display = "none";
        document.getElementsByTagName("body")[0].appendChild(link);
        link.click();
    }

    $("body").on('click', ".rr-report-premise-excerpt", function (e) {
        var idPremise = $(this).data("id-premise");
        $("#excerptModal").find("[name='Excerpt.IdObject']").val(idPremise);
        $("#excerptModal").find("[name='Excerpt.ExcerptType']").val(1);
        $("#excerptModal").find("input, textarea, select").prop("disabled", false);
        $("#excerptModal").modal("show");
        e.preventDefault();
    });

    $("body").on('click', ".rr-report-mun-sub-premises-excerpt", function (e) {
        var idPremise = $(this).data("id-premise");
        $("#excerptModal").find("[name='Excerpt.IdObject']").val(idPremise);
        $("#excerptModal").find("[name='Excerpt.ExcerptType']").val(3);
        $("#excerptModal").find("input, textarea, select").prop("disabled", false);
        $("#excerptModal").modal("show");
        e.preventDefault();
    });

    $("body").on('click', ".rr-report-sub-premise-excerpt", function (e) {
        var idSubPremise = $(this).data("id-sub-premise");
        $("#excerptModal").find("[name='Excerpt.IdObject']").val(idSubPremise);
        $("#excerptModal").find("[name='Excerpt.ExcerptType']").val(2);
        $("#excerptModal").find("input, textarea, select").prop("disabled", false);
        $("#excerptModal").modal("show");
        e.preventDefault();
    });

    $("body").on('click', ".rr-report-premise-notice-to-bks", function (e) {
        var idPremise = $(this).data("id-premise");
        $("#noticeToBksModal").find("[name='NoticeToBks.NoticeType']").val(1);
        $("#noticeToBksModal").find("[name='NoticeToBks.IdObject']").val(idPremise);
        $("#noticeToBksModal").find("input, textarea, select").prop("disabled", false);
        $("#noticeToBksModal").modal("show");
        e.preventDefault();
    });

    $("body").on('click', ".rr-report-sub-premise-notice-to-bks", function (e) {
        var idSubPremise = $(this).data("id-sub-premise");
        $("#noticeToBksModal").find("[name='NoticeToBks.NoticeType']").val(2);
        $("#noticeToBksModal").find("[name='NoticeToBks.IdObject']").val(idSubPremise);
        $("#noticeToBksModal").find("input, textarea, select").prop("disabled", false);
        $("#noticeToBksModal").modal("show");
        e.preventDefault();
    });

    $("body").on('click', ".rr-report-premise-area", function (e) {
        var idPremise = $(this).data("id-premise");
        url = "/PremiseReports/GetPremiseArea?idPremise=" + idPremise;
        downloadFile(url);
        e.preventDefault();
    });

    $("#excerptForm, #noticeToBksForm").on("change", "select", function () {
        fixBootstrapSelectHighlightOnChange($(this));
    });
    
    $("#excerptModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var isValid = $(this).closest("#excerptForm").valid();
        if (!isValid) {
            fixBootstrapSelectHighlight($(this).closest("#excerptForm"));
            return false;
        } 
        var idObject = $("#excerptModal").find("[name='Excerpt.IdObject']").val();
        var excerptType = $("#excerptModal").find("[name='Excerpt.ExcerptType']").val();
        var excerptNumber = $("#excerptModal").find("[name='Excerpt.ExcerptNumber']").val();
        var excerptDate = $("#excerptModal").find("[name='Excerpt.ExcerptDate']").val();
        var signer = $("#excerptModal").find("[name='Excerpt.Signer']").val();
        if ($("#excerptModal").find(".input-validation-error").length > 0) {
            return false;
        }

        switch (excerptType) {
            case "1":
                var url = "/PremiseReports/GetExcerptPremise?idPremise=" + idObject + "&excerptNumber=" + encodeURIComponent(excerptNumber)
                    + "&excerptDateFrom=" + excerptDate + "&signer=" + signer;
                break;
            case "2":
                url = "/PremiseReports/GetExcerptSubPremise?idSubPremise=" + idObject + "&excerptNumber=" + encodeURIComponent(excerptNumber)
                    + "&excerptDateFrom=" + excerptDate + "&signer=" + signer;
                break;
            case "3":
                url = "/PremiseReports/GetExcerptMunSubPremise?idPremise=" + idObject + "&excerptNumber=" + encodeURIComponent(excerptNumber)
                    + "&excerptDateFrom=" + excerptDate + "&signer=" + signer;
                break;
        }
        if (url !== undefined) {
            downloadFile(url);
        }

        $("#excerptModal").modal("hide"); 
    });

    $("#noticeToBksModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var isValid = $(this).closest("#noticeToBksForm").valid();
        if (!isValid) {
            fixBootstrapSelectHighlight($(this).closest("#noticeToBksForm"));
            return false;
        }
        var idObject = $("#noticeToBksModal").find("[name='NoticeToBks.IdObject']").val();
        var noticeType = $("#noticeToBksModal").find("[name='NoticeToBks.NoticeType']").val();
        var actionText = $("#noticeToBksModal").find("[name='NoticeToBks.ActionText']").val();
        var paymentType = $("#noticeToBksModal").find("[name='NoticeToBks.PaymentType']").val();
        var signer = $("#noticeToBksModal").find("[name='NoticeToBks.Signer']").val();
        if ($("#noticeToBksModal").find(".input-validation-error").length > 0) {
            return false;
        }
        switch (noticeType) {
            case "1":
                var url = "/PremiseReports/GetPremiseNoticeToBks?idPremise=" + idObject + "&actionText=" + actionText
                    + "&paymentType=" + paymentType + "&signer=" + signer;
                break;
            case "2":
                var url = "/PremiseReports/GetSubPremiseNoticeToBks?idSubPremise=" + idObject + "&actionText=" + actionText
                    + "&paymentType=" + paymentType + "&signer=" + signer;
                break;
        }
        if (url !== undefined) {
            downloadFile(url);
        }

        $("#noticeToBksModal").modal("hide");
    });
});
