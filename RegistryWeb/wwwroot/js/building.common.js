let getBuilding = function () {
    let building = {
        id_building: $('#building [name="IdBuilding"]').val(),
        id_street: $('#building [name="IdStreet"]').val(),
        house: $('#building [name="House"]').val(),
        id_decree: $('#building [name="IdDecree"]').val(),
        id_state: $('#building [name="IdState"]').val(),
        id_structure_type: $('#building [name="IdStructureType"]').val(),
        id_structure_type_overlap: $('#building [name="IdStructureTypeOverlap"]').val(),
        id_heating_type: $('#building [name="IdHeatingType"]').val(),
        total_area: $('#building [name="TotalArea"]').val(),
        living_area: $('#building [name="LivingArea"]').val(),
        unliving_area: $('#building [name="UnlivingArea"]').val(),
        common_property_area: $('#building [name="CommonPropertyArea"]').val(),
        floors: $('#building [name="Floors"]').val(),
        entrances: $('#building [name="Entrances"]').val(),
        num_premises: $('#building [name="NumPremises"]').val(),
        num_rooms: $('#building [name="NumRooms"]').val(),
        num_apartments: $('#building [name="NumApartments"]').val(),
        num_shared_apartments: $('#building [name="NumSharedApartments"]').val(),
        series: $('#building [name="Series"]').val(),
        cadastral_num: $('#building [name="CadastralNum"]').val(),
        cadastral_cost: $('#building [name="CadastralCost"]').val(),
        balance_cost: $('#building [name="BalanceCost"]').val(),
        land_cadastral_num: $('#building [name="LandCadastralNum"]').val(),
        land_cadastral_date: $('#building [name="LandCadastralDate"]').val(),
        land_area: $('#building [name="LandArea"]').val(),
        startup_year: $('#building [name="StartupYear"]').val(),
        reg_date: $('#building [name="RegDate"]').val(),
        date_owner_emergency: $('#building [name="DateOwnerEmergency"]').val(),
        demolished_fact_date: $('#building [name="DemolishedFactDate"]').val(),
        is_memorial: $('#building [name="IsMemorial"]').val(),
        memorial_number: $('#building [name="MemorialNumber"]').val(),
        memorial_date: $('#building [name="MemorialDate"]').val(),
        memorial_name_org: $('#building [name="MemorialNameOrg"]').val(),
        improvement: $('#building [name="Improvement"]').val(),
        elevator: $('#building [name="Elevator"]').val(),
        rubbish_chute: $('#building [name="RubbishChute.Value"]').val(),
        plumbing: $('#building [name="Plumbing.Value"]').val(),
        hot_water_supply: $('#building [name="HotWaterSupply.Value"]').val(),
        canalization: $('#building [name="Canalization.Value"]').val(),
        electricity: $('#building [name="Electricity.Value"]').val(),
        radio_network: $('#building [name="RadioNetwork.Value"]').val(),
        wear: $('#building [name="Wear"]').val(),
        rent_coefficient: $('#building [name="RentCoefficient"]').val(),
        housing_cooperative: $('#building [name="HousingCooperative"]').val(),
        description: $('#building [name="Description"]').val(),
        BTI_rooms: $('#building [name="BtiRooms"]').val()
    };
    return building;
}
let createBuilding = function () {
    console.log('asdf');
    let owrs = getOwnershipRights();
    let building = getBuilding();
    console.log(owrs);
    console.log(building);
    $.ajax({
        async: false,
        type: 'POST',
        url: window.location.origin + '/Buildings/Create',
        data: { owrs },
        success: function () {
            alert('good!');
        }
    });
}
$(function () {
    $('#buildingToggle').on('click', $('#building'), elementToogle);
    $('#createBtn').click(createBuilding);
    var action = $('#building').data("action");
    if (action == "Details" || action == "Delete") {
        $('#building select').prop('disabled', true);
        $('#building input').prop('disabled', true);
        $('#building textarea').prop('disabled', true);
        $('#building input[type="hidden"]').prop('disabled', false);
        $('#building input[type="submit"]').prop('disabled', false);
    }
});