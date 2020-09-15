$(document).ready(function () {

    $("body").on('click', ".type1", function (e) {
        var modal = $("#JFModal");
        switch ($(this).data("id-modal")) {
            case 0:
                modal.find("[name='JF.JFType']").val(0);
                modal.find(".modal-title").html("Аварийные ЖП");
                break;
            case 1:
                modal.find("[name='JF.JFType']").val(1);
                modal.find(".modal-title").html("Коммерческий ЖФ");
                break;
            case 2:
                modal.find("[name='JF.JFType']").val(2);
                modal.find(".modal-title").html("Специализированный ЖФ");
                break;
            case 3:
                modal.find("[name='JF.JFType']").val(3);
                modal.find(".modal-title").html("Социальный ЖФ");
                break;
            case 4:
                modal.find("[name='JF.JFType']").val(4);
                modal.find(".modal-title").html("Текущие фонды МЖП");
                break;
        }

        modal.find("input, textarea, select").prop("disabled", false);
        modal.modal("show");

        e.preventDefault();
    });

    $("body").on('click', ".reestr", function (e) {
        var modal = $("#reestrModal");

        switch ($(this).data("id-bupr"))
        {
            case 0:
                modal.find(".but1").html("Все здания");
                modal.find(".but2").html("Муниципальные здания");
                modal.find("[name='reestr.RepType']").val(1);
                break;
            case 1:
                modal.find(".but1").html("Все помещения");
                modal.find(".but2").html("Муниципальные помещения");
                modal.find("[name='reestr.RepType']").val(2);
                break;
        }

        modal.find("input, textarea, select").prop("disabled", false);
        modal.modal("show");

        e.preventDefault();
    });

    $("#JFModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var isValid = $(this).closest("#JFForm").valid();
        if (!isValid) {
            fixBootstrapSelectHighlight($(this).closest("#JFForm"));
            return false;
        }
        var modal = $("#JFModal");
        var JFType = modal.find("[name='JF.JFType']").val();
        var regions = modal.find("[name='JF.Regions']").val();
        if (modal.find(".input-validation-error").length > 0) {
            return false;
        }
        var url = "/RegistryObjectsReports/GetEmergencyJP?JFType=" + JFType + "&regions=" + regions;

        if (url !== undefined) {
            downloadFile(url);
        }

        $("#JFModal").modal("hide");
    });
    
    $("#reestrModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var isValid = $(this).closest("#reestrForm").valid();
        if (!isValid) {
            fixBootstrapSelectHighlight($(this).closest("#reestrForm"));
            return false;
        }
        var modal = $("#reestrModal");
        if (modal.find(".input-validation-error").length > 0) {
            return false;
        }
        var RepType = modal.find("[name='reestr.RepType']").val();
        var typeReport = $(this).data("id-typerep");
        var url = null;
        switch (RepType)
        {
            case "1":
                url = "/RegistryObjectsReports/GetMunicipalBuilding?typeReport=" + typeReport;
                break;
            case "2":
                url = "/RegistryObjectsReports/GetMunicipalPremise?typeReport=" + typeReport;
                break;
        }

        if (url !== undefined) {
            downloadFile(url);
        }

        $("#reestrModal").modal("hide");
    });
});