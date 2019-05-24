var autocompleteStreet = function (address) {
    $(address).find('.street').autocomplete({
        source: function (request, response) {
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/Address/AutocompleteStreet',
                dataType: 'json',
                data: { text: request.term },
                success: function (data) {
                    response($.map(data, function (item) {
                        return { label: item.streetName, value: item.streetName, idStreet: item.idStreet };
                    }))
                }
            });
        },
        select: function (event, ui) {
            var i = $(address).attr('data-id');
            $(address).find('.kladrStreets').val(ui.item.idStreet);
            kladrStreetSelectListChange(address);
        },
        minLength: 3,
        messages: {
            noResults: '',
            results: ''
        }
    });
};

var typeAddressRadiobuttonChange = function (address) {
    var i = address.attr('data-id');
    //Здание
    if ($('#radio1_' + i).is(':checked')) {
        $('#premisesTypesGroup_' + i).hide();
        $('#premisesNumGroup_' + i).hide();
        $('#subPremisesNumGroup_' + i).hide();
    }
    //Помещение внутри здания
    if ($('#radio2_' + i).is(':checked')) {
        $('#premisesTypesGroup_' + i).show();
        $('#premisesNumGroup_' + i).show();
        $('#subPremisesNumGroup_' + i).hide();
    }
    //Квартира внутри помещения
    if ($('#radio3_' + i).is(':checked')) {
        $('#premisesTypesGroup_' + i).show();
        $('#premisesNumGroup_' + i).show();
        $('#subPremisesNumGroup_' + i).show();
    }
    kladrStreetSelectListChange(address);
}

var kladrStreetSelectListChange = function (address) {
    $(address).find('.house').empty();
    $(address).find('.premisesTypes').empty();
    $(address).find('.premisesNum').empty();
    $(address).find('.subPremisesNum').empty();
    builidngSelectListDisplay(address);
    builidngSelectListChange(address)
};

var builidngSelectListChange = function (address) {
    var i = address.attr('data-id');
    //Помещение внутри здания
    if ($('#radio2_' + i).is(':checked') || $('#radio3_' + i).is(':checked')) {
        premisesTypeSelectListDisplay(address);
        premisesTypesSelectListChange(address);
    }
}

var premisesTypesSelectListChange = function (address) {
    premisesTypeAsNumTextDisplay(address);
    premisesNumSelectListDisplay(address);
    premisesNumSelectListChange(address);
};

var premisesNumSelectListChange = function (address) {
    var i = address.attr('data-id');
    //Квартира внутри помещения
    if ($('#radio3_' + i).is(':checked')) {
        subPremisesNumSelectListDisplay(address);
    }
}


var builidngSelectListDisplay = function (address) {
    var idStreet = $(address).find('.kladrStreets').val();
    $.ajax({
        async: false,
        type: 'POST',
        url: window.location.origin + '/Address/GetBuildingsSelectList',
        data: { idStreet: idStreet },
        success: function (data) {
            $(address).find('.house').empty();
            $(address).find('.house').append(data);
        }
    });
}

var premisesTypeSelectListDisplay = function (address) {
    var idBuilding = $(address).find('.house').val();
    $.ajax({
        async: false,
        type: 'POST',
        url: window.location.origin + '/Address/GetPremisesTypeSelectList',
        data: { idBuilding: idBuilding },
        success: function (data) {
            $(address).find('.premisesTypes').empty();
            $(address).find('.premisesTypes').append(data);
        }
    });
};

var premisesTypeAsNumTextDisplay = function (address) {
    var idPremisesType = $(address).find('.premisesTypes').val();
    $.ajax({
        async: false,
        type: 'POST',
        url: window.location.origin + '/Address/GetPremisesTypeAsNumText',
        data: { idPremisesType: idPremisesType },
        success: function (data) {
            $(address).find('.premisesTypeAsNum').empty();
            $(address).find('.premisesTypeAsNum').append(data.premisesTypeAsNum);
        }
    });
};

var premisesNumSelectListDisplay = function (address) {
    var idPremisesType = $(address).find('.premisesTypes').val();
    var idBuilding = $(address).find('.house').val();
    $.ajax({
        async: false,
        type: 'POST',
        url: window.location.origin + '/Address/GetPremisesNumSelectList',
        data: { idBuilding: idBuilding, idPremisesType: idPremisesType },
        success: function (data) {
            $(address).find('.premisesNum').empty();
            $(address).find('.premisesNum').append(data);
        }
    });
};

var subPremisesNumSelectListDisplay = function (address) {
    var idPremise = $(address).find('.premisesNum').val();
    $.ajax({
        async: false,
        type: 'POST',
        url: window.location.origin + '/Address/GetSubPremisesNumSelectList',
        data: { idPremise: idPremise },
        success: function (data) {
            $(address).find('.subPremisesNum').empty();
            $(address).find('.subPremisesNum').append(data);
        }
    });
};

$(function () {
    $('#addresses').change(function (event) {
        var address = $(event.target).parents().filter('.address');
        if ($(event.target).hasClass('typeAddress'))
            typeAddressRadiobuttonChange(address);
        if ($(event.target).hasClass('kladrStreets'))
            kladrStreetSelectListChange(address);
        if ($(event.target).hasClass('house'))
            builidngSelectListChange(address);
        if ($(event.target).hasClass('premisesTypes'))
            premisesTypesSelectListChange(address);
        if ($(event.target).hasClass('premisesNum'))
            premisesNumSelectListChange(address);
    });

    $('.typeAddress').checkboxradio({ icon: false });
    typeAddressRadiobuttonChange($('.address'));
    autocompleteStreet($('.address'));
});