"""
Unit tests for CarDex CLI
Run with: pytest test_suite.py -v --cov=. --cov-report=html

Test Suite Organization:
- APIClient: Server connection, authentication, and data retrieval tests
- Display: Visual formatting and output rendering tests  
- CLIClient: Command processing, transformations, and application flow tests
"""
import pytest
from datetime import datetime, timedelta
from unittest.mock import Mock, patch
import requests

from cli_client import CLIClient
from api_client import APIClient
from cli_display import Display


# ============================================================================
# API CLIENT TESTS
# ============================================================================

class TestApiClient:
    """Tests for API Client - server communication and data retrieval"""
    
    class TestInitialization:
        """Client initialization and connection setup"""
        
        def test_initializes_with_default_values(self):
            client = APIClient()
            assert client.connected is False
            assert client.access_token is None
    
    class TestConnection:
        """Server connection and health check tests"""
        
        @patch('requests.get')
        def test_successfully_connects_to_server(self, mock_get, capsys):
            mock_response = Mock()
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            result = client.connect()
            
            assert result is True
            assert client.connected is True
            captured = capsys.readouterr()
            assert "Connecting to CarDex API" in captured.out
            assert "[DONE]" in captured.out
        
        @patch('requests.get')
        def test_fails_to_connect_when_server_unavailable(self, mock_get, capsys):
            mock_get.side_effect = requests.exceptions.RequestException("Connection refused")
            
            client = APIClient()
            result = client.connect()
            
            assert result is False
            assert client.connected is False
            captured = capsys.readouterr()
            assert "[FAIL]" in captured.out
        
        @patch('requests.get')
        def test_health_check_returns_true_on_success(self, mock_get, capsys):
            mock_response = Mock()
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            result = client.healthCheck()
            
            assert result is True
            assert "Connecting to CarDex API" in capsys.readouterr().out
        
        @patch('requests.get')
        def test_health_check_returns_false_on_failure(self, mock_get, capsys):
            mock_get.side_effect = requests.exceptions.Timeout()
            
            client = APIClient()
            result = client.healthCheck()
            
            assert result is False
    
    class TestAuthentication:
        """Authentication and token management tests"""
        
        @patch('requests.post')
        def test_successful_login_stores_token(self, mock_post):
            mock_response = Mock()
            mock_response.json.return_value = {"accessToken": "test-token-123"}
            mock_response.raise_for_status = Mock()
            mock_post.return_value = mock_response
            
            client = APIClient()
            result = client.login("testuser", "testpass")
            
            assert result is True
            assert client.access_token == "test-token-123"
            mock_post.assert_called_once()
        
        @patch('requests.post')
        def test_login_fails_with_invalid_credentials(self, mock_post, capsys):
            mock_response = Mock()
            mock_response.status_code = 401
            mock_post.side_effect = requests.exceptions.HTTPError(response=mock_response)
            
            client = APIClient()
            result = client.login("baduser", "badpass")
            
            assert result is False
            assert client.access_token is None
            captured = capsys.readouterr()
            assert "Invalid credentials" in captured.out
        
        @patch('requests.post')
        def test_login_fails_with_connection_error(self, mock_post, capsys):
            mock_post.side_effect = requests.exceptions.ConnectionError("Network error")
            
            client = APIClient()
            result = client.login("testuser", "testpass")
            
            assert result is False
            assert client.access_token is None
        
        def test_get_headers_returns_authorization_header(self):
            client = APIClient()
            client.access_token = "test-token"
            
            headers = client.getHeaders()
            
            assert headers == {"Authorization": "Bearer test-token"}
        
        def test_get_headers_raises_exception_when_not_authenticated(self):
            client = APIClient()
            
            with pytest.raises(Exception, match="Not authenticated"):
                client.getHeaders()
    
    class TestTradeRetrieval:
        """Fetching trade data from API"""
        
        @patch('requests.get')
        def test_retrieves_completed_trades_successfully(self, mock_get):
            mock_response = Mock()
            mock_response.json.return_value = {
                "trades": [
                    {"id": "1", "type": "FOR_PRICE", "price": 5000},
                    {"id": "2", "type": "FOR_CARD", "price": 0}
                ]
            }
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            client.access_token = "test-token"
            trades = client.getCompletedTrades(limit=5)
            
            assert isinstance(trades, list)
            assert len(trades) == 2
            assert all('id' in trade for trade in trades)
        
        @patch('requests.get')
        def test_respects_completed_trades_limit_parameter(self, mock_get):
            mock_response = Mock()
            mock_response.json.return_value = {"trades": [{"id": str(i)} for i in range(3)]}
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            client.access_token = "test-token"
            trades = client.getCompletedTrades(limit=3)
            
            # Verify the API was called with correct params
            call_args = mock_get.call_args
            assert call_args[1]['params']['limit'] == 3
        
        @patch('requests.get')
        def test_retrieves_open_trades_successfully(self, mock_get):
            mock_response = Mock()
            mock_response.json.return_value = {
                "trades": [
                    {"id": "1", "cardId": "card-1"},
                    {"id": "2", "cardId": "card-2"}
                ]
            }
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            client.access_token = "test-token"
            trades = client.getOpenTrades(limit=5)
            
            assert isinstance(trades, list)
            assert len(trades) == 2
        
        @patch('requests.get')
        def test_respects_open_trades_limit_parameter(self, mock_get):
            mock_response = Mock()
            mock_response.json.return_value = {"trades": [{"id": str(i)} for i in range(2)]}
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            client.access_token = "test-token"
            trades = client.getOpenTrades(limit=2)
            
            call_args = mock_get.call_args
            assert call_args[1]['params']['limit'] == 2
    
    class TestShopRetrieval:
        """Fetching shop and collection data from API"""
        
        @patch('requests.get')
        def test_retrieves_available_packs_successfully(self, mock_get):
            mock_response = Mock()
            mock_response.json.return_value = {
                "collections": [
                    {"id": "1", "name": "JDM Legends"},
                    {"id": "2", "name": "Muscle Cars"}
                ]
            }
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            client.access_token = "test-token"
            packs = client.getAvailablePacks()
            
            assert isinstance(packs, list)
            assert len(packs) == 2
            assert all('name' in pack for pack in packs)
        
        @patch('requests.get')
        def test_retrieves_collections_successfully(self, mock_get):
            mock_response = Mock()
            mock_response.json.return_value = {
                "collections": [
                    {"id": "1", "name": "Collection 1", "cardCount": 10},
                    {"id": "2", "name": "Collection 2", "cardCount": 15}
                ]
            }
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            client.access_token = "test-token"
            collections = client.getCollections()
            
            assert isinstance(collections, list)
            assert len(collections) == 2
    
    class TestCardRetrieval:
        """Fetching individual card details"""
        
        @patch('requests.get')
        def test_retrieves_card_details_successfully(self, mock_get):
            mock_response = Mock()
            mock_response.json.return_value = {
                "id": "card-123",
                "name": "2019 Subaru WRX STI",
                "grade": "LIMITED_RUN"
            }
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            client.access_token = "test-token"
            card = client.getCard("card-123")
            
            assert card is not None
            assert card["id"] == "card-123"
            assert card["name"] == "2019 Subaru WRX STI"
        
        @patch('requests.get')
        def test_returns_none_when_card_not_found(self, mock_get):
            mock_response = Mock()
            mock_response.status_code = 404
            mock_get.side_effect = requests.exceptions.HTTPError(response=mock_response)
            
            client = APIClient()
            client.access_token = "test-token"
            card = client.getCard("nonexistent")
            
            assert card is None
        
        @patch('requests.get')
        def test_raises_exception_for_non_404_errors(self, mock_get):
            mock_response = Mock()
            mock_response.status_code = 500
            mock_get.side_effect = requests.exceptions.HTTPError(response=mock_response)
            
            client = APIClient()
            client.access_token = "test-token"
            
            with pytest.raises(requests.exceptions.HTTPError):
                client.getCard("card-123")
    
    class TestEnrichedTradeRetrieval:
        """Fetching trades with full card details"""
        
        @patch('requests.get')
        def test_enriches_open_trades_with_card_details(self, mock_get):
            # Mock responses for trades and cards
            def side_effect(url, **kwargs):
                mock_response = Mock()
                mock_response.raise_for_status = Mock()
                
                if "trades" in url:
                    mock_response.json.return_value = {
                        "trades": [
                            {"id": "trade-1", "cardId": "card-1", "wantCardId": None}
                        ]
                    }
                elif "cards/card-1" in url:
                    mock_response.json.return_value = {
                        "id": "card-1",
                        "name": "Test Car",
                        "grade": "FACTORY"
                    }
                return mock_response
            
            mock_get.side_effect = side_effect
            
            client = APIClient()
            client.access_token = "test-token"
            trades = client.getOpenTradesWithDetails(limit=5)
            
            assert len(trades) == 1
            assert "cardDetails" in trades[0]
            assert trades[0]["cardDetails"]["name"] == "Test Car"
        
        @patch('requests.get')
        def test_enriches_card_for_card_trades_with_want_card(self, mock_get):
            def side_effect(url, **kwargs):
                mock_response = Mock()
                mock_response.raise_for_status = Mock()
                
                if "trades" in url:
                    mock_response.json.return_value = {
                        "trades": [
                            {"id": "trade-1", "cardId": "card-1", "wantCardId": "card-2"}
                        ]
                    }
                elif "cards/card-1" in url:
                    mock_response.json.return_value = {"id": "card-1", "name": "Car A"}
                elif "cards/card-2" in url:
                    mock_response.json.return_value = {"id": "card-2", "name": "Car B"}
                return mock_response
            
            mock_get.side_effect = side_effect
            
            client = APIClient()
            client.access_token = "test-token"
            trades = client.getOpenTradesWithDetails(limit=5)
            
            assert "wantCardDetails" in trades[0]
            assert trades[0]["wantCardDetails"]["name"] == "Car B"
        
        @patch('requests.get')
        def test_enriches_completed_trades_with_both_cards(self, mock_get):
            def side_effect(url, **kwargs):
                mock_response = Mock()
                mock_response.raise_for_status = Mock()
                
                if "history" in url:
                    mock_response.json.return_value = {
                        "trades": [
                            {
                                "id": "trade-1",
                                "sellerCardId": "card-1",
                                "buyerCardId": "card-2"
                            }
                        ]
                    }
                elif "cards/card-1" in url:
                    mock_response.json.return_value = {"id": "card-1", "name": "Seller Car"}
                elif "cards/card-2" in url:
                    mock_response.json.return_value = {"id": "card-2", "name": "Buyer Car"}
                return mock_response
            
            mock_get.side_effect = side_effect
            
            client = APIClient()
            client.access_token = "test-token"
            trades = client.getCompletedTradesWithDetails(limit=5)
            
            assert "sellerCardDetails" in trades[0]
            assert "buyerCardDetails" in trades[0]
            assert trades[0]["sellerCardDetails"]["name"] == "Seller Car"
            assert trades[0]["buyerCardDetails"]["name"] == "Buyer Car"
        
        @patch('requests.get')
        def test_handles_missing_card_details_gracefully(self, mock_get):
            def side_effect(url, **kwargs):
                mock_response = Mock()
                mock_response.raise_for_status = Mock()
                
                if "trades" in url:
                    mock_response.json.return_value = {
                        "trades": [
                            {"id": "trade-1", "cardId": "missing-card"}
                        ]
                    }
                elif "cards/missing-card" in url:
                    mock_response.status_code = 404
                    raise requests.exceptions.HTTPError(response=mock_response)
                return mock_response
            
            mock_get.side_effect = side_effect
            
            client = APIClient()
            client.access_token = "test-token"
            trades = client.getOpenTradesWithDetails(limit=5)
            
            # Should not crash, just not add cardDetails
            assert len(trades) == 1
            assert "cardDetails" not in trades[0]


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
            
            def test_formats_30_seconds_ago_as_just_now(self):
                past = datetime.now() - timedelta(seconds=30)
                result = Display.formatTimeAgo(past)
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
            
            def test_formats_single_hour_without_plural(self):
                past = datetime.now() - timedelta(hours=1)
                result = Display.formatTimeAgo(past)
                assert "1 hour ago" in result
            
            def test_formats_days_ago_correctly(self):
                past = datetime.now() - timedelta(days=2)
                result = Display.formatTimeAgo(past)
                assert "2 days ago" in result
            
            def test_formats_single_day_without_plural(self):
                past = datetime.now() - timedelta(days=1)
                result = Display.formatTimeAgo(past)
                assert "1 day ago" in result
        
        class TestGradeFormatting:
            """Card grade formatting with star symbols"""
            
            def test_formats_factory_grade_with_one_star(self):
                result = Display.formatGrade("FACTORY")
                assert result == "★"
            
            def test_formats_lowercase_factory_grade(self):
                result = Display.formatGrade("factory")
                assert result == "★"
            
            def test_formats_limited_run_grade_with_two_stars(self):
                result = Display.formatGrade("LIMITED_RUN")
                assert result == "★ ★"
            
            def test_formats_limited_with_two_stars(self):
                result = Display.formatGrade("LIMITED")
                assert result == "★ ★"
            
            def test_formats_nismo_grade_with_three_stars(self):
                result = Display.formatGrade("NISMO")
                assert result == "★ ★ ★"
            
            def test_formats_unknown_grade_with_placeholder(self):
                result = Display.formatGrade("UNKNOWN")
                assert result == "¯¯¯"
            
            def test_handles_none_grade(self):
                result = Display.formatGrade(None) # type: ignore[arg-type]
                assert result == "¯¯¯"
    
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
                    "buyer_username": "TestBuyer",
                    "vehicle": "Test Car",
                    "grade": "FACTORY",
                    "price": 5000,
                    "executed_date": datetime.now() - timedelta(minutes=10),
                    "seller_vehicle": "Test Car"
                }
            ]
            display.showCompletedTrades(trades)
            
            captured = capsys.readouterr()
            assert "COMPLETED TRADES" in captured.out
            assert "TestBuyer" in captured.out
            assert "┌────────────┐" in captured.out
            assert "└────────────┘" in captured.out
            assert "★" in captured.out
            assert "C A R" in captured.out
            assert "D E X" in captured.out
            assert "©5,000" in captured.out
        
        def test_renders_card_trade_with_both_vehicles(self, capsys):
            display = Display()
            trades = [
                {
                    "id": "test-002",
                    "type": "FOR_CARD",
                    "buyer_username": "Buyer1",
                    "seller_vehicle": "Car A",
                    "buyer_vehicle": "Car B",
                    "grade": "NISMO",
                    "price": 0,
                    "executed_date": datetime.now(),
                    "vehicle": "Car A"
                }
            ]
            display.showCompletedTrades(trades)
            
            captured = capsys.readouterr()
            assert "Buyer1" in captured.out
            assert "Car B → Car A" in captured.out
            assert "★ ★ ★" in captured.out
    
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
            assert "©10,000" in captured.out
        
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
                    "name": "Test Collection",
                    "cardCount": 25,
                    "description": "Test pack description"
                }
            ]
            Display.showPacks(packs)
            
            captured = capsys.readouterr()
            assert "SHOP" in captured.out
            assert "Test Collection" in captured.out
            assert "25 possible cards" in captured.out
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
            assert "Test collection description" in captured.out
    
    class TestLogoAndEasterEggs:
        """ASCII art and visual elements"""
        
        def test_displays_cardex_logo_on_startup(self, capsys):
            Display.showCardexLogo()
            captured = capsys.readouterr()
            assert "═══" in captured.out
            assert "L I V E   M A R K E T" in captured.out
            assert "MNNNNNNNN" in captured.out
        
        def test_displays_car_ascii_art_with_beep_beep(self, capsys):
            Display.showCar()
            captured = capsys.readouterr()
            assert "beep beep" in captured.out


# ============================================================================
# CLI CLIENT TESTS
# ============================================================================

class TestCLIClient:
    """Tests for main CLI application - command processing and flow control"""
    
    class TestInitialization:
        """CLI initialization tests"""
        
        @patch('os.system')
        def test_initializes_with_default_api_client(self, mock_system):
            cli = CLIClient()
            assert cli.api_client is not None
            assert cli.display is not None
            assert cli.running is False
            assert cli.username is None
            mock_system.assert_called_once()
        
        @patch('os.system')
        def test_initializes_with_custom_api_client(self, mock_system):
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
    
    class TestTransformations:
        """Data transformation tests"""
        
        def test_parses_iso_timestamp_with_z_suffix(self):
            iso_string = "2024-01-15T10:30:00Z"
            result = CLIClient.parseISOTimestamp(iso_string)
            assert isinstance(result, datetime)
            assert result.year == 2024
            assert result.month == 1
        
        def test_parses_iso_timestamp_without_z_suffix(self):
            iso_string = "2024-01-15T10:30:00"
            result = CLIClient.parseISOTimestamp(iso_string)
            assert isinstance(result, datetime)
        
        def test_handles_invalid_timestamp_gracefully(self, capsys):
            result = CLIClient.parseISOTimestamp("invalid-date")
            assert isinstance(result, datetime)
            captured = capsys.readouterr()
            assert "Warning" in captured.out
        
        def test_transforms_completed_price_trade(self):
            trade = {
                "sellerCardDetails": {
                    "name": "Test Car",
                    "grade": "factory"
                },
                "buyerUsername": "TestBuyer",
                "price": 5000,
                "executedDate": "2024-01-15T10:00:00Z"
            }
            
            result = CLIClient.transformCompletedTrade(trade)
            
            assert result["vehicle"] == "Test Car"
            assert result["grade"] == "FACTORY"
            assert result["buyer_username"] == "TestBuyer"
            assert result["type"] == "FOR_PRICE"
            assert result["price"] == 5000
        
        def test_transforms_completed_card_trade(self):
            trade = {
                "sellerCardDetails": {"name": "Car A", "grade": "nismo"},
                "buyerCardDetails": {"name": "Car B", "grade": "limited"},
                "buyerUsername": "Trader",
                "price": 0,
                "executedDate": "2024-01-15T10:00:00Z"
            }
            
            result = CLIClient.transformCompletedTrade(trade)
            
            assert result["seller_vehicle"] == "Car A"
            assert result["buyer_vehicle"] == "Car B"
            assert result["type"] == "FOR_CARD"
        
        def test_transforms_open_price_trade(self):
            trade = {
                "cardDetails": {
                    "name": "Open Car",
                    "grade": "LIMITED_RUN"
                },
                "username": "Seller123",
                "price": 10000
            }
            
            result = CLIClient.transformOpenTrade(trade)
            
            assert result["vehicle"] == "Open Car"
            assert result["grade"] == "LIMITED_RUN"
            assert result["seller_username"] == "Seller123"
            assert result["type"] == "FOR_PRICE"
            assert result["want_vehicle"] is None
        
        def test_transforms_open_card_trade(self):
            trade = {
                "cardDetails": {"name": "Offering Car", "grade": "factory"},
                "wantCardDetails": {"name": "Wanted Car"},
                "username": "Trader",
                "price": 0
            }
            
            result = CLIClient.transformOpenTrade(trade)
            
            assert result["type"] == "FOR_CARD"
            assert result["want_vehicle"] == "Wanted Car"
        
        def test_transforms_collection(self):
            collection = {
                "id": "col-1",
                "name": "JDM Legends",
                "description": "Classic JDM cars",
                "cardCount": 15
            }
            
            result = CLIClient.transformCollection(collection)
            
            assert result["name"] == "JDM Legends"
            assert result["desc"] == "Classic JDM cars"
            assert result["cardCount"] == 15
            assert result["vehicle_count"] == 15
            assert result["pack_price"] == 2000
        
        def test_handles_missing_fields_in_transformation(self):
            trade = {}
            result = CLIClient.transformCompletedTrade(trade)
            
            assert result["vehicle"] == "Unknown Vehicle"
            assert result["buyer_username"] == "Unknown"
            assert result["grade"] == "FACTORY"
    
    class TestWelcomeAndHelp:
        """Welcome message and help display tests"""
        
        @patch('os.system')
        def test_displays_login_screen(self, mock_system, capsys):
            cli = CLIClient()
            cli.showLogin()
            captured = capsys.readouterr()
            assert "Please login below" in captured.out
        
        @patch('os.system')
        def test_displays_welcome_message_with_username(self, mock_system, capsys):
            cli = CLIClient()
            cli.username = "TestUser"
            cli.showWelcome()
            captured = capsys.readouterr()
            assert "Welcome back TestUser!" in captured.out
            assert "help" in captured.out
        
        @patch('os.system')
        def test_displays_all_available_commands_in_help(self, mock_system, capsys):
            cli = CLIClient()
            cli.showHelp()
            captured = capsys.readouterr()
            assert "trades" in captured.out
            assert "open" in captured.out
            assert "vroom" in captured.out
            assert "shop" in captured.out
            assert "collections" in captured.out
            assert "exit" in captured.out
    
    class TestCommandHandlers:
        """Individual command handler tests"""
        
        @patch('os.system')
        def test_handles_trades_command_by_fetching_completed_trades(self, mock_system):
            mock_client = Mock()
            mock_client.getCompletedTradesWithDetails.return_value = []
            cli = CLIClient(api_client=mock_client)
            cli.handleTrades()
            mock_client.getCompletedTradesWithDetails.assert_called_once_with(limit=5)
        
        @patch('os.system')
        def test_handles_trades_command_with_error(self, mock_system, capsys):
            mock_client = Mock()
            mock_client.getCompletedTradesWithDetails.side_effect = Exception("API Error")
            cli = CLIClient(api_client=mock_client)
            cli.handleTrades()
            captured = capsys.readouterr()
            assert "Error fetching completed trades" in captured.out
        
        @patch('os.system')
        def test_handles_open_command_by_fetching_open_trades(self, mock_system):
            mock_client = Mock()
            mock_client.getOpenTradesWithDetails.return_value = []
            cli = CLIClient(api_client=mock_client)
            cli.handleOpen()
            mock_client.getOpenTradesWithDetails.assert_called_once_with(limit=5)
        
        @patch('os.system')
        def test_handles_open_command_with_error(self, mock_system, capsys):
            mock_client = Mock()
            mock_client.getOpenTradesWithDetails.side_effect = Exception("API Error")
            cli = CLIClient(api_client=mock_client)
            cli.handleOpen()
            captured = capsys.readouterr()
            assert "Error fetching open trades" in captured.out
        
        @patch('os.system')
        def test_handles_vroom_command_by_showing_car(self, mock_system, capsys):
            cli = CLIClient()
            cli.handleVroom()
            captured = capsys.readouterr()
            assert "beep beep" in captured.out
        
        @patch('os.system')
        def test_handles_shop_command_by_fetching_packs(self, mock_system):
            mock_client = Mock()
            mock_client.getAvailablePacks.return_value = []
            cli = CLIClient(api_client=mock_client)
            cli.handleShop()
            mock_client.getAvailablePacks.assert_called_once()
        
        @patch('os.system')
        def test_handles_shop_command_with_error(self, mock_system, capsys):
            mock_client = Mock()
            mock_client.getAvailablePacks.side_effect = Exception("API Error")
            cli = CLIClient(api_client=mock_client)
            cli.handleShop()
            captured = capsys.readouterr()
            assert "Error fetching packs" in captured.out
        
        @patch('os.system')
        def test_handles_collections_command_by_fetching_collections(self, mock_system):
            mock_client = Mock()
            mock_client.getCollections.return_value = []
            cli = CLIClient(api_client=mock_client)
            cli.handleCollections()
            mock_client.getCollections.assert_called_once()
        
        @patch('os.system')
        def test_handles_collections_command_with_error(self, mock_system, capsys):
            mock_client = Mock()
            mock_client.getCollections.side_effect = Exception("API Error")
            cli = CLIClient(api_client=mock_client)
            cli.handleCollections()
            captured = capsys.readouterr()
            assert "Error fetching collections" in captured.out
    
    class TestCommandProcessing:
        """Command parsing and processing tests"""
        
        @patch('os.system')
        def test_processes_exit_command_and_returns_false(self, mock_system):
            cli = CLIClient()
            result = cli.processCommand("exit")
            assert result is False
        
        @patch('os.system')
        def test_processes_help_command_and_displays_help(self, mock_system, capsys):
            cli = CLIClient()
            result = cli.processCommand("help")
            assert result is True
            captured = capsys.readouterr()
            assert "Available Commands" in captured.out
        
        @patch('os.system')
        def test_processes_trades_command_successfully(self, mock_system):
            mock_client = Mock()
            mock_client.getCompletedTradesWithDetails.return_value = []
            cli = CLIClient(api_client=mock_client)
            result = cli.processCommand("trades")
            assert result is True
        
        @patch('os.system')
        def test_processes_open_command_successfully(self, mock_system):
            mock_client = Mock()
            mock_client.getOpenTradesWithDetails.return_value = []
            cli = CLIClient(api_client=mock_client)
            result = cli.processCommand("open")
            assert result is True
        
        @patch('os.system')
        def test_processes_vroom_command_successfully(self, mock_system, capsys):
            cli = CLIClient()
            result = cli.processCommand("vroom")
            assert result is True
            captured = capsys.readouterr()
            assert "beep beep" in captured.out
        
        @patch('os.system')
        def test_processes_shop_command_successfully(self, mock_system):
            mock_client = Mock()
            mock_client.getAvailablePacks.return_value = []
            cli = CLIClient(api_client=mock_client)
            result = cli.processCommand("shop")
            assert result is True
        
        @patch('os.system')
        def test_processes_collections_command_successfully(self, mock_system):
            mock_client = Mock()
            mock_client.getCollections.return_value = []
            cli = CLIClient(api_client=mock_client)
            result = cli.processCommand("collections")
            assert result is True
        
        @patch('os.system')
        def test_ignores_empty_commands(self, mock_system):
            cli = CLIClient()
            result = cli.processCommand("")
            assert result is True
        
        @patch('os.system')
        def test_handles_unknown_commands_gracefully(self, mock_system, capsys):
            cli = CLIClient()
            result = cli.processCommand("invalid")
            assert result is True
            captured = capsys.readouterr()
            assert "Unknown command" in captured.out
        
        @patch('os.system')
        def test_processes_commands_case_insensitively(self, mock_system):
            cli = CLIClient()
            assert cli.processCommand("EXIT") is False
            assert cli.processCommand("Exit") is False
            assert cli.processCommand("eXiT") is False
        
        @patch('os.system')
        def test_processes_login_successfully(self, mock_system):
            mock_client = Mock()
            mock_client.login.return_value = True
            cli = CLIClient(api_client=mock_client)
            result = cli.processLogin("testuser", "testpass")
            assert result is True
        
        @patch('os.system')
        def test_processes_login_failure(self, mock_system):
            mock_client = Mock()
            mock_client.login.return_value = False
            cli = CLIClient(api_client=mock_client)
            result = cli.processLogin("baduser", "badpass")
            assert result is False
    
    class TestApplicationFlow:
        """Main application flow and loop tests"""
        
        @patch('os.system')
        def test_exits_gracefully_when_connection_fails(self, mock_system, capsys):
            mock_client = Mock()
            mock_client.connect.return_value = False
            cli = CLIClient(api_client=mock_client)
            cli.run()
            captured = capsys.readouterr()
            assert "Failed to connect" in captured.out
        
        @patch('os.system')
        @patch('getpass.getpass')
        @patch('builtins.input')
        def test_runs_login_and_command_loop_until_exit(self, mock_input, mock_getpass, mock_system, capsys):
            mock_input.side_effect = ['testuser', 'help', 'exit']
            mock_getpass.return_value = 'testpass'
            mock_client = Mock()
            mock_client.connect.return_value = True
            mock_client.login.return_value = True
            cli = CLIClient(api_client=mock_client)
            cli.run()
            
            captured = capsys.readouterr()
            assert "Welcome back testuser!" in captured.out
            assert "Goodbye!" in captured.out
        
        @patch('os.system')
        @patch('getpass.getpass')
        @patch('builtins.input')
        def test_handles_failed_login_and_retries(self, mock_input, mock_getpass, mock_system):
            mock_input.side_effect = ['baduser', 'gooduser', 'exit']
            mock_getpass.side_effect = ['badpass', 'goodpass']
            mock_client = Mock()
            mock_client.connect.return_value = True
            mock_client.login.side_effect = [False, True]
            cli = CLIClient(api_client=mock_client)
            cli.run()
            
            # Should have attempted login twice
            assert mock_client.login.call_count == 2
        
        @patch('os.system')
        @patch('builtins.input')
        def test_handles_keyboard_interrupt_during_login(self, mock_input, mock_system, capsys):
            mock_input.side_effect = KeyboardInterrupt()
            mock_client = Mock()
            mock_client.connect.return_value = True
            cli = CLIClient(api_client=mock_client)
            cli.run()
            
            captured = capsys.readouterr()
            assert "Exiting CarDex Live Market" in captured.out
        
        @patch('os.system')
        @patch('getpass.getpass')
        @patch('builtins.input')
        def test_handles_keyboard_interrupt_during_command_loop(self, mock_input, mock_getpass, mock_system, capsys):
            mock_input.side_effect = ['testuser', KeyboardInterrupt()]
            mock_getpass.return_value = 'testpass'
            mock_client = Mock()
            mock_client.connect.return_value = True
            mock_client.login.return_value = True
            cli = CLIClient(api_client=mock_client)
            cli.run()
            
            captured = capsys.readouterr()
            assert "Exiting CarDex Live Market" in captured.out
        
        @patch('os.system')
        @patch('builtins.input')
        def test_handles_eof_error_during_login(self, mock_input, mock_system, capsys):
            mock_input.side_effect = EOFError()
            mock_client = Mock()
            mock_client.connect.return_value = True
            cli = CLIClient(api_client=mock_client)
            cli.run()
            
            captured = capsys.readouterr()
            assert "Exiting CarDex Live Market" in captured.out
        
        @patch('os.system')
        @patch('getpass.getpass')
        @patch('builtins.input')
        def test_handles_eof_error_during_command_loop(self, mock_input, mock_getpass, mock_system, capsys):
            mock_input.side_effect = ['testuser', EOFError()]
            mock_getpass.return_value = 'testpass'
            mock_client = Mock()
            mock_client.connect.return_value = True
            mock_client.login.return_value = True
            cli = CLIClient(api_client=mock_client)
            cli.run()
            
            captured = capsys.readouterr()
            assert "Exiting CarDex Live Market" in captured.out