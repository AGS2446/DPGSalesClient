
$("#select-quarter").val($("#hdnSelectedValue").val());


$.post('GetSalesReport', { strKey: $("#hdnSelectedValue").val() }).done(function (data) {
    debugger;
    if (data != null && data != undefined) {

        var pieOptions = {
            //Boolean - Whether we should show a stroke on each segment
            segmentShowStroke: true,
            //String - The colour of each segment stroke
            segmentStrokeColor: '#fff',
            //Number - The width of each segment stroke
            segmentStrokeWidth: 2,
            //Number - The percentage of the chart that we cut out of the middle
            percentageInnerCutout: 0, // This is 0 for Pie charts
            //Number - Amount of animation steps
            animationSteps: 100,
            //String - Animation easing effect
            animationEasing: 'easeOutBounce',
            //Boolean - Whether we animate the rotation of the Doughnut
            animateRotate: true,
            //Boolean - Whether we animate scaling the Doughnut from the centre
            animateScale: false,
            //Boolean - whether to make the chart responsive to window resizing
            responsive: true,
            // Boolean - whether to maintain the starting aspect ratio or not when responsive, if set to false, will take up entire container
            maintainAspectRatio: true,
            //String - A legend template
            legendTemplate: '<ul class="<%=name.toLowerCase()%>-legend"><% for (var i=0; i<segments.length; i++){%><li><span style="background-color:<%=segments[i].fillColor%>"></span><%if(segments[i].label){%><%=segments[i].label%> - <label style="color:<%=segments[i].fillColor%>"><%=segments[i].value%> Crs</label><%}%></li><%}%></ul>'
        }

        //Create pie or douhnut chart
        // You can switch between pie and douhnut using the method below.

        $('#divChartContainer').empty();
        $('#divChartContainer').append('<canvas id="pieChart" style="height:250px"></canvas>');
        var pieChartCanvas = $('#pieChart').get(0).getContext('2d')
        var pieChart = new Chart(pieChartCanvas).Pie(data, pieOptions)
        $("#pie_legend").html(pieChart.generateLegend());

    }
});


$('#period').change(function (objEdit) {
    debugger;
    $.post('GetSalesReport', { strKey: $(this).val() }).done(function (data) {
        debugger;
        if (data != null && data != undefined) {
            var pieOptions = {
                //Boolean - Whether we should show a stroke on each segment
                segmentShowStroke: true,
                //String - The colour of each segment stroke
                segmentStrokeColor: '#fff',
                //Number - The width of each segment stroke
                segmentStrokeWidth: 2,
                //Number - The percentage of the chart that we cut out of the middle
                percentageInnerCutout: 0, // This is 0 for Pie charts
                //Number - Amount of animation steps
                animationSteps: 100,
                //String - Animation easing effect
                animationEasing: 'easeOutBounce',
                //Boolean - Whether we animate the rotation of the Doughnut
                animateRotate: true,
                //Boolean - Whether we animate scaling the Doughnut from the centre
                animateScale: false,
                //Boolean - whether to make the chart responsive to window resizing
                responsive: true,
                // Boolean - whether to maintain the starting aspect ratio or not when responsive, if set to false, will take up entire container
                maintainAspectRatio: true,
                //String - A legend template
                legendTemplate: '<ul class="<%=name.toLowerCase()%>-legend"><% for (var i=0; i<segments.length; i++){%><li><span style="background-color:<%=segments[i].fillColor%>"></span><%if(segments[i].label){%><%=segments[i].label%> - <label style="color:<%=segments[i].fillColor%>"><%=segments[i].value%> Crs</label><%}%></li><%}%></ul>'
            }

            //Create pie or douhnut chart
            // You can switch between pie and douhnut using the method below.

            $('#divChartContainer').empty();
            $('#divChartContainer').append('<canvas id="pieChart" style="height:250px"></canvas>');
            var pieChartCanvas = $('#pieChart').get(0).getContext('2d')
            var pieChart = new Chart(pieChartCanvas).Pie(data, pieOptions)
            $("#pie_legend").html(pieChart.generateLegend());

        }
    });

});


