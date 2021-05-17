var searchModal = function () {
    var isValid = $(this).closest(".filterForm").valid();
    if (!isValid) {
        fixBootstrapSelectHighlight($(this).closest(".filterForm"));
        return false;
    }

    addressClear();
    $("form.filterForm").submit();
};
var filterClearModal = function () {
    resetModalForm($("form.filterForm"));
    filterIdRegionChange();
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
});