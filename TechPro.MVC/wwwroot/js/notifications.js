// Real-time Notifications System
let notificationConnection = null;
let unreadCount = 0;

function initNotifications() {
    // Load notifications on page load
    loadNotifications();
    
    // Realtime SignalR tạm tắt để tránh lỗi kết nối khi chưa cấu hình hub cross-origin
    // Vẫn giữ cơ chế poll định kỳ qua HTTP cho nhẹ nhàng.

    // Poll for notifications every 30 seconds
    setInterval(loadNotifications, 30000);
}

function loadNotifications(showBadge = false) {
    $.getJSON('/api/Notification', { unreadOnly: false }, function(response) {
        if (response.success) {
            unreadCount = response.unreadCount || 0;
            updateNotificationBadge(unreadCount);
            renderNotificationList(response.data || []);
            
            if (showBadge && unreadCount > 0) {
                showNotificationBadge();
            }
        }
    }).fail(function() {
        console.error('Failed to load notifications');
    });
}

function renderNotificationList(notifications) {
    const container = document.getElementById('notificationList');
    if (!container) return;
    
    if (!notifications || notifications.length === 0) {
        container.innerHTML = `
            <div class="text-center py-3 text-muted">
                <i class="bi bi-inbox fs-4 d-block mb-2"></i>
                <small>Không có thông báo</small>
            </div>
        `;
        return;
    }
    
    const html = notifications.map(notif => {
        const timeAgo = getTimeAgo(new Date(notif.createdAt));
        const iconClass = notif.type === 'error' ? 'bi-exclamation-circle text-danger' :
                         notif.type === 'warning' ? 'bi-exclamation-triangle text-warning' :
                         notif.type === 'success' ? 'bi-check-circle text-success' :
                         'bi-info-circle text-info';
        const readClass = notif.isRead ? '' : 'fw-bold';
        
        return `
            <li class="px-2 py-2 border-bottom ${notif.isRead ? '' : 'bg-light'}" style="cursor: pointer;" onclick="markAsRead('${notif.id}'); ${notif.link ? `window.location.href='${notif.link}'` : ''}">
                <div class="d-flex align-items-start gap-2">
                    <i class="bi ${iconClass} mt-1"></i>
                    <div class="flex-grow-1">
                        <div class="${readClass} small">${escapeHtml(notif.title)}</div>
                        ${notif.message ? `<div class="text-muted small">${escapeHtml(notif.message)}</div>` : ''}
                        <div class="text-muted" style="font-size: 0.75rem;">${timeAgo}</div>
                    </div>
                    ${!notif.isRead ? '<span class="badge bg-primary rounded-pill" style="font-size: 0.5rem;">Mới</span>' : ''}
                </div>
            </li>
        `;
    }).join('');
    
    container.innerHTML = html;
}

function getTimeAgo(date) {
    const now = new Date();
    const diff = Math.floor((now - date) / 1000); // seconds
    
    if (diff < 60) return 'Vừa xong';
    if (diff < 3600) return `${Math.floor(diff / 60)} phút trước`;
    if (diff < 86400) return `${Math.floor(diff / 3600)} giờ trước`;
    if (diff < 604800) return `${Math.floor(diff / 86400)} ngày trước`;
    return date.toLocaleDateString('vi-VN');
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function updateNotificationBadge(count) {
    const badge = document.getElementById('notificationBadge');
    if (badge) {
        if (count > 0) {
            badge.textContent = count > 99 ? '99+' : count;
            badge.style.display = 'inline-block';
        } else {
            badge.style.display = 'none';
        }
    }
}

function showNotificationToast(notification) {
    const type = notification.type || 'info';
    const title = notification.title || 'Thông báo mới';
    const message = notification.message || '';
    
    toastr[type](message, title, {
        timeOut: 5000,
        onclick: function() {
            if (notification.link) {
                window.location.href = notification.link;
            }
        }
    });
}

function showNotificationBadge() {
    // Animate notification badge
    const badge = document.getElementById('notificationBadge');
    if (badge) {
        badge.classList.add('animate__animated', 'animate__pulse');
        setTimeout(() => {
            badge.classList.remove('animate__animated', 'animate__pulse');
        }, 2000);
    }
}

function markAsRead(notificationId) {
    $.ajax({
        url: `/api/Notification/${notificationId}/read`,
        type: 'POST',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function() {
            loadNotifications();
        },
        error: function() {
            console.error('Failed to mark notification as read');
        }
    });
}

function markAllAsRead() {
    $.ajax({
        url: '/api/Notification/read-all',
        type: 'POST',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function() {
            loadNotifications();
            toastr.success('Đã đánh dấu tất cả là đã đọc');
        }
    });
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    initNotifications();
    
    // Load notifications when dropdown is shown
    const dropdown = document.getElementById('notificationDropdown');
    if (dropdown) {
        dropdown.addEventListener('shown.bs.dropdown', function() {
            loadNotifications();
        });
    }
});

