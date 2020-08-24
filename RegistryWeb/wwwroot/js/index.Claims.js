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
        $(".c-arithmetic-op").next(".form-group").find("input[type='text']").each(function (idx, elem) {
            $(elem).val($(elem).val().replace(',', '.'));
        });
        $("form.filterForm").submit();
        $("#filterModal").modal("hide");
    }
};

var filterClearModal = function () {
    $("#filterModal input[type='text'], #filterModal input[type='date'], #filterModal input[type='hidden'], #filterModal select").val("");
    $('#FilterOptions_IdStreet, #FilterOptions_IdClaimState').selectpicker('render');
    $("form.filterForm").valid();
};
var filterClear = function () {
    filterClearModal();
    $("form.filterForm").submit();
};

var toggleDetails = function (self, selector) {
    var claimDetail = $(self).closest("td").find(selector);
    var icon = $(self).find(".oi");
    if (icon.hasClass("oi-chevron-bottom")) {
        icon.removeClass("oi-chevron-bottom");
        icon.addClass("oi-chevron-top");
        claimDetail.show();
    } else {
        icon.addClass("oi-chevron-bottom");
        icon.removeClass("oi-chevron-top");
        claimDetail.hide();
    }
};

var toggleClaimDetails = function (e) {
    toggleDetails(this, ".rr-claim-detail");
    e.preventDefault();
};

var toggleAmountDetails = function (e) {
    toggleDetails(this, ".rr-amount-detail");
    e.preventDefault();
};

$(function () {
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

    $(".rr-claim-details").on("click", toggleClaimDetails);
    $(".rr-amount-details").on("click", toggleAmountDetails);

    $(".c-arithmetic-op").each(function (idx, elem) {
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

    $("#filterModal .input-group .dropdown-menu .dropdown-item").on("click", function (e) {
        var op = $(this).text();
        var prepend = $(this).closest(".input-group-prepend").find("button");
        prepend.text(op);
        var formGroup = prepend.closest(".form-group");
        var input = formGroup.prev(".c-arithmetic-op");
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

    $(".c-arithmetic-op").next(".form-group").find("input[type='text']").each(function (idx, elem) {
        if ($(elem).val() !== "" && $(elem).val() !== null) {
            hasAccountSumFilter = true;
        }
        $(elem).val($(elem).val().replace('.', ','));
    });

});