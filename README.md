# puebla-backend

## Introduction
  
REST API for [web application (movies)](https://www.github.com/fmadrian/movies) made with ASP.NET Core, and PostgreSQL.

Manages the following data: Movies, categories, and film studios.

Integrates Cloudinary to manage images related to movies, and SendGrid to send emails related to authentication (recover password and confirm account's emails).

  
## How to use

The following steps are mandatory for the application to function as intended.

### Prerequisites

1. A Cloudinary account with its cloud name, API key and API secret.
2. A SendGrid account with an API key and an authorized sender email.
3. Fill all environment variables defined in appsettings.json
 
### Setup

1. Apply all the database migrations.
```bash
 dotnet ef database update
```
2. **Start the application with the variable DbSettings__SeedData on.**
3. Close the application.
4. **Set the variable DbSettings__SeedData to off.**
5. Start the application again.

### Seeded data

**The data should only be seeded the first time the application is started.**
There is 1 administrator user, 10 other users, 25 categories, 25 film studios, and 105 movies.

#### Default credentials for administrator user

```
username: admin
password: Admin#123
```

#### Default credentials for other users

The entire list of users can be checked in the application.

```
username: johndoe
password: User#123
```