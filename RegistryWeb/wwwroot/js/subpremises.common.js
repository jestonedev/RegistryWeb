var subpremisesToggle = function (e) {
    arrowAnimation($(this));
    $('#subpremisesTable').toggle();
    e.preventDefault();
}
$(function () {
    $('#subpremisesTable').hide();
    $('#subpremisesToggle').click(subpremisesToggle);
});