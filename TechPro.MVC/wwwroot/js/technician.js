/**
 * TechPro - Technician Kanban Board v2
 * Features: Drag & Drop, Quick Filter, Live Timer, Real-time Notifications
 */

// ─── CONSTANTS ────────────────────────────────────────────────────────────────
const CSRF_TOKEN = () => document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
const STATUS_LABELS = {
    pending:       { label: 'Chờ kiểm tra',   color: '#f59e0b' },
    repairing:     { label: 'Đang sửa',        color: '#3b82f6' },
    waiting_parts: { label: 'Chờ linh kiện',  color: '#ef4444' },
    done:          { label: 'Hoàn thành',       color: '#22c55e' }
};

let signalRConnection = null;
let draggedCard       = null;
let dragSourceColumn  = null;
let liveTimerInterval = null;

// ─── INIT ──────────────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    if (typeof lucide !== 'undefined') lucide.createIcons();

    initDragAndDrop();
    initFilterBar();
    initLiveTimers();
    initSignalR();
});

// ══════════════════════════════════════════════════════════════════════════════
// 1. DRAG & DROP (HTML5 native API)
// ══════════════════════════════════════════════════════════════════════════════
function initDragAndDrop() {
    const columns  = document.querySelectorAll('.kanban-column .card-body');
    const cards    = document.querySelectorAll('.ticket-card');

    // Make cards draggable
    cards.forEach(card => {
        card.setAttribute('draggable', 'true');

        card.addEventListener('dragstart', (e) => {
            draggedCard      = card;
            dragSourceColumn = card.closest('.kanban-column');
            card.classList.add('dragging');
            e.dataTransfer.effectAllowed = 'move';
            e.dataTransfer.setData('text/plain', card.dataset.ticketId);
        });

        card.addEventListener('dragend', () => {
            draggedCard = null;
            card.classList.remove('dragging');
            document.querySelectorAll('.kanban-column').forEach(col => {
                col.querySelector('.card-body')?.classList.remove('drag-over');
            });
        });
    });

    // Make columns accept drops
    columns.forEach(colBody => {
        colBody.addEventListener('dragover', (e) => {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';
            colBody.classList.add('drag-over');
        });

        colBody.addEventListener('dragleave', (e) => {
            if (!colBody.contains(e.relatedTarget)) {
                colBody.classList.remove('drag-over');
            }
        });

        colBody.addEventListener('drop', async (e) => {
            e.preventDefault();
            colBody.classList.remove('drag-over');
            if (!draggedCard) return;

            const targetColumn = colBody.closest('.kanban-column');
            if (targetColumn === dragSourceColumn) return;

            const ticketId    = draggedCard.dataset.ticketId;
            const newStatus   = targetColumn.dataset.status;
            const oldStatus   = dragSourceColumn.dataset.status;

            if (!newStatus || !ticketId) return;

            // Optimistic UI — move card immediately
            colBody.insertBefore(draggedCard, colBody.querySelector('.empty-state'));
            updateColumnCount(targetColumn, +1);
            updateColumnCount(dragSourceColumn, -1);
            hideEmptyState(targetColumn);
            checkEmptyState(dragSourceColumn);

            // API call
            const ok = await changeTicketStatus(ticketId, newStatus);
            if (!ok) {
                // Rollback
                toastr.error('Không thể cập nhật trạng thái. Đang hoàn tác...');
                const srcBody = dragSourceColumn.querySelector('.card-body');
                srcBody.insertBefore(draggedCard, srcBody.querySelector('.empty-state'));
                updateColumnCount(targetColumn, -1);
                updateColumnCount(dragSourceColumn, +1);
                hideEmptyState(dragSourceColumn);
                checkEmptyState(targetColumn);
            } else {
                // Update badge on card
                updateCardStatusBadge(draggedCard, newStatus);
                showStatusChangedToast(ticketId, oldStatus, newStatus);
            }
        });
    });
}

async function changeTicketStatus(ticketId, newStatus) {
    try {
        const res = await fetch('/KyThuat/CapNhatTrangThai', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': CSRF_TOKEN()
            },
            body: `id=${encodeURIComponent(ticketId)}&trangThai=${encodeURIComponent(newStatus)}`
        });
        const data = await res.json();
        return data.success === true;
    } catch (err) {
        console.error('Status change failed:', err);
        return false;
    }
}

function updateColumnCount(column, delta) {
    const badge = column.querySelector('.column-count');
    if (badge) {
        const cur = parseInt(badge.textContent, 10) || 0;
        badge.textContent = Math.max(0, cur + delta);
    }
}

function hideEmptyState(column) {
    const empty = column.querySelector('.empty-state');
    if (empty) empty.style.display = 'none';
}

function checkEmptyState(column) {
    const cards = column.querySelectorAll('.ticket-card');
    const empty = column.querySelector('.empty-state');
    if (empty) empty.style.display = cards.length === 0 ? 'flex' : 'none';
}

function updateCardStatusBadge(card, newStatus) {
    // no visible status badge on card currently — placeholder for future
}

function showStatusChangedToast(ticketId, from, to) {
    const fromLabel = STATUS_LABELS[from]?.label ?? from;
    const toLabel   = STATUS_LABELS[to]?.label ?? to;
    toastr.success(
        `<strong>${ticketId}</strong><br>${fromLabel} → <strong>${toLabel}</strong>`,
        'Cập nhật tiến độ',
        { timeOut: 3000, escapeHtml: false }
    );
}

// ══════════════════════════════════════════════════════════════════════════════
// 2. QUICK FILTER BAR
// ══════════════════════════════════════════════════════════════════════════════
function initFilterBar() {
    // Create filter bar and insert before kanban board
    const wrapper = document.querySelector('.kanban-board-wrapper');
    if (!wrapper) return;

    const filterBarHtml = `
    <div id="techFilterBar" class="mb-3 d-flex flex-wrap align-items-center gap-2">
        <!-- Search -->
        <div class="input-group input-group-sm" style="max-width: 260px;">
            <span class="input-group-text bg-white border-end-0">
                <i class="bi bi-search text-muted"></i>
            </span>
            <input type="text" id="filterSearch" class="form-control border-start-0 shadow-none"
                   placeholder="Tìm tên khách, máy, mã phiếu..." oninput="applyFilters()">
        </div>

        <!-- Mine only toggle -->
        <div class="form-check form-check-sm form-switch mb-0 ms-1">
            <input class="form-check-input" type="checkbox" id="filterMine" onchange="applyFilters()">
            <label class="form-check-label small fw-medium" for="filterMine">Việc của tôi</label>
        </div>

        <!-- Status pills (click to hide/show column) -->
        <div class="d-flex gap-1 ms-auto">
            <button class="filter-pill active" data-col="pending"       onclick="toggleColumn(this, 'pending')"       style="--pill-color:#f59e0b">Chờ kiểm tra</button>
            <button class="filter-pill active" data-col="repairing"     onclick="toggleColumn(this, 'repairing')"     style="--pill-color:#3b82f6">Đang sửa</button>
            <button class="filter-pill active" data-col="waiting_parts" onclick="toggleColumn(this, 'waiting_parts')" style="--pill-color:#ef4444">Chờ linh kiện</button>
            <button class="filter-pill active" data-col="done"          onclick="toggleColumn(this, 'done')"          style="--pill-color:#22c55e">Hoàn thành</button>
        </div>

        <!-- Clear button -->
        <button class="btn btn-sm btn-outline-secondary" onclick="clearFilters()">
            <i class="bi bi-x-lg me-1"></i>Xóa lọc
        </button>
    </div>`;

    wrapper.insertAdjacentHTML('beforebegin', filterBarHtml);

    // Add filter bar CSS
    const style = document.createElement('style');
    style.textContent = `
        .filter-pill {
            font-size: 0.75rem; font-weight: 600; border-radius: 999px;
            border: 2px solid var(--pill-color); color: var(--pill-color);
            background: transparent; padding: 3px 12px; cursor: pointer;
            transition: all 0.2s;
        }
        .filter-pill.active {
            background: var(--pill-color); color: #fff;
        }
        .filter-pill:hover { opacity: 0.85; }

        .kanban-column .drag-over {
            background: rgba(59,130,246,0.06) !important;
            border: 2px dashed #3b82f6 !important;
            border-radius: 12px;
        }
        .ticket-card.dragging {
            opacity: 0.45;
            transform: rotate(2deg) scale(0.97);
            box-shadow: 0 20px 40px rgba(0,0,0,0.18) !important;
        }
        .ticket-card[draggable="true"] { cursor: grab; }
        .ticket-card[draggable="true"]:active { cursor: grabbing; }

        /* Live timer badge */
        .live-timer {
            font-size: 0.72rem; font-weight: 700; letter-spacing: 0.5px;
            padding: 2px 8px; border-radius: 999px;
        }
        .live-timer.urgent  { background: #fee2e2; color: #dc2626; }
        .live-timer.warning { background: #fef3c7; color: #d97706; }
        .live-timer.normal  { background: #f1f5f9; color: #475569; }
    `;
    document.head.appendChild(style);
}

function applyFilters() {
    const searchQ  = (document.getElementById('filterSearch')?.value || '').toLowerCase().trim();
    const mineOnly = document.getElementById('filterMine')?.checked;
    const myUserId = document.body.dataset.userId || '';

    document.querySelectorAll('.ticket-card').forEach(card => {
        const text      = card.textContent.toLowerCase();
        const assigneeId = card.dataset.assigneeId || '';

        const matchSearch = !searchQ || text.includes(searchQ);
        const matchMine   = !mineOnly || assigneeId === myUserId || myUserId === '';

        card.style.display = (matchSearch && matchMine) ? '' : 'none';
    });
}

function toggleColumn(btn, colId) {
    btn.classList.toggle('active');
    const col = document.querySelector(`.kanban-column[data-status="${colId}"]`);
    if (col) col.style.display = btn.classList.contains('active') ? '' : 'none';
}

function clearFilters() {
    const searchInput = document.getElementById('filterSearch');
    if (searchInput) searchInput.value = '';
    const mineCheck = document.getElementById('filterMine');
    if (mineCheck) mineCheck.checked = false;

    document.querySelectorAll('.kanban-column').forEach(col => col.style.display = '');
    document.querySelectorAll('.filter-pill').forEach(btn => btn.classList.add('active'));
    applyFilters();
}

// ══════════════════════════════════════════════════════════════════════════════
// 3. LIVE TIMER — shows elapsed time per card
// ══════════════════════════════════════════════════════════════════════════════
function initLiveTimers() {
    const cards = document.querySelectorAll('.ticket-card[data-received-at]');
    if (!cards.length) return;

    // Render badge HTML into each card footer
    cards.forEach(card => {
        const footer = card.querySelector('.border-top');
        if (!footer) return;
        const timerSpan = document.createElement('span');
        timerSpan.className = 'live-timer normal ms-auto text-nowrap flex-shrink-0';
        timerSpan.dataset.receivedAt = card.dataset.receivedAt;
        footer.appendChild(timerSpan);
    });

    // Tick every second
    tickTimers();
    liveTimerInterval = setInterval(tickTimers, 1000);
}

function tickTimers() {
    const now = Date.now();
    document.querySelectorAll('.live-timer').forEach(el => {
        const received = new Date(el.dataset.receivedAt).getTime();
        if (isNaN(received)) return;
        const elapsed = Math.floor((now - received) / 1000);
        const h = Math.floor(elapsed / 3600);
        const m = Math.floor((elapsed % 3600) / 60);
        const s = elapsed % 60;

        el.textContent = h > 0
            ? `${h}g ${String(m).padStart(2,'0')}p`
            : `${String(m).padStart(2,'0')}:${String(s).padStart(2,'0')}`;

        // Color urgency
        el.className = 'live-timer ms-auto text-nowrap flex-shrink-0 ' + (
            elapsed > 14400 ? 'urgent' :
            elapsed > 3600  ? 'warning' : 'normal'
        );
    });
}

// ══════════════════════════════════════════════════════════════════════════════
// 4. REAL-TIME SignalR PUSH NOTIFICATIONS (upgraded — no full page reload)
// ══════════════════════════════════════════════════════════════════════════════
function initSignalR() {
    if (typeof signalR === 'undefined') return;

    signalRConnection = new signalR.HubConnectionBuilder()
        .withUrl('/ticketHub')
        .withAutomaticReconnect([0, 2000, 5000, 10000])
        .build();

    // New ticket pushed from reception
    signalRConnection.on('TicketCreated', function (ticketId, deviceName, description) {
        showPushNotification({
            type: 'info',
            icon: 'bi-ticket-detailed',
            title: `📋 Phiếu mới: ${ticketId}`,
            message: `${deviceName || 'Thiết bị'} — ${description || ''}`,
            link: `/KyThuat/ChiTiet/${ticketId}`,
            sound: true
        });
        // Inject new card into "pending" column without reload
        injectNewCardIntoKanban(ticketId, deviceName, description);
    });

    // Status changed on another device
    signalRConnection.on('TicketStatusChanged', function (ticketId, oldStatus, newStatus, changedByName) {
        const toLabel = STATUS_LABELS[newStatus]?.label ?? newStatus;
        showPushNotification({
            type: 'warning',
            icon: 'bi-arrow-left-right',
            title: `🔄 ${ticketId} đổi trạng thái`,
            message: `→ ${toLabel}${changedByName ? ` bởi ${changedByName}` : ''}`,
            link: `/KyThuat/ChiTiet/${ticketId}`
        });
    });

    // Parts request approved
    signalRConnection.on('PartsApproved', function (ticketId, partName) {
        showPushNotification({
            type: 'success',
            icon: 'bi-box-seam',
            title: `✅ Linh kiện đã duyệt`,
            message: `${partName} cho phiếu ${ticketId}`,
            link: `/KyThuat/ChiTiet/${ticketId}`
        });
        // Move card from waiting_parts → repairing
        const card = document.querySelector(`.ticket-card[data-ticket-id="${ticketId}"]`);
        if (card) {
            const repairingCol = document.querySelector('.kanban-column[data-status="repairing"] .card-body');
            if (repairingCol) repairingCol.insertBefore(card, repairingCol.querySelector('.empty-state'));
        }
    });

    signalRConnection.onreconnected(() => {
        toastr.info('Đã kết nối lại hệ thống thời gian thực.', '', { timeOut: 2000 });
    });

    signalRConnection.onclose(() => {
        console.warn('[SignalR] Connection closed.');
    });

    signalRConnection.start()
        .then(() => console.info('[SignalR] Connected to TicketHub'))
        .catch(err => console.error('[SignalR] Connection failed:', err));
}

function showPushNotification({ type = 'info', icon = 'bi-bell', title, message, link, sound = false }) {
    // Toastr toast
    const toastFn = toastr[type] || toastr.info;
    toastFn(
        `${message}${link ? `<br><a href="${link}" class="small text-white-50">Xem chi tiết →</a>` : ''}`,
        title,
        {
            timeOut: 6000,
            extendedTimeOut: 3000,
            escapeHtml: false,
            onclick: link ? () => window.location.href = link : null
        }
    );

    // Browser Notification API (if permitted)
    if (Notification.permission === 'granted') {
        const notif = new Notification(title, {
            body: message,
            icon: '/favicon.ico'
        });
        if (link) notif.onclick = () => window.open(link);
    }

    // Sound ping
    if (sound) playNotificationSound();

    // Update header bell badge count
    const badge = document.getElementById('notificationBadge');
    if (badge) {
        const cur = parseInt(badge.textContent, 10) || 0;
        badge.textContent = cur + 1;
        badge.style.display = 'inline-block';
        badge.classList.add('pulse-badge');
        setTimeout(() => badge.classList.remove('pulse-badge'), 1500);
    }
}

function playNotificationSound() {
    try {
        const ctx = new (window.AudioContext || window.webkitAudioContext)();
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.connect(gain);
        gain.connect(ctx.destination);
        osc.type = 'sine';
        osc.frequency.setValueAtTime(880, ctx.currentTime);
        gain.gain.setValueAtTime(0.15, ctx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.4);
        osc.start(ctx.currentTime);
        osc.stop(ctx.currentTime + 0.4);
    } catch (_) {}
}

function injectNewCardIntoKanban(ticketId, deviceName, description) {
    const col = document.querySelector('.kanban-column[data-status="pending"] .card-body');
    if (!col) return;

    const card = document.createElement('div');
    card.className = 'card mb-3 ticket-card border-0 rounded-3 shadow-sm ticket-hover new-card-flash';
    card.dataset.ticketId = ticketId;
    card.dataset.receivedAt = new Date().toISOString();
    card.setAttribute('draggable', 'true');
    card.style.cursor = 'pointer';
    card.onclick = () => window.location.href = `/KyThuat/ChiTiet/${ticketId}`;
    card.innerHTML = `
        <div class="card-body p-3">
            <div class="d-flex justify-content-between align-items-center mb-2">
                <span class="badge bg-primary bg-opacity-10 text-primary rounded-pill px-2 fw-semibold small ticket-badge d-inline-flex align-items-center justify-content-center text-nowrap flex-shrink-0" style="min-width: 90px; height: 26px;">${ticketId}</span>
                <span class="badge bg-warning bg-opacity-10 text-warning border border-warning border-opacity-25 px-2 py-1 rounded-pill" style="font-size:0.7rem;">MỚI</span>
            </div>
            <h6 class="card-title fw-bold text-dark mb-1 fs-6">${escapeHtml(deviceName || 'Thiết bị')}</h6>
            <p class="card-text small text-muted mb-3 description-clamp">${escapeHtml(description || '')}</p>
            <div class="d-flex justify-content-between align-items-center mt-auto pt-2 border-top border-light gap-2">
                <div class="d-flex align-items-center text-muted small fw-medium text-nowrap flex-shrink-0">
                    <i class="bi bi-calendar4 me-1"></i> Vừa tiếp nhận
                </div>
                <span class="live-timer normal text-nowrap flex-shrink-0" data-received-at="${new Date().toISOString()}">00:00</span>
            </div>
        </div>`;

    col.insertAdjacentElement('afterbegin', card);
    hideEmptyState(col.closest('.kanban-column'));
    updateColumnCount(col.closest('.kanban-column'), +1);

    // Re-attach drag events to new card
    card.addEventListener('dragstart', (e) => {
        draggedCard = card;
        dragSourceColumn = card.closest('.kanban-column');
        card.classList.add('dragging');
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/plain', card.dataset.ticketId);
    });
    card.addEventListener('dragend', () => {
        draggedCard = null;
        card.classList.remove('dragging');
    });

    // Remove flash after animation
    setTimeout(() => card.classList.remove('new-card-flash'), 2500);
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// ══════════════════════════════════════════════════════════════════════════════
// 5. LEGACY FUNCTIONS (kept for backward compatibility)
// ══════════════════════════════════════════════════════════════════════════════
function ganChoToi(ticketId) {
    if (!confirm('Xác nhận nhận phiếu này về tay?')) return;
    fetch('/KyThuat/GanChoToi', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': CSRF_TOKEN()
        },
        body: `id=${encodeURIComponent(ticketId)}`
    })
    .then(r => r.json())
    .then(data => {
        if (data.success) {
            toastr.success('Đã nhận phiếu! Bắt đầu thôi 💪');
            setTimeout(() => location.reload(), 1200);
        } else {
            toastr.error(data.message || 'Không thể nhận phiếu.');
        }
    })
    .catch(() => toastr.error('Lỗi kết nối.'));
}

function capNhatTrangThai(ticketId, trangThai) {
    fetch('/KyThuat/CapNhatTrangThai', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': CSRF_TOKEN()
        },
        body: `id=${encodeURIComponent(ticketId)}&trangThai=${encodeURIComponent(trangThai)}`
    })
    .then(r => r.json())
    .then(data => {
        if (data.success) {
            toastr.success('Đã cập nhật trạng thái!');
            setTimeout(() => location.reload(), 900);
        } else {
            toastr.error(data.message || 'Cập nhật thất bại.');
        }
    })
    .catch(() => toastr.error('Lỗi kết nối.'));
}
