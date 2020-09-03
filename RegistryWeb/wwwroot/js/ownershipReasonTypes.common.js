$(document).ready(function ()
{
    var action = $('#deleteform').data("action");
    if (action === "Delete" || $("form#deleteform input[type='submit']").hasClass("disabled")) {
        $('select').prop('disabled', true);
        $('input').prop('disabled', true);
        $('textarea').prop('disabled', true);
        $('input[type="hidden"]').prop('disabled', false);
        $('input[type="submit"]').prop('disabled', false);
    }

    $('.owner-reason-type-toggler').each(function (idx, e) {
        $(e).on('click', $('#' + $(e).data("for")), elementToogleHide);
    });

    $('.page-link').off("click");
    $('.page-link').click(function (e) {
        $('input[name="PageOptions.CurrentPage"]').val($(this).data("page"));
        $("form.formsub").submit();
        e.preventDefault();
    }); 
});