const shell = document.getElementById('adminShell');
document.getElementById('toggleSidebar')?.addEventListener('click', () => shell.classList.toggle('sidebar-collapsed'));
document.getElementById('adminLogoutButton')?.addEventListener('click', async () => {
    await fetch('/admin/logout', { method: 'POST' });
    localStorage.removeItem('tin_tot_token');
    localStorage.removeItem('tin_tot_user');
    location.href = '/admin/login';
});
