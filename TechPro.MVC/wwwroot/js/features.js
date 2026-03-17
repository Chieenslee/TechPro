// Real-time Stats Widget
function initStatsWidget() {
    setInterval(async function() {
        try {
            const response = await fetch('/Features/GetStats');
            const result = await response.json();
            if (result.success) {
                updateStatsDisplay(result.data);
            }
        } catch (error) {
            console.error('Error fetching stats:', error);
        }
    }, 30000); // Update every 30 seconds
}

function updateStatsDisplay(stats) {
    // Update stats cards if they exist
    const elements = {
        'totalTickets': stats.totalTickets,
        'completedTickets': stats.completedTickets,
        'totalStores': stats.totalStores,
        'totalParts': stats.totalParts,
        'todayTickets': stats.todayTickets
    };

    for (const [key, value] of Object.entries(elements)) {
        const el = document.getElementById(`stat-${key}`);
        if (el) {
            const currentValue = parseInt(el.textContent) || 0;
            if (currentValue !== value) {
                animateValue(el, currentValue, value, 1000);
            }
        }
    }
}

function animateValue(element, start, end, duration) {
    if (start === end || isNaN(start) || isNaN(end)) {
        element.textContent = end.toLocaleString('vi-VN');
        return;
    }
    const range = end - start;
    const increment = range / (duration / 16);
    if (increment === 0) {
        element.textContent = end.toLocaleString('vi-VN');
        return;
    }
    let current = start;

    const timer = setInterval(function() {
        current += increment;
        if ((increment > 0 && current >= end) || (increment < 0 && current <= end)) {
            current = end;
            clearInterval(timer);
        }
        element.textContent = Math.floor(current).toLocaleString('vi-VN');
    }, 16);
}

// Dark Mode Toggle
function initDarkMode() {
    const darkModeToggle = document.getElementById('darkModeToggle');
    const isDarkMode = localStorage.getItem('darkMode') === 'true';

    if (isDarkMode) {
        document.body.classList.add('dark-mode');
    }

    if (darkModeToggle) {
        darkModeToggle.addEventListener('click', function() {
            document.body.classList.toggle('dark-mode');
            localStorage.setItem('darkMode', document.body.classList.contains('dark-mode'));
        });
    }
}

// Keyboard Shortcuts
function initKeyboardShortcuts() {
    document.addEventListener('keydown', function(e) {
        // Ctrl/Cmd + K: Quick search
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            const searchInput = document.querySelector('input[type="search"], input[placeholder*="Tìm"]');
            if (searchInput) {
                searchInput.focus();
            }
        }

        // Ctrl/Cmd + /: Open chatbot
        if ((e.ctrlKey || e.metaKey) && e.key === '/') {
            e.preventDefault();
            const chatbotToggle = document.getElementById('chatbotToggle');
            if (chatbotToggle) {
                chatbotToggle.click();
            }
        }

        // Escape: Close modals/chatbot
        if (e.key === 'Escape') {
            const chatbotWindow = document.getElementById('chatbotWindow');
            if (chatbotWindow && chatbotWindow.style.display !== 'none') {
                document.getElementById('chatbotClose')?.click();
            }
        }
    });
}

// Notification Sound
function playNotificationSound(type = 'info') {
    const audio = new Audio();
    audio.volume = 0.3;
    
    switch (type) {
        case 'success':
            audio.src = 'data:audio/wav;base64,UklGRnoGAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQoGAACBhYqFbF1fdJivrJBhNjVgodDbq2EcBj+a2/LDciUFLIHO8tiJNwgZaLvt559NEAxQp+PwtmMcBjiR1/LMeSwFJHfH8N2QQAoUXrTp66hVFApGn+DyvmwhBTGH0fPTgjMGHGm+7+OZURAJR6Hh8sBrJAUwgM/z1oY5CB1ou+3nn00QDFCn4/C2YxwGOJHX8sx5LAUkd8fw3ZBACBRdtOnrqFUUCkaf4PK+bCEFMYfR89OCMwYcab7v45lREAlHoeHywGskBTCAz/PWhjkIHWi77eefTRAMUKfj8LZjHAY4kdfyzHksBSR3x/DdkEA=';
            break;
        case 'error':
            audio.src = 'data:audio/wav;base64,UklGRnoGAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQoGAACBhYqFbF1fdJivrJBhNjVgodDbq2EcBj+a2/LDciUFLIHO8tiJNwgZaLvt559NEAxQp+PwtmMcBjiR1/LMeSwFJHfH8N2QQAoUXrTp66hVFApGn+DyvmwhBTGH0fPTgjMGHGm+7+OZURAJR6Hh8sBrJAUwgM/z1oY5CB1ou+3nn00QDFCn4/C2YxwGOJHX8sx5LAUkd8fw3ZBACBRdtOnrqFUUCkaf4PK+bCEFMYfR89OCMwYcab7v45lREAlHoeHywGskBTCAz/PWhjkIHWi77eefTRAMUKfj8LZjHAY4kdfyzHksBSR3x/DdkEA=';
            break;
        default:
            return; // No sound for info
    }
    
    audio.play().catch(() => {}); // Ignore errors
}

// Initialize all features
document.addEventListener('DOMContentLoaded', function() {
    initStatsWidget();
    initDarkMode();
    initKeyboardShortcuts();
});

