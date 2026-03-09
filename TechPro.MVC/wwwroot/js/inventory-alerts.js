// Inventory Alerts & Low Stock Warnings
function checkInventoryAlerts() {
    $.getJSON('/Kho/GetLowStockItems', function(response) {
        if (response.success && response.data && response.data.length > 0) {
            response.data.forEach(item => {
                showInventoryAlert(item);
            });
        }
    });
}

function showInventoryAlert(item) {
    const alertType = item.soLuongTon === 0 ? 'error' : 'warning';
    const message = item.soLuongTon === 0 
        ? `${item.tenLinhKien} đã hết hàng!`
        : `${item.tenLinhKien} sắp hết (còn ${item.soLuongTon} sản phẩm)`;
    
    toastr[alertType](message, 'Cảnh báo kho', {
        timeOut: 10000,
        onclick: function() {
            window.location.href = '/Kho?tab=inventory';
        }
    });
}

// Check alerts every 5 minutes
setInterval(checkInventoryAlerts, 300000);

// Check on page load
document.addEventListener('DOMContentLoaded', function() {
    if (window.location.pathname.includes('/QuanLy') || window.location.pathname.includes('/Kho')) {
        checkInventoryAlerts();
    }
});

