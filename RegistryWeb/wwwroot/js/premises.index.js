﻿var autocompleteFilterOptionsAddress = function () {
    $('#FilterOptions_Address_Text').autocomplete({
        source: function (request, response) {
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/Address/AutocompleteFilterOptionsAddress',
                dataType: 'json',
                data: { text: request.term },
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
            //addressFilterClearBtnShow();
        },
        delay: 300,
        minLength: 3
    });
};

var addressClear = function () {
    $('input[name="FilterOptions.Address.AddressType"]').val("");
    $('input[name="FilterOptions.Address.Id"]').val("");
    $('#FilterOptions_Address_Text').val("");
};

var focusOutFilterOptionsAddress = function () {
    if ($('input[name="FilterOptions.Address.Id"]').val() == "") {
        addressClear();
    }
};

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
    autocompleteFilterOptionsAddress();
    addressFilterClearBtnVisibility();
    $('#FilterOptions_Address_Text').focusout(focusOutFilterOptionsAddress);
    $('#addressFilterClearBtn').click(addressFilterClear);
    $('#searchModalBtn').click(searchModal);
    $('#filterClearModalBtn').click(filterClearModal);
    $('#filterClearBtn').click(filterClear);

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