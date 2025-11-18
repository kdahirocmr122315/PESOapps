// helper: convert "rgb(r, g, b)" to "#rrggbb", otherwise return trimmed lowercase
function normalizeColor(value) {
    if (!value) return '';
    value = value.trim().toLowerCase();
    // rgb(...) -> hex
    const m = value.match(/^rgb\(\s*([0-9]+)\s*,\s*([0-9]+)\s*,\s*([0-9]+)\s*\)$/i);
    if (m) {
        const r = parseInt(m[1], 10);
        const g = parseInt(m[2], 10);
        const b = parseInt(m[3], 10);
        return '#' + [r, g, b].map(n => n.toString(16).padStart(2, '0')).join('');
    }
    return value; // already hex or something else
}

function toggleTheme() {
    const root = document.documentElement;
    const button = document.getElementById("themeButton");
    const icon = document.getElementById('icons');
    const anim = document.querySelector(".btn__icon");

    // guard
    if (!root) return;

    const rawBg = getComputedStyle(root).getPropertyValue('--bgcolor');
    const currentBg = normalizeColor(rawBg);

    // compare normalized form; make the hex lowercase for consistency
    if (currentBg === '#f2f4f7') {
        // Switch to dark (you labeled this "light mode" earlier — confirm semantics)
        root.style.setProperty('--bgcolor', '#07522d');
        root.style.setProperty('--font-color', '#ffffff');
        root.style.setProperty('--text-shadow', '#317154');
        root.style.setProperty('--box-shadow', '#ffffffbf');
        root.style.setProperty('--1A5AA5', '#317154');
        root.style.setProperty('--0AA9F9', '#0aa9f9');
        root.style.setProperty('--hsla', 'hsla(0, 0%, 100%, .1)');
        root.style.setProperty('--muted', '#080809');
        root.style.setProperty('--box-shadow-log', 'inset 0px 0px 15px -4px var(--box-shadow)');
        if (button) { button.classList.add("dark-mode"); button.classList.remove("light-mode"); }
        if (icon) { icon.classList.remove('fa-sun'); icon.classList.add('fa-moon'); }
        if (anim) { anim.classList.add('animated'); setTimeout(() => anim.classList.remove('animated'), 500); }
    } else {
        // Switch to light (or "default")
        root.style.setProperty('--bgcolor', '#f2f4f7');
        root.style.setProperty('--font-color', '#080809');
        root.style.setProperty('--text-shadow', '#ffffff');
        root.style.setProperty('--box-shadow', '#080809bf');
        root.style.setProperty('--1A5AA5', '#ffffff');
        root.style.setProperty('--0AA9F9', '#e1e1e1');
        root.style.setProperty('--hsla', 'rgb(255, 255, 255, .8)');
        root.style.setProperty('--muted', '#6c757d');
        root.style.setProperty('--box-shadow-log', '9px 9px 16px #bebebe, -9px -9px 16px #ffffff');
        if (button) { button.classList.add("light-mode"); button.classList.remove("dark-mode"); }
        if (icon) { icon.classList.add('fa-sun'); icon.classList.remove('fa-moon'); }
        if (anim) { anim.classList.add('animated'); setTimeout(() => anim.classList.remove('animated'), 500); }
    }
}

// Initialize when DOM is ready. Replace toggleTheme() with any startup logic.
document.addEventListener('DOMContentLoaded', () => {
    // Optionally set initial state or attach click handlers:
    const btn = document.getElementById('themeButton');
    if (btn) btn.addEventListener('click', toggleTheme);

    // Optionally call toggleTheme() once to apply theme based on current CSS variable:
    // toggleTheme();
});
