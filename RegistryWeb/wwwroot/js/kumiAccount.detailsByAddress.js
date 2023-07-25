$(function () {
    var descElem = $("textarea[name='Description']");

    $("#DetailsByAddressEditDescription").on("click", function () {
        var description = descElem.val();
        descElem.data("prevDescription", description);
        descElem.removeAttr("disabled");
        $(this).addClass("d-none");
        $("#DetailsByAddressSaveDescription, #DetailsByAddressCanceDescription").removeClass("d-none");
    });

    $("#DetailsByAddressCanceDescription").on("click", function () {
        var description = descElem.data("prevDescription");
        descElem.val(description);
        descElem.attr("disabled", "disabled");
        $("#DetailsByAddressEditDescription").removeClass("d-none");
        $("#DetailsByAddressSaveDescription, #DetailsByAddressCanceDescription").addClass("d-none");
    });

    $("#DetailsByAddressSaveDescription").on("click", function () {
        descElem.attr("disabled", "disabled");
        var description = descElem.val();
        var idAccount = $("input[name='IdAccount']").val();
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/KumiAccounts/SaveDescriptionForAddress',
            data: {
                idAccount, description
            },
            success: function () {
                $("#DetailsByAddressEditDescription").removeClass("d-none");
                $("#DetailsByAddressSaveDescription, #DetailsByAddressCanceDescription").addClass("d-none");          
            }
        });
    });
});