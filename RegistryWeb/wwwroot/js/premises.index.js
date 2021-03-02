﻿var searchModal = function () {
	var isValid = $(this).closest(".filterForm").valid();
    if (!isValid) {
        fixBootstrapSelectHighlight($(this).closest(".filterForm"));
        return false;
    }

    addressClear();
    $("form.filterForm").submit();
};

var filterClearModal = function () {
    $('input[name="FilterOptions.IdPremise"]').val("");
    $('#FilterOptions_IdStreet').val("");
    $('#FilterOptions_IdStreet').selectpicker('render');
    $('input[name="FilterOptions.House"]').val("");
    $('input[name="FilterOptions.PremisesNum"]').val("");
    $('input[name="FilterOptions.Floors"]').val("");
    $('input[name="FilterOptions.CadastralNum"]').val("");

    $('#FilterOptions_IdsObjectState').selectpicker("deselectAll");
    $('#FilterOptions_IdFundType').selectpicker("deselectAll");
    $('#FilterOptions_IdsComment').selectpicker("deselectAll");
    $('#FilterOptions_IdsDoorKeys').selectpicker("deselectAll");

    $('#FilterOptions_IdsOwnershipRightType').selectpicker("deselectAll");
    $('input[name="FilterOptions.NumberOwnershipRight"]').val("");
    $('input[name="FilterOptions.DateOwnershipRight"]').val("");

    $('#FilterOptions_IdsRestrictionType').selectpicker("deselectAll");
    $('input[name="FilterOptions.RestrictionNum"]').val("");
    $('input[name="FilterOptions.RestrictionDate"]').val("");

    $('input[name="FilterOptions.StDateOwnershipRight"]').val("");
    $('input[name="FilterOptions.EndDateOwnershipRight"]').val("");
};

var filterClear = function () {
    filterClearModal();
    $("form.filterForm").submit();
};


$(document).ready(function () {
    $('#searchModalBtn').click(searchModal);
    $('#filterModal #FilterOptions_IdRegion').on('change', function (e) {
        var idRegion = $(this).selectpicker('val');
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Premises/GetKladrStreets',
            dataType: 'json',
            data: { idRegion },
            success: function (data) {
                var select = $('#filterModal #FilterOptions_IdStreet');
                select.find('option[value]').remove();
                $.each(data, function (i, d) {
                    select.append('<option value="' + d.idStreet + '">' + d.streetName + '</option>');
                });
                select.selectpicker('refresh');
            }
        });
        e.preventDefault();
    });
    $('#filterClearModalBtn').click(filterClearModal);
    $('#filterClearBtn').click(filterClear);
    $("#filterModalShow").on("click", function (e) {
        e.preventDefault();
        var modal = $("#filterModal");
        modal.modal('show');
    });
});