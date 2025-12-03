-- Migration: Add CURRENCY_FROM_TRADE to reward_enum
-- This fixes the trade buy functionality by adding the missing enum value
-- that the backend expects when creating rewards for completed trades.

-- Add the new enum value to reward_enum
ALTER TYPE reward_enum ADD VALUE IF NOT EXISTS 'CURRENCY_FROM_TRADE';
