var searchModal = function () {
    addressClear();
    $("form.filterForm").submit();
};
var filterClearModal = function () {
    resetModalForm($("form.filterForm"));
};
var filterClear = function () {
    var form = $("form.filterForm");
    resetModalForm(form);
    form.submit();
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