//Date range picker
$('.select2').select2()

$('#startDate').datepicker({
    format: 'mm/yyyy',
    autoclose: true,
    viewMode: 'months',
    minViewMode: 'months'
});

$.post('GetProjectOrderBookedReport').done(function (resultData) {
    debugger;
    if (resultData != null && resultData != undefined) {
        var barChartData = {
            labels: resultData.labelData,
            datasets: [
             {
                 label: 'Cumulative upto Prev Month',
                 fillColor: '#9C27B0',
                 strokeColor: 'rgba(210, 214, 222, 1)',
                 pointColor: 'rgba(210, 214, 222, 1)',
                 pointStrokeColor: '#c1c7d1',
                 pointHighlightFill: '#fff',
                 pointHighlightStroke: 'rgba(220,220,220,1)',
                 data: resultData.c_PrevMonthData
             }, {
                 label: 'For this Month',
                 fillColor: '#00a65a',
                 strokeColor: 'rgba(60,141,188,0.8)',
                 pointColor: '#3b8bba',
                 pointStrokeColor: 'rgba(60,141,188,1)',
                 pointHighlightFill: '#fff',
                 pointHighlightStroke: 'rgba(60,141,188,1)',
                 data: resultData.c_CurrentMonthData
             }, {
                 label: 'Cumulative upto This Month',
                 fillColor: '#3c8dbc',
                 strokeColor: 'rgba(60,141,188,0.8)',
                 pointColor: '#3b8bba',
                 pointStrokeColor: 'rgba(60,141,188,1)',
                 pointHighlightFill: '#fff',
                 pointHighlightStroke: 'rgba(60,141,188,1)',
                 data: resultData.c_UptoCurrentMonthData
             }, {
                 label: 'Last Year Cumulative upto Prev Month',
                 fillColor: '#F44336',
                 strokeColor: 'rgba(210, 214, 222, 1)',
                 pointColor: 'rgba(210, 214, 222, 1)',
                 pointStrokeColor: '#c1c7d1',
                 pointHighlightFill: '#fff',
                 pointHighlightStroke: 'rgba(220,220,220,1)',
                 data: resultData.p_PrevMonthData
             }, {
                 label: 'Last Year Value for this Month',
                 fillColor: '#8BC34A',
                 strokeColor: 'rgba(60,141,188,0.8)',
                 pointColor: '#3b8bba',
                 pointStrokeColor: 'rgba(60,141,188,1)',
                 pointHighlightFill: '#fff',
                 pointHighlightStroke: 'rgba(60,141,188,1)',
                 data: resultData.p_CurrentMonthData
             }, {
                 label: 'Last Year Cumulative upto This Month',
                 fillColor: '#72d4e0',
                 strokeColor: 'rgba(60,141,188,0.8)',
                 pointColor: '#3b8bba',
                 pointStrokeColor: 'rgba(60,141,188,1)',
                 pointHighlightFill: '#fff',
                 pointHighlightStroke: 'rgba(60,141,188,1)',
                 data: resultData.p_UptoCurrentMonthData
             }]
        }
        barChartData.datasets[1].fillColor = '#00a65a'
        barChartData.datasets[1].strokeColor = '#00a65a'
        barChartData.datasets[1].pointColor = '#00a65a'
        var barChartOptions = {
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
            maintainAspectRatio: true
        }

        barChartOptions.datasetFill = false

        $('#divChartContainer').empty();
        $('#divChartContainer').append('<canvas id="barChart" style="height:250px"></canvas>');
        var barChartCanvas = $('#barChart').get(0).getContext('2d')
        var barChart = new Chart(barChartCanvas).Bar(barChartData, barChartOptions)
        $("#bar_legend").html(barChart.generateLegend());


    }
});


$("#filter").click(function () {
    debugger;
    $.post('GetProjectOrderBookedReport', { startDate: $('#startDate').val(), division: $('#division').val(), branch: $('#branch').val().toString() }).done(function (resultData) {
        debugger;
        if (resultData != null && resultData != undefined) {
            var barChartData = {
                labels: resultData.labelData,
                datasets: [
                 {
                     label: 'Cumulative upto Prev Month',
                     fillColor: '#9C27B0',
                     strokeColor: 'rgba(210, 214, 222, 1)',
                     pointColor: 'rgba(210, 214, 222, 1)',
                     pointStrokeColor: '#c1c7d1',
                     pointHighlightFill: '#fff',
                     pointHighlightStroke: 'rgba(220,220,220,1)',
                     data: resultData.c_PrevMonthData
                 }, {
                     label: 'For this Month',
                     fillColor: '#00a65a',
                     strokeColor: 'rgba(60,141,188,0.8)',
                     pointColor: '#3b8bba',
                     pointStrokeColor: 'rgba(60,141,188,1)',
                     pointHighlightFill: '#fff',
                     pointHighlightStroke: 'rgba(60,141,188,1)',
                     data: resultData.c_CurrentMonthData
                 }, {
                     label: 'Cumulative upto This Month',
                     fillColor: '#3c8dbc',
                     strokeColor: 'rgba(60,141,188,0.8)',
                     pointColor: '#3b8bba',
                     pointStrokeColor: 'rgba(60,141,188,1)',
                     pointHighlightFill: '#fff',
                     pointHighlightStroke: 'rgba(60,141,188,1)',
                     data: resultData.c_UptoCurrentMonthData
                 }, {
                     label: 'Last Year Cumulative upto Prev Month',
                     fillColor: '#9C27B0',
                     strokeColor: 'rgba(210, 214, 222, 1)',
                     pointColor: 'rgba(210, 214, 222, 1)',
                     pointStrokeColor: '#c1c7d1',
                     pointHighlightFill: '#fff',
                     pointHighlightStroke: 'rgba(220,220,220,1)',
                     data: resultData.p_PrevMonthData
                 }, {
                     label: 'Last Year Value for this Month',
                     fillColor: '#00a65a',
                     strokeColor: 'rgba(60,141,188,0.8)',
                     pointColor: '#3b8bba',
                     pointStrokeColor: 'rgba(60,141,188,1)',
                     pointHighlightFill: '#fff',
                     pointHighlightStroke: 'rgba(60,141,188,1)',
                     data: resultData.p_CurrentMonthData
                 }, {
                     label: 'Last Year Cumulative upto This Month',
                     fillColor: '#3c8dbc',
                     strokeColor: 'rgba(60,141,188,0.8)',
                     pointColor: '#3b8bba',
                     pointStrokeColor: 'rgba(60,141,188,1)',
                     pointHighlightFill: '#fff',
                     pointHighlightStroke: 'rgba(60,141,188,1)',
                     data: resultData.p_UptoCurrentMonthData
                 }]
            }
            barChartData.datasets[1].fillColor = '#00a65a'
            barChartData.datasets[1].strokeColor = '#00a65a'
            barChartData.datasets[1].pointColor = '#00a65a'
            var barChartOptions = {
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
                maintainAspectRatio: true
            }

            barChartOptions.datasetFill = false

            $('#divChartContainer').empty();
            $('#divChartContainer').append('<canvas id="barChart" style="height:250px"></canvas>');
            var barChartCanvas = $('#barChart').get(0).getContext('2d')
            var barChart = new Chart(barChartCanvas).Bar(barChartData, barChartOptions)
            $("#bar_legend").html(barChart.generateLegend());


        }
    });

});


