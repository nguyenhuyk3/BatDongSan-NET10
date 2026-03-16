window.showToast = function (title, message, isSuccess = true) {
    document.getElementById('toastTitle').innerText = title;
    document.getElementById('toastBody').innerText = message;

    const toastEl = document.getElementById('liveToast');

    // Đổi màu tiêu đề
    const header = toastEl.querySelector('.toast-header');
    header.classList.remove('bg-success', 'bg-danger');
    header.classList.add(isSuccess ? 'bg-success' : 'bg-danger');

    const bsToast = new bootstrap.Toast(toastEl, { delay: 3000 });
    bsToast.show();
};