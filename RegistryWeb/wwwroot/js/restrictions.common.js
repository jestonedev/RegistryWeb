var restrictionsToggle = function (e) {
    arrowAnimation($(this));
    $('#restrictionsTable').toggle();
    e.preventDefault();
}
$(function () {
    $('#restrictionsTable').hide();
    $('#restrictionsToggle').click(restrictionsToggle);
});