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

    descriptionSelect.addEventListener("change", function () {
        if (descriptionSelect.value === "custom") {
            customDescriptionContainer.style.display = "block";
            questionDescription.value = customDescription.value;
            customDescription.focus();
        }
        else {
            customDescriptionContainer.style.display = "none";
            customDescription.value = "";
            questionDescription.value = descriptionSelect.value;
        }
    });

    customDescription.addEventListener("input", function () {
        questionDescription.value = customDescription.value;
    });
});