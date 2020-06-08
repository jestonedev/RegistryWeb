
var fundbodyToggle = function (e) {
    arrowAnimation($(this));
    $('#fundbody').toggle();
    e.preventDefault();
}
$(function () {

    var action = $('#r-fundshistory-form').data("action");

    if (action == "Index")
    {
        $('select').prop('disabled', true);
        $('input').prop('disabled', true);
        $('textarea').prop('disabled', true);
    }    
    else if (action == "Delete")
    {
        $('select').prop('readonly', true);
        $('input').prop('readonly', true);
        $('textarea').prop('readonly', true);
        $('#del').prop('readonly', false);
        $('select').attr('readonly', 'readonly');
        /*$('select').prop('disabled', true);
        $('input').prop('disabled', true);
        $('textarea').prop('disabled', true);
        $('input[type="hidden"]').prop('disabled', false);
        $('input[type="submit"]').prop('disabled', false);*/
    }
    

    $('#fundbodyToggle').click(fundbodyToggle);

    $('#includecheck').on("click", function (e)
    {
        if ($('#includecheck').is(':checked'))
        {
            $('.include').removeAttr('disabled');
        } else {

                $('.include').attr('disabled', 'disabled');
               }
    });

    $('#excludecheck').on("click", function (e) {
        if ($('#excludecheck').is(':checked')) {
            $('.exclude').removeAttr('disabled');
        } else {

                $('.exclude').attr('disabled', 'disabled');
               }
    });

    $("#edit").on("click", function (e) {
        $('select').prop('disabled', false);
        $('input').prop('disabled', false);
        $('textarea').prop('disabled', false);
    });

    $("#delete").on("click", function (e) {
        $('select').prop('disabled', true);
        $('input').prop('disabled', true);
        $('textarea').prop('disabled', true);
    });



});