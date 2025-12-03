# Second Frontend - CarDexCLI

A command-line interface that shows realtime market data for CarDex.

> **NOTE**  
> We have a section near the bottom which addresses which **User Stories** and acceptance criteria this CLI currently meets.

</br>

## Frontend Brief

### File Structure
```bash
CarDexCLI/
├── api_client.py     # API client wrapper (with dummy data)
├── cli_client.py     # Main CLI application
├── cli_display.py    # Display and formatting utilities
├── test_suite.py     # Unit tests with coverage
├── requirements.txt  # Python dependencies
├── config.py         # Global/Shared vars
└── README.md         # This file!!
```

### How to Run
While we develop a Makefile for single-command running, you can run this frontend using:
```bash
# While inside /CarDexCLI

# Install dependencies
pip install -r requirements.txt

# Run the CLI
python cli_client.py

# Run tests
pytest test_suite.py -v --cov=.
```

### Test Coverage
This CLI currently has ~99% code test coverage, as shown by the `pytest` coverage report:
```bash
===================== test session starts ===============================================
cachedir: .pytest_cache
plugins: cov-4.1.0
collected 54
items

test_suite.py::TestApiClient::TestInitialization::test_initializes_with_default_server_url PASSED    [1%]
test_suite.py::TestApiClient::TestInitialization::test_initializes_with_custom_server_url  PASSED    [3%]
...
test_suite.py::TestCarDexCLI::TestApplicationFlow::test_handles_eof_error_gracefully       PASSED  [100%]


-- coverage: platform win32, python 3.13.1-final-0 --
Name                              Stmts   Miss  Cover
-----------------------------------------------------
api_client.py                        21      0   100%
cli_client.py                        73      3    96%
cli_display.py                      120      0   100%
test_suite.py                       354      0   100%
-----------------------------------------------------
TOTAL                               568      3    99%
```

</br>

## Commands

### `help`
Display available commands.
```
open        - Show the top 5 latest open trades
trades      - Show the top 5 latest completed trades
shop        - View all available packs and their prices
collections - View all available collections and their prices
vroom       - ...?
help        - Show this help message
exit        - Exit the application
```

</br>

### `open` - Latest 5 open trades
Fetch the 5 newest trades that are open waiting for a buyer within CarDex and display them in a neat format.

#### EXAMPLE - RAW OUTPUT
```bash
┌────────────┐
│ ★ ★       │
│            │  TurboLover
│  C A R     │  2019 Subaru WRX STI
│     D E X  │
│            │  ASKING FOR
│      ©9000 │  ©9,000
└────────────┘
```
#### EXAMPLE - BREAKDOWN
```bash
# CARD
# <rarity, factory = ★, limited = ★★, nismo = ★★★>
# <value>
┌────────────┐
│ ★ ★       │  # TRADERS' INFO
│            │  TurboLover          # <seller>
│  C A R     │  2019 Subaru WRX STI # <card>
│     D E X  │   
│            │  ASKING FOR
│      ©9000 │  ©9,000              # <price>
└────────────┘
```

</br>

### `trades` - Latest 5 trades executed
Fetch the 5 newest trades that were executed within CarDex and display them in a neat format.

#### EXAMPLE - RAW OUTPUT
```bash
┌────────────┐
│ ★ ★ ★     │
│            │  ClassicCollector
│  C A R     │  3 hours ago
│     D E X  │
│            │  1993 Mazda RX-7 FD → 2002 Acura NSX
│      ©5000 │
└────────────┘
```
#### EXAMPLE - BREAKDOWN
```bash
# CARD
# <rarity, factory = ★, limited = ★★, nismo = ★★★>
# <value>
┌────────────┐
│ ★ ★ ★     │  # TRADERS' INFO
│            │  ClassicCollector  #<buyer>
│  C A R     │  3 hours ago       #<timestamp>
│     D E X  │   
│            │  1993 Mazda RX-7 FD → 2002 Acura NSX
│      ©5000 │  #    <buyer card> for <seller card>
└────────────┘
```

</br>

### `shop` - Show all packs for sale
Fetch all card packs currently available in the CarDex shop.

#### EXAMPLE - RAW OUTPUT
```bash
 ╦╦╦╦╦╦╦╦╦╦╦╦╦╦
 ╠╩╩╩╩╩╩╩╩╩╩╩╩╣
 │            │
 │ B O O S T  │  JDM Legends
 │    P A C K │  6 possible cards
 │            │
 │            │
 │            │
 ╠╦╦╦╦╦╦╦╦╦╦╦╦╣
 ╩╩╩╩╩╩╩╩╩╩╩╩╩╩
```
#### EXAMPLE - BREAKDOWN
```bash
 ╦╦╦╦╦╦╦╦╦╦╦╦╦╦
 ╠╩╩╩╩╩╩╩╩╩╩╩╩╣
 │            │
 │ B O O S T  │  JDM Legends       # <name>
 │    P A C K │  6 possible cards  # <cards> number of cards
 │            │
 │            │
 │            │
 ╠╦╦╦╦╦╦╦╦╦╦╦╦╣
 ╩╩╩╩╩╩╩╩╩╩╩╩╩╩
```

</br>

### `collections` - View all available collections and their prices
Fetch all collections, showing vehicle count and price.  

#### EXAMPLE - RAW OUTPUT
```bash
[1] JDM Legends
--------------------------------------------------------------------------------
  Price:       ©50,000
  Vehicles:    3
  Description: JDM Legends
```

#### EXAMPLE - BREAKDOWN
```bash
[1] JDM Legends             # <name>
--------------------------------------------------------------------------------
  Price:       ©50,000      # <price>
  Vehicles:    3            # <count> number of vehicles inside
  Description: JDM Legends  # <desc>
```

</br>

### `exit`
Stop the CLI.

</br>

### `vroom`
What could this secret command do...?

</br>
</br>

# Sprint Progress
As mentioned above, this CLI currently relates to the following **User Stores**; this list will grow over the next sprint.

## 1. [Pack Purchases](https://github.com/VSHAH1210/CarDex/issues/4)
- User can browse available packs from different collections (JDM, Muscle, Supercars, etc.)
- Each pack displays its collection name and currency cost

## 2. [Listing Card for Sale](https://github.com/VSHAH1210/CarDex/issues/7)
- User can view active listings, up to 5 at a time.

## 3. [Completing Marketplace Transactions](https://github.com/VSHAH1210/CarDex/issues/9)
- User can browse active marketplace listings (both currency sales and card trades)
- Trade history is recorded with execution date and details, as shown by recently executed trades

