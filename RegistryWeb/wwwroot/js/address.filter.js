var autocompleteFilterOptionsAddress = function () {
    $('#FilterOptions_Address_Text').autocomplete({
        source: function (request, response) {
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/Address/AutocompleteFilterOptionsAddress',
                dataType: 'json',
                data: { text: request.term },
                success: function (data) {
                    if (data !== "0" && data !== undefined) {
                        $('input[name="FilterOptions.Address.AddressType"]').val(data.idAddressType);
                        response($.map(data.autocompletePairs, function (pair) {
                            return { label: pair.item2, value: pair.item2, id: pair.item1 };
                        }));
                    }
                }
            });
        },
        select: function (event, ui) {
            var inputAddressType = $('input[name="FilterOptions.Address.AddressType"]');
            var idAddressType = inputAddressType.val();
            filterClearModal();
            inputAddressType.val(idAddressType);
            $('input[name="FilterOptions.Address.Id"]').val(ui.item.id);
            $('input[name="FilterOptions.Address.Text"]').val(ui.item.value);            
            $("form.filterForm").submit();
        },
        delay: 300,
        minLength: 3
    });
};

var addressClear = function () {
    $('input[name="FilterOptions.Address.AddressType"]').val("");
    $('input[name="FilterOptions.Address.Id"]').val("");
};

var focusOutFilterOptionsAddress = function () {
    if ($('input[name="FilterOptions.Address.Id"]').val() === "") {
        addressClear();
        $('input[name="FilterOptions.Address.Text"]').val("");
    }
};

var addressFilterClear = function () {
    addressClear();
    $("form.filterForm").submit();
};

$(function () {
    autocompleteFilterOptionsAddress();
    $('#FilterOptions_Address_Text').focusout(focusOutFilterOptionsAddress);
    $('#addressFilterClearBtn').click(addressFilterClear);
});