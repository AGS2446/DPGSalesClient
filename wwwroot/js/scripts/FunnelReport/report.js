$('.select2').select2()

$('#DateFrom').datepicker({
    autoclose: true,
    format: 'dd/mm/yyyy',
    endDate: "today"
});

$('#DateTo').datepicker({
    autoclose: true,
    format: 'dd/mm/yyyy',
    endDate: "today",
}).on('changeDate', function (ev) {

    if ($('#DateFrom').val() == "") {
        AlertPopup('Please select Date From ');
        $('#DateTo').val('');

    } else if (ConvertStrDate($('#DateFrom').val()) > ConvertStrDate($('#DateTo').val())) {
        AlertPopup('Date From should be less than or equal to Date To ');
        $('#DateFrom').val('');
        $('#DateTo').val('');
    }
});

function ConvertStrDate(strDate) {
    debugger;
    if (strDate != null && strDate != "" && strDate != undefined) {
        return new Date(strDate.toString().split('/')[2], strDate.toString().split('/')[1], strDate.toString().split('/')[0])
    } else {
        return null;

    }
}

$('#btnSubmitFunnel').click(function () {
    debugger;
    $('#Division').val($('#divisionid').val());
    $('#Branch').val($('#branchid').val());
    $('#Segment').val($('#segmentid').val());
    var validator = $("#frmFunnelReport").validate();
    var isValid = validator.form();
    var data = $("#frmFunnelReport").serialize();

    if (isValid) {
        debugger;

        $.post('GetFunnelReportChart', { Division: $('#Division').val(), Branch: $('#Branch').val(), Segment: $('#Segment').val(), DateFrom: $('#DateFrom').val(), DateTo: $('#DateTo').val() }).done(function (resultData) {
            if (resultData !== null && resultData !== undefined) {
                $("#initialload").hide();
                debugger;
                var leadValue = '';
                var orderValue = '';
                var quoteValue = '';

                var Enquiries = [];
                var Leads = [];
                var Orders = [];
                var Quotes = [];
                var ModelName = [];
                var Value = [];


                $.each(resultData, function (i, item) {
                    if (i == "enquiries") {
                        Enquiries.push('Enquiries - Count :' + item)
                    }
                    if (i == "enquiryValue") {
                        var enqval = parseFloat(item).toFixed(2);
                        if (enqval != "NaN") {
                            Enquiries.push(parseFloat(enqval));
                        }
                        else if (enqval == "NaN") {
                            Enquiries[1] = 0;
                        }
                    }
                    if (i == "leads") {
                        Leads.push('Leads - Count :' + item)
                    }
                    if (i == "orders") {
                        Orders.push('Orders - Count :' + item)
                    }
                    if (i == "quotes") {
                        Quotes.push('Quotes - Count :' + item)
                    }


                    if (i == "leadValue") {
                        leadValue = parseFloat(item).toFixed(2);
                    }
                    if (i == "orderValue") {
                        orderValue = parseFloat(item).toFixed(2);
                    }
                    if (i == "quoteValue") {
                        quoteValue = parseFloat(item).toFixed(2);
                    }
                    if (Leads[0] != null && Leads[0] != undefined && Leads[1] == null) {
                        if (leadValue != "NaN") {
                            Leads.push(parseFloat(leadValue));
                        }
                        else if (leadValue == "NaN") {
                            Leads[1] = 0;
                        }
                    }
                    if (Orders[0] != null && Orders[0] != undefined && Orders[1] == null) {
                        if (orderValue != "NaN") {
                            Orders.push(parseFloat(orderValue));
                        }
                        else if (orderValue == "NaN") {
                            Orders[1] = 0;
                        }
                    }
                    if (Quotes[0] != null && Quotes[0] != undefined && Quotes[1] == null) {
                        if (quoteValue != "NaN") {
                            Quotes.push(parseFloat(quoteValue));
                        }
                        else if (quoteValue == "NaN") {
                            Quotes[1] = 0;
                        }
                    }

                })


                Highcharts.chart('container', {
                    chart: {
                        type: 'funnel'
                    },
                    credits: {
                        enabled: false
                    },
                    exporting: {
                        enabled: false
                    },
                    title: {
                        text: 'Sales funnel'
                    },
                    plotOptions: {
                        series: {
                            dataLabels: {
                                enabled: true,
                                format: '<b>{point.name}</b>  ({point.y:,.0f} <b> L</b>) ',
                                color: (Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black',
                                softConnector: true
                            },
                            center: ['40%', '50%'],
                            neckWidth: '20%',
                            neckHeight: '15%',
                            width: '50%'
                        }
                    },
                    legend: {
                        enabled: false
                    },
                    series: [{
                        name: 'Value',
                        data: [
                             Leads, Enquiries, Quotes, Orders,

                        ]
                    }]
                });

            }

        });

    } else {
        //  alert('failed');
    }
});
