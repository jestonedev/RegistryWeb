function updateRentObjectAreas(rentObjectElem) {
    buildingElem = rentObjectElem.find('select[name^="IdBuilding"]');
    premisesElem = rentObjectElem.find('select[name^="IdPremises"]');
    subPremisesElem = rentObjectElem.find('select[name^="IdSubPremises"]');
    var currentOption = null;
    if (subPremisesElem.val() !== null && subPremisesElem.val() !== "") {
        currentOption = rentObjectElem.find('select[name^="IdSubPremises"] option[value="' + subPremisesElem.val() + '"]');
    } else if (premisesElem.val() !== null && premisesElem.val() !== "") {
        currentOption = rentObjectElem.find('select[name^="IdPremises"] option[value="' + premisesElem.val() + '"]');
    } else if (buildingElem.val() !== null && buildingElem.val() !== "") {
        currentOption = rentObjectElem.find('select[name^="IdBuilding"] option[value="' + buildingElem.val() + '"]');
    }
    if (currentOption === null) {
        rentObjectElem.find('input[name^="TotalArea"]').val(0);
        rentObjectElem.find('input[name^="LivingArea"]').val(0);
    } else {
        rentObjectElem.find('input[name^="TotalArea"]').val(currentOption.data("total-area").toString().replace(".", ","));
        rentObjectElem.find('input[name^="LivingArea"]').val(currentOption.data("living-area").toString().replace(".", ","));
    }
}

function getTenancyRentHouses() {
    var idStreet = $(this).val();
    var rentObjectElem = $(this).closest(".list-group-item");
    var buildingToSelect = rentObjectElem.find('select[name^="IdBuilding"]');
    var buildingPrevId = rentObjectElem.find('input[name^="IdBuildingPrev"]').val();
    buildingToSelect.empty();
    buildingToSelect.append("<option></option>");
    buildingToSelect.selectpicker('refresh');
    $.getJSON('/TenancyRentObjects/GetHouses', { idStreet: idStreet }, function (buildings) {
        $(buildings).each(function (idx, building) {
            var option = '<option data-total-area="' + building.totalArea + '" data-living-area="' + building.livingArea +'" value="' + building.idBuilding + '">' + building.house + '</option>';
            buildingToSelect.append(option);
        });
        buildingToSelect.val(buildingPrevId);
        buildingToSelect.selectpicker('refresh');
        buildingToSelect.change();
        updateRentObjectAreas(rentObjectElem);
    });
}

function getTenancyRentPremises() {
    var idBuilding = $(this).val();
    var rentObjectElem = $(this).closest(".list-group-item");
    var premiseToSelect = rentObjectElem.find('select[name^="IdPremises"]');
    var premisePrevId = rentObjectElem.find('input[name^="IdPremisesPrev"]').val();
    premiseToSelect.empty();
    premiseToSelect.append("<option></option>");
    premiseToSelect.selectpicker('refresh');
    $.getJSON('/TenancyRentObjects/GetPremises', { idBuilding: idBuilding }, function (premises) {
        $(premises).each(function (idx, premise) {
            var option = '<option data-total-area="' + premise.totalArea + '" data-living-area="' + premise.livingArea +'" value="' + premise.idPremises + '">' + premise.premisesNum + '</option>';
            premiseToSelect.append(option);
        });
        premiseToSelect.val(premisePrevId);
        premiseToSelect.selectpicker('refresh');
        premiseToSelect.change();
        updateRentObjectAreas(rentObjectElem);
    });
}

function getTenancyRentSubPremises() {
    var idPremise = $(this).val();
    var rentObjectElem = $(this).closest(".list-group-item");
    var subPremiseToSelect = rentObjectElem.find('select[name^="IdSubPremises"]');
    var subPremisePrevId = rentObjectElem.find('input[name^="IdSubPremisesPrev"]').val();
    subPremiseToSelect.empty();
    subPremiseToSelect.append("<option></option>");
    subPremiseToSelect.selectpicker('refresh');
    $.getJSON('/TenancyRentObjects/GetSubPremises', { idPremise: idPremise }, function (subPremises) {
        $(subPremises).each(function (idx, subPremise) {
            var option = '<option data-total-area="' + subPremise.totalArea + '" data-living-area="' + subPremise.livingArea +'" value="' + subPremise.idSubPremises + '">' + subPremise.subPremisesNum + '</option>';
            subPremiseToSelect.append(option);
        });
        subPremiseToSelect.val(subPremisePrevId);
        subPremiseToSelect.selectpicker('refresh');
        updateRentObjectAreas(rentObjectElem);
    });
}

function updateSubPremisesArea() {
    var rentObjectElem = $(this).closest(".list-group-item");
    updateRentObjectAreas(rentObjectElem);
}

function addTenancyRentObject(e) {
    let action = $('#TenancyProcessRentObjects').data('action');

    $.ajax({
        type: 'POST',
        url: window.location.origin + '/TenancyProcesses/AddRentObject',
        data: { action },
        success: function (elem) {
            let list = $('#TenancyProcessRentObjects');
            list.find(".rr-list-group-item-empty").hide();
            let tenancyRentObjectToggle = $('#TenancyProcessRentObjectsForm .tenancy-process-toggler');
            if (!isExpandElemntArrow(tenancyRentObjectToggle)) // развернуть при добавлении, если было свернуто 
                tenancyRentObjectToggle.click();
            list.append(elem);
            elem = list.find(".list-group-item").last();
            elem.find("select").selectpicker("refresh");
            elem.find(".tenancy-rent-object-edit-btn").first().click();
            $([document.documentElement, document.body]).animate({
                scrollTop: $(elem).offset().top
            }, 1000);
            refreshValidationTenancyRentObjectForm();
        }
    });
    e.preventDefault();
}

function deleteTenancyRentObject(e) {
    e.preventDefault();
    let isOk = confirm("Вы уверены что хотите удалить нанимаемое жилье?");
    if (isOk) {
        let tenancyRentObjectElem = $(this).closest(".list-group-item");
        let idProcess = $("#TenancyProcessRentObjects").data("id");
        let idObject = 0;
        let addressType = tenancyRentObjectElem.find("input[name^='AddressTypePrev']").val();
        switch (addressType) {
            case "Building":
                idObject = tenancyRentObjectElem.find("input[name^='IdBuildingPrev']").val();
                break;
            case "Premise":
                idObject = tenancyRentObjectElem.find("input[name^='IdPremisesPrev']").val();
                break;
            case "SubPremise":
                idObject = tenancyRentObjectElem.find("input[name^='IdSubPremisesPrev']").val();
                break;
            default:
                tenancyRentObjectElem.remove();
                return;
        }
        
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/TenancyRentObjects/DeleteRentObject',
            data: { idObject: idObject, addressType: addressType, idProcess: idProcess },
            success: function (ind) {
                if (ind === 1) {
                    tenancyRentObjectElem.remove();
                    if ($("#TenancyProcessRentObjects .list-group-item").length === 1) {
                        $("#TenancyProcessRentObjects .rr-list-group-item-empty").show();
                    }
                }
                else {
                    alert("Ошибка удаления!");
                }
            }
        });
    }
}

function editTenancyRentObject(e) {
    let tenancyRentObject = $(this).closest(".list-group-item");
    let fields = tenancyRentObject.find('input, select, textarea');
    let yesNoPanel = tenancyRentObject.find('.yes-no-panel');
    let editDelPanel = tenancyRentObject.find('.edit-del-panel');
    fields.filter(function (idx, elem) { return !/^(TotalArea|LivingArea)_/.test($(elem).prop("name")); }).prop('disabled', false);
    tenancyRentObject.find("select").selectpicker('refresh');
    editDelPanel.hide();
    yesNoPanel.show();
    e.preventDefault();
}

function cancelEditTenancyRentObject(e) {
    let tenancyRentObjectElem = $(this).closest(".list-group-item");
    let addressType = tenancyRentObjectElem.find("input[name^='AddressTypePrev']").val();
    //Отменить изменения внесенные в жилье
    if (addressType !== "None") {
        let idStreetElem = tenancyRentObjectElem.find("select[name^='IdStreet']");
        idStreetElem.val(tenancyRentObjectElem.find("input[name^='IdStreetPrev']").val());
        idStreetElem.selectpicker('refresh');
        idStreetElem.change();
        tenancyRentObjectElem.find("input[name^='RentArea_']").val(tenancyRentObjectElem.find("input[name^='RentAreaPrev_']").val());

        showEditDelPanelTenancyRentObject(tenancyRentObjectElem);
        clearValidationsTenancyRentObject(tenancyRentObjectElem);
    }
    //Отменить вставку нового жилья
    else {
        tenancyRentObjectElem.remove();
        if ($("#TenancyProcessRentObjects .list-group-item").length === 1) {
            $("#TenancyProcessRentObjects .rr-list-group-item-empty").show();
        }
    }
    e.preventDefault();
}

function showEditDelPanelTenancyRentObject(tenancyRentObjectElem) {
    let fields = tenancyRentObjectElem.find('input, select, textarea');
    fields.prop('disabled', true).selectpicker('refresh');
    let editDelPanel = tenancyRentObjectElem.find('.edit-del-panel');
    let yesNoPanel = tenancyRentObjectElem.find('.yes-no-panel');
    yesNoPanel.hide();
    tenancyRentObjectElem.removeClass("list-group-item-warning");
    editDelPanel.show();
}

function clearValidationsTenancyRentObject(tenancyRentObjectElem) {
    $(tenancyRentObjectElem).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
    $(tenancyRentObjectElem).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");
}

function refreshValidationTenancyRentObjectForm() {
    var form = $("#TenancyProcessRentObjectsForm")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
}

function getTenancyRentObjects() {
    var items = $("#TenancyProcessRentObjects .list-group-item").filter(function (idx, elem) {
        return !$(elem).hasClass("rr-list-group-item-empty");
    });
    return items.map(function (idx, elem) { return getTenancyRentObject($(elem)) });
}

function getTenancyRentObject(tenancyRentObjectElem) {
    let buildingElem = tenancyRentObjectElem.find("select[id^='IdBuilding']");
    let premisesElem = tenancyRentObjectElem.find("select[id^='IdPremises']");
    let subPremisesElem = tenancyRentObjectElem.find("select[id^='IdSubPremises']");
    let addressType = "None";
    let idObject = 0;
    if (subPremisesElem.val() !== null && subPremisesElem.val() !== "") {
        addressType = "SubPremise";
        idObject = subPremisesElem.val();
    } else
        if (premisesElem.val() !== null && premisesElem.val() !== "") {
            addressType = "Premise";
            idObject = premisesElem.val();
        } else
            if (buildingElem.val() !== null && buildingElem.val() !== "") {
                addressType = "Building";
                idObject = buildingElem.val();
            }
    
    let addressTypePrev = tenancyRentObjectElem.find("input[name^='AddressTypePrev']").val();
    let idObjectPrev = 0;
    switch (addressTypePrev) {
        case "Building":
            idObjectPrev = tenancyRentObjectElem.find("input[name^='IdBuildingPrev']").val();
            break;
        case "Premise":
            idObjectPrev = tenancyRentObjectElem.find("input[name^='IdPremisesPrev']").val();
            break;
        case "SubPremise":
            idObjectPrev = tenancyRentObjectElem.find("input[name^='IdSubPremisesPrev']").val();
            break;
    }
    return {
        IdProcess: $("#TenancyProcessRentObjects").data("id"),
        IdObject: idObject,
        AddressType: addressType,
        IdObjectPrev: idObjectPrev,
        AddressTypePrev: addressTypePrev,
        RentArea: tenancyRentObjectElem.find("input[id^='RentArea_']").val()
    };
}

function tenancyRentObjectToFormData(rentObject) {
    var formData = new FormData();
    for (var property in rentObject) {
        formData.append(property, rentObject[property]);
    }
    return formData;
}

function tenancyRentObjectCustomValidations(tenancyRentObjectElem, validator) {
    var isValid = true;
    var totalAreaElem = tenancyRentObjectElem.find("input[name^='TotalArea']");
    var rentAreaElem = tenancyRentObjectElem.find("input[name^='RentArea_']");
    if (parseFloat(rentAreaElem.val().replace(",", ".")) > parseFloat(totalAreaElem.val().replace(",", "."))) {
        error = {};
        error[rentAreaElem.attr("name")] = "Площадь койко-места не может превышать общую площадь";
        validator.showErrors(error);
        isValid = false;
    }

    return isValid;
}

function saveTenancyRentObject(e) {
    let tenancyRentObjectElem = $(this).closest(".list-group-item");
    var rentAreaElem = tenancyRentObjectElem.find("input[id^='RentArea_']");
    rentAreaElem.val(rentAreaElem.val().replace('.', ','));
    let isValid = tenancyRentObjectElem.find("input, textarea, select").valid();
    if (!tenancyRentObjectCustomValidations(tenancyRentObjectElem, $("#TenancyProcessRentObjectsForm").validate())) {
        isValid = false;
    }
    if (isValid) {
        let rentObject = getTenancyRentObject(tenancyRentObjectElem);
        let tenancyRentObject = tenancyRentObjectToFormData(rentObject);
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyRentObjects/SaveRentObject',
            data: tenancyRentObject,
            processData: false,
            contentType: false,
            success: function (state) {
                if (state.error === 0) {
                    tenancyRentObjectElem.find("input[name^='AddressTypePrev']").val(rentObject.AddressType);
                    tenancyRentObjectElem.find("input[name^='IdStreetPrev']").val(tenancyRentObjectElem.find("select[id^='IdStreet']").val());
                    tenancyRentObjectElem.find("input[name^='IdBuildingPrev']").val(tenancyRentObjectElem.find("select[id^='IdBuilding']").val());
                    tenancyRentObjectElem.find("input[name^='IdPremisesPrev']").val(tenancyRentObjectElem.find("select[id^='IdPremises']").val());
                    tenancyRentObjectElem.find("input[name^='IdSubPremisesPrev']").val(tenancyRentObjectElem.find("select[id^='IdSubPremises']").val());
                    tenancyRentObjectElem.find("input[name^='RentAreaPrev']").val(rentAreaElem.val());
                    showEditDelPanelTenancyRentObject(tenancyRentObjectElem);
                } else {
                    alert("Во время сохранения произошла ошибка");
                }
            }
        });
    } else {
        tenancyRentObjectElem.find("select").each(function (idx, elem) {
            var id = $(elem).prop("id");
            var name = $(elem).prop("name");
            var errorSpan = $("span[data-valmsg-for='" + name + "']");
            if (errorSpan.hasClass("field-validation-error")) {
                $("button[data-id='" + id + "']").addClass("input-validation-error");
            }
        });
        $([document.documentElement, document.body]).animate({
            scrollTop: tenancyRentObjectElem.find(".input-validation-error").first().offset().top - 35
        }, 1000);
    }
    e.preventDefault();
}

$(function () {
    $("#TenancyProcessRentObjectsForm").on("click", "#rentObjectAdd", addTenancyRentObject);
    $('#TenancyProcessRentObjects').on('click', '.tenancy-rent-object-delete-btn', deleteTenancyRentObject);
    $('#TenancyProcessRentObjects').on('click', '.tenancy-rent-object-edit-btn', editTenancyRentObject);
    $('#TenancyProcessRentObjects').on('click', '.tenancy-rent-object-cancel-btn', cancelEditTenancyRentObject);
    $('#TenancyProcessRentObjects').on('click', '.tenancy-rent-object-save-btn', saveTenancyRentObject);
    $('#TenancyProcessRentObjects').on('change', 'select[name^="IdStreet"]', getTenancyRentHouses);
    $('#TenancyProcessRentObjects').on('change', 'select[name^="IdBuilding"]', getTenancyRentPremises);
    $('#TenancyProcessRentObjects').on('change', 'select[name^="IdPremises"]', getTenancyRentSubPremises);
    $('#TenancyProcessRentObjects').on('change', 'select[name^="IdSubPremises"]', updateSubPremisesArea);
    $('#TenancyProcessRentObjects select[name^="IdStreet"]').change();
});