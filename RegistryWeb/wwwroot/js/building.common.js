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
let createBuildingClick = function () {
    let building = getBuilding();
    //Запуск ручной валидации, тк отсутсвует submit
    var buildingIsValid = $('#building').valid();
    var ownershipRightsIsValid = $("#ownershipRightsForm").valid();
    if (buildingIsValid && ownershipRightsIsValid) {
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/Buildings/Create',
            data: { building },
            success: function (idBuilding) {
                if (idBuilding < 1) {
                    alert('Ошибка создания!');
                }
                window.location.href = window.location.origin + '/Buildings';
            }
        });
    }
}
let editBuildingClick = function () {
    let cadastralCost = $('#building [name="CadastralCost"]');
    let balanceCost = $('#building [name="BalanceCost"]');
    let rentCoefficient = $('#building [name="RentCoefficient"]');
    cadastralCost.val(cadastralCost.val().replace('.', ','));
    balanceCost.val(balanceCost.val().replace('.', ','));
    rentCoefficient.val(rentCoefficient.val().replace('.', ','));
    $('#building').submit();
}
let memorialCardClick = function () {
    var isMemorial = $('#IsMemorial').is(':checked');
    $('#MemorialNumber').prop('disabled', !isMemorial);
    $('#MemorialDate').prop('disabled', !isMemorial);
    $('#MemorialNameOrg').prop('disabled', !isMemorial);
}
$(function () {
    var action = $('#building').data("action");
    if (action == "Details" || action == "Delete") {
        $('#building select').prop('disabled', true);
        $('#building input').prop('disabled', true);
        $('#building textarea').prop('disabled', true);
        $('#building input[type="hidden"]').prop('disabled', false);
        $('#building input[type="submit"]').prop('disabled', false);
    }
    memorialCardClick();
    $('#buildingToggle').on('click', $('#building'), elementToogle);
    $('#createBtn').click(createBuildingClick);
    $('#editBtn').click(editBuildingClick);
    $('#IsMemorial').click(memorialCardClick);
});