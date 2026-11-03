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

    // Keep this breakpoint in sync with achievements.css.
    const desktopLayout = window.matchMedia("(min-width: 992px)");
    let maxVisible = desktopLayout.matches ? 3 : 1;
    const timeBetweenNotifications = 2500;
    const displayTime = 8000;
    const highlightTime = 4000;

    const pendingNotifications = [...notifications];
    const activeNotifications = new Map();

    // Follow the actual navbar height, including wrapped or enlarged text.
    const navbar = document.querySelector(".site-navbar");
    if (navbar) {
        const updateNavbarOffset = () => container.style.setProperty(
            "--notification-top", `${navbar.getBoundingClientRect().height + 12}px`
        );
        new ResizeObserver(updateNavbarOffset).observe(navbar);
        updateNavbarOffset();
    }

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
        const events = new AbortController();

        // Requeue extra cards when switching to a smaller screen.
        // They are not marked as read until they actually close.
        activeNotifications.set(notification, () => {
            clearTimeout(timeoutId);
            clearTimeout(highlightTimeoutId);
            events.abort();
            notification.style.display = "none";
            notification.classList.remove("expiring");
            activeNotifications.delete(notification);
        });

        function closeNotification() {
            clearTimeout(timeoutId);
            clearTimeout(highlightTimeoutId);

            if (notification.classList.contains("closing")) {
                return;
            }

            notification.classList.add("closing");
            events.abort();

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
                activeNotifications.delete(notification);
                applyNotificationLimit();
            }, 300);
        }

        if (closeButton) {
            closeButton.addEventListener(
                "click",
                closeNotification,
                { signal: events.signal }
            );
        }

        // Changes to a neutral colour after the unlock highlight.
        highlightTimeoutId = setTimeout(() => {
            notification.classList.add("expiring");
        }, highlightTime);

        function startCloseTimer() {
            clearTimeout(timeoutId);
            if (activeNotifications.has(notification) &&
                !notification.classList.contains("closing") &&
                !notification.matches(":hover, :focus-within")) {
                timeoutId = setTimeout(closeNotification, displayTime);
            }
        }

        notification.addEventListener("mouseenter", () => clearTimeout(timeoutId), { signal: events.signal });
        notification.addEventListener("mouseleave", startCloseTimer, { signal: events.signal });
        notification.addEventListener("focusin", (event) => {
            clearTimeout(timeoutId);
            if (!notification.contains(event.relatedTarget)) {
                returnFocus = event.relatedTarget;
            }
        }, { signal: events.signal });
        notification.addEventListener("focusout", () => {
            // Wait until focus has settled before checking whether it is still inside.
            setTimeout(startCloseTimer, 0);
        }, { signal: events.signal });

        startCloseTimer();
    }

    function tryShowNextNotification() {
        if (
            pendingNotifications.length === 0 ||
            activeNotifications.size >= maxVisible
        ) {
            return;
        }

        const notification = pendingNotifications.shift();

        showNotification(notification);
    }

    function applyNotificationLimit() {
        maxVisible = desktopLayout.matches ? 3 : 1;
        // Preserve the card being used with the keyboard when the window shrinks.
        const active = [...activeNotifications.keys()].sort((a, b) =>
            Number(b.contains(document.activeElement)) - Number(a.contains(document.activeElement))
        );
        for (const notification of active.slice(maxVisible)) {
            if (!notification.classList.contains("closing")) {
                activeNotifications.get(notification)();
                pendingNotifications.unshift(notification);
            }
        }
    }

    desktopLayout.addEventListener("change", applyNotificationLimit);

    // Shows the first notification immediately.
    tryShowNextNotification();

    // Uses one fixed interval so new notifications
    // always appear at a consistent pace.
    const notificationInterval = setInterval(() => {
        if (pendingNotifications.length === 0 && activeNotifications.size === 0) {
            clearInterval(notificationInterval);
            return;
        }

        tryShowNextNotification();
    }, timeBetweenNotifications);
});
