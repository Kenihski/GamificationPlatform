# Gamification Platform for Course Learning

## ITPE3200 Web Applications – Basic Project 2026H

Gamification Platform is an ASP.NET Core MVC web application developed as part of the ITPE3200 Web Applications course.

The purpose of the application is to turn course content into interactive challenges and quizzes. Users can create challenges, test their knowledge, receive scores, view previous attempts, and compare results through leaderboards.

This version represents the Minimum Viable Product (MVP) of the application and will be further developed for the Exam Group Project.

---

## Test Accounts

The application contains several dummy users that can be used to test the functionality of the application.

All dummy users use the same password:

```text
Password123!
```

| Username | Email               | Role          |
| -------- | ------------------- | ------------- |
| admin    | admin@example.com   | Administrator |
| Alice    | alice@example.com   | User          |
| Bob      | bob@example.com     | User          |
| Charlie  | charlie@example.com | User          |
| Emma     | emma@example.com    | User          |
| David    | david@example.com   | User          |

### Testing as a User

The normal user accounts can be used to test functionality such as:

- Logging in
- Viewing published challenges
- Taking quizzes
- Viewing scores and previous attempts
- Viewing leaderboards
- Creating challenges
- Managing their own challenges and questions
- Publishing and unpublishing their own challenges

### Testing as an Administrator

The administrator account can additionally be used to test:

- Viewing all users
- Viewing individual user details
- Viewing users' challenge history and completed attempts
- Managing challenges created by other users

The database is seeded with challenges, questions, attempts, and other test data so that the main functionality can be tested immediately after starting the application.

> The accounts above are dummy accounts created for development and testing only.

---

## Technologies

The application is built using:

- .NET 10.0
- ASP.NET Core MVC
- Entity Framework Core
- SQLite
- Bootstrap
- HTML
- CSS
- JavaScript

### Development Environment

- .NET 10.0
- Node.js v24.19.0

Node.js v24.19.0 was installed in the development environment.

---

## Running the Application

### Requirements

Make sure the following is installed:

- .NET 10.0 SDK

### Start the Application

Open a terminal in the project directory.

Restore the project dependencies:

```bash
dotnet restore
```

Start the application:

```bash
dotnet run
```

The terminal will display the local URL where the application is running.

Open this URL in a web browser.

The dummy accounts listed above can then be used to test the application's functionality.

---

## Main Features

The current version includes:

- User registration and login
- User and administrator roles
- Challenge creation, editing, and deletion
- Question creation, editing, and deletion
- Multiple-choice questions
- Draft and published challenges
- Challenge ownership
- Challenge time limits
- Automatic quiz scoring
- Challenge attempts and attempt history
- Leaderboards
- Grid and table views for challenges
- Dynamic Challenge of the Day
- Server-side authorization
- Database storage using Entity Framework Core and SQLite

---

## User Roles

The application supports two user roles.

### User

A normal user can:

- Register and log in
- View published challenges
- Complete published challenges
- View their previous attempts
- View leaderboards
- Create their own challenges
- Add and manage questions in their challenges
- Edit and delete their own challenges
- Publish and unpublish their own challenges

### Administrator

An administrator has additional access and can:

- View all users
- View user details
- View users' challenge history
- View completed attempts
- Manage challenges created by other users

---

## Project Structure

The application follows the Model-View-Controller (MVC) architecture.

### Models

The main models include:

- `Challenge`
- `Question`
- `QuestionOption`
- `User`
- `UserChallenge`
- `ChallengeAttempt`
- `AttemptAnswer`

### Views

Razor Views are used to display the user interface.

The application includes views for challenges, quizzes, questions, users, leaderboards, authentication, and the home page.

### Controllers

The main controllers include:

- `ChallengeController` – challenge management
- `QuizController` – taking quizzes, submitting answers, and viewing attempts
- `LeaderboardController` – challenge leaderboards
- `QuestionController` – question management
- `UserController` – users, login, and registration
- `HomeController` – home page content

---

## Database

The application uses SQLite together with Entity Framework Core.

The database contains relationships between users, challenges, questions, answer options, challenge attempts, and submitted answers.

The development database is seeded with test data so that the application's functionality can be demonstrated without manually creating data.

### Database Seeding

The current development version recreates and seeds the database when the application starts in the Development environment.

This is intentional for the current MVP version and ensures that the same test data is available when the application is started.

---

## Validation, Error Handling and Logging

The application validates forms and quiz submissions on the server before saving data. Validation covers required fields, lengths, question points, allowed images/time limits, answer options, correct-answer selection, and duplicate usernames. Authentication cookies are checked against the current user database so a cookie for a deleted account is rejected before a protected database operation.

Expected errors receive form messages or friendly HTTP 400/401/403/404 pages. Unexpected failures outside Development receive a generic HTTP 500 page with a request reference. DAL exceptions are logged and rethrown for upstream handling.

Serilog records controller events, database failures, initialization events, and HTTP requests in the console and per-run files under `Logs/`. Logs include request references and do not deliberately record submitted credentials or entire models.

See [Task 5 implementation summary](docs/Task5.md) for the complete rules, the DAL logging added in this chat, database setup, and verification instructions. GitHub Actions builds the application and runs isolated HTTP/database checks on `Salman-branch`.

---

## Code Structure

The project is structured according to the MVC pattern and separates responsibilities into models, views, controllers, view models, validation attributes, and DAL repositories.

Quiz functionality and leaderboard functionality are separated into their own controllers to keep the challenge management code more modular and easier to maintain.

JavaScript functionality is stored in separate JavaScript files instead of being placed directly inside Razor Views.

Comments in the source code are written in English.

---

## External Code and Inspiration

The project was developed as part of the ITPE3200 Web Applications course.

Course examples, documentation, and external resources have been used as references and inspiration during development.

AI tools have been used as assistance for explanations, debugging, code suggestions, and refactoring during development.

All code has been reviewed and adapted to the structure and requirements of this project.

---

## Project Status

This is the Basic Project / MVP version of the application.

The current application demonstrates the main concept and core functionality, including creating and managing challenges, completing quizzes, calculating scores, storing attempts, displaying history, and displaying leaderboards.

The application is intended to be further developed for the final Exam Group Project.

A Data Access Layer (DAL) and the Repository Pattern are implemented. Controllers use injected repository interfaces for database access, and the repositories use Entity Framework Core for asynchronous queries and persistence.

Additional functionality and improvements to the existing features are also planned for the Exam Group Project. The current authentication and user management solution may also be further developed, including evaluating the use of ASP.NET Core Identity.

The architecture and existing functionality may therefore be adjusted as the project develops toward the final Exam Group Project.

