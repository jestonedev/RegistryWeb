var autocompleteFilterOptionsAddress = function () {
	$('#FilterOptions_Address_Text').autocomplete({
		source: function (request, response) {
			$.ajax({
				type: 'POST',
				url: window.location.origin + '/Address/AutocompleteFilterOptionsAddress',
				dataType: 'json',
                data: { text: request.term, isBuldings: true },
				success: function (data) {
					if (data != 0 && data != undefined) {
						$('input[name="FilterOptions.Address.AddressType"]').val(data.idAddressType);
						response($.map(data.autocompletePairs, function (pair) {
							return { label: pair.item2, value: pair.item2, id: pair.item1 };
						}))
					}
				}
			});
		},
		select: function (event, ui) {
			$('input[name="FilterOptions.Address.Id"]').val(ui.item.id);
			addressFilterClearBtnShow();
		},
		delay: 300,
		minLength: 3
	});
}
var addressClear = function () {
	$('input[name="FilterOptions.Address.AddressType"]').val("");
	$('input[name="FilterOptions.Address.Id"]').val("");
	$('#FilterOptions_Address_Text').val("");
}
var focusOutFilterOptionsAddress = function () {
	if ($('input[name="FilterOptions.Address.Id"]').val() == "") {
		addressClear();
	}
}
var addressFilterClearBtnShow = function () {
	$('#FilterOptions_Address_Text').prop("disabled", true);
	$('#addressFilterClearBtn').show();
};
var addressFilterClear = function () {
	addressClear();
	$('#FilterOptions_Address_Text').prop("disabled", false);
	$('#addressFilterClearBtn').hide();
};
var formSubmit = function () {
	$('#FilterOptions_Address_Text').prop("disabled", false);
	$("form.r-filter-form").submit();
};

$(function () {
	autocompleteFilterOptionsAddress();
	$('#FilterOptions_Address_Text').focusout(focusOutFilterOptionsAddress);
	$('#addressFilterClearBtn').click(addressFilterClear);
    $('#r-search-btn').click(formSubmit);

    $('.idBuildingCheckbox').click(function (e) {
        var id = +$(this).data('id');
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Buildings/SessionIdBuildings',
            data: { idBuilding: id, isCheck: $(this).prop('checked') }
        });
    });
	//$("a.sort").click(function () {
	//	$('input[name="OrderOptions.OrderField"]').val($(this).data("order-field"));
	//	$('input[name="OrderOptions.OrderDirection"]').val($(this).data("order-direction"));
	//	formSubmit();
	//});
	$('.page-link').click(function () {
		$('input[name="PageOptions.CurrentPage"]').val($(this).data("page"));
		formSubmit();
	});
	if ($('input[name="FilterOptions.Address.Id"]').val() != "") {
		addressFilterClearBtnShow();
	}
	else {
		$('#addressFilterClearBtn').hide();
	}
});