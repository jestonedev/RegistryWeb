var form = $("form.r-filter-form");
$(".page-link").on("click", function () {
	form.find("input[name='PageOptions.CurrentPage']").val($(this).data("page"));
	form.submit();
});
$(".r-table-header-cell a").on("click", function () {
    form.find("input[name='OrderOptions.OrderField']").val($(this).data("order-field"));
    form.find("input[name='OrderOptions.OrderDirection']").val($(this).data("order-direction"));
    form.submit();
});