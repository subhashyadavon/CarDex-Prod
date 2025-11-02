"""
Unit tests for CarDex CLI
Run with: pytest test_cardex_cli.py -v --cov=. --cov-report=html
"""
import pytest
from datetime import datetime, timedelta
from unittest.mock import Mock, patch, call
from io import StringIO

from cardex_cli import CarDexCLI
from api_client import APIClient
from display import Display


class TestAPIClient:
    """Tests for APIClient class"""
    
    def test_init_default_url(self):
        """Test APIClient initialization with default URL"""
        client = APIClient()
        assert client.base_url == "http://localhost:3000"
        assert client.connected is False
    
    def test_init_custom_url(self):
        """Test APIClient initialization with custom URL"""
        client = APIClient("http://example.com:8080")
        assert client.base_url == "http://example.com:8080"
    
    def test_connect(self, capsys):
        """Test successful connection"""
        client = APIClient()
        result = client.connect()
        
        assert result is True
        assert client.connected is True
        
        captured = capsys.readouterr()
        assert "Connecting to CarDex server..." in captured.out
        assert "Connected successfully!" in captured.out
    
    def test_get_completed_trades(self):
        """Test fetching completed trades"""
        client = APIClient()
        trades = client.get_completed_trades(limit=5)
        
        assert isinstance(trades, list)
        assert len(trades) <= 5
        assert all('id' in trade for trade in trades)
        assert all('type' in trade for trade in trades)
        assert all('seller_username' in trade for trade in trades)
    
    def test_get_completed_trades_limit(self):
        """Test completed trades respects limit parameter"""
        client = APIClient()
        trades = client.get_completed_trades(limit=3)
        
        assert len(trades) == 3
    
    def test_get_open_trades(self):
        """Test fetching open trades"""
        client = APIClient()
        trades = client.get_open_trades(limit=5)
        
        assert isinstance(trades, list)
        assert len(trades) <= 5
        assert all('id' in trade for trade in trades)
        assert all('seller_username' in trade for trade in trades)
        assert all('vehicle' in trade for trade in trades)
    
    def test_get_open_trades_limit(self):
        """Test open trades respects limit parameter"""
        client = APIClient()
        trades = client.get_open_trades(limit=2)
        
        assert len(trades) == 2
    
    def test_get_available_packs(self):
        """Test fetching available packs"""
        client = APIClient()
        packs = client.get_available_packs()
        
        assert isinstance(packs, list)
        assert len(packs) > 0
        assert all('id' in pack for pack in packs)
        assert all('collection_name' in pack for pack in packs)
        assert all('price' in pack for pack in packs)
    
    def test_get_collections(self):
        """Test fetching collections"""
        client = APIClient()
        collections = client.get_collections()
        
        assert isinstance(collections, list)
        assert len(collections) > 0
        assert all('id' in col for col in collections)
        assert all('name' in col for col in collections)
        assert all('pack_price' in col for col in collections)


class TestDisplay:
    """Tests for Display class"""
    
    def test_show_cardex_logo(self, capsys):
        """Test showing CarDex logo"""
        Display.show_cardex_logo()
        
        captured = capsys.readouterr()
        # Check for new logo content
        assert "╔═══" in captured.out
        assert "L I V E   M A R K E T" in captured.out
        assert "NNNNNNNNN" in captured.out  # Part of the new logo design
    
    def test_show_car(self, capsys):
        """Test showing ASCII car"""
        Display.show_car()
        
        captured = capsys.readouterr()
        assert "beep beep" in captured.out
    
    def test_format_time_ago_just_now(self):
        """Test formatting time for very recent event"""
        now = datetime.now()
        result = Display.format_time_ago(now)
        assert result == "just now"
    
    def test_format_time_ago_minutes(self):
        """Test formatting time for minutes ago"""
        past = datetime.now() - timedelta(minutes=5)
        result = Display.format_time_ago(past)
        assert "5 minutes ago" in result
    
    def test_format_time_ago_one_minute(self):
        """Test formatting time for one minute ago"""
        past = datetime.now() - timedelta(minutes=1)
        result = Display.format_time_ago(past)
        assert "1 minute ago" in result
    
    def test_format_time_ago_hours(self):
        """Test formatting time for hours ago"""
        past = datetime.now() - timedelta(hours=3)
        result = Display.format_time_ago(past)
        assert "3 hours ago" in result
    
    def test_format_time_ago_days(self):
        """Test formatting time for days ago"""
        past = datetime.now() - timedelta(days=2)
        result = Display.format_time_ago(past)
        assert "2 days ago" in result
    
    def test_format_grade_factory(self):
        """Test formatting FACTORY grade"""
        result = Display.format_grade("FACTORY")
        assert result == "★"
    
    def test_format_grade_limited_run(self):
        """Test formatting LIMITED_RUN grade"""
        result = Display.format_grade("LIMITED_RUN")
        assert result == "★ ★"
    
    def test_format_grade_nismo(self):
        """Test formatting NISMO grade"""
        result = Display.format_grade("NISMO")
        assert result == "★ ★ ★"
    
    def test_format_grade_unknown(self):
        """Test formatting unknown grade"""
        result = Display.format_grade("UNKNOWN")
        assert result == "?"
    
    def test_show_completed_trades_empty(self, capsys):
        """Test showing empty completed trades list"""
        display = Display()
        display.show_completed_trades([])
        
        captured = capsys.readouterr()
        assert "No completed trades found" in captured.out
    
    def test_show_completed_trades_with_data(self, capsys):
        """Test showing completed trades with data"""
        display = Display()
        trades = [
            {
                "id": "test-001",
                "type": "FOR_PRICE",
                "seller_username": "TestSeller",
                "buyer_username": "TestBuyer",
                "vehicle": "Test Car",
                "grade": "FACTORY",
                "price": 5000,
                "executed_date": datetime.now() - timedelta(minutes=10)
            }
        ]
        display.show_completed_trades(trades)
        
        captured = capsys.readouterr()
        assert "COMPLETED TRADES" in captured.out
        assert "TestBuyer" in captured.out
        assert "┌────────────┐" in captured.out  # Box top
        assert "└────────────┘" in captured.out  # Box bottom
        assert "│ ★" in captured.out  # Star for grade
        assert "C A R" in captured.out  # New card design
        assert "D E X" in captured.out  # New card design
        assert "5,000" in captured.out
    
    def test_show_completed_trades_card_type(self, capsys):
        """Test showing card-for-card trade"""
        display = Display()
        trades = [
            {
                "id": "test-002",
                "type": "FOR_CARD",
                "seller_username": "Seller1",
                "buyer_username": "Buyer1",
                "seller_vehicle": "Car A",
                "buyer_vehicle": "Car B",
                "grade": "NISMO",
                "price": 0,
                "executed_date": datetime.now()
            }
        ]
        display.show_completed_trades(trades)
        
        captured = capsys.readouterr()
        assert "Buyer1" in captured.out
        assert "Car A → Car B" in captured.out
        assert "★ ★ ★" in captured.out  # NISMO grade with spaces
        assert "┌────────────┐" in captured.out
        assert "C A R" in captured.out
    
    def test_show_open_trades_empty(self, capsys):
        """Test showing empty open trades list"""
        display = Display()
        display.show_open_trades([])
        
        captured = capsys.readouterr()
        assert "No open trades found" in captured.out
    
    def test_show_open_trades_with_data(self, capsys):
        """Test showing open trades with data"""
        display = Display()
        trades = [
            {
                "id": "open-001",
                "type": "FOR_PRICE",
                "seller_username": "OpenSeller",
                "vehicle": "Open Car",
                "grade": "LIMITED_RUN",
                "price": 10000,
                "want_vehicle": None
            }
        ]
        display.show_open_trades(trades)
        
        captured = capsys.readouterr()
        assert "OPEN TRADES" in captured.out
        assert "OpenSeller" in captured.out
        assert "Open Car" in captured.out
        assert "ASKING FOR" in captured.out  # New format
        assert "10,000" in captured.out
        assert "C A R" in captured.out  # Card design in open trades
    
    def test_show_open_trades_for_card(self, capsys):
        """Test showing open trade wanting a card"""
        display = Display()
        trades = [
            {
                "id": "open-002",
                "type": "FOR_CARD",
                "seller_username": "CardTrader",
                "vehicle": "Trade Car",
                "grade": "NISMO",
                "price": 0,
                "want_vehicle": "Desired Car"
            }
        ]
        display.show_open_trades(trades)
        
        captured = capsys.readouterr()
        assert "Desired Car" in captured.out
        assert "ASKING FOR" in captured.out
        assert "CardTrader" in captured.out
    
    def test_show_packs_empty(self, capsys):
        """Test showing empty packs list"""
        Display.show_packs([])
        
        captured = capsys.readouterr()
        assert "No packs available" in captured.out
    
    def test_show_packs_with_data(self, capsys):
        """Test showing packs with data"""
        packs = [
            {
                "id": "pack-001",
                "collection_name": "Test Collection",
                "price": 1500,
                "description": "Test pack description"
            }
        ]
        Display.show_packs(packs)
        
        captured = capsys.readouterr()
        assert "SHOP" in captured.out
        assert "Test Collection" in captured.out
        assert "1,500" in captured.out
        assert "Test pack description" in captured.out
        assert "B O O S T" in captured.out  # New pack design
        assert "P A C K" in captured.out  # New pack design
        assert "╦╦╦╦╦╦╦╦╦╦╦╦╦╦" in captured.out  # Pack box design
    
    def test_show_collections_empty(self, capsys):
        """Test showing empty collections list"""
        Display.show_collections([])
        
        captured = capsys.readouterr()
        assert "No collections available" in captured.out
    
    def test_show_collections_with_data(self, capsys):
        """Test showing collections with data"""
        collections = [
            {
                "id": "col-001",
                "name": "Test Collection",
                "pack_price": 2000,
                "vehicle_count": 25,
                "description": "Test collection description"
            }
        ]
        Display.show_collections(collections)
        
        captured = capsys.readouterr()
        assert "ALL COLLECTIONS" in captured.out
        assert "Test Collection" in captured.out
        assert "©2,000" in captured.out  # Updated format with © symbol
        assert "25" in captured.out


class TestCarDexCLI:
    """Tests for CarDexCLI class"""
    
    def test_init_default(self):
        """Test CLI initialization with default API client"""
        cli = CarDexCLI()
        assert cli.api_client is not None
        assert cli.display is not None
        assert cli.running is False
    
    def test_init_custom_client(self):
        """Test CLI initialization with custom API client"""
        mock_client = Mock()
        cli = CarDexCLI(api_client=mock_client)
        assert cli.api_client == mock_client
    
    def test_connect(self):
        """Test connecting to server"""
        mock_client = Mock()
        mock_client.connect.return_value = True
        
        cli = CarDexCLI(api_client=mock_client)
        result = cli.connect()
        
        assert result is True
        mock_client.connect.assert_called_once()
    
    def test_show_welcome(self, capsys):
        """Test showing welcome message"""
        cli = CarDexCLI()
        cli.show_welcome()
        
        captured = capsys.readouterr()
        assert "Welcome to CarDex Live Market!" in captured.out
        assert "help" in captured.out
    
    def test_show_help(self, capsys):
        """Test showing help message"""
        cli = CarDexCLI()
        cli.show_help()
        
        captured = capsys.readouterr()
        assert "trades" in captured.out
        assert "open" in captured.out
        assert "vroom" in captured.out
        assert "shop" in captured.out
        assert "collections" in captured.out
    
    def test_handle_trades(self):
        """Test handling trades command"""
        mock_client = Mock()
        mock_client.get_completed_trades.return_value = []
        
        cli = CarDexCLI(api_client=mock_client)
        cli.handle_trades()
        
        mock_client.get_completed_trades.assert_called_once_with(limit=5)
    
    def test_handle_open(self):
        """Test handling open command"""
        mock_client = Mock()
        mock_client.get_open_trades.return_value = []
        
        cli = CarDexCLI(api_client=mock_client)
        cli.handle_open()
        
        mock_client.get_open_trades.assert_called_once_with(limit=5)
    
    def test_handle_vroom(self, capsys):
        """Test handling vroom command"""
        cli = CarDexCLI()
        cli.handle_vroom()
        
        captured = capsys.readouterr()
        assert "beep beep" in captured.out
    
    def test_handle_shop(self):
        """Test handling shop command"""
        mock_client = Mock()
        mock_client.get_available_packs.return_value = []
        
        cli = CarDexCLI(api_client=mock_client)
        cli.handle_shop()
        
        mock_client.get_available_packs.assert_called_once()
    
    def test_handle_collections(self):
        """Test handling collections command"""
        mock_client = Mock()
        mock_client.get_collections.return_value = []
        
        cli = CarDexCLI(api_client=mock_client)
        cli.handle_collections()
        
        mock_client.get_collections.assert_called_once()
    
    def test_process_command_exit(self):
        """Test processing exit command"""
        cli = CarDexCLI()
        result = cli.process_command("exit")
        assert result is False
    
    def test_process_command_help(self, capsys):
        """Test processing help command"""
        cli = CarDexCLI()
        result = cli.process_command("help")
        
        assert result is True
        captured = capsys.readouterr()
        assert "Available Commands" in captured.out
    
    def test_process_command_trades(self):
        """Test processing trades command"""
        mock_client = Mock()
        mock_client.get_completed_trades.return_value = []
        
        cli = CarDexCLI(api_client=mock_client)
        result = cli.process_command("trades")
        
        assert result is True
        mock_client.get_completed_trades.assert_called_once()
    
    def test_process_command_open(self):
        """Test processing open command"""
        mock_client = Mock()
        mock_client.get_open_trades.return_value = []
        
        cli = CarDexCLI(api_client=mock_client)
        result = cli.process_command("open")
        
        assert result is True
        mock_client.get_open_trades.assert_called_once()
    
    def test_process_command_vroom(self, capsys):
        """Test processing vroom command"""
        cli = CarDexCLI()
        result = cli.process_command("vroom")
        
        assert result is True
        captured = capsys.readouterr()
        assert "beep beep" in captured.out
    
    def test_process_command_shop(self):
        """Test processing shop command"""
        mock_client = Mock()
        mock_client.get_available_packs.return_value = []
        
        cli = CarDexCLI(api_client=mock_client)
        result = cli.process_command("shop")
        
        assert result is True
        mock_client.get_available_packs.assert_called_once()
    
    def test_process_command_collections(self):
        """Test processing collections command"""
        mock_client = Mock()
        mock_client.get_collections.return_value = []
        
        cli = CarDexCLI(api_client=mock_client)
        result = cli.process_command("collections")
        
        assert result is True
        mock_client.get_collections.assert_called_once()
    
    def test_process_command_empty(self):
        """Test processing empty command"""
        cli = CarDexCLI()
        result = cli.process_command("")
        assert result is True
    
    def test_process_command_unknown(self, capsys):
        """Test processing unknown command"""
        cli = CarDexCLI()
        result = cli.process_command("invalid")
        
        assert result is True
        captured = capsys.readouterr()
        assert "Unknown command" in captured.out
    
    def test_process_command_case_insensitive(self):
        """Test that commands are case insensitive"""
        cli = CarDexCLI()
        
        result1 = cli.process_command("EXIT")
        result2 = cli.process_command("Exit")
        result3 = cli.process_command("eXiT")
        
        assert result1 is False
        assert result2 is False
        assert result3 is False
    
    def test_run_failed_connection(self, capsys):
        """Test run when connection fails"""
        mock_client = Mock()
        mock_client.connect.return_value = False
        
        cli = CarDexCLI(api_client=mock_client)
        cli.run()
        
        captured = capsys.readouterr()
        assert "Failed to connect" in captured.out
    
    @patch('builtins.input', side_effect=['help', 'exit'])
    def test_run_with_commands(self, mock_input, capsys):
        """Test run with help and exit commands"""
        mock_client = Mock()
        mock_client.connect.return_value = True
        
        cli = CarDexCLI(api_client=mock_client)
        cli.run()
        
        captured = capsys.readouterr()
        assert "Welcome to CarDex Live Market!" in captured.out
        assert "Available Commands" in captured.out
        assert "Goodbye!" in captured.out
    
    @patch('builtins.input', side_effect=KeyboardInterrupt())
    def test_run_keyboard_interrupt(self, mock_input, capsys):
        """Test handling keyboard interrupt"""
        mock_client = Mock()
        mock_client.connect.return_value = True
        
        cli = CarDexCLI(api_client=mock_client)
        cli.run()
        
        captured = capsys.readouterr()
        assert "Exiting CarDex CLI" in captured.out
    
    @patch('builtins.input', side_effect=EOFError())
    def test_run_eof_error(self, mock_input, capsys):
        """Test handling EOF error"""
        mock_client = Mock()
        mock_client.connect.return_value = True
        
        cli = CarDexCLI(api_client=mock_client)
        cli.run()
        
        captured = capsys.readouterr()
        assert "Exiting CarDex CLI" in captured.out