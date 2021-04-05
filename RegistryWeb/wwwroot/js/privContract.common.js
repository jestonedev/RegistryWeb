$(function () {
    var action = $('#privatizationForm').attr('data-action');    
    if (action == 'Details' || action == 'Delete') {
        $('select option:not(:selected)').attr('disabled', true)
        $('input').attr('readonly', true);
        $('textarea').attr('readonly', true);
        $('input:checkbox').attr('disabled', true)
    }

    function privatizationToggle(e) {
        $(this).children('span').toggleClass('oi-chevron-top').toggleClass('oi-chevron-bottom');
        $('#privatization').toggle();
        e.preventDefault();
    }
    function privContractorsToggle(e) {
        $(this).children('span').toggleClass('oi-chevron-top').toggleClass('oi-chevron-bottom');
        $('#privContractors').toggle();
        e.preventDefault();
    }

    $('#privatizationToggle').on('click', privatizationToggle);
    $('#privContractorsToggle').on('click', privContractorsToggle);

    
    //$('#ownerProcessEdit, #ownerProcessCreate').click(function (e) {
    //    e.preventDefault();
    //    if (form.valid()) {
    //        form.submit();
    //    }
    //    $('input[data-val="true"]')
    //        .removeAttr('aria-describedby')
    //        .removeAttr('aria-invalid');
    //});
    //$('#ownerProcessDelete').click(function (e) {
    //    e.preventDefault();
    //    $('#ownerProcessForm').submit();
    //});    
});