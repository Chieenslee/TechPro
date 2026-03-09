function switchLoginMode(mode) {
    const buttons = document.querySelectorAll('.mode-btn');
    const tenantField = document.getElementById('tenantIdField');
    const tenantInput = document.getElementById('tenantIdInput');
    const modeInput = document.getElementById('loginModeInput');
    const form = document.getElementById('loginForm');
    
    // Remove active class from all buttons
    buttons.forEach(btn => btn.classList.remove('active'));
    
    // Add active class to clicked button
    const activeBtn = document.querySelector(`[data-mode="${mode}"]`);
    if (activeBtn) activeBtn.classList.add('active');
    
    // Update hidden input
    if (modeInput) modeInput.value = mode;
    
    // Parse URL params to keep existing values or errors if possible (optional step)
    
    // Toggle fields based on mode
    if (mode === 'system') {
        if (tenantField) {
            tenantField.classList.add('d-none');
        }
        if (tenantInput) {
            tenantInput.required = false;
        }
    } else {
        if (tenantField) {
            tenantField.classList.remove('d-none');
            // Add animation class
            tenantField.classList.remove('animate-in');
            void tenantField.offsetWidth; // Trigger reflow
            tenantField.classList.add('animate-in');
        }
        if (tenantInput) {
            tenantInput.required = true;
        }
    }
}

function quickLogin(mode, email, tenantId = null) {
    switchLoginMode(mode);
    
    const form = document.getElementById('loginForm');
    const submitBtn = document.getElementById('submitBtn');
    const submitText = document.getElementById('submitText');
    const submitIcon = document.getElementById('submitIcon');
    const submitLoader = document.getElementById('submitLoader');
    
    // Disable all demo buttons
    const demoButtons = document.querySelectorAll('.demo-btn');
    demoButtons.forEach(btn => {
        btn.disabled = true;
    });
    
    document.getElementById('emailInput').value = email;
    document.getElementById('passwordInput').value = 'demo123';
    
    if (mode === 'store' && tenantId) {
        document.getElementById('tenantIdInput').value = tenantId;
    }
    
    // Update button state
    if (submitBtn) submitBtn.disabled = true;
    if (submitText) submitText.textContent = 'Đang xử lý...';
    if (submitIcon) submitIcon.classList.add('d-none');
    if (submitLoader) submitLoader.classList.remove('d-none');
    
    // Submit form after short delay
    setTimeout(() => {
        form.submit();
    }, 300);
}

// Handle form submission
document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('loginForm');
    const submitBtn = document.getElementById('submitBtn');
    const submitText = document.getElementById('submitText');
    const submitIcon = document.getElementById('submitIcon');
    const submitLoader = document.getElementById('submitLoader');
    
    if (form) {
        form.addEventListener('submit', function(e) {
            // Update button state
            if (submitBtn) submitBtn.disabled = true;
            if (submitText) submitText.textContent = 'Đang xử lý...';
            if (submitIcon) submitIcon.classList.add('d-none');
            if (submitLoader) submitLoader.classList.remove('d-none');
            
            // Disable demo buttons
            const demoButtons = document.querySelectorAll('.demo-btn');
            demoButtons.forEach(btn => {
                btn.disabled = true;
            });
        });
    }
});
