# Task 5 — Server-side input validation, error handling, and logging

**Course:** ITPE3200 Web Applications  
**Project:** GamificationPlatform (.NET 10 / ASP.NET Core MVC / Entity Framework Core)  
**Verified branch:** `Salman-branch`  
**Last verified tests:** 70 passed, 0 failed, 0 skipped; GitHub Actions Release run [39695659659](https://github.com/Kenihski/GamificationPlatform/actions/runs/39695659659) on commit `5417429` (22 October 2026, Oslo time).

## Scope and requirements

Task 5 is **“Writing and coding of error handling, logging, and input validation, in close collaboration with others.”** The assessed functionality includes **“Forms and input validation (server-side)”** and **“Error handling and logging (server-side).”** The following sections describe the actual implementation after the group's October 2026 refactoring. The earlier Task 5 work and its tests are retained; new work on `Salman-branch` adapts it to ASP.NET Core Identity and the shared image service.

This document describes code and verifiable results; it does not claim undocumented team interactions or manual tests that have not been performed.

## 1. Input validation (server-side)

MVC Data Annotations, `ModelState`, controller-level business rules, and Identity provide complementary validation. The POST actions only save eligible data after their applicable checks succeed. Razor forms display errors through validation summaries and field messages.

| Area | Server-side rules and relevant files |
| --- | --- |
| Challenge forms | `Models/Challenge.cs`: required title (maximum 100 characters) and description (maximum 500); `AllowedTimeLimitAttribute` permits `null` or 10, 20, 30, 60, 90, 120 minutes. `ChallengeController` checks `ModelState`, image permissions, and conflicting upload/selection. |
| Challenge ownership and server-controlled fields | Create sets the logged-in user as owner, makes new challenges unpublished/community, and calculates points independently. Update preserves the existing owner. A user cannot modify someone else's challenge unless authorized as admin. |
| Question forms | `Models/Question.cs`: required title (maximum 150 characters), optional description (maximum 500) and explanation (maximum 1,000), and points between 1 and 100. `QuestionController` validates the question type and answer configuration. |
| Choice answers | Two to four filled answer options; up to four submitted fields, each maximum 200 characters. Filled options must be distinct after trim/case normalization. Correct-answer indexes must refer to filled options; single-choice questions must have exactly one correct answer. |
| Short answers | One to four accepted answers, maximum 200 characters each; duplicates after trim/case normalization are rejected. |
| Selected challenge/question images | Images are optional. `IImageService.IsAllowedImageAsync` checks whether a selected image is registered as a default image or belongs to the current user. On editing, retaining the existing image is permitted, including for authorized admin edits. The controllers intentionally remove the default `ImageUrl` model-state entry and then perform service-based checks. |
| Image uploads | `Services/ImageService.cs` verifies non-empty files, maximum 5 MiB, JPG/PNG/WebP extensions, and corresponding image-file signatures. It cleans up the written file if subsequent persistence fails. `UserImageController` also validates uploads; it currently implements some upload checks separately. |
| Registration | `RegisterViewModel` checks username length (3–50), email format/length, password length (6–100), confirmation, and username/password inequality. `AccountController` trims input and checks for existing username/email. ASP.NET Core Identity also enforces its configured password rules, normalized username uniqueness, and `RequireUniqueEmail = true`. |
| Login | `LoginViewModel` validates required credentials and username/email length; `AccountController` uses `SignInManager` for credential verification and lockout. Failed logins receive a generic message. |
| Quiz Submit/Exit | `QuizController` rejects invalid `ModelState`, nonpositive identifiers, ineligible/finished attempts, foreign question/option IDs, incorrect answer types, excessive selections for single-choice, and short answers over 200 characters. The server enforces attempt time limits. |
| Achievement administration | `AchievementAdminViewModel` and `AchievementController.ValidateAchievement` enforce required fields, name/description lengths, valid requirement values, approved achievement types, and permitted award points. |
| State-changing forms and redirects | State-changing POST actions use anti-forgery attributes; redirects based on return URLs accept local destinations only. |

An invalid question Create/Update request is rejected before altering the question or calling the history-reset operation. Invalid quiz submissions do not persist scored answers. These guarantees are checked in representative unit tests, not exhaustively in a live browser.

## 2. Error handling and authorization

`Program.cs` configures `UseDeveloperExceptionPage()` during Development and `UseExceptionHandler("/Home/Error")` outside Development. `UseStatusCodePagesWithReExecute("/Home/ErrorStatus", "?code={0}")` supplies friendly status pages. `HomeController` returns generic, nontechnical messages, appropriate HTTP status codes, and a request reference for diagnostic correlation.

| Situation | Application behavior |
| --- | --- |
| Invalid ordinary form | Re-display the form with field/summary messages; do not save invalid changes. |
| Missing record | Controller returns `NotFound()` (404) where appropriate. |
| Unauthenticated user accessing protected content | Identity challenges the request and normally redirects the browser to `/Account/Login`. |
| Authenticated non-admin denied protected content | Identity redirects to `/Account/AccessDenied`; the action now explicitly sets **HTTP 403 Forbidden** while showing the friendly view (`Controllers/AccountController.cs`, commit `e313ae8`). The redirect response itself can be 302. |
| Invalid quiz request | `BadRequest()` (400) before persisting an attempt. |
| Unexpected exception outside Development | Generic HTTP 500 response and request ID; technical exception details remain in logs. |
| Database failure | `ChallengeRepository`, `QuestionRepository`, `UserRepository`, and `AttemptRepository` log exceptions and rethrow with `throw;`, preserving original failures. Ordinary not-found results are not treated as database exceptions. |
| Development initialization failure | `DBInit` logs the exception and rethrows rather than reporting successful startup. |

### ASP.NET Core Identity migration

The group moved registration, login, logout, and access-denied handling to `Controllers/AccountController.cs`, using `UserManager<User>` and `SignInManager<User>`. `Controllers/UserController.cs` is now for administrator user views. Roles are evaluated via Identity, e.g. `User.IsInRole("Admin")`, not the previous `IsAdmin` property. Identity is configured in `Program.cs`, with authentication middleware before authorization.

`Authentication/ValidateUserCookieEvents.cs` is **legacy code that is not registered** by the current Identity configuration. It must not be described as actively rejecting invalid cookies. Identity provides its own security-stamp validation. Development startup currently recreates the database and seeded numeric user IDs, so immediate invalidation of previously issued, otherwise-valid cookies after a reset has not been independently verified. A proposed Development-only change to revalidate the security stamp on every request was **deliberately deferred and not committed**, because it is not necessary to satisfy the stated Task 5 assessment requirements. This residual edge case is not claimed as fully tested.

## 3. Logging

The application uses Serilog through injected `ILogger<T>`. `Program.cs` writes console output and a separate timestamped file in `Logs/` for each run, enriches events with a request identifier, and enables Serilog HTTP request logging. `appsettings.json` configures Information-level application logging and reduces framework verbosity.

| Component | Logged behavior / level |
| --- | --- |
| `ChallengeController` | Successful creation/changes/publication and rejected forms or management requests — Information / Warning. |
| `QuestionController` | Question creation/changes/deletion, attempt-history reset consequences, validation rejections, access denial — Information / Warning. |
| `QuizController` | Attempt start/completion, late attempts, invalid submissions, rejected access — Information / Warning. |
| `AccountController` | Successful registration/login/logout and failed registration/login — Information / Warning. |
| `AchievementController` | Achievement management success after database save — Information. |
| `AchievementService` | Unlock success **after** the relevant database save (regular achievements and goals have separate persistence phases) — Information. |
| `AchievementRepository` | No longer reports create/update/delete/unlock success merely from EF Core change tracking; notification-seen success is logged after its save. |
| Four original Task 5 DAL repositories | Query/save exceptions with exception details, repository/operation names, and relevant IDs when applicable — Error. |
| `DBInit` | Development initialization started/completed/failed — Information / Error. |
| HTTP request middleware and status handler | Method, path, HTTP status, duration, user/reference IDs; friendly error-status events — Request logging / Warning. |

**Logging correction — commit `bdc817e`:** EF Core's `AddAsync`, `Update`, and `Remove` stage changes rather than persisting them. Premature repository messages could falsely state that an achievement was created, updated, deleted, or unlocked even if `SaveChangesAsync` later failed. The correction removes those premature messages, retains post-save controller logs, and logs unlocks in the service after each successful persistence phase.

Application code does not deliberately log raw passwords, password hashes, authentication cookies, email form values, or complete submitted models. Exceptions retain technical details in server logs rather than being exposed in friendly error pages.

## 4. Database and current architecture

`ChallengeDbContext` inherits `IdentityDbContext<User, IdentityRole<int>, int>`. User management and authentication are integrated with Identity. The development database is deleted, migrated, and seeded on startup through `DBInit.cs` and the separated `DAL/Seeding/*Seeder.cs` files; this behavior is **Development-only**. Production does not automatically delete or seed the database. Apply migrations appropriately when deploying production instances.

Image selection and upload logic are shared through `IImageService` for challenge/question controllers. The older inline image-repository/environment dependencies were removed from those controller constructors; corresponding tests were updated rather than reverting the refactor.

## 5. Verification and limits

The existing GitHub Actions workflow, `.github/workflows/build.yml`, runs a Release application build followed by `dotnet test` for pushes to `Salman-branch` and `main`, pull requests to `main`, and manual dispatch.

```bash
dotnet build GamificationPlatform.csproj --configuration Release
dotnet test tests/GamificationPlatform.Tests/GamificationPlatform.Tests.csproj --configuration Release
```

**Verified CI results (22 October 2026, Oslo time):**

- Previous working implementation: **68/68 passing**, prior to the group's Identity/image-service refactoring.
- After adapting the five affected test files: **68/68 passing**, [GitHub Actions run 39695339329](https://github.com/Kenihski/GamificationPlatform/actions/runs/39695339329) (commit `a057b82`).
- After adding two focused regression tests: **70/70 passing, 0 failed, 0 skipped**, [GitHub Actions run 39695659659](https://github.com/Kenihski/GamificationPlatform/actions/runs/39695659659) (commit `5417429`). Application Release build also passed.

The unit tests cover Data Annotations, controller rejection/acceptance paths, malformed quiz answers, database save-failure propagation/logging, shared HTTP error actions, the AccessDenied HTTP 403 action, and absence of pre-save achievement-creation success logging. They use xUnit and NSubstitute and follow Arrange–Act–Assert principles; `MockQueryable.NSubstitute` remains a declared dependency but is no longer needed by the refactored image-service controller fixtures.

**Test coverage limits:** Direct controller unit tests do not execute the complete MVC request pipeline, Identity redirects/cookie behavior, authorization filters, anti-forgery middleware, Razor rendering, real file uploads, or end-to-end persistence. No new complete browser/integration verification is claimed. The HTTP 403 regression test checks the action's status code, not the entire Identity redirect flow; the achievement logging regression test checks staged repository operations, not every service save-failure scenario.

### Suggested manual demonstration (not recorded as completed)

1. Submit invalid challenge/question forms; confirm validation messages and no changes to saved data.
2. Verify normal-user/admin navigation and access denial (including the 302-to-403 flow); check a missing-page 404 and generic 500 display without leaking exception details.
3. Create/edit content successfully; inspect `Logs/` and correlate request identifiers while ensuring credentials are not logged.
4. Check invalid quiz answers, time limits, and achievement unlocks. If demonstrating a simulated database failure, use temporary test data rather than production records.

## 6. Task 5 changes on the updated Salman branch

The branch was first fast-forwarded to the group's latest `main` at `ad53114b`, without changing `main`. The following **four additional Task 5 commits** were then made:

| Commit | Commit message | Purpose |
| --- | --- | --- |
| [`e313ae8`](https://github.com/Kenihski/GamificationPlatform/commit/e313ae83814d77751873f31c77c19d470f840b2d) | `fix(task5): return HTTP 403 on access denied page` | Set the correct status on the Identity access-denied view. |
| [`bdc817e`](https://github.com/Kenihski/GamificationPlatform/commit/bdc817e203a6e4880f15516189f5d076f52e0918) | `fix(task5): log achievement success only after persistence` | Remove misleading pre-save logs; report unlock success after database saves. |
| [`a057b82`](https://github.com/Kenihski/GamificationPlatform/commit/a057b82eb43c3ab6427154d93212c720706d0eb3) | `test(task5): adapt existing tests to Identity and image service refactor` | Update five legacy test files for constructor and image-service changes; restore 68 passing tests. |
| [`5417429`](https://github.com/Kenihski/GamificationPlatform/commit/541742952f632d794e3a36b3d0484b2e69cfa94d) | `test(task5): cover HTTP 403 and prevent premature achievement logs` | Add two regression tests and correct the question-validation fixture; confirm 70 passing tests. |

This document is updated to reflect the current branch and verified workflow results. Neither the optional Identity-cookie configuration change nor a merge into `main` is part of this Task 5 completion work.

## 7. Course-material alignment and responsibility

The implementation uses server-side Data Annotations and `ModelState` (input-validation lectures/demos), `ILogger<T>`, Serilog, and exception handling (logging/error-handling lectures/demos), ASP.NET Core Identity and role authorization (security demo), and xUnit/NSubstitute Arrange–Act–Assert tests (unit-testing demo). GitHub Actions and certain mocking utilities are supporting implementation tools; do not present them as explicitly required course techniques.

The group should review agreed validation limits and ownership rules. Describe collaboration in the assessment using what the group actually did rather than inferring collaboration from repository code alone. Task 6 (AJAX communication) is a separate responsibility and is not asserted to be completed by this document.
