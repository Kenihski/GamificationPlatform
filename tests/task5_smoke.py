"""Exercise Task 5 through real HTTP requests and an isolated SQLite database.

Run after `dotnet build --configuration Release`. No Python packages are needed.
The database and log files live in a temporary directory; project data is untouched.
"""
from pathlib import Path
import contextlib
import html
import http.cookiejar
import os
import re
import shutil
import socket
import sqlite3
import subprocess
import tempfile
import time
import urllib.error
import urllib.parse
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
DLL = ROOT / "bin/Release/net10.0/GamificationPlatform.dll"
CHECKS = 0


def check(condition, label):
    global CHECKS
    if not condition:
        raise AssertionError(label)
    CHECKS += 1
    print(f"PASS: {label}", flush=True)


class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None


class Client:
    def __init__(self, base):
        self.base = base
        self.opener = urllib.request.build_opener(
            urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()), NoRedirect())

    def request(self, path, fields=None):
        data = None if fields is None else urllib.parse.urlencode(fields).encode()
        request = urllib.request.Request(self.base + path, data=data)
        try:
            response = self.opener.open(request, timeout=10)
        except urllib.error.HTTPError as error:
            response = error
        with response:
            return response.code, response.read().decode(), response.headers

    def post(self, path, fields, token_page=None):
        if token_page:
            status, body, _ = self.request(token_page)
            assert status == 200, (token_page, status)
            token = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', body)
            assert token, token_page
            fields = dict(fields, __RequestVerificationToken=html.unescape(token.group(1)))
        return self.request(path, fields)


def scalar(database, sql, parameters=()):
    with sqlite3.connect(database) as connection:
        return connection.execute(sql, parameters).fetchone()[0]


@contextlib.contextmanager
def server(directory, database, environment):
    with socket.socket() as listener:
        listener.bind(("127.0.0.1", 0))
        port = listener.getsockname()[1]
    base = f"http://127.0.0.1:{port}"
    env = dict(os.environ, ASPNETCORE_ENVIRONMENT=environment,
               ASPNETCORE_URLS=base, ASPNETCORE_CONTENTROOT=str(ROOT),
               ConnectionStrings__ChallengeDbContextConnection=f"Data Source={database}",
               DOTNET_NOLOGO="true")
    output_path = directory / f"server-{environment}.txt"
    with output_path.open("w") as output:
        process = subprocess.Popen([shutil.which("dotnet") or "dotnet", str(DLL)],
                                   cwd=directory, env=env, stdout=output, stderr=subprocess.STDOUT)
        try:
            client = Client(base)
            for _ in range(80):
                if process.poll() is not None:
                    raise RuntimeError(output_path.read_text())
                try:
                    if client.request("/User/Login")[0] == 200:
                        break
                except (OSError, urllib.error.URLError):
                    pass
                time.sleep(0.25)
            else:
                raise RuntimeError("Application startup timed out.\n" + output_path.read_text())
            yield client
        finally:
            process.terminate()
            try:
                process.wait(timeout=10)
            except subprocess.TimeoutExpired:
                process.kill()
                process.wait()


def main():
    if not DLL.exists():
        raise RuntimeError("Build the application in Release configuration first.")
    with tempfile.TemporaryDirectory(prefix="task5-checks-") as temporary:
        directory = Path(temporary)
        database = directory / "test.db"
        password = "SmokePass123!"
        registration = dict(Username="SmokeUser", Email="smoke@example.test",
                            Password=password, ConfirmPassword=password)
        with server(directory, database, "Development") as client:
            check(scalar(database, 'SELECT COUNT(*) FROM "__EFMigrationsHistory"') == 10,
                  "All migrations, including unique usernames, applied to a fresh database")
            status, body, _ = client.request("/missing-task5-page")
            check(status == 404 and "Page not found" in body, "Friendly 404 preserves its HTTP status")
            check(client.request("/User/Table")[0] == 302, "Anonymous admin-page access redirects to login")
            check(client.post("/User/Register", registration)[0] == 400,
                  "Missing anti-forgery token is rejected")
            bad = dict(registration, Username="a", Email="invalid", ConfirmPassword="mismatch")
            status, body, _ = client.post("/User/Register", bad, "/User/Register")
            check(status == 200 and "Username must be" in body and "valid email" in body and
                  "Passwords do not match" in body, "Invalid registration displays field errors")
            same = dict(registration, Username="SamePass", Password="SamePass", ConfirmPassword="SamePass")
            check("cannot be the same" in client.post("/User/Register", same, "/User/Register")[1],
                  "Username and password cannot match")
            check(client.post("/User/Register", registration, "/User/Register")[0] == 302,
                  "Valid registration succeeds")
            user_id = scalar(database, "SELECT UserId FROM Users WHERE Username='SmokeUser'")
            duplicates = dict(registration, Username="  smokeuser  ")
            status, body, _ = client.post("/User/Register", duplicates, "/User/Register")
            check(status == 200 and "already taken" in body and
                  scalar(database, "SELECT COUNT(*) FROM Users WHERE Username='SmokeUser'") == 1,
                  "Duplicate usernames are rejected after trimming and case-insensitive lookup")
            with sqlite3.connect(database) as connection:
                try:
                    connection.execute("INSERT INTO Users (Username,Email,PasswordHash,IsAdmin) VALUES (?,?,?,0)",
                                       ("SMOKEUSER", "duplicate@example.test", "test-only"))
                except sqlite3.IntegrityError:
                    unique = True
                else:
                    unique = False
            check(unique, "Database unique index also prevents competing duplicate registrations")
            status, body, _ = client.post("/User/Login", dict(Username="SmokeUser", Password="wrong"), "/User/Login")
            check(status == 200 and "Invalid username or password" in body, "Invalid login has a generic message")
            status, _, headers = client.post("/User/Login", dict(Username="SMOKEUSER", Password=password,
                                                  ReturnUrl="https://invalid.example/"), "/User/Login")
            check(status == 302 and headers["Location"] == "/Challenge/Grid",
                  "Login accepts case variants and rejects external return URLs")
            status, body, _ = client.request("/User/Table")
            check(status == 403 and "Access denied" in body, "Normal user receives a friendly admin-access denial")
            challenge = dict(Title="Task 5 smoke challenge", Description="Smoke test", ImageUrl="/images/Question_1.png",
                             TimeLimitMinutes="", CreatedByUserId="1", IsPublished="true", MaxPoints="9999")
            for limit in ("-10", "0", "5", "9999", "invalid"):
                status, body, _ = client.post("/Challenge/Create", dict(challenge, TimeLimitMinutes=limit), "/Challenge/Create")
                check(status == 200 and scalar(database, "SELECT COUNT(*) FROM Challenges WHERE Title=?",
                                              (challenge["Title"],)) == 0, f"Invalid time limit {limit} is not saved")
            check(client.post("/Challenge/Create", dict(challenge, ImageUrl="https://invalid.example/image.png"),
                              "/Challenge/Create")[0] == 200 and
                  scalar(database, "SELECT COUNT(*) FROM Challenges WHERE Title=?", (challenge["Title"],)) == 0,
                  "Unlisted image URL is rejected on the server")
            check(client.post("/Challenge/Create", dict(challenge, Title="", Description="x" * 501),
                              "/Challenge/Create")[0] == 200, "Required and maximum-length challenge fields are validated")
            check(client.post("/Challenge/Create", challenge, "/Challenge/Create")[0] == 302,
                  "Valid challenge creation succeeds")
            challenge_id = scalar(database, "SELECT ChallengeId FROM Challenges WHERE Title=?", (challenge["Title"],))
            check(scalar(database, "SELECT CreatedByUserId FROM Challenges WHERE ChallengeId=?", (challenge_id,)) == user_id and
                  scalar(database, "SELECT IsPublished FROM Challenges WHERE ChallengeId=?", (challenge_id,)) == 0,
                  "Posted ownership and publication fields cannot override server values")
            client.post(f"/Challenge/Publish/{challenge_id}", {}, f"/Challenge/Details/{challenge_id}")
            check(scalar(database, "SELECT IsPublished FROM Challenges WHERE ChallengeId=?", (challenge_id,)) == 0,
                  "Empty challenge cannot be published")
            question = {"Question.Title": "Smoke question", "Question.Description": "Choose A", "Question.Points": "10",
                        "Question.ImageUrl": "", "Question.ChallengeId": str(challenge_id),
                        "Options[0]": "A", "Options[1]": "B", "Options[2]": "", "Options[3]": "", "CorrectOption": "0"}
            page = f"/Question/Create?challengeId={challenge_id}"
            status, body, _ = client.request(page)
            required_description = re.search(
                r'<label[^>]*for="Question_Description"[^>]*>\s*Description\s*</label>\s*'
                r'<span class="text-danger">\*</span>',
                body)
            check(status == 200 and required_description is not None,
                  "Question description is visibly marked as required")
            invalid_questions = [
                (dict(question, **{"Question.Points": "0"}), "Question points outside the allowed range"),
                ({k: v for k, v in question.items() if k != "CorrectOption"}, "Missing correct-answer selection"),
                (dict(question, **{"Options[1]": " a "}), "Duplicate answer options"),
                (dict(question, **{"Options[0]": "x" * 201}), "Excessively long answer text"),
                (dict(question, **{"Options[4]": "Extra"}), "More than four answer fields"),
                (dict(question, **{"Options[1]": ""}), "Fewer than two filled answers"),
                (dict(question, CorrectOption="3"), "An empty selected correct answer"),
            ]
            for fields, label in invalid_questions:
                status, _, _ = client.post("/Question/Create", fields, page)
                check(status == 200 and scalar(database, "SELECT COUNT(*) FROM Questions WHERE ChallengeId=?", (challenge_id,)) == 0,
                      label + " is rejected without saving")
            for title in ("Smoke question one", "Smoke question two"):
                check(client.post("/Question/Create", dict(question, **{"Question.Title": title}), page)[0] == 302,
                      title + " is created")
            with sqlite3.connect(database) as connection:
                questions = connection.execute("SELECT QuestionId FROM Questions WHERE ChallengeId=? ORDER BY QuestionId",
                                               (challenge_id,)).fetchall()
                q1, q2 = (row[0] for row in questions)
                option1 = connection.execute("SELECT QuestionOptionId FROM QuestionOptions WHERE QuestionId=? AND IsCorrect=1", (q1,)).fetchone()[0]
                option2 = connection.execute("SELECT QuestionOptionId FROM QuestionOptions WHERE QuestionId=? AND IsCorrect=1", (q2,)).fetchone()[0]
                foreign_challenge = connection.execute("SELECT ChallengeId FROM Challenges WHERE CreatedByUserId != ? LIMIT 1", (user_id,)).fetchone()[0]
            check(client.request(f"/Challenge/Update/{foreign_challenge}")[0] == 403,
                  "A normal user cannot edit another user's challenge")
            client.post(f"/Challenge/Publish/{challenge_id}", {}, f"/Challenge/Details/{challenge_id}")
            check(scalar(database, "SELECT IsPublished FROM Challenges WHERE ChallengeId=?", (challenge_id,)) == 1,
                  "A challenge with questions can be published")
            take = f"/Quiz/Take/{challenge_id}"
            check(client.request(take)[0] == 200, "Published challenge starts an attempt")
            attempt = scalar(database, "SELECT MAX(ChallengeAttemptId) FROM ChallengeAttempts")
            submission = dict(challengeId=challenge_id, challengeAttemptId=attempt)
            for fields, label in [(dict(submission, **{f"answers[{q1}]": option2}), "Option belonging to a different question"),
                                  (dict(submission, **{f"answers[{q1}]": 999999}), "Nonexistent option"),
                                  (dict(submission, **{"answers[999999]": option1}), "Question outside the challenge"),
                                  (dict(submission, **{f"answers[{q1}]": "invalid"}), "Malformed answer ID")]:
                status, body, _ = client.post("/Quiz/Submit", fields, take)
                check(status == 400 and "Invalid request" in body and
                      scalar(database, "SELECT Completed FROM ChallengeAttempts WHERE ChallengeAttemptId=?", (attempt,)) == 0 and
                      scalar(database, "SELECT COUNT(*) FROM AttemptAnswers WHERE ChallengeAttemptId=?", (attempt,)) == 0,
                      label + " is rejected without completing the attempt")
            status, _, _ = client.post("/Quiz/Submit", dict(submission, **{f"answers[{q1}]": option1}), take)
            check(status == 200 and scalar(database, "SELECT Score FROM ChallengeAttempts WHERE ChallengeAttemptId=?", (attempt,)) == 10,
                  "Server calculates score and gives unanswered questions zero points")
            repeat_status, repeat_body, _ = client.post("/Quiz/Submit", submission, take)
            check(repeat_status == 404,
                  f"Completed attempt cannot be submitted again (HTTP {repeat_status}; {repeat_body[:300]})")
            exit_attempt = scalar(database, "SELECT MAX(ChallengeAttemptId) FROM ChallengeAttempts")
            client.post("/Quiz/Exit", dict(challengeId=challenge_id, challengeAttemptId=exit_attempt,
                                         **{f"answers[{q1}]": option1}), take)
            check(scalar(database, "SELECT Completed FROM ChallengeAttempts WHERE ChallengeAttemptId=?", (exit_attempt,)) == 1,
                  "Exit Challenge completes the attempt")
            client.request(take)
            unanswered_attempt = scalar(database, "SELECT MAX(ChallengeAttemptId) FROM ChallengeAttempts")
            unanswered_status, _, _ = client.post("/Quiz/Submit", dict(challengeId=challenge_id, challengeAttemptId=unanswered_attempt), take)
            check(unanswered_status == 200 and scalar(database, "SELECT Score FROM ChallengeAttempts WHERE ChallengeAttemptId=?", (unanswered_attempt,)) == 0 and
                  scalar(database, "SELECT Completed FROM ChallengeAttempts WHERE ChallengeAttemptId=?", (unanswered_attempt,)) == 1,
                  "A quiz with no selected answers can be submitted with zero points")
            update_page = f"/Challenge/Update/{challenge_id}"
            update = dict(challenge, ChallengeId=challenge_id, TimeLimitMinutes="0")
            check(client.post("/Challenge/Update", update, update_page)[0] == 200 and
                  scalar(database, "SELECT TimeLimitMinutes FROM Challenges WHERE ChallengeId=?", (challenge_id,)) is None,
                  "Invalid time limit is also rejected during editing")
            check(client.post("/Challenge/Update", dict(update, TimeLimitMinutes="10"), update_page)[0] == 302,
                  "Valid challenge time limit can be saved")
            client.request(take)
            timed_attempt = scalar(database, "SELECT MAX(ChallengeAttemptId) FROM ChallengeAttempts")
            with sqlite3.connect(database) as connection:
                connection.execute("UPDATE ChallengeAttempts SET StartedAt=datetime('now','-20 minutes') WHERE ChallengeAttemptId=?", (timed_attempt,))
            client.post("/Quiz/Submit", dict(challengeId=challenge_id, challengeAttemptId=timed_attempt,
                                            **{f"answers[{q1}]": option1}), take)
            check(scalar(database, "SELECT Score FROM ChallengeAttempts WHERE ChallengeAttemptId=?", (timed_attempt,)) == 0,
                  "Server enforces the quiz time limit independently of JavaScript")
            invalid_update = dict(question, **{"Question.QuestionId": str(q1), "Options[1]": "A"})
            client.post("/Question/Update", invalid_update, f"/Question/Update/{q1}")
            check(scalar(database, "SELECT COUNT(*) FROM ChallengeAttempts WHERE UserChallengeId IN (SELECT UserChallengeId FROM UserChallenges WHERE ChallengeId=?)", (challenge_id,)) > 0,
                  "Invalid question edit preserves existing attempt history")
            check(client.post("/Question/Update", dict(question, **{"Question.QuestionId": str(q1), "Question.Title": "Edited smoke question"}),
                              f"/Question/Update/{q1}")[0] == 302 and
                  scalar(database, "SELECT COUNT(*) FROM ChallengeAttempts WHERE UserChallengeId IN (SELECT UserChallengeId FROM UserChallenges WHERE ChallengeId=?)", (challenge_id,)) == 0,
                  "Valid question edit resets previous attempt history")
            for question_id in (q1, q2):
                check(client.post(f"/Question/DeleteConfirmed/{question_id}", {}, f"/Question/Delete/{question_id}")[0] == 302,
                      "Question deletion succeeds")
            check(scalar(database, "SELECT IsPublished FROM Challenges WHERE ChallengeId=?", (challenge_id,)) == 0,
                  "Deleting the last question automatically unpublishes the challenge")
            check(client.post("/User/Logout", {}, "/")[0] == 302 and client.request("/Challenge/MyChallenges")[0] == 302,
                  "Logout removes authenticated access")

            # A database reset or account deletion can leave a valid browser
            # cookie pointing to a user that no longer exists.
            stale_client = Client(client.base)
            stale_registration = dict(registration, Username="StaleCookieUser",
                                      Email="stale-cookie@example.test")
            check(stale_client.post("/User/Register", stale_registration, "/User/Register")[0] == 302 and
                  stale_client.post("/User/Login", dict(Username="StaleCookieUser", Password=password),
                                    "/User/Login")[0] == 302,
                  "Stale-cookie test account can authenticate")
            status, stale_form, _ = stale_client.request("/Challenge/Create")
            stale_token = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', stale_form)
            assert status == 200 and stale_token
            stale_user_id = scalar(database, "SELECT UserId FROM Users WHERE Username='StaleCookieUser'")
            with sqlite3.connect(database) as connection:
                connection.execute("DELETE FROM Users WHERE UserId=?", (stale_user_id,))
            stale_title = "Stale cookie challenge"
            stale_challenge = dict(challenge, Title=stale_title,
                                   __RequestVerificationToken=html.unescape(stale_token.group(1)))
            status, _, headers = stale_client.request("/Challenge/Create", stale_challenge)
            login_redirect = urllib.parse.urlparse(headers["Location"]).path
            check(status == 302 and login_redirect == "/User/Login" and
                  scalar(database, "SELECT COUNT(*) FROM Challenges WHERE Title=?", (stale_title,)) == 0,
                  "Cookie for a deleted user is rejected before a protected database operation")

        # Production preserves the isolated migrated database and exercises both
        # save and read failures through the friendly global exception handler.
        with server(directory, database, "Production") as client:
            with sqlite3.connect(database) as connection:
                connection.execute("CREATE TRIGGER simulate_save_failure BEFORE INSERT ON Users BEGIN SELECT RAISE(ABORT,'simulated save failure'); END")
            status, body, _ = client.post("/User/Register", dict(registration, Username="SaveFailure"), "/User/Register")
            check(status == 500 and "We could not complete your request" in body and "Reference:" in body and
                  "simulated save failure" not in body and "SqliteException" not in body,
                  "Production save failure returns a friendly 500 without technical details")
            check(scalar(database, "SELECT COUNT(*) FROM Users WHERE Username='SaveFailure'") == 0,
                  "Failed database save does not persist the user")
            with sqlite3.connect(database) as connection:
                connection.execute("DROP TABLE Challenges")
            status, body, _ = client.request("/Challenge/Grid")
            check(status == 500 and "We could not complete your request" in body and "no such table" not in body,
                  "Production read failure returns a friendly 500")

        logs = "\n".join(path.read_text() for path in (directory / "Logs").glob("*.txt"))
        for message in ("UserRepository", "SaveChangesAsync", "ChallengeRepository", "GetPublishedChallengesAsync",
                        "registered successfully", "logged in successfully", "logged out successfully",
                        "created question", "updated question", "deleted question", "automatically unpublished",
                        "Authentication cookie rejected", "HTTP", "denied", "initialization completed"):
            check(message in logs, f"Logs include {message}")
        check(password not in logs and registration["Email"] not in logs,
              "Logs do not include submitted passwords or email values")
        print(f"Task 5 checks passed: {CHECKS}", flush=True)


if __name__ == "__main__":
    main()
