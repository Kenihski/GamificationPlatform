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

    const removeImageButton =
        document.getElementById("removeImageButton");

    if (!imageSelect || !imagePreview) {
        return;
    }

    // Updates the image preview.
    function updateImagePreview(imageUrl) {

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

    // Shows Remove only for a selected personal image.
    function updateRemoveButton() {

        if (!removeImageButton || !myImageSelect) {
            return;
        }

        removeImageButton.style.display =
            imageCategory.value === "my" && myImageSelect.value
                ? "inline-block"
                : "none";
    }

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

        updateRemoveButton();
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
            updateRemoveButton();
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

    // Deletes a personal image after confirmation.
    if (removeImageButton && myImageSelect) {

        removeImageButton.addEventListener("click", async function () {

            const selectedOption =
                myImageSelect.options[myImageSelect.selectedIndex];

            if (!selectedOption || !selectedOption.value) {
                return;
            }

            const imageId = selectedOption.dataset.imageId;

            if (!imageId) {
                alert("Could not identify the selected image.");
                return;
            }

            const confirmed = confirm(
                "Are you sure you want to permanently delete this image?"
            );

            if (!confirmed) {
                return;
            }

            // Read the antiforgery token from the form.
            const token = document.querySelector(
                'input[name="__RequestVerificationToken"]'
            );

            if (!token) {
                alert("Could not verify the request.");
                return;
            }

            removeImageButton.disabled = true;

            try {

                const formData = new FormData();

                formData.append("id", imageId);

                formData.append(
                    "__RequestVerificationToken",
                    token.value
                );

                const response = await fetch(
                    "/UserImage/Delete",
                    {
                        method: "POST",
                        body: formData,
                        credentials: "same-origin",
                        redirect: "manual"
                    }
                );

                // The controller redirects after deletion.
                // Reload to retrieve the updated image list.
                if (response.status === 0 ||
                    response.type === "opaqueredirect" ||
                    response.status === 302 ||
                    response.status === 303) {

                    window.location.reload();
                    return;
                }

                if (!response.ok) {
                    alert("Could not delete the image.");
                    return;
                }

                window.location.reload();
            }
            catch (error) {

                console.error("Image deletion failed:", error);

                alert("Could not delete the image.");
            }
            finally {

                removeImageButton.disabled = false;
            }
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

        updateRemoveButton();
    }

    initializeImageSelector();
});