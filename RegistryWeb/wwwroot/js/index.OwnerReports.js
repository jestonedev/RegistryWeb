$(function () {
    $('.reportStatistic').hide();
    $('.toggleReportStatistic').click(function (event) {
        var id = $(this).data('id');
        var reportStatistic = $('.reportStatistic').filter(function (i) {
            return $(this).data('id') === id;
        });
        if (reportStatistic.is(":hidden")) {
            console.log('развернуть');
            $.ajax({
                async: false,
                type: 'POST',
                url: window.location.origin + '/Statistic/ReestrStatistic',
                success: function (data) {
                    reportStatistic.find('.date_S').empty();
                    reportStatistic.find('.countMKD_S').empty();
                    console.log(data.date);
                    console.log(data.countMKD);
                    reportStatistic.find('.date_S').append(data.date);
                    reportStatistic.find('.countMKD_S').append(data.countMKD);
                }
            });
        }
        reportStatistic.toggle();
    });
});