$.validator.addMethod("greaterThan",
    function (value, element, param) {
        var $otherElement = $(param);
        return parseFloat(value) > parseFloat($otherElement.val());
    },"Turn over value should greater than margin value");


$('#frmOrderAddProduct').validate({
    errorClass: "error-class",
    rules: {
        BusinessSegment: {

        },
        ProductSeg: {
            required: true
        },
        Quantity: {
            required: true,
            digits: true
        },
        ContractValue: {
            required: true,
            number: true
        },
        TotalTonnage: {
            required: true,
            number: true
        },
        TurnoverValue: {
            required: function () {
                if ($('#WonLossValue').val() == "Win") {
                    if (('#MarginValue').val() != null && ('#MarginValue').val() != '') {
                        return true;
                    }
                    else
                    return false;
                }                    
                else
                    return false;
            },
            number: true,
            greaterThan:"#MarginValue"
        },
        MarginValue: {
            required: function () {
                if ($('#WonLossValue').val() == "Win") 
                     return true;   
                else
                    return false;
            },
            number: true
        }
    },
    messages: {
        BusinessSegment: {
            required: "Business Segment Missing"
        },
        ProductSeg: {
            required: "Please select Product Segment"
        },
        Quantity: {
            required: "Please provide Total Quantity",
            digits: "Please enter numarical values"
        },
        ContractValue: {
            required: "Please provide Total value",
            number: "Please enter numarical values"
        },
        TotalTonnage: {
            required: "Please provide Tonnage",
            number: "Please enter numerical values"
        },
        TurnoverValue: {
            number: "Please enter numerical values"
        },
        MarginValue: {
            number: "Please enter numerical values"
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




    