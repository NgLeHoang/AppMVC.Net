function AutoUploadPhoto() {
    var formData = new FormData();
    var id = $("#box-photo-upload").data("id");
    formData.append("id", id);

    var totalFile = document.getElementById("selectfileupload").files.length;
    if (totalFile == 0) return;

    var fileData = document.getElementById("selectfileupload").files[0];
    formData.append("FileUpload", fileData);

    var urlUpload = "@Url.Action(UploadPhotoApi)";

    $.ajax({
        data: formData,
        cache: false,
        url: urlUpload,
        type: "POST",
        contentType: false,
        processData: false,
        success: function (data) 
        {
            LoadPhotos();
        }
    });
}
function ClickButtonUpload() {
    $("#selectfileupload").click();
}
function setClickDeletePhoto() {
    $("#box-photo-upload .photodetails span").click(function () {
        if (confirm("Bạn có chắc chắn muốn xóa ảnh?") != true) return;
        
        var spanButton = $(this);
        var id = spanButton.data("id");

        var formData = new FormData();
        formData.append("id", id);

        var urlDeletePhoto = "@Url.Action(DeletePhoto)";

        $.ajax({
            data: formData,
            cache: false,
            url: urlDeletePhoto,
            type: "POST",
            contentType: false,
            processData: false,
            success: function (data) {

                LoadPhotos();
            }
        });
    });
}
function LoadPhotos() {
    var box = $("#box-photo-upload");
    var ProductID = box.data("id");
    box.empty();

    var formData = new FormData();
    formData.append("id", ProductID);

    var urlListPhoto = "@Url.Action(ListPhotos)";

    $.ajax({
        data: formData,
        cache: false,
        url: urlListPhoto,
        type: "POST",
        contentType: false,
        processData: false,
        success: function (data) {
            data.photos.forEach(function (item) {
                var element = $(
                    '<div class="photodetails w-25 p-1">'
                    + '<img class="w-100" src="' + item.path + '">'
                    + '<span class="btn btn-danger" data-id="' + item.id + '">Xóa ảnh</span> </div>'
                );
                box.append(element);
            });
            setClickDeletePhoto();
        }
    });

}

$(document).ready(function () {
    LoadPhotos();
});