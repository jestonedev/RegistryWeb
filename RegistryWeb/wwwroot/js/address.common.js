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

var typeAddressSelectListChange = function (address) {
    addressListsDisplay(address);
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
    if ($(address).find('.typeAddress').val() == 2 || $(address).find('.typeAddress').val() == 3) {
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
    if ($(address).find('.typeAddress').val() == 3) {
        subPremisesNumSelectListDisplay(address);
    }
}


var addressListsDisplay = function (address) {
    var i = address.attr('data-id');
    //Здание
    if ($(address).find('.typeAddress').val() == 1) {
        $('#premisesTypesGroup_' + i).hide();
        $('#premisesNumGroup_' + i).hide();
        $('#subPremisesNumGroup_' + i).hide();
    }
    //Помещение внутри здания
    if ($(address).find('.typeAddress').val() == 2) {
        $('#premisesTypesGroup_' + i).show();
        $('#premisesNumGroup_' + i).show();
        $('#subPremisesNumGroup_' + i).hide();
    }
    //Квартира внутри помещения
    if ($(address).find('.typeAddress').val() == 3) {
        $('#premisesTypesGroup_' + i).show();
        $('#premisesNumGroup_' + i).show();
        $('#subPremisesNumGroup_' + i).show();
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

var addressDelete = function (id) {
    if ($('.addressBlock').length == 1)
        return;
    $('.addressBlock').filter(function (index) {
        return $(this).attr('data-id') === id;
    }).remove();
    //Преобразование id к числу
    recalculationAddressId(+id);
}

//id обязательно должно быть числом. Иначе некорректная работа
var recalculationAddressId = function (id) {
    if (id == $('.addressBlock').length)
        return;
    for (var i = id; i < $('.addressBlock').length; i++) {
        var oldId = i + 1;
        $('.addressBlock').get(i).setAttribute('data-id', i);
        $('#premisesTypesGroup_' + oldId).attr('id', 'premisesTypesGroup_' + i);
        $('#premisesNumGroup_' + oldId).attr('id', 'premisesNumGroup_' + i);
        $('#subPremisesNumGroup_' + oldId).attr('id', 'subPremisesNumGroup_' + i);
        $('select[name="Addresses[' + oldId + '].IdTypeAddress"]').attr('name', 'Addresses[' + i + '].IdTypeAddress');
        $('select[name="Addresses[' + oldId + '].IdStreet"]').attr('name', 'Addresses[' + i + '].IdStreet');
        $('select[name="Addresses[' + oldId + '].IdBuilding"]').attr('name', 'Addresses[' + i + '].IdBuilding');
        $('select[name="Addresses[' + oldId + '].IdPremisesType"]').attr('name', 'Addresses[' + i + '].IdPremisesType');
        $('select[name="Addresses[' + oldId + '].IdPremise"]').attr('name', 'Addresses[' + i + '].IdPremise');
        $('select[name="Addresses[' + oldId + '].IdSubPremise"]').attr('name', 'Addresses[' + i + '].IdSubPremise');
    }
}

$(function () {
    $('#addresses').change(function (event) {
        var address = $(event.target).parents().filter('.addressBlock');
        if ($(event.target).hasClass('typeAddress'))
            typeAddressSelectListChange(address);
        if ($(event.target).hasClass('kladrStreets'))
            kladrStreetSelectListChange(address);
        if ($(event.target).hasClass('house'))
            builidngSelectListChange(address);
        if ($(event.target).hasClass('premisesTypes'))
            premisesTypesSelectListChange(address);
        if ($(event.target).hasClass('premisesNum'))
            premisesNumSelectListChange(address);
    });
    $('#addresses').click(function (event) {
        var id = $(event.target).parents().filter('.addressBlock').attr('data-id');
        if ($(event.target).hasClass('oi-x'))
            addressDelete(id);
    });
    addressListsDisplay($('.addressBlock'));
    autocompleteStreet($('.addressBlock'));
});