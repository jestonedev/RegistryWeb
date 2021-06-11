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
    $(".c-arithmetic-op").val(2);
    filterIdRegionChange();
    resetModalForm($("form.filterForm"));
    $("form.filterForm").valid();
};
var filterClear = function () {
    filterClearModal();
    $("form.filterForm").submit();
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
            select.selectpicker('destroy');
            select.find('option[value]').remove();
            $.each(data, function (i, d) {
                select.append('<option value="' + d.idStreet + '">' + d.streetName + '</option>');
            });
            select.selectpicker();
        }
    });
}


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