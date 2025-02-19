function toggleTheme() {
    let root = document.documentElement;
    let button = document.getElementById("themeButton");
    let currentBg = getComputedStyle(root).getPropertyValue('--bgcolor').trim();
    let icon = document.getElementById('icons');
    let anim = document.querySelector(".btn__icon");

    if (currentBg === '#00449E') {
        // Switch to dark mode
        root.style.setProperty('--bgcolor', '#ffffff'); 
        root.style.setProperty('--font-color', '#000000'); 
        root.style.setProperty('--text-shadow', '#ffffff');
        root.style.setProperty('--box-shadow', '#000000BF');
        root.style.setProperty('--1A5AA5', '#ffffff');
        root.style.setProperty('--0AA9F9', '#e1e1e1');
        root.style.setProperty('--hsla', 'rgb(255, 255, 255,.8)');
        button.classList.add("light-mode");
        button.classList.remove("dark-mode");
        icon.classList.add('fa-sun');
        icon.classList.remove('fa-moon');
        anim.classList.add('animated');

        setTimeout(() => {
            anim.classList.remove('animated');
        }, 500)
    } else {
        // Switch to light mode
        root.style.setProperty('--bgcolor', '#00449E');  
        root.style.setProperty('--font-color', '#ffffff'); 
        root.style.setProperty('--text-shadow', '#1A5AA5');
        root.style.setProperty('--box-shadow', '#FFFFFFBF'); 
        root.style.setProperty('--1A5AA5', '#1A5AA5')
        root.style.setProperty('--0AA9F9', '#0AA9F9');            ;
        root.style.setProperty('--hsla', 'hsla(0, 0%, 100%, .1)'); 
        button.classList.add("dark-mode");
        button.classList.remove("light-mode");
        icon.classList.remove('fa-sun');
        icon.classList.add('fa-moon');
        anim.classList.add('animated');

        setTimeout(() => {
            anim.classList.remove('animated');
        }, 500)
    }

}
load();

