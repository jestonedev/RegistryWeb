
$(function () {
    $('.tenancy-process-toggler').each(function (idx, e) {
        $(e).on('click', $('#' + $(e).data("for")), elementToogle);
    });
});
