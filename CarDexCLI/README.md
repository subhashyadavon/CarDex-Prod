# Second Frontend - CarDexCLI

A command-line interface that shows realtime market data for CarDex.

</br>

## Project Structure
```bash
CarDexCLI/
├── cardex_cli.py      # Main CLI application
├── api_client.py      # API client wrapper (with dummy data)
├── display.py         # Display and formatting utilities
├── test_cardex_cli.py # Unit tests
├── requirements.txt   # Python dependencies
└── README.md         # This file
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
 │    P A C K │  Classic Japanese sports cars
 │            │
 │   ©2,000   │
 │            │
 ╠╦╦╦╦╦╦╦╦╦╦╦╦╣
 ╩╩╩╩╩╩╩╩╩╩╩╩╩╩
```
#### EXAMPLE - BREAKDOWN
```bash
# PACK
# <price>
 ╦╦╦╦╦╦╦╦╦╦╦╦╦╦
 ╠╩╩╩╩╩╩╩╩╩╩╩╩╣
 │            │
 │ B O O S T  │  JDM Legends                   # <name>
 │    P A C K │  Classic Japanese sports cars  # <desc>
 │            │
 │   ©2,000   │
 │            │
 ╠╦╦╦╦╦╦╦╦╦╦╦╦╣
 ╩╩╩╩╩╩╩╩╩╩╩╩╩╩
```

</br>

### `exit`
Stop the CLI.

### `vroom`
What could this secret command do...?

## Features

- 🚗 View completed and open trades
- 🛍️ Browse available packs in the shop
- 📚 Explore all card collections
- 🎨 ASCII art and formatted output
- 🧪 Comprehensive test coverage (>80%)



## Installation & Quick Start

### Option 1: Using Make (Recommended)

If you have `make` installed (most Linux/Mac systems):

```bash
# See all available commands
make

# Install dependencies
make install

# Run the CLI
make run

# Run tests
make test

# Run tests with coverage
make coverage

# Clean up generated files
make clean
```

### Option 2: Using the Bash Script

If you don't have `make` installed:

```bash
# See all available commands
./run.sh

# Install dependencies
./run.sh install

# Run the CLI
./run.sh run

# Run tests
./run.sh test

# Run tests with coverage
./run.sh coverage

# Clean up generated files
./run.sh clean
```

### Option 3: Manual Commands

You can also run commands directly:

```bash
# Install dependencies
pip install -r requirements.txt

# Run the CLI
python cardex_cli.py

# Run tests
pytest test_cardex_cli.py -v --cov=.
```

## Available Commands

Once the CLI is running, you can use the following commands:

- `trades` - Show the top 5 latest completed trades
- `open` - Show the top 5 latest open trades
- `vroom` - Display a cool ASCII car (beep beep!)
- `shop` - View all available packs and their prices
- `collections` - View all available collections and their prices
- `help` - Show help message with all commands
- `exit` - Exit the application

## Running Tests

### Quick Way

```bash
# Run tests
make test

# Or using bash script
./run.sh test
```

### With Coverage Report

```bash
# Run tests with coverage
make coverage

# Or using bash script
./run.sh coverage
```

### Manual Way

Run all tests with coverage report:

```bash
pytest test_cardex_cli.py -v --cov=. --cov-report=term-missing
```

Generate HTML coverage report:

```bash
pytest test_cardex_cli.py -v --cov=. --cov-report=html
```

Then open `htmlcov/index.html` in your browser to view the detailed coverage report.

## Code Architecture

### API Client (`api_client.py`)

The `APIClient` class handles all server communication. Currently uses dummy data, but is structured for easy API integration:

```python
# Current dummy implementation
def get_completed_trades(self, limit: int = 5) -> List[Dict]:
    # TODO: Replace with actual API call
    # return requests.get(f"{self.base_url}/api/trades/completed?limit={limit}").json()
    return dummy_trades[:limit]
```

To integrate with a real API:
1. Add `requests` to requirements.txt
2. Replace the dummy data returns with actual HTTP calls
3. Add error handling and authentication as needed

### Display (`display.py`)

The `Display` class handles all output formatting:
- ASCII art logos and graphics
- Formatted tables and lists
- Time formatting utilities
- Grade/rarity indicators

### Main CLI (`cardex_cli.py`)

The `CarDexCLI` class manages:
- Command processing and routing
- User input handling
- Application flow control
- Integration between API client and display

## Integrating the Real API

When your API is ready, update these files:

1. **api_client.py**: Replace dummy functions with real API calls
2. **requirements.txt**: Add `requests` or your HTTP client of choice
3. Update base_url in initialization as needed

Example API integration:

```python
import requests

def get_completed_trades(self, limit: int = 5) -> List[Dict]:
    try:
        response = requests.get(
            f"{self.base_url}/api/trades/completed",
            params={"limit": limit}
        )
        response.raise_for_status()
        return response.json()
    except requests.RequestException as e:
        print(f"Error fetching trades: {e}")
        return []
```

## Test Coverage

The project includes comprehensive unit tests covering:
- ✅ API client methods
- ✅ Display formatting functions
- ✅ CLI command processing
- ✅ Error handling
- ✅ Edge cases

Target coverage: **>80%**

## Future Enhancements

Potential improvements for the CLI:
- User authentication
- Interactive trade creation
- Pack opening simulation
- Collection browsing with filtering
- User inventory management
- Trade notifications

## License

This is a project component for the CarDex card game.