# Subapp 1 — Integrated UX and challenge-category changes

Date: 2026-11-04
Integration branch: `Salman-Freyr-integration-ux`
Base: `Freyr-branch`
Features incorporated from: `Salman-ux-confirmation-registration-category`

## Integration approach
Built directly on Freyr's branch to retain its responsive design, accessibility work, default "Question 1" challenge image, and related tests. Applied our quiz, registration, and challenge-category changes on top. Original source branches and `main` were not modified.

## Increment 1 — Category creation and owner-admin changes

Commits:
- `0b39b11` feat(challenge): allow admin owners to change their challenge type
- `86da55c` feat(challenge): offer admins Core or Community at creation
- `edcdb1a` feat(challenge): show Change Type only to admin challenge owners

### What
- Regular users always create Community challenges, including if they forge `IsCore=true` in a request; the controller overrides that value.
- Administrators select Core/Community when creating challenges.
- The challenge details page shows a single **Change Type** button only to an admin who created that challenge.
- `ChangeType` is a POST endpoint with role authorization, antiforgery validation, and server-side challenge-owner verification.
- The endpoint toggles Core ↔ Community for admin-owned challenges. The former `MakeCore` and `MakeCommunity` endpoints have been removed.
- The regular update action does not bind changes to the persisted `IsCore` field.

### Why
Category choice is unrestricted for admins at creation, but later changes are permitted only for their own challenges, not those created by other admins or community users. Checking the owner on the server prevents bypassing the UI using a forged request.

### Compatibility
Freyr's default-image fallback is preserved in `ChallengeController.Create` and the Create view, and the new type selector is added without replacing his image controls.

## Increment 2 — Registration success on the login page

Commits:
- `9169833` feat(account): show registration success on the login redirect
- `146ee97` feat(account): display registration success feedback on login page
- `7eb7036` fix(account): redirect modal registration to login with success feedback

### What
- After a successful ordinary registration, the controller sets a one-time `TempData["SuccessMessage"]` and redirects to `Account/Login`, as before.
- The login view shows the message: "Registration successful! Your account has been created. You can now log in."
- Successful AJAX/modal registration now responds with a login URL, and the modal JavaScript navigates to that page so the same notification is visible.
- Failed registrations retain existing validation/error handling; registration never automatically authenticates the user.
- The previous proposed separate RegistrationSuccess page was deliberately not carried into this integration because it conflicts with the revised redirect requirement.

### Why
The user gets clear registration feedback while keeping the familiar login destination and reducing unnecessary interactions.

## Increment 3 — Quiz unanswered-question confirmation

Commit:
- `fbd52f9` fix(quiz): retain unanswered-question submission confirmation

### What
- Use the actual `.quiz-question-card` selector in `wwwroot/js/challenge.js`.
- Warn when some questions are unanswered, and use stronger wording when all are unanswered.
- Leave fully answered submissions, timeout auto-submission, and exit confirmation behavior intact.

### Why
The old selector `.question-card` did not match the Razor view and incorrectly counted zero questions, so the warning could be bypassed.

## Increment 4 — Regression test additions

Commit:
- `bfc6b7e` test(challenge): cover Core creation and admin-owner type changes

### What
- Added xUnit tests for a regular user submitting `IsCore=true`, admin Core creation, denial when an admin does not own the challenge, and owner-admin type toggling in both directions.
- Preserved Freyr's existing default-image tests.

### Why
Role and ownership checks are security-sensitive and should be covered with repeatable tests rather than relying exclusively on button visibility.

## Verification and release notes
- Static source review performed; runtime/browser testing and `dotnet test` have **not** been executed in this environment.
- Before merge to main, run `dotnet test tests/GamificationPlatform.Tests/GamificationPlatform.Tests.csproj` and perform browser checks.
- Recommended browser checks: register via full-page and modal flows (success and failure); check the message on Login; regular user versus admin challenge creation; admin-owned versus another user's challenge; both category-toggle directions; quiz with 0/N, some/N, N/N answers, exit, and timer expiry; image fallback and validation.
- `main`, `Freyr-branch`, and the original Salman UX branch remain unchanged.
