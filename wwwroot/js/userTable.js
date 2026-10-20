
document.addEventListener("DOMContentLoaded", () => {
    const searchInput = document.getElementById("userSearch");
    const userRows = document.querySelectorAll(".users-table-row");
    const noResults = document.getElementById("usersNoResults");

    if (!searchInput || !noResults) {
        return;
    }

    searchInput.addEventListener("input", () => {
        const searchTerm = searchInput.value.trim().toLowerCase();
        let visibleCount = 0;

        userRows.forEach(row => {
            const searchableText =
                (row.dataset.userSearch || "").toLowerCase();

            const matches = searchableText.includes(searchTerm);

            row.hidden = !matches;

            if (matches) {
                visibleCount++;
            }
        });

        noResults.hidden = visibleCount > 0;
    });
});
