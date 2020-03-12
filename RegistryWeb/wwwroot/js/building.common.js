var buildingToggle = function (e) {
    arrowAnimation($(this));
    $('#building').toggle();
    e.preventDefault();
}
$(function () {
    $('#buildingToggle').click(buildingToggle);
});