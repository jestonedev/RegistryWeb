
var fundbodyToggle = function (e) {
    arrowAnimation($(this));
    $('#fundbody').toggle();
    e.preventDefault();
}

function refreshFormValidation(form) {
    var form = form.removeData("validator").removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
}


$(function ()
{
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
        $(this).closest("tr").css("color", "black");
    });

    $("#delete").on("click", function (e) {
        $('select').prop('disabled', true);
        $('input').prop('disabled', true);
        $('textarea').prop('disabled', true);

    });

    $("select#IdFundType").on("change", function (e) {
        $(this).css("border-color", "#CED4DA");

    });

    $("form#r-fundshistory-form").on("submit", function (e) {
        $("button[data-id]").removeClass("input-validation-error");
        var isFormValid = $(this).valid();
        if (!isFormValid) {
            $("select").each(function (idx, elem) {

                var id = $(elem).prop("id");
                $("#" + id).css("border-color", "#dc3545");
                var name = $(elem).prop("name");
                var errorSpan = $("span[data-valmsg-for='" + name + "']");
                if (errorSpan.hasClass("field-validation-error")) {
                    $("button[data-id='" + id + "']").addClass("input-validation-error");
                }
            });
            e.preventDefault();
        }
    });

    $("form").on("change", "select", function () {
        var isValid = $(this).valid();
        var id = $(this).prop("id");
        if (!isValid) {
            $("button[data-id='" + id + "']").addClass("input-validation-error");
        } else {

            $("button[data-id='" + id + "']").removeClass("input-validation-error");
        }
    });

    

    $('.table tr').click(function ()
    {        
        $('.table tr').removeClass('active1');            
        $(this).addClass('active1');
    });




   


});