
var fundbodyToggle = function (e) {
    arrowAnimation($(this));
    $('#fundbody').toggle();
    e.preventDefault();
};

$(function ()
{
    var action = $('#r-fundshistory-form').data("action");
    var now = new Date();
    var day = ("0" + now.getDate()).slice(-2);
    var month = ("0" + (now.getMonth() + 1)).slice(-2);
    var today = now.getFullYear() + "-" + (month) + "-" + (day);

    if (action === "Index" || action === "Delete")
    {
        $('#fundbody select').prop('disabled', true);
        $('#fundbody input').prop('disabled', true);
        $('#fundbody textarea').prop('disabled', true);
    }

    $('#fundbodyToggle').click(fundbodyToggle);


    $('#includecheck').on("change", function (e) {
        if ($('#includecheck').is(':checked') && (action === "Create" || action === "Edit")) {
            $('.include').removeAttr('disabled');
            var includeDate = $("#FundHistory_IncludeRestrictionDate");
            if (includeDate.val() === "") {
                includeDate.val(today);
            }
        } else {
            $('.include').attr('disabled', 'disabled');
        }
    });

    $('#excludecheck').on("change", function (e) {
        if ($('#excludecheck').is(':checked') && (action === "Create" || action === "Edit")) {
            $('.exclude').removeAttr('disabled');
            var excludeDate = $("#FundHistory_ExcludeRestrictionDate");
            if (excludeDate.val() === "") {
                excludeDate.val(today);
            }
        } else {
            $('.exclude').attr('disabled', 'disabled');
        }
    });

    $('#includecheck, #excludecheck').change();

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

    $("form#r-fundshistory-form").on("submit", function (e) {
        $("button[data-id]").removeClass("input-validation-error");
        var isFormValid = $(this).valid();
        if (!isFormValid) {
            e.preventDefault();
        }
    });

    $("form").on("change", "select", function () {
        $(this).valid();
    });

    $("#fundshistory tbody tr").on('click', function () {
        var tr = $(this);

        $('.table tr').removeClass('active1');
        tr.addClass('active1');
        var idFund = tr.find('input[name$="IdFund"]').val();

        $.getJSON('/FundsHistory/Details?' + "idFund=" + idFund, function (data) {
            ostatockArray = data;
            $(ostatockArray).each(function (idx, elem) {
                $("select[name$='IdFundType']").val(elem.idFundType);
                $("input[name$='ProtocolDate']").val(elem.protocolDate === null ? "" : elem.protocolDate.substr(0, 10));
                $("input[name$='ProtocolNumber']").val(elem.protocolNumber);
                $("textarea").val(elem.description);

                $("input[name$='IncludeRestrictionNumber']").val(elem.includeRestrictionNumber);
                $("input[name$='IncludeRestrictionDate']").val(elem.includeRestrictionDate === null ? "" : elem.includeRestrictionDate.substr(0, 10));
                $("input[name$='IncludeRestrictionDescription']").val(elem.includeRestrictionDescription);
                if (elem.includeRestrictionNumber !== null || elem.includeRestrictionDate !== null || elem.includeRestrictionDescription !== null) {
                    $("#includecheck").prop("checked", "checked");
                } else {
                    $("#includecheck").prop("checked", "");
                }

                $("input[name$='ExcludeRestrictionNumber']").val(elem.excludeRestrictionNumber);
                $("input[name$='ExcludeRestrictionDate']").val(elem.excludeRestrictionDate === null ? "" : elem.excludeRestrictionDate.substr(0, 10));
                $("input[name$='ExcludeRestrictionDescription']").val(elem.excludeRestrictionDescription);

                if (elem.excludeRestrictionNumber !== null || elem.excludeRestrictionDate !== null || elem.excludeRestrictionDescription !== null) {
                    $("#excludecheck").prop("checked", "checked");
                } else {
                    $("#excludecheck").prop("checked", "");
                }
            });
        });
    });
});