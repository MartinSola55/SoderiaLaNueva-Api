# Soderia la Nueva API

Soderia la Nueva is an API for managing a water distribution business. This API is built with .NET 8 and uses PostgreSQL as the database.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Usage](#usage)
- [License](#license)

## Prerequisites

Before you begin, ensure you have the following prerequisites:

- .NET 8 SDK installed
- Docker to run the PostgreSQL database
- Visual Studio for a better development experience

## Installation

1. **Clone this repository:**

   ```bash
   git clone https://github.com/your-username/soderialanueva-api.git
   cd soderialanueva-api
   ```

2. **Set up the database:**

   ```bash
   docker-compose up -d
   ```

3. **Set up the environment variables:**
   
      The password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.

      Set up the `secrets.json` file in the root of the project with the following content:

      ```json
      {
         "AdminEmail": "your-email",
         "AdminPassword": "your-password"
      }
      ```


## Usage

1. **To run any of the database migrations:**
      
      ```bash
      dotnet ef database update
      ```

      or in the Package Manager Console:

      ```package-manager
      Update-Database
      ```

2. **To run the application:**

      ```bash
      dotnet run
      ```

   API will be available at `http://localhost:5000`.


## License

This project uses the following license: [MIT](https://choosealicense.com/licenses/mit/).
