$(document).ready(function () {
    $('.owner-reason-type-toggler').each(function (idx, e) {
        $(e).on('click', $('#' + $(e).data("for")), elementToogleHide);
    });
});