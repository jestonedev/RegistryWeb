var autocompleteFilterOptionsAddress = function () {
    $('input[name="FilterOptions.Address.Text"]').autocomplete({
        source: function (request, response) {
            var self = $(this.element);
            var isBuildings = self.data("isBuildings");
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/Address/AutocompleteFilterOptionsAddress' + (self.hasClass("rr-alt-address-search") ? "Alt" : ""),
                dataType: 'json',
                data: { text: request.term, isBuildings: isBuildings },
                success: function (data) {
                    if (data !== "0" && data !== undefined) {
                        if (self.hasClass("rr-alt-address-search")) {
                            response($.map(data.addresses, function (addr) {
                                return { label: addr, value: addr, type: data.addressType };
                            }));
                        } else {
                            response($.map(data.autocompletePairs, function (pair) {
                                return { label: pair.item2, value: pair.item2, id: pair.item1, type: data.addressType };
                            }));
                        }
                    }
                }
            });
        },
        select: function (event, ui) {
            var inputAddressType = $('input[name="FilterOptions.Address.AddressType"]');
            var inputAddressText = $('input[name="FilterOptions.Address.Text"]');
                filterClearModal();
            inputAddressType.val(ui.item.type);
            if (!inputAddressText.hasClass("rr-alt-address-search"))
                $('input[name="FilterOptions.Address.Id"]').val(ui.item.id);
            inputAddressText.val(ui.item.value);            
            $("form.filterForm").submit();
        },
        delay: 300,
        minLength: 3
    }).focus();
};

var addressClear = function () {
    $('input[name="FilterOptions.Address.AddressType"]').val("");
    $('input[name="FilterOptions.Address.Id"]').val("");
    $('input[name="FilterOptions.Address.Text"]').val("");
};

var focusOutFilterOptionsAddress = function () {
    var addressTypeElem = $('input[name="FilterOptions.Address.AddressType"]');
    if (addressTypeElem.val() === "" || addressTypeElem.val() === "None" ) {
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
    $('input[name="FilterOptions.Address.Text"]').focusout(focusOutFilterOptionsAddress);
    $('#addressFilterClearBtn').click(addressFilterClear);
});