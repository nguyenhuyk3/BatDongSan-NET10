export function scanAndHoistLegendTitle(bodySelector, outletSelector) {
    const body = document.querySelector(bodySelector) || document;
    keepOnly($('.main-body'), ['py-4', 'px-8', 'main-body']);
    const outlet = document.querySelector(outletSelector);
    if (!outlet) return;

    const hoist = () => {
        const h1 = body.querySelector("h1.legend-title");
        let titleText = h1?.getAttribute('data-page-class') || h1?.textContent?.trim() || "";
        const slug = toSlug(titleText);
        if (!titleText) titleText = location.pathname || "/";
        if (h1 && h1.textContent.trim().length) {
            outlet.textContent = h1.textContent.trim();
            h1.style.display = "none"; // ẩn h1 gốc trong body
        } else {
            outlet.textContent = ""; // fallback
        }
        
        body.classList.add(slug);        
    };

    // chạy sau khi DOM vẽ xong 1 tick
    requestAnimationFrame(hoist);

    // Quan sát thay đổi DOM (nội dung render/async)
    const key = "__legendTitleObserver";
    if (window[key]) window[key].disconnect();
    const obs = new MutationObserver((muts) => {
        // Nếu có node mới hoặc thay đổi text thì hoist lại
        hoist();
    });
    obs.observe(body, { childList: true, subtree: true, characterData: true });
    window[key] = obs;

    // Back/forward
    window.addEventListener("popstate", () => requestAnimationFrame(hoist), { passive: true });
}

function toSlug(input) {
    if (!input) return "";
    let s = input.normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/đ/g, 'd')
        .replace(/Đ/g, 'D');
    s = s.replace(/^\/+|\/+$/g, '')
        .replace(/[\/\s_]+/g, '-')
        .replace(/[^a-zA-Z0-9-]/g, '')
        .replace(/-+/g, '-')
        .toLowerCase();
    return s || 'home';
}
function keepOnly($els, keepList) {
    const keep = new Set(keepList);
    $els.each(function () {
        const $el = $(this);
        const classes = ($el.attr('class') || '').split(/\s+/).filter(Boolean);
        const toRemove = classes.filter(c => !keep.has(c));
        if (toRemove.length) $el.removeClass(toRemove.join(' '));
        keepList.forEach(c => $el.addClass(c));
    });
}
