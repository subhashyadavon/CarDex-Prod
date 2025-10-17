# Testing Plan

This document outlines the testing strategy, tools, and quality assurance approach for our project.  
This is a modified version of the template provided in the course notes, meant to guide Sprint 1 testing documentation.

---

## Testing Goals and Scope  
*Explain *what parts of the system* will be tested (backend, frontend, API, etc.) and *why*—clarify the purpose and extent of your testing.*

### Frontend  
This sprint we focused on the logic and backend layers of our project, since full Frontend implemention was not required yet.  
As such, we decided to fully implement one Controller, TradingEngine, in our React frontend. This engine fully tests 18 cases within our App's trading system, and provides a report and summary of such cases. We will use this as the basis for all other app engine components, as the format and testing process is very thorough and easy to understand.

### Backend
For Sprint 1, our primary focus in backend testing was to verify controller-service integration, business logic correctness across our ASP.NET Core Web API.  
We have implemented unit tests and integration tests to test business logic across the database and API layers to ensure our backend is consistent and stable. 

---

## Testing Frameworks and Tools  
*List the frameworks (e.g., Jest, Pytest, Cypress) and tools used for running tests, measuring coverage, or mocking data, and explain why they were chosen.*

### Jest
Our React frontend uses Jest, which generates coverage reports and provides the helpful testing functions, describe(), test()/it(), expect(), and beforeEach().  
This was chosen because it comes with `npm`, uses easily understandable syntax, and plugs in easily with typescript (our chosen React language).

### xUnit
Used for backend unit and integration testing. Provides `[Fact]` and `[Theory]` attributes, async support and integration with .NET. Chosen for its reliability and native compatibility with out WebAPI project. 

### Moq
Used to mock services, isolating controller and service logic from external dependencies. Chosen for the simplicity of mocking functionality with .NET. 

### Coverlet & ReportGenerator
Used to measure and visualize code coveragae. Chosen for easy integration with `dotnet test` as well as integration into our CI pipeline. 

---

## Test Organization and Structure  
Describe how your tests are arranged in folders or files (e.g., `tests/unit`, `tests/integration`), and how naming conventions help identify test purpose.

### Frontend
Tests are located in `CarDexFrontend/src/__tests__/logic` because this is the industry standard way of organizing typescript React test suites. Moreover, the names of each test component match the component being tested, such as `TradingEngine.test.ts` testing `TradingEngine.ts`. 

### Backend
Tests are located in `CarDexBackend/tests/UnitTests` and `CarDexBackend/tests/IntegrationTests` to ensure our backend tests are all located in a similarly-named subdirectory for organization. The names of each test component match the component being tested, such as `AuthControllerTests.cs`. 

---

## Coverage Targets  
Specify measurable goals (e.g., 100% API method coverage, ≥80% logic class coverage) and link them to your grading or sprint requirements.

### Frontend
We are aiming for (and have reached) ≥80% logic class coverage in our implemented engine thus far.  

### Backend
We are targeting (and reached) 100% method coverage across the API controllers, ensuring all endpoints are tested at least once.  
Core logic classes aim for ≥80% line coverage to confirm business rules have been thoroughly validated.  

---

## Running Tests  
### Backend (CarDexBackend)

#### Run: Web API

```bash
# From the project root
dotnet run --project CarDexBackend/CarDexBackend.Api

# Once running, visit the swagger to see all the controllers
# (Auth, Cards, Collections,   Packs, Trades, Users)
http://localhost:5083/swagger
```

#### Run: Tests

##### NORMAL
```bash
# From the project root
# This runs all unit tests across the backend layer
dotnet test
```

##### WITH COVERAGE (COVERLET)
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

Example:
```bash
# Run backend tests with coverage
pytest --cov=app tests/

# Run frontend tests
npm run test -- --coverage
```

### Frontend Setup (CarDexFrontend)
At the current moment, we just have the TradingEngine as a Frontend component written.  
It will be used as a baseline for the rest of the logic engines, including their tests.

#### Run: Tests
Ensure you are in the directory `/CarDexFrontend`.  
> **NOTE: On windows you may need to be in administrator mode to run the tests. Using cmd does not require that.**  

##### NORMAL (WITHOUT WATCH MODE)
```bash
# While in /CarDexFrontend
npm test -- --watchAll=false
```

##### WITH COVERAGE 
```bash
# While in /CarDexFrontend
# The coverage report will be generated in `/CarDexFrontend/coverage/lcov-report`
npm test -- --coverage --watchAll=false

# Open the report in your browser (Windows)
# You can also just simply open the index.html file
start coverage/lcov-report/index.html
```

---

## Reporting and Results  
Explain where to find test reports (HTML, console, CI output) and how to interpret them.  
Include screenshots or links if applicable (e.g., `/coverage/index.html`).

### Frontend
Find coverage report in `/CarDexFrontend/coverage/lcov-report/index.html`  
This shows a webpage that displays our test coverage (in %) of our React Frontend thus far.

### Backend
Find backend coverage reports in `CarDex/coveragereport/index.html` (only once they have been generated by running the testing & reporting setup) generated using Coverlet and ReportGenerator.  
This HTML report displays line, method, and class coverage percentages for our controllers, services, and backend testing suite, helping us track progress toward our coverage targets.

---

## Test Data and Environment Setup

### Backend (CarDexBackend)

#### 🧩 Prerequisites
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

### Frontend (CarDexFrontend)

#### 🧩 Prerequisites
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
```

---

## Quality Assurance and Exceptions  
*Identify any untested components, justify why they’re excluded, and explain how you maintain overall quality (e.g., through manual tests or code reviews).*

### Frontend
As mentioned above, we fully implemented only one Controller, the TradingEngine, since Frontend implementation was not full required for this sprint. Since we are using it as a baseline for all other Controllers, we ensured all 18 tests were well documented and commented nicely for future template use.

### Backend
Some backend components, such as the database integration and certain error-handling edge cases, were not fully tested this sprint. These were excluded because persistence and full database integration are planned for later sprints as our data schema is solidified.  

---

## Continuous Integration [Once set up]
Note if your tests run automatically in a CI pipeline (GitHub Actions, GitLab CI, etc.) and how that helps maintain consistency.

In this project, the tests are configured to execute automatically within a Continuous Integration (CI) pipeline using GitHub Actions.  
This automation ensures that each new code commit or pull request triggers a standardized testing process, eliminating manual intervention and reducing the likelihood of human error. By running tests in a consistent and controlled environment, the CI pipeline promotes reproducibility of results, early detection of defects, and overall codebase stability. Consequently, this approach enhances the reliability and maintainability of the software across its development lifecycle.

## Disclaimer  
Some sections of this document were written with AI assistance for clarity and consistency, then reviewed and edited by our team to ensure accuracy and alignment with our project.