var restrictionsToggle = function (e) {
    arrowAnimation($(this));
    $('#restrictionsTable').toggle();
    e.preventDefault();
}
$(function () {
    var action = $('form').data('action');
    var addressType = $('form').data('addressType');
    //var tr = $("#restrictions tr").first();
    //trTemplate = tr[0].outerHTML;

    $('#restrictionAdd').click(function (e) {
        var id = $('.restrictionBlock').length;
        //$('#restrictions').append(trTemplate);
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Premises/RestrictionAdd',
            data: { id: id, addressType: addressType, action: action },
            success: function (data) {
                $('#restrictions').append(data);
                //refreshValidation();
            }
        });
        e.preventDefault();
    });

    $('#restrictionsTable').hide();
    $('#restrictionsToggle').click(restrictionsToggle);
});

/*var refreshValidation = function () {
    var form = $("#r-restriction-form")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
}*/


/*
var restrictionDelete = function (owner) {
    if ($('.restrictionBlock').length == 1)
        return;
    var iOwner = +owner.attr('data-i-restriction');//Преобразование iOwner к числу
    owner.remove();
    //recalculationIdOwner(iOwner);
}
*/
