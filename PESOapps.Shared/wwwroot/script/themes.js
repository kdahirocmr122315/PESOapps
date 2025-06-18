function toggleTheme() {
    let root = document.documentElement;
    let button = document.getElementById("themeButton");
    let currentBg = getComputedStyle(root).getPropertyValue('--bgcolor').trim();
    let icon = document.getElementById('icons');
    let anim = document.querySelector(".btn__icon");

    if (currentBg === '#F2F4F7') {
        // Switch to light mode
        root.style.setProperty('--bgcolor', '#07522D');
        root.style.setProperty('--font-color', '#ffffff');
        root.style.setProperty('--text-shadow', '#317154');
        root.style.setProperty('--box-shadow', '#FFFFFFBF');
        root.style.setProperty('--1A5AA5', '#317154');
        root.style.setProperty('--0AA9F9', '#0AA9F9');
        root.style.setProperty('--hsla', 'hsla(0, 0%, 100%, .1)');
        root.style.setProperty('--muted', '#080809');
        button.classList.add("dark-mode");
        button.classList.remove("light-mode");
        icon.classList.remove('fa-sun');
        icon.classList.add('fa-moon');
        anim.classList.add('animated');

        setTimeout(() => {
            anim.classList.remove('animated');
        }, 500)
    } else {

        // Switch to dark mode
        root.style.setProperty('--bgcolor', '#F2F4F7');
        root.style.setProperty('--font-color', '#080809');
        root.style.setProperty('--text-shadow', '#ffffff');
        root.style.setProperty('--box-shadow', '#080809BF');
        root.style.setProperty('--1A5AA5', '#ffffff');
        root.style.setProperty('--0AA9F9', '#e1e1e1');
        root.style.setProperty('--hsla', 'rgb(255, 255, 255,.8)');
        root.style.setProperty('--muted', '#6c757d');
        button.classList.add("light-mode");
        button.classList.remove("dark-mode");
        icon.classList.add('fa-sun');
        icon.classList.remove('fa-moon');
        anim.classList.add('animated');

        setTimeout(() => {
            anim.classList.remove('animated');
        }, 500)

    }

}
load();

