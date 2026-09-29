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