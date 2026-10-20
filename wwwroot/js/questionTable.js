
document.addEventListener("DOMContentLoaded", () => {
    const searchInput = document.getElementById("questionSearch");
    const navigationButtons = Array.from(
        document.querySelectorAll(".questions-nav-button")
    );
    const questionRows = Array.from(
        document.querySelectorAll(".questions-table-row")
    );

    if (!searchInput || navigationButtons.length === 0) {
        return;
    }

    const challengePageSize = 12;
    const questionPageSize = 10;

    let selectedChallengeId = "all";
    let challengePage = 1;
    let questionPage = 1;

    const challengePagination = document.getElementById(
        "questionChallengePagination"
    );
    const challengePrevious = document.getElementById(
        "questionChallengePrevious"
    );
    const challengeNext = document.getElementById(
        "questionChallengeNext"
    );
    const challengePageInfo = document.getElementById(
        "questionChallengePageInfo"
    );

    const tablePagination = document.getElementById(
        "questionTablePagination"
    );
    const tablePrevious = document.getElementById(
        "questionTablePrevious"
    );
    const tableNext = document.getElementById(
        "questionTableNext"
    );
    const tablePageInfo = document.getElementById(
        "questionTablePageInfo"
    );

    const resultsHeading = document.getElementById(
        "questionsResultsHeading"
    );
    const resultCount = document.getElementById(
        "questionsResultCount"
    );
    const noResults = document.getElementById(
        "questionsNoResults"
    );

    function renderChallengeNavigation() {
        const totalPages = Math.ceil(
            navigationButtons.length / challengePageSize
        );

        const start = (challengePage - 1) * challengePageSize;
        const end = start + challengePageSize;

        navigationButtons.forEach((button, index) => {
            button.hidden = index < start || index >= end;

            button.setAttribute(
                "aria-pressed",
                String(button.dataset.challengeId === selectedChallengeId)
            );
        });

        challengePagination.hidden = totalPages <= 1;
        challengePrevious.disabled = challengePage === 1;
        challengeNext.disabled = challengePage === totalPages;

        challengePageInfo.textContent =
            `Page ${challengePage} of ${totalPages}`;
    }

    function getFilteredQuestions() {
        const searchTerm = searchInput.value.trim().toLowerCase();

        return questionRows.filter(row => {
            const matchesChallenge =
                selectedChallengeId === "all" ||
                row.dataset.challengeId === selectedChallengeId;

            const searchableText =
                (row.dataset.questionSearch || "").toLowerCase();

            return matchesChallenge &&
                searchableText.includes(searchTerm);
        });
    }

    function renderQuestions() {
        const filteredQuestions = getFilteredQuestions();
        const totalPages = Math.max(
            1,
            Math.ceil(filteredQuestions.length / questionPageSize)
        );

        questionPage = Math.min(questionPage, totalPages);

        // Hide all rows before showing the current page.
        questionRows.forEach(row => {
            row.hidden = true;
        });

        const start = (questionPage - 1) * questionPageSize;
        const visibleRows = filteredQuestions.slice(
            start,
            start + questionPageSize
        );

        visibleRows.forEach(row => {
            row.hidden = false;
        });

        const selectedButton = navigationButtons.find(
            button => button.dataset.challengeId === selectedChallengeId
        );

        const heading = selectedButton
            ? Array.from(selectedButton.childNodes)
                .filter(node => node.nodeType === Node.TEXT_NODE)
                .map(node => node.textContent.trim())
                .join(" ")
            : "All Questions";

        resultsHeading.textContent = heading;

        const count = filteredQuestions.length;
        resultCount.textContent =
            `${count} ${count === 1 ? "question" : "questions"}`;

        noResults.hidden = count !== 0;

        tablePagination.hidden = count === 0 || totalPages <= 1;
        tablePrevious.disabled = questionPage === 1;
        tableNext.disabled = questionPage === totalPages;
        tablePageInfo.textContent =
            `Page ${questionPage} of ${totalPages}`;
    }

    navigationButtons.forEach(button => {
        button.addEventListener("click", () => {
            selectedChallengeId = button.dataset.challengeId;
            questionPage = 1;

            renderChallengeNavigation();
            renderQuestions();
        });
    });

    searchInput.addEventListener("input", () => {
        questionPage = 1;
        renderQuestions();
    });

    challengePrevious.addEventListener("click", () => {
        if (challengePage > 1) {
            challengePage--;
            renderChallengeNavigation();
        }
    });

    challengeNext.addEventListener("click", () => {
        const totalPages = Math.ceil(
            navigationButtons.length / challengePageSize
        );

        if (challengePage < totalPages) {
            challengePage++;
            renderChallengeNavigation();
        }
    });

    tablePrevious.addEventListener("click", () => {
        if (questionPage > 1) {
            questionPage--;
            renderQuestions();
        }
    });

    tableNext.addEventListener("click", () => {
        const totalPages = Math.ceil(
            getFilteredQuestions().length / questionPageSize
        );

        if (questionPage < totalPages) {
            questionPage++;
            renderQuestions();
        }
    });

    renderChallengeNavigation();
    renderQuestions();
});
