document.addEventListener("DOMContentLoaded", function () {
    const descriptionSelect =
        document.getElementById("descriptionSelect");

    const customDescriptionContainer =
        document.getElementById("customDescriptionContainer");

    const customDescription =
        document.getElementById("customDescription");

    const questionDescription =
        document.getElementById("questionDescription");

    if (!descriptionSelect ||
        !customDescriptionContainer ||
        !customDescription ||
        !questionDescription) {
        return;
    }

    function loadExistingDescription() {
        const existingDescription =
            questionDescription.value;

        const matchingOption =
            Array.from(descriptionSelect.options)
                .find(option =>
                    option.value === existingDescription);

        if (matchingOption) {
            descriptionSelect.value =
                existingDescription;

            customDescriptionContainer.style.display =
                "none";
        }
        else if (existingDescription) {
            descriptionSelect.value =
                "custom";

            customDescription.value =
                existingDescription;

            customDescriptionContainer.style.display =
                "block";
        }
        else {
            descriptionSelect.value = "";

            customDescriptionContainer.style.display =
                "none";
        }
    }

    descriptionSelect.addEventListener(
        "change",
        function () {
            if (descriptionSelect.value === "custom") {
                customDescriptionContainer.style.display =
                    "block";

                questionDescription.value =
                    customDescription.value;

                customDescription.focus();
            }
            else {
                customDescriptionContainer.style.display =
                    "none";

                customDescription.value = "";

                questionDescription.value =
                    descriptionSelect.value;
            }
        });

    customDescription.addEventListener(
        "input",
        function () {
            questionDescription.value =
                customDescription.value;
        });

    loadExistingDescription();
});