<p align="center">
  <img src="assets/logo_header.png" alt="Cardex Logo" width="800"/>
</p>

# Cardex

**Cardex** is a digital trading card game where players collect, trade, and race cars.  
Built as part of our COMP 4350 course project.  

## Project Overview
Cardex combines the excitement of collectible card games with the thrill of racing.  
Players can:
- Open randomized packs of car cards with unique stats and rarities  
- Trade cards with other players with currency  
- View their garage and collection progress  

## Documentation

- Project Proposal: [Sprint 0 Proposal](./sprint0.md)
- (NEW) App Architecture: [Architecture Breakdown](./docs/architecture.md)
- Branching Strategies: [Branching Strategies](/docs/Branching-Strategies.md)
- Coding Conventions: [Coding Conventions](/docs/Coding-Conventions.md)

---

## Team Members - Group 7
- Alejandro Labra
- Ansh Nileshkumar Patel
- Vansh Chetankumar Shah
- Jotham Simiyon Stanlirajan
- Ian Spellman
- Subhash Yadav

---

## Tech Stack 
- **Frontend (Web):** React  
- **Frontend (CLI):** Python  
- **Backend API:** ASP.NET Core 8 + Swagger/OpenAPI  
- **Database:** PostgreSQL + Prisma, hosted on Supabase  
- **DevOps:** Docker + GitHub Actions (CI/CD)  

</br>

## Project setup

### 🧩 Prerequisites
  - For normal run
    - [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.
    - python
  - For testing
    - [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (v8.0.414)
    - reportgenerator (dotnet tool)
    - Git
    - Optional: Visual Studio 2022 or VS Code

> Docker Dekstop must be running before continuing.  

### Running Dev / Local
```bash
# From the project root
docker-compose up --build

# Access the applications as follows
# API Base URL: `http://localhost:5001`
# Swagger UI:   `http://localhost:5001/swagger`
# Database:     `localhost:5432`
#   - User:       `postgres`
#   - Password:   `postgres`
#   - Database: `  cardex`
```

</br>

### Running Production
```bash
# From the project root
docker-compose -f docker-compose.prod.yml --env-file .env up --build -d

# You can now use the React and CLI using live database data
```

</br>

## Project Apps

### Using react
Access the WebApp at `http://localhost/app`, given the Docker containers are running.

#### RUNNING TESTS - NORMAL (WITHOUT WATCH MODE)
```bash
# While in /CarDexFrontend
# Note: On windows you may need to be in administrator mode to run the tests.
#       Using cmd does not require that.
npm test -- --watchAll=false
```

#### RUNNING TEST - WITH COVERAGE 
```bash
# While in /CarDexFrontend
# The coverage report will be generated in `/CarDexFrontend/coverage/lcov-report`
npm test -- --coverage --watchAll=false

# Open the report in your browser (Windows)
# You can also just simply open the index.html file
start coverage/lcov-report/index.html
```

</br>

### Using python

#### RUNNING CLI
```bash
# While inside /CarDexCLI

# Run the CLI
python cli_client.py
```

#### RUNNING TESTS
```bash
# While inside /CarDexCLI

# Run tests
pytest test_suite.py -v --cov=.
```

</br>

### DotNet

#### RUNNING TEST - NORMAL
```bash
# From the project root
# This runs all unit tests across the backend layer
dotnet test
```

#### RUNNING TESTS - WITH COVERAGE (COVERLET)
```bash
# From the project root
# Runs tests and collects coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate an HTML report, ensure you followed the tool install steps in the setup steps
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# Open the report in your browser (Windows)
# You can also just simply open the index.html file
start coveragereport/index.html
```

</br>
</br>

## AI Disclaimer

Portions of this project were developed with assistance from OpenAI’s ChatGPT.
Specifically, AI assistance was used to:

- Format XML documentation comments for controllers and DTOs.
- Provide mock service structure and test case suggestions for the unit tests.
- Helping with writing documentation markdown (md) files.  



