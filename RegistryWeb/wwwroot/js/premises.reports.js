$(document).ready(function () {
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

    $("body").on('click', ".rr-report-premise-area, #massreference", function (e) {
        var idPremise = $(this).data("id-premise");
        url = "/PremiseReports/GetPremisesArea?idPremise=" + idPremise;
        downloadFile(url);
        e.preventDefault();
    });

/*__________для массовых__________*/
    $("body").on('click', "#massact", function (e) {
        $("#massactModal").find("[name='massact.ActType']").val(1);
        $("#massactModal").find("input, textarea, select").prop("disabled", false);
        $("#massactModal").modal("show");

        e.preventDefault();
    });

    $("body").on('click', "#massexcerpt", function (e) {
        $("#excerptModal").find("[name='Excerpt.ExcerptType']").val(4);
        $("#excerptModal").find("input, textarea, select").prop("disabled", false);
        //$("#excerptModal").modal("show");
        $('#excerptModal').modal('toggle');
        e.preventDefault();
    });

    $("body").on('click', "#restriction", function (e) {        
        $("#restrictionModal").find("input, textarea, select").prop("disabled", false);
        $("#restrictionModal").modal("toggle");

        e.preventDefault();
    });

    $("body").on('click', "#ownershipright", function (e) {
        var idPremises = "";

        $('table tr').each(function (row) {
            $(this).find('#prem').each(function (cell) {
                idPremises += ($(this).html() + ', ');
            });
        });
        idPremises = idPremises.substring(0, idPremises.length - 2);
        $("#ownershiprightModal").find("[name='ownershipright.IdObjects']").val(idPremises);
        $("#ownershiprightModal").find("[name='ownershipright.ActType']").val(1);
        $("#ownershiprightModal").find("input, textarea, select").prop("disabled", false);
        $("#ownershiprightModal").modal("toggle");

        e.preventDefault();
    });

        var row0 = $(".row0").clone();
        var row1 = $(".row1").clone();
        var row2 = $(".row2").clone();
    $("body").on('click', "#moreinformation, #currentstate, #dateRMI", function (e) {
        $(".blocker").empty();        
        $("#putdownModal").find("[name='putdown.ActType']").val(1);
        switch ($(this).data("id-modal"))
        {
            case 0:
                $(".blocker").html(row0);
                $(".putdownbut").html("Добавить");
                $(".putdowntitle").html("Проставить дополнительные сведения");
                break;
            case 1:
                $(".blocker").html(row1);
                $(".blocker [data-id], .blocker .selectpicker option.bs-title-option").remove();
                $(".blocker .selectpicker ").selectpicker('refresh');
                $(".putdownbut").html("Изменить");
                $(".putdowntitle").html("Изменить состояние");
                break;
            case 2:
                $(".blocker").html(row2);
                $(".putdownbut").html("Изменить");
                $(".putdowntitle").html("Изменить дату включения в РМИ");
                break;
        }
        $("#putdownModal").find("input, textarea, select").prop("disabled", false);
        $("#putdownModal").modal("toggle");

        e.preventDefault();
    });

    $("body").on('click', "#export", function (e) {
        url = "/PremiseReports/GetPremisesExport";
        downloadFile(url);
        e.preventDefault();
    });

    $("body").on('click', "#hiringhistory", function (e) {
        url = "/PremiseReports/GetPremisesTenancyHistory";
        downloadFile(url);
        e.preventDefault();
    });

    $("#excerptForm, #noticeToBksForm, #massactForm, #restrictionForm, #putdownForm, #ownershiprightForm")
        .on("change", "select", function () {
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
                var url = "/PremiseReports/GetSubPremiseNoticeToBks?idSubPremise=" + idObject + "&actionText=" + actionText
                    + "&paymentType=" + paymentType + "&signer=" + signer;
                break;
        }
        if (url !== undefined) {
            downloadFile(url);
        }

        $("#noticeToBksModal").modal("hide");
    });
    
    $("#massactModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var isValid = $(this).closest("#massactForm").valid();
        if (!isValid) {
            fixBootstrapSelectHighlight($(this).closest("#massactForm"));
            return false;
        }
        var actType = $("#massactModal").find("[name='massact.ActType']").val();
        var actdate = $("#massactModal").find("[name='massact.ActDate']").val();
        var commision = $("#massactModal").find("[name='massact.Commision']").val();
        var clerk = $("#massactModal").find("[name='massact.Clerk']").val();
		var isNotResides = null;
        if ($("#massactModal").find("[name='massact.isNotResides']").is(':checked')) {
            isNotResides = "True";
        } else {
            isNotResides = "False";
        }
		
        if ($("#massactModal").find(".input-validation-error").length > 0) {
            return false;
        }
        switch (actType) {
            case "1":
                var url = "/PremiseReports/GetPremisesAct?actDate=" + actdate
                    + "&isNotResides=" + isNotResides + "&commision=" + commision + "&clerk=" + clerk;
                break;
            /*case "2":
                var url = "/PremiseReports/GetSubPremiseActs?idSubPremise=" + idPremises + "&actdate=" + actdate
                    + "&home=" + home + "&participants=" + participants + "&signer=" + signer;
                break;*/
        }
        if (url !== undefined) {
            downloadFile(url);
        }

        $("#massactModal").modal("hide");
    });

	//________для проставлений________
    $("#putdownModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var isValid = $(this).closest("#putdownForm").valid();
        if (!isValid) {
            fixBootstrapSelectHighlight($(this).closest("#putdownForm"));
            return false;
        }

        var description = $("#putdownModal").find("[name='premise.Description']").val();
        var regdate = $("#putdownModal").find("[name='premise.RegDate']").val();
        var state = $("#putdownModal").find("[name='premise.State']").val();

        if ($("#putdownModal").find(".input-validation-error").length > 0) {
            return false;
        }

        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Premises/UpdatePremises',
            data: { description: description, regDate: regdate, stateId: state },
            success: function (status) {
                if (status == 0) {
                    $(".status").html("Операция выполнена успешно!");
                    $(".status").addClass("alert alert-success");
                    $("#putdownModal").modal("toggle");
                }
                else {
                    $(".status").html("Ошибка!");
                    $(".status").addClass("alert alert-danger");
                    $("#putdownModal").modal("toggle");
                }
            }

        });

        $("#putdownModal").modal("hide");
    });
											 
});
