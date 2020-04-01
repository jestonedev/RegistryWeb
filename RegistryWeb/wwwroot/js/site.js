var arrowAnimation = function (arrow) {
    if (arrow.html() === '∧') {
        arrow.html('∨');
    }
    else {
        arrow.html('∧');
    }
}
$(function () {
    $("input[type='number']").inputSpinner();
});