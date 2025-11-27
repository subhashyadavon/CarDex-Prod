#!/usr/bin/env python3

import sys
import getpass
import os
from datetime import datetime

from api_client  import APIClient
from cli_display import Display


class CLIClient:

    """Main CLI application for CarDex"""
    
    def __init__(self, api_client=None):

        """Initialize CLI with an API client"""
        self.api_client = api_client or APIClient()
        self.display = Display()
        self.running = False
        
        self.username = None

        # Clear the terminal
        os.system('cls' if os.name == 'nt' else 'clear')
    
    @staticmethod
    def parseISOTimestamp(iso_string: str) -> datetime:
        """
        Parse ISO 8601 timestamp string to datetime object
        Handles both with and without 'Z' suffix
        """
        try:
            # Remove 'Z' and parse
            if iso_string.endswith('Z'):
                iso_string = iso_string[:-1]
            # Parse the timestamp (assumes UTC)
            return datetime.fromisoformat(iso_string)
        except Exception as e:
            print(f"Warning: Could not parse timestamp '{iso_string}': {e}")
            return datetime.now()
    
    @staticmethod
    def transformCompletedTrade(trade: dict) -> dict:
        """
        Transform API completed trade response to display format
        
        API provides: sellerCardDetails, buyerCardDetails, buyerUsername, executedDate, type, price
        Display expects: grade, executed_date, buyer_username, vehicle, type, price, 
                        seller_vehicle, buyer_vehicle
        """
        transformed = {}
        
        # Extract seller card details
        seller_card = trade.get("sellerCardDetails", {})
        transformed["grade"] = seller_card.get("grade", "FACTORY").upper()
        transformed["vehicle"] = seller_card.get("name", "Unknown Vehicle")
        transformed["seller_vehicle"] = seller_card.get("name", "Unknown Vehicle")
        
        # Extract buyer card details (if card-for-card trade)
        buyer_card = trade.get("buyerCardDetails")
        if buyer_card:
            transformed["buyer_vehicle"] = buyer_card.get("name", "Unknown Vehicle")
        else:
            transformed["buyer_vehicle"] = None
        
        # Extract trade information
        transformed["buyer_username"] = trade.get("buyerUsername", "Unknown")
        transformed["type"] = "FOR_CARD" if buyer_card else "FOR_PRICE"
        transformed["price"] = trade.get("price", 0)
        
        # Parse timestamp
        iso_date = trade.get("executedDate")
        if iso_date:
            transformed["executed_date"] = CLIClient.parseISOTimestamp(iso_date)
        else:
            transformed["executed_date"] = datetime.now()
        
        return transformed
    
    @staticmethod
    def transformOpenTrade(trade: dict) -> dict:
        """
        Transform API open trade response to display format
        
        API provides: cardDetails, wantCardDetails, username, price, type
        Display expects: grade, seller_username, vehicle, price, type, want_vehicle
        """
        transformed = {}
        
        # Extract card details
        card = trade.get("cardDetails", {})
        transformed["grade"] = card.get("grade", "FACTORY").upper()
        transformed["vehicle"] = card.get("name", "Unknown Vehicle")
        
        # Extract want card details (if card-for-card trade)
        want_card = trade.get("wantCardDetails")
        if want_card:
            transformed["want_vehicle"] = want_card.get("name", "Unknown Vehicle")
            transformed["type"] = "FOR_CARD"
        else:
            transformed["want_vehicle"] = None
            transformed["type"] = "FOR_PRICE"
        
        # Extract trade information
        transformed["seller_username"] = trade.get("username", "Unknown")
        transformed["price"] = trade.get("price", 0)
        
        return transformed
    
    @staticmethod
    def transformCollection(collection: dict) -> dict:
        """
        Transform API collection response to display format
        
        API provides: id, name, theme, description, cardCount
        Display expects: name, desc, cardCount (for now)
        """
        return {
            "name": collection.get("name", "Unknown Collection"),
            "desc": collection.get("description", "No description available"),
            "cardCount": collection.get("cardCount", 0),
            "description": collection.get("description", "No description available"),
            "pack_price": collection.get("price", 0),
            "vehicle_count": collection.get("cardCount", 0)
        }
        
    def connect(self):

        """Connect to the CarDex server"""
        return self.api_client.connect()
    
    def showLogin(self):

        """Display login screen and prompt login"""
        self.display.showCardexLogo()
        print("Please login below")

    def showWelcome(self):

        """Display welcome message and ASCII art"""

        # Clear the terminal
        os.system('cls' if os.name == 'nt' else 'clear')

        self.display.showCardexLogo()
        print(f"Welcome back {self.username}!")
        print("Type 'help' for available commands or 'exit' to quit.\n")
        
    
    def showHelp(self):

        """Display available commands"""
        help_text = """
Available Commands:
  open        - Show the top 5 latest open trades
  trades      - Show the top 5 latest completed trades
  shop        - View all available packs and their prices
  collections - View all available collections and their prices
  vroom       - Show a cool car (vroom vroom!)
  help        - Show this help message
  exit        - Log out of CarDex Live Market
"""
        print(help_text)
    
    def handleTrades(self):
        """Handle the 'trades' command - fetch and display completed trades"""
        try:
            # Fetch completed trades WITH card details
            trades = self.api_client.getCompletedTradesWithDetails(limit=5)
            
            # Transform each trade to display format
            transformed_trades = [self.transformCompletedTrade(t) for t in trades]
            
            # Display
            self.display.showCompletedTrades(transformed_trades)
            
        except Exception as e:
            print(f"Error fetching completed trades: {e}")
    
    def handleOpen(self):
        """Handle the 'open' command - fetch and display open trades"""
        try:
            # Fetch open trades WITH card details
            trades = self.api_client.getOpenTradesWithDetails(limit=5)
            
            # Transform each trade to display format
            transformed_trades = [self.transformOpenTrade(t) for t in trades]
            
            # Display
            self.display.showOpenTrades(transformed_trades)
            
        except Exception as e:
            print(f"Error fetching open trades: {e}")
    
    def handleVroom(self):

        """Handle the 'vroom' command"""
        self.display.showCar()
    
    def handleShop(self):
        """Handle the 'shop' command - fetch and display available packs"""
        try:
            packs = self.api_client.getAvailablePacks()
            
            # Transform collections to display format
            transformed_packs = [self.transformCollection(c) for c in packs]
            
            self.display.showPacks(transformed_packs)
            
        except Exception as e:
            print(f"Error fetching packs: {e}")
    
    def handleCollections(self):
        """Handle the 'collections' command - fetch and display all collections"""
        try:
            collections = self.api_client.getCollections()
            
            # Transform collections to display format
            transformed_collections = [self.transformCollection(c) for c in collections]
            
            self.display.showCollections(transformed_collections)
            
        except Exception as e:
            print(f"Error fetching collections: {e}")
    
    def processLogin(self, username, password):

        success = self.api_client.login(username, password)
        return success


    def processCommand(self, command):

        """Process a user command and return True to continue, False to exit"""
        command = command.strip().lower()
        
        if command == 'exit':
            return False
        elif command == 'help':
            self.showHelp()
        elif command == 'trades':
            self.handleTrades()
        elif command == 'open':
            self.handleOpen()
        elif command == 'vroom':
            self.handleVroom()
        elif command == 'shop':
            self.handleShop()
        elif command == 'collections':
            self.handleCollections()
        elif command == '':
            pass  # Ignore empty commands
        else:
            print(f"Unknown command: '{command}'. Type 'help' for available commands.")
        
        return True
    
    def run(self):
        """Main CLI loop"""
        # Connect to server
        if not self.connect():
            print("Failed to connect to CarDex server. Exiting...")
            return
        
        # Login loop
        self.showLogin()
        while self.username == None:
            try:
                username = input("[Username]: ")
                password = getpass.getpass("[Password]: ")
                success = self.processLogin(username, password)

                if(success):
                    self.username = username
                    self.showWelcome()

            except KeyboardInterrupt:
                print("\n\nExiting CarDex Live Market.")
                return
            except EOFError:
                print("\n\nExiting CarDex Live Market.")
                return
        
        # Main command loop
        self.running = True
        while self.running:
            try:
                command = input("cardex> ").strip()
                self.running = self.processCommand(command)
            except KeyboardInterrupt:
                print("\n\nExiting CarDex Live Market.")
                return
            except EOFError:
                print("\n\nExiting CarDex Live Market.")
                return
        
        print("Goodbye!")

# main()
# Run the app.
def main():

    cli = CLIClient()
    cli.run()

if __name__ == "__main__":
    main()