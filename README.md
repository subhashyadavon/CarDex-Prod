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
- Trade cards with other players  
- Complete themed collections for rewards  
- Race cars based on their performance stats  
- Upgrade and customize their garage  

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
- **Frontend (Mobile):** Flutter  
- **Frontend (Web):** React  
- **Backend API:** ASP.NET Core 8 + Swagger/OpenAPI
- **Database:** PostgreSQL + Prisma, hosted on Supabase
- **DevOps:** Docker + GitHub Actions (CI/CD)  

---

## Repository Structure

    CarDex/
    ├── assets/                        # Images, design assets, and other media
    ├── CarDexBackend/                 # Backend (ASP.NET Core)
    │   ├── CarDexBackend.Api/         # API layer – controllers, request/response models
    │   ├── CarDexBackend.Domain/      # Domain layer – core entities, enums, and domain logic
    │   ├── CarDexBackend.Services/    # Service layer – business logic, interfaces, and implementations
    │   ├── Shared/                    # Shared DTOs, constants, and localization files
    │   ├── scripts/                   # Utility or setup scripts (regression testing)
    │   ├── tests/                     # Unit and integration tests
    ├── CarDexDatabase/                # Database project
    ├── CarDexFrontend/                # Frontend project (React frontend, CLI frontend)
    ├── docs/                          # Documentation (reports, architecture diagrams, etc.)

</br>
</br>

## Backend Setup (CarDexBackend)

### 🧩 Prerequisites
  - [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (v8.0.414)
  - reportgenerator (dotnet tool)
  - Git
  - Optional: Visual Studio 2022 or VS Code  
</br>

```bash
# Check installation
dotnet --version

# Check reportgenerator tool, this should show 'dotnet-reportgenerator-globaltool' installed
dotnet tool list --global
# (If not installed) run this command to install the tool
dotnet tool install -g dotnet-reportgenerator-globaltool

# Clone and restore dependencies
git clone https://github.com/VSHAH1210/CarDex.git
cd CarDexBackend
dotnet restore

# Build all projects
dotnet build
```

### Run: Web API

```bash
# From the project root
dotnet run --project CarDexBackend/CarDexBackend.Api

# Once running, visit the swagger to see all the controllers
# (Auth, Cards, Collections,   Packs, Trades, Users)
http://localhost:5083/swagger
```

### Run: Tests

#### NORMAL
```bash
# From the project root
# This runs all unit tests across the backend layer
dotnet test
```

#### WITH COVERAGE (COVERLET)
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

## Frontend Setup
At the current moment, we just have the TradingEngine as a Frontend component written.  
It will be used as a baseline for the rest of the logic engines, including their tests.

### 🧩 Prerequisites
  - [Node.js](https://nodejs.org/) (v18.0 or higher)
  - npm (comes with Node.js)
  - Optional: VS Code with TypeScript extensions


```bash
# Check installation
node --version
npm --version

# Clone
git clone https://github.com/VSHAH1210/CarDex.git
cd CarDexFrontend
npm install
```

### Run: Tests
Ensure you are in the directory `/CarDexFrontend`.  
> **NOTE: On windows you may need to be in administrator mode to run the tests. Using cmd does not require that.**  

#### NORMAL (WITHOUT WATCH MODE)
```bash
# While in /CarDexFrontend
npm test -- --watchAll=false
```

#### WITH COVERAGE 
```bash
# While in /CarDexFrontend
# The coverage report will be generated in `/CarDexFrontend/coverage/lcov-report`
npm test -- --coverage --watchAll=false

# Open the report in your browser (Windows)
# You can also just simply open the index.html file
start coverage/lcov-report/index.html
```

</br>

## AI Disclaimer

Portions of this project were developed with assistance from OpenAI’s ChatGPT.
Specifically, AI assistance was used to:

- Format XML documentation comments for controllers and DTOs.
- Provide mock service structure and test case suggestions for the unit tests.
- Helping with writing documentation markdown (md) files.  

