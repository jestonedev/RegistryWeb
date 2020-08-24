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
            $('input[name="FilterOptions.Address.Id"]').val(ui.item.id);
            filterClearModal();
            $("form.filterForm").submit();
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
    if ($('input[name="FilterOptions.Address.Id"]').val() === "") {
        addressClear();
    }
};

var addressFilterClearBtnVisibility = function () {
    if ($('input[name="FilterOptions.Address.Id"]').val() !== "") {
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
    $("form.filterForm").valid();
};
var filterClear = function () {
    filterClearModal();
    $("form.filterForm").submit();
};

var togglePaymentDetails = function (e) {
    var paymentsDetail = $(this).closest("td").find(".rr-payments-detail");
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

$(function () {
    $('#AccountSumFiltersToggler').on("click", $("#AccountSumFilters"), elementToogleHide);
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

    $("a.sort").click(function (e) {
        $('input[name="OrderOptions.OrderField"]').val($(this).data("order-field"));
        $('input[name="OrderOptions.OrderDirection"]').val($(this).data("order-direction"));
        $('#FilterOptions_Address_Text').prop("disabled", false);
        $("form.filterForm").submit();
        e.preventDefault();
    });

    $('.page-link').click(function (e) {
        $('input[name="PageOptions.CurrentPage"]').val($(this).data("page"));
        $('#FilterOptions_Address_Text').prop("disabled", false);
        $("form.filterForm").submit();
        e.preventDefault();
    });

    $(".rr-payments-details").on("click", togglePaymentDetails);

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
});