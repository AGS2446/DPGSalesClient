$('#LeadMaturityDate').datepicker({
    autoclose: true,
    format: 'd/m/yyyy',
    startDate: "today"
});
//$('#DocumentCreatedDate').datepicker({
//    autoclose: true,
//    format: 'd/m/yyyy',
//    endDate: "today"

//});



//initial call
CurrencyChangeMethod();

$('#Currency').change(function () {
    debugger;
    CurrencyChangeMethod();
    //if ($('#Currency').val() != "INR")
    //{
    //    //$('#Currency').width('50%');
    //    //$('#CurrencyValue').css("visibility", "visible");
    //    $('#CurrencyValue').val('');
    //} else {
    //    //$('#Currency').width('95%');
    //    //$('#CurrencyValue').css("visibility", "hidden");
    //}   
});

function CurrencyChangeMethod() {
    if ($('#Currency').val() != "INR" && $('#Currency').val() != "") {
        //  $('#CurrencyValue').val('');
        $('#CurrencyValue').attr('readonly', false);
    } else {
        // $('#CurrencyValue').val('');
        $('#CurrencyValue').attr('readonly', true);
    }
}


$('#btnCustSelect').click(function () {
    $.post('CustomerAccount', { accId: $('.CustUl>li.custactive>a>div>span').html() }).done(function (data) {
        debugger;
        if (data != null && data != undefined) {
            debugger;

            if (data.length > 0) {
                $('#CustomerCode').val(data[0].customerId);
                $('#CustomerName').val(data[0].customerName);
                $('#City').val(data[0].city);
                $('#State').val(data[0].state);
                $('#ContactPerson').val(data[0].contactperson);
                $('#CustomerAddress').val(data[0].customeraddress);
                $('#MobileNumber').val(data[0].mobilenumber);
                $('#Pincode').val(data[0].pincode);

                $('#leadCustomersModal').modal('hide');
            }
        }
    });

});
$('#btnCustomerSearch').click(function () {
    debugger;
    $('#initialload').show();
    if ($('#Branch').val().length > 0) {
        $.post('customers', { strKey: $('#CustomerName').val(), branch: $('#Branch').val() }).done(function (data) {

            if (data != null && data != undefined && data.length > 1) {
                $('#ulCustomers').empty();
                for (var i = 0; i < data.length; i++) {
                    $('#ulCustomers').append('<li><a href="#"><div><p>' + data[i].customerName + '</p><span>' + data[i].customerId + '</span>' + " " + '<span>' + data[i].customeraddress + '</span> </div></a></li>')
                }

                $('#ulCustomers>li').click(function () {
                    debugger;
                    var vs1 = this;
                    $('#ulCustomers>li').removeClass('custactive');
                    $(vs1).addClass('custactive');

                });
                $('#initialload').hide();
                $('#leadCustomersModal').modal('show');

            } else if (data.length == 1) {
                $('#CustomerCode').val(data[0].customerId);
                $('#CustomerName').val(data[0].customerName);
                $('#City').val(data[0].city);
                $('#State').val(data[0].state);
                $('#ContactPerson').val(data[0].contactpersion);
                $('#CustomerAddress').val(data[0].customeraddress);
                $('#MobileNumber').val(data[0].mobilenumber);
                $('#Pincode').val(data[0].pincode);

                $('#initialload').hide();
            } else {
                AlertPopup('No Data available');
                $('#initialload').hide();
            }
        });
    }
    else {
        AlertPopup('Please select Branch');
        $('#initialload').hide();
    }
    return false;
});

$('#Region').change(function (objEdit) {
    $("#initialload").show();
    debugger;
    $.post('GetBranchs', { strOrgId: $('#Region').val().split('#')[1], strDivision: $('#Division').val().split('#')[1] }).done(function (data) {
        debugger;
        if (data != null && data != undefined) {
            debugger;


            if (data.plants.length == 1) {
                $('#Branch').empty();
                $('#Branch').append('<option value="' + data.plants[0].name + '#' + data.plants[0].code + '">' + data.plants[0].name + '</option>');
                FillPlants($('#Division').val().split('#')[1], data.plants[0].code);
            } else {
                $('#Branch').empty();
                $('#Branch').append('<option value="">SELECT</option>');
                for (var i = 0; i < data.plants.length; i++) {
                    $('#Branch').append('<option value="' + data.plants[i].name + '#' + data.plants[i].code + '">' + data.plants[i].name + '</option>');
                }

                $('#Plant').empty();
                $('#Plant').append('<option value="">SELECT</option>');

                $('#SalesOffice').val('');
            }


        }
    }).always(function () {
        $("#initialload").hide();
    });


});
$('#Branch').change(function (objEdit) {

    debugger;
    FillPlants($('#Division').val().split('#')[1], $('#Branch').val().split('#')[1]);
});

function FillPlants(strDivId, strBrchId) {
    $("#initialload").show();
    $.post('GetSalesOfficePlants', { strOrgId: strBrchId, strDivision: strDivId }).done(function (data) {
        debugger;
        if (data != null && data != undefined) {

            $('#SalesOffice').val(data.salesoffice);
            $('#Plant').empty();
            if (data.plants.length > 1)
                $('#Plant').append('<option value="">SELECT</option>');

            for (var i = 0; i < data.plants.length; i++) {
                $('#Plant').append('<option value="' + data.plants[i].name + '#' + data.plants[i].code + '">' + data.plants[i].name + '</option>')
            }
        }
    });
    $.post('AssignedToUsers', { strDivision: strDivId, strBranch: strBrchId }).done(function (data) {
        debugger;
        if (data != null && data != undefined) {

            $('#LeadAssignTo').empty();
            if (data.length > 1)
                $('#LeadAssignTo').append('<option value="">SELECT</option>');
            debugger;
            for (var i = 0; i < data.length; i++) {
                $('#LeadAssignTo').append('<option value="' + data[i].text + '#' + data[i].value + '">' + data[i].text + '</option>')
            }
        }
    }).always(function () {
        $("#initialload").hide();
    });
}

function ConvertStrDate(strDate) {
    if (strDate != null && strDate != "" && strDate != undefined) {
        return new Date(strDate.toString().split('/')[2], strDate.toString().split('/')[1], strDate.toString().split('/')[0])
    } else {
        return null;

    }
}

$('#CustomerSegment').change(function () {

    $("#initialload").show();
    debugger;
    $('#CustomerSubSegment').empty();
    $('#CustomerSubSegment').append('<option value="">SELECT</option>')
    $.post('SubSegment', { strKey: $('#CustomerSegment').val().split('#')[0] }).done(function (data) {


        debugger;
        if (data != null && data != undefined) {

            debugger;
            for (var i = 0; i < data.length; i++) {
                $('#CustomerSubSegment').append('<option value="' + data[i].value + '">' + data[i].text + '</option>')
            }
        }
    }).always(function () {
        $("#initialload").hide();
    });

});

$('#CustomerName').on('input', function (e) {
    $('#CustomerCode').val('');
    $('#City').val('');
    $('#State').val('');
    $('#MobileNumber').val('');
    $('#Pincode').val('');
    $('#ContactPerson').val('');
    $('#CustomerAddress').val('');
});


//jQuery.validator.addMethod("greaterStart", function (value, element, params) {
//    debugger;
//    return this.optional(element) || new Date(value) <= new Date($(params).val());
//}, 'Must be greater than start date.');
//Form Validation


jQuery.validator.addMethod("AlphaFullStopAndSpace", function (value, element) {
    return this.optional(element) || /^[a-zA-Z\s\.]*$/i.test(value);
}, "It should allow Alphabets and Some special characters (Space and '.')");

jQuery.validator.addMethod("AlphaAndSpace", function (value, element) {
    return this.optional(element) || /^[a-zA-Z\s\.]*$/i.test(value);
}, "It should allow Alphabets and special character (Space)");

jQuery.validator.addMethod("Mobile", function (value, element) {
    return this.optional(element) || /^[0-9]{10,12}$/i.test(value);
}, "Please provide valid mobile number");

jQuery.validator.addMethod("Pincode", function (value, element) {
    return this.optional(element) || /^[0-9]{6}$/i.test(value);
}, "Please provide valid Pincode");


jQuery.validator.addMethod("DigitsTwoDecimal", function (value, element) {

    if ($('#Division').val().split('#')[0] == "CC") {
        return this.optional(element) || /^\d{0,4}(\.\d{0,2})?$/i.test(value);
    } else {
        return true;
    };    
},"You can include four digits with two decimal places only ex:1234.56 or 1234");
//New
$("#frmLeadCreate").validate({
    errorClass: "error-class",
    rules: {
        Division: {
            required: true
        },
        Region: {
            required: true
        },
        Branch: {
            required: true
        },
        SalesOffice: {
            required: true
        },
        Plant: {
            required: true
        },
        CustomerCode: {
        },
        CustomerName: {
            required: true
        },
        City: {
            required: true,
            AlphaAndSpace: true
        },
        State: {
            required: true,
            AlphaAndSpace: true
        },
        ContactPerson: {
            required: true,
            AlphaFullStopAndSpace: true
        },
        CustomerAddress: {
            required: true
        },
        MobileNumber: {
            required: true,
            Mobile: true
        },
        Pincode: {
            required: true,
            Pincode: true
        },
        CustomerSegment: {
            required: true
        },
        CustomerSubSegment: {
            required: true
        },
        CustomerClassification: {
            required: true
        },
        CustomerType: {
            required: true
        },
        BusinessSegment: {
            required: true
        },
        ProdcutRequired: {
            required: true
        },
        Probability: {
            required: true
        },
        ProjectName: {
            required: true
        },
        Status: {
            required: true
        },
        ContractValue_IN_LAKHS: {
            required: true,
            number: true,
            DigitsTwoDecimal: true
            
        },
        Priority: {
            required: true
        },
        Currency: {
            required: true
        },
        CurrencyValue: {
            required: function (ele) {
                if ($('#Currency').val() == "" || $('#Currency').val() == "INR") {
                    return false;
                } else {
                    return true;
                }
            },
            number: true
        },
        LeadAssignTo: {
            required: true
        },
        LeadMaturityDate: {
            required: true
        },
        Classification1: {
            required: true
        },
        Classification3: {
            required: true
        },
        Classification4: {
            required: true
        },
        Description: {
            required: true
        },
        Architect: {
            AlphaFullStopAndSpace: true
        },
        Consultant: {
            AlphaFullStopAndSpace: true
        }

    },
    messages: {
        LeadMaturityDate: {
            required: "Please provide Lead Maturity date"
        },
        Division: {
            required: "Please select Division"
        },
        Region: {
            required: "Please select Region"
        },
        Branch: {
            required: "Please select Branch"
        },
        SalesOffice: {
            required: "Please select Branch to get Sales Office"
        },
        Plant: {
            required: "Please select Plant"
        },
        CustomerCode: {
            required: "Please select Customer"
        },
        CustomerName: {
            required: "Please provide Customer name"
        },
        CustomerSegment: {
            required: "Please select Customer Segment"
        },
        CustomerSubSegment: {
            required: "Please select Customer sub segment"
        },
        CustomerClassification: {
            required: "Please select Customer Classification"
        },
        CustomerType: {
            required: "Please select Customer Type"
        },
        BusinessSegment: {
            required: "Please select Business Segment"
        },
        ProdcutRequired: {
            required: "Please select Product Required"
        },
        Probability: {
            required: "Please select Probability"
        },
        ProjectName: {
            required: "Please provide Project Name"
        },
        Status: {
            required: "Please provide status"
        },
        ContractValue_IN_LAKHS: {
            required: "Please provide Contract Value",
            number: "Please provide numerical values",           

        },
        Priority: {
            required: "Please select priority"
        },
        Currency: {
            required: "Please select Currency"
        },
        CurrencyValue: {
            required: "Please select Currency Value",
            number: "Please provide numerical values"
        },
        LeadAssignTo: {
            required: "Please select Lead Assign To user"
        },

        Classification1: {
            required: "Please select Classification 1"
        },
        Classification3: {
            required: "Please select Classification 3"
        },
        Classification4: {
            required: "Please select Classification 4"
        },

        Description: {
            required: "Please provide Description"
        },
        Architect: {
            AlphaFullStopAndSpace: "It should allow Alphabets, and Some special characters (Space and '.')"
        },
        Consultant: {
            AlphaFullStopAndSpace: "It should allow Alphabets, and Some special characters (Space and '.')"
        }

    },
    submitHandler: function (form) {
        // do other things for a valid form
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

//Edit
$("#frmLeadEdit").validate({
    errorClass: "error-class",
    rules: {
        Division: {
            required: true
        },
        Region: {
            required: true
        },
        Branch: {
            required: true
        },
        SalesOffice: {
            required: true
        },
        Plant: {
            required: true
        },
        CustomerCode: {

        },
        CustomerName: {
            required: true
        },
        City: {
            required: true,
            AlphaAndSpace: true
        },
        State: {
            required: true,
            AlphaAndSpace: true
        },
        ContactPerson: {
            required: true,
            AlphaFullStopAndSpace: true
        },
        CustomerAddress: {
            required: true
        },
        MobileNumber: {
            required: true,
            Mobile: true
        },
        Pincode: {
            required: true,
            Pincode: true
        },
        CustomerSegment: {
            required: true
        },
        CustomerSubSegment: {
            required: true
        },
        CustomerClassification: {
            required: true
        },
        CustomerType: {
            required: true
        },
        BusinessSegment: {
            required: true
        },
        ProdcutRequired: {
            required: true
        },
        Probability: {
            required: true
        },
        ProjectName: {
            required: true
        },
        Status: {
            required: true
        },
        ContractValue_IN_LAKHS: {
            required: true,
            number: true,
            DigitsTwoDecimal: true
        },
        Priority: {
            required: true
        },
        Currency: {
            required: true
        },
        CurrencyValue: {
            required: function (ele) {
                if ($('#Currency').val() == "" || $('#Currency').val() == "INR") {
                    return false;
                } else {
                    return true;
                }
            },
            number: true
        },
        LeadAssignTo: {
            required: true
        },
        LeadMaturityDate: {
            required: true
        },
        Classification1: {
            required: true
        },
        Classification3: {
            required: true
        },
        Classification4: {
            required: true
        },
        Description: {
            required: true
        },
        Architect: {
            AlphaFullStopAndSpace: true
        },
        Consultant: {
            AlphaFullStopAndSpace: true
        }

    },
    messages: {
        LeadMaturityDate: {
            required: "Please provide Lead Maturity date"

        },

        Division: {
            required: "Please select Division"
        },
        Region: {
            required: "Please select Region"
        },
        Branch: {
            required: "Please select Branch"
        },
        SalesOffice: {
            required: "Please select Branch to get Sales Office"
        },
        Plant: {
            required: "Please select Plant"
        },
        CustomerCode: {
            required: "Please select Customer"
        },
        CustomerName: {
            required: "Please provide Customer name"
        },
        CustomerSegment: {
            required: "Please select Customer Segment"
        },
        CustomerSubSegment: {
            required: "Please select Customer sub segment"
        },
        CustomerClassification: {
            required: "Please select Customer Classification"
        },
        CustomerType: {
            required: "Please select Customer Type"
        },
        BusinessSegment: {
            required: "Please select Business Segment"
        },
        ProdcutRequired: {
            required: "Please select Product Required"
        },
        Probability: {
            required: "Please select Probability"
        },
        ProjectName: {
            required: "Please provide Project Name"
        },
        Status: {
            required: "Please provide status"
        },
        ContractValue_IN_LAKHS: {
            required: "Please provide Contract Value",
            number: "Please provide numerical values"
        },
        Priority: {
            required: "Please select priority"
        },
        Currency: {
            required: "Please select Currency"
        },
        CurrencyValue: {
            required: "Please select Currency Value",
            number: "Please provide numerical values"
        },
        LeadAssignTo: {
            required: "Please select Lead Assign To user"
        },

        Classification1: {
            required: "Please select Classification 1"
        },
        Classification3: {
            required: "Please select Classification 3"
        },
        Classification4: {
            required: "Please select Classification 4"
        },
        Description: {
            required: "Please provide Description"
        },
        Architect: {
            AlphaFullStopAndDot: "It should allow Alphabets, and Some special characters (Space and '.')"
        },
        Consultant: {
            AlphaFullStopAndDot: "It should allow Alphabets, and Some special characters (Space and '.')"
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





