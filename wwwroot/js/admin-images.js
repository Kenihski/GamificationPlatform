
document.addEventListener("DOMContentLoaded", function () {
    const deleteForms = document.querySelectorAll(".delete-image-form");

    deleteForms.forEach(function (form) {
        form.addEventListener("submit", function (event) {
            const confirmed = confirm(
                "Are you sure you want to permanently delete this image? " +
                "It will also be removed from any challenges or questions that use it."
            );

            if (!confirmed) {
                event.preventDefault();
            }
        });
    });
});
