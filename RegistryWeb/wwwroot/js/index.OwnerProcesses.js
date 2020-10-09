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
var createOwnerProcess = function () {
    var form = $("form.filterForm")[0];
    form.action = '/OwnerProcesses/Create';
    form.submit();
};
$(function () {
    $('#searchModalBtn').click(searchModal);
    $('#filterClearModalBtn').click(filterClearModal);
    $('#filterClearBtn').click(filterClear);
    $('#createOwnerProcessBtn').click(createOwnerProcess);
    $("#filterModalShow").on("click", function (e) {
        e.preventDefault();
        var modal = $("#filterModal");
        modal.modal('show');
    });
});