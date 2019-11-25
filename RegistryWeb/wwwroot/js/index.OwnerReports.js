$(function () {
    $('.reportStatistic').hide();
    $('.toggleReportStatistic').click(function (event) {
        var toggleReportStatistic = $(this);
        var id = toggleReportStatistic.data('id');
        var reportStatistic = $('.reportStatistic').filter(function (i) {
            return $(this).data('id') === id;
        });
        if (reportStatistic.is(":hidden")) {
            toggleReportStatistic
                .prop('disabled', true)
                .html('<span class="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true"></span>Статистика');
            $.ajax({
                async: true,
                type: 'POST',
                url: window.location.origin + '/Statistic/ReestrStatistic',
                success: function (data) {
                    reportStatistic.find('.date_S').empty();
                    reportStatistic.find('.countMKD_S').empty();
                    reportStatistic.find('.countPremAndSubPrem_S').empty();
                    reportStatistic.find('.countTenancy_S').empty();
                    reportStatistic.find('.countOwner_S').empty();
                    reportStatistic.find('.countInhabitant_S').empty();
                    reportStatistic.find('.totalAreaSum_S').empty();
                    reportStatistic.find('.livingAreaSum_S').empty();
                    reportStatistic.find('.date_S').append(data.date);
                    reportStatistic.find('.countMKD_S').append(data.countMKD);
                    reportStatistic.find('.countPremAndSubPrem_S').append(data.countTenancy + data.countOwner);
                    reportStatistic.find('.countTenancy_S').append(data.countTenancy);
                    reportStatistic.find('.countOwner_S').append(data.countOwner);
                    reportStatistic.find('.countInhabitant_S').append(data.countInhabitant);
                    reportStatistic.find('.totalAreaSum_S').append(data.totalAreaSum);
                    reportStatistic.find('.livingAreaSum_S').append(data.livingAreaSum);
                    toggleReportStatistic
                        .prop('disabled', false)
                        .html('Статистика');
                    reportStatistic.show();
                }
            });
        }
        else {
            reportStatistic.hide();
        }
    });
});