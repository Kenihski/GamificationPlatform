
setTimeout(function () {
    const successMessage =
        document.getElementById("successMessage");

    const errorMessage =
        document.getElementById("errorMessage");

    if (successMessage) {
        successMessage.style.display = "none";
    }

    if (errorMessage) {
        errorMessage.style.display = "none";
    }
}, 3000);

document.addEventListener("DOMContentLoaded", () => {
    const navigationButtons = [
        ...document.querySelectorAll(".user-history-nav-button")
    ];

    if (navigationButtons.length === 0) {
        return;
    }

    const historyCards = [
        ...document.querySelectorAll(".user-history-card")
    ];

    const previousButton = document.getElementById("historyPrevious");
    const nextButton = document.getElementById("historyNext");
    const pageInfo = document.getElementById("historyPageInfo");

    const pageSize = 12;
    const totalPages = Math.ceil(navigationButtons.length / pageSize);

    let currentPage = 0;
    let selectedIndex = 0;

    function updateHistory() {
        navigationButtons.forEach((button, index) => {
            const isOnCurrentPage =
                Math.floor(index / pageSize) === currentPage;

            button.hidden = !isOnCurrentPage;
            button.setAttribute(
                "aria-pressed",
                String(index === selectedIndex)
            );
        });

        historyCards.forEach((card, index) => {
            card.hidden = index !== selectedIndex;
        });

        if (previousButton && nextButton && pageInfo) {
            previousButton.disabled = currentPage === 0;
            nextButton.disabled = currentPage === totalPages - 1;

            pageInfo.textContent =
                `Page ${currentPage + 1} of ${totalPages}`;
        }
    }

    navigationButtons.forEach((button, index) => {
        button.addEventListener("click", () => {
            selectedIndex = index;
            updateHistory();
        });
    });

    if (previousButton && nextButton) {
        previousButton.addEventListener("click", () => {
            if (currentPage > 0) {
                currentPage--;
                selectedIndex = currentPage * pageSize;
                updateHistory();
            }
        });

        nextButton.addEventListener("click", () => {
            if (currentPage < totalPages - 1) {
                currentPage++;
                selectedIndex = currentPage * pageSize;
                updateHistory();
            }
        });
    }

    updateHistory();
});
