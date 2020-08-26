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
            filterClearModal();
            $("form.filterForm").submit();
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
var addressFilterClearBtnVisibility = function () {
    if ($('input[name="FilterOptions.Address.Id"]').val() != "") {
        $('#FilterOptions_Address_Text').prop("disabled", true);
        $('#addressFilterClearBtn').show();
    }
    else {
        $('#addressFilterClearBtn').hide();
    }
};
var addressFilterClear = function () {
	addressClear();
	$('#FilterOptions_Address_Text').prop("disabled", false);
    $('#addressFilterClearBtn').hide();
    $("form.filterForm").submit();
};
var searchModal = function () {
    addressClear();

    var isValid = $(this).closest(".filterForm").valid();
    if (!isValid) {
        fixBootstrapSelectHighlight($(this).closest(".filterForm"));
        return false;
    }

    $("form.filterForm").submit();
};
var filterClearModal = function () {
    $('input[name="FilterOptions.IdBuilding"]').val("");
    $('#FilterOptions_IdStreet').val("");
    $('#FilterOptions_IdStreet').selectpicker('render');
    $('input[name="FilterOptions.House"]').val("");
    $('input[name="FilterOptions.Floors"]').val("");
    $('input[name="FilterOptions.Entrances"]').val("");
    $('#FilterOptions_IdsObjectState').selectpicker("deselectAll");
    $('#FilterOptions_IdDecree').val("");
    $('#FilterOptions_IdDecree').selectpicker('render');
    $('input[name="FilterOptions.DateOwnershipRight"]').val("");
    $('input[name="FilterOptions.NumberOwnershipRight"]').val("");
    $('#FilterOptions_IdsOwnershipRightType').selectpicker("deselectAll");
};
var filterClear = function () {
    filterClearModal();
    $("form.filterForm").submit();
}
$(function () {
    autocompleteFilterOptionsAddress();
    addressFilterClearBtnVisibility();
	$('#FilterOptions_Address_Text').focusout(focusOutFilterOptionsAddress);
	$('#addressFilterClearBtn').click(addressFilterClear);
    $('#searchModalBtn').click(searchModal);
    $('#filterClearModalBtn').click(filterClearModal);
    $('#filterClearBtn').click(filterClear);

    //$('.idBuildingCheckbox').click(function (e) {
    //    var id = +$(this).data('id');
    //    $.ajax({
    //        type: 'POST',
    //        url: window.location.origin + '/Buildings/SessionIdBuildings',
    //        data: { idBuilding: id, isCheck: $(this).prop('checked') }
    //    });
    //});

    $("#filterModalShow").on("click", function (e) {
        e.preventDefault();
        var modal = $("#filterModal");
        modal.modal('show');
    });

    $("a.sort").click(function () {
        $('input[name="OrderOptions.OrderField"]').val($(this).data("order-field"));
        $('input[name="OrderOptions.OrderDirection"]').val($(this).data("order-direction"));
        $("form.filterForm").submit();
    });
	$('.page-link').click(function () {
		$('input[name="PageOptions.CurrentPage"]').val($(this).data("page"));
        $('#FilterOptions_Address_Text').prop("disabled", false);
        $("form.filterForm").submit();
    });

    $('body').on('keydown', '.only_number', function (event) // Запрещаем все, кроме цифр на основной клавиатуре, а так же Num-клавиатуре
    {
        //console.log(event.keyCode + "   " + event.key);

        if (event.keyCode == 46 || event.keyCode == 8)
            return;
        else if (event.key.match(/([а-яА-Я]+)/) || event.key.match(/([a-zA-Z]+)/) || event.key != "Backspace" || event.key != "Delete")
            event.preventDefault();
        banLetters();
    });

    $('body').on('keydown', '.homecadastral', function (event) // Запрещаем все, кроме цифр на основной клавиатуре, а так же Num-клавиатуре
    {
        //console.log(event.keyCode + "   " + event.key);

        if (event.keyCode == 46 || event.keyCode == 8 || (event.shiftKey && event.keyCode == 54) || event.keyCode == 191 || (event.shiftKey && event.keyCode == 220) || event.keyCode == 220 || (event.shiftKey && (event.keyCode == 54 || event.keyCode == 186)))
            return;
        else if (event.key.match(/([а-яА-Я]+)/) || event.key.match(/([a-zA-Z]+)/) || event.key != "Backspace" || event.key != "Delete")
            event.preventDefault();
        banLetters();
    });
});