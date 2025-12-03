// src/components/CreateTradeModal/CreateTradeModal.tsx

import React, { useState, useEffect } from "react";
import styles from "./CreateTradeModal.module.css";
import "../../App.css";

import Button from "../Button/Button";
import TextInput from "../TextInput/TextInput";
import Card, { CarCardProps } from "../Card/Card";

import { useAuth } from "../../hooks/useAuth";
import { cardService, Vehicle } from "../../services/cardService";
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
  id: string;     // UI id (unique per row)
  cardId: string; // underlying card id used in payload
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

// Get the user id off an OpenTrade (handles userId or user_id)
const getTradeUserId = (trade: OpenTrade): string | null => {
  const t: any = trade;
  const raw = t.userId ?? t.user_id ?? null;
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

/**
 * Try to find the best matching vehicle for a given card.
 * We support:
 *  - Exact match by vehicleId (if backend ever sends id)
 *  - Fuzzy match by make+model appearing in card.name / description
 */
const findVehicleForCard = (
  card: any | undefined,
  vehicles: Vehicle[]
): Vehicle | null => {
  if (!card || vehicles.length === 0) return null;

  // 1. Try by explicit vehicle id if present
  const rawVehicleId =
    (card as any)?.vehicleId ??
    (card as any)?.vehicle_id ??
    null;

  if (rawVehicleId != null) {
    const idStr = String(rawVehicleId);
    const byId = vehicles.find(
      (v) => v.id != null && String(v.id) === idStr
    );
    if (byId) return byId;
  }

  // 2. Try by matching make+model against card text
  const name: string = (card?.name ?? "").toString();
  const description: string = (card?.description ?? "").toString();
  const combined = `${name} ${description}`.toLowerCase();

  if (!combined.trim()) return null;

  const byName = vehicles.find((v) => {
    const full = `${v.make} ${v.model}`.toLowerCase();
    return combined.includes(full) || full.includes(name.toLowerCase());
  });

  if (byName) return byName;

  return null;
};

// Map CardWithVehicle -> PlayerCard (for user's own cards)
const mapCardWithVehicleToPlayerCard = (
  card: CardWithVehicle
): PlayerCard => {
  const makeModel = `${card.year} ${card.make} ${card.model}`.trim();
  const rarity = gradeToRarity(card.grade);
  const value = card.value ?? 0;

  return {
    id: String(card.id),      // UI id
    cardId: String(card.id),  // underlying card id
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

/**
 * Map OpenTrade + card DTO + vehicles list -> PlayerCard
 * Used for "Select Card to Buy" – shows REAL car stats instead of placeholder trade metadata.
 */
const mapOpenTradeToPlayerCard = (
  trade: OpenTrade,
  card: any | undefined,
  vehicles: Vehicle[]
): PlayerCard => {
  const cardId = getTradeCardId(trade);
  const safeCardId = cardId ?? "Unknown";

  const name: string | undefined = card?.name;
  const description: string | undefined = card?.description;

  const vehicle = findVehicleForCard(card, vehicles);

  const make = vehicle?.make ?? undefined;
  const model = vehicle?.model ?? undefined;
  const year = vehicle?.year ?? undefined;

  const makeModel =
    (make && model && `${make} ${model}`) ||
    description ||
    name ||
    `Card #${safeCardId}`;

  const cardName =
    name ||
    description ||
    (make && model ? `${year ?? ""} ${make} ${model}`.trim() : makeModel);

  const grade = card?.grade ?? "FACTORY";
  const rarity: CarCardProps["rarity"] = gradeToRarity(grade);

  const cardValueNumber: number | undefined =
    card?.value != null ? card.value : vehicle?.value;

  const cardValueString =
    cardValueNumber != null ? cardValueNumber.toLocaleString() : "";

  const stat1 = vehicle?.stat1 ?? null;
  const stat2 = vehicle?.stat2 ?? null;
  const stat3 = vehicle?.stat3 ?? null;

  return {
    // UI id = trade id (unique per open trade row)
    id: String(trade.id),
    // This is the CARD ID that will be used as wantCardId
    cardId: safeCardId,
    card: {
      makeModel,
      cardName,
      imageUrl:
        card?.imageUrl ||
        (vehicle as any)?.image ||
        "/assets/cards/placeholder-card.png",

      stat1Label: "POWER",
      stat1Value: stat1 != null ? String(stat1) : "--",

      stat2Label: "WEIGHT",
      stat2Value: stat2 != null ? String(stat2) : "--",

      stat3Label: "BRAKING",
      stat3Value: stat3 != null ? String(stat3) : "--",

      stat4Label: "VALUE",
      stat4Value: cardValueString || "--",

      grade,
      value: cardValueString,
      rarity,
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

  // Separate UI selection ids so only one visible card highlights per section
  const [selectedOfferedUiId, setSelectedOfferedUiId] = useState<string | null>(
    null
  );
  const [selectedRequestedUiId, setSelectedRequestedUiId] = useState<
    string | null
  >(null);

  const [price, setPrice] = useState("");

  // Vehicles + card cache for "Select Card to Buy"
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [cardCache, setCardCache] = useState<Record<string, any>>({});

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

  // 2) Load all vehicles once (for "Select Card to Buy" stats)
  useEffect(() => {
    const fetchVehicles = async () => {
      try {
        const vehiclesResponse = await cardService.getAllVehicles();
        setVehicles(vehiclesResponse);
      } catch (err) {
        console.error("[CreateTradeModal] Failed to load vehicles:", err);
      }
    };

    fetchVehicles();
  }, []);

  // All open trades (from context)
  const allOpenTrades: OpenTrade[] = Array.isArray(filteredTrades)
    ? filteredTrades
    : Array.isArray(trades)
    ? trades
    : [];

  // Card IDs that are already in open trades for THIS user
  const userOwnedCardIdsInOpenTrades = new Set(
    allOpenTrades
      .filter((t) => getTradeUserId(t) === user?.id)
      .map((t) => getTradeCardId(t))
      .filter((id): id is string => !!id)
  );

  // Build UI list for owned cards, but filter out any card already in user's open trades
  const ownedCards: PlayerCard[] = ownedCardEntities
    .map(mapCardWithVehicleToPlayerCard)
    .filter((pc) => !userOwnedCardIdsInOpenTrades.has(pc.cardId));

  // Only show trades from other users AND with a valid card id (for "Select Card to Buy")
  const otherUsersTrades = allOpenTrades.filter((t) => {
    const cid = getTradeCardId(t);
    const tradeUserId = getTradeUserId(t);
    return tradeUserId !== user?.id && !!cid;
  });

  // 3) Fetch card details for buyable trades
  useEffect(() => {
    const allCardIds = otherUsersTrades
      .map((t) => getTradeCardId(t))
      .filter((id): id is string => !!id);

    const missingIds = allCardIds.filter((id) => !(id in cardCache));

    if (missingIds.length === 0) return;

    const fetchCards = async () => {
      try {
        const results = await Promise.all(
          missingIds.map((id) => cardService.getCardById(id))
        );

        setCardCache((prev) => {
          const next: Record<string, any> = { ...prev };
          results.forEach((card: any) => {
            if (card && card.id != null) {
              next[String(card.id)] = card;
            }
          });
          return next;
        });
      } catch (err) {
        console.error("[CreateTradeModal] Failed to load trade cards:", err);
      }
    };

    fetchCards();
  }, [otherUsersTrades, cardCache]);

  // Build UI list for "Select Card to Buy" using real vehicle stats
  const buyableTradeCards: PlayerCard[] = otherUsersTrades.map((trade) => {
    const cid = getTradeCardId(trade);
    const cardDetails = cid ? cardCache[cid] : undefined;
    return mapOpenTradeToPlayerCard(trade, cardDetails, vehicles);
  });

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
              <span className="body-2">
                No cards available (or all your cards are already in open trades).
              </span>
            </div>
          ) : (
            <div className={styles.cardRow}>
              {ownedCards.map((item) => (
                <div
                  key={item.id}
                  className={`${styles.cardWrapper} ${
                    selectedOfferedUiId === item.id ? styles.selected : ""
                  }`}
                  onClick={() => {
                    setOfferedCardId(item.cardId);
                    setSelectedOfferedUiId(item.id);
                  }}
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
                      selectedRequestedUiId === item.id ? styles.selected : ""
                    }`}
                    onClick={() => {
                      setRequestedCardId(item.cardId);
                      setSelectedRequestedUiId(item.id);
                    }}
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
