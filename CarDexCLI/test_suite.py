"""
Unit tests for CarDex CLI
Run with: pytest test_cardex_cli.py -v --cov=. --cov-report=html

Test Suite Organization:
- APIClient: Server connection and data retrieval tests
- Display: Visual formatting and output rendering tests  
- CarDexCLI: Command processing and application flow tests
"""
import pytest
from datetime import datetime, timedelta
from unittest.mock import Mock, patch
from io import StringIO

from cli_client  import CLIClient
from api_client  import APIClient
from cli_display import Display


# ============================================================================
# API CLIENT TESTS
# ============================================================================

class TestApiClient:
    """Tests for API Client - server communication and data retrieval"""
    
    class TestInitialization:
        """Client initialization and connection setup"""
        
        def test_initializes_with_default_server_url(self):
            client = APIClient()
            assert client.base_url == "http://localhost:3000"
            assert client.connected is False
        
        def test_initializes_with_custom_server_url(self):
            client = APIClient("http://example.com:8080")
            assert client.base_url == "http://example.com:8080"
        
        def test_successfully_connects_to_server(self, capsys):
            client = APIClient()
            result = client.connect()
            
            assert result is True
            assert client.connected is True
            
            captured = capsys.readouterr()
            assert "Connecting to CarDex server..." in captured.out
            assert "Connected successfully!" in captured.out
    
    class TestTradeRetrieval:
        """Fetching trade data from API"""
        
        def test_retrieves_completed_trades_successfully(self):
            client = APIClient()
            trades = client.getCompletedTrades(limit=5)
            
            assert isinstance(trades, list)
            assert len(trades) <= 5
            assert all('id' in trade for trade in trades)
            assert all('type' in trade for trade in trades)
            assert all('seller_username' in trade for trade in trades)
        
        def test_respects_completed_trades_limit_parameter(self):
            client = APIClient()
            trades = client.getCompletedTrades(limit=3)
            assert len(trades) == 3
        
        def test_retrieves_open_trades_successfully(self):
            client = APIClient()
            trades = client.getOpenTrades(limit=5)
            
            assert isinstance(trades, list)
            assert len(trades) <= 5
            assert all('id' in trade for trade in trades)
            assert all('seller_username' in trade for trade in trades)
            assert all('vehicle' in trade for trade in trades)
        
        def test_respects_open_trades_limit_parameter(self):
            client = APIClient()
            trades = client.getOpenTrades(limit=2)
            assert len(trades) == 2
    
    class TestShopRetrieval:
        """Fetching shop and collection data from API"""
        
        def test_retrieves_available_packs_successfully(self):
            client = APIClient()
            packs = client.getAvailablePacks()
            
            assert isinstance(packs, list)
            assert len(packs) > 0
            assert all('id' in pack for pack in packs)
            assert all('collection_name' in pack for pack in packs)
            assert all('price' in pack for pack in packs)
        
        def test_retrieves_collections_successfully(self):
            client = APIClient()
            collections = client.getCollections()
            
            assert isinstance(collections, list)
            assert len(collections) > 0
            assert all('id' in col for col in collections)
            assert all('name' in col for col in collections)
            assert all('pack_price' in col for col in collections)


# ============================================================================
# DISPLAY TESTS
# ============================================================================

class TestDisplay:
    """Tests for Display module - visual formatting and output rendering"""
    
    class TestUtilityFormatters:
        """Formatting utilities for time, grades, and values"""
        
        class TestTimeFormatting:
            """Relative time formatting (e.g., '5 minutes ago')"""
            
            def test_formats_current_time_as_just_now(self):
                now = datetime.now()
                result = Display.formatTimeAgo(now)
                assert result == "just now"
            
            def test_formats_minutes_ago_correctly(self):
                past = datetime.now() - timedelta(minutes=5)
                result = Display.formatTimeAgo(past)
                assert "5 minutes ago" in result
            
            def test_formats_single_minute_without_plural(self):
                past = datetime.now() - timedelta(minutes=1)
                result = Display.formatTimeAgo(past)
                assert "1 minute ago" in result
            
            def test_formats_hours_ago_correctly(self):
                past = datetime.now() - timedelta(hours=3)
                result = Display.formatTimeAgo(past)
                assert "3 hours ago" in result
            
            def test_formats_days_ago_correctly(self):
                past = datetime.now() - timedelta(days=2)
                result = Display.formatTimeAgo(past)
                assert "2 days ago" in result
        
        class TestGradeFormatting:
            """Card grade formatting with star symbols"""
            
            def test_formats_factory_grade_with_one_star(self):
                result = Display.formatGrade("FACTORY")
                assert result == "★"
            
            def test_formats_limited_run_grade_with_two_stars(self):
                result = Display.formatGrade("LIMITED_RUN")
                assert result == "★ ★"
            
            def test_formats_nismo_grade_with_three_stars(self):
                result = Display.formatGrade("NISMO")
                assert result == "★ ★ ★"
            
            def test_formats_unknown_grade_with_question_mark(self):
                result = Display.formatGrade("UNKNOWN")
                assert result == "?"
    
    class TestCompletedTradesDisplay:
        """Rendering completed trade cards"""
        
        def test_displays_message_when_no_trades_available(self, capsys):
            display = Display()
            display.showCompletedTrades([])
            
            captured = capsys.readouterr()
            assert "No completed trades found" in captured.out
        
        def test_renders_price_trade_card_with_full_details(self, capsys):
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
            display.showCompletedTrades(trades)
            
            captured = capsys.readouterr()
            assert "COMPLETED TRADES" in captured.out
            assert "TestBuyer" in captured.out
            assert "┌────────────┐" in captured.out
            assert "└────────────┘" in captured.out
            assert "│ ★" in captured.out
            assert "C A R" in captured.out
            assert "D E X" in captured.out
            assert "5,000" in captured.out
        
        def test_renders_card_trade_with_both_vehicles(self, capsys):
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
            display.showCompletedTrades(trades)
            
            captured = capsys.readouterr()
            assert "Buyer1" in captured.out
            assert "Car A → Car B" in captured.out
            assert "★ ★ ★" in captured.out
            assert "┌────────────┐" in captured.out
            assert "C A R" in captured.out
    
    class TestOpenTradesDisplay:
        """Rendering open trade listings"""
        
        def test_displays_message_when_no_open_trades_available(self, capsys):
            display = Display()
            display.showOpenTrades([])
            
            captured = capsys.readouterr()
            assert "No open trades found" in captured.out
        
        def test_renders_price_based_open_trade_card(self, capsys):
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
            display.showOpenTrades(trades)
            
            captured = capsys.readouterr()
            assert "OPEN TRADES" in captured.out
            assert "OpenSeller" in captured.out
            assert "Open Car" in captured.out
            assert "ASKING FOR" in captured.out
            assert "10,000" in captured.out
            assert "C A R" in captured.out
        
        def test_renders_card_based_open_trade_with_wanted_vehicle(self, capsys):
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
            display.showOpenTrades(trades)
            
            captured = capsys.readouterr()
            assert "Desired Car" in captured.out
            assert "ASKING FOR" in captured.out
            assert "CardTrader" in captured.out
    
    class TestShopDisplay:
        """Rendering boost pack cards in shop"""
        
        def test_displays_message_when_no_packs_available(self, capsys):
            Display.showPacks([])
            captured = capsys.readouterr()
            assert "No packs available" in captured.out
        
        def test_renders_boost_pack_card_with_full_details(self, capsys):
            packs = [
                {
                    "id": "pack-001",
                    "collection_name": "Test Collection",
                    "price": 1500,
                    "description": "Test pack description"
                }
            ]
            Display.showPacks(packs)
            
            captured = capsys.readouterr()
            assert "SHOP" in captured.out
            assert "Test Collection" in captured.out
            assert "1,500" in captured.out
            assert "Test pack description" in captured.out
            assert "B O O S T" in captured.out
            assert "P A C K" in captured.out
            assert "╦╦╦╦╦╦╦╦╦╦╦╦╦╦" in captured.out
    
    class TestCollectionsDisplay:
        """Rendering collection listings"""
        
        def test_displays_message_when_no_collections_available(self, capsys):
            Display.showCollections([])
            captured = capsys.readouterr()
            assert "No collections available" in captured.out
        
        def test_renders_collection_with_all_details(self, capsys):
            collections = [
                {
                    "id": "col-001",
                    "name": "Test Collection",
                    "pack_price": 2000,
                    "vehicle_count": 25,
                    "description": "Test collection description"
                }
            ]
            Display.showCollections(collections)
            
            captured = capsys.readouterr()
            assert "ALL COLLECTIONS" in captured.out
            assert "Test Collection" in captured.out
            assert "©2,000" in captured.out
            assert "25" in captured.out
    
    class TestLogoAndEasterEggs:
        """ASCII art and visual elements"""
        
        def test_displays_cardex_logo_on_startup(self, capsys):
            Display.showCardexLogo()
            captured = capsys.readouterr()
            assert "╔═══" in captured.out
            assert "L I V E   M A R K E T" in captured.out
            assert "NNNNNNNNN" in captured.out
        
        def test_displays_car_ascii_art_with_beep_beep(self, capsys):
            Display.showCar()
            captured = capsys.readouterr()
            assert "beep beep" in captured.out


# ============================================================================
# CLI APPLICATION TESTS
# ============================================================================

class TestCarDexCLI:
    """Tests for main CLI application - command processing and flow control"""
    
    class TestInitialization:
        def test_initializes_with_default_api_client(self):
            cli = CLIClient()
            assert cli.api_client is not None
            assert cli.display is not None
            assert cli.running is False
        
        def test_initializes_with_custom_api_client(self):
            mock_client = Mock()
            cli = CLIClient(api_client=mock_client)
            assert cli.api_client == mock_client
        
        def test_connects_to_server_successfully(self):
            mock_client = Mock()
            mock_client.connect.return_value = True
            cli = CLIClient(api_client=mock_client)
            result = cli.connect()
            assert result is True
            mock_client.connect.assert_called_once()
    
    class TestWelcomeAndHelp:
        def test_displays_welcome_message_with_instructions(self, capsys):
            cli = CLIClient()
            cli.showWelcome()
            captured = capsys.readouterr()
            assert "Welcome to CarDex Live Market!" in captured.out
            assert "help" in captured.out
        
        def test_displays_all_available_commands_in_help(self, capsys):
            cli = CLIClient()
            cli.showHelp()
            captured = capsys.readouterr()
            assert "trades" in captured.out
            assert "open" in captured.out
            assert "vroom" in captured.out
            assert "shop" in captured.out
            assert "collections" in captured.out
    
    class TestCommandHandlers:
        def test_handles_trades_command_by_fetching_completed_trades(self):
            mock_client = Mock()
            mock_client.getCompletedTrades.return_value = []
            cli = CLIClient(api_client=mock_client)
            cli.handleTrades()
            mock_client.getCompletedTrades.assert_called_once_with(limit=5)
        
        def test_handles_open_command_by_fetching_open_trades(self):
            mock_client = Mock()
            mock_client.getOpenTrades.return_value = []
            cli = CLIClient(api_client=mock_client)
            cli.handleOpen()
            mock_client.getOpenTrades.assert_called_once_with(limit=5)
        
        def test_handles_vroom_command_by_showing_car(self, capsys):
            cli = CLIClient()
            cli.handleVroom()
            captured = capsys.readouterr()
            assert "beep beep" in captured.out
        
        def test_handles_shop_command_by_fetching_packs(self):
            mock_client = Mock()
            mock_client.getAvailablePacks.return_value = []
            cli = CLIClient(api_client=mock_client)
            cli.handleShop()
            mock_client.getAvailablePacks.assert_called_once()
        
        def test_handles_collections_command_by_fetching_collections(self):
            mock_client = Mock()
            mock_client.getCollections.return_value = []
            cli = CLIClient(api_client=mock_client)
            cli.handleCollections()
            mock_client.getCollections.assert_called_once()
    
    class TestCommandProcessing:
        def test_processes_exit_command_and_returns_false(self):
            cli = CLIClient()
            result = cli.processCommand("exit")
            assert result is False
        
        def test_processes_help_command_and_displays_help(self, capsys):
            cli = CLIClient()
            result = cli.processCommand("help")
            assert result is True
            captured = capsys.readouterr()
            assert "Available Commands" in captured.out
        
        def test_processes_trades_command_successfully(self):
            mock_client = Mock()
            mock_client.getCompletedTrades.return_value = []
            cli = CLIClient(api_client=mock_client)
            result = cli.processCommand("trades")
            assert result is True
            mock_client.getCompletedTrades.assert_called_once()
        
        def test_processes_open_command_successfully(self):
            mock_client = Mock()
            mock_client.getOpenTrades.return_value = []
            cli = CLIClient(api_client=mock_client)
            result = cli.processCommand("open")
            assert result is True
            mock_client.getOpenTrades.assert_called_once()
        
        def test_processes_vroom_command_successfully(self, capsys):
            cli = CLIClient()
            result = cli.processCommand("vroom")
            assert result is True
            captured = capsys.readouterr()
            assert "beep beep" in captured.out
        
        def test_processes_shop_command_successfully(self):
            mock_client = Mock()
            mock_client.getAvailablePacks.return_value = []
            cli = CLIClient(api_client=mock_client)
            result = cli.processCommand("shop")
            assert result is True
            mock_client.getAvailablePacks.assert_called_once()
        
        def test_processes_collections_command_successfully(self):
            mock_client = Mock()
            mock_client.getCollections.return_value = []
            cli = CLIClient(api_client=mock_client)
            result = cli.processCommand("collections")
            assert result is True
            mock_client.getCollections.assert_called_once()
        
        def test_ignores_empty_commands(self):
            cli = CLIClient()
            result = cli.processCommand("")
            assert result is True
        
        def test_handles_unknown_commands_gracefully(self, capsys):
            cli = CLIClient()
            result = cli.processCommand("invalid")
            assert result is True
            captured = capsys.readouterr()
            assert "Unknown command" in captured.out
        
        def test_processes_commands_case_insensitively(self):
            cli = CLIClient()
            assert cli.processCommand("EXIT") is False
            assert cli.processCommand("Exit") is False
            assert cli.processCommand("eXiT") is False
    
    class TestApplicationFlow:
        def test_exits_gracefully_when_connection_fails(self, capsys):
            mock_client = Mock()
            mock_client.connect.return_value = False
            cli = CLIClient(api_client=mock_client)
            cli.run()
            captured = capsys.readouterr()
            assert "Failed to connect" in captured.out
        
        @patch('builtins.input', side_effect=['help', 'exit'])
        def test_runs_command_loop_until_exit(self, mock_input, capsys):
            mock_client = Mock()
            mock_client.connect.return_value = True
            cli = CLIClient(api_client=mock_client)
            cli.run()
            captured = capsys.readouterr()
            assert "Welcome to CarDex Live Market!" in captured.out
            assert "Available Commands" in captured.out
            assert "Goodbye!" in captured.out
        
        @patch('builtins.input', side_effect=KeyboardInterrupt())
        def test_handles_keyboard_interrupt_gracefully(self, mock_input, capsys):
            mock_client = Mock()
            mock_client.connect.return_value = True
            cli = CLIClient(api_client=mock_client)
            cli.run()
            captured = capsys.readouterr()
            assert "Exiting CarDex CLI" in captured.out
        
        @patch('builtins.input', side_effect=EOFError())
        def test_handles_eof_error_gracefully(self, mock_input, capsys):
            mock_client = Mock()
            mock_client.connect.return_value = True
            cli = CLIClient(api_client=mock_client)
            cli.run()
            captured = capsys.readouterr()
            assert "Exiting CarDex CLI" in captured.out
