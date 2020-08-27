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
    $('#filterClearModalBtn').click(filterClearModal);
    $('#filterClearBtn').click(filterClear);

    $("#filterModalShow").on("click", function (e) {
        e.preventDefault();
        var modal = $("#filterModal");
        modal.modal('show');
    });

    $('.idPremiseCheckbox').click(function (e)
    {
        var id = +$(this).data('id');
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Premises/SessionIdPremises',
            data: { idPremise: id, isCheck: $(this).prop('checked') }
        });
    });

    $(".AllPremiseCheckbox").click(function ()
    {
        if ($(this).is(':checked'))
            $('td input:checkbox').prop('checked', true);
        else $('td input:checkbox').prop('checked', false);
    });

    $('.addselect').click(function (e)
    {
        var filterOptions = $(".filterForm").find("input, select, textarea").filter(function (idx, elem) {
            return /^FilterOptions\./.test($(elem).attr("name"));
        });
        var data = {};
        filterOptions.each(function (idx, elem) {
            data[$(elem).attr("name")] = $(elem).val();
        });

        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Premises/AddSessionSelectedAndFilteredPremises',
            data: data,
            success: function (status) {
                if (status == 0)
                {
                    $(".info").html("Помещения успешно добавлены в мастер массовых операций");//alert("Помещения успешно добавлены в мастер массовых операций");
                    $(".info").addClass("alert alert-success");
                }
                if (status == -1)
                {
                    $(".info").html("Отсутствуют помещения для добавления в мастер массовых операций");//alert("Отсутствуют помещения для добавления в мастер массовых операций");
                    $(".info").addClass("alert alert-danger");
                }
            }
        });
    });
});