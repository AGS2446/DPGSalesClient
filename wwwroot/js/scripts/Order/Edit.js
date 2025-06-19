
$(document).ready(function () {
    var wonVal = $('#WonLossValue').val();
    if (wonVal.toUpperCase() == 'LOST') {

        readonly(document.getElementById('Reasons'), false);
        readonly(document.getElementById('CompName'), false);
        readonly(document.getElementById('CompititorPrice'), false);

        readonly(document.getElementById('TurnOver'), true);
        readonly(document.getElementById('TotalTonnage'), true);
        readonly(document.getElementById('GrossMargin'), true);
        readonly(document.getElementById('PODate'), true);
        readonly(document.getElementById('PONo'), true);
       
    } else {

        readonly(document.getElementById('Reasons'), true);
        readonly(document.getElementById('CompName'), true);
        readonly(document.getElementById('CompititorPrice'), true);

        readonly(document.getElementById('TurnOver'), false);
        readonly(document.getElementById('TotalTonnage'), false);
        readonly(document.getElementById('GrossMargin'), false);
        readonly(document.getElementById('PODate'), false);
        readonly(document.getElementById('PONo'), false);
    }
});

$('#PODate').datepicker({
    autoclose: true,
    format: 'd/m/yyyy'
});
//$('#DocumentCreatedDate').datepicker({
//    autoclose: true,
//    format: 'd/m/yyyy',
//     endDate: "today",
//});

$('#btnCustSelect').click(function () {
    //$('#CustomerCode').val($('.CustUl>li.custactive>a>div>span').html());
    //$('#CustomerName').val($('.CustUl>li.custactive>a>div>p').html());
    //$('#leadCustomersModal').modal('hide');


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

                $('#enqCustomersModal').modal('hide');
            } 


        }
    });

});
$('#btnCustomerSearch').click(function () {
    debugger;
    $('#initialload').show();
    $.post('customers', { strKey: $('#CustomerName').val() }).done(function (data) {

        if (data != null && data != undefined && data.length > 1) {
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
            $('#enqCustomersModal').modal('show');

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

    return false;
});


$('#Region').change(function (objEdit) {
    $("#initialload").show();
    debugger;
    $.post('GetBranchs', { strOrgId: $('#Region').val().split('#')[1], strDivision: $('#Division').val().split('#')[1]  }).done(function (data) {
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
    $.post('GetSalesOfficePlants', { strOrgId: strBrchId, strDivision: strDivId  }).done(function (data) {
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
    }).always(function () {
        $("#initialload").hide();
    });
   
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

$('#WonLossValue').change(function () {
    debugger;
 
    var wonVal = $('#WonLossValue').val();
    if (wonVal.toUpperCase() == 'LOST') {

        readonly(document.getElementById('Reasons'), false);
        readonly(document.getElementById('CompName'), false);
        readonly(document.getElementById('CompititorPrice'), false);

        readonly(document.getElementById('TurnOver'), true);
        readonly(document.getElementById('TotalTonnage'), true);
        readonly(document.getElementById('GrossMargin'), true);
        readonly(document.getElementById('PODate'), true);
        readonly(document.getElementById('PONo'), true);

    } else {

        readonly(document.getElementById('Reasons'), true);
        readonly(document.getElementById('CompName'), true);
        readonly(document.getElementById('CompititorPrice'), true);

        readonly(document.getElementById('TurnOver'), false);
        readonly(document.getElementById('TotalTonnage'), false);
        readonly(document.getElementById('GrossMargin'), false);
        readonly(document.getElementById('PODate'), false);
        readonly(document.getElementById('PONo'), false);
    }

});

$("#GrossMargin").focusout(function () {
    if (($('#WonLossValue').val() != null && $('#WonLossValue').val() != '' && $('#WonLossValue').val() == 'Win') && ($('#TurnOver').val() != null && $('#TurnOver').val() != '') && ($('#TurnOver').val() != null && $('#TurnOver').val() != '') && ($('#GrossMargin').val() != null && $('#GrossMargin').val() != '')) {
        var res = $('#TurnOver').val() - $("#GrossMargin").val();
        $("#TotalCost").val(res);
        if (res < 0) {
            AlertPopup('Turn over value should greater than Margin value');
            $("#TotalCost").val(0);
        } else
            $("#TotalCost").val(res);
    }
});
$("#TurnOver").focusout(function () {
    if (($('#WonLossValue').val() != null && $('#WonLossValue').val() != '' && $('#WonLossValue').val() == 'Win') && ($('#GrossMargin').val() != null && $('#GrossMargin').val() != '')) {
        var res = $('#TurnOver').val() - $("#GrossMargin").val();
        if (res < 0) {
            AlertPopup('Turn over value should greater than Margin value');
            $("#TotalCost").val(0);
        } else
            $("#TotalCost").val(res);

    }
});

$('#btnEditProduct').click(function () {
    debugger;

    //create
    var validator = $("#ordEditForm").validate();
    var isValid = validator.form();
    var data = $("#ordEditForm").serialize();

    if (isValid) {
        $.post('NavigateOrderItems', data).done(function (data) {
            if (data != null) {
                if (data.status == "FAILED") {
                    AlertPopup('Provide all values');
                } else {
                    debugger;
                    window.location = "OrderItems?BusinessSegment=" + data.businessSegment + "&ordId=" + data.quoteId;
                }
            }

        });

    } else {
        //  alert('failed');
    }

    return false;
});

$('#btnContractValue').click(function () {
    debugger;
    $('#txtContractValue').val($('#ContractValue').val());
    $('#contractModal').modal('show');
    $("#initialload").hide();
});

$('#btnContractValueSave').click(function () {
    debugger;
    if ($('#txtContractValue').val() != null && $('#txtContractValue').val() != "")
        $.post('../Order/UpdateContractValue', { ordId: $('#OrderID').val(), contractValue: parseFloat($('#txtContractValue').val()) }).done(function (data) {
            if (data != null) {
                if (data.status == "SUCCESS") {
                    $('#contractModal').modal('hide');
                    $("#initialload").hide();
                    //window.location = "Details?enqId=" + $('#EnquiryID').val() + "&status=" + $('#Status').val();
                     window.location = "Edit?ordId=" + $('#OrderID').val() + "&stBack=";
                    //AlertPopup("Contract Value has been updated");
                } else {
                    $("#initialload").hide();
                    $('#contractModal').modal('hide');
                    AlertPopup("Updation failed");
                }
            }
        });
});

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
//Edit
$("#ordEditForm").validate({
    errorClass: "error-class",
    rules: {
     
        Region: {
            required: true
        },
        Branch: {
            required: true
        },
        CustomerCode: {            
        },
        CustomerName: {
            required: true
        },
         City: {
            required: true,
            AlphaAndSpace:true
        },
        State: {
            required: true,
            AlphaAndSpace:true
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
            Mobile:true
        },
        Pincode: {
            required: true,
            Pincode:true
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
        ProjectName: {
            required: true
        },       
        PODate: {
            required: function () {
                if ($('#WonLossValue').val() == 'Lost')
                    return false;
                else if ($('#WonLossValue').val() == 'Win') {
                    return true;
                }

            }
        },
        PONo: {
            required: function () {
                if ($('#WonLossValue').val() == 'Lost')
                    return false;
                else if ($('#WonLossValue').val() == 'Win') {
                    return true;
                }

            }
        },
        OrderType: {
            required: true
        },
        TotalItemValue: {
            required: true,
            number:true
        },
        WonLossValue: {
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
        AssignTo: {
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
        ContractValue: {
            required: true,
            number: true
        },
        Reasons: {
            required: function () {
                if ($('#WonLossValue').val() == 'Lost')
                    return true;
                else
                    return false;
            }
        },
        CompName: {
            required: function () {
                if ($('#WonLossValue').val() == 'Lost')
                    return true;
                else
                    return false;
            }
        },
        TotalCost: {
            number: true
        },
        TotalTonnage: {
            number: true,
            required: function () {
                if ($('#WonLossValue').val() == 'Lost')
                    return false;
                else if ($('#WonLossValue').val() == 'Win') {
                    return true;
                }

            }
        },
        GrossMargin: {
            number: true,
            required: function () {
                if ($('#WonLossValue').val() == 'Lost')
                    return false;
                else if ($('#WonLossValue').val() == 'Win') {
                    return true;
                }

            }
        },
        TurnOver: {
            number: true,
            required: function () {
                if ($('#WonLossValue').val() == 'Lost')
                    return false;
                else if ($('#WonLossValue').val() == 'Win') {
                    return true;
                }

            }
        },
        CompititorPrice: {
            number: true,
            required: function () {
                if ($('#WonLossValue').val() == 'Lost')
                    return true;
                else
                    return false;
            }
        },
        Architect: {
            AlphaFullStopAndSpace: true
        },
        Consultant: {
            AlphaFullStopAndSpace: true
        }

    },
    messages: {        
        Reasons: {
            required: "Please select Reason"
        },
        CompName: {
            required: "Please select Compititor Name"
        },
        Region: {
            required: "Please select Region"
        },
        Branch: {
            required: "Please select Branch"
        },
        CustomerCode: {
            required: "Please provide Customer code"
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
        PONo: {
            required: "Please provide PO No"
        },
        PODate: {
            required: "Please provide PO Date"
        },
        ProjectName: {
            required: "Please provide Project Name"
        },
        OrderType: {
            required: "Please select OrderType"
        },
        TotalItemValue: {
            required: "Please provide Total Item Value",
            number: "Please provide numerical values"
        },
        AssignTo: {
            required: "Missing Assigned To"
        },
        WonLossValue: {
            required: "Please select Won Loss status"
        },
        CurrencyValue: {
            required: "Please select Currency Value",
            number: "Please provide numerical values"
        },
        Classification1: {
            required: "Please select Classification1"
        },
        Classification3: {
            required: "Please select Classification3"
        },
        Classification4: {
            required: "Please select Classification 4"
        },
        ContractValue: {
            required: "Please provide Contract Value",
            number: "Please provide numerical values"
        },
        TotalCost: {
            number: "Please provide numerical values"
        },
        TotalTonnage: {
            number: "Please provide numerical values",
            required: "Please provide Total Tonnage"
        },
        GrossMargin: {
            number: "Please provide numerical values",
            required: "Please provide Gross Margin"
        },
        TurnOver: {
            number: "Please provide numerical values",
            required: "Please provide Turn Over Value"
        },
        CompititorPrice: {
            number: "Please provide numerical values",
            required: "Please provide Compititor Price"
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
        } else {
            error.insertAfter(element);
        }
    },
    invalidHandler: function (event, validator) {
        // 'this' refers to the form
        $("#initialload").hide();
    }
});


