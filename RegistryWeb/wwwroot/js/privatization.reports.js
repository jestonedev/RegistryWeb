$(document).ready(function () {
    $("body").on('click', ".rr-priv-common-modal-report", function (e) {
        resetModalForm($("#PrivCommonReportForm"));

        var idReoprt = $(this).data("report-id");
        var modal = $("#PrivCommonReportModal");
        modal.find("[name='ReportType']").val(idReoprt);
        modal.find(".modal-title").text($(this).attr("title"));
        modal.find("[name='ReportName']").val($(this).attr("title"));


        modal.find("[data-report-ids]").each(function (idx, elem) {
            var ids = $(this).data("report-ids");
            if ((ids | 0) !== ids)
                ids = $(ids.split(',')).map(function (idx, elem) { return parseInt(elem); }).toArray();
            else
                ids = [ids];
            if (ids.indexOf(idReoprt) >= 0)
                $(this).show();
            else
                $(this).hide();
        });

        refreshValidationForm($("#PrivCommonReportForm"));

        modal.modal("show");
        e.preventDefault();
    });

    $("body").on('click', ".rr-priv-quarter-modal-report", function (e) {
        var modal = $("#PrivQuarterReportModal");

        refreshValidationForm($("#PrivQuarterReportForm"));

        modal.modal("show");
        e.preventDefault();
    });

    var reportIdRegionChange = function () {
        var idsRegion = $('#PrivCommonReport_IdRegion').selectpicker('val');
        var select = $('#PrivCommonReportModal #PrivCommonReport_IdStreet');
        select.selectpicker('destroy');
        select.find('option[value]').remove();
        select.selectpicker('render');
        if (idsRegion.length === 0) {
            idsRegion = [null]; // Запрашиваем все улицы
        }
        for (var i = 0; i < idsRegion.length; i++) {
            var idRegion = idsRegion[i];
            $.ajax({
                    type: 'POST',
                    url: window.location.origin + '/Address/GetKladrStreets',
                    dataType: 'json',
                    data: { idRegion },
                    success: function (data) {
                        $.each(data, function (i, d) {
                            select.append('<option value="' + d.idStreet + '">' + d.streetName + '</option>');
                        });
                        select.selectpicker('refresh');
                    }
                });
        }
    };

    $('#PrivCommonReportModal #PrivCommonReport_IdRegion').on('change', function (e) {
        reportIdRegionChange();
        e.preventDefault();
    });

    $("#PrivCommonReportModal .rr-report-submit, #PrivQuarterReportModal  .rr-report-submit").on('click', function () {
        $(this).closest('form').submit();
        $(this).closest(".modal").modal("hide");
    });

    $("body").on('click', ".rr-priv-report-contract", function (e) {
        var idContract = $(this).data("id-contract");
        url = "/PrivatizationReports/GetContract?idContract=" + idContract;
        downloadFile(url);
        e.preventDefault();
    });

    $("body").on('click', ".rr-priv-report-contract-kumi", function (e) {
        var idContract = $(this).data("id-contract");
        url = "/PrivatizationReports/GetContractKumi?idContract=" + idContract;
        downloadFile(url);
        e.preventDefault();
    });
});
