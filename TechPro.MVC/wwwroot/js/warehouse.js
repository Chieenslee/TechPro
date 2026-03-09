// Warehouse Page JavaScript

function duyetYeuCau(id) {
    if (!confirm('Xác nhận duyệt và xuất kho linh kiện này?')) return;
    
    $.ajax({
        url: '/Kho/DuyetYeuCau',
        type: 'POST',
        data: { id: id },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                toastr.success(response.message);
                setTimeout(() => location.reload(), 1000);
            } else {
                toastr.error(response.message);
            }
        },
        error: function() {
            toastr.error('Có lỗi xảy ra.');
        }
    });
}

function tuChoiYeuCau(id) {
    if (!confirm('Xác nhận từ chối yêu cầu này?')) return;
    
    $.ajax({
        url: '/Kho/TuChoiYeuCau',
        type: 'POST',
        data: { id: id },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                toastr.success(response.message);
                setTimeout(() => location.reload(), 1000);
            } else {
                toastr.error(response.message);
            }
        },
        error: function() {
            toastr.error('Có lỗi xảy ra.');
        }
    });
}

function xacNhanTraXac(id) {
    if (!confirm('Xác nhận đã nhận xác linh kiện này?')) return;
    
    $.ajax({
        url: '/Kho/XacNhanTraXac',
        type: 'POST',
        data: { id: id },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                toastr.success(response.message);
                setTimeout(() => location.reload(), 1000);
            } else {
                toastr.error(response.message);
            }
        },
        error: function() {
            toastr.error('Có lỗi xảy ra.');
        }
    });
}

