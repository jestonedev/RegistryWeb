$(document).ready(function () {
    $('.premise-toggler').each(function (idx, e) {
        $(e).on('click', $('#' + $(e).data("for")), elementToogle);
    });

    $("#IdStreet").on('change', function () {
        var idStreet = $(this).val() || 0;
        $.getJSON('/Premises/GetHouse/?' + "streetId=" + idStreet, function (data) {
            var options = "<option></option>";
            $(data).each(function (idx, elem) {
                options += "<option value=" + elem.idBuilding + ">" + elem.house + "</option>";
            });

            var buildingElem = $("select[name$='Premise.IdBuilding']");
            var idBuilding = $("input[name='IdBuildingPrev']").val();
            buildingElem.html(options).selectpicker('refresh').val(idBuilding).selectpicker('render');
        });
    });

    $("form#r-premises-form").on("submit", function (e) {
        $("input.decimal").each(function (idx, elem) {
            $(elem).val($(elem).val().replace(".", ","));
        });
        $("button[data-id]").removeClass("input-validation-error");
        var isValid = $(this).valid();
        if (!isValid) {
            $(this).find("select").each(function (idx, elem) {
                var id = $(elem).prop("id");
                var name = $(elem).prop("name");
                var errorSpan = $("span[data-valmsg-for='" + name + "']");
                if (errorSpan.hasClass("field-validation-error")) {
                    $("button[data-id='" + id + "']").addClass("input-validation-error");
                }
            });
            e.preventDefault();
        }
    });

    $("form#r-premises-form").on("change", "select", function () {
        var isValid = $(this).valid();
        var id = $(this).prop("id");
        if (!isValid) {
            $("button[data-id='" + id + "']").addClass("input-validation-error");
        } else {

            $("button[data-id='" + id + "']").removeClass("input-validation-error");
        } 
    });

    $("select#IdStreet").val($("input[name='IdStreetPrev']").val()).selectpicker('refresh').change();

    var action = $('#r-premises-form').data("action");
    if (action === "Details" || action === "Delete") {
        $('select').prop('disabled', true);
        $('input').prop('disabled', true);
        $('textarea').prop('disabled', true);
        $('input[type="hidden"]').prop('disabled', false);
        $('input[type="submit"]').prop('disabled', false);
    }
});