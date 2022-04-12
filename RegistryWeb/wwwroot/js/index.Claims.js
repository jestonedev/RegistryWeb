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
    resetModalForm($("form.filterForm"));
    filterIdRegionChange();    
    $("#FilterOptions_IsCurrentState").prop("checked", false);
    $("#FilterOptions_IsCurrentState").prop("disabled", true);
    $(".selectpicker[name$=Op]").selectpicker("val", 0);
    $("input[name$=DateTo]").hide();
    $(".rr-id-claim-state-4").hide();
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
    $('#searchModalBtn').click(searchModal);
    $('#filterModal #FilterOptions_IdRegion').on('change', function (e) {
        filterIdRegionChange();
        e.preventDefault();
    });
    $('#filterModal #FilterOptions_IdClaimState').on('change', function (e) {
        if ($(this).val() == '') {
            $('#FilterOptions_IsCurrentState').prop('checked', false);
            $('#FilterOptions_IsCurrentState').prop('disabled',true);
        }
        else {
            $('#FilterOptions_IsCurrentState').prop('disabled', false);
        }
        e.preventDefault();
    });
    $('#filterClearModalBtn').click(filterClearModal);
    $('#filterClearBtn').click(filterClear);

    $("#filterModalShow").on("click", function (e) {
        e.preventDefault();
        var modal = $("#filterModal");
        if ($('#FilterOptions_IsCurrentState').is(':checked')) {
            $('#FilterOptions_IsCurrentState').prop('disabled', false);
        }
        modal.modal('show');
    });

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

    $(".selectpicker[name$=Op]").on("change", function (e) {
        var inputDateTo = $(this).closest(".input-group").find("input[name$=DateTo]");
        inputDateTo.hide();
        inputDateTo.val(null);
        if ($(this).val() == 3) {
            inputDateTo.show();
        }
        e.preventDefault();
    });

    $("#FilterOptions_IdClaimState").on("change", function (e) {
        $(".rr-id-claim-state-4").hide();
        $(".rr-id-claim-state-4 select").selectpicker("val", 0);
        $(".rr-id-claim-state-4 input").val(null);
        $(".rr-id-claim-state-4 input[name$=DateTo]").hide();
        if ($(this).val() == 4) {
            $(".rr-id-claim-state-4").show();
        }
        e.preventDefault();
    });

    if ($("#FilterOptions_IdClaimState").val() != 4) {
        $(".rr-id-claim-state-4").hide();
    }

    $("input[name$=DateTo]").each(function (idx, elem) {
        var selectOp = $(this).closest(".input-group").find("select[name$=Op]");
        if (selectOp.val() != 3) {
            $(this).hide();
        }
    });

    $(".c-arithmetic-op").next(".form-group").find("input[type='text']").each(function (idx, elem) {
        if ($(elem).val() !== "" && $(elem).val() !== null) {
            hasAccountSumFilter = true;
        }
        $(elem).val($(elem).val().replace('.', ','));
    });

    filterIdRegionChange();
});