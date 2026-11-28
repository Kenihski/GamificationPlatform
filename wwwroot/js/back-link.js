// "Back" buttons return to the page the user came from when it was on this site,
// for example My Challenges or the challenge details. Otherwise they follow their link.
document.addEventListener("click", function (event) {
    const link = event.target.closest("a[data-back-link]");

    if (link && document.referrer.startsWith(window.location.origin + "/")) {
        event.preventDefault();
        history.back();
    }
});
