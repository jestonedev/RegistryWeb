let getBuilding = function () {
    let building = {
        IdBuilding: $('#building [name="IdBuilding"]').val(),
        IdStreet: $('#building [name="IdStreet"]').val(),
        House: $('#building [name="House"]').val(),
        IdDecree: $('#building [name="IdDecree"]').val(),
        IdState: $('#building [name="IdState"]').val(),
        IdStructureType: $('#building [name="IdStructureType"]').val(),
        IdStructureTypeOverlap: $('#building [name="IdStructureTypeOverlap"]').val(),
        IdHeatingType: $('#building [name="IdHeatingType"]').val(),
        TotalArea: $('#building [name="TotalArea"]').val(),
        LivingArea: $('#building [name="LivingArea"]').val(),
        UnlivingArea: $('#building [name="UnlivingArea"]').val(),
        CommonPropertyArea: $('#building [name="CommonPropertyArea"]').val(),
        Floors: $('#building [name="Floors"]').val(),
        Entrances: $('#building [name="Entrances"]').val(),
        NumPremises: $('#building [name="NumPremises"]').val(),
        NumRooms: $('#building [name="NumRooms"]').val(),
        NumApartments: $('#building [name="NumApartments"]').val(),
        NumSharedApartments: $('#building [name="NumSharedApartments"]').val(),
        Series: $('#building [name="Series"]').val(),
        CadastralNum: $('#building [name="CadastralNum"]').val(),
        CadastralCost: $('#building [name="CadastralCost"]').val(),
        BalanceCost: $('#building [name="BalanceCost"]').val(),
        LandCadastralNum: $('#building [name="LandCadastralNum"]').val(),
        LandCadastralDate: $('#building [name="LandCadastralDate"]').val(),
        LandArea: $('#building [name="LandArea"]').val(),
        StartupYear: $('#building [name="StartupYear"]').val(),
        RegDate: $('#building [name="RegDate"]').val(),
        DateOwnerEmergency: $('#building [name="DateOwnerEmergency"]').val(),
        DemolishedFactDate: $('#building [name="DemolishedFactDate"]').val(),
        IsMemorial: $('#building [name="IsMemorial"]').is(':checked'),
        MemorialNumber: $('#building [name="MemorialNumber"]').val(),
        MemorialDate: $('#building [name="MemorialDate"]').val(),
        MemorialNameOrg: $('#building [name="MemorialNameOrg"]').val(),
        Improvement: $('#building [name="Improvement"]').is(':checked'),
        Elevator: $('#building [name="Elevator"]').is(':checked'),
        RubbishChute: $('#building [name="RubbishChute.Value"]').is(':checked'),
        Plumbing: $('#building [name="Plumbing.Value"]').is(':checked'),
        HotWaterSupply: $('#building [name="HotWaterSupply.Value"]').is(':checked'),
        Canalization: $('#building [name="Canalization.Value"]').is(':checked'),
        Electricity: $('#building [name="Electricity.Value"]').is(':checked'),
        RadioNetwork: $('#building [name="RadioNetwork.Value"]').is(':checked'),
        Wear: $('#building [name="Wear"]').val(),
        RentCoefficient: $('#building [name="RentCoefficient"]').val(),
        HousingCooperative: $('#building [name="HousingCooperative"]').val(),
        Description: $('#building [name="Description"]').val(),
        BtiRooms: $('#building [name="BtiRooms"]').val(),

        OwnershipBuildingsAssoc: CreateOwnershipBuildingsAssoc()
    };
    return building;
}
let getBuildingFormData = function () {
    let buildingFormData = new FormData();
    buildingFormData.append('IdBuilding', $('#building [name="IdBuilding"]').val());
    buildingFormData.append('IdStreet', $('#building [name="IdStreet"]').val());
    buildingFormData.append('House', $('#building [name="House"]').val());
    buildingFormData.append('IdDecree', $('#building [name="IdDecree"]').val());
    buildingFormData.append('IdState', $('#building [name="IdState"]').val());
    buildingFormData.append('IdStructureType', $('#building [name="IdStructureType"]').val());
    buildingFormData.append('IdStructureTypeOverlap', $('#building [name="IdStructureTypeOverlap"]').val());
    buildingFormData.append('IdHeatingType', $('#building [name="IdHeatingType"]').val());
    buildingFormData.append('TotalArea', $('#building [name="TotalArea"]').val());
    buildingFormData.append('LivingArea', $('#building [name="LivingArea"]').val());
    buildingFormData.append('UnlivingArea', $('#building [name="UnlivingArea"]').val());
    buildingFormData.append('CommonPropertyArea', $('#building [name="CommonPropertyArea"]').val());
    buildingFormData.append('Floors', $('#building [name="Floors"]').val());
    buildingFormData.append('Entrances', $('#building [name="Entrances"]').val());
    buildingFormData.append('NumPremises', $('#building [name="NumPremises"]').val());
    buildingFormData.append('NumRooms', $('#building [name="NumRooms"]').val());
    buildingFormData.append('NumApartments', $('#building [name="NumApartments"]').val());
    buildingFormData.append('NumSharedApartments', $('#building [name="NumSharedApartments"]').val());
    buildingFormData.append('Series', $('#building [name="Series"]').val());
    buildingFormData.append('CadastralNum', $('#building [name="CadastralNum"]').val());
    buildingFormData.append('CadastralCost', $('#building [name="CadastralCost"]').val());
    buildingFormData.append('BalanceCost', $('#building [name="BalanceCost"]').val());
    buildingFormData.append('LandCadastralNum', $('#building [name="LandCadastralNum"]').val());
    buildingFormData.append('LandCadastralDate', $('#building [name="LandCadastralDate"]').val());
    buildingFormData.append('LandArea', $('#building [name="LandArea"]').val());
    buildingFormData.append('StartupYear', $('#building [name="StartupYear"]').val());
    buildingFormData.append('RegDate', $('#building [name="RegDate"]').val());
    buildingFormData.append('DateOwnerEmergency', $('#building [name="DateOwnerEmergency"]').val());
    buildingFormData.append('DemolishedFactDate', $('#building [name="DemolishedFactDate"]').val());
    buildingFormData.append('IsMemorial', $('#building [name="IsMemorial"]').is(':checked'));
    buildingFormData.append('MemorialNumber', $('#building [name="MemorialNumber"]').val());
    buildingFormData.append('MemorialDate', $('#building [name="MemorialDate"]').val());
    buildingFormData.append('MemorialNameOrg', $('#building [name="MemorialNameOrg"]').val());
    buildingFormData.append('Improvement', $('#building [name="Improvement"]').is(':checked'));
    buildingFormData.append('Elevator', $('#building [name="Elevator"]').is(':checked'));
    buildingFormData.append('RubbishChute', $('#building [name="RubbishChute"]').is(':checked'));
    buildingFormData.append('Plumbing', $('#building [name="Plumbing"]').is(':checked'));
    buildingFormData.append('HotWaterSupply', $('#building [name="HotWaterSupply"]').is(':checked'));
    buildingFormData.append('Canalization', $('#building [name="Canalization"]').is(':checked'));
    buildingFormData.append('Electricity', $('#building [name="Electricity"]').is(':checked'));
    buildingFormData.append('RadioNetwork', $('#building [name="RadioNetwork"]').is(':checked'));
    buildingFormData.append('Wear', $('#building [name="Wear"]').val());
    buildingFormData.append('RentCoefficient', $('#building [name="RentCoefficient"]').val());
    buildingFormData.append('HousingCooperative', $('#building [name="HousingCooperative"]').val());
    buildingFormData.append('Description', $('#building [name="Description"]').val());
    buildingFormData.append('BtiRooms', $('#building [name="BtiRooms"]').val());
    let restrictions = CreateRestrictionBuildingsAssoc();
    for (let i = 0; i < restrictions.length; i++) {
        buildingFormData.append('RestrictionBuildingsAssoc[' + i + '].IdBuilding', $('#building [name="IdBuilding"]').val());
        buildingFormData.append('RestrictionBuildingsAssoc[' + i + '].IdRestriction', restrictions[i].RestrictionNavigation.IdRestriction);
        buildingFormData.append('RestrictionBuildingsAssoc[' + i + '].RestrictionNavigation.IdRestriction', restrictions[i].RestrictionNavigation.IdRestriction);
        buildingFormData.append('RestrictionBuildingsAssoc[' + i + '].RestrictionNavigation.Number', restrictions[i].RestrictionNavigation.Number);
        buildingFormData.append('RestrictionBuildingsAssoc[' + i + '].RestrictionNavigation.Date', restrictions[i].RestrictionNavigation.Date);
        buildingFormData.append('RestrictionBuildingsAssoc[' + i + '].RestrictionNavigation.Description', restrictions[i].RestrictionNavigation.Description);
        buildingFormData.append('RestrictionBuildingsAssoc[' + i + '].RestrictionNavigation.IdRestrictionType', restrictions[i].RestrictionNavigation.IdRestrictionType);
        buildingFormData.append('RestrictionFile[' + i + ']', restrictions[i].RestrictionNavigation.RestrictionFile.files[0]);
    }
    let obas = CreateOwnershipBuildingsAssoc();
    for (let i = 0; i < obas.length; i++) {
        buildingFormData.append('OwnershipBuildingsAssoc[' + i + '].IdBuilding', $('#building [name="IdBuilding"]').val());
        buildingFormData.append('OwnershipBuildingsAssoc[' + i + '].IdOwnershipRight', obas[i].OwnershipRightNavigation.IdOwnershipRight);
        buildingFormData.append('OwnershipBuildingsAssoc[' + i + '].OwnershipRightNavigation.IdOwnershipRight', obas[i].OwnershipRightNavigation.IdOwnershipRight);
        buildingFormData.append('OwnershipBuildingsAssoc[' + i + '].OwnershipRightNavigation.Number', obas[i].OwnershipRightNavigation.Number);
        buildingFormData.append('OwnershipBuildingsAssoc[' + i + '].OwnershipRightNavigation.Date', obas[i].OwnershipRightNavigation.Date);
        buildingFormData.append('OwnershipBuildingsAssoc[' + i + '].OwnershipRightNavigation.Description', obas[i].OwnershipRightNavigation.Description);
        buildingFormData.append('OwnershipBuildingsAssoc[' + i + '].OwnershipRightNavigation.IdOwnershipRightType', obas[i].OwnershipRightNavigation.IdOwnershipRightType);
        buildingFormData.append('OwnershipBuildingsAssoc[' + i + '].OwnershipRightNavigation.ResettlePlanDate', obas[i].OwnershipRightNavigation.ResettlePlanDate);
        buildingFormData.append('OwnershipBuildingsAssoc[' + i + '].OwnershipRightNavigation.DemolishPlanDate', obas[i].OwnershipRightNavigation.DemolishPlanDate);
        buildingFormData.append('OwnershipRightFile[' + i + ']', obas[i].OwnershipRightNavigation.OwnershipRightFile.files[0]);
    }    
    let buildingDemolitionInfo = _buildingDemolitionInfo.getJson();
    buildingFormData.append('DemolishedPlanDate', buildingDemolitionInfo.DemolishedPlanDate);
    let buildingDemolitionActFiles = buildingDemolitionInfo.BuildingDemolitionActFiles;
    for (let i = 0; i < buildingDemolitionActFiles.length; i++) {
        buildingFormData.append('BuildingDemolitionActFiles[' + i + '].Id', buildingDemolitionActFiles[i].Id);
        buildingFormData.append('BuildingDemolitionActFiles[' + i + '].IdBuilding', buildingDemolitionActFiles[i].IdBuilding);
        buildingFormData.append('BuildingDemolitionActFiles[' + i + '].IdActFile', buildingDemolitionActFiles[i].IdActFile);
        buildingFormData.append('BuildingDemolitionActFiles[' + i + '].IdActTypeDocument', buildingDemolitionActFiles[i].IdActTypeDocument);
        buildingFormData.append('BuildingDemolitionActFiles[' + i + '].Number', buildingDemolitionActFiles[i].Number);
        buildingFormData.append('BuildingDemolitionActFiles[' + i + '].Date', buildingDemolitionActFiles[i].Date);
        buildingFormData.append('BuildingDemolitionActFiles[' + i + '].Name', buildingDemolitionActFiles[i].Name);
        buildingFormData.append('BuildingDemolitionActFile[' + i + ']', buildingDemolitionActFiles[i].File);
    }
    return buildingFormData;
}
let getInputTemplate = function (name, value) {
    return '<input type="hidden" name="' + name + '" value="' + value + '">';
};

let createBuildingClick = function (event) {
    event.preventDefault();
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
        let buildingDemolitionActFiles = buildingDemolitionInfo.BuildingDemolitionActFiles;
        for (let i = 0; i < buildingDemolitionActFiles.length; i++) {
            let bdaf = "BuildingDemolitionActFiles[" + i + "].";
            $('#building').append(getInputTemplate(bdaf + 'Id', buildingDemolitionActFiles[i].Id));
            $('#building').append(getInputTemplate(bdaf + 'IdBuilding', buildingDemolitionActFiles[i].Id));
            $('#building').append(getInputTemplate(bdaf + 'IdActFile', buildingDemolitionActFiles[i].Id));
            $('#building').append(getInputTemplate(bdaf + 'IdActTypeDocument', buildingDemolitionActFiles[i].Id));
            $('#building').append(getInputTemplate(bdaf + 'Number', buildingDemolitionActFiles[i].Id));
            $('#building').append(getInputTemplate(bdaf + 'Date', buildingDemolitionActFiles[i].Id));
            $('#building').append(getInputTemplate(bdaf + 'Name', buildingDemolitionActFiles[i].Id));
            let file = $(buildingDemolitionActFiles[i].FileInput).clone();
            file.attr("name", "BuildingDemolitionActFile[" + i + "]");
            $('#building').append(file);
        }
        $('#building').submit();
    }
};

let editBuildingClick = function (e) {
    if ($("#editBtn").hasClass("disabled")) {
        e.preventDefault();
        return;
    }
    let cadastralCost = $('#building [name="CadastralCost"]');
    let balanceCost = $('#building [name="BalanceCost"]');
    let rentCoefficient = $('#building [name="RentCoefficient"]');
    cadastralCost.val(cadastralCost.val().replace('.', ','));
    balanceCost.val(balanceCost.val().replace('.', ','));
    rentCoefficient.val(rentCoefficient.val().replace('.', ','));
    $('#building').submit();
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
    if (action == "Details" || action == "Delete" || !canEditBaseInfo) {
        $('#building select').prop('disabled', true);
        $('#building input').prop('disabled', true);
        $('#building textarea').prop('disabled', true);
        $('#building input[type="hidden"]').prop('disabled', false);
        $('#building input[type="submit"]').prop('disabled', false);
    }
    $('#buildingToggle').on('click', $('#building'), elementToogle);
    $('#createBtn').click(createBuildingClick);
    $('#editBtn').click(editBuildingClick);
    $('#IsMemorial').click(memorialCardClick);
});