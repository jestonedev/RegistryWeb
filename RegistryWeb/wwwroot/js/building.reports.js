$(document).ready(function () {
    function downloadFile(url) {
        var link = document.createElement('a');
        link.href = url;
        link.target = "_blank";
        link.style.display = "none";
        document.getElementsByTagName("body")[0].appendChild(link);
        link.click();
    }

    $("body").on('click', ".rr-report-building-excerpt", function (e) {
        var idBuilding = $(this).data("id-building");
        $("#excerptModal").find("[name='Excerpt.IdObject']").val(idBuilding);
        $("#excerptModal").find("input, textarea, select").prop("disabled", false);
        $("#excerptModal").modal("show");
        e.preventDefault();
    });

    $("#excerptForm").on("change", "select", function () {
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
        var excerptNumber = $("#excerptModal").find("[name='Excerpt.ExcerptNumber']").val();
        var excerptDate = $("#excerptModal").find("[name='Excerpt.ExcerptDate']").val();
        var signer = $("#excerptModal").find("[name='Excerpt.Signer']").val();
        if ($("#excerptModal").find(".input-validation-error").length > 0) {
            return false;
        }

        var url = "/BuildingReports/GetExcerptBuilding?idBuilding=" + idObject + "&excerptNumber=" + encodeURIComponent(excerptNumber)
            + "&excerptDateFrom=" + excerptDate + "&signer=" + signer;
        if (url !== undefined) {
            downloadFile(url);
        }

        $("#excerptModal").modal("hide"); 
    });
});
