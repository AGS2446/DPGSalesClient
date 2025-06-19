$("#frmSalesOrderEditItem").validate({
    errorClass: "error-class",
    rules: {
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
            digits: true
        },
        TotalValue: {
            required: true,
            number: true
        }

    },
    messages: {
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
            digits: "Please enter only digits"
        },
        TotalValue: {
            required: "Please enter TotalValue",
            number: "Please enter only numbers"
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