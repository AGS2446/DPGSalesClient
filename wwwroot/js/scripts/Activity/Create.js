$('#btnCustSelect').click(function () {
    $('#CustomerID').val($('.CustUl>li.custactive>a>div>span').html());
    $('#Name').val($('.CustUl>li.custactive>a>div>p').html());
    $('#exampleModal').modal('hide');
});

$('#btnCustomerSearch').click(function () {
    debugger;
    $('#initialload').show();
    $.post('customers', { strKey: $('#Name').val() }).done(function (data) {

        if (data.length > 1) {
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
            $('#initialload').hide();
            $('#exampleModal').modal('show');

        } else if (data.length === 1) {
            $('#CustomerID').val(data[0].customerId)
            $('#Name').val(data[0].customerName)

            $('#initialload').hide();
        } else {
            AlertPopup('No Data available');
            $('#initialload').hide();
        }
    });

    return false;
});

$('#dtVisitOn').datepicker({
    autoclose: true,
    format: 'd/m/yyyy',
    startDate: "today"
});


$("#ActivityNewForm").validate({
    errorClass: "error-class",
    rules: {
        VisitOn: {
            required: true
        },
        ObjectiveofVisit: {
            required: true
        },
        ListofDicsussion: {
            required: true
        },
        SupportRequired: {
            required: true
        }
    },
    messages: {
        VisitOn: {
            required: "Please provide visit date"
        },
        ObjectiveofVisit: {
            required: "Please select Objective Of Visit"
        },
        ListofDicsussion: {
            required: "Please provide List of discussion"
        },
        SupportRequired: {
            required: "Please provide Support Required"
        }
    },
    submitHandler: function (form) {
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


$("#ActivityEditForm").validate({
    errorClass: "error-class",
    rules: {
        VisitOn: {
            required: true
        },
        ObjectiveofVisit: {
            required: true
        },
        ListofDicsussion: {
            required: true
        },
        SupportRequired: {
            required: true
        }
    },
    messages: {
        VisitOn: {
            required: "Please provide visit date"
        },
        ObjectiveofVisit: {
            required: "Please select Objective Of Visit"
        },
        ListofDicsussion: {
            required: "Please provide List of discussion"
        },
        SupportRequired: {
            required: "Please provide Support Required"
        }
    },
    submitHandler: function (form) {
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