function getPaymentAccountTableSetting() {
    return {
        HasTenant: $('#Column_Tenant').is(':checked'),
        HasTotalArea: $('#Column_TotalArea').is(':checked'),
        HasLivingArea: $('#Column_LivingArea').is(':checked'),
        HasPrescribed: $('#Column_Prescribed').is(':checked'),
        HasBalanceInput: $('#Column_BalanceInput').is(':checked'),
        HasBalanceTenancy: $('#Column_BalanceTenancy').is(':checked'),
        HasBalanceInputPenalties: $('#Column_BalanceInputPenalties').is(':checked'),
        HasBalanceDgi: $('#Column_BalanceDgi').is(':checked'),
        HasBalancePadun: $('#Column_BalancePadun').is(':checked'),
        HasBalancePkk: $('#Column_BalancePkk').is(':checked'),
        HasChargingTotal: $('#Column_ChargingTotal').is(':checked'),
        HasChargingTenancy: $('#Column_ChargingTenancy').is(':checked'),
        HasChargingPenalties: $('#Column_ChargingPenalties').is(':checked'),
        HasChargingDgi: $('#Column_ChargingDgi').is(':checked'),
        HasChargingPadun: $('#Column_ChargingPadun').is(':checked'),
        HasChargingPkk: $('#Column_ChargingPkk').is(':checked'),
        HasTransferBalance: $('#Column_TransferBalance').is(':checked'),
        HasRecalcTenancy: $('#Column_RecalcTenancy').is(':checked'),
        HasRecalcPenalties: $('#Column_RecalcPenalties').is(':checked'),
        HasRecalcDgi: $('#Column_RecalcDgi').is(':checked'),
        HasRecalcPadun: $('#Column_RecalcPadun').is(':checked'),
        HasRecalcPkk: $('#Column_RecalcPkk').is(':checked'),
        HasPaymentTenancy: $('#Column_PaymentTenancy').is(':checked'),
        HasPaymentPenalties: $('#Column_PaymentPenalties').is(':checked'),
        HasPaymentDgi: $('#Column_PaymentDgi').is(':checked'),
        HasPaymentPadun: $('#Column_PaymentPadun').is(':checked'),
        HasPaymentPkk: $('#Column_PaymentPkk').is(':checked'),
        HasBalanceOutputTotal: $('#Column_BalanceOutputTotal').is(':checked'),
        HasBalanceOutputTenancy: $('#Column_BalanceOutputTenancy').is(':checked'),
        HasBalanceOutputPenalties: $('#Column_BalanceOutputPenalties').is(':checked'),
        HasBalanceOutputDgi: $('#Column_BalanceOutputDgi').is(':checked'),
        HasBalanceOutputPadun: $('#Column_BalanceOutputPadun').is(':checked'),
        HasBalanceOutputPkk: $('#Column_BalanceOutputPkk').is(':checked')
    };
}
function getArrayHeaderTableSetting() {
    var arr = [{ name: "Date", visible: true }];
    var inputs = $('#configModalForm input');
    for (var i = 0; i < inputs.length; i++) {
        arr.push({
            name: inputs[i].id.substr(7),
            visible: inputs[i].checked
        });
    }
    return arr;
}
$(document).ready(function () {
    var cols = getArrayHeaderTableSetting();
    var table = $('#ttt').DataTable({
        columns: cols,
        fixedHeader: true,
        fixedColumns: true,
        scrollX: true,
        scrollY: "55vh",
        scrollCollapse: true,
        paging: false,
        ordering: false,
        info: false,
        searching: false
    });
    $("#configModalShow").on("click", function (e) {
        e.preventDefault();
        $("#configModal").modal("show");
    });
    $("#configModalForm .r-config-apply").on("click", function (e) {
        e.preventDefault();
        $("[id^='Column_']").each(function () {
            var name = this.id.substr(7);
            var isEnable = this.checked;
            var col = table.column(name + ':name');
            if ( !col.visible() && isEnable || col.visible() && !isEnable) {
                col.visible(isEnable);
            }
        });
        var setting = getPaymentAccountTableSetting();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/PaymentAccounts/SavePaymentAccountTableJson',
            data: setting,
            success: function (isSuccess) {
                if (!isSuccess) {
                    alert("Ошибка сохранения настроек! Обратитесь в ЦИТ!");
                }
            }
        });
        $("#configModal").modal("hide");
    });
});