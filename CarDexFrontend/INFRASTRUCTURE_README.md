# CarDex Frontend Infrastructure

This document describes the API integration layer and state management infrastructure for the CarDex frontend application.

## Project Structure

```
src/
├── api/
│   └── apiClient.ts          # Axios instance with auth interceptors
├── config/
│   └── api.config.ts         # API endpoint configuration
├── context/
│   ├── AuthContext.tsx       # Authentication state management
│   ├── GameContext.tsx       # Game/inventory state management
│   └── TradeContext.tsx      # Trade state management
├── hooks/
│   ├── useAuth.ts            # Hook to access auth state
│   ├── useGame.ts            # Hook to access game state
│   └── useTrade.ts           # Hook to access trade state
└── services/
    ├── authService.ts        # Auth API calls
    ├── cardService.ts        # Card API calls
    ├── packService.ts        # Pack API calls
    ├── tradeService.ts       # Trade API calls
    ├── userService.ts        # User API calls
    └── collectionService.ts  # Collection API calls
```

## Quick Start

### Backend Setup
Make sure the backend API is running on `http://localhost:5083`

### Environment Variables
The `.env` file is configured with:
```
REACT_APP_API_URL=http://localhost:5083
```

## Authentication

### Using Auth in Your Pages

```tsx
import { useAuth } from '../hooks/useAuth';

function LoginPage() {
  const { login, isAuthenticated, user } = useAuth();

  const handleLogin = async () => {
    try {
      await login({ email: 'user@example.com', password: 'password123' });
      // User is now logged in, navigate to dashboard
    } catch (error) {
      // Handle login error
      console.error('Login failed:', error);
    }
  };

  return (
    <div>
      {isAuthenticated ? (
        <p>Welcome, {user?.username}!</p>
      ) : (
        <button onClick={handleLogin}>Login</button>
      )}
    </div>
  );
}
```

### Auth Hook API

```tsx
const {
  user,              // Current user object or null
  token,             // JWT token or null
  isAuthenticated,   // Boolean: true if user is logged in
  isLoading,         // Boolean: true during initial load
  login,             // Function: (credentials) => Promise<void>
  register,          // Function: (userData) => Promise<void>
  logout,            // Function: () => Promise<void>
} = useAuth();
```

## Game State Management

### Using Game State in Your Pages

```tsx
import { useGame } from '../hooks/useGame';
import { useAuth } from '../hooks/useAuth';
import { useEffect } from 'react';

function GaragePage() {
  const { user } = useAuth();
  const { userCards, loadUserCards, isLoading } = useGame();

  useEffect(() => {
    if (user) {
      loadUserCards(user.id);
    }
  }, [user]);

  if (isLoading) return <p>Loading...</p>;

  return (
    <div>
      <h1>My Garage</h1>
      {userCards.map(card => (
        <div key={card.id}>Card #{card.id}</div>
      ))}
    </div>
  );
}
```

### Game Hook API

```tsx
const {
  cards,             // All available cards
  userCards,         // Current user's cards
  packs,             // Available packs
  collection,        // User's collection
  isLoading,         // Loading state
  loadCards,         // Function: () => Promise<void>
  loadUserCards,     // Function: (userId) => Promise<void>
  loadPacks,         // Function: () => Promise<void>
  loadCollection,    // Function: (userId) => Promise<void>
  openPack,          // Function: (packId) => Promise<OpenPackResponse>
  refreshInventory,  // Function: (userId) => Promise<void>
} = useGame();
```

### Opening a Pack

```tsx
import { useGame } from '../hooks/useGame';
import { useAuth } from '../hooks/useAuth';

function OpenPackPage() {
  const { user } = useAuth();
  const { openPack, refreshInventory } = useGame();

  const handleOpenPack = async (packId: number) => {
    try {
      const result = await openPsack(packId);
      console.log('Received cards:', result.cards);
      
      // Refresh user's inventory after opening pack
      if (user) {
        await refreshInventory(user.id);
      }
    } catch (error) {
      console.error('Failed to open pack:', error);
    }
  };

  return <button onClick={() => handleOpenPack(1)}>Open Pack</button>;
}
```

## Trade Management

### Using Trade State in Your Pages

```tsx
import { useTrade } from '../hooks/useTrade';
import { useAuth } from '../hooks/useAuth';
import { useEffect } from 'react';

function TradePage() {
  const { user } = useAuth();
  const { filteredTrades, loadTrades, createTrade, acceptTrade } = useTrade();

  useEffect(() => {
    loadTrades();
  }, []);

  const handleCreateTrade = async () => {
    if (!user) return;
    
    try {
      await createTrade({
        offeringUserId: user.id,
        receivingUserId: 2,
        offeredCardIds: [1, 2],
        requestedCardIds: [3],
      });
      // Trade created successfully
    } catch (error) {
      console.error('Failed to create trade:', error);
    }
  };

  const handleAcceptTrade = async (tradeId: number) => {
    try {
      const completedTrade = await acceptTrade(tradeId);
      console.log('Trade completed:', completedTrade);
    } catch (error) {
      console.error('Failed to accept trade:', error);
    }
  };

  return (
    <div>
      <button onClick={handleCreateTrade}>Create Trade</button>
      {filteredTrades.map(trade => (
        <div key={trade.id}>
          Trade #{trade.id}
          <button onClick={() => handleAcceptTrade(trade.id)}>Accept</button>
        </div>
      ))}
    </div>
  );
}
```

### Trade Hook API

```tsx
const {
  trades,            // All open trades
  filteredTrades,    // Filtered trades based on statusFilter
  statusFilter,      // Current filter (TradeEnum | null)
  isLoading,         // Loading state
  loadTrades,        // Function: () => Promise<void>
  loadUserTrades,    // Function: (userId) => Promise<void>
  createTrade,       // Function: (tradeData) => Promise<OpenTrade>
  acceptTrade,       // Function: (tradeId) => Promise<CompletedTrade>
  cancelTrade,       // Function: (tradeId) => Promise<void>
  setStatusFilter,   // Function: (status) => void
  refreshTrades,     // Function: () => Promise<void>
} = useTrade();
```

## Direct API Service Usage

If you need to call API endpoints directly without using hooks:

```tsx
import { cardService } from '../services/cardService';

async function fetchCardDetails(cardId: number) {
  try {
    const card = await cardService.getCardById(cardId);
    console.log('Card details:', card);
  } catch (error) {
    console.error('Failed to fetch card:', error);
  }
}
```