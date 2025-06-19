$('.trg-remove-role').click(function () {
    debugger;
    $('#RoleID').val($(this).val());
    $('#removeRoleModal').modal('show');
});

$('#btnRemoveRole').click(function () {
    debugger;
    $.post('../RemoveRole', { strUserID: $('#UserID').val(), strRoleID: $('#RoleID').val() }).done(function (data) {
        debugger;

        if (data) {
            window.location = '../Details/' + $('#UserID').val();
        }
        else {
            $('#removeRoleModal').modal('hide');
            $("#remove-role-error-msg").addClass("success-class").removeClass("error-class");
            $("#remove-role-error-msg").html("Unable to to Remove Role: " + $('#RoleID').val()).show().fadeOut(1600);

        }
    });
});

$('.trg-remove-position').click(function () {
    debugger;
    $('#IDs').val($(this).val());
    $('#removePositionModal').modal('show');
});

$('#btnRemovePosition').click(function () {
    debugger;
    $.post('../RemovePosition', { strUserID: $('#UserID').val(), strIDs: $('#IDs').val() }).done(function (data) {
        debugger;

        if (data) {
            window.location = '../Details/' + $('#UserID').val();
        }
        else {
            $('#removePositionModal').modal('hide');
            $("#remove-position-error-msg").addClass("success-class").removeClass("error-class");
            $("#remove-position-error-msg").html("Unable to to Remove Role: " + $('#IDs').val()).show().fadeOut(1600);

        }
    });
});

$('.trg-remove-reporting').click(function () {
    debugger;
    $('#ReportingID').val($(this).val());
    $('#removeReportingModal').modal('show');
});

$('#btnRemoveReporting').click(function () {
    debugger;
    $.post('../RemoveReporting', { strUserID: $('#UserID').val(), strReportingID: $('#ReportingID').val() }).done(function (data) {
        debugger;

        if (data) {
            window.location = '../Details/' + $('#UserID').val();
        }
        else {
            $('#removeReportingModal').modal('hide');
            $("#remove-reporting-error-msg").addClass("success-class").removeClass("error-class");
            $("#remove-reporting-error-msg").html("Unable to to Remove Role: " + $('#ReportingID').val()).show().fadeOut(1600);

        }
    });
});