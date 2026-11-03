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

