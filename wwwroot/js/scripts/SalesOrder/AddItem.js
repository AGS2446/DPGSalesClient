$('#btnMaterialNOSearch').click(function () {
    debugger;

    $.post('Materials', { strKey: $('#MaterialNo').val() }).done(function (data) {

        if (data != null && data.length > 1) {
            debugger;
            $('#ulMaterials').empty();
            for (var i = 0; i < data.length; i++) {
                $('#ulMaterials').append('<li><a href="#"><div><p>' + data[i].materialDesc + '</p><span>' + data[i].materialNo + '</span></div></a></li>')
            }

            $('#ulMaterials>li').click(function () {
                debugger;
                var vs1 = this;
                $('#ulMaterials>li').removeClass('custactive');
                $(vs1).addClass('custactive');

            });

            $('#soMaterialModal').modal('show');

        } else if (data != null && data.length == 1) {
            debugger;
            $('#MaterialNo').val(data[0].materialNo);
            $('#MaterialDesc').val(data[0].materialDesc);
            if ($('#MaterialNo').val()) {
                $('#Plant').empty();
                $('#Plant').append('<option value="">SELECT</option>')
                $.post('GetPlant', { strMaterial: $('#MaterialNo').val() }).done(function (data) {
                    debugger;
                    if (data != null && data != undefined) {

                        debugger;
                        for (var i = 0; i < data.length; i++) {
                            $('#Plant').append('<option value="' + data[i].value + '">' + data[i].text + '</option>')
                        }
                    }
                });
            }
        }
    });

    return false;
});

$('#btnMaterialSelect').click(function () {
    $.post('Materials', { strKey: $('.CustUl>li.custactive>a>div>p').html() }).done(function (data) {
        debugger;
        if (data != null && data != undefined) {
            debugger;

            if (data != null && data.length == 1) {
                $('#MaterialNo').val(data[0].materialNo);
                $('#MaterialDesc').val(data[0].materialDesc);

                $('#soMaterialModal').modal('hide');


            }
        }

    });

    if ($('#MaterialNo').val()) {
        $('#Plant').empty();
        $('#Plant').append('<option value="">SELECT</option>')
        $.post('GetPlant', { strMaterial: $('#MaterialNo').val() }).done(function (data) {
            debugger;
            if (data != null && data != undefined) {

                debugger;
                for (var i = 0; i < data.length; i++) {
                    $('#Plant').append('<option value="' + data[i].value + '">' + data[i].text + '</option>')
                }
            }
        });
    }

});

$("#frmSalesOrderAddItem").validate({
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