# Task 5: Error handling, logging, and input validation

Task 5 covers writing and coding error handling, logging, and input validation in collaboration with the group. This document describes the implementation in `Salman-branch`, including the DAL logging added earlier in this chat. It is also a checklist for explaining and demonstrating the work.

## Input validation

Validation runs on the server before data is saved. MVC validates data annotations and the controllers inspect `ModelState`. Invalid form input is returned to the form with understandable validation messages.

| Input or operation | Server-side rule |
| --- | --- |
| Challenge title and description | Required; maximum 100 and 500 characters respectively |
| Challenge time limit | Unlimited (`null`) or 10, 20, 30, 60, 90, or 120 minutes, matching the form dropdown |
| Challenge image | Required and must identify one of the 13 bundled images |
| Question title and description | Required; maximum 150 and 500 characters respectively |
| Question points | Between 1 and 100 |
| Question image | Optional; if supplied, must identify one of the bundled images |
| Answer options | Between 2 and 4 filled options, at most four submitted fields; maximum 200 characters per filled answer; distinct after trimming and ignoring case |
| Correct answer | Explicit selection required; must refer to a filled answer field |
| Registration | Username 3–50 characters, email format and maximum length, password length, matching confirmation, and username/password comparison |
| Username uniqueness | Registration trims the username and checks for an existing account. A SQLite unique index prevents competing registrations from creating duplicates. `NOCASE` treats ASCII case variants such as `Alice` and `alice` as the same username |
| Login | Required username/password, username length limit, password verification, generic invalid-credentials message |
| Quiz submission and exit | Valid model binding and positive IDs; the attempt must belong to the authenticated user and challenge and be unfinished. Every submitted question/option pair must belong to that challenge question |
| Time-limited attempts | Deadline checked on the server, with the existing five-second submission allowance. Late answers receive no points |
| Ownership and stored fields | Owners/admins are checked on the server. Creation copies editable fields into a new entity, so submitted ownership, publication state, IDs, or nested entity graphs cannot set server-controlled values |
| Return URLs | Redirects use local URLs only |
| POST forms | Anti-forgery tokens protect state-changing requests |

Email addresses are validated for format; they are not account identifiers and are not required to be unique. SQLite's built-in `NOCASE` collation covers ASCII case folding. Existing usernames may contain other Unicode characters, whose different case variants are not merged by this collation.

Invalid quiz answers are rejected before any answers are added or an attempt is completed. A corrected submission can reuse the unfinished attempt. Editing a question with invalid input leaves its previous questions, scores, and attempt history intact.

## Error handling

| Situation | Application behavior |
| --- | --- |
| Invalid ordinary form | Re-displays the form with field/summary messages |
| Missing resource | Returns HTTP 404 with the shared error page |
| Authentication required | Cookie authentication redirects to login, or returns HTTP 401 where appropriate |
| Cookie references a deleted user | Cookie validation checks the current database user, rejects the stale principal, expires the cookie, and redirects protected requests to login before a database write can occur |
| Forbidden action | Returns HTTP 403 with an understandable access-denied page |
| Malformed quiz request or missing anti-forgery token | Returns HTTP 400 with the shared error page |
| Failed login | Generic “Invalid username or password” message |
| Duplicate registration | Field error explaining that the username is taken; a competing unique-constraint failure is also handled |
| Publishing an empty challenge | Rejects publication and displays a useful message |
| Deleting the last published question | Warns before deletion, unpublishes the challenge, and displays a success message |
| Unexpected exception outside development | `UseExceptionHandler` renders a generic HTTP 500 page containing a request reference, without exposing SQL, stack traces, or exception messages |
| Unexpected exception during development | Developer exception page supports debugging |
| Database query/save failure | Repository logs the exception and rethrows with `throw;`, preserving its stack trace for upstream handling |
| Development initialization failure | Seeding logs the failure and stops startup; it does not claim that initialization succeeded |

Error pages are available to anonymous users, do not query the database, and are not cached. Status-code re-execution retains the original HTTP status. Question history deletion, last-question unpublishing, and question deletion are saved together through the shared scoped DbContext.

Cookie validation is centralized in `Authentication/ValidateUserCookieEvents.cs`. A signed cookie can remain in a browser after the development database is recreated, or after an account is deleted. Without validation, the cookie's old user ID can pass `[Authorize]` and later cause a foreign-key failure when a new challenge or other owned record is saved. The validator looks up the numeric user ID through `IUserRepository`; a missing or malformed identity is rejected and signed out before controller actions run. Centralizing this rule protects every authenticated controller instead of duplicating user-existence checks in individual actions.

## Logging, including the DAL work from this chat

Serilog receives injected `ILogger<T>` events and writes them to the console and a separate file under `Logs/` for each application run. These generated files remain ignored by Git.

| Layer | Logged information | Level |
| --- | --- | --- |
| Challenge controller | Creation, update, deletion, publication/unpublication; invalid forms, empty publication, and denied management actions | Information / Warning |
| Question controller | Creation, update, deletion, reset of previous attempts/scores, and automatic unpublishing; invalid forms and denied management actions | Information / Warning |
| User controller | Registration, successful login/logout, failed login, duplicate registration, and invalid forms | Information / Warning |
| Quiz controller | Start/completion, explicit exit, invalid attempts, invalid question/option IDs, late submissions, and denied attempt access | Information / Warning |
| Cookie validation | Rejection of cookies with a malformed user ID or an account that no longer exists | Warning |
| Four DAL repositories | Failed reads and saves, with the exception, repository, operation name, and relevant numeric IDs for reads | Error |
| DBInit | Development initialization start/completion and initialization failure | Information / Error |
| Request middleware | HTTP method, path, response status, duration, and diagnostic user/request IDs | Information / Error |
| Shared status handler | Request reference, original path, status, and user ID for bare error responses | Warning |

The earlier DAL logging commit added typed loggers and `try/catch` around database execution in `ChallengeRepository`, `QuestionRepository`, `UserRepository`, and `AttemptRepository`. Save failures are logged inside `SaveChangesAsync`; their create/update/delete callers do not catch and log that same exception again. Normal “not found” results are not treated as database exceptions, and cancelled operations are not logged as DAL errors.

The log output includes the source category and request ID. The same reference appears on error pages, allowing the team to find the corresponding request logs. Passwords, password hashes, email input values, authentication cookies, and entire submitted models are not deliberately logged; EF sensitive-data logging remains disabled.

Successful user actions belong in controllers, while database failures belong in repositories. Interfaces declare operations and do not execute or log them. EF Core and exception middleware may also produce diagnostics for the same failure; the error-page action does not add another application exception log.

## Database setup

The `AddUniqueUsernames` migration adds the username collation and unique index. Development initialization still recreates and seeds the dummy database, as it did before this work, but now applies migrations to build the schema. Development data is therefore reset on startup. Browser cookies can survive that reset, so cookie validation signs out identities whose database account disappeared instead of allowing a later foreign-key failure.

Production does not delete, seed, or automatically migrate databases. Apply migrations as a separate setup step before using an existing production database. Back up data first. Case-colliding usernames must be resolved before adding the unique index; migration failure must not be bypassed by silently discarding accounts.

An older database created with `EnsureCreated` has no migration history and cannot simply be upgraded by running the initial migrations over existing tables. For disposable MVP data, start in Development to recreate it. For data that must be preserved, plan a migration baseline with the group before applying migrations. The checked-in MVP database is not rewritten by this Task 5 code change.

## Verification and demonstration

GitHub Actions runs `.github/workflows/task5-checks.yml` on pushes to `Salman-branch` and pull requests to `main`. The workflow builds the application and runs `tests/task5_smoke.py` against an isolated temporary database. The latest workflow result is the source of truth for whether the checked commit passed.

Run the same checks locally:

```bash
dotnet build GamificationPlatform.csproj --configuration Release
python tests/task5_smoke.py
```

The smoke checks exercise real HTTP requests, MVC binding/validation, anti-forgery tokens, cookies, authorization, migrations, database uniqueness, invalid quiz option IDs, score calculation, timeout handling, question history resets, automatic unpublishing, production database read/save failures, and the resulting log files. They delete an authenticated test user directly from the isolated database and confirm that the remaining browser cookie is rejected before a protected operation can run. They also check that invalid quiz submissions do not change the attempt, browser error pages hide technical details, and the test password and email do not appear in logs.

For a group demonstration, show an invalid form and its message, a denied action, a valid question change and its log, and an isolated database failure with the HTTP 500 page and matching request reference. Use temporary test data for failure demonstrations.

## Responsibility and collaboration

The pre-existing validation and challenge/quiz logs, the DAL logging commit from this chat, and this completion pass together form the Task 5 implementation. The group should review the shared field limits and business rules documented above. Repository code cannot establish how work was coordinated between group members; describe that collaboration in the assignment report using the team's actual process.

Task 6 (AJAX communication) is a separate deliverable. The JavaScript form behavior and image previews should not be described as completing Task 6 unless they actually make the required asynchronous server requests.
