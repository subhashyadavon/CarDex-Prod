// src/components/CreateTradeModal/CreateTradeModal.tsx

import React, { useState, useEffect } from "react";
import styles from "./CreateTradeModal.module.css";
import "../../App.css";

import Button from "../Button/Button";
import TextInput from "../TextInput/TextInput";
import Card, { CarCardProps } from "../Card/Card";

import { useAuth } from "../../hooks/useAuth";
import { cardService } from "../../services/cardService";
import { useTrade } from "../../hooks/useTrade";
import {
  CardWithVehicle,
  GradeEnum,
  OpenTrade,
  TradeEnum,
} from "../../types/types";

type TradeKind = "currency" | "card";

export interface CreateTradePayload {
  offeredCardId: string; // the card you're listing
  type: TradeKind; // "currency" | "card"
  price?: number;
  requestedCardId?: string; // the CARD ID you want in return (for card trades)
}

interface CreateTradeModalProps {
  mode: "sell" | "trade"; // which button opened it
  onClose: () => void;
  onSubmit: (payload: CreateTradePayload) => void;
}

interface PlayerCard {
  id: string;
  card: CarCardProps;
}

// -------- helpers --------

// Get the card id off an OpenTrade (handles cardId or card_id)
const getTradeCardId = (trade: OpenTrade): string | null => {
  const t: any = trade;
  const raw = t.cardId ?? t.card_id ?? null;
  if (raw === null || raw === undefined) return null;
  return String(raw);
};

// GradeEnum -> rarity string used by Card component
const gradeToRarity = (grade: GradeEnum | string): CarCardProps["rarity"] => {
  if (grade === GradeEnum.NISMO || grade === "NISMO") return "nismo";
  if (grade === GradeEnum.LIMITED_RUN || grade === "LIMITED_RUN")
    return "limited";
  return "factory";
};

// Map CardWithVehicle -> PlayerCard (for user's own cards)
const mapCardWithVehicleToPlayerCard = (
  card: CardWithVehicle
): PlayerCard => {
  const makeModel = `${card.year} ${card.make} ${card.model}`.trim();
  const rarity = gradeToRarity(card.grade);
  const value = card.value ?? 0;

  return {
    id: String(card.id),
    card: {
      makeModel: makeModel || "Unknown Vehicle",
      cardName: `${card.make} ${card.model}`.trim() || "Card",
      imageUrl: card.vehicleImage || "/assets/cards/placeholder-card.png",
      stat1Label: "POWER",
      stat1Value: String(card.stat1 ?? ""),
      stat2Label: "WEIGHT",
      stat2Value: String(card.stat2 ?? ""),
      stat3Label: "BRAKING",
      stat3Value: String(card.stat3 ?? ""),
      stat4Label: "VALUE",
      stat4Value: value ? value.toLocaleString() : "",
      grade: card.grade,
      value: value ? value.toLocaleString() : "",
      rarity,
    },
  };
};

// Map OpenTrade -> PlayerCard-ish card for "Select Card to Buy"
const mapOpenTradeToPlayerCard = (trade: OpenTrade): PlayerCard => {
  const isForPrice = trade.type === TradeEnum.FOR_PRICE;
  const displayPrice = trade.price?.toLocaleString() ?? "-";

  const cardId = getTradeCardId(trade);
  const safeCardId = cardId ?? "Unknown";

  return {
    // IMPORTANT: this is the CARD ID we want in return
    id: safeCardId,

    card: {
      makeModel: `Card #${safeCardId}`,
      cardName: isForPrice ? "For Currency" : "For Card",
      imageUrl: "/assets/cards/placeholder-card.png",
      stat1Label: "PRICE",
      stat1Value: displayPrice,
      stat2Label: "TYPE",
      stat2Value: isForPrice ? "For Currency" : "For Card",
      stat3Label: "OWNER",
      stat3Value: trade.user_id,
      stat4Label: "CARD ID",
      stat4Value: safeCardId,
      grade: "FACTORY",
      value: displayPrice,
      rarity: "factory",
    },
  };
};

const CreateTradeModal: React.FC<CreateTradeModalProps> = ({
  mode,
  onClose,
  onSubmit,
}) => {
  const { user } = useAuth();
  const { filteredTrades, trades } = useTrade();

  const [ownedCardEntities, setOwnedCardEntities] = useState<CardWithVehicle[]>(
    []
  );
  const [isLoadingOwned, setIsLoadingOwned] = useState<boolean>(true);

  const [tradeType, setTradeType] = useState<TradeKind>(
    mode === "sell" ? "currency" : "card"
  );
  const [offeredCardId, setOfferedCardId] = useState<string | null>(null);
  const [requestedCardId, setRequestedCardId] = useState<string | null>(null);
  const [price, setPrice] = useState("");

  // 1) Load user's own cards (for "Select Card to Sell")
  useEffect(() => {
    const fetchUserCards = async () => {
      if (!user) {
        setIsLoadingOwned(false);
        return;
      }

      setIsLoadingOwned(true);
      try {
        const response = await cardService.getUserCardsWithVehicles(user.id);
        setOwnedCardEntities(response.cards || []);
      } catch (err) {
        console.error("[CreateTradeModal] Failed to load user cards:", err);
        setOwnedCardEntities([]);
      } finally {
        setIsLoadingOwned(false);
      }
    };

    fetchUserCards();
  }, [user?.id]);

  // Build UI list for owned cards
  const ownedCards: PlayerCard[] = ownedCardEntities.map(
    mapCardWithVehicleToPlayerCard
  );

  // Use filteredTrades if available, otherwise raw trades
  const allOpenTrades: OpenTrade[] = Array.isArray(filteredTrades)
    ? filteredTrades
    : Array.isArray(trades)
    ? trades
    : [];

  // Only show trades from other users AND with a valid card id
  const otherUsersTrades = allOpenTrades.filter((t) => {
    const cid = getTradeCardId(t);
    return t.user_id !== user?.id && !!cid;
  });

  const buyableTradeCards: PlayerCard[] = otherUsersTrades.map(
    mapOpenTradeToPlayerCard
  );

  const canSubmit =
    !!offeredCardId &&
    (tradeType === "currency"
      ? price.trim().length > 0
      : !!requestedCardId);

  const handleSubmit = () => {
    if (!offeredCardId) return;

    if (tradeType === "currency") {
      onSubmit({
        offeredCardId,
        type: "currency",
        price: Number(price),
      });
    } else {
      if (!requestedCardId) return;
      onSubmit({
        offeredCardId,
        type: "card",
        requestedCardId, // CARD ID -> becomes wantCardId in Trade.tsx
      });
    }

    onClose();
  };

  return (
    <div className={styles.backdrop}>
      <div className={styles.modal}>
        {/* HEADER */}
        <div className={styles.header}>
          <div className="header-1">Create Trade</div>
          <Button size="regular" variant="secondary" onClick={onClose}>
            Close
          </Button>
        </div>

        {/* SELECT CARD TO SELL (USER'S OWN CARDS) */}
        <div className={styles.section}>
          <div className="card-2">Select Card to Sell</div>

          {isLoadingOwned ? (
            <div className={styles.cardRow}>
              <span className="body-2">Loading your cards...</span>
            </div>
          ) : ownedCards.length === 0 ? (
            <div className={styles.cardRow}>
              <span className="body-2">No cards available</span>
            </div>
          ) : (
            <div className={styles.cardRow}>
              {ownedCards.map((item) => (
                <div
                  key={item.id}
                  className={`${styles.cardWrapper} ${
                    offeredCardId === item.id ? styles.selected : ""
                  }`}
                  onClick={() => setOfferedCardId(item.id)}
                >
                  <Card {...item.card} />
                </div>
              ))}
            </div>
          )}
        </div>

        {/* TRADE TYPE TOGGLE */}
        <div className={styles.section}>
          <div className={styles.tradeTypeRow}>
            <Button
              size="regular"
              variant="secondary"
              className={`${styles.typeButton} ${
                tradeType === "currency" ? styles.active : ""
              }`}
              onClick={() => setTradeType("currency")}
            >
              Set Price
            </Button>

            <span className={`body-2 ${styles.orLabel}`}>or</span>

            <Button
              size="regular"
              variant="secondary"
              className={`${styles.typeButton} ${
                tradeType === "card" ? styles.active : ""
              }`}
              onClick={() => setTradeType("card")}
            >
              Select Card to Buy
            </Button>
          </div>
        </div>

        {/* BRANCH UI */}
        {tradeType === "currency" ? (
          // FOR_PRICE: user sells their card for currency
          <div className={styles.section}>
            <TextInput
              label="Price (Cr)"
              placeholder="e.g. 125000"
              value={price}
              onChange={(e) => setPrice(e.target.value)}
              size="regular"
              type="text"
            />
          </div>
        ) : (
          // FOR_CARD: user wants to get another card from open trades
          <div className={styles.section}>
            <div className="card-2">Select Card to Buy</div>

            {buyableTradeCards.length === 0 ? (
              <div className={styles.cardRow}>
                <span className="body-2">No open trades available</span>
              </div>
            ) : (
              <div className={styles.cardRow}>
                {buyableTradeCards.map((item) => (
                  <div
                    key={item.id}
                    className={`${styles.cardWrapper} ${
                      requestedCardId === item.id ? styles.selected : ""
                    }`}
                    onClick={() => setRequestedCardId(item.id)}
                  >
                    <Card {...item.card} />
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* FOOTER */}
        <div className={styles.footer}>
          <Button variant="secondary" size="regular" onClick={onClose}>
            Cancel
          </Button>
          <Button
            variant="primary"
            size="regular"
            disabled={!canSubmit}
            onClick={handleSubmit}
          >
            Create Trade
          </Button>
        </div>
      </div>
    </div>
  );
};

export default CreateTradeModal;
