
$('#RoleID').change(function () {
    debugger;
    $("#initialload").show();
    debugger;
    $('#OrgID').empty();
    $('#OrgID').append('<option value=""> ---Select--- </option>')

    $.post('../OrganizationByRole', { user: $('#UserID').val(), role: $('#RoleID').val().split('#')[1], client: $('#ClientID').val().split('#')[1], lob: $('#LOBID').val().split('#')[1] }).done(function (data) {
        debugger;
        if (data != null && data != undefined) {
            debugger;
            for (var i = 0; i < data.length; i++) {
                $('#OrgID').append('<option value="' + data[i].value + '">' + data[i].text + '</option>')
            }
        }
    }).always(function () {
        $("#initialload").hide();
    });

});


//New
$("#formAssignPosition").validate({
    errorClass: "error-class",
    rules: {
        ClientID: {
            required: true
        },
        LOBID: {
            required: true
        },
        OrgID: {
            required: true
        },
        RoleID: {
            required: true
        }
    },
    messages: {
        ClientID: {
            required: "Please select Client"
        },
        LOBID: {
            required: "Please select Line of Business"
        },
        OrgID: {
            required: "Please select Organization"
        },
        RoleID: {
            required: "Please select Role"
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





