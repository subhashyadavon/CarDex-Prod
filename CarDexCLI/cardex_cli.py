#!/usr/bin/env python3

import sys
from api_client import APIClient
from display import Display


class CarDexCLI:

    """Main CLI application for CarDex"""
    
    def __init__(self, api_client=None):

        """Initialize CLI with an API client"""
        self.api_client = api_client or APIClient()
        self.display = Display()
        self.running = False
        
    def connect(self):

        """Connect to the CarDex server"""
        return self.api_client.connect()
    
    def show_welcome(self):

        """Display welcome message and ASCII art"""
        self.display.show_cardex_logo()
        print("\nWelcome to CarDex Live Market!")
        print("Type 'help' for available commands or 'exit' to quit.\n")
    
    def show_help(self):

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
    
    def handle_trades(self):

        """Handle the 'trades' command"""
        trades = self.api_client.get_completed_trades(limit=5)
        self.display.show_completed_trades(trades)
    
    def handle_open(self):

        """Handle the 'open' command"""
        trades = self.api_client.get_open_trades(limit=5)
        self.display.show_open_trades(trades)
    
    def handle_vroom(self):

        """Handle the 'vroom' command"""
        self.display.show_car()
    
    def handle_shop(self):

        """Handle the 'shop' command"""
        packs = self.api_client.get_available_packs()
        self.display.show_packs(packs)
    
    def handle_collections(self):

        """Handle the 'collections' command"""
        collections = self.api_client.get_collections()
        self.display.show_collections(collections)
    
    def process_command(self, command):

        """Process a user command and return True to continue, False to exit"""
        command = command.strip().lower()
        
        if command == 'exit':
            return False
        elif command == 'help':
            self.show_help()
        elif command == 'trades':
            self.handle_trades()
        elif command == 'open':
            self.handle_open()
        elif command == 'vroom':
            self.handle_vroom()
        elif command == 'shop':
            self.handle_shop()
        elif command == 'collections':
            self.handle_collections()
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
        self.show_welcome()
        
        # Main command loop
        self.running = True
        while self.running:
            try:
                command = input("cardex> ").strip()
                self.running = self.process_command(command)
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

    cli = CarDexCLI()
    cli.run()

if __name__ == "__main__":
    main()