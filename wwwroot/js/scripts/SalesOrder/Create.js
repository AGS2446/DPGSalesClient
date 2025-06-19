$('#PODate').datepicker({
    autoclose: true,
    format: 'd/m/yyyy',
    startDate: "today",
});

$('#btnCustomerSearch').click(function () {
    debugger;

    $.post('Customers', { strKey: $('#CustomerName').val() }).done(function (data) {

        if (data!=null && data.length > 1) {
            $('#ulCustomers').empty();
            for (var i = 0; i < data.length; i++) {
                $('#ulCustomers').append('<li><a href="#"><div><p>' + data[i].customerName + '</p><span>' + data[i].customerId + '</span></div></a></li>')
            }

            $('#ulCustomers>li').click(function () {
                debugger;
                var vs1 = this;
                $('#ulCustomers>li').removeClass('custactive');
                $(vs1).addClass('custactive');

            });

            $('#enqCustomersModal').modal('show');

        } else if (data != null && data.length == 1) {
            $('#CustomerCode').val(data[0].customerId);
            $('#CustomerName').val(data[0].customerName);          
        }
    });

    return false;
});
$('#btnCustSelect').click(function () {
    $.post('Customers', { strKey: $('.CustUl>li.custactive>a>div>p').html() }).done(function (data) {
        debugger;
        if (data != null && data != undefined) {
            debugger;

            if (data != null && data.length == 1) {
                $('#CustomerCode').val(data[0].customerId);
                $('#CustomerName').val(data[0].customerName);               

                $('#enqCustomersModal').modal('hide');
            } 
        }
    });

});



$('#btnShipToPartySearch').click(function () {
    debugger;

    $.post('Customers', { strKey: $('#ShipToPartyName').val() }).done(function (data) {

        if (data != null && data.length > 1) {
            $('#ulShipToParty').empty();
            for (var i = 0; i < data.length; i++) {
                $('#ulShipToParty').append('<li><a href="#"><div><p>' + data[i].customerName + '</p><span>' + data[i].customerId + '</span></div></a></li>')
            }

            $('#ulShipToParty>li').click(function () {
                debugger;
                var vs1 = this;
                $('#ulShipToParty>li').removeClass('custactive');
                $(vs1).addClass('custactive');

            });

            $('#enqShipToPartyModal').modal('show');

        } else if (data != null && data.length == 1) {
            $('#ShipToPartyCode').val(data[0].customerId);
            $('#ShipToPartyName').val(data[0].customerName);
        }
    });

    return false;
});

$('#SalesOrg').change(function () {
    debugger;
    $('#DistChannel').empty();
    $('#DistChannel').append('<option value="">SELECT</option>')
    $.post('GetDistChannel', { strKey: $('#SalesOrg').val() }).done(function (data) {
        debugger;
        if (data != null && data != undefined) {

            debugger;
            for (var i = 0; i < data.length; i++) {
                $('#DistChannel').append('<option value="' + data[i].value + '">' + data[i].text + '</option>')
            }
        }
    });

});

$('#DistChannel').change(function () {
    debugger;
    $('#Division').empty();
    $('#Division').append('<option value="">SELECT</option>')
    $.post('GetDivision', { strSalesOrg: $('#SalesOrg').val(), strDistChannel: $('#DistChannel').val() }).done(function (data) {
        debugger;
        if (data != null && data != undefined) {

            debugger;
            for (var i = 0; i < data.length; i++) {
                $('#Division').append('<option value="' + data[i].value + '">' + data[i].text + '</option>')
            }
        }
    });

});

$('#OrderType').change(function () {
    debugger;
    $('#ConditionType').empty();
    $('#ConditionType').append('<option value="">SELECT</option>')
    $.post('GetConditionType', { strOrderType: $('#OrderType').val()}).done(function (data) {
        debugger;
        if (data != null && data != undefined) {

            debugger;
            for (var i = 0; i < data.length; i++) {
                $('#ConditionType').append('<option value="' + data[i].value + '">' + data[i].text + '</option>')
            }
        }
    });

});

$('#btnShipSelect').click(function () {
        $.post('Customers', { strKey: $('.CustUl>li.custactive>a>div>p').html() }).done(function (data) {
        debugger;
        if (data != null && data != undefined) {
            debugger;

            if (data.length == 1) {
                $('#ShipToPartyCode').val(data[0].customerId);
                $('#ShipToPartyName').val(data[0].customerName);

                $('#enqShipToPartyModal').modal('hide');
            }
        }
    });

});

$('#btnAddItems').click(function () {
    debugger;

    //create
    var validator = $("#soCreateForm").validate();
    var isValid = validator.form();
    var data = $("#soCreateForm").serialize();

    if (isValid) {
        $.post('NavigateSOItems', data).done(function (data) {
            if (data != null) {
                if (data.status == "FAILED") {

                } else {
                    debugger;
                    window.location = "SalesOrderItems?soId=" + data.salesORderID;
                }
            }

        });

    } else {
        //  alert('failed');
    }

    return false;
});


//Form Validation

//New
$("#soCreateForm").validate({
    errorClass: "error-class",
    rules: {       
        OrderType: {
            required: true
        },
        SalesOrg: {
            required: true
        },
        DistChannel: {
            required: true
        },
        Division: {
            required: true
        },
        PONumber: {
            required: true
        },
        PODate: {
            required: true
        },
        ConditionType: {
            required: true
        },
        CustomerCode: {
            required: true
        },
        CustomerName: {
            required: true
        },
        ShipToPartyCode: {
            required: true
        },
        ShipToPartyName: {
            required: true
        },
        MaterialNo: {
            required: true
        },
        MaterialDesc: {
            required: true
        },
        Plant: {
            required: true
        },
        Quantity: {
            required: true,
            digits:true
        },
        TotalValue: {
            required: true,
            number:true
        }        

    },
    messages: {
        OrderType: {
            required: "Please select Order Type"

        },
        SalesOrg: {
            required: "Please select Sales Organization"

        },
        DistChannel: {
            required: "Please select Distbution Channel"
        },
        Division: {
            required: "Please select Division"
        },
        PONumber: {
            required: "Please enter PO Number"
        },
        PODate: {
            required: "Please select PO Date"
        },
        ConditionType: {
            required: "Please select Condition Type"
        },
        CustomerCode: {
            required: "Please provide Customer Code"
        },
        CustomerName: {
            required: "Please provide Customer Name"
        },
        ShipToPartyCode: {
            required: "Please provide Ship To Party Code"
        },
        ShipToPartyName: {
            required: "Please provide Ship To Party Name"
        },
        MaterialNo: {
            required: "Please provide MaterialNo"
        },
        MaterialDesc: {
            required: "Please provide Material Desc"
        },
        Plant: {
            required: "Please select Plant"
        },
        Quantity: {
            required: "Please enter Quantity",
            digits:"Please enter only digits"
        },
        TotalValue: {
            required: "Please enter TotalValue",
            number:"Please enter only numbers"
        }       

    },
    submitHandler: function (form) {
        $("#initialload").hide();
        // do other things for a valid form
        form.submit();
    },
    errorPlacement: function (error, element) {
        if (element.parent('.input-group').length) {
            error.insertAfter(element.parent());
        } else {
            error.insertAfter(element);
        }
    },
    invalidHandler: function (event, validator) {
        // 'this' refers to the form
        $("#initialload").hide();
    }
});


