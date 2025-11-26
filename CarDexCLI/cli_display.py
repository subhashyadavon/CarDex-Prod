"""
Display module for CarDex CLI - Handles all output formatting and display
"""
from typing import List, Dict
from datetime import datetime

# Display helpers
D_LOGO = """
═══════════════════════════════════════════════════════════════════════════════
    MNNNNNNNN       NNNN      NNNNNNNNM   NNNNNNNNN    MNNNNNNNN   NNN    NNN
   NNN     NNN    NNN  NNN    NN     NNM  NNM    NNN   MNN          NNN  NNN
  NNN            NNNNNNNNNN   NNMMMNNM    NNM      NN  MNNNNNNNN      NNNN     
   NNN     NNN   NN     NNN   NN    NNM   NNM     NN   MNN          NNN  NNN  
    MNNNNNNNM   NNM      NNN  NN     NNM  NNNNNNNNN    MNNNNNNNN   NNN    NNN
════════════════════════════════ L I V E   M A R K E T ════════════════════════════
"""

D_VROOM = r"""
                  ______
                 /|_||_\`.__
                (   _    _ _\
                =`-(_)--(_)-'

                beep beep
"""


class Display:
    """Handles all display and formatting for the CLI"""
    
    @staticmethod
    def showCardexLogo():
        """Display CARDEX logo"""
        print(D_LOGO)
    
    @staticmethod
    def showCar():
        """Display easter egg art"""
        print(D_VROOM)
    
    @staticmethod
    def formatTimeAgo(dt: datetime) -> str:
        """Format a datetime as a relative time string"""
        now = datetime.now()
        diff = now - dt
        
        if diff.total_seconds() < 60:
            return "just now"
        elif diff.total_seconds() < 3600:
            minutes = int(diff.total_seconds() / 60)
            return f"{minutes} minute{'s' if minutes != 1 else ''} ago"
        elif diff.total_seconds() < 86400:
            hours = int(diff.total_seconds() / 3600)
            return f"{hours} hour{'s' if hours != 1 else ''} ago"
        else:
            days = int(diff.total_seconds() / 86400)
            return f"{days} day{'s' if days != 1 else ''} ago"
    
    @staticmethod
    def formatGrade(grade: str) -> str:
        """
        Format a card grade with visual indicator
        Handles various grade formats from API: factory, limited, nismo, etc.
        """
        # Normalize grade to uppercase for comparison
        grade_upper = str(grade).upper()
        
        # Map various grade formats to star display
        if "FACTORY" in grade_upper:
            return "★"
        elif "LIMITED" in grade_upper:
            return "★ ★"
        elif "NISMO" in grade_upper:
            return "★ ★ ★"
        else:
            # Unknown grade - show placeholder
            return "¯¯¯"
    
    def showCompletedTrades(self, trades: List[Dict]):
        """Display completed trades in card-like box format"""
        if not trades:
            print("No completed trades found.\n")
            return
        
        print("\n" + "=" * 80)
        print("COMPLETED TRADES - Latest 5".center(80))
        print("=" * 80 + "\n")
        
        for i, trade in enumerate(trades, 1):
            stars = self.formatGrade(trade['grade'])
            time_ago = self.formatTimeAgo(trade['executed_date'])
            buyer = trade['buyer_username']
            
            # Determine the trade line based on type
            if trade['type'] == 'FOR_PRICE':
                # Price trade: ©price → vehicle
                trade_line = f"©{trade['price']:,} → {trade['vehicle']}"
                value = trade['price']
            else:  # FOR_CARD
                # Card trade: buyer_vehicle → seller_vehicle
                if trade.get('buyer_vehicle'):
                    trade_line = f"{trade['buyer_vehicle']} → {trade['seller_vehicle']}"
                else:
                    trade_line = f"Bought {trade['seller_vehicle']}"
                value = trade.get('price', 0)  # May not have a value for card trades
            
            # Format value for bottom of card
            value_str = f"©{value:,}"
            
            # Build the card box with info to the right
            print(f"┌────────────┐")
            print(f"│ {stars:<10} │")
            print(f"│            │  {buyer}")
            print(f"│  C A R     │  {time_ago}")
            print(f"│     D E X  │")
            print(f"│            │  {trade_line}")
            print(f"│ {value_str:>10} │")
            print(f"└────────────┘")
            print()
        
        print("=" * 80 + "\n")

    def showOpenTrades(self, trades: List[Dict]):
        """Display open trades in a formatted table"""
        if not trades:
            print("No open trades found.\n")
            return
        
        print("\n" + "=" * 80)
        print("OPEN TRADES - Latest 5".center(80))
        print("=" * 80 + "\n")
        
        for i, trade in enumerate(trades, 1):

            stars = self.formatGrade(trade['grade'])
            seller = trade['seller_username']
            vehicle = trade['vehicle']
            value = f"©{trade['price']}"

            if trade['type'] == 'FOR_PRICE':
                wants = f"©{trade['price']:,}"
            else:  # FOR_CARD
                wants = f"{trade.get('want_vehicle', 'Any Card')}"

            print(f"┌────────────┐")
            print(f"│ {stars:<10} │")
            print(f"│            │  {seller}")
            print(f"│  C A R     │  {vehicle}")
            print(f"│     D E X  │")
            print(f"│            │  ASKING FOR")
            print(f"│ {value:>10} │  {wants}")
            print(f"└────────────┘")
            print()
        
        print("=" * 80 + "\n")

        
    
    @staticmethod
    def showPacks(packs: List[Dict]):
        """Display available packs"""
        if not packs:
            print("No packs available.\n")
            return
        
        print("\n" + "=" * 80)
        print("SHOP - AVAILABLE PACKS".center(80))
        print("=" * 80 + "\n")
        
        for i, pack in enumerate(packs, 1):

            name  = pack['name']
            cards = pack['cardCount']

            print(f" ╦╦╦╦╦╦╦╦╦╦╦╦╦╦")
            print(f" ╠╩╩╩╩╩╩╩╩╩╩╩╩╣")
            print(f" │            │")
            print(f" │ B O O S T  │  {name}")
            print(f" │    P A C K │  {cards} possible cards")
            print(f" │            │")
            print(f" │            │")
            print(f" │            │")
            print(f" ╠╦╦╦╦╦╦╦╦╦╦╦╦╣")
            print(f" ╩╩╩╩╩╩╩╩╩╩╩╩╩╩")
            print()
        
        print("=" * 80 + "\n")
    
    @staticmethod
    def showCollections(collections: List[Dict]):
        """Display all collections"""
        if not collections:
            print("No collections available.\n")
            return
        
        print("\n" + "=" * 80)
        print("ALL COLLECTIONS".center(80))
        print("=" * 80)
        
        for i, col in enumerate(collections, 1):
            print(f"\n[{i}] {col['name']}")
            print("-" * 80)
            print(f"  Price:       ©{col['pack_price']:,}")
            print(f"  Vehicles:    {col['vehicle_count']}")
            print(f"  Description: {col['description']}")
        
        print("\n" + "=" * 80 + "\n")