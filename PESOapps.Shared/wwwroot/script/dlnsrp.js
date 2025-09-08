function openTab(evt, tabName) {
    var i, tabcontent, tabbtns;
    tabcontent = document.getElementsByClassName("tab-content");
    for (i = 0; i < tabcontent.length; i++) {
        tabcontent[i].style.display = "none";
        tabcontent[i].classList.remove("active");
    }
    tabbtns = document.getElementsByClassName("tab-btn");
    for (i = 0; i < tabbtns.length; i++) {
        tabbtns[i].classList.remove("active");
    }
    document.getElementById(tabName).style.display = "block";
    document.getElementById(tabName).classList.add("active");
    evt.currentTarget.classList.add("active");
}

function printForm() {
    // Clone both front and back pages
    var frontPage = document.getElementById("frontPage").innerHTML;
    var backPage = document.getElementById("backPage").innerHTML;

    // Combine them in printable format
    var printContents = `
        <div class="modal-body-print">
            <div>${frontPage}</div>
            <div style="page-break-before: always;"></div> <!-- Forces new page -->
            <div>${backPage}</div>
        </div>
    `;

    // Open new window/tab
    var newWindow = window.open('', '_blank', 'width=1000,height=800');

    newWindow.document.write('<html><head><title>Print</title>');
    newWindow.document.write('<style>body{font-family:Poppins, sans-serif; font-size:12px;} p, ul {margin-block-start: 0em; margin-block-end: 0em;} table{border-collapse:collapse;width:100%;} td, th{border:1px solid black; padding:3px;} .page-break{page-break-before:always;}</style>');
    newWindow.document.write('</head><body>');
    newWindow.document.write(printContents);
    newWindow.document.write('</body></html>');

    newWindow.document.close();
    newWindow.focus();

    // Trigger print
    newWindow.print();
}



function closeNSRP() {
    document.querySelector(".modal-overlay").style.display = "none";
}

function openNSRP() {
    document.querySelector(".modal-overlay").style.display = "flex";
}
