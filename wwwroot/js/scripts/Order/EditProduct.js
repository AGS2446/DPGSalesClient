$('#frmOrderEditProduct').validate({
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
            number: true
        },
        MarginValue: {
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

