// Hides the navbar while scrolling down and shows it again when the user
// scrolls up, moves the mouse to the top of the window or tabs into it.
// Keyboard focus is handled in CSS with :focus-within.
document.addEventListener("DOMContentLoaded", function () {
    const header = document.querySelector(".site-header");
    if (!header) return;

    const topZone = 40;
    const scrollTolerance = 4;
    let lastScrollY = window.scrollY;
    let revealedByMouse = false;

    function headerHeight() {
        return header.offsetHeight;
    }

    function setHidden(hidden) {
        header.classList.toggle("site-header-hidden", hidden);
    }

    // The hidden position moves the header up by exactly its own height.
    function updateHeight() {
        document.documentElement.style.setProperty("--site-header-height", headerHeight() + "px");
    }
    updateHeight();
    new ResizeObserver(updateHeight).observe(header);

    // Show the navbar at once when keyboard focus enters it, before the browser
    // scrolls the page to bring the focused element into view.
    header.addEventListener("focusin", function () {
        if (!header.classList.contains("site-header-hidden")) return;
        header.classList.add("site-header-instant");
        setHidden(false);
        requestAnimationFrame(function () {
            header.classList.remove("site-header-instant");
        });
    });

    window.addEventListener("scroll", function () {
        const scrollY = window.scrollY;

        if (scrollY <= headerHeight()) {
            setHidden(false);
            revealedByMouse = false;
        } else if (scrollY > lastScrollY + scrollTolerance) {
            setHidden(true);
            revealedByMouse = false;
        } else if (scrollY < lastScrollY - scrollTolerance) {
            setHidden(false);
        }

        lastScrollY = scrollY;
    }, { passive: true });

    document.addEventListener("mousemove", function (event) {
        if (window.scrollY <= headerHeight()) return;

        if (event.clientY <= topZone) {
            setHidden(false);
            revealedByMouse = true;
        } else if (revealedByMouse &&
                   event.clientY > headerHeight() + 24 &&
                   !header.contains(document.activeElement)) {
            setHidden(true);
            revealedByMouse = false;
        }
    }, { passive: true });
});
