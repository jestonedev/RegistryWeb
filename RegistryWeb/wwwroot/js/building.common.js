let getInputTemplate = function (name, value) {
    return '<input type="hidden" name="' + name + '" value="' + value + '">';
};

let isCustomOrganization = false;

$(function () {
    $("#addOrganizationBtn").off();
    $("#addOrganizationBtn").on('click', function (e) {
        $("#addOrganizationBtn").hide();
        $("#cancelOrganizationBtn").show(); 
        $("#organizationContainer").hide();
        $("#addOrganizationContainer").show();
        isCustomOrganization = true;
        e.preventDefault();
    });

    $("#cancelOrganizationBtn").off();
    $("#cancelOrganizationBtn").on('click', function (e) {
        $("#cancelOrganizationBtn").hide();
        $("#addOrganizationBtn").show();
        $("#addOrganizationContainer").hide();
        $('#CustomOrganization').val('');
        $("#organizationContainer").show();
        isCustomOrganization = false;
        e.preventDefault();
    });
});

let saveNewBuilding = function (e) {
    let orgSelectElem = $('#organizationContainer').find('[class="selectpicker form-control" ]');
    let customOrganization = $("#CustomOrganization").val();
    //if (!$("#customOrganization").valid()) return false;
    let code = 0;
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/BuildingDemolitionInfo/AddOrganization',
        data: { organizationName: customOrganization },
        async: false,
        success: function (id) {
            code = id;
            if (id > 0) {
                orgSelectElem.append("<option value='" + id + "'>" + customOrganization + "</option>");
                $("#cancelOrganization").click();
                orgSelectElem.val(id).selectpicker("refresh");
            } else
                if (id === -3) {
                    $("#cancelOrganizationBtn").click();
                    var duplicateOption = orgSelectElem.find("option").filter(function (idx, elem) {
                        return $(elem).text() === customDocumentIssuedBy;
                    });
                    var optionId = 0;
                    if (duplicateOption.length > 0) {
                        optionId = duplicateOption.prop("value");
                    } else {
                        alert('Произошла ошибка при сохранении управляющей компании или ТСЖ');
                    }
                    orgSelectElem.val(optionId).selectpicker("refresh");
                    code = optionId;
                } else {
                    alert('Произошла ошибка при сохранении управляющей компании или ТСЖ');
                    return false;
                }
        }
    });
    return code > 0;
    e.preventDefault();
}

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

let updateDisableState = function (state, action, canEditBaseInfo, canEditLandInfo) {
    let disableBaseInfo = state ? !(canEditBaseInfo && action !== "Details" && action !== "Delete") : state;
    $('#building select').selectpicker("refresh").prop('disabled', disableBaseInfo).selectpicker("refresh");

    $('#building input').attr('disabled', disableBaseInfo);
    $('#building textarea').attr('disabled', disableBaseInfo);
    if (!canEditBaseInfo && canEditLandInfo && action === "Edit") {
        $("#LandCadastralNum, #LandCadastralDate, #LandArea").attr('disabled', false);
    }
    if (!$("#IsMemorial").is(":checked") && !disableBaseInfo) {
        $("#MemorialNumber, #MemorialDate, #MemorialNameOrg").attr('disabled', true);
    }
};

let editBuildingClick = function (e) {
    if (isCustomOrganization && $('#CustomOrganization').val() != '') {
        console.log(saveNewBuilding(e));
    }
    $("input.decimal").each(function (idx, elem) {
        $(elem).val($(elem).val().replace(".", ","));
    });

    var action = $('#building').data("action");
    var canEditBaseInfo = JSON.parse($('#building').data("caneditbaseinfo").toLowerCase());
    var canEditLandInfo = JSON.parse($('#building').data("caneditlandinfo").toLowerCase());
    updateDisableState(false, action, canEditBaseInfo, canEditLandInfo);

    if (canEditLandInfo && !canEditBaseInfo) {
        $('select').selectpicker("refresh").prop('disabled', false);
        $('#building input').attr('disabled', false);
        $('#building textarea').attr('disabled', false);
    }

    $("button[data-id], .bootstrap-select").removeClass("input-validation-error");
    let buildingIsValid = $('#building').valid();
    let restrictionsIsValid = $("#restrictionsForm").valid();
    let ownershipRightsIsValid = $("#ownershipRightsForm").valid();
    let buildingDemolitionInfoValid = _buildingDemolitionInfo.form.valid();

    var itemsInEditMode = $("ul.list-group .yes-no-panel, #buildingDemolitionInfoSave").filter(function (idx, elem) {
        return $(elem).css("display") !== "none";
    });

    if (!buildingIsValid || !restrictionsIsValid || !ownershipRightsIsValid || !buildingDemolitionInfoValid) {
        updateDisableState(true, action, canEditBaseInfo, canEditLandInfo);
        onSubmitErrorsPostProcessing();
    } else
    if (itemsInEditMode.length > 0) {
        itemsInEditMode.each(function (idx, elem) {
            var body = $(elem).closest("ul.list-group, .card-body");
            var listGroupItem = $(elem).closest(".list-group-item, .card-body");
            if ($(elem).attr("id") === "buildingDemolitionInfoSave") {
                body = $("#buildingDemolitionInfoBlock");
                listGroupItem = body;
            }
            if (body.hasClass("toggle-hide")) {
                var toggler = $(elem).closest(".card").find("[id$='Toggle']").first();
                toggler.click();
            }
            if (!listGroupItem.hasClass("list-group-item-warning")) {
                listGroupItem.addClass("list-group-item-warning");
            }
        });
        var firstItemInEditMode = itemsInEditMode.first();
        var scrollBody = firstItemInEditMode.closest(".list-group-item, .card-body");
        if (firstItemInEditMode.attr("id") === "buildingDemolitionInfoSave") {
            firstItemInEditMode = $("#buildingDemolitionInfoBlock");
            scrollBody = firstItemInEditMode;
        }
        $([document.documentElement, document.body]).animate({
            scrollTop: scrollBody.offset().top
        }, 1000);

        e.preventDefault();
        updateDisableState(true, action, canEditBaseInfo, canEditLandInfo);
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
    memorialCardClick();
    var action = $('#building').data("action");
    var canEditBaseInfo = JSON.parse($('#building').data("caneditbaseinfo").toLowerCase());
    var canEditLandInfo = JSON.parse($('#building').data("caneditlandinfo").toLowerCase());
    if (action === "Details" || action === "Delete" || !canEditBaseInfo) {
        updateDisableState(true, action, canEditBaseInfo, canEditLandInfo);
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