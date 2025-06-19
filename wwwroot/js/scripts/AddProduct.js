
jQuery.validator.addMethod("DigitsTwoDecimal", function (value, element) {
    if ($('#Division').val().split('#')[0] == "CC") {
        return this.optional(element) || /^\d{0,4}(\.\d{0,2})?$/i.test(value);
    } else {
        return true;
    };    
}, "You must include four digits with two decimal places only ex:1234.56 or 1234");

    $('#frmAddProduct').validate({
        errorClass: "error-class",
        rules: {
            BusinessSegment: {
                required: true
            },
            ProductSeg: {
                required: true
            },
            Quantity: {
                required: true,
                digits: true
            },
            TotalValue: {
                required: true,
                number: true,
                DigitsTwoDecimal: true
            },
            TotalTonnage: {
                number:true
            }
        },
        messages: {
            BusinessSegment: {
                required: "Please provide Business Segment"
            },
            ProductSeg: {
                required: "Please select Product Segment"
            },
            Quantity: {
                required: "Please provide Total Quantity",
                digits: "Please enter digits only"
            },
            TotalValue: {
                required: "Please provide Total value",
                number: "Please enter numerical values"
            },
            TotalTonnage: {
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


    