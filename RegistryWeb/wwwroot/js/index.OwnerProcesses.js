var searchModal = function () {
    addressClear();
    $("form.filterForm").submit();
};
var filterClearModal = function () {
    resetModalForm($(this));
};
var filterClear = function () {
    resetModalForm($(this));
    $("form.filterForm").submit();
};
$(function () {
    $('#searchModalBtn').click(searchModal);
    $('#filterClearModalBtn').click(filterClearModal);
    $('#filterClearBtn').click(filterClear);
    $("#filterModalShow").on("click", function (e) {
        e.preventDefault();
        var modal = $("#filterModal");
        modal.modal('show');
    });
});