document.addEventListener("DOMContentLoaded", function () {
    const items = document.querySelectorAll('.item'); // Selectăm toate elementele cu clasa .item
    const paginationControls = document.querySelectorAll('.buton-schimbare-pagina'); // Selectăm toate butoanele de paginare
    const itemsPerPage = 3; // Numărul de iteme pe pagină

    // Funcția pentru afișarea itemelor pentru pagina curentă
    function showPage(pageNumber) {
        // Ascundem toate item-urile
        items.forEach((item, index) => {
            item.style.display = "none"; // Ascunde toate
            if (index >= (pageNumber - 1) * itemsPerPage && index < pageNumber * itemsPerPage) {
                item.style.display = "flex"; // Afișează doar cele din pagina curentă
            }
        });

        // Actualizăm starea butoanelor
        paginationControls.forEach(control => control.classList.remove('active')); // Eliminăm clasa active
        document.querySelector(`.buton-schimbare-pagina[data-page="${pageNumber}"]`).classList.add('active'); // Adăugăm active pentru butonul curent
    }

    // Adăugăm evenimente de click pe fiecare buton
    paginationControls.forEach(control => {
        control.addEventListener('click', function () {
            const pageNumber = parseInt(this.dataset.page); // Preluăm pagina din atributul data-page
            showPage(pageNumber); // Afișăm pagina respectivă
        });
    });

    // Afișăm implicit prima pagină
    showPage(1);
});
