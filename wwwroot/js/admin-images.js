
document.addEventListener("DOMContentLoaded", function () {
    const deleteForms = document.querySelectorAll(".delete-image-form");

    deleteForms.forEach(function (form) {
        form.addEventListener("submit", function (event) {
            const confirmed = confirm(
                "Are you sure you want to permanently delete this image? " +
                "Every challenge and question using it will use the Question 1 default image instead."
            );

            if (!confirmed) {
                event.preventDefault();
            }
        });
    });
});
