// Reception Page JavaScript - Tách riêng từ View
// Chỉ xử lý UI interactions, không xử lý dữ liệu

document.addEventListener('DOMContentLoaded', function() {
    // Initialize Lucide icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
    
    // Search form submission
    const searchForm = document.getElementById('searchForm');
    if (searchForm) {
        searchForm.addEventListener('submit', function(e) {
            // Form will submit normally, no need to prevent default
        });
    }
    
    // Real-time search (optional - debounced)
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        let searchTimeout;
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            // Could implement live search here if needed
        });
    }
    
    // Check-in modal form submission
    const checkInForm = document.getElementById('checkInForm');
    if (checkInForm) {
        checkInForm.addEventListener('submit', function(e) {
            // Form will submit via POST, backend handles data
            const submitBtn = checkInForm.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Đang lưu...';
            }
        });
    }
    
    // Serial number warranty check (if implemented)
    const serialNumberInput = document.getElementById('serialNumber');
    if (serialNumberInput) {
        serialNumberInput.addEventListener('blur', function() {
            const serial = this.value.trim();
            if (serial.length >= 5) {
                checkWarrantyStatus(serial);
            }
        });
    }
});

// Check warranty status (AJAX call to backend)
async function checkWarrantyStatus(serial) {
    const warrantyInfo = document.getElementById('warrantyInfo');
    if (!warrantyInfo) return;
    
    warrantyInfo.innerHTML = '<span class="text-info small"><i class="bi bi-hourglass-split me-1"></i>Đang kiểm tra...</span>';
    
    try {
        const response = await fetch(`/Home/Search?query=${encodeURIComponent(serial)}&mode=warranty`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || '',
                'Content-Type': 'application/x-www-form-urlencoded'
            }
        });
        
        if (response.ok) {
            const data = await response.json();
            if (data.isValid) {
                warrantyInfo.innerHTML = '<span class="text-success small"><i class="bi bi-check-circle me-1"></i>✅ Còn bảo hành</span>';
                // Auto-fill device model if available
                const deviceModelInput = document.getElementById('deviceModel');
                if (deviceModelInput && data.model) {
                    deviceModelInput.value = data.model;
                }
            } else {
                warrantyInfo.innerHTML = '<span class="text-warning small"><i class="bi bi-exclamation-triangle me-1"></i>⚠️ Hết bảo hành</span>';
            }
        } else {
            warrantyInfo.innerHTML = '';
        }
    } catch (error) {
        console.error('Warranty check error:', error);
        warrantyInfo.innerHTML = '';
    }
}

// Table row click navigation
document.querySelectorAll('tbody tr[onclick]').forEach(row => {
    row.style.cursor = 'pointer';
});
