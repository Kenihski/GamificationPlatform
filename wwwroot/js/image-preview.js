document.addEventListener("DOMContentLoaded", function () {

    const imageSelect =
        document.getElementById("imageSelect");

    const imagePreview =
        document.getElementById("imagePreview");

    // Support both Challenge and Question views.
    const imageCategory =
        document.getElementById("imageCategory") ||
        document.getElementById("imageCategorySelect");

    const uploadImageSection =
        document.getElementById("uploadImageSection");

    const myImagesSection =
        document.getElementById("myImagesSection");

    const defaultImagesSection =
        document.getElementById("defaultImagesSection");

    const imageUpload =
        document.getElementById("imageUpload");

    const myImageSelect =
        document.getElementById("myImageSelect") ||
        document.getElementById("myImagesSelect");

    const defaultImageSelect =
        document.getElementById("defaultImageSelect") ||
        document.getElementById("defaultImagesSelect");

    if (!imageSelect || !imagePreview) {
        return;
    }

    // Forms provide the default used when the image selection is empty.
    const fallbackImageUrl = imageSelect.dataset.defaultImage || "";
    const imageSelectionStatus = document.getElementById("imageSelectionStatus");

    // Updates the image preview.
    function updateImagePreview(imageUrl) {

        imageUrl = imageUrl || fallbackImageUrl;
        if (imageSelectionStatus) {
            imageSelectionStatus.textContent = imageUrl === fallbackImageUrl
                ? "Selected image: Question 1 (default)."
                : "A custom image is selected. You can change it using the image controls above.";
        }

        if (imageUrl) {
            imagePreview.src = imageUrl;
            imagePreview.style.display = "block";
        }
        else {
            imagePreview.removeAttribute("src");
            imagePreview.style.display = "none";
        }
    }

    // Supports older pages with a simple image dropdown.
    if (!imageCategory) {

        imageSelect.addEventListener("change", function () {
            updateImagePreview(imageSelect.value);
        });

        updateImagePreview(imageSelect.value);

        return;
    }

    // Remember the original image on Update.
    const originalImageUrl = imageSelect.value;

    // Shows the selected image category.
    function updateImageCategory() {

        const category = imageCategory.value;

        if (uploadImageSection) {
            uploadImageSection.style.display =
                category === "upload" ? "block" : "none";
        }

        if (myImagesSection) {
            myImagesSection.style.display =
                category === "my" ? "block" : "none";
        }

        if (defaultImagesSection) {
            defaultImagesSection.style.display =
                category === "default" ? "block" : "none";
        }

        if (imageUpload) {

            imageUpload.disabled = category !== "upload";

            if (category !== "upload") {
                imageUpload.value = "";
            }
        }

        if (category === "current") {

            // Restore the original image.
            imageSelect.value = originalImageUrl;
            updateImagePreview(originalImageUrl);
        }
        else if (category === "my") {

            const imageUrl = myImageSelect
                ? myImageSelect.value
                : "";

            imageSelect.value = imageUrl;
            updateImagePreview(imageUrl);
        }
        else if (category === "default") {

            const imageUrl = defaultImageSelect
                ? defaultImageSelect.value
                : "";

            imageSelect.value = imageUrl;
            updateImagePreview(imageUrl);
        }
        else if (category === "upload") {

            imageSelect.value = "";

            if (!imageUpload || !imageUpload.files[0]) {
                updateImagePreview("");
            }
        }
        else {

            // No image selected.
            imageSelect.value = "";
            updateImagePreview("");
        }

    }

    // Changes between image categories.
    imageCategory.addEventListener(
        "change",
        updateImageCategory
    );

    // Selects an existing personal image.
    if (myImageSelect) {

        myImageSelect.addEventListener("change", function () {

            if (imageCategory.value !== "my") {
                return;
            }

            imageSelect.value = myImageSelect.value;

            updateImagePreview(myImageSelect.value);
        });
    }

    // Selects a default image.
    if (defaultImageSelect) {

        defaultImageSelect.addEventListener("change", function () {

            if (imageCategory.value !== "default") {
                return;
            }

            imageSelect.value = defaultImageSelect.value;

            updateImagePreview(defaultImageSelect.value);
        });
    }

    // Previews a newly selected upload.
    if (imageUpload) {

        imageUpload.addEventListener("change", function () {

            const file = imageUpload.files[0];

            if (!file) {
                updateImagePreview("");
                return;
            }

            const allowedTypes = [
                "image/jpeg",
                "image/png",
                "image/webp"
            ];

            if (!allowedTypes.includes(file.type)) {

                imageUpload.value = "";
                updateImagePreview("");

                alert("Please select a JPG, PNG or WebP image.");
                return;
            }

            if (file.size > 5 * 1024 * 1024) {

                imageUpload.value = "";
                updateImagePreview("");

                alert("The image must be smaller than 5 MB.");
                return;
            }

            const reader = new FileReader();

            reader.onload = function (event) {

                // Only show the preview if Upload is still selected
                // and the same file is still selected.
                if (imageCategory.value === "upload" &&
                    imageUpload.files[0] === file) {

                    updateImagePreview(event.target.result);
                }
            };

            reader.readAsDataURL(file);
        });
    }

    // Initialize without clearing an existing image on Update.
    function initializeImageSelector() {

        const category = imageCategory.value;

        if (uploadImageSection) {
            uploadImageSection.style.display =
                category === "upload" ? "block" : "none";
        }

        if (myImagesSection) {
            myImagesSection.style.display =
                category === "my" ? "block" : "none";
        }

        if (defaultImagesSection) {
            defaultImagesSection.style.display =
                category === "default" ? "block" : "none";
        }

        if (imageUpload) {
            imageUpload.disabled = category !== "upload";
        }

        // Keep the existing image URL on Update.
        updateImagePreview(originalImageUrl);

    }

    initializeImageSelector();

    // Back/Forward may restore the selected option after the page has loaded.
    window.addEventListener("pageshow", function () {
        setTimeout(updateImageCategory, 0);
    });
});
