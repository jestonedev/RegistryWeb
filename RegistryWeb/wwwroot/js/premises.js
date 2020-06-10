﻿$(document).ready(function () {
    $('.premise-toggler').each(function (idx, e) {
        $(e).on('click', $('#' + $(e).data("for")), elementToogle);
    });

    $("#IdStreet").on('change', function () {
        var idStreet = $(this).val();
        if (idStreet === "") return;
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
        var isFormValid = $(this).valid();
        var isOwnershipsValid = $("#ownershipRightsForm").valid();
        var isRestrictionsValid = $("#restrictionsForm").valid();
        if (!isFormValid || !isOwnershipsValid || !isRestrictionsValid) {
            $(this).find("select").each(function (idx, elem) {
                var id = $(elem).prop("id");
                var name = $(elem).prop("name");
                var errorSpan = $("span[data-valmsg-for='" + name + "']");
                if (errorSpan.hasClass("field-validation-error")) {
                    $("button[data-id='" + id + "']").addClass("input-validation-error");
                }
            });
            e.preventDefault();
        } else if ($(this).data("action") === "Create") {
            var inputTemplate = "<input type='hidden' name='{0}' value='{1}'>";
            var ownerships = CreateOwnershipPremisesAssoc();
            for (var i = 0; i < ownerships.length; i++) {
                var opa = "Premise.OwnershipPremisesAssoc[" + i + "].";
                var orn = opa + "OwnershipRightNavigation.";
                $(this).append(inputTemplate.replace('{0}', opa + "IdOwnershipRight").replace('{1}', ownerships[i].IdOwnershipRight));
                $(this).append(inputTemplate.replace('{0}', opa + "IdPremises").replace('{1}', ownerships[i].IdPremises));
                $(this).append(inputTemplate.replace('{0}', orn + "Date").replace('{1}', ownerships[i].OwnershipRightNavigation.Date));
                $(this).append(inputTemplate.replace('{0}', orn + "Number").replace('{1}', ownerships[i].OwnershipRightNavigation.Number));
                $(this).append(inputTemplate.replace('{0}', orn + "Description").replace('{1}', ownerships[i].OwnershipRightNavigation.Description));
                $(this).append(inputTemplate.replace('{0}', orn + "IdOwnershipRightType").replace('{1}', ownerships[i].OwnershipRightNavigation.IdOwnershipRightType));
                $(this).append(inputTemplate.replace('{0}', orn + "DemolishPlanDate").replace('{1}', ownerships[i].OwnershipRightNavigation.DemolishPlanDate));
                $(this).append(inputTemplate.replace('{0}', orn + "ResettlePlanDate").replace('{1}', ownerships[i].OwnershipRightNavigation.ResettlePlanDate));
            }
            var restrictions = CreateRestrictionPremisesAssoc();
            for (var j = 0; j < restrictions.length; j++) {
                var rpa = "Premise.RestrictionPremisesAssoc[" + j + "].";
                var rrn = rpa + "RestrictionNavigation.";
                $(this).append(inputTemplate.replace('{0}', rpa + "IdRestriction").replace('{1}', restrictions[j].IdRestriction));
                $(this).append(inputTemplate.replace('{0}', rpa + "IdPremises").replace('{1}', restrictions[j].IdPremises));
                $(this).append(inputTemplate.replace('{0}', rrn + "Date").replace('{1}', restrictions[j].RestrictionNavigation.Date));
                $(this).append(inputTemplate.replace('{0}', rrn + "Number").replace('{1}', restrictions[j].RestrictionNavigation.Number));
                $(this).append(inputTemplate.replace('{0}', rrn + "Description").replace('{1}', restrictions[j].RestrictionNavigation.Description));
                $(this).append(inputTemplate.replace('{0}', rrn + "IdRestrictionType").replace('{1}', restrictions[j].RestrictionNavigation.IdRestrictionType));
            }
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