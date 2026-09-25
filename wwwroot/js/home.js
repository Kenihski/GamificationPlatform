document.addEventListener("DOMContentLoaded", function () {

    const message = document.getElementById("successMessage");

    if (message) {
        setTimeout(function () {
            message.remove();
        }, 3000);
    }

});