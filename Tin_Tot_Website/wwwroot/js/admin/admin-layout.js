const shell = document.getElementById('adminShell');
document.getElementById('toggleSidebar')?.addEventListener('click', () => shell.classList.toggle('sidebar-collapsed'));
document.getElementById('adminLogoutButton')?.addEventListener('click', async () => {
    await fetch('/admin/logout', { method: 'POST' });
    localStorage.removeItem('tin_tot_token');
    localStorage.removeItem('tin_tot_user');
    location.href = '/admin/login';
});
(() => {
    const token = localStorage.getItem('tin_tot_token');
    if (!token) return;

    const messageBtn = document.getElementById('adminMessageBtn');
    const messageBadge = document.getElementById('adminMessageBadge');
    const conversationList = document.getElementById('adminPopupConversationList');
    const chatHeader = document.getElementById('adminPopupChatHeader');
    const chatBody = document.getElementById('adminPopupChatBody');
    const chatInput = document.getElementById('adminPopupChatInput');
    const sendBtn = document.getElementById('adminPopupSendBtn');

    let selectedReceiverKey = null;
    let selectedReceiverId = null;

    const authHeaders = () => ({ 'Content-Type': 'application/json', Authorization: `Bearer ${token}` });

    const profile = JSON.parse(localStorage.getItem('tin_tot_user') || '{}');
    const myUserId = Number(profile.id || 0);

    const fallbackAvatar = (name) => {
        const firstChar = (name || '?').trim().charAt(0).toUpperCase() || '?';
        return `https://ui-avatars.com/api/?name=${encodeURIComponent(firstChar)}&background=dee2e6&color=495057&size=80`;
    };

    const timeFmt = (v) => {
        if (!v) return '';
        const d = new Date(v);
        if (Number.isNaN(d.getTime())) return '';
        return d.toLocaleString('vi-VN', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit' });
    };

    const setMessageBadge = (count) => {
        messageBadge.textContent = count;
        messageBadge.classList.toggle('d-none', !count || count <= 0);
    };

    const updateActiveConversation = () => {
        conversationList?.querySelectorAll('.list-group-item').forEach(item => {
            item.classList.toggle('active', item.dataset.receiverKey === selectedReceiverKey);
        });
    };

    const loadConversations = async () => {
        const resp = await fetch('/api/messages/conversations', { headers: authHeaders() });
        if (!resp.ok) return;
        const json = await resp.json();
        if (!json.success) return;

        let unreadCount = 0;
        conversationList.innerHTML = '';

        json.data.forEach(x => {
            const el = document.createElement('button');
            el.type = 'button';
            el.className = 'list-group-item list-group-item-action';
            el.dataset.receiverKey = x.receiverKey;
            el.dataset.receiverId = x.receiverId;
            el.dataset.receiverName = x.displayName;
            el.innerHTML = `
                <div class='d-flex align-items-start gap-2'>
                    <img src='${x.avatar || fallbackAvatar(x.displayName)}' class='rounded-circle' style='width:36px;height:36px;object-fit:cover' alt='${x.displayName}' />
                    <div class='min-w-0'>
                        <div class='text-truncate'>${x.displayName}</div>
                        <small class='text-muted d-block text-truncate'>${x.lastMessage || ''}</small>
                    </div>
                </div>`;
            el.onclick = () => selectConversation(x.receiverKey, x.displayName, Number(x.receiverId));
            conversationList.appendChild(el);

            if (x.receiverId !== myUserId) unreadCount += 1;
        });

        setMessageBadge(unreadCount);
        updateActiveConversation();
    };

    const renderMessage = (m) => {
        const isSelf = Number(m.senderId || m.SenderId) === myUserId;
        const wrap = document.createElement('div');
        wrap.className = `mb-2 d-flex ${isSelf ? 'justify-content-end' : 'justify-content-start'}`;

        const bubble = document.createElement('div');
        bubble.className = `p-2 rounded ${isSelf ? 'bg-primary text-white' : 'bg-white border'}`;
        bubble.style.maxWidth = '78%';
        bubble.innerHTML = `<div>${(m.content || m.Content || '').replace(/</g, '&lt;')}</div><small class='opacity-75'>${timeFmt(m.sentAt || m.SentAt)}</small>`;

        wrap.appendChild(bubble);
        chatBody.appendChild(wrap);
    };

    const loadHistory = async () => {
        if (!selectedReceiverKey) return;
        const resp = await fetch(`/api/messages/history/${encodeURIComponent(selectedReceiverKey)}`, { headers: authHeaders() });
        if (!resp.ok) return;
        const json = await resp.json();
        if (!json.success) return;

        chatBody.innerHTML = '';
        json.data.forEach(renderMessage);
        chatBody.scrollTop = chatBody.scrollHeight;
    };

    const selectConversation = async (receiverKey, receiverName, receiverId) => {
        selectedReceiverKey = receiverKey;
        selectedReceiverId = receiverId;
        chatHeader.textContent = `Đang chat với ${receiverName}`;
        updateActiveConversation();
        await loadHistory();
    };

    const sendPopupMessage = async () => {
        if (!selectedReceiverKey) return;
        const content = (chatInput.value || '').trim();
        if (!content) return;

        const resp = await fetch('/api/messages/send', {
            method: 'POST',
            headers: authHeaders(),
            body: JSON.stringify({ receiverKey: selectedReceiverKey, content })
        });
        const json = await resp.json();
        if (!resp.ok || !json.success) return;

        chatInput.value = '';
        await loadHistory();
        await loadConversations();
    };

    sendBtn?.addEventListener('click', sendPopupMessage);
    chatInput?.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
            e.preventDefault();
            sendPopupMessage();
        }
    });

    messageBtn?.addEventListener('click', async () => {
        await loadConversations();
        if (selectedReceiverKey) await loadHistory();
    });

    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/messages')
        .withAutomaticReconnect()
        .build();

    connection.on('ReceiveMessage', async () => {
        await loadConversations();
        if (selectedReceiverKey) await loadHistory();
    });

    loadConversations();
    connection.start().catch(() => { });
})();