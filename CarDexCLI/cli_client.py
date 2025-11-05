#!/usr/bin/env python3

import sys
from api_client  import APIClient
from cli_display import Display


class CLIClient:

    """Main CLI application for CarDex"""
    
    def __init__(self, api_client=None):

        """Initialize CLI with an API client"""
        self.api_client = api_client or APIClient()
        self.display = Display()
        self.running = False
        
    def connect(self):

        """Connect to the CarDex server"""
        return self.api_client.connect()
    
    def showWelcome(self):

        """Display welcome message and ASCII art"""
        self.display.showCardexLogo()
        print("\nWelcome to CarDex Live Market!")
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
  exit        - Exit the application
"""
        print(help_text)
    
    def handleTrades(self):

        """Handle the 'trades' command"""
        trades = self.api_client.getCompletedTrades(limit=5)
        self.display.showCompletedTrades(trades)
    
    def handleOpen(self):

        """Handle the 'open' command"""
        trades = self.api_client.getOpenTrades(limit=5)
        self.display.showOpenTrades(trades)
    
    def handleVroom(self):

        """Handle the 'vroom' command"""
        self.display.showCar()
    
    def handleShop(self):

        """Handle the 'shop' command"""
        packs = self.api_client.getAvailablePacks()
        self.display.showPacks(packs)
    
    def handleCollections(self):

        """Handle the 'collections' command"""
        collections = self.api_client.getCollections()
        self.display.showCollections(collections)
    
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
        
        # Show welcome screen
        self.showWelcome()
        
        # Main command loop
        self.running = True
        while self.running:
            try:
                command = input("cardex> ").strip()
                self.running = self.processCommand(command)
            except KeyboardInterrupt:
                print("\n\nExiting CarDex CLI. Thanks for playing!")
                break
            except EOFError:
                print("\n\nExiting CarDex CLI. Thanks for playing!")
                break
        
        print("Goodbye!")

# main()
# Run the app.
def main():

    cli = CLIClient()
    cli.run()

if __name__ == "__main__":
    main()