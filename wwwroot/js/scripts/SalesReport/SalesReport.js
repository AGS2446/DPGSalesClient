
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

$('.select2').select2()

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

    showTooltips: true,
    tooltipTemplate: "<%= label %> (<%=Math.round(circumference / 6.283 * 100) %> %)",
    //tooltipTemplate: " <%=label%>: <%= numeral(value).format('($00[.]00)') %> - <%= numeral(circumference / 6.283).format('(0[.][00]%)') %>" ,

    //String - A legend template
    legendTemplate: '<ul class="<%=name.toLowerCase()%>-legend"><% for (var i=0; i<segments.length; i++){%><li> <button type="submit" class="btn btn-link" value="<%=segments[i].label%>" name="legendexport" id="legendexport"><span style="background-color:<%=segments[i].fillColor%>"></span><%if(segments[i].label){%><%=segments[i].label%> - <label style="color:<%=segments[i].fillColor%>"><%=segments[i].value%> Crs</label><%}%> </button></li><%}%></ul>'
}


$.post('GetSalesReport').done(function (data) {
    debugger;
    if (data != null && data != undefined) {

        //Create pie or douhnut chart
        // You can switch between pie and douhnut using the method below.

        $('#divChartContainer').empty();
        $('#divChartContainer').append('<canvas id="pieChart" style="height:250px"></canvas>');
        var pieChartCanvas = $('#pieChart').get(0).getContext('2d')
        var pieChart = new Chart(pieChartCanvas).Pie(data, pieOptions)
        $("#pie_legend").html(pieChart.generateLegend());

    }
    $("#initialload").hide();
});

$("#filter").click(function () {
    debugger;
    $.post('GetSalesReport', { dateFrom: $('#DateFrom').val(),dateTo:$('#DateTo').val(), division: $('#division').val().toString(), branch: $('#branch').val().toString() }).done(function (data) {
        debugger;
        $("#initialload").hide();
        if (data != null && data != undefined) {          
            
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
