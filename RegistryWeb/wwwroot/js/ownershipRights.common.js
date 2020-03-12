var ownershipRightsToggle = function (e) {
    arrowAnimation($(this));
    $('#ownershipRightsTable').toggle();
    e.preventDefault();
}
$(function () {
    $('#ownershipRightsTable').hide();
    $('#ownershipRightsToggle').click(ownershipRightsToggle);
});