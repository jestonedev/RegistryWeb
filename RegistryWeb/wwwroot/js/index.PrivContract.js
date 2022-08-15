var searchModal = function () {
    addressClear();
    $("#FilterOptions_FrontSideRegNumber").val("");
    $("form.filterForm").submit();
};

var filterClearModal = function () {
    $("#filterModal input[type='text'], #filterModal input[type='date'], #filterModal input[type='hidden'], #filterModal select").val("");
    $('#FilterOptions_IdStreet, #FilterOptions_IdRegion').selectpicker('render');
    $("#filterModal input[type='checkbox']").prop("checked", false);
    filterIdRegionChange();
    $("form.filterForm").valid();
};
var filterClear = function () {
    filterClearModal();
    $("form.filterForm").submit();
};

var filterIdRegionChange = function () {
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
$(function () {
    $('#searchModalBtn').click(searchModal);
    $('#filterClearModalBtn').click(filterClearModal);
    $('#filterClearBtn').click(filterClear);
    $('#filterModal #FilterOptions_IdRegion').on('change', function (e) {
        filterIdRegionChange();
        e.preventDefault();
    });

    $("#filterModalShow").on("click", function (e) {
        e.preventDefault();
        var modal = $("#filterModal");
        modal.modal('show');
    });

    $("#FilterOptions_FrontSideRegNumber").keyup(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            filterClear();
        }
    });

    $("#regNumberFilterClearBtn").click(function (event) {
        event.preventDefault();
        $("#FilterOptions_FrontSideRegNumber").val("");
        filterIdRegionChange();
        $("form.filterForm").submit();
    });

});