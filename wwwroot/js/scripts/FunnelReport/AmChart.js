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

                var Enquiries = {};
                var Leads = {};
                var Orders = {};
                var Quotes = {};
                var ModelName = {};
                var Value = {};


                $.each(resultData, function (i, item) {
                    if (i == "enquiries") {
                        //Enquiries.push({ "title": 'Enquiries - Count :' + item })
                        Enquiries['title'] = 'Enquiries - Count :' + item;
                    }
                    if (i == "enquiryValue") {
                        var enqval = parseFloat(item).toFixed(2);
                        if (enqval != "NaN") {
                            Enquiries['value'] = parseFloat(enqval)
                        }
                        else if (enqval == "NaN") {
                            Enquiries['value'] = 0;
                        }
                    }
                    if (i == "leads") {
                        Leads['title'] = 'Leads - Count :' + item
                    }
                    if (i == "orders") {
                        Orders['title']= 'Orders - Count :' + item
                    }
                    if (i == "quotes") {
                        Quotes['title']= 'Quotes - Count :' + item
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
                    if (Leads['title'] != null && Leads['title'] != undefined && Leads['value'] == null) {
                        if (leadValue != "NaN") {
                            Leads['value'] = parseFloat(leadValue)
                        }
                        else if (leadValue == "NaN") {
                            Leads['value'] = 0;
                        }
                    }
                    if (Orders['title'] != null && Orders['title'] != undefined && Orders['value'] == null) {
                        if (orderValue != "NaN") {
                            Orders['value'] = parseFloat(orderValue)
                        }
                        else if (orderValue == "NaN") {
                            Orders['value'] = 0;
                        }
                    }
                    if (Quotes['title'] != null && Quotes['title'] != undefined && Quotes['value'] == null) {
                        if (quoteValue != "NaN") {
                            Quotes['value'] = parseFloat(quoteValue)
                        }
                        else if (quoteValue == "NaN") {
                            Quotes['value'] = 0;
                        }
                    }

                })
                //Am chart 
                var chart = AmCharts.makeChart("container", {
                    "type": "funnel",
                    "theme": "light",
                    "hideCredits":true,
                    "dataProvider": [Leads, Enquiries, Quotes, Orders, ],
                    "balloon": {
                        "fixedPosition": true
                    },
                    "valueField": "value",
                    "titleField": "title",
                    "marginRight": 300,
                    "marginLeft": 150,
                    "startX": -500,
                    "depth3D": 100,
                    "angle": 40,
                    "outlineAlpha": 1,
                    "outlineColor": "#FFFFFF",
                    "outlineThickness": 2,
                    "labelPosition": "right",
                    "balloonText": "[[title]], value: [[value]] Lac[[description]]",
                    "export": {
                        "enabled": false
                    }
                });
                //Endreasion

                //Highcharts.chart('container', {
                //    chart: {
                //        type: 'funnel'
                //    },
                //    credits: {
                //        enabled: false
                //    },
                //    exporting: {
                //        enabled: false
                //    },
                //    title: {
                //        text: 'Sales funnel'
                //    },
                //    plotOptions: {
                //        series: {
                //            dataLabels: {
                //                enabled: true,
                //                format: '<b>{point.name}</b>  ({point.y:,.0f} <b> L</b>) ',
                //                color: (Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black',
                //                softConnector: true
                //            },
                //            center: ['40%', '50%'],
                //            neckWidth: '20%',
                //            neckHeight: '15%',
                //            width: '50%'
                //        }
                //    },
                //    legend: {
                //        enabled: false
                //    },
                //    series: [{
                //        name: 'Value',
                //        data: [
                //             Leads, Enquiries, Quotes, Orders,

                //        ]
                //    }]
                //});

            }

        });

    } else {
        //  alert('failed');
    }


});
