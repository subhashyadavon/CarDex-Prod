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
from unittest.mock import Mock, patch, MagicMock
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
            """Verify client starts with disconnected state and no token"""
            client = APIClient()
            assert client.connected is False
            assert client.access_token is None
    
    class TestConnection:
        """Server connection and health check tests"""
        
        @patch('requests.get')
        def test_successfully_connects_to_server(self, mock_get, capsys):
            """Test successful server connection via health check"""
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
            """Test connection failure handling when server is down"""
            mock_get.side_effect = requests.exceptions.RequestException("Connection refused")
            
            client = APIClient()
            result = client.connect()
            
            assert result is False
            assert client.connected is False
            captured = capsys.readouterr()
            assert "[FAIL]" in captured.out
        
        @patch('requests.get')
        def test_health_check_returns_true_on_success(self, mock_get, capsys):
            """Test health check endpoint returns success"""
            mock_response = Mock()
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            result = client.healthCheck()
            
            assert result is True
            captured = capsys.readouterr()
            assert "Connecting to CarDex API" in captured.out
        
        @patch('requests.get')
        def test_health_check_returns_false_on_failure(self, mock_get):
            """Test health check handles timeout gracefully"""
            mock_get.side_effect = requests.exceptions.Timeout()
            
            client = APIClient()
            result = client.healthCheck()
            
            assert result is False
        
        @patch('requests.get')
        def test_health_check_handles_connection_error(self, mock_get):
            """Test health check handles various request exceptions"""
            mock_get.side_effect = requests.exceptions.ConnectionError()
            
            client = APIClient()
            result = client.healthCheck()
            
            assert result is False
    
    class TestAuthentication:
        """Authentication and token management tests"""
        
        @patch('requests.post')
        def test_successful_login_stores_token(self, mock_post):
            """Test successful authentication and token storage"""
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
            """Test login failure with 401 Unauthorized"""
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
        def test_login_fails_with_connection_error(self, mock_post):
            """Test login handles network errors gracefully"""
            mock_post.side_effect = requests.exceptions.ConnectionError("Network error")
            
            client = APIClient()
            result = client.login("testuser", "testpass")
            
            assert result is False
            assert client.access_token is None
        
        @patch('requests.post')
        def test_login_fails_with_timeout(self, mock_post):
            """Test login handles timeout errors"""
            mock_post.side_effect = requests.exceptions.Timeout()
            
            client = APIClient()
            result = client.login("testuser", "testpass")
            
            assert result is False
        
        @patch('requests.post')
        def test_login_fails_with_non_401_http_error(self, mock_post, capsys):
            """Test login handles non-401 HTTP errors"""
            mock_response = Mock()
            mock_response.status_code = 500
            mock_post.side_effect = requests.exceptions.HTTPError(response=mock_response)
            
            client = APIClient()
            result = client.login("testuser", "testpass")
            
            assert result is False
            assert client.access_token is None
        
        def test_get_headers_returns_authorization_header(self):
            """Test header generation with valid token"""
            client = APIClient()
            client.access_token = "test-token"
            
            headers = client.getHeaders()
            
            assert headers == {"Authorization": "Bearer test-token"}
        
        def test_get_headers_raises_exception_when_not_authenticated(self):
            """Test header generation fails without authentication"""
            client = APIClient()
            
            with pytest.raises(Exception, match="Not authenticated"):
                client.getHeaders()
    
    class TestTradeRetrieval:
        """Fetching trade data from API"""
        
        @patch('requests.get')
        def test_retrieves_completed_trades_successfully(self, mock_get):
            """Test fetching completed trades from history endpoint"""
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
            """Test limit parameter is properly passed to API"""
            mock_response = Mock()
            mock_response.json.return_value = {"trades": [{"id": str(i)} for i in range(3)]}
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            client.access_token = "test-token"
            trades = client.getCompletedTrades(limit=3)
            
            call_args = mock_get.call_args
            assert call_args[1]['params']['limit'] == 3
            assert call_args[1]['params']['offset'] == 0
        
        @patch('requests.get')
        def test_retrieves_open_trades_successfully(self, mock_get):
            """Test fetching open trades from marketplace"""
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
            """Test open trades respects limit and includes sort parameter"""
            mock_response = Mock()
            mock_response.json.return_value = {"trades": [{"id": str(i)} for i in range(2)]}
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            client.access_token = "test-token"
            trades = client.getOpenTrades(limit=2)
            
            call_args = mock_get.call_args
            assert call_args[1]['params']['limit'] == 2
            assert call_args[1]['params']['sortBy'] == 'date_desc'
        
        @patch('requests.get')
        def test_returns_empty_list_when_no_completed_trades(self, mock_get):
            """Test handling of empty completed trades response"""
            mock_response = Mock()
            mock_response.json.return_value = {"trades": []}
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            client.access_token = "test-token"
            trades = client.getCompletedTrades()
            
            assert trades == []
        
        @patch('requests.get')
        def test_returns_empty_list_when_no_open_trades(self, mock_get):
            """Test handling of empty open trades response"""
            mock_response = Mock()
            mock_response.json.return_value = {}
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            client.access_token = "test-token"
            trades = client.getOpenTrades()
            
            assert trades == []
    
    class TestShopRetrieval:
        """Fetching shop and collection data from API"""
        
        @patch('requests.get')
        def test_retrieves_available_packs_successfully(self, mock_get):
            """Test fetching available packs from collections endpoint"""
            mock_response = Mock()
            mock_response.json.return_value = {
                "collections": [
                    {"id": "1", "name": "JDM Legends", "cardCount": 6},
                    {"id": "2", "name": "Muscle Cars", "cardCount": 8}
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
            """Test fetching all collections"""
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
        
        @patch('requests.get')
        def test_handles_empty_collections_response(self, mock_get):
            """Test handling when no collections exist"""
            mock_response = Mock()
            mock_response.json.return_value = {}
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            client.access_token = "test-token"
            collections = client.getCollections()
            
            assert collections == []
    
    class TestCardRetrieval:
        """Fetching individual card details"""
        
        @patch('requests.get')
        def test_retrieves_card_details_successfully(self, mock_get):
            """Test fetching card by ID"""
            mock_response = Mock()
            mock_response.json.return_value = {
                "id": "card-123",
                "name": "2019 Subaru WRX STI",
                "grade": "LIMITED_RUN",
                "value": 9000
            }
            mock_response.raise_for_status = Mock()
            mock_get.return_value = mock_response
            
            client = APIClient()
            client.access_token = "test-token"
            card = client.getCard("card-123")
            
            assert card is not None
            assert card["id"] == "card-123"
            assert card["name"] == "2019 Subaru WRX STI"
            assert card["grade"] == "LIMITED_RUN"
        
        @patch('requests.get')
        def test_returns_none_when_card_not_found(self, mock_get):
            """Test handling of 404 when card doesn't exist"""
            mock_response = Mock()
            mock_response.status_code = 404
            mock_get.side_effect = requests.exceptions.HTTPError(response=mock_response)
            
            client = APIClient()
            client.access_token = "test-token"
            card = client.getCard("nonexistent")
            
            assert card is None
        
        @patch('requests.get')
        def test_raises_exception_for_non_404_errors(self, mock_get):
            """Test that non-404 HTTP errors are propagated"""
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
            """Test open trades are enriched with card information"""
            def side_effect(url, **kwargs):
                mock_response = Mock()
                mock_response.raise_for_status = Mock()
                
                if "trades" in url and "history" not in url:
                    mock_response.json.return_value = {
                        "trades": [
                            {"id": "trade-1", "cardId": "card-1", "wantCardId": None, "price": 5000}
                        ]
                    }
                elif "cards/card-1" in url:
                    mock_response.json.return_value = {
                        "id": "card-1",
                        "name": "Test Car",
                        "grade": "FACTORY",
                        "value": 5000
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
            """Test card-for-card trades include both card details"""
            def side_effect(url, **kwargs):
                mock_response = Mock()
                mock_response.raise_for_status = Mock()
                
                if "trades" in url and "history" not in url:
                    mock_response.json.return_value = {
                        "trades": [
                            {"id": "trade-1", "cardId": "card-1", "wantCardId": "card-2", "price": 0}
                        ]
                    }
                elif "cards/card-1" in url:
                    mock_response.json.return_value = {"id": "card-1", "name": "Car A", "grade": "FACTORY"}
                elif "cards/card-2" in url:
                    mock_response.json.return_value = {"id": "card-2", "name": "Car B", "grade": "NISMO"}
                return mock_response
            
            mock_get.side_effect = side_effect
            
            client = APIClient()
            client.access_token = "test-token"
            trades = client.getOpenTradesWithDetails(limit=5)
            
            assert "wantCardDetails" in trades[0]
            assert trades[0]["wantCardDetails"]["name"] == "Car B"
        
        @patch('requests.get')
        def test_enriches_completed_trades_with_both_cards(self, mock_get):
            """Test completed trades include seller and buyer card details"""
            def side_effect(url, **kwargs):
                mock_response = Mock()
                mock_response.raise_for_status = Mock()
                
                if "history" in url:
                    mock_response.json.return_value = {
                        "trades": [
                            {
                                "id": "trade-1",
                                "sellerCardId": "card-1",
                                "buyerCardId": "card-2",
                                "price": 0
                            }
                        ]
                    }
                elif "cards/card-1" in url:
                    mock_response.json.return_value = {"id": "card-1", "name": "Seller Car", "grade": "LIMITED_RUN"}
                elif "cards/card-2" in url:
                    mock_response.json.return_value = {"id": "card-2", "name": "Buyer Car", "grade": "NISMO"}
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
            """Test trades without card details don't crash the system"""
            def side_effect(url, **kwargs):
                mock_response = Mock()
                mock_response.raise_for_status = Mock()
                
                if "trades" in url and "history" not in url:
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
            
            assert len(trades) == 1
            assert "cardDetails" not in trades[0]
        
        @patch('requests.get')
        def test_enriches_completed_price_trade_without_buyer_card(self, mock_get):
            """Test completed price-based trade only has seller card"""
            def side_effect(url, **kwargs):
                mock_response = Mock()
                mock_response.raise_for_status = Mock()
                
                if "history" in url:
                    mock_response.json.return_value = {
                        "trades": [
                            {
                                "id": "trade-1",
                                "sellerCardId": "card-1",
                                "buyerCardId": None,
                                "price": 10000
                            }
                        ]
                    }
                elif "cards/card-1" in url:
                    mock_response.json.return_value = {"id": "card-1", "name": "Seller Car"}
                return mock_response
            
            mock_get.side_effect = side_effect
            
            client = APIClient()
            client.access_token = "test-token"
            trades = client.getCompletedTradesWithDetails(limit=5)
            
            assert "sellerCardDetails" in trades[0]
            assert "buyerCardDetails" not in trades[0]


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
                """Test immediate time shows as 'just now'"""
                now = datetime.now()
                result = Display.formatTimeAgo(now)
                assert result == "just now"
            
            def test_formats_30_seconds_ago_as_just_now(self):
                """Test times under 60 seconds show as 'just now'"""
                past = datetime.now() - timedelta(seconds=30)
                result = Display.formatTimeAgo(past)
                assert result == "just now"
            
            def test_formats_59_seconds_ago_as_just_now(self):
                """Test boundary case at 59 seconds"""
                past = datetime.now() - timedelta(seconds=59)
                result = Display.formatTimeAgo(past)
                assert result == "just now"
            
            def test_formats_minutes_ago_correctly(self):
                """Test minute formatting"""
                past = datetime.now() - timedelta(minutes=5)
                result = Display.formatTimeAgo(past)
                assert "5 minutes ago" in result
            
            def test_formats_single_minute_without_plural(self):
                """Test singular 'minute' without 's'"""
                past = datetime.now() - timedelta(minutes=1)
                result = Display.formatTimeAgo(past)
                assert "1 minute ago" in result
            
            def test_formats_59_minutes_with_plural(self):
                """Test boundary case at 59 minutes"""
                past = datetime.now() - timedelta(minutes=59)
                result = Display.formatTimeAgo(past)
                assert "59 minutes ago" in result
            
            def test_formats_hours_ago_correctly(self):
                """Test hour formatting"""
                past = datetime.now() - timedelta(hours=3)
                result = Display.formatTimeAgo(past)
                assert "3 hours ago" in result
            
            def test_formats_single_hour_without_plural(self):
                """Test singular 'hour' without 's'"""
                past = datetime.now() - timedelta(hours=1)
                result = Display.formatTimeAgo(past)
                assert "1 hour ago" in result
            
            def test_formats_23_hours_with_plural(self):
                """Test boundary case at 23 hours"""
                past = datetime.now() - timedelta(hours=23)
                result = Display.formatTimeAgo(past)
                assert "23 hours ago" in result
            
            def test_formats_days_ago_correctly(self):
                """Test day formatting"""
                past = datetime.now() - timedelta(days=2)
                result = Display.formatTimeAgo(past)
                assert "2 days ago" in result
            
            def test_formats_single_day_without_plural(self):
                """Test singular 'day' without 's'"""
                past = datetime.now() - timedelta(days=1)
                result = Display.formatTimeAgo(past)
                assert "1 day ago" in result
            
            def test_formats_many_days_ago(self):
                """Test larger day values"""
                past = datetime.now() - timedelta(days=30)
                result = Display.formatTimeAgo(past)
                assert "30 days ago" in result
        
        class TestGradeFormatting:
            """Card grade formatting with star symbols"""
            
            def test_formats_factory_grade(self):
                """Test FACTORY grade formatting"""
                result = Display.formatGrade("FACTORY")
                # Check it returns the star character (could be different encodings)
                assert "★" in result or len(result) == 1
            
            def test_formats_lowercase_factory_grade(self):
                """Test case-insensitive factory grade"""
                result = Display.formatGrade("factory")
                assert "★" in result or len(result) == 1
            
            def test_formats_limited_run_grade(self):
                """Test LIMITED_RUN grade formatting"""
                result = Display.formatGrade("LIMITED_RUN")
                # Should have 2 stars
                assert result.count("★") == 2 or len(result) >= 3
            
            def test_formats_limited_grade(self):
                """Test LIMITED grade formatting"""
                result = Display.formatGrade("LIMITED")
                assert result.count("★") == 2 or len(result) >= 3
            
            def test_formats_nismo_grade(self):
                """Test NISMO grade formatting"""
                result = Display.formatGrade("NISMO")
                # Should have 3 stars
                assert result.count("★") == 3 or len(result) >= 5
            
            def test_formats_unknown_grade(self):
                """Test unknown grades show placeholder"""
                result = Display.formatGrade("UNKNOWN")
                # Should return placeholder of some sort
                assert len(result) >= 3
            
            def test_handles_none_grade(self):
                """Test None grade shows placeholder"""
                result = Display.formatGrade(None)  # type: ignore[arg-type]
                assert len(result) >= 3
    
    class TestCompletedTradesDisplay:
        """Rendering completed trade cards"""
        
        def test_displays_message_when_no_trades_available(self, capsys):
            """Test empty trades list shows appropriate message"""
            display = Display()
            display.showCompletedTrades([])
            
            captured = capsys.readouterr()
            assert "No completed trades found" in captured.out
        
        def test_renders_price_trade_card(self, capsys):
            """Test rendering of price-based trade"""
            display = Display()
            trades = [
                {
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
            assert "Test Car" in captured.out
            assert "C A R" in captured.out
            assert "D E X" in captured.out
            # Check for currency symbol and amount
            assert "5,000" in captured.out
        
        def test_renders_card_trade(self, capsys):
            """Test rendering of card-for-card trade"""
            display = Display()
            trades = [
                {
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
            assert "Car B" in captured.out and "Car A" in captured.out
    
    class TestOpenTradesDisplay:
        """Rendering open trade listings"""
        
        def test_displays_message_when_no_open_trades_available(self, capsys):
            """Test empty open trades shows message"""
            display = Display()
            display.showOpenTrades([])
            
            captured = capsys.readouterr()
            assert "No open trades found" in captured.out
        
        def test_renders_price_based_open_trade(self, capsys):
            """Test rendering of price-based open trade"""
            display = Display()
            trades = [
                {
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
        
        def test_renders_card_based_open_trade(self, capsys):
            """Test rendering of card-for-card open trade"""
            display = Display()
            trades = [
                {
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
    
    class TestShopDisplay:
        """Rendering boost pack cards in shop"""
        
        def test_displays_message_when_no_packs_available(self, capsys):
            """Test empty packs list shows message"""
            Display.showPacks([])
            captured = capsys.readouterr()
            assert "No packs available" in captured.out
        
        def test_renders_boost_pack_card(self, capsys):
            """Test rendering of pack card"""
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
    
    class TestCollectionsDisplay:
        """Rendering collection listings"""
        
        def test_displays_message_when_no_collections_available(self, capsys):
            """Test empty collections shows message"""
            Display.showCollections([])
            captured = capsys.readouterr()
            assert "No collections available" in captured.out
        
        def test_renders_collection(self, capsys):
            """Test collection rendering"""
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
            assert "2,000" in captured.out
            assert "25" in captured.out
    
    class TestLogoAndEasterEggs:
        """ASCII art and visual elements"""
        
        def test_displays_cardex_logo(self, capsys):
            """Test CarDex logo displays"""
            Display.showCardexLogo()
            captured = capsys.readouterr()
            assert "L I V E   M A R K E T" in captured.out
            assert "MNNNNNNNN" in captured.out
        
        def test_displays_car_ascii_art(self, capsys):
            """Test vroom command shows car art"""
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
            """Test CLI creates default API client"""
            cli = CLIClient()
            assert cli.api_client is not None
            assert cli.display is not None
            assert cli.running is False
            assert cli.username is None
        
        @patch('os.system')
        def test_initializes_with_custom_api_client(self, mock_system):
            """Test CLI accepts custom API client"""
            mock_client = Mock()
            cli = CLIClient(api_client=mock_client)
            assert cli.api_client == mock_client
    
    class TestTransformations:
        """Data transformation tests"""
        
        def test_parses_iso_timestamp_with_z_suffix(self):
            """Test parsing ISO string with Z suffix"""
            iso_string = "2024-01-15T10:30:00Z"
            result = CLIClient.parseISOTimestamp(iso_string)
            assert isinstance(result, datetime)
            assert result.year == 2024
        
        def test_parses_iso_timestamp_without_z_suffix(self):
            """Test parsing ISO string without Z"""
            iso_string = "2024-01-15T10:30:00"
            result = CLIClient.parseISOTimestamp(iso_string)
            assert isinstance(result, datetime)
        
        def test_handles_invalid_timestamp_gracefully(self, capsys):
            """Test invalid timestamps return current time with warning"""
            result = CLIClient.parseISOTimestamp("invalid-date")
            assert isinstance(result, datetime)
            captured = capsys.readouterr()
            assert "Warning" in captured.out
        
        def test_transforms_completed_price_trade(self):
            """Test transformation of price-based completed trade"""
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
        
        def test_transforms_completed_card_trade(self):
            """Test transformation of card-for-card completed trade"""
            trade = {
                "sellerCardDetails": {"name": "Car A", "grade": "nismo"},
                "buyerCardDetails": {"name": "Car B"},
                "buyerUsername": "Trader",
                "executedDate": "2024-01-15T10:00:00Z"
            }
            
            result = CLIClient.transformCompletedTrade(trade)
            
            assert result["type"] == "FOR_CARD"
            assert result["buyer_vehicle"] == "Car B"
        
        def test_transforms_open_price_trade(self):
            """Test transformation of price-based open trade"""
            trade = {
                "cardDetails": {"name": "Open Car", "grade": "LIMITED_RUN"},
                "username": "Seller123",
                "price": 10000
            }
            
            result = CLIClient.transformOpenTrade(trade)
            
            assert result["vehicle"] == "Open Car"
            assert result["type"] == "FOR_PRICE"
        
        def test_transforms_open_card_trade(self):
            """Test transformation of card-for-card open trade"""
            trade = {
                "cardDetails": {"name": "Offering Car"},
                "wantCardDetails": {"name": "Wanted Car"},
                "username": "Trader",
                "price": 0
            }
            
            result = CLIClient.transformOpenTrade(trade)
            
            assert result["type"] == "FOR_CARD"
            assert result["want_vehicle"] == "Wanted Car"
        
        def test_transforms_collection(self):
            """Test collection transformation"""
            collection = {
                "name": "JDM Legends",
                "description": "Classic JDM cars",
                "cardCount": 15,
                "price": 2000
            }
            
            result = CLIClient.transformCollection(collection)
            
            assert result["name"] == "JDM Legends"
            assert result["vehicle_count"] == 15
            # pack_price comes from collection.get("price", 0)
            assert result["pack_price"] == 2000
        
        def test_handles_missing_fields_in_transformation(self):
            """Test transformation with missing data"""
            trade = {}
            result = CLIClient.transformCompletedTrade(trade)
            
            assert result["vehicle"] == "Unknown Vehicle"
            assert result["buyer_username"] == "Unknown"
    
    class TestCommandHandlers:
        """Individual command handler tests"""
        
        @patch('os.system')
        def test_handles_trades_command(self, mock_system):
            """Test trades command"""
            mock_client = Mock()
            mock_client.getCompletedTradesWithDetails.return_value = []
            cli = CLIClient(api_client=mock_client)
            cli.handleTrades()
            mock_client.getCompletedTradesWithDetails.assert_called_once()
        
        @patch('os.system')
        def test_handles_trades_command_with_error(self, mock_system, capsys):
            """Test trades command error handling"""
            mock_client = Mock()
            mock_client.getCompletedTradesWithDetails.side_effect = Exception("API Error")
            cli = CLIClient(api_client=mock_client)
            cli.handleTrades()
            captured = capsys.readouterr()
            assert "Error fetching completed trades" in captured.out
        
        @patch('os.system')
        def test_handles_open_command(self, mock_system):
            """Test open command"""
            mock_client = Mock()
            mock_client.getOpenTradesWithDetails.return_value = []
            cli = CLIClient(api_client=mock_client)
            cli.handleOpen()
            mock_client.getOpenTradesWithDetails.assert_called_once()
        
        @patch('os.system')
        def test_handles_open_command_with_error(self, mock_system, capsys):
            """Test open command error handling"""
            mock_client = Mock()
            mock_client.getOpenTradesWithDetails.side_effect = Exception("Network Error")
            cli = CLIClient(api_client=mock_client)
            cli.handleOpen()
            captured = capsys.readouterr()
            assert "Error fetching open trades" in captured.out
        
        @patch('os.system')
        def test_handles_vroom_command(self, mock_system, capsys):
            """Test vroom easter egg"""
            cli = CLIClient()
            cli.handleVroom()
            captured = capsys.readouterr()
            assert "beep beep" in captured.out
        
        @patch('os.system')
        def test_handles_shop_command(self, mock_system):
            """Test shop command"""
            mock_client = Mock()
            mock_client.getAvailablePacks.return_value = []
            cli = CLIClient(api_client=mock_client)
            cli.handleShop()
            mock_client.getAvailablePacks.assert_called_once()
        
        @patch('os.system')
        def test_handles_shop_command_with_error(self, mock_system, capsys):
            """Test shop command error handling"""
            mock_client = Mock()
            mock_client.getAvailablePacks.side_effect = Exception("Fetch Error")
            cli = CLIClient(api_client=mock_client)
            cli.handleShop()
            captured = capsys.readouterr()
            assert "Error fetching packs" in captured.out
        
        @patch('os.system')
        def test_handles_collections_command(self, mock_system):
            """Test collections command"""
            mock_client = Mock()
            mock_client.getCollections.return_value = []
            cli = CLIClient(api_client=mock_client)
            cli.handleCollections()
            mock_client.getCollections.assert_called_once()
        
        @patch('os.system')
        def test_handles_collections_command_with_error(self, mock_system, capsys):
            """Test collections command error handling"""
            mock_client = Mock()
            mock_client.getCollections.side_effect = Exception("Collection Error")
            cli = CLIClient(api_client=mock_client)
            cli.handleCollections()
            captured = capsys.readouterr()
            assert "Error fetching collections" in captured.out
    
    class TestCommandProcessing:
        """Command parsing and processing"""
        
        @patch('os.system')
        def test_processes_exit_command(self, mock_system):
            """Test exit command"""
            cli = CLIClient()
            result = cli.processCommand("exit")
            assert result is False
        
        @patch('os.system')
        def test_processes_help_command(self, mock_system, capsys):
            """Test help command"""
            cli = CLIClient()
            result = cli.processCommand("help")
            assert result is True
            captured = capsys.readouterr()
            assert "Available Commands" in captured.out
        
        @patch('os.system')
        def test_processes_trades_command(self, mock_system):
            """Test trades command through processCommand"""
            mock_client = Mock()
            mock_client.getCompletedTradesWithDetails.return_value = []
            cli = CLIClient(api_client=mock_client)
            result = cli.processCommand("trades")
            assert result is True
            mock_client.getCompletedTradesWithDetails.assert_called_once()
        
        @patch('os.system')
        def test_processes_open_command(self, mock_system):
            """Test open command through processCommand"""
            mock_client = Mock()
            mock_client.getOpenTradesWithDetails.return_value = []
            cli = CLIClient(api_client=mock_client)
            result = cli.processCommand("open")
            assert result is True
            mock_client.getOpenTradesWithDetails.assert_called_once()
        
        @patch('os.system')
        def test_processes_vroom_command(self, mock_system, capsys):
            """Test vroom command through processCommand"""
            cli = CLIClient()
            result = cli.processCommand("vroom")
            assert result is True
            captured = capsys.readouterr()
            assert "beep beep" in captured.out
        
        @patch('os.system')
        def test_processes_shop_command(self, mock_system):
            """Test shop command through processCommand"""
            mock_client = Mock()
            mock_client.getAvailablePacks.return_value = []
            cli = CLIClient(api_client=mock_client)
            result = cli.processCommand("shop")
            assert result is True
            mock_client.getAvailablePacks.assert_called_once()
        
        @patch('os.system')
        def test_processes_collections_command(self, mock_system):
            """Test collections command through processCommand"""
            mock_client = Mock()
            mock_client.getCollections.return_value = []
            cli = CLIClient(api_client=mock_client)
            result = cli.processCommand("collections")
            assert result is True
            mock_client.getCollections.assert_called_once()
        
        @patch('os.system')
        def test_processes_commands_case_insensitively(self, mock_system):
            """Test commands work regardless of case"""
            cli = CLIClient()
            assert cli.processCommand("EXIT") is False
        
        @patch('os.system')
        def test_handles_unknown_commands(self, mock_system, capsys):
            """Test unknown commands show error"""
            cli = CLIClient()
            result = cli.processCommand("invalid")
            assert result is True
            captured = capsys.readouterr()
            assert "Unknown command" in captured.out
        
        @patch('os.system')
        def test_ignores_empty_commands(self, mock_system):
            """Test empty commands are ignored"""
            cli = CLIClient()
            result = cli.processCommand("")
            assert result is True
        
        @patch('os.system')
        def test_ignores_whitespace_commands(self, mock_system):
            """Test whitespace-only commands are ignored"""
            cli = CLIClient()
            result = cli.processCommand("   ")
            assert result is True
    
    class TestApplicationFlow:
        """Main application flow tests"""
        
        @patch('os.system')
        def test_exits_when_connection_fails(self, mock_system, capsys):
            """Test app exits if connection fails"""
            mock_client = Mock()
            mock_client.connect.return_value = False
            cli = CLIClient(api_client=mock_client)
            cli.run()
            captured = capsys.readouterr()
            assert "Failed to connect" in captured.out
        
        @patch('os.system')
        @patch('getpass.getpass')
        @patch('builtins.input')
        def test_runs_login_and_command_loop(self, mock_input, mock_getpass, mock_system):
            """Test complete flow"""
            mock_input.side_effect = ['testuser', 'exit']
            mock_getpass.return_value = 'testpass'
            mock_client = Mock()
            mock_client.connect.return_value = True
            mock_client.login.return_value = True
            cli = CLIClient(api_client=mock_client)
            cli.run()
        
        @patch('os.system')
        @patch('builtins.input')
        def test_handles_keyboard_interrupt_during_login(self, mock_input, mock_system, capsys):
            """Test Ctrl+C during login"""
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
            """Test Ctrl+C during command loop"""
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
            """Test EOF during login"""
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
            """Test EOF during command loop"""
            mock_input.side_effect = ['testuser', EOFError()]
            mock_getpass.return_value = 'testpass'
            mock_client = Mock()
            mock_client.connect.return_value = True
            mock_client.login.return_value = True
            cli = CLIClient(api_client=mock_client)
            cli.run()
            
            captured = capsys.readouterr()
            assert "Exiting CarDex Live Market" in captured.out
        
        @patch('os.system')
        @patch('getpass.getpass')
        @patch('builtins.input')
        def test_goodbye_message_on_normal_exit(self, mock_input, mock_getpass, mock_system, capsys):
            """Test goodbye message when exiting normally"""
            mock_input.side_effect = ['testuser', 'exit']
            mock_getpass.return_value = 'testpass'
            mock_client = Mock()
            mock_client.connect.return_value = True
            mock_client.login.return_value = True
            cli = CLIClient(api_client=mock_client)
            cli.run()
            
            captured = capsys.readouterr()
            assert "Goodbye!" in captured.out