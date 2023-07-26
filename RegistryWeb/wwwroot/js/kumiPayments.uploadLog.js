$(function () {
    var modal = $("#filterModal");

    $(".rr-ul-bind-mo-with-payment").on("click", function (e) {
        e.preventDefault();
        var idOrder = $(this).data("idOrder");
        var requisits = $(this).data("requisits");
        var sum = $(this).data("sum");
        modal.find("#FilterOptions_Sum").val(sum);
        modal.find(".modal-title").text("Поиск платежа для связки с ордером " + requisits);
        modal.find("#bindModalBtn").addClass("d-none");
        $("#filterModal .rr-search-mo-payment-result, #filterModal .rr-search-mo-payment-error").html("").addClass("d-none");
        modal.data("idOrder", idOrder);
        modal.modal('show');
        modal.find("#searchModalBtn").click();
    });

    $("#filterModal").on('change', ".rr-search-mo-payment-result input[type='radio']", function () {
        $("#filterModal #bindModalBtn").removeClass("d-none");
    });

    $("#filterModal #bindModalBtn").on("click", function (e) {
        e.preventDefault();
        $("#filterModal #bindModalBtn").prop("disabled", "disabled");
        var data = {
            "IdPayment": $("#filterModal .rr-search-mo-payment-result input[type='radio']:checked").val(),
            "IdOrder": $("#filterModal").data("idOrder")
        };
        var url = window.location.origin + '/KumiPayments/ApplyMemorialOrderWithPair';

        $.ajax({
            async: true,
            type: 'POST',
            url: url,
            data: data,
            success: function (result) {
                if (result.state === "Error") {
                    $("#filterModal .rr-search-mo-payment-error").removeClass("d-none").text(result.error);
                    $("#filterModal #bindModalBtn").prop("disabled", "");
                } else {
                    location.reload();
                }
            }
        });
    });
});