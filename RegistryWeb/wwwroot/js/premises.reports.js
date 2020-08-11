$(document).ready(function () {

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

    $("body").on('click', ".rr-report-premise-area, #massReferenceBtn", function (e) {
        url = "/PremiseReports/GetPremisesArea";
        if ($(this).data("id-premise") !== undefined) {
            var idPremise = $(this).data("id-premise");
            url = "/PremiseReports/GetPremiseArea?idPremise=" + idPremise;
        }
        downloadFile(url);
        e.preventDefault();
    });
    
    $("body").on('click', "#massActBtn", function (e) {
        $("#massActModal").find("input, textarea, select").prop("disabled", false);
        $("#massActModal").modal("show");

        e.preventDefault();
    });

    $("body").on('click', "#massExcerptBtn", function (e) {
        $("#excerptModal").find("[name='Excerpt.ExcerptType']").val(4);
        $("#excerptModal").find("input, textarea, select").prop("disabled", false);
        $('#excerptModal').modal('toggle');
        e.preventDefault();
    });

    function attachFileForPaste(e) {
        var fileWrapper = $(this).closest(".form-group");
        fileWrapper.find('input[type="file"]').click();
        e.preventDefault();
    }

    function changeFileAttachmentForPaste() {
        var fileWrapper = $(this).closest(".form-group");
        var nameParts = $(this).val().split('\\');
        var name = nameParts[nameParts.length - 1];
        if ($(this).val() !== "") {
            var elem = fileWrapper.find("[name$='Description']");
            if (elem.val() === "" || elem.val() === null)
                elem.val(name);
            let fileBtns = fileWrapper.find(".rr-file-buttons");
            fileWrapper.find(".rr-file-attach").hide();
            fileBtns.append(fileWrapper.find(".rr-file-remove").show());
        } else {
            let fileBtns = fileWrapper.find(".rr-file-buttons");
            fileWrapper.find(".rr-file-remove").hide();
            fileBtns.append(fileWrapper.find(".rr-file-attach").show());
        }
    }

    function removeFileForPaste(e) {
        var fileWrapper = $(this).closest(".form-group");
        fileWrapper.find('input[type="file"]').val("").change();
        e.preventDefault();
    }

    $("#restrictionForm .rr-file-attach, #ownershipRightForm .rr-file-attach").on('click', attachFileForPaste);
    $('#restrictionForm .rr-file-remove, #ownershipRightForm .rr-file-remove').on('click', removeFileForPaste);
    $('#restrictionForm input[name="RestrictionFile"], #ownershipRightForm input[name="OwnershipRightFile"]').on('change', changeFileAttachmentForPaste);

    $("body").on('click', "#restrictionBtn", function (e) {
        $("#restrictionModal").find("input, textarea, select").prop("disabled", false);
        $("#restrictionModal").modal("toggle");
        e.preventDefault();
    });

    $("body").on('click', "#ownershipRightBtn", function (e) {
        $("#ownershipRightModal").find("input, textarea, select").prop("disabled", false);
        $("#ownershipRightModal").modal("toggle");
        e.preventDefault();
    });

    var row0 = $(".row0").clone();
    var row1 = $(".row1").clone();
    var row2 = $(".row2").clone();

    $("body").on('click', "#updateDescriptionBtn, #updateCurrentStateBtn, #updateRegDateBtn", function (e) {
        var modal = $("#updatePremiseModal");
        modal.find(".modal-body").empty();
        switch ($(this).data("id-modal"))
        {
            case 0:
                modal.find(".modal-body").html(row0);
                modal.find(".rr-report-submit").html("Добавить");
                modal.find(".modal-title").html("Обновить дополнительные сведения");
                break;
            case 1:
                modal.find(".modal-body").html(row1);
                modal.find(".rr-report-submit").html("Изменить");
                modal.find(".modal-title").html("Изменить текущее состояние");
                break;
            case 2:
                modal.find(".modal-body").html(row2);
                modal.find(".rr-report-submit").html("Изменить");
                modal.find(".modal-title").html("Изменить дату включения в РМИ");
                break;
        }
        modal.find("input, textarea, select").prop("disabled", false);
        modal.find("[data-id], .selectpicker option.bs-title-option").remove();
        modal.find(".selectpicker").selectpicker('refresh');
        modal.modal("toggle");

        e.preventDefault();
    });

    $("body").on('click', "#exportBtn", function (e) {
        url = "/PremiseReports/GetPremisesExport";
        downloadFile(url);
        e.preventDefault();
    });

    $("body").on('click', "#historyBtn", function (e) {
        url = "/PremiseReports/GetPremisesTenancyHistory";
        downloadFile(url);
        e.preventDefault();
    });

    $("#excerptForm, #noticeToBksForm, #massActForm, #restrictionForm, #ownershipRightForm, #updatePremiseForm").on("change", "select", function () {
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
            case "4":
                url = "/PremiseReports/GetMassExcerptPremise?excerptNumber=" + encodeURIComponent(excerptNumber)
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
                url = "/PremiseReports/GetSubPremiseNoticeToBks?idSubPremise=" + idObject + "&actionText=" + actionText
                    + "&paymentType=" + paymentType + "&signer=" + signer;
                break;
        }
        if (url !== undefined) {
            downloadFile(url);
        }

        $("#noticeToBksModal").modal("hide");
    });

    $("#massActModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var isValid = $(this).closest("#massActForm").valid();
        if (!isValid) {
            fixBootstrapSelectHighlight($(this).closest("#massActForm"));
            return false;
        }
        var actdate = $("#massActModal").find("[name='MassAct.ActDate']").val();
        var commision = $("#massActModal").find("[name='MassAct.Commision']").val();
        var clerk = $("#massActModal").find("[name='MassAct.Clerk']").val();
        var isNotResides = null;
        if ($("#massActModal").find("[name='MassAct.IsNotResides']").is(':checked')) {
            isNotResides = "True";
        } else {
            isNotResides = "False";
        }

        if ($("#massActModal").find(".input-validation-error").length > 0) {
            return false;
        }
        var url = "/PremiseReports/GetPremisesAct?actDate=" + actdate
            + "&isNotResides=" + isNotResides + "&commision=" + commision + "&clerk=" + clerk;
        downloadFile(url);
        $("#massActModal").modal("hide");
    });

    $("#restrictionModal .rr-report-submit, #ownershipRightModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $(this).closest("form");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return false;
        }
        form.submit();
    });

    $("#updatePremiseModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $(this).closest("#updatePremiseForm");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return false;
        }

        form.submit();
    });

    $(".pagination .page-link").on("click", function (e) {
        var path = location.pathname;
        var page = $(this).data("page");
        location.href = path + "?PageOptions.CurrentPage=" + page;
        e.preventDefault();
    });
});
