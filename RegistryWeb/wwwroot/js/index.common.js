$(function () {
    $('.page-link').click(function () {
        $('input[name="PageOptions.CurrentPage"]').val($(this).data("page"));
        $("form.r-filter-form").submit();
    });    
});