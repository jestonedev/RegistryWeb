$(document).ready(function () {
    let nameController = $('.info').data('controller');

    $('.idCheckbox').click(function (e) {
        var id = +$(this).data('id');
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/' + nameController + '/CheckIdToSession',
            data: { id, isCheck: $(this).prop('checked') }
        });
    });

    $('.addselect').click(function (e) {
        var filterOptions = $(".filterForm").find("input, select, textarea").filter(function (idx, elem) {
            return /^FilterOptions\./.test($(elem).attr("name"));
        });
        var data = {};
        filterOptions.each(function (idx, elem) {
            if (data[$(elem).attr("name")] === undefined) {
                if ($(elem).attr("type") === "checkbox")
                    data[$(elem).attr("name")] = $(elem).prop("checked");
                else
                    data[$(elem).attr("name")] = $(elem).val();
            }
        });

        var nameObject = null;
        switch (nameController) {
            case "Premises": nameObject = "Помещения"; break;
            case "Buildings": nameObject = "Здания"; break;
            case "OwnerProcesses": nameObject = "Процессы собственности"; break;
            case "TenancyProcesses": nameObject = "Процессы найма"; break;
            case "Claims": nameObject = "Претензионно-исковые работы"; break;
            case "PaymentAccounts": nameObject = "Лицевые счета"; break;
            case "KumiAccounts": nameObject = "Лицевые счета"; break;
        }
        if (nameObject === null) {
            $(".info").html("Мультимастер для данного реестра отсутствует");
            $(".info").addClass("alert alert-danger");
        }
        else {
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/' + nameController + '/AddSelectedAndFilteredIdsToSession',
                data: data,
                success: function (status) {
                    if (status == 0) {
                        $(".info").html(nameObject + " успешно добавлены в мастер массовых операций");
                        $(".info").addClass("alert alert-success");
                    }
                    if (status == -1) {
                        $(".info").html(nameObject + " для добавления в мастер массовых операций отсутствуют");
                        $(".info").addClass("alert alert-danger");
                    }
                }
            });
        }
    });
});