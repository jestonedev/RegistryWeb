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
            $(address).find('.kladrStreets').val(ui.item.idStreet);
            kladrStreetSelectListChange(address);
        },
        minLength: 3
    });
};

var typeAddressSelectListChange = function (address) {
    recalculationTypeAddressChange(address);
    addressListsDisplay(address);
    kladrStreetSelectListChange(address);
}

var recalculationTypeAddressChange = function (address) {
    var addresses = $('.addressBlock');
    var id = addresses.index(address);
    
    var oldClass = "";
    if (address.hasClass('buildingBlock'))
        oldClass = "buildingBlock";
    else if (address.hasClass('premiseBlock'))
        oldClass = "premiseBlock";
    else if (address.hasClass('subPremiseBlock'))
        oldClass = "subPremiseBlock";
    address.removeClass(oldClass);

    var newIdTypeAddr = address.find('.typeAddress').val();
    var newClass = "";
    if (newIdTypeAddr == 1)
        newClass = "buildingBlock";
    else if (newIdTypeAddr == 2)
        newClass = "premiseBlock";
    else if (newIdTypeAddr == 3)
        newClass = "subPremiseBlock";
    address.addClass(newClass);

    var indOld = -1;
    var indNew = -1;
    for (var i = 0; i < addresses.length; i++) {
        if (addresses[i].classList.contains(oldClass)) {
            indOld++;
            if (i > id) {
                addresses[i].setAttribute('data-id', indOld);
                recalculationId(addresses[i], indOld, oldClass);
            }
        }
        if (addresses[i].classList.contains(newClass)) {
            indNew++;
            if (i >= id) {
                addresses[i].setAttribute('data-id', indNew);
                recalculationId(addresses[i], indNew, newClass);
            }
        }
    }
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
    if ($(address).find('.typeAddress').val() == 1) {
        $(address).find('.addressIdAssoc').val(0);
        $(address).find('.addressId').val($(address).find('.house').val());
    }
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
    if ($(address).find('.typeAddress').val() == 2) {
        $(address).find('.addressIdAssoc').val(0);
        $(address).find('.addressId').val($(address).find('.premisesNum').val());
    }
    if ($(address).find('.typeAddress').val() == 3) {
        subPremisesNumSelectListDisplay(address);
        subPremisesNumSelectListChange(address);
    }
}

var subPremisesNumSelectListChange = function (address) {
    if ($(address).find('.typeAddress').val() == 3) {
        $(address).find('.addressIdAssoc').val(0);
        $(address).find('.addressId').val($(address).find('.subPremisesNum').val());
    }
}


var addressListsDisplay = function (address) {
    //Здание
    if ($(address).find('.typeAddress').val() == 1) {
        $(address).find('.premisesTypesGroup').hide();
        $(address).find('.premisesNumGroup').hide();
        $(address).find('.subPremisesNumGroup').hide();
    }
    //Помещение внутри здания
    if ($(address).find('.typeAddress').val() == 2) {
        $(address).find('.premisesTypesGroup').show();
        $(address).find('.premisesNumGroup').show();
        $(address).find('.subPremisesNumGroup').hide();
    }
    //Квартира внутри помещения
    if ($(address).find('.typeAddress').val() == 3) {
        $(address).find('.premisesTypesGroup').show();
        $(address).find('.premisesNumGroup').show();
        $(address).find('.subPremisesNumGroup').show();
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

var addressDelete = function (address) {
    if ($('.addressBlock').length == 1)
        return;
    var id = +address.attr('data-id'); //Преобразование id к числу
    var idTypeAddress = address.find('.typeAddress').val();
    address.remove();
    var className = "";
    if (idTypeAddress == 1)
        className = "buildingBlock";
    else if (idTypeAddress == 2)
        className = "premiseBlock";
    else if (idTypeAddress == 3)
        className = "subPremiseBlock";
    var list = $('.' + className);
    if (id == list.length)
        return;
    for (var i = id; i < list.length; i++) {
        list[i].setAttribute('data-id', i);
        recalculationId(list[i], i, className);
    }
}

var recalculationId = function (address, id, className) {
    var addressAssoc = "";
    var idAddressAssoc = "";
    if (className == "buildingBlock") {
        addressAssoc = "OwnerBuildingsAssoc";
        idAddressAssoc = "IdBuilding";
    }
    else if (className == "premiseBlock") {
        addressAssoc = "OwnerPremisesAssoc";
        idAddressAssoc = "IdPremise";
    }
    else if (className == "subPremiseBlock") {
        addressAssoc = "OwnerSubPremisesAssoc";
        idAddressAssoc = "IdSubPremise";
    }
    $(address).find('.addressIdProcess').attr('name', addressAssoc + '[' + id + '].IdProcess');
    $(address).find('.addressIdAssoc').attr('name', addressAssoc + '[' + id + '].IdAssoc');
    $(address).find('.addressId').attr('name', addressAssoc + '[' + id + '].' + idAddressAssoc);
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
        if ($(event.target).hasClass('subPremisesNum'))
            subPremisesNumSelectListChange(address);
    });
    $('#addresses').click(function (event) {
        if ($(event.target).hasClass('oi-x')) {
            var address = $(event.target).parents().filter('.addressBlock');
            addressDelete(address);
        }
    });
    for (var i = 0; i < $('.addressBlock').length; i++) {
        addressListsDisplay($('.addressBlock')[i]);
        autocompleteStreet($('.addressBlock')[i]);
    }
});