# UX and Challenge Category Changes — 2026-11-03

Working branch: `Salman-ux-confirmation-registration-category`

## Increment 1 — Unanswered question confirmation

Commit: `1e47a2711361d89c234632475d21a17fbb653aa2`
Message: `fix(quiz): confirm submission when questions are unanswered`

### What changed
- Updated `wwwroot/js/challenge.js` to find quiz questions using the actual `.quiz-question-card` class from `Views/Quiz/Take.cshtml`.
- Show distinct confirmation text when all questions are unanswered versus when only some remain unanswered.
- Keep the existing submit-without-confirmation path for fully answered attempts, existing exit confirmation, and automatic timer expiration.

### Why
The existing handler was querying `.question-card`, which was not used by the quiz view, so no questions were counted and the confirmation never appeared. The change warns students without blocking intentional submission.

### Verification
Code review: matched the question-card selector against the Razor markup; no runtime/browser tests executed in this environment.
Manual checks recommended: submit 0/N, some/N, and N/N answers; cancel then confirm; test radio, checkbox, short-answer, exit, and timer expiration.


## Increment 2 — Registration success confirmation

Commits:
- `a90b4f6` — `feat(account): redirect successful registration to confirmation page`
- `6f1bdb0` — `feat(account): add accessible registration success page with login choice`
- `2a7bb20` — `feat(account): show registration success and optional login in modal`

### What changed
- Successful full-page registrations now use a dedicated `RegistrationSuccess` action and Razor view rather than redirecting immediately to Login.
- Confirmation shows an explicit Log In action (preserving a safe local return URL) and Stay on Site action.
- Successful modal registration displays a success alert and an optional Log In button, rather than automatically switching to the login modal.
- Registration failures still use existing validation/error responses; successful registration does not automatically authenticate the user.

### Why
Users need positive feedback that the account was created before deciding whether to log in. Both full-page and modal registration flows are supported.

### Verification
Code reviewed against `AccountController`, `Register.cshtml`, `_Layout.cshtml`, and `auth-modal.js`. Runtime tests not executed. Manually test full-page and modal registration, duplicate email/username, invalid input, returning to the site, and the login action.

## Increment 3 — Immutable challenge category

Commits:
- `891501c` — `fix(challenge): restrict category to creation and remove category-changing endpoints`
- `24f40b3` — `fix(challenge): remove admin category-change buttons`
- `5bc9786` — `feat(challenge): allow admins to choose immutable category at creation`

### What changed
- Removed `MakeCore` and `MakeCommunity` POST actions from `ChallengeController`.
- Removed admin category-conversion controls from the challenge details view.
- Admins may select Core or Community when creating a new challenge. Ordinary users always create Community challenges; a crafted request cannot create Core because the controller enforces the role.
- Existing challenge updates still change only allowlisted fields and intentionally never update `IsCore`.

### Why
Challenge classification is a creation-time decision. Removing both the UI and its POST endpoints prevents subsequent admin conversions; enforcing the role in the controller prevents ordinary users from setting Core with a crafted form submission.

### Verification
Source reviewed against controller create/update methods and create/details Razor views. Runtime tests not executed. Manually test admin Core creation, admin Community creation, non-admin crafted IsCore=true, edit preserving category, and removed conversion routes.

## Overall status
Changes committed to a separate development branch. Not merged into `main`. No runtime, browser, or automated test execution was performed here; all verification noted above is static code review.
