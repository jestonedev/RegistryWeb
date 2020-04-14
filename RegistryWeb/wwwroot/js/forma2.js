var multiReportPremiseClick = function () {
    var btn = $(this);
    var idBuilding = btn.data("idbuilding");
    var ids = $.makeArray($(".trPremise")
            .filter(function () {
                return $(this).data("idbuilding") == idBuilding;
            }).map(function () {
                return $(this).data("idpremise");
            })
        );
    btn
        .prop("disabled", true)
        .removeClass("oi-document")
        .addClass("oi-data-transfer-download")
        .html("");
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/OwnerReports/MultiForma2',
        data: { ids: ids },
        fail: function (msg) {
            $("html").html(msg.responseText);
        },
        success: function (result) {
            window.location = window.location.origin + '/OwnerReports/GetMultiForma2?fileNameReport=' + result.fileNameReport;
            btn
                .prop("disabled", false)
                .removeClass("oi-data-transfer-download")
                .addClass("oi-document")
                .html("2");
        }
    });
}
var trBuildingClick = function (event) {
    if (event.target.nodeName == "BUTTON" || event.target.nodeName == "A")
        return;
    var b = $(this);
    var idBuilding = b.data("idbuilding");
    var p = $(".trPremise").filter(function () {
        return $(this).data("idbuilding") == idBuilding;
    });
    if (b.data("checked")) {
        b.data("checked", false);
        p.hide();
    }
    else {
        b.data("checked", true);
        p.show();
    }
}
var initialization = function () {
    $(".trPremise").hide();

    $(".trBuilding").on("click", trBuildingClick);
    
    $(".multiReportPremise").on("click", multiReportPremiseClick);
}
$(function () {
    initialization();
});
