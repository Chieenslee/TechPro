(() => {
  const state = {
    hub: null,
    connected: false,
    channels: [],
    activeChannelId: null,
    typingUsers: new Map(), // name -> timeoutId
    selfName: null,
    selfRole: null,
  };

  const $ = (sel) => document.querySelector(sel);
  const $$ = (sel) => Array.from(document.querySelectorAll(sel));

  function escapeHtml(s) {
    return (s ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#039;");
  }

  function formatTime(isoOrDate) {
    const d = isoOrDate instanceof Date ? isoOrDate : new Date(isoOrDate);
    const hh = String(d.getHours()).padStart(2, "0");
    const mm = String(d.getMinutes()).padStart(2, "0");
    return `${hh}:${mm}`;
  }

  function setStatus(text, kind = "muted") {
    const el = $("#chatConnStatus");
    if (!el) return;
    el.textContent = text;
    el.className = `small fw-medium chat-status chat-status--${kind}`;
  }

  function channelBadge(channel) {
    const roles = channel.allowedRoles ?? channel.AllowedRoles ?? [];
    if (!roles.length) return "";
    return `<span class="badge rounded-pill bg-light text-slate-600 border border-slate-200 ms-2">${roles.length} roles</span>`;
  }

  function renderChannels(filter = "") {
    const list = $("#chatChannelList");
    if (!list) return;

    const q = filter.trim().toLowerCase();
    const items = state.channels
      .filter((c) => !q || (c.name ?? c.Name ?? "").toLowerCase().includes(q))
      .map((c) => {
        const id = c.id ?? c.Id;
        const name = c.name ?? c.Name;
        const icon = c.icon ?? c.Icon ?? "chat-dots-fill";
        const color = c.color ?? c.Color ?? "#4f46e5";
        const active = id === state.activeChannelId;
        return `
          <button type="button"
            class="chat-channel-item ${active ? "is-active" : ""}"
            data-channel-id="${escapeHtml(id)}"
            title="${escapeHtml(name)}">
            <span class="chat-channel-avatar" style="--ch:${escapeHtml(color)}">
              <i class="bi bi-${escapeHtml(icon)}"></i>
            </span>
            <span class="chat-channel-meta">
              <span class="chat-channel-name">${escapeHtml(name)}</span>
              <span class="chat-channel-sub">Nhấn để mở kênh</span>
            </span>
          </button>
        `;
      })
      .join("");

    list.innerHTML =
      items ||
      `<div class="p-3 text-center text-slate-500 small">Không có kênh phù hợp.</div>`;

    $$("#chatChannelList .chat-channel-item").forEach((btn) => {
      btn.addEventListener("click", async () => {
        const channelId = btn.getAttribute("data-channel-id");
        await openChannel(channelId);
      });
    });
  }

  function renderMessage(msg) {
    const senderName = msg.senderName ?? msg.SenderName ?? "Unknown";
    const senderRole = msg.senderRole ?? msg.SenderRole ?? "";
    const body = msg.body ?? msg.Body ?? "";
    const sentAt = msg.sentAt ?? msg.SentAt ?? new Date().toISOString();

    const isMine =
      state.selfName &&
      senderName &&
      state.selfName.toLowerCase() === senderName.toLowerCase();

    const roleColor =
      senderRole === "StoreAdmin" || senderRole === "SystemAdmin"
        ? "text-rose-600"
        : senderRole === "Technician"
          ? "text-emerald-600"
          : senderRole === "Storekeeper"
            ? "text-amber-600"
            : "text-indigo-600";

    if (isMine) {
      return `
        <div class="chat-msg chat-msg--me">
          <div class="chat-msg__meta">
            <span class="chat-msg__time">${formatTime(sentAt)}</span>
            <span class="chat-msg__name">Tôi</span>
          </div>
          <div class="chat-bubble chat-bubble--me">${escapeHtml(body)}</div>
        </div>
      `;
    }

    return `
      <div class="chat-msg">
        <div class="chat-msg__meta">
          <span class="chat-msg__name ${roleColor}">${escapeHtml(senderName)}</span>
          <span class="chat-msg__time">${formatTime(sentAt)}</span>
        </div>
        <div class="chat-bubble">${escapeHtml(body)}</div>
      </div>
    `;
  }

  function appendMessage(msg) {
    const area = $("#chatMessages");
    if (!area) return;
    area.insertAdjacentHTML("beforeend", renderMessage(msg));
    area.scrollTop = area.scrollHeight;
  }

  function setHeader(channel) {
    const name = channel?.name ?? channel?.Name ?? "Chat";
    const icon = channel?.icon ?? channel?.Icon ?? "chat-dots-fill";
    const color = channel?.color ?? channel?.Color ?? "#4f46e5";
    const elName = $("#chatActiveName");
    const elIcon = $("#chatActiveIcon");
    if (elName) elName.textContent = name;
    if (elIcon) {
      elIcon.className = `bi bi-${icon}`;
      const wrap = $("#chatActiveAvatar");
      if (wrap) wrap.style.setProperty("--ch", color);
    }
  }

  function renderTyping() {
    const el = $("#chatTyping");
    if (!el) return;
    const names = Array.from(state.typingUsers.keys());
    if (!names.length) {
      el.classList.add("d-none");
      el.textContent = "";
      return;
    }
    el.classList.remove("d-none");
    el.textContent =
      names.length === 1
        ? `${names[0]} đang nhập...`
        : `${names.slice(0, 2).join(", ")} đang nhập...`;
  }

  async function openChannel(channelId) {
    if (!state.hub) return;
    if (state.activeChannelId === channelId) return;

    const prev = state.activeChannelId;
    state.activeChannelId = channelId;
    renderChannels($("#chatChannelSearch")?.value ?? "");

    const channel = state.channels.find((c) => (c.id ?? c.Id) === channelId);
    setHeader(channel);
    setStatus("Đang tải tin nhắn...", "muted");

    const area = $("#chatMessages");
    if (area) area.innerHTML = "";
    state.typingUsers.clear();
    renderTyping();

    try {
      if (prev) await state.hub.invoke("LeaveChannel", prev);
      await state.hub.invoke("JoinChannel", channelId);
      const recent = await state.hub.invoke("GetRecent", channelId, 80);
      (recent ?? []).forEach(appendMessage);
      setStatus("Đã kết nối", "ok");
    } catch (e) {
      console.error(e);
      setStatus("Không thể vào kênh (không đủ quyền hoặc lỗi mạng).", "bad");
    }
  }

  function setupHub() {
    if (!window.signalR) {
      setStatus("Thiếu SignalR client.", "bad");
      return null;
    }
    return new signalR.HubConnectionBuilder()
      .withUrl("/chatHub")
      .withAutomaticReconnect([0, 1000, 2000, 5000, 10000])
      .build();
  }

  async function boot() {
    const root = $("#internalChatApp");
    if (!root) return;

    state.selfName = root.getAttribute("data-user-name") || null;
    state.selfRole = root.getAttribute("data-user-role") || null;

    setStatus("Đang kết nối...", "muted");
    state.hub = setupHub();
    if (!state.hub) return;

    state.hub.onreconnecting(() => setStatus("Đang reconnect...", "muted"));
    state.hub.onreconnected(() => setStatus("Đã kết nối lại", "ok"));
    state.hub.onclose(() => setStatus("Mất kết nối", "bad"));

    state.hub.on("Message", (msg) => {
      if ((msg.channelId ?? msg.ChannelId) !== state.activeChannelId) return;
      appendMessage(msg);
    });

    state.hub.on("Typing", (channelId, name, _role, isTyping) => {
      if (channelId !== state.activeChannelId) return;
      if (!name || name === state.selfName) return;
      if (!isTyping) {
        const t = state.typingUsers.get(name);
        if (t) clearTimeout(t);
        state.typingUsers.delete(name);
        renderTyping();
        return;
      }
      const old = state.typingUsers.get(name);
      if (old) clearTimeout(old);
      const timeoutId = setTimeout(() => {
        state.typingUsers.delete(name);
        renderTyping();
      }, 1800);
      state.typingUsers.set(name, timeoutId);
      renderTyping();
    });

    state.hub.on("PresenceChanged", (channelId, count) => {
      if (channelId !== state.activeChannelId) return;
      const el = $("#chatOnlineCount");
      if (el) el.textContent = String(count ?? 0);
    });

    await state.hub.start();
    setStatus("Đã kết nối", "ok");

    state.channels = (await state.hub.invoke("GetMyChannels")) ?? [];
    renderChannels("");

    const first = state.channels[0];
    if (first) await openChannel(first.id ?? first.Id);

    const search = $("#chatChannelSearch");
    if (search) {
      search.addEventListener("input", () => renderChannels(search.value));
    }

    const form = $("#chatSendForm");
    const input = $("#chatInput");
    const btn = $("#chatSendBtn");

    let typingTimer = null;
    if (input) {
      input.addEventListener("input", () => {
        if (!state.activeChannelId) return;
        if (typingTimer) clearTimeout(typingTimer);
        state.hub.invoke("Typing", state.activeChannelId, true).catch(() => {});
        typingTimer = setTimeout(() => {
          state.hub.invoke("Typing", state.activeChannelId, false).catch(() => {});
        }, 900);
      });
    }

    if (form && input) {
      form.addEventListener("submit", async (e) => {
        e.preventDefault();
        const body = input.value.trim();
        if (!body || !state.activeChannelId) return;
        input.value = "";
        btn?.setAttribute("disabled", "disabled");
        try {
          await state.hub.invoke("SendMessage", state.activeChannelId, body);
        } catch (err) {
          console.error(err);
          setStatus("Gửi thất bại. Kiểm tra mạng/phiên đăng nhập.", "bad");
        } finally {
          btn?.removeAttribute("disabled");
          input.focus();
        }
      });
    }
  }

  document.addEventListener("DOMContentLoaded", boot);
})();

