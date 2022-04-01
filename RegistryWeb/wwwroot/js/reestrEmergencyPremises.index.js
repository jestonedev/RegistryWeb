var autocompleteFilterOptionsAddress = function () {
	$('#FilterOptions_Address_Text').autocomplete({
		source: function (request, response) {
			$.ajax({
				type: 'POST',
				url: window.location.origin + '/Address/AutocompleteFilterOptionsAddress',
				dataType: 'json',
				data: { text: request.term },
				success: function (data) {
					if (data != 0 && data != undefined) {
                        $('input[name="FilterOptions.Address.AddressType"]').val(data.addressType);
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
var reestrStatisticToggle = function (event) {
	var reestrStatistic = $('#reestrStatistic');
	var toggleReestrStatistic = $('#toggleReestrStatistic');
	if (reestrStatistic.is(":hidden")) {
		toggleReestrStatistic
			.prop('disabled', true)
			.html('<span class="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true"></span>Статистика');
		$.ajax({
			async: true,
			type: 'POST',
			url: window.location.origin + '/ReestrEmergencyPremises/ReestrStatistic',
			success: function (data) {
				reestrStatistic.find('.date_S').empty();
				reestrStatistic.find('.countMKD_S').empty();
				reestrStatistic.find('.countPremAndSubPrem_S').empty();
				reestrStatistic.find('.countTenancy_S').empty();
				reestrStatistic.find('.countOwner_S').empty();
				reestrStatistic.find('.countInhabitant_S').empty();
				reestrStatistic.find('.totalAreaSum_S').empty();
				reestrStatistic.find('.livingAreaSum_S').empty();
				reestrStatistic.find('.date_S').append(data.date);
				reestrStatistic.find('.countMKD_S').append(data.countMKD);
				reestrStatistic.find('.countPremAndSubPrem_S').append(data.countTenancy + data.countOwner);
				reestrStatistic.find('.countTenancy_S').append(data.countTenancy);
				reestrStatistic.find('.countOwner_S').append(data.countOwner);
				reestrStatistic.find('.countInhabitant_S').append(data.countInhabitant);
				reestrStatistic.find('.totalAreaSum_S').append(data.totalAreaSum);
				reestrStatistic.find('.livingAreaSum_S').append(data.livingAreaSum);
				toggleReestrStatistic
					.prop('disabled', false)
					.html('Статистика');
				reestrStatistic.show();
			}
		});
	}
	else {
		reestrStatistic.hide();
	}
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
	$('#reestrStatistic').hide();
	$('#toggleReestrStatistic').click(reestrStatisticToggle);

	autocompleteFilterOptionsAddress();
	$('#FilterOptions_Address_Text').focusout(focusOutFilterOptionsAddress);
	$('#addressFilterClearBtn').click(addressFilterClear);
	$('#r-search-btn').click(formSubmit);

	$("a.sort").click(function () {
		$('input[name="OrderOptions.OrderField"]').val($(this).data("order-field"));
		$('input[name="OrderOptions.OrderDirection"]').val($(this).data("order-direction"));
		formSubmit();
	});
	$('.page-link').click(function (e) {
		$('input[name="PageOptions.CurrentPage"]').val($(this).data("page"));
        formSubmit();
        e.preventDefault();
	});
	if ($('input[name="FilterOptions.Address.Id"]').val() != "") {
		addressFilterClearBtnShow();
	}
	else {
		$('#addressFilterClearBtn').hide();
	}
});