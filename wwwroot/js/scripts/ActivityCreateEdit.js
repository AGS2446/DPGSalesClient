$('#btnCustSelect').click(function () {
    //$('#CustomerID').val($('.CustUl>li.custactive>a>div>span').html());
    //$('#Name').val($('.CustUl>li.custactive>a>div>p').html());
    //$('#exampleModal').modal('hide');

    $.post('Customers', { strKey: $('.CustUl>li.custactive>a>div>p').html() }).done(function (data) {
        debugger;
        if (data !== null && data !== undefined) {
            debugger;

            if (data.length === 1) {
                $('#CustomerID').val(data[0].customerId);
                $('#Name').val(data[0].customerName);
                $('#ContactName').val(data[0].contactperson);
                $('#Designation').val('');
                $('#EmailID').val(data[0].emailid);
                $('#MobileNumber').val(data[0].mobilenumber);              

                $('#activityCustomersModal').modal('hide');
            }


        }
    });
});

$('#btnCustomerSearch').click(function () {
    debugger;

    $.post('customers', { strKey: $('#Name').val() }).done(function (data) {
        if (data != null && data != undefined) {
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

                $('#activityCustomersModal').modal('show');

            } else if (data.length === 1) {
                $('#CustomerID').val(data[0].customerId);
                $('#Name').val(data[0].customerName);
                $('#ContactName').val(data[0].contactperson);
                $('#Designation').val('');
                $('#MobileNumber').val(data[0].mobilenumber);
                $('#EmailID').val(data[0].emailid);
            }
        }
        else {
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

$('#Name').keyup(function () { $('#CustomerID').val('') });

jQuery.validator.addMethod("AlphaComaAnd", function (value, element) {
    return this.optional(element) || /^[a-zA-Z\s\&\,]*$/i.test(value);
}, "Numarics and Special characters are not allowed, Other than '&' and ','");

jQuery.validator.addMethod("AlphaFullStopAndSpace", function (value, element) {
    return this.optional(element) || /^[a-zA-Z\s\.]*$/i.test(value);
}, "It should allow Alphabets and Some special characters (Space and '.')");

jQuery.validator.addMethod("Mobile", function (value, element) {
    return this.optional(element) || /^[0-9]+$/i.test(value);
}, "Please provide valid mobile number");



$("#ActivityNewForm").validate({
    errorClass: "error-class",
    rules: {
        CustomerID: {
           
        },
        Name: {
            required: true
        },
        ContactName: {
            required: true,
            AlphaFullStopAndSpace: true
        },
        Designation: {
            required: true,
            AlphaComaAnd:true
        },
        MobileNumber: {
            required: true,
            minlength: 10,
            maxlength: 10,
            Mobile:true
        },
        EmailID: {
            required: true,
            email:true
        },
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
        CustomerID: {
            required: "Please provide Customer Id"
        },
        Name: {
            required: "Please provide Customer name"
        },
        ContactName: {
            required: "Please provide Contact Person Name",
            AlphaFullStopAndSpace: "It should allow Alphabets and Some special characters (Space and '.')"
        },
        Designation: {
            required: "Please provide Contact Person designation",
            AlphaComaAnd: "Numarics and Special characters are not allowed, Other than '&' and ','"
        },
        MobileNumber: {
            required: "Please provide Mobile number",
            minlength: "In valid Mobile numer",
            maxlength: "In valid Mobile numer",
            Mobile:"Please provide valid mobile number"
        },
        EmailID: {
            required: "Please provide email id",
            email:"Invalid email id"
        },
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
        CustomerID: {          
        },
        Name: {
            required: true
        },
        ContactName: {
            required: true,
            AlphaFullStopAndSpace: true
        },
        Designation: {
            required: true,
            AlphaComaAnd: true
        },
        MobileNumber: {
            required: true,
            minlength: 10,
            maxlength: 10,
            Mobile: true
        },
        EmailID: {
            required: true,
            email:true
        },
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
        CustomerID: {
            required: "Please provide Customer Id"
        },
        Name: {
            required: "Please provide Customer name"
        },
        ContactName: {
            required: "Please provide Contact Person Name",
            AlphaFullStopAndSpace: "It should allow Alphabets and Some special characters (Space and '.')"
        },
        Designation: {
            required: "Please provide Contact Person designation",
            AlphaComaAnd: "Numarics and Special characters are not allowed, Other than '&' and ','"
        },
        MobileNumber: {
            required: "Please provide Mobile number",
            minlength: "Invalid Mobile numer",
            maxlength: "Invalid Mobile numer",
            Mobile: "Please provide valid mobile number"
        },
        EmailID: {
            required: "Please provide email id",
            email:"Invalid email id"
        },
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

