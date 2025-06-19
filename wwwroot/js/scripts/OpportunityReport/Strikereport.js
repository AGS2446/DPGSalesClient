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

$("#frmOpportunityReport").validate({
    errorClass: "error-class",
    rules: {
        DateFrom: {
            required: true
        },
        DateTo: {
            required: true
        }

    },
    messages: {

        DateFrom: {
            required: "Please select Date From"
        },
        DateTo: {
            required: "Please select Date To"
        }

    },
    submitHandler: function (form) {
        // do other things for a valid form
        $('#Division').val($('#divisionid').val());
        $('#Branch').val($('#branchid').val());
        $("#initialload").hide();
        form.submit();
    },
    errorPlacement: function (error, element) {
        if (element.parent('.input-group').length) {
            error.insertAfter(element.parent());
        }
        else {
            error.insertAfter(element);
        }

        //else if (element.parent('.my-input-group').length) {
        //        error.insertAfter(element.parent());
        //    }
    },
    invalidHandler: function (event, validator) {
        // 'this' refers to the form
        $("#initialload").hide();
    }
});

$('#StrikeLeadbtnSubmit').click(function () {
    debugger;
    $('#Division').val($('#divisionid').val());
    $('#Branch').val($('#branchid').val());
    var validator = $("#frmOpportunityReport").validate();
    var isValid = validator.form();
    var data = $("#frmOpportunityReport").serialize();

    if (isValid) {
        debugger;

        $.post('GetStrikeLeadReport', data).done(function (resultData) {
            if (resultData !== null && resultData !== undefined) {
                $("#initialload").hide();
                debugger;

                var barChartData = {
                    labels: resultData.labelData,
                    datasets: [
                  {
                      fillOpacity: 0.5,
                      fill: true,
                      backgroundColor: "rgba(75,192,192,0.4)",

                      label: 'Months',
                      fillColor: 'rgba(75,192,192,1)',
                      //strokeColor: '#B67AFF',
                      pointColor: '#fff',
                      //pointStrokeColor: 'rgba(60,141,188,1)',
                      //pointHighlightFill: '#fff',
                      //pointHighlightStroke: 'rgba(60,141,188,1)',
                      data: resultData.monthData,

                      //bezierCurve: true,
                      //fill: true,
                      //lineTension: 0.1,
                      //backgroundColor: "#ffccdd",
                      borderColor: "rgba(75,192,192,1)",
                      borderCapStyle: 'butt',
                      borderDash: [],
                      borderDashOffset: 0.0,
                      borderJoinStyle: 'miter',
                      pointBorderColor: "rgba(75,192,192,1)",
                     // pointBackgroundColor: "#fff",
                      pointBorderWidth: 1,
                      pointHoverRadius: 5,
                      pointHoverBackgroundColor: "rgba(75,192,192,1)",
                      pointHoverBorderColor: "rgba(220,220,220,1)",
                      pointHoverBorderWidth: 2,
                      pointRadius: 5,
                      pointHitRadius: 10,
                      //data: resultData.monthData
                  }

                    ]
                }
                var option = {
                    showLines: true,
                    //fill: true,
                    //backgroundColor: "rgba(75,192,192,0.4)",
                };
                //barChartData.datasets[1].fillColor = '#00a65a'
                //barChartData.datasets[1].strokeColor = '#00a65a'
                //barChartData.datasets[1].pointColor = '#00a65a'
                var barChartOptions = {
                    showTooltips: false,
                    fill: true,
                    bezierCurve: true,
                    spanGaps: true,
                    //Boolean - Whether the scale should start at zero, or an order of magnitude down from the lowest value
                    scaleBeginAtZero: true,
                    //Boolean - Whether grid lines are shown across the chart
                    scaleShowGridLines: true,
                    //String - Colour of the grid lines
                    scaleGridLineColor: 'rgba(0,0,0,.05)',
                    //Number - Width of the grid lines
                    scaleGridLineWidth: 1,
                    //Boolean - Whether to show horizontal lines (except X axis)
                    scaleShowHorizontalLines: true,
                    //Boolean - Whether to show vertical lines (except Y axis)
                    scaleShowVerticalLines: true,
                    //Boolean - If there is a stroke on each bar
                    barShowStroke: true,
                    //Number - Pixel width of the bar stroke
                    barStrokeWidth: 2,
                    //Number - Spacing between each of the X value sets
                    barValueSpacing: 5,
                    //Number - Spacing between data sets within X values
                    barDatasetSpacing: 1,
                    //String - A legend template
                    legendTemplate: '<ul class="<%=name.toLowerCase()%>-legend"><% for (var i=0; i<datasets.length; i++){%><li><span style="background-color:<%=datasets[i].fillColor%>"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>',
                    //Boolean - whether to make the chart responsive
                    responsive: true,
                    fillOpacity: 0.5,
                    maintainAspectRatio: true,
                    onAnimationComplete: function () {

                        var ctx = this.chart.ctx;
                        ctx.font = this.scale.font;
                        ctx.fillStyle = this.scale.textColor
                        ctx.textAlign = "center";
                        ctx.textBaseline = "bottom";

                        this.datasets.forEach(function (dataset) {
                            dataset.points.forEach(function (points) {
                                ctx.fillText(points.value + ' Cr.', points.x, points.y - 10);
                            });
                        })
                    }
                }

                //barChartOptions.datasetFill = false

                $('#divChartContainer').empty();
                $('#divChartContainer').append('<canvas id="barChartOpprtunity" style="height:250px"></canvas>');
                var barChartCanvas = $('#barChartOpprtunity').get(0).getContext('2d')
                var barChart = new Chart(barChartCanvas).Line(barChartData, barChartOptions)
                //$("#bar_legend").html(barChart.generateLegend());
            }

        });

    } else {
        //  alert('failed');
    }
});


//--------------

$('#EnquairySubmit').click(function () {
    debugger;
    $('#Division').val($('#divisionid').val());
    $('#Branch').val($('#branchid').val());
    var validator = $("#frmOpportunityReport").validate();
    var isValid = validator.form();
    var data = $("#frmOpportunityReport").serialize();

    if (isValid) {
        debugger;

        $.post('GetStrikeEnquiryReport', data).done(function (resultData) {
            if (resultData !== null && resultData !== undefined) {
                $("#initialload").hide();
                debugger;

                var barChartData = {
                    labels: resultData.labelData,
                    datasets: [
                  {
                      fillOpacity: 0.5,
                      fill: true,
                      backgroundColor: "rgba(75,192,192,0.4)",

                      label: 'Months',
                      fillColor: 'rgba(75,192,192,1)',
                      //strokeColor: '#B67AFF',
                      pointColor: '#fff',
                      //pointStrokeColor: 'rgba(60,141,188,1)',
                      //pointHighlightFill: '#fff',
                      //pointHighlightStroke: 'rgba(60,141,188,1)',
                      data: resultData.monthData,

                      //bezierCurve: true,
                      //fill: true,
                      //lineTension: 0.1,
                      //backgroundColor: "#ffccdd",
                      borderColor: "rgba(75,192,192,1)",
                      borderCapStyle: 'butt',
                      borderDash: [],
                      borderDashOffset: 0.0,
                      borderJoinStyle: 'miter',
                      pointBorderColor: "rgba(75,192,192,1)",
                      // pointBackgroundColor: "#fff",
                      pointBorderWidth: 1,
                      pointHoverRadius: 5,
                      pointHoverBackgroundColor: "rgba(75,192,192,1)",
                      pointHoverBorderColor: "rgba(220,220,220,1)",
                      pointHoverBorderWidth: 2,
                      pointRadius: 5,
                      pointHitRadius: 10,
                      //data: resultData.monthData
                  }

                    ]
                }
                var option = {
                    showLines: true,
                    //fill: true,
                    //backgroundColor: "rgba(75,192,192,0.4)",
                };
                //barChartData.datasets[1].fillColor = '#00a65a'
                //barChartData.datasets[1].strokeColor = '#00a65a'
                //barChartData.datasets[1].pointColor = '#00a65a'
                var barChartOptions = {
                    showTooltips: false,
                    fill: true,
                    bezierCurve: true,
                    spanGaps: true,
                    //Boolean - Whether the scale should start at zero, or an order of magnitude down from the lowest value
                    scaleBeginAtZero: true,
                    //Boolean - Whether grid lines are shown across the chart
                    scaleShowGridLines: true,
                    //String - Colour of the grid lines
                    scaleGridLineColor: 'rgba(0,0,0,.05)',
                    //Number - Width of the grid lines
                    scaleGridLineWidth: 1,
                    //Boolean - Whether to show horizontal lines (except X axis)
                    scaleShowHorizontalLines: true,
                    //Boolean - Whether to show vertical lines (except Y axis)
                    scaleShowVerticalLines: true,
                    //Boolean - If there is a stroke on each bar
                    barShowStroke: true,
                    //Number - Pixel width of the bar stroke
                    barStrokeWidth: 2,
                    //Number - Spacing between each of the X value sets
                    barValueSpacing: 5,
                    //Number - Spacing between data sets within X values
                    barDatasetSpacing: 1,
                    //String - A legend template
                    legendTemplate: '<ul class="<%=name.toLowerCase()%>-legend"><% for (var i=0; i<datasets.length; i++){%><li><span style="background-color:<%=datasets[i].fillColor%>"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>',
                    //Boolean - whether to make the chart responsive
                    responsive: true,
                    fillOpacity: 0.5,
                    maintainAspectRatio: true,
                    onAnimationComplete: function () {

                        var ctx = this.chart.ctx;
                        ctx.font = this.scale.font;
                        ctx.fillStyle = this.scale.textColor
                        ctx.textAlign = "center";
                        ctx.textBaseline = "bottom";

                        this.datasets.forEach(function (dataset) {
                            dataset.points.forEach(function (points) {
                                ctx.fillText(points.value + ' Cr.', points.x, points.y - 10);
                            });
                        })
                    }
                }

                //barChartOptions.datasetFill = false

                $('#divChartContainer').empty();
                $('#divChartContainer').append('<canvas id="barChartOpprtunity" style="height:250px"></canvas>');
                var barChartCanvas = $('#barChartOpprtunity').get(0).getContext('2d')
                var barChart = new Chart(barChartCanvas).Line(barChartData, barChartOptions)
                //$("#bar_legend").html(barChart.generateLegend());
            }

        });

    } else {
        //  alert('failed');
    }
});

//quotereport

$('#StrikeQuoteSubmit').click(function () {
    debugger;
    $('#Division').val($('#divisionid').val());
    $('#Branch').val($('#branchid').val());
    var validator = $("#frmOpportunityReport").validate();
    var isValid = validator.form();
    var data = $("#frmOpportunityReport").serialize();

    if (isValid) {
        debugger;

        $.post('GetStrikeQuoteReport', data).done(function (resultData) {
            if (resultData !== null && resultData !== undefined) {
                $("#initialload").hide();
                debugger;

                var barChartData = {
                    labels: resultData.labelData,
                    datasets: [
                  {
                      fillOpacity: 0.5,
                      fill: true,
                      backgroundColor: "rgba(75,192,192,0.4)",

                      label: 'Months',
                      fillColor: 'rgba(75,192,192,1)',
                      //strokeColor: '#B67AFF',
                      pointColor: '#fff',
                      //pointStrokeColor: 'rgba(60,141,188,1)',
                      //pointHighlightFill: '#fff',
                      //pointHighlightStroke: 'rgba(60,141,188,1)',
                      data: resultData.monthData,

                      //bezierCurve: true,
                      //fill: true,
                      //lineTension: 0.1,
                      //backgroundColor: "#ffccdd",
                      borderColor: "rgba(75,192,192,1)",
                      borderCapStyle: 'butt',
                      borderDash: [],
                      borderDashOffset: 0.0,
                      borderJoinStyle: 'miter',
                      pointBorderColor: "rgba(75,192,192,1)",
                      // pointBackgroundColor: "#fff",
                      pointBorderWidth: 1,
                      pointHoverRadius: 5,
                      pointHoverBackgroundColor: "rgba(75,192,192,1)",
                      pointHoverBorderColor: "rgba(220,220,220,1)",
                      pointHoverBorderWidth: 2,
                      pointRadius: 5,
                      pointHitRadius: 10,
                      //data: resultData.monthData
                  }

                    ]
                }
                var option = {
                    showLines: true,
                    //fill: true,
                    //backgroundColor: "rgba(75,192,192,0.4)",
                };
                //barChartData.datasets[1].fillColor = '#00a65a'
                //barChartData.datasets[1].strokeColor = '#00a65a'
                //barChartData.datasets[1].pointColor = '#00a65a'
                var barChartOptions = {
                    showTooltips: false,
                    fill: true,
                    bezierCurve: true,
                    spanGaps: true,
                    //Boolean - Whether the scale should start at zero, or an order of magnitude down from the lowest value
                    scaleBeginAtZero: true,
                    //Boolean - Whether grid lines are shown across the chart
                    scaleShowGridLines: true,
                    //String - Colour of the grid lines
                    scaleGridLineColor: 'rgba(0,0,0,.05)',
                    //Number - Width of the grid lines
                    scaleGridLineWidth: 1,
                    //Boolean - Whether to show horizontal lines (except X axis)
                    scaleShowHorizontalLines: true,
                    //Boolean - Whether to show vertical lines (except Y axis)
                    scaleShowVerticalLines: true,
                    //Boolean - If there is a stroke on each bar
                    barShowStroke: true,
                    //Number - Pixel width of the bar stroke
                    barStrokeWidth: 2,
                    //Number - Spacing between each of the X value sets
                    barValueSpacing: 5,
                    //Number - Spacing between data sets within X values
                    barDatasetSpacing: 1,
                    //String - A legend template
                    legendTemplate: '<ul class="<%=name.toLowerCase()%>-legend"><% for (var i=0; i<datasets.length; i++){%><li><span style="background-color:<%=datasets[i].fillColor%>"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>',
                    //Boolean - whether to make the chart responsive
                    responsive: true,
                    fillOpacity: 0.5,
                    maintainAspectRatio: true,
                    onAnimationComplete: function () {

                        var ctx = this.chart.ctx;
                        ctx.font = this.scale.font;
                        ctx.fillStyle = this.scale.textColor
                        ctx.textAlign = "center";
                        ctx.textBaseline = "bottom";

                        this.datasets.forEach(function (dataset) {
                            dataset.points.forEach(function (points) {
                                ctx.fillText(points.value + ' Cr.', points.x, points.y - 10);
                            });
                        })
                    }
                }

                //barChartOptions.datasetFill = false

                $('#divChartContainer').empty();
                $('#divChartContainer').append('<canvas id="barChartOpprtunity" style="height:250px"></canvas>');
                var barChartCanvas = $('#barChartOpprtunity').get(0).getContext('2d')
                var barChart = new Chart(barChartCanvas).Line(barChartData, barChartOptions)
                //$("#bar_legend").html(barChart.generateLegend());
            }

        });

    } else {
        //  alert('failed');
    }
});






