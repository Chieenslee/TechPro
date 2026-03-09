// Ticket Detail Page JavaScript - chat, parts, notes, scratches

function themYeuCauLinhKien() {
    const select = document.getElementById('partSelect');
    if (!select || !select.value) {
        toastr.warning('Vui lòng chọn linh kiện');
        return;
    }
    
    const option = select.options[select.selectedIndex];
    const partId = select.value;
    const partName = option.text.split(' (')[0];
    
    $.ajax({
        url: '/KyThuat/TaoYeuCauLinhKien',
        type: 'POST',
        data: { 
            ticketId: ticketId,
            partId: partId,
            quantity: 1
        },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                toastr.success('Đã tạo yêu cầu linh kiện');
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

function luuKetQuaKiemTra() {
    const notes = document.getElementById('diagnosisNotes')?.value;
    if (!notes || !notes.trim()) {
        toastr.warning('Vui lòng nhập kết quả kiểm tra');
        return;
    }
    
    $.ajax({
        url: '/KyThuat/LuuKetQuaKiemTra',
        type: 'POST',
        data: {
            ticketId: ticketId,
            notes: notes
        },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                toastr.success('Đã lưu kết quả kiểm tra');
            } else {
                toastr.error(response.message);
            }
        },
        error: function() {
            toastr.error('Có lỗi xảy ra.');
        }
    });
}

document.addEventListener('DOMContentLoaded', function() {
    renderChat();
    
    // Initialize Lucide icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }

    // Khởi tạo SignalR cho chat & scratch
    if (typeof signalR !== 'undefined') {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/ticketHub")
            .build();

        connection.on("ChatMessage", function (tId, userName, message, time) {
            if (tId === ticketId) {
                fetchNotes();
            }
        });

        connection.on("ScratchUpdated", function (tId) {
            if (tId === ticketId) {
                fetchScratchMarks();
            }
        });

        connection.start().catch(function (err) {
            console.error("SignalR connection error: ", err);
        });
    }

    fetchNotes();
    fetchScratchMarks();
});

// ---------------- CHAT ----------------
let chatHistory = [];

function fetchNotes() {
    $.getJSON(`/KyThuat/GetNotes?ticketId=${ticketId}`, function (res) {
        if (res.success) {
            chatHistory = res.data || [];
            renderChat();
        }
    });
}

function sendChatMessage(event) {
    event.preventDefault();
    const input = document.getElementById('chatMessageInput');
    if (!input || !input.value.trim()) return;
    const message = input.value.trim();

    $.ajax({
        url: '/KyThuat/AddNote',
        type: 'POST',
        data: {
            ticketId: ticketId,
            message: message
        },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (res) {
            if (res.success) {
                input.value = '';
                fetchNotes();
            } else {
                toastr.error(res.message || 'Không gửi được tin nhắn.');
            }
        },
        error: function () {
            toastr.error('Có lỗi xảy ra.');
        }
    });
}

function renderChat() {
    const container = document.getElementById('chatHistory');
    if (!container) return;
    
    container.innerHTML = chatHistory.map(msg => {
        const isMe = msg.userName === window.currentUserName;
        return `
            <div class="mb-3">
                <div class="d-flex flex-column ${isMe ? 'align-items-end' : 'align-items-start'}">
                    <div class="p-2 rounded mb-1 ${isMe ? 'bg-primary text-white' : 'bg-light'}" style="max-width: 80%;">
                        <div class="small">${msg.message}</div>
                    </div>
                    <span class="text-muted small">${msg.userName} • ${msg.time}</span>
                </div>
            </div>
        `;
    }).join('');
    
    container.scrollTop = container.scrollHeight;
}

// ---------------- SCRATCH MARKS ----------------
let scratchMarks = [];

function fetchScratchMarks() {
    $.getJSON(`/KyThuat/GetScratchMarks?ticketId=${ticketId}`, function (res) {
        if (res.success) {
            scratchMarks = res.data || [];
            if (typeof renderMarks === 'function') {
                renderMarks();
            }
        }
    });
}

function saveScratchMarks() {
    $.ajax({
        url: `/KyThuat/SaveScratchMarks?ticketId=${ticketId}`,
        type: 'POST',
        data: JSON.stringify(scratchMarks),
        contentType: 'application/json',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(res) {
            if (res.success) {
                toastr.success('Đã lưu đánh dấu xước');
            } else {
                toastr.error(res.message || 'Không lưu được đánh dấu.');
            }
        },
        error: function() {
            toastr.error('Có lỗi xảy ra.');
        }
    });
}

