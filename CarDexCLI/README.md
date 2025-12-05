# Second Frontend - CarDexCLI

A command-line interface that shows realtime market data for CarDex.

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
> Ensure the docker containers are running, so the CLI can connect to the database. See README.md in the root for details.
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
collected 99 items

test_suite.py::TestApiClient::TestInitialization::test_initializes_with_default_values   PASSED    [1%]
test_suite.py::TestApiClient::TestConnection::test_successfully_connects_to_server       PASSED    [3%]
...
test_suite.py::TestCLIClient::TestApplicationFlow::test_goodbye_message_on_normal_exit   PASSED  [100%]


---------- coverage: platform win32, python 3.13.1-final-0 -----------
Name             Stmts   Miss  Cover
------------------------------------
api_client.py      104      0   100%
cli_client.py      161      3    98%
cli_display.py     127      1    99%
config.py            7      7     0%
test_suite.py      772      0   100%
------------------------------------
TOTAL             1171     11    99%
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
