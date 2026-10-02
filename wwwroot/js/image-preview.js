document.addEventListener("DOMContentLoaded", function () {

    const imageSelect =
        document.getElementById("imageSelect");

    const imagePreview =
        document.getElementById("imagePreview");


    if (!imageSelect || !imagePreview) {
        return;
    }


    function updateImagePreview() {

        const selectedImage =
            imageSelect.value;


        if (selectedImage) {

            imagePreview.src =
                selectedImage;

            imagePreview.style.display =
                "block";
        }
        else {

            imagePreview.src =
                "";

            imagePreview.style.display =
                "none";
        }
    }


    // Updates the preview when a new
    // image is selected.
    imageSelect.addEventListener(
        "change",
        updateImagePreview
    );


    // Shows the selected image if the
    // form is loaded with an existing value.
    updateImagePreview();

});