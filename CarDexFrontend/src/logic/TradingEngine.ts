import
{ 
  User, 
  Card, 
  OpenTrade, 
  CompletedTrade, 
  TradeEnum,
  ValidationResult 
} from '../types/types';


/*
TradingEnging
Handles all trades, ensuring they are valid before executing them.
*/
export class TradingEngine
{
  
    // VALIDATION
    // > Can a trade be executed between the seller and the user?
    validateTrade(
        trade: OpenTrade, 
        sellerUser: User, 
        buyerUser: User,
        sellerCard: Card,
        buyerCard?: Card
    ):ValidationResult
    {
    
        // 1. Seller must own Card
        if (!sellerUser.owned_cards.includes(trade.card_id))
        {
            return { valid: false,  error: "Seller does not own the card" };
        }

        // 2. Card must belong to Seller
        if (sellerCard.user_id !== sellerUser.id)
        {
            return { valid: false, error: "Card does not belong to seller" };
        }

        // 3. Type must be valid

        // 3a. FOR_PRICE -> currency must be sufficient
        if (trade.type === TradeEnum.FOR_PRICE)
        {
            return this.validateCurrencyTrade(trade, buyerUser);
        }
        // 3b. FOR_CARD -> Card must exist
        else if (trade.type === TradeEnum.FOR_CARD)
        {
            return this.validateCardTrade(trade, buyerUser, buyerCard);
        }
        // 3c. Unsupported trade type
        else
        {
            return { valid: false, error: "Invalid trade type" };
        }

    }

    // VALIDATION HELPERS
    // > Are the currency values valid for this trade?
    private validateCurrencyTrade(
        trade: OpenTrade, 
        buyerUser: User
    ): ValidationResult
    {
        // 1. User must have enough funds
        if (buyerUser.currency < trade.price)
        {
            return { valid: false, error: "Insufficient funds" };
        }

        // 2. Trade must be a positive price
        if (trade.price < 0)
        {
            return { valid: false, error: "Price cannot be negative" };
        }

        // Validation complete
        return { valid: true };
  }

    // > Is the Buyer Card relevant to the trade? 
    private validateCardTrade(
        trade: OpenTrade, 
        buyerUser: User,
        buyerCard?: Card
    ): ValidationResult
    {
        // 1. Want Card must exist
        if (!trade.want_card_id)
        {
            return { valid: false, error: "Card trade must specify wanted card" };
        }

        // 2. Buyer must own the wanted Card
        if (!buyerUser.owned_cards.includes(trade.want_card_id))
        {
            return { valid: false, error: "Buyer does not own the requested card" };
        }

        // 3. Buyer Card must belong to the Buyer
        if (buyerCard && buyerCard.user_id !== buyerUser.id)
        {
            return { valid: false, error: "Requested card does not belong to buyer" };
        }

        // Validation complete
        return { valid: true };
    }

    // TRADE EXECUTION
    // > Executes a validated trade and returns the completed trade record
    executeTrade(
        trade: OpenTrade, 
        sellerUser: User, 
        buyerUser: User,
        sellerCard: Card,
        buyerCard?: Card
    ): CompletedTrade
    {

        // Begin validation
        const validation = this.validateTrade(
            trade, 
            sellerUser, 
            buyerUser, 
            sellerCard, 
            buyerCard
        );

        // Ensure validity
        if (!validation.valid) {
            throw new Error(validation.error);
        }

        // Generate completed trade
        const completedTrade: CompletedTrade = {
            id: this.generateTradeId(),
            type: trade.type,
            seller_user_id: sellerUser.id,
            seller_card_id: trade.card_id,
            buyer_user_id: buyerUser.id,
            buyer_card_id: trade.want_card_id || null,
            executed_date: new Date(),
            price: trade.price
        };

        return completedTrade;
    }

    // GENERIC HELPERS

    // > Calculate card-to-card value fairness
    calculateTradeFairness(
        card1Value: number, 
        card2Value: number,
        thresholdPercent: number = 20
    ): 'fair' | 'unfair'
    {
        const difference = Math.abs(card1Value - card2Value);
        const maxValue = Math.max(card1Value, card2Value);
        const percentDifference = (difference / maxValue) * 100;

        return percentDifference <= thresholdPercent ? 'fair' : 'unfair';
    }

    // > Generate random trade id (returns string to match GUID format)
    private generateTradeId(): string {
        // Generate a simple UUID-like string for frontend use
        // In production, backend will generate actual GUIDs
        return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    }
}