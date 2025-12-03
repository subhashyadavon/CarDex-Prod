"""
API Client for CarDex - Handles all server communication
CORRECTED based on actual Swagger API specification
"""
from typing import List, Dict, Optional
import requests

# API Paths
BASE_URL        = "http://localhost:8080"
GET_HEALTHCHECK = f"{BASE_URL}/health"
POST_LOGIN      = f"{BASE_URL}/auth/login"
GET_OPEN_TRADES = f"{BASE_URL}/trades"
GET_EXEC_TRADES = f"{BASE_URL}/trades/history"
GET_COLLECTIONS = f"{BASE_URL}/collections"
GET_CARD        = f"{BASE_URL}/cards"  # + /{cardId}


class APIClient:
    """Client for communicating with the CarDex API"""

    def __init__(self):
        """
        Initialize API client with server URL
        """
        self.connected = False
        self.access_token = None

    def connect(self) -> bool:
        """
        Check if .NET server is running and responsive
        
        Returns:
            bool: True if server is reachable, False otherwise
        """

        # Perform healthcheck to see API connection
        self.connected = self.healthCheck()
        return self.connected
        
    def getHeaders(self) -> Dict[str, str]:
        """
        Build HTTP headers with JWT authentication token
        
        Returns:
            Dict with Authorization header
        """
        if not self.access_token:
            raise Exception("Not authenticated. Please login first.")
        
        return { 
            "Authorization": f"Bearer {self.access_token}"
        }
    
    def healthCheck(self) -> bool:
        """
        Ping API using healthcheck endpoint, to test connection

        Returns:
            bool: True if ping was OK, False otherwise
        """

        print("Connecting to CarDex API..................................................", end="", flush=True)

        try:
            response = requests.get(
                GET_HEALTHCHECK,
                timeout=10
            )
            response.raise_for_status()
            
            print("[DONE]")
            return True
            
        except requests.exceptions.RequestException as e:
            print("[FAIL]")
            print(f"Health Check failed: {e}")
            return False


        
    def login(self, username: str, password: str) -> bool:
        """
        Authenticate with .NET API and receive JWT access token
            
        Returns:
            bool: True if login successful, False otherwise
        """
        try:
            response = requests.post(
                POST_LOGIN,
                json={
                    "username": username,
                    "password": password
                },
                timeout=10
            )
            response.raise_for_status()
            
            # Extract token from response
            data = response.json()
            self.access_token = data["accessToken"]
            
            return True
            
        except requests.exceptions.HTTPError  as e:

            if(e.response.status_code == 401):
                print(f"Invalid credentials. To create an account, please use our webapp.\n")

        except requests.exceptions.RequestException as e:

            print(f"Login failed: {e}\n")
            self.access_token = None

        return False

    def getCompletedTrades(self, limit: int = 5) -> List[Dict]:
        """
        Fetch COMPLETED trades (executed transactions)
            
        Returns:
            List[Dict]: Completed trades
        """

        # Build query parameters
        params = {
            "limit": limit,
            "offset": 0 
        }
        
        response = requests.get(
            GET_EXEC_TRADES,
            headers=self.getHeaders(),
            params=params,
            timeout=10
        )
        response.raise_for_status()
        
        data = response.json()
        return data.get("trades", [])

    def getOpenTrades(self, limit: int = 5) -> List[Dict]:
        """
        Fetch OPEN trades (active marketplace listings)
            
        Returns:
            List[Dict]: Open trades matching filters
        """

        # Build query parameters dict, to specify limit
        params = {
            "limit": limit,
            "offset": 0,
            "sortBy": "date_desc"
        }
        
        response = requests.get(
            GET_OPEN_TRADES,
            headers=self.getHeaders(),
            params=params,
            timeout=10
        )
        response.raise_for_status()
        
        data = response.json()
        return data.get("trades", [])

    def getAvailablePacks(self) -> List[Dict]:
        """
        Fetch available packs that can be purchased
        
        Returns:
            List[Dict]: All collections (which represent available packs)
        """
        response = requests.get(
            GET_COLLECTIONS,
            headers=self.getHeaders(),
            timeout=10
        )
        response.raise_for_status()
        
        data = response.json()
        return data.get("collections", [])

    def getCollections(self) -> List[Dict]:
        """
        Fetch all collections in the game
        
        Returns:
            List[Dict]: All collections
        """
        response = requests.get(
            GET_COLLECTIONS,
            headers=self.getHeaders(),
            timeout=10
        )
        response.raise_for_status()
        
        data = response.json()
        return data.get("collections", [])
    
    def getCard(self, card_id: str) -> Optional[Dict]:
        """
        Fetch detailed information about a specific card
        
        Args:
            card_id: UUID of the card to fetch
            
        Returns:
            Dict: Card details including name, grade, value, vehicleId, etc.
                  Returns None if card not found
        """
        try:
            response = requests.get(
                f"{GET_CARD}/{card_id}",
                headers=self.getHeaders(),
                timeout=10
            )
            response.raise_for_status()
            return response.json()
            
        except requests.exceptions.HTTPError as e:
            if e.response.status_code == 404:
                return None
            raise

    def getOpenTradesWithDetails(self, limit: int = 5) -> List[Dict]:
        """
        Fetch OPEN trades with full card details merged in
        
        This function:
        1. Fetches open trades
        2. For each trade, fetches the associated card details
        3. Merges card info into the trade object
        
        Returns:
            List[Dict]: Open trades with card details included
                Each trade will have a 'cardDetails' key with full card info
        """
        trades = self.getOpenTrades(limit)
        
        # Enrich each trade with card details
        for trade in trades:
            card_id = trade.get("cardId")
            if card_id:
                card_details = self.getCard(card_id)
                if card_details:
                    trade["cardDetails"] = card_details
                    
            # Also fetch wantCard details if it's a card-for-card trade
            want_card_id = trade.get("wantCardId")
            if want_card_id:
                want_card_details = self.getCard(want_card_id)
                if want_card_details:
                    trade["wantCardDetails"] = want_card_details
                    
        return trades

    def getCompletedTradesWithDetails(self, limit: int = 5) -> List[Dict]:
        """
        Fetch COMPLETED trades with full card details for both parties
        
        This function:
        1. Fetches completed trades
        2. For each trade, fetches both seller's and buyer's card details
        3. Merges card info into the trade object
        
        Returns:
            List[Dict]: Completed trades with card details included
                Each trade will have 'sellerCardDetails' and optionally 'buyerCardDetails'
        """
        trades = self.getCompletedTrades(limit)
        
        # Enrich each trade with card details from both parties
        for trade in trades:
            # Get seller's card details
            seller_card_id = trade.get("sellerCardId")
            if seller_card_id:
                seller_card = self.getCard(seller_card_id)
                if seller_card:
                    trade["sellerCardDetails"] = seller_card
            
            # Get buyer's card details (if card-for-card trade)
            buyer_card_id = trade.get("buyerCardId")
            if buyer_card_id:
                buyer_card = self.getCard(buyer_card_id)
                if buyer_card:
                    trade["buyerCardDetails"] = buyer_card
                    
        return trades