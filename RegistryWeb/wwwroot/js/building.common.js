let getInputTemplate = function (name, value) {
    return '<input type="hidden" name="' + name + '" value="' + value + '">';
};

let createBuildingClick = function (event) {
    event.preventDefault();
    $("input.decimal").each(function (idx, elem) {
        $(elem).val($(elem).val().replace(".", ","));
    });
    $("button[data-id], .bootstrap-select").removeClass("input-validation-error");
    let buildingIsValid = $('#building').valid();
    let restrictionsIsValid = $("#restrictionsForm").valid();
    let ownershipRightsIsValid = $("#ownershipRightsForm").valid();
    let buildingDemolitionInfoValid = _buildingDemolitionInfo.form.valid();
    if (buildingIsValid && restrictionsIsValid && ownershipRightsIsValid && buildingDemolitionInfoValid) {
        var restrictions = CreateRestrictionBuildingsAssoc();
        for (let i = 0; i < restrictions.length; i++) {
            let rba = "RestrictionBuildingsAssoc[" + i + "].";
            let rn = rba + "RestrictionNavigation.";
            $('#building').append(getInputTemplate(rba + 'IdBuilding', $('#building [name="IdBuilding"]').val()));
            $('#building').append(getInputTemplate(rba + 'IdRestriction', restrictions[i].RestrictionNavigation.IdRestriction));
            $('#building').append(getInputTemplate(rn + 'IdRestriction', restrictions[i].RestrictionNavigation.IdRestriction));
            $('#building').append(getInputTemplate(rn + 'Number', restrictions[i].RestrictionNavigation.Number));
            $('#building').append(getInputTemplate(rn + 'Date', restrictions[i].RestrictionNavigation.Date));
            $('#building').append(getInputTemplate(rn + 'DateStateReg', restrictions[i].RestrictionNavigation.DateStateReg));
            $('#building').append(getInputTemplate(rn + 'Description', restrictions[i].RestrictionNavigation.Description));
            $('#building').append(getInputTemplate(rn + 'IdRestrictionType', restrictions[i].RestrictionNavigation.IdRestrictionType));
            let file = $(restrictions[i].RestrictionNavigation.RestrictionFile).clone();
            file.attr("name", "RestrictionFile[" + i + "]");
            $('#building').append(file);
        }
        let obas = CreateOwnershipBuildingsAssoc();
        for (let i = 0; i < obas.length; i++) {
            let oba = "OwnershipBuildingsAssoc[" + i + "].";
            let orn = oba + "OwnershipRightNavigation.";
            $('#building').append(getInputTemplate(oba + 'IdBuilding', $('#building [name="IdBuilding"]').val()));
            $('#building').append(getInputTemplate(oba + 'IdOwnershipRight', obas[i].OwnershipRightNavigation.IdOwnershipRight));
            $('#building').append(getInputTemplate(orn + 'IdOwnershipRight', obas[i].OwnershipRightNavigation.IdOwnershipRight));
            $('#building').append(getInputTemplate(orn + 'Number', obas[i].OwnershipRightNavigation.Number));
            $('#building').append(getInputTemplate(orn + 'Date', obas[i].OwnershipRightNavigation.Date));
            $('#building').append(getInputTemplate(orn + 'Description', obas[i].OwnershipRightNavigation.Description));
            $('#building').append(getInputTemplate(orn + 'IdOwnershipRightType', obas[i].OwnershipRightNavigation.IdOwnershipRightType));
            $('#building').append(getInputTemplate(orn + 'ResettlePlanDate', obas[i].OwnershipRightNavigation.ResettlePlanDate));
            $('#building').append(getInputTemplate(orn + 'DemolishPlanDate', obas[i].OwnershipRightNavigation.DemolishPlanDate));
            let file = $(obas[i].OwnershipRightNavigation.OwnershipRightFile).clone();
            file.attr("name", "OwnershipRightFile[" + i + "]");
            $('#building').append(file);
        }
        let buildingDemolitionInfo = _buildingDemolitionInfo.getJson();
        $('#building').append(getInputTemplate('DemolishedPlanDate', buildingDemolitionInfo.DemolishedPlanDate));
        $('#building').append(getInputTemplate('DemolishedFactDate', buildingDemolitionInfo.DemolishedFactDate));
        $('#building').append(getInputTemplate('DateOwnerEmergency', buildingDemolitionInfo.DateOwnerEmergency));
        $('#building').append(getInputTemplate('DemandForDemolishingDeliveryDate', buildingDemolitionInfo.DemandForDemolishingDeliveryDate));
        let buildingDemolitionActFiles = buildingDemolitionInfo.BuildingDemolitionActFiles;
        for (let i = 0; i < buildingDemolitionActFiles.length; i++) {
            let bdaf = "BuildingDemolitionActFiles[" + i + "].";
            $('#building').append(getInputTemplate(bdaf + 'Id', buildingDemolitionActFiles[i].Id));
            $('#building').append(getInputTemplate(bdaf + 'IdBuilding', buildingDemolitionActFiles[i].IdBuilding));
            $('#building').append(getInputTemplate(bdaf + 'IdActFile', buildingDemolitionActFiles[i].IdActFile));
            $('#building').append(getInputTemplate(bdaf + 'IdActTypeDocument', buildingDemolitionActFiles[i].IdActTypeDocument));
            $('#building').append(getInputTemplate(bdaf + 'Number', buildingDemolitionActFiles[i].Number));
            $('#building').append(getInputTemplate(bdaf + 'Date', buildingDemolitionActFiles[i].Date));
            $('#building').append(getInputTemplate(bdaf + 'Name', buildingDemolitionActFiles[i].Name));
            let file = $(buildingDemolitionActFiles[i].FileInput).clone();
            file.attr("name", "BuildingDemolitionActFile[" + i + "]");
            $('#building').append(file);
        }
        let attachmentFiles = CreateBuildingAttachmentFilesAssoc();
        for (let i = 0; i < attachmentFiles.length; i++) {
            let baf = "BuildingAttachmentFilesAssoc[" + i + "].";
            let oaf = baf + "ObjectAttachmentFileNavigation.";
            $('#building').append(getInputTemplate(baf + 'IdBuilding', $('#building [name="IdBuilding"]').val()));
            $('#building').append(getInputTemplate(baf + 'IdAttachment', attachmentFiles[i].ObjectAttachmentFileNavigation.IdAttachment));
            $('#building').append(getInputTemplate(oaf + 'IdAttachment', attachmentFiles[i].ObjectAttachmentFileNavigation.IdAttachment));
            $('#building').append(getInputTemplate(oaf + 'Description', attachmentFiles[i].ObjectAttachmentFileNavigation.Description));
            let file = $(attachmentFiles[i].ObjectAttachmentFileNavigation.AttachmentFile).clone();
            file.attr("name", "AttachmentFile[" + i + "]");
            $('#building').append(file);
        }
        $('#building').submit();
    } else {
        onSubmitErrorsPostProcessing();
    }
};

let editBuildingClick = function (e) {
    $("input.decimal").each(function (idx, elem) {
        $(elem).val($(elem).val().replace(".", ","));
    });
    $("button[data-id], .bootstrap-select").removeClass("input-validation-error");
    let buildingIsValid = $('#building').valid();
    let restrictionsIsValid = $("#restrictionsForm").valid();
    let ownershipRightsIsValid = $("#ownershipRightsForm").valid();
    let buildingDemolitionInfoValid = _buildingDemolitionInfo.form.valid();

    var itemsInEditMode = $("ul.list-group .yes-no-panel, #buildingDemolitionInfoSave").filter(function (idx, elem) {
        return $(elem).css("display") !== "none";
    });

    if (!buildingIsValid || !restrictionsIsValid || !ownershipRightsIsValid || !buildingDemolitionInfoValid) {
        onSubmitErrorsPostProcessing();
    } else
    if (itemsInEditMode.length > 0) {
        itemsInEditMode.each(function (idx, elem) {
            if ($(elem).closest("ul.list-group, .card-body").hasClass("toggle-hide")) {
                var toggler = $(elem).closest(".card").find("[id$='Toggle']").first();
                toggler.click();
            }
            var listGroupItem = $(elem).closest(".list-group-item, .card-body");
            if (!listGroupItem.hasClass("list-group-item-warning")) {
                listGroupItem.addClass("list-group-item-warning");
            }
        });
        $([document.documentElement, document.body]).animate({
            scrollTop: itemsInEditMode.first().closest(".list-group-item, .card-body").offset().top
        }, 1000);

        e.preventDefault();
    }
     else {
        $('#building').submit();
    }
    e.preventDefault();
};

let deleteBuildingClick = function (e) {
    $('#building').submit();
    e.preventDefault();
}

let onSubmitErrorsPostProcessing = function () {
    $("select").each(function (idx, elem) {
        var id = $(elem).prop("id");
        var name = $(elem).prop("name");
        var errorSpan = $("span[data-valmsg-for='" + name + "']");
        if (errorSpan.hasClass("field-validation-error")) {
            $("button[data-id='" + id + "']").addClass("input-validation-error");
        }
    });

    $(".toggle-hide").each(function (idx, elem) {
        if ($(elem).find(".field-validation-error").length > 0) {
            var toggler = $(elem).closest(".card").find("[id$='Toggle']");
            if (!isExpandElemntArrow(toggler)) {
                toggler.click();
            }
        }
    });
    $([document.documentElement, document.body]).animate({
        scrollTop: $(".input-validation-error").first().offset().top - 35
    }, 1000);
};

let memorialCardClick = function () {
    var isMemorial = $('#IsMemorial').is(':checked');
    $('#MemorialNumber').prop('disabled', !isMemorial);
    $('#MemorialDate').prop('disabled', !isMemorial);
    $('#MemorialNameOrg').prop('disabled', !isMemorial);
};

$(function () {
    var action = $('#building').data("action");
    var canEditBaseInfo = JSON.parse($('#building').data("caneditbaseinfo").toLowerCase());
    memorialCardClick();
    if (action === "Details" || action === "Delete" || !canEditBaseInfo) {
        $('select').selectpicker("refresh").prop('disabled', true);
        $('#building input[type="checkbox"]').click(function () {
            return false;
        });
        $('#building input').attr('disabled', true);
        $('#building textarea').attr('disabled', true);
    }
    $('#buildingToggle').on('click', $('#building'), elementToogleHide);
    $('#createBtn').click(createBuildingClick);
    $('#editBtn').click(editBuildingClick);
    $('#deleteBtn').click(deleteBuildingClick);
    $('#IsMemorial').click(memorialCardClick);

    $("form").on("change", "select", function () {
        var isValid = $(this).valid();
        var id = $(this).prop("id");
        if (!isValid) {
            $("button[data-id='" + id + "']").addClass("input-validation-error");
        } else {

            $("button[data-id='" + id + "']").removeClass("input-validation-error");
        }
    });
});