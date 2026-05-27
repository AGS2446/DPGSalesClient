  $('#tblAttachments').DataTable({
            'paging': false,
            'lengthChange': false,
            'searching': false,
            'ordering': false,
            'info': false,
            'autoWidth': false,
            'order': [[4, 'desc']],
             columnDefs: [
                      { type: 'date-uk', targets: 4 }
                ]

        });


$('#btnAdd').click(function () { 
    handleFileSelect();
});

function CheckFileExtension(filename) {
    debugger;
    var blRes = false;
    var lsExt = ['PDF', 'XLSX', 'TXT', 'PPT', 'PNG', 'JPG', 'JPEG', 'DOCX', 'DOC','XLX','XLS'];
    var ext = filename.split('.')[filename.split('.').length - 1];


    if (lsExt.indexOf(ext.toUpperCase()) > -1) {
        blRes = true;
    }

    return blRes;
}

function handleFileSelect() {
    debugger;
    if (!window.File || !window.FileReader || !window.FileList || !window.Blob) {

        
        AlertPopup('The File APIs are not fully supported in this browser.');
        return;
    }

    input = document.getElementById('cpFileupload');
    if (!input) {
        AlertPopup("Um, couldn't find the fileinput element.");
    }
    else if (!input.files) {
        AlertPopup("This browser doesn't seem to support the `files` property of file inputs.");
    }
    else if (!input.files[0]) {
        AlertPopup("Please select a file before clicking 'Add'");
    } else if ((input.files[0].size / 1024 / 1024) > 10) {
        AlertPopup("File size should be below 3 MB");
    }
    else {
        debugger;
        file = input.files[0];       

        if (CheckFileExtension(file.name)) {

            fr = new FileReader();
            fr.onload = receivedText;
            //fr.readAsText(file);
            fr.readAsDataURL(file);
        } else {
            AlertPopup('Unsupported file format');

        }

    }
}
function RefreshItems(arItems) {
    debugger;
    if (arItems.Files != null && arItems.Files.length > 0) {
        $('#uploadFileList').empty();
        for (var intI = 0; intI < arItems.Files.length ; intI++) {
            if (arItems.Files[intI].Name != "") {
                $('#uploadFileList').append('<li style="list-style-type: none;background-color: #0a7baf;color: white;padding: 5px;border-bottom: #89b7c1;border-bottom-style: solid;border-bottom-width: 1px;"><span>' + arItems.Files[intI].Name + '</span> <a href="#!" class="pull-right" onclick="deleteItem(this)"><i class="fa fa-times text-yellow"></i></a> </li>');
            }
        }
    }

}

function deleteItem(itm) {
    debugger;
    var itm = $($(itm).parent().children()[0]).html();

    var lsData = $('#datFile').val();
    var jsonObj = JSON.parse(lsData);
    if (jsonObj != undefined && jsonObj != null) {


        var lsExistFileName = jsonObj.Files.filter(function (x) { return x.Name == itm });

        if (lsExistFileName != undefined && lsExistFileName != null && lsExistFileName.length > 0) {
            var idx = jsonObj.Files.indexOf(lsExistFileName[0]);
            if (idx > -1) {
                jsonObj.Files[idx].Name = "";
                jsonObj.Files[idx].File = "";
                $('#datFile').val(JSON.stringify(jsonObj));
                debugger;
                RefreshItems(jsonObj);
            }

        }
    }

    return false;
}

function receivedText() {
    debugger;
    // document.getElementById('editor').appendChild(document.createTextNode(fr.result));
    $('#DataURL').val(fr.result);
    var lsData = $('#datFile').val();
    var jsonObj = JSON.parse(lsData);
    if (jsonObj != undefined && jsonObj != null) {


        var lsExistFileName = jsonObj.Files.filter(function (x) { return x.Name == file.name });

        if (lsExistFileName == undefined || lsExistFileName == null || lsExistFileName.length == 0) {
            var fileAdded = false;
            for (var intF = 0; intF < jsonObj.Files.length; intF++) {

                if (jsonObj.Files[intF].Name == "") {
                    jsonObj.Files[intF].Name = file.name;
                    jsonObj.Files[intF].File = fr.result;
                    $('#datFile').val(JSON.stringify(jsonObj));

                    $('#uploadFileList').append('<li style="list-style-type: none;background-color: #0a7baf;color: white;padding: 5px;border-bottom: #89b7c1;border-bottom-style: solid;border-bottom-width: 1px;" ><span>' + file.name + '</span> <a href="#!" class="pull-right" onclick="deleteItem(this)"><i class="fa fa-times text-yellow"></i></a> </li>')
                    fileAdded = true;
                    var input = $("#cpFileupload");
                    input.val("");
                    return;
                }


            }
            if (!fileAdded) {
                AlertPopup('Sorry you can upload 3 files only.')

            }
        } else {
            AlertPopup('Already file added');
        }
    }
}


