function autocompleteFilterOptionsAddress() {
    $('#addressSearch').autocomplete({
        source: function (request, response) {
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/Address/AutocompleteFilterOptionsAddress',
                dataType: 'json',
                data: { text: request.term },
                success: function (data) {
                    if (data !== "0" && data !== undefined) {
                        response($.map(data.autocompletePairs, function (pair) {
                            return { label: pair.item2, value: pair.item2, id: pair.item1, addressType: data.addressType };
                        }));
                    }
                }
            });
        },
        select: function (event, ui) {
            $('#addressClearBtn').removeClass('disabled');
            $('#addressAddBtn').removeClass('disabled');
            $('#addressSearch')
                .attr('readonly', true)
                .data('id', ui.item.id)
                .data('addresstype', ui.item.addressType);
        },
        delay: 300,
        minLength: 3
    });
};
function recalculationAddresses(li, i) {
    li.data('i', i);
    var inputs = li.find('input');
    var addressType = li.data('addresstype');
    if (addressType == "Building") {
        assocName = "OwnerBuildingsAssoc";
        idName = "IdBuilding";
    }
    else if (addressType == "Premise") {
        assocName = "OwnerPremisesAssoc";
        idName = "IdPremise";
    }
    else if (addressType == "SubPremise") {
        assocName = "OwnerSubPremisesAssoc";
        idName = "IdSubPremise";
    }
    inputs[0].name = assocName + "[" + i + "].IdProcess";
    inputs[1].name = assocName + "[" + i + "].IdAssoc";
    inputs[2].name = assocName + "[" + i + "]." + idName;
}
function addressClear() {
    $('#addressClearBtn').addClass('disabled');
    $('#addressAddBtn').addClass('disabled');
    $('#addressSearch')
        .data('idaddresstype', '')
        .val(null)
        .attr('readonly', false);
}
function addressAdd() {
    var addressSearch = $('#addressSearch');
    var addressType = addressSearch.data('addresstype');
    if (addressType != "Building" && addressType != "Premise" && addressType != "SubPremise") {
        alert("Выбранный адрес не является зданием, помещением или комантой!")
        addressClear();
        return;
    }
    var addressVM = {
        idProcess: $('input[name="IdProcess"]').val(),
        id: addressSearch.data('id'),
        i: $('.address').filter(function (index) {
            var type = $(this).parents('li').data('addresstype');
            return type == addressType;
        }).length,
        text: addressSearch.val(),
        addressType: addressType,
        action: $('#ownerProcessForm').data('action')
    };
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/OwnerProcesses/AddressAdd',
        data: addressVM,
        success: function (data) {
            $('#addresses').append(data);
            if ($('.address').length == 1) {
                $('.addressDeleteBtn').addClass('disabled');
            }
            else {
                $('.addressDeleteBtn').removeClass('disabled');
            }
            $('#addressSearch')
                .attr('readonly', false)
                .removeClass('input-validation-error');
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.log(jqXHR);
            console.log(textStatus);
            console.log(errorThrown);
        }
    });
    addressClear();
}
function addressDelete(li) {
    var addressType = li.data('addresstype');
    li.remove();
    var i = 0;
    $('#addresses li').each(function (index) {
        if ($(this).data('addresstype') == addressType) {
            recalculationAddresses($(this), i)
            i++;
        }
    });
    if ($('.address').length == 1) {
        $('.addressDeleteBtn').addClass('disabled');
    }
}
$(function () {
    autocompleteFilterOptionsAddress();
    if ($('.address').length == 1) {
        $('.addressDeleteBtn').addClass('disabled');
    }
    $('#addresses').click(function (event) {
        //Удаление адрессов
        if ($(event.target).hasClass('addressDeleteBtn')) {
            addressDelete($(event.target).parents('li'))
        }
    });
    $('#addressClearBtn').click(addressClear);
    $('#addressAddBtn').click(addressAdd);
});