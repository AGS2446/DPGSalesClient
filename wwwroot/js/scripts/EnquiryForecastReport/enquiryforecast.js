//Initialize Select2 Elements
$('.select2').select2()

$('#DateFrom').datepicker({
    autoclose: true,
    format: 'dd/mm/yyyy'      
});
$('#DateTo').datepicker({
    autoclose: true,
    format: 'dd/mm/yyyy',        
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

$("#frmEnquiryForecastReport").validate({
    errorClass: "error-class",
    rules: {
        DateFrom: {
            required:true
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
        $('#Division').val($('#DivisionID').val());
        $('#Branch').val($('#BranchID').val());
        //$("#initialload").hide();
        form.submit();
    },
    errorPlacement: function (error, element) {
        if (element.parent('.input-group').length) {
            error.insertAfter(element.parent());
        }
        else {
            error.insertAfter(element);
        }
    },
    invalidHandler: function (event, validator) {
        // 'this' refers to the form
        //$("#initialload").hide();
    }
});
$("#btnSearch").click(function () {
    debugger;
    if($('#DateFrom').val()==null && $('#DateTo').val()==null)
    {
        alert("Date from and Date fields can not be empty");
        return false;
    }

    $("#Division").val($("#DivisionID").val());
    $("#Branch").val($("#BranchID").val());
    
    var validator = $("#frmEnquiryForecastReport").validate();
    var isValid = validator.form();
    var data = $("#frmEnquiryForecastReport").serialize();

    if (isValid) {
        $.post('GetEnquiryForecastReport', data).done(function (resultData) {
            debugger;

            if (resultData != null && resultData != undefined) {
                //$("#initialload").hide();
                debugger;
                var lineChartData = {
                    labels: resultData.label,
                    datasets: [
                        {
                           
                            label: 'Months',
                            fillColor: '#4DA59A',
                            strokeColor: 'rgba(60,141,188,0.8)',
                            pointColor: '#3b8bba',
                            pointStrokeColor: 'rgba(60,141,188,1)',
                            pointHighlightFill: '#fff',
                            pointHighlightStroke: 'rgba(60,141,188,1)',
                            data: resultData.month
                        }
                        
                    ]
                }

                var lineChartOptions = {
                    showTooltips: false,
                    showScale: true,
                    //Boolean - Whether grid lines are shown across the chart
                    scaleShowGridLines: false,
                    //String - Colour of the grid lines
                    scaleGridLineColor: 'rgba(0,0,0,.05)',
                    //Number - Width of the grid lines
                    scaleGridLineWidth: 1,
                    //Boolean - Whether to show horizontal lines (except X axis)
                    scaleShowHorizontalLines: true,
                    //Boolean - Whether to show vertical lines (except Y axis)
                    scaleShowVerticalLines: true,
                    //Boolean - Whether the line is curved between points
                    bezierCurve: true,
                    //Number - Tension of the bezier curve between points
                    bezierCurveTension: 0.3,
                    //Boolean - Whether to show a dot for each point
                    pointDot: false,
                    //Number - Radius of each point dot in pixels
                    pointDotRadius: 4,
                    //Number - Pixel width of point dot stroke
                    pointDotStrokeWidth: 1,
                    //Number - amount extra to add to the radius to cater for hit detection outside the drawn point
                    pointHitDetectionRadius: 20,
                    //Boolean - Whether to show a stroke for datasets
                    datasetStroke: true,
                    //Number - Pixel width of dataset stroke
                    datasetStrokeWidth: 2,
                    //Boolean - Whether to fill the dataset with a color
                    datasetFill: true,
                    //String - A legend template
                    legendTemplate: '<ul class="<%=name.toLowerCase()%>-legend"><% for (var i=0; i<datasets.length; i++){%><li><span style="background-color:<%=datasets[i].lineColor%>"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>',
                    //Boolean - whether to maintain the starting aspect ratio or not when responsive, if set to false, will take up entire container
                    maintainAspectRatio: true,
                    //Boolean - whether to make the chart responsive to window resizing
                    responsive: true,
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
                lineChartOptions.datasetFill = false
                $('#divChartContainer').empty();
                $('#divChartContainer').append('<canvas id="lineChart" style="height:250px"></canvas>');
                var lineChartCanvas = $('#lineChart').get(0).getContext('2d')
                //var barChart = new Chart(barChartCanvas).Bar(barChartData, barChartOptions)
                var lineChart = new Chart(lineChartCanvas)
                lineChart.Line(lineChartData, lineChartOptions)
            }
        });
    }
  
});


$("#btnExport").click(function(){
    debugger;
    if($('#DateFrom').val()==null && $('#DateTo').val()==null)
    {
        alert("Date from and Date fields can not be empty");
        return false;
    } else {
        $("#frmEnquiryForecastReport").submit();
    }

});


