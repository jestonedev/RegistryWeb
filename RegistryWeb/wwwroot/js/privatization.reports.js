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

        switch (idReoprt) {
            case 12: case 22: case 23:
                modal.find("label[for='PrivCommonReport_StartDate']").text("Дата подачи заявления с");
                modal.find("label[for='PrivCommonReport_EndDate']").text("Дата подачи заявления по");
                break;
            case 18:
                modal.find("label[for='PrivCommonReport_StartDate']").text("Дата выдачи гражданам с");
                modal.find("label[for='PrivCommonReport_EndDate']").text("Дата выдачи гражданам по");
                break;
            default:
                modal.find("label[for='PrivCommonReport_StartDate']").text("Дата рег. по договору с");
                modal.find("label[for='PrivCommonReport_EndDate']").text("Дата рег. по договору по");
        }

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
        var modal = $("#PrivContractModal");
        var idContract = $(this).data("id-contract");
        modal.find("#PrivContract_IdContract").val(idContract);
        modal.find("#PrivContract_IdContractType").val(1);
        modal.modal('show');
        e.preventDefault();
    });

    $("body").on('click', ".rr-priv-report-contract-kumi", function (e) {
        var modal = $("#PrivContractModal");
        var idContract = $(this).data("id-contract");
        modal.find("#PrivContract_IdContract").val(idContract);
        modal.find("#PrivContract_IdContractType").val(2);
        modal.modal('show');
        e.preventDefault();
    });

    var idOwnerSigners = undefined;

    $('#PrivContractModal #PrivContract_IdOwner').on('change', function (e) {
        if (idOwnerSigners === undefined) {
            idOwnerSigners = $("#PrivContract_IdOwnerSigner option[data-id-owner]").clone(true);
        }
        var idOwner = $("#PrivContract_IdOwner").val() | 0;
        $("#PrivContract_IdOwnerSigner option[data-id-owner]").remove();
        idOwnerSigners.each(function (idx, elem) {
            if ($(elem).data("id-owner") === idOwner) {
                $("#PrivContract_IdOwnerSigner").append($(elem));
            }
        });
        $("#PrivContract_IdOwnerSigner").selectpicker("refresh");
        e.preventDefault();
    });

    $("#PrivContractModal").on("show.bs.modal", function () {
        $(this).find('select').prop('disabled', false);
        $(this).find('input').prop('disabled', false);
        $(this).find('textarea').prop('disabled', false);
        $(this).find("select").selectpicker("refresh");
    });

    $("#PrivContractModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var modal = $("#PrivContractModal");
        var idContract = modal.find("#PrivContract_IdContract").val();
        var idContractType = modal.find("#PrivContract_IdContractType").val();
        var idOwner = modal.find("#PrivContract_IdOwner").val();
        var idOwnerSigner = modal.find("#PrivContract_IdOwnerSigner").val();
        var idContractKind = modal.find("#PrivContract_IdContractKind").val();
        var url = "/PrivatizationReports/GetContract?IdContract=" + idContract + "&ContractType=" + idContractType + "&ContractKind=" + idContractKind +
            "&IdOwner=" + idOwner + "&IdOwnerSigner=" + idOwnerSigner;
        downloadFile(url);
        $("#PrivContractModal").modal("hide");
    });

    $('#PrivContractModal #PrivContract_IdOwner').change();

    $("#PrivContractModal").on("change", "select", function () {
        fixBootstrapSelectHighlightOnChange($(this));
    });
});
