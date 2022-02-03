﻿var searchModal = function () {
    addressClear();
    if ($("form.filterForm").valid()) {
        $("#AccountSumFilters").find("input[type='text']").each(function (idx, elem) {
            $(elem).val($(elem).val().replace(',', '.'));
        });
        $("form.filterForm").submit();
        $("#filterModal").modal("hide");
    }
};

var filterClearModal = function () {
    $("#filterModal input[type='text'], #filterModal input[type='date'], #filterModal input[type='hidden'], #filterModal select").val("");
    $('#FilterOptions_IdStreet, #FilterOptions_IdPreset').selectpicker('render');
    $("#filterModal input[type='checkbox']").prop("checked", false);
    filterIdRegionChange();
    $("form.filterForm").valid();
};
var filterClear = function () {
    filterClearModal();
    $("form.filterForm").submit();
};

var toggleAdditionInfo = function (e) {
    var paymentsDetail = $(this).closest("td").find("div.rr-account-additional-info");
    var icon = $(this).find(".oi");
    if (icon.hasClass("oi-chevron-bottom")) {
        icon.removeClass("oi-chevron-bottom");
        icon.addClass("oi-chevron-top");
        paymentsDetail.show();
    } else {
        icon.addClass("oi-chevron-bottom");
        icon.removeClass("oi-chevron-top");
        paymentsDetail.hide();
    }
    e.preventDefault();
};

var filterIdRegionChange = function (e) {
    var idRegion = $('#FilterOptions_IdRegion').selectpicker('val');
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/Address/GetKladrStreets',
        dataType: 'json',
        data: { idRegion },
        success: function (data) {
            var select = $('#filterModal #FilterOptions_IdStreet');
            var value = select.val();
            select.selectpicker('destroy');
            select.find('option[value]').remove();
            $.each(data, function (i, d) {
                select.append('<option value="' + d.idStreet + '">' + d.streetName + '</option>');
            });
            select.selectpicker();
            select.val(value);
            select.selectpicker('refresh');
        }
    });
};

$(function () {
    $('#AccountSumFiltersToggler').on("click", $("#AccountSumFilters"), elementToogleHide);
    $('#searchModalBtn').click(searchModal);
    $('#filterModal #FilterOptions_IdRegion').on('change', function (e) {
        filterIdRegionChange();
        e.preventDefault();
    });
    $('#filterClearModalBtn').click(filterClearModal);
    $('#filterClearBtn').click(filterClear);

    $("#filterModalShow").on("click", function (e) {
        e.preventDefault();
        var modal = $("#filterModal");
        modal.modal('show');
    });

    $(".rr-additional-info").on("click", toggleAdditionInfo);

    $("#AccountSumFilterCriteria").on("change", function () {
        var val = $(this).val();
        if (val === "1")
            $("#FilterOptions_AtDate").val("");
        $("#FilterOptions_AtDate").prop("disabled", val === "2" ? "" : "disabled");
    });

    $("#AccountSumFilters input[type='hidden']").each(function (idx, elem) {
        var op = $(elem).val();
        var formGroup = $(elem).next(".form-group");
        var prepend = formGroup.find(".input-group .input-group-prepend button");
        switch (op) {
            case "1":
                prepend.text("≥");
                break;
            case "2":
                prepend.text("≤");
                break;
            default:
                prepend.text("≤");
                $(elem).val("2");
        }
    });

    $("#AccountSumFilters .input-group .dropdown-menu .dropdown-item").on("click", function (e) {
        var op = $(this).text();
        var prepend = $(this).closest(".input-group-prepend").find("button");
        prepend.text(op);
        var formGroup = prepend.closest(".form-group");
        var input = formGroup.prev("input[type='hidden']");
        switch (op) {
            case "≥":
                input.val("1");
                break;
            case "≤":
                input.val("2");
                break;
            default:
                input.val("");
        }
    });

    var hasAccountSumFilter = false;
    $("#AccountSumFilters").find("input[type='text']").each(function (idx, elem) {
        if ($(elem).val() !== "" && $(elem).val() !== null) {
            hasAccountSumFilter = true;
        }
        $(elem).val($(elem).val().replace('.', ','));
    });
    if (hasAccountSumFilter) {
        var toggler = $("#AccountSumFiltersToggler").first();
        if (!isExpandElemntArrow(toggler)) {
            toggler.click();
        }
    }

    $("#FilterOptions_FrontSideAccount").keyup(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            filterClear();
        }
    });
    $("#accountFilterClearBtn").click(function (event) {
        event.preventDefault();
        $("#FilterOptions_FrontSideAccount").val("");
        filterIdRegionChange();
        $("form.filterForm").submit();
    });

    filterIdRegionChange();
});