var restrictionsToggle = function (e) {
    arrowAnimation($(this));
    $('#restrictionsTable').toggle();
    e.preventDefault();
}
$(function () {
    $('#restrictionsTable').hide();
    $('#restrictionsToggle').click(restrictionsToggle);

    var action = $('form').data('action');
    var addressType = $('form').data('addresstype');
    var tr = $("#restrictions tr").first();
    //trTemplate = tr[0].outerHTML;

    $('#restrictionAdd').click(function (e) {
        var id = $('.restrictionBlock').length;
        //$('#restrictions').append(trTemplate);
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Premises/RestrictionAdd',
            data: { id: id, addressType: "Premise", action: action },
            success: function (data) {
                $('#restrictions').append(data);
                //refreshValidation();
            }
        });
        e.preventDefault();
    });


    /* var action = $('form').data('action');
     $('#restrictionAdd').click(function (e) {
         var id = $('.restrictionBlock').length;
         var addressType = $(this).data('addressType');
         $.ajax({
             type: 'POST',
             url: window.location.origin + '/Premises/RestrictionAdd',
             data: { type: addressType, id: id, action: action },
             success: function (data) {
                 $('#restrictions').append(data);
                 refreshValidation();
             }
         });
         e.preventDefault();
     });*/
});