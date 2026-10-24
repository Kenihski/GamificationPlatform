document.addEventListener("DOMContentLoaded", () => {
    const notifications = Array.from(
        document.querySelectorAll(".achievement-notification")
    );

    if (notifications.length === 0) {
        return;
    }

    const container = document.querySelector(
        ".achievement-notification-container"
    );

    const antiForgeryToken = document.querySelector(
        "#achievement-notification-antiforgery " +
        "input[name='__RequestVerificationToken']"
    )?.value;

    const markSeenUrl = container.dataset.markSeenUrl;

    const maxVisible = 1;
    const timeBetweenNotifications = 2500;
    const displayTime = 8000;
    const highlightTime = 4000;

    let nextIndex = 0;
    let visibleCount = 0;

    async function markNotificationSeen(notification) {
        const userAchievementId =
            notification.dataset.userAchievementId;

        if (
            !userAchievementId ||
            !antiForgeryToken ||
            !markSeenUrl
        ) {
            return;
        }

        const formData = new URLSearchParams();

        formData.append(
            "userAchievementId",
            userAchievementId
        );

        formData.append(
            "__RequestVerificationToken",
            antiForgeryToken
        );

        try {
            const response = await fetch(markSeenUrl, {
                method: "POST",
                headers: {
                    "Content-Type":
                        "application/x-www-form-urlencoded"
                },
                body: formData,
                keepalive: true
            });

            if (!response.ok) {
                console.error(
                    "Could not mark achievement notification as seen."
                );
            }
        }
        catch (error) {
            console.error(
                "Could not mark achievement notification as seen.",
                error
            );
        }
    }

    function showNotification(notification) {
        visibleCount++;

        // Move the next queued notification into view.
        container.prepend(notification);
        notification.style.display = "flex";

        const announcement = document.getElementById("achievement-announcement");
        if (announcement) {
            announcement.textContent = [
                notification.querySelector(".achievement-notification-content").textContent,
                notification.querySelector(".achievement-notification-points").textContent
            ].join(" ").replace(/\s+/g, " ").trim();
        }

        const closeButton = notification.querySelector(
            ".achievement-notification-close"
        );

        let timeoutId;
        let highlightTimeoutId;
        let returnFocus = null;

        function closeNotification() {
            clearTimeout(timeoutId);
            clearTimeout(highlightTimeoutId);

            if (notification.classList.contains("closing")) {
                return;
            }

            notification.classList.add("closing");

            // Marks the notification as seen when it is closed.
            markNotificationSeen(notification);

            setTimeout(() => {
                if (notification.contains(document.activeElement)) {
                    const target = returnFocus?.isConnected && returnFocus.tabIndex >= 0
                        ? returnFocus
                        : document.querySelector("main");

                    if (target) {
                        if (target.tagName === "MAIN") {
                            target.setAttribute("tabindex", "-1");
                        }
                        target.focus({ preventScroll: true });
                    }
                }
                notification.style.display = "none";
                visibleCount--;
            }, 300);
        }

        if (closeButton) {
            closeButton.addEventListener(
                "click",
                closeNotification
            );
        }

        // Changes to a neutral colour after the unlock highlight.
        highlightTimeoutId = setTimeout(() => {
            notification.classList.add("expiring");
        }, highlightTime);

        function startCloseTimer() {
            clearTimeout(timeoutId);
            if (!notification.classList.contains("closing") &&
                !notification.matches(":hover, :focus-within")) {
                timeoutId = setTimeout(closeNotification, displayTime);
            }
        }

        notification.addEventListener("mouseenter", () => clearTimeout(timeoutId));
        notification.addEventListener("mouseleave", startCloseTimer);
        notification.addEventListener("focusin", (event) => {
            clearTimeout(timeoutId);
            if (!notification.contains(event.relatedTarget)) {
                returnFocus = event.relatedTarget;
            }
        });
        notification.addEventListener("focusout", () => {
            // Wait until focus has settled before checking whether it is still inside.
            setTimeout(startCloseTimer, 0);
        });

        startCloseTimer();
    }

    function tryShowNextNotification() {
        if (
            nextIndex >= notifications.length ||
            visibleCount >= maxVisible
        ) {
            return;
        }

        const notification = notifications[nextIndex];
        nextIndex++;

        showNotification(notification);
    }

    // Shows the first notification immediately.
    tryShowNextNotification();

    // Uses one fixed interval so new notifications
    // always appear at a consistent pace.
    const notificationInterval = setInterval(() => {
        if (nextIndex >= notifications.length) {
            clearInterval(notificationInterval);
            return;
        }

        tryShowNextNotification();
    }, timeBetweenNotifications);
});
