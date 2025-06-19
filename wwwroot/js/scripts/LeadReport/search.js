$('.select2').select2()
$('#reservation').daterangepicker()

$('#DateFrom').datepicker({
    autoclose: true,
    format: 'yyyy/mm/dd',
    endDate: "today",
    onClose: function (selectedDate) {
                $("#DateTo").datepicker("option", "minDate", selectedDate);
            }
});

$('#DateTo').datepicker({
    autoclose: true,
    format: 'yyyy/mm/dd',
    endDate: "today",    
    onClose: function (selectedDate) {
        $("#DateFrom").datepicker("option", "maxDate", selectedDate);
    }
});

$("#frmLeadReport").validate({
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
$('#btnSubmit').click(function () {
    debugger;
    $('#Division').val($('#divisionid').val());
    $('#Branch').val($('#branchid').val());
    var validator = $("#frmLeadReport").validate();
    var isValid = validator.form();
    var data = $("#frmLeadReport").serialize();

    if (isValid) {
        debugger;

        $.post('GetLeadReportChart', data).done(function (resultData) {
            if (resultData !== null && resultData !== undefined) {
                $("#initialload").hide();
                debugger;

                var barChartData = {
                    labels: resultData.labelData,
                    datasets: [
                  {
                      label: 'Months',
                      fillColor: '#4DA59A',
                      strokeColor: 'rgba(60,141,188,0.8)',
                      pointColor: '#3b8bba',
                      pointStrokeColor: 'rgba(60,141,188,1)',
                      pointHighlightFill: '#fff',
                      pointHighlightStroke: 'rgba(60,141,188,1)',
                      data: resultData.monthData
                  }

                    ]
                }
                //barChartData.datasets[1].fillColor = '#00a65a'
                //barChartData.datasets[1].strokeColor = '#00a65a'
                //barChartData.datasets[1].pointColor = '#00a65a'
                var barChartOptions = {
                    showTooltips: false,
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
                    maintainAspectRatio: true,
                    onAnimationComplete: function () {

                        var ctx = this.chart.ctx;
                        ctx.font = this.scale.font;
                        ctx.fillStyle = this.scale.textColor
                        ctx.textAlign = "center";
                        ctx.textBaseline = "bottom";

                        this.datasets.forEach(function (dataset) {
                            dataset.bars.forEach(function (bar) {
                                ctx.fillText(bar.value, bar.x, bar.y - 5);
                            });
                        })
                    }
                }

                barChartOptions.datasetFill = false

                $('#divChartContainer').empty();
                $('#divChartContainer').append('<canvas id="barChart" style="height:250px"></canvas>');
                var barChartCanvas = $('#barChart').get(0).getContext('2d')
                var barChart = new Chart(barChartCanvas).Bar(barChartData, barChartOptions)
                //$("#bar_legend").html(barChart.generateLegend());
            }

        });

    } else {
        //  alert('failed');
    }
});

$('#btnGo').click(function () {
    debugger;
    $('#Division').val($('#divisionid').val());
    $('#Branch').val($('#branchid').val());
    var validator = $("#frmLeadReport").validate();
    var isValid = validator.form();
    var data = $("#frmLeadReport").serialize();

    if (isValid) {
        debugger;

        $.post('GetLeadReportChartSide', { DateRange: $('#reservation').val(), Division: $('#Division').val(), Branch: $('#Branch').val() }).done(function (resultData) {
            if (resultData !== null && resultData !== undefined) {
                $("#initialload").hide();
                debugger;

                var barChartData = {
                    labels: resultData.labelData,
                    datasets: [
                  {
                      label: 'Months',
                      fillColor: '#4DA59A',
                      strokeColor: 'rgba(60,141,188,0.8)',
                      pointColor: '#3b8bba',
                      pointStrokeColor: 'rgba(60,141,188,1)',
                      pointHighlightFill: '#fff',
                      pointHighlightStroke: 'rgba(60,141,188,1)',
                      data: resultData.monthData
                  }

                    ]
                }
                //barChartData.datasets[1].fillColor = '#00a65a'
                //barChartData.datasets[1].strokeColor = '#00a65a'
                //barChartData.datasets[1].pointColor = '#00a65a'
                var barChartOptions = {
                    showTooltips: false,
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
                    maintainAspectRatio: true,
                    onAnimationComplete: function () {

                        var ctx = this.chart.ctx;
                        ctx.font = this.scale.font;
                        ctx.fillStyle = this.scale.textColor
                        ctx.textAlign = "center";
                        ctx.textBaseline = "bottom";

                        this.datasets.forEach(function (dataset) {
                            dataset.bars.forEach(function (bar) {
                                ctx.fillText(bar.value, bar.x, bar.y - 5);
                            });
                        })
                    }
                }

                barChartOptions.datasetFill = false

                $('#divChartContainer').empty();
                $('#divChartContainer').append('<canvas id="barChart" style="height:250px"></canvas>');
                var barChartCanvas = $('#barChart').get(0).getContext('2d')
                var barChart = new Chart(barChartCanvas).Bar(barChartData, barChartOptions)
                //$("#bar_legend").html(barChart.generateLegend());
            }

        });

    } else {
        //  alert('failed');
    }
});
