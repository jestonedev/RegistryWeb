var buildingToggle = function (e) {
    arrowAnimation($(this));
    $('#building').toggle();
    e.preventDefault();
}
$(function () {
    $('#buildingToggle').click(buildingToggle);

    var action = $('#buildingsForm').data("action");
    if (action == "Details" || action == "Delete") {
        $('select').prop('disabled', true);
        $('input').prop('disabled', true);
        $('textarea').prop('disabled', true);
        $('input[type="hidden"]').prop('disabled', false);
        $('input[type="submit"]').prop('disabled', false);
    }
});