$(function () {
    $(".rr-delete-correction").on("click", function (e) {
        var idCorrection = $(this).data("idCorrection");
        var modal = $("#ConfirmDeleteCorrectionModal");
        modal.find("#DeleteCorrection_IdCorrection").val(idCorrection);
        modal.modal('show');
        e.preventDefault();
    });

    $("#confirmDeleteCorrection").on("click", function (e) {
        var modal = $("#ConfirmDeleteCorrectionModal");
        $(this).attr("disabled", "disabled");
        var idCorrection = modal.find("#DeleteCorrection_IdCorrection").val();

        $.ajax({
            async: true,
            type: 'POST',
            url: window.location.origin + '/KumiAccounts/DeleteCorrection',
            data: { idCorrection },
            success: function (result) {
                if (result.state === "Success")
                    location.reload();
                else {
                    $("#ConfirmDeleteCorrectionModal .rr-delete-correction-error").text(result.error);
                    $(this).removeAttr("disabled");
                }
            }
        });
    });
});