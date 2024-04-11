<div align="center">

# Blame Sight - Project Description

<br>
  
[![Commits](https://img.shields.io/github/commit-activity/w/GitBlameGame/BlameSight)](https://github.com/GitBlameGame/BlameSight/activity)
[![CI/CD](https://github.com/GitBlameGame/BlameSight/actions/workflows/prod-ci-cd.yaml/badge.svg)](https://github.com/GitBlameGame/BlameSight/actions/workflows/prod-ci-cd.yaml)

</div>

Introducing "CodeBlame" - the app where coworkers can point fingers and throw shade over bad code, all in the spirit of camaraderie! With its seamless integration with GitHub, CodeBlame makes assigning fault as easy as pushing code.

CodeBlame isn't just about pointing fingers; it's about fostering a culture of constructive criticism and banter. So, next time you encounter spaghetti code or a function named "tempFuncForNowSeriouslyDontUseThis," fire up CodeBlame and let the blaming begin!


## Project Resources:

[![Documentation](https://img.shields.io/badge/View-Project%20Documentation-blue?style=for-the-badge)](https://levelups.atlassian.net/wiki/spaces/CBG/overview)&ensp;

[![Project Management](https://img.shields.io/badge/View-Project%20Issue%20Board-blue?style=for-the-badge)](https://levelups.atlassian.net/jira/software/projects/CBG/boards/1)&ensp;


## Setup
1. Clone this repository to your local machine. 
   ```
   git clone https://github.com/GitBlameGame/BlameSight
   ```

2. Setup Terraform Bootstrap
   - Setup local AWS auth for account
   - Navigate to bootstrap folder
   - Run "terraform init" and then "terraform apply"
   - This will setup the IDP and role for the github actions runner.

3. Configure secret variables in the GitHub repository settings:
   - Go to Settings > Secrets and Variables.
   - Under Actions, set the secret variable "AWS_ASSUME_ROLE" to the ARN of your IAM role (from bootstrap output).

4. Make changes and push to Github. The pipelines handle infrastructure, API/BOT deployment, and CLI releases.

5. Open AWS secrets manager
    - Update JWT_SECRET_KEY to your JWT secret key.
    - Update DISCORD_BOT_TOKEN to your discord bot token.

## Runing fully locally
This project uses .NET 8

#### API
**Found in `Backend/BlameSightBackend`**

The application's required environment variables are as follows:
```env
DB_USERNAME = your_username
DB_PASSWORD = your_pass
DB_URL = localhost:5432/your_db
JWT_SECRET_KEY = your_jwt_secret_key
```

To run this use `dotnet run` to run in the terminal or use VSCODE/Visual Studio.

#### BOT
**Found in `Frontend/DiscordBot`**

Similar to the API, required values as follows:
```env
DISCORD_BOT_TOKEN = your_bot_token
API_ENDPOINT = your_api_endpoint
```

To run this use `dotnet run` to run in the terminal or use VSCODE/Visual Studio.

#### CLI
**Found in `Frontend/BlameSightFrontend`**

To run this use `dotnet run` to run in the terminal or use VSCODE/Visual Studio.

#### Database
Requires a PostgreSQL database. You can set this up however you want, and choose your own username and password, as long as you set the env variables.
