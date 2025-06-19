$('#LogonName').focusout(function () {
    debugger;
    if ($(this).val()) {
        $.post('CheckLogonName', { strKey: $(this).val() }).done(function (data) {
            if (data) {

                $("#logonname-msg").addClass("success-class").removeClass("error-class");
                $("#logonname-msg").html("Logon Name available").show().fadeOut(1600);
            }
            else {
                $("#LogonName").val('');
                $("#logonname-msg").addClass("error-class").removeClass("success-class");
                $("#logonname-msg").html("Logon Name not available, provide another Name").show().fadeOut(1600);
            }
        });
    }
    else {
        $("#logonname-msg").addClass("error-class").removeClass("success-class");
        $("#logonname-msg").html("Please provide Logon Name").show().fadeOut(1600);
    }

    return false;
});

//New
$("#frmUserCreate").validate({
    errorClass: "error-class",
    rules: {
        LogonName: {
            required: true
        },
        FirstName: {
            required: true
        },
        LastName: {
            required: true
        },
        Mobile: {
            required: true
        },
        Email: {
            required: true
        }
    },
    messages: {
        LogonName: {
            required: "Please provide Logon Name"
        },
        FirstName: {
            required: "Please provide First Name"
        },
        LastName: {
            required: "Please provide Last Name"
        },
        Mobile: {
            required: "Please provide Mobile"
        },
        Email: {
            required: "Please provide Email"
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
    },
    invalidHandler: function (event, validator) {
        // 'this' refers to the form
        $("#initialload").hide();
    }
});



//Edit
$("#frmUserEdit").validate({
    errorClass: "error-class",
    rules: {
        LogonName: {
            required: true
        },
        FirstName: {
            required: true
        },
        LastName: {
            required: true
        },
        Mobile: {
            required: true
        },
        Email: {
            required: true
        },
        Status: {
            required:true
        }

    },
    messages: {
        LogonName: {
            required: "Please provide Logon Name"
        },
        FirstName: {
            required: "Please provide First Name"
        },
        LastName: {
            required: "Please provide Last Name"
        },
        Mobile: {
            required: "Please provide Mobile"
        },
        Email: {
            required: "Please provide Email"
        },
        Status: {
            required:"Please select Status"
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
    },
    invalidHandler: function (event, validator) {
        // 'this' refers to the form
        $("#initialload").hide();
    }
});





