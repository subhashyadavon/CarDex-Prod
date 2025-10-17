# Sprint 1 - Deliverables

</br>

## NOTE!!
We wrote an entire architecture breakdown document, please take a look at it to see the scope of our project and our planning processes. It is located [here](docs/architecture.md)

</br>

## Testing Plan
Our testing plan is written [here](docs/Testing-Plan.md)

</br>
</br>

## Coverage
Our coverage tests are as follows:

### Frontend
Located at `/CarDexFrontend/coverage/lcov-report/index.html`

#### TEST EXECUTION 
![Frontend Tests](docs/reproducible_assets/FrontendTest2.png)

#### COVERAGE SUMMARY
![Frontend Coverage](docs/reproducible_assets/FrontEndTests.png)  

</br>

### Backend
Located at `/coveragereport/index.html`

#### UNIT TESTS & DETAIL
![Backend Unit Tests](docs/reproducible_assets/BackendUnitTests.png)
![Backend Coverage Detail](docs/reproducible_assets/BackendTests.png)

#### COVERAGE SUMMARY
![Backend Coverage Summary](docs/reproducible_assets/CoverageReport.png)

</br>
</br>

## UI Test Approach
We have 2 separate frontends in our project, one using React and one providing a Command Line Interface (CLI). Both are being treated as the View part of our ViewViewModelModel project setup. That is, they do barely any logic or validation, and instead display (sanitized) data from our ViewModel logic layers, such as our Trading Engine or API Controller.   

### React
Our React UI will be mainly displaying components, such as Cards, Packs, Users and more. These components will display images and text, and therefore will have basic coverage testing to ensure proper behavior if they do end up receiving an invalid or missing value. 

Moreover, the input elements a User can interact with will be Input fields (for login, searching, etc.) and Buttons (buy, sell, back, etc.). For the former, we will use **Fuzz Testing** by providing invalid, random, and unexpected values to fully stress the system, alongside some **Boundary Testing** for rule compliance. For the latter, we can use basic integration testing to validate results from known actions. In both cases, we have minimal logic handling in this View layer, and thus most validation will be handed off to the ViewModel Controllers.

### CLI
Our CLI UI is a simple market summary, showing recent and open trades. As such, we will have a basic suite of supported commands and use command parsing as the main was of interaction. As such, we will again use **Fuzz Testing** to 'throw a wrench' into the command inputs, to stress the system. It follows that we are aiming for basic but well-implemented functionality for this interface.

</br>
</br>

## Testing Importance

### Top 3 Unit Tests

#### 1. [User Story 7](https://github.com/VSHAH1210/CarDex/issues/8)
- `TradingEngine.ts` tested with `TradingEngine.test.ts`
- Our TradingEngine validates both cards exist and are owned by correct users. This is the baseline validation test for any trade-related event in our app.

#### 2. [User Story 7](https://github.com/VSHAH1210/CarDex/issues/8)
- `TradingEngine.ts` tested with `TradingEngine.test.ts`
- Once again, our Engine validates the proper trade type and the current offer (must be card-for-card or must be currency), and the cost (currency) is non-zero, if applicable.

#### 3. [User Story 7](https://github.com/VSHAH1210/CarDex/issues/8)
- `TradingEngine.ts` tested with `TradingEngine.test.ts`
- Lastly, again our Engine ensures the user completing the trade has either the wanted card (if of type FOR_CARD), or has the correct amount of currency to complete it (if of type FOR_CURRENCY).


### Top 3 Intergration Tests
#### 1. [Test for GetAllCards with sorting](https://github.com/VSHAH1210/CarDex/blob/feature/sprint1-cleanup/CarDexBackend/tests/IntegrationTests/CarDexBackend.IntegrationTests.Services/CardServiceTest.cs#L146)
- Tests the `GetAllCards_WithSorting` method to ensure it returns cards sorted in descending order by value.

#### 2. [Test for ExecuteTrade](https://github.com/VSHAH1210/CarDex/blob/feature/sprint1-cleanup/CarDexBackend/tests/IntegrationTests/CarDexBackend.IntegrationTests.Services/TradeServiceTest.cs#L295)
- Tests the `ExecuteTrade` method to ensure it completes the trade and transfers currency between the buyer and seller.

#### 3. [Test for GetUserTrades](https://github.com/VSHAH1210/CarDex/blob/feature/sprint1-cleanup/CarDexBackend/tests/IntegrationTests/CarDexBackend.IntegrationTests.Services/UserServiceTest.cs#L138)
- Tests the `GetUserTrades` method to ensure it returns the correct trades for a user based on the trade type.


### Top 3 Acceptance Tests

</br>
</br>

## Reproducible Environments
Our written report is located [here](docs/ReproducibilityReport.md)
