document.addEventListener("DOMContentLoaded", function () {
    const element = document.getElementById("featuredChallengesCarousel");

    if (!element || element.querySelectorAll(".carousel-item").length < 2) {
        return;
    }

    // Manage both pause conditions together so leaving with the mouse cannot
    // restart rotation while a keyboard user is still inside the carousel.
    const carousel = new bootstrap.Carousel(element, { ride: false, pause: false });

    function updateRotation() {
        if (element.matches(":hover") || element.contains(document.activeElement)) {
            carousel.pause();
        } else {
            carousel.cycle();
        }
    }

    element.addEventListener("mouseenter", updateRotation);
    element.addEventListener("mouseleave", updateRotation);
    element.addEventListener("focusin", updateRotation);
    element.addEventListener("focusout", function () {
        // Wait until the browser has moved focus to its next destination.
        setTimeout(updateRotation, 0);
    });

    updateRotation();
});
