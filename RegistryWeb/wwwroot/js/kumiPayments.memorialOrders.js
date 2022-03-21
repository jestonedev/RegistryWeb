$(function () {
    $("#clearMemorialOrderModalBtn").on('click', memorialOrderModalClear);

    function memorialOrderModalClear(e) {
        $('#resultMemorialOrderModal').html('');
        $('#errorMemorialOrderModal').html('');
        var idPayment = $("#MemorialOrder_IdPayment").val();
        var returnUrl = $("#MemorialOrder_ReturnUrl").val();
        resetModalForm($("#MemorialOrderModalForm"));
        $("#MemorialOrder_IdPayment").val(idPayment);
        $("#MemorialOrder_ReturnUrl").val(returnUrl);
        $('#setMemorialOrderModalBtn').attr('disabled', true);
    }

    $("#searchMemorialOrderModalBtn").on('click', memorialOrderModalSearch);

    function memorialOrderModalSearch(e) {
        $(".input-decimal").each(function (idx, elem) {
            $(elem).val($(elem).val().replace(".", ","));
        });
        var div = $('#resultMemorialOrderModal');
        div.html("");
        $('#errorMemorialOrderModal').html('');
        $('#setMemorialOrderModalBtn').attr('disabled', true);
        $('#searchMemorialOrderModalBtn').text('Ищем...').attr('disabled', true);
        $.ajax({
            async: true,
            type: 'POST',
            url: window.location.origin + '/KumiPayments/GetMemorialOrders',
            data: {
                "FilterOptions.NumDocument": $('#MemorialOrder_NumDocument').val(),
                "FilterOptions.DateDocument": $('#MemorialOrder_DateDocument').val(),
                "FilterOptions.Sum": $('#MemorialOrder_Sum').val(),
                "FilterOptions.Kbk": $('#MemorialOrder_Kbk').val(),
                "FilterOptions.Okato": $('#MemorialOrder_Okato').val()
            },
            success: function (result) {
                var table = "<table class='table table-bordered mb-0'>";
                table += "<thead><tr><th></th><th>№ / Дата</th><th>Сумма</th><th>КБК</th></tr></thead><tbody>";
                for (var i = 0; i < result.memorialOrders.length; i++) {
                    var memorialOrder = result.memorialOrders[i];
                    var idOrder = memorialOrder.idOrder;
                    var numDocument = memorialOrder.numDocument;
                    var dateDocumentStr = null;
                    if (memorialOrder.dateDocument !== null) {
                        var dateDocument = new Date(memorialOrder.dateDocument);
                        var year = dateDocument.getFullYear();
                        var month = dateDocument.getMonth() + 1;
                        var day = dateDocument.getDate();
                        dateDocumentStr = (day < 10 ? "0" + day : day) + "." + (month < 10 ? "0" + month : month) + "." + year;
                    }
                    var sum = memorialOrder.sumZach;
                    var kbk = memorialOrder.kbk;

                    var radioButton = "<div class='form-check'><input style='margin-top: -7px' name='MemorialOrder_IdOrder' value='" + idOrder + "' type='radio' class='form-check-input'></div>";

                    
                    table += "<tr>";

                    table += "<td style='vertical-align: middle'>" + radioButton + "</td>";
                    table += "<td>№ " + numDocument + " от " + dateDocumentStr + "</td><td>" + sum + "</td><td>" + kbk + "</td>";
                    table += "</tr>";

                }
                if (result.memorialOrders.length < result.count) {
                    table += "<tr><td colspan='4' class='text-center'><i class='text-danger'>Всего найдено " + result.count + " совпадений. Уточните запрос</i></td></tr>";
                }
                if (result.count === 0) {
                    table += "<tr><td colspan='4' class='text-center'><i class='text-danger'>Ордеры не найдены</i></td></tr>";
                }
                table += "</tbody></table>";
                div.html(table);
                $('#searchMemorialOrderModalBtn').text('Найти').attr('disabled', false);
            }
        });
        e.preventDefault();
    }

    $("#resultMemorialOrderModal").on('click', "[name='MemorialOrder_IdOrder'][type='radio']", function (e) {
        if ($(this).is(":checked")) {
            $("#setMemorialOrderModalBtn").attr('disabled', false);
        }
    });

    $("#setMemorialOrderModalBtn").on('click', memorialOrderModalSet);

    function memorialOrderModalSet(e) {
        var modal = $("#MemorialOrderModal");
        var createNewPaymentAction = $('#setMemorialOrderModalBtn').text() === "Создать";
        var action = $("#paymentsForm").attr('data-action');
        $('#setMemorialOrderModalBtn').text('Сохраняем...').attr('disabled', true);
        var data = {
            "IdPayment": modal.find("#MemorialOrder_IdPayment").val(),
            "IdOrder": $("[name='MemorialOrder_IdOrder']:checked").val(),
            "ReturnUrl": modal.find("#MemorialOrder_ReturnUrl").val()
        };
        var url = window.location.origin + '/KumiPayments/ApplyMemorialOrder';
        var saveBtnTitle = "Привязать";
        if (createNewPaymentAction) {
            url = window.location.origin + '/KumiPayments/CreateByMemorialOrder';
            saveBtnTitle = "Создать";
        }
        
        $.ajax({
            async: true,
            type: 'POST',
            url: url,
            data: data,
            success: function (result) {
                if (result.state === "Error") {
                    var errorElem = $("#errorMemorialOrderModal");
                    errorElem.html("<span class='text-danger'>" + result.error + "</span>");
                } else {
                    if (action !== undefined)
                        location.href = result.redirectUrl;
                    else 
                        window.open(result.redirectUrl, '_blank');
                }
                $('#setMemorialOrderModalBtn').text(saveBtnTitle).attr('disabled', false);
            }
        });

        e.preventDefault();
    }

    $(".rr-apply-memorial-order").on("click", function (e) {
        var idPayment = $(this).data("idPayment");
        $("#MemorialOrderModalForm").find("#MemorialOrder_IdPayment").val(idPayment);
        var modal = $("#MemorialOrderModal");
        modal.find("input[type='text'], input[type='date']").prop("disabled", false);
        modal.find("#setMemorialOrderModalBtn").text("Привязать");
        modal.modal('show');
        e.preventDefault();
    });

    $(".rr-add-payment-by-memorial-order").on("click", function (e) {
        var modal = $("#MemorialOrderModal");
        modal.find("input[type='text'], input[type='date']").prop("disabled", false);
        modal.find("#setMemorialOrderModalBtn").text("Создать");
        modal.modal('show');
        e.preventDefault();
    });
});