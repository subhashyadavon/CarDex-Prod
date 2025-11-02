"""
API Client for CarDex - Handles all server communication
"""
from datetime import datetime, timedelta
from typing import List, Dict, Optional


class APIClient:
    """Client for communicating with the CarDex API"""
    
    def __init__(self, base_url: str = "http://localhost:3000"):
        """Initialize API client with server URL"""
        self.base_url = base_url
        self.connected = False
    
    def connect(self) -> bool:
        """
        Connect to the CarDex server
        
        Returns:
            bool: True if connection successful, False otherwise
        """
        # TODO: Replace with actual API connection
        # Example: requests.get(f"{self.base_url}/health")
        print("Connecting to CarDex server...")
        self.connected = True
        print("Connected successfully!")
        return True
    
    def get_completed_trades(self, limit: int = 5) -> List[Dict]:
        """
        Fetch completed trades from the API
        
        Args:
            limit: Maximum number of trades to return
            
        Returns:
            List of trade dictionaries
        """
        # TODO: Replace with actual API call
        # Example: requests.get(f"{self.base_url}/api/trades/completed?limit={limit}")
        
        # Dummy data for now
        dummy_trades = [
            {
                "id": "trade-001",
                "type": "FOR_PRICE",
                "seller_username": "SpeedDemon",
                "buyer_username": "CarCollector",
                "vehicle": "1999 Nissan Skyline GT-R R34",
                "grade": "NISMO",
                "price": 15000,
                "executed_date": datetime.now() - timedelta(minutes=5)
            },
            {
                "id": "trade-002",
                "type": "FOR_CARD",
                "seller_username": "JDMKing",
                "buyer_username": "TurboFan",
                "seller_vehicle": "2020 Toyota Supra",
                "buyer_vehicle": "1995 Mazda RX-7",
                "grade": "LIMITED_RUN",
                "price": 0,
                "executed_date": datetime.now() - timedelta(minutes=15)
            },
            {
                "id": "trade-003",
                "type": "FOR_PRICE",
                "seller_username": "DriftMaster",
                "buyer_username": "NissanFan",
                "vehicle": "2015 Nissan 370Z",
                "grade": "FACTORY",
                "price": 5000,
                "executed_date": datetime.now() - timedelta(hours=1)
            },
            {
                "id": "trade-004",
                "type": "FOR_PRICE",
                "seller_username": "HondaLife",
                "buyer_username": "VtecKicker",
                "vehicle": "2000 Honda S2000",
                "grade": "LIMITED_RUN",
                "price": 8500,
                "executed_date": datetime.now() - timedelta(hours=2)
            },
            {
                "id": "trade-005",
                "type": "FOR_CARD",
                "seller_username": "RotaryFan",
                "buyer_username": "ClassicCollector",
                "seller_vehicle": "1993 Mazda RX-7 FD",
                "buyer_vehicle": "2002 Acura NSX",
                "grade": "NISMO",
                "price": 0,
                "executed_date": datetime.now() - timedelta(hours=3)
            }
        ]
        
        return dummy_trades[:limit]
    
    def get_open_trades(self, limit: int = 5) -> List[Dict]:
        """
        Fetch open trades from the API
        
        Args:
            limit: Maximum number of trades to return
            
        Returns:
            List of open trade dictionaries
        """
        # TODO: Replace with actual API call
        # Example: requests.get(f"{self.base_url}/api/trades/open?limit={limit}")
        
        # Dummy data for now
        dummy_trades = [
            {
                "id": "open-001",
                "type": "FOR_PRICE",
                "seller_username": "FastCars",
                "vehicle": "2021 Honda Civic Type R",
                "grade": "LIMITED_RUN",
                "price": 12000,
                "want_vehicle": None
            },
            {
                "id": "open-002",
                "type": "FOR_CARD",
                "seller_username": "JDMCollector",
                "vehicle": "1998 Toyota Supra",
                "grade": "NISMO",
                "price": 0,
                "want_vehicle": "Any Nissan GT-R"
            },
            {
                "id": "open-003",
                "type": "FOR_PRICE",
                "seller_username": "DriftKing",
                "vehicle": "2003 Nissan 350Z",
                "grade": "FACTORY",
                "price": 4500,
                "want_vehicle": None
            },
            {
                "id": "open-004",
                "type": "FOR_PRICE",
                "seller_username": "TurboLover",
                "vehicle": "2019 Subaru WRX STI",
                "grade": "LIMITED_RUN",
                "price": 9000,
                "want_vehicle": None
            },
            {
                "id": "open-005",
                "type": "FOR_CARD",
                "seller_username": "RotaryLife",
                "vehicle": "1991 Mazda RX-7 FC",
                "grade": "FACTORY",
                "price": 0,
                "want_vehicle": "Any Honda S2000"
            }
        ]
        
        return dummy_trades[:limit]
    
    def get_available_packs(self) -> List[Dict]:
        """
        Fetch available packs from the API
        
        Returns:
            List of pack dictionaries
        """
        # TODO: Replace with actual API call
        # Example: requests.get(f"{self.base_url}/api/packs")
        
        # Dummy data for now
        return [
            {
                "id": "pack-001",
                "collection_name": "JDM Legends",
                "price": 1000,
                "description": "Classic Japanese sports cars"
            },
            {
                "id": "pack-002",
                "collection_name": "Modern Marvels",
                "price": 1500,
                "description": "Latest performance vehicles"
            },
            {
                "id": "pack-003",
                "collection_name": "Drift Masters",
                "price": 1200,
                "description": "Perfect for sideways action"
            },
            {
                "id": "pack-004",
                "collection_name": "Nismo Collection",
                "price": 2000,
                "description": "Exclusive Nissan performance editions"
            }
        ]
    
    def get_collections(self) -> List[Dict]:
        """
        Fetch all collections from the API
        
        Returns:
            List of collection dictionaries
        """
        # TODO: Replace with actual API call
        # Example: requests.get(f"{self.base_url}/api/collections")
        
        # Dummy data for now
        return [
            {
                "id": "col-001",
                "name": "JDM Legends",
                "pack_price": 1000,
                "vehicle_count": 25,
                "description": "Iconic Japanese sports cars from the 90s and 2000s"
            },
            {
                "id": "col-002",
                "name": "Modern Marvels",
                "pack_price": 1500,
                "vehicle_count": 30,
                "description": "The latest high-performance vehicles"
            },
            {
                "id": "col-003",
                "name": "Drift Masters",
                "pack_price": 1200,
                "vehicle_count": 20,
                "description": "Cars built for drifting and sideways fun"
            },
            {
                "id": "col-004",
                "name": "Nismo Collection",
                "pack_price": 2000,
                "vehicle_count": 15,
                "description": "Rare Nissan Motorsport International editions"
            },
            {
                "id": "col-005",
                "name": "Classic American Muscle",
                "pack_price": 1800,
                "vehicle_count": 22,
                "description": "Powerful V8 monsters from the USA"
            }
        ]
