$(function () {
    var reestrStatistic = $('#reestrStatistic');
    var toggleReestrStatistic = $('#toggleReestrStatistic');
    reestrStatistic.hide();
    toggleReestrStatistic.click(function (event) {
        if (reestrStatistic.is(":hidden")) {
            toggleReestrStatistic
                .prop('disabled', true)
                .html('<span class="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true"></span>Статистика');
            $.ajax({
                async: true,
                type: 'POST',
                url: window.location.origin + '/ReestrEmergencyPremises/ReestrStatistic',
                success: function (data) {
                    reestrStatistic.find('.date_S').empty();
                    reestrStatistic.find('.countMKD_S').empty();
                    reestrStatistic.find('.countPremAndSubPrem_S').empty();
                    reestrStatistic.find('.countTenancy_S').empty();
                    reestrStatistic.find('.countOwner_S').empty();
                    reestrStatistic.find('.countInhabitant_S').empty();
                    reestrStatistic.find('.totalAreaSum_S').empty();
                    reestrStatistic.find('.livingAreaSum_S').empty();
                    reestrStatistic.find('.date_S').append(data.date);
                    reestrStatistic.find('.countMKD_S').append(data.countMKD);
                    reestrStatistic.find('.countPremAndSubPrem_S').append(data.countTenancy + data.countOwner);
                    reestrStatistic.find('.countTenancy_S').append(data.countTenancy);
                    reestrStatistic.find('.countOwner_S').append(data.countOwner);
                    reestrStatistic.find('.countInhabitant_S').append(data.countInhabitant);
                    reestrStatistic.find('.totalAreaSum_S').append(data.totalAreaSum);
                    reestrStatistic.find('.livingAreaSum_S').append(data.livingAreaSum);
                    toggleReestrStatistic
                        .prop('disabled', false)
                        .html('Статистика');
                    reestrStatistic.show();
                }
            });
        }
        else {
            reestrStatistic.hide();
        }
    });
});