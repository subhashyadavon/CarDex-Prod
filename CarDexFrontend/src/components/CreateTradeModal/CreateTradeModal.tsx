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
} from "../../types/types";

export interface CreateTradePayload {
  offeredCardId: string;
  price: number;
}

interface CreateTradeModalProps {
  onClose: () => void;
  onSubmit: (payload: CreateTradePayload) => void;
}

interface PlayerCard {
  id: string;
  cardId: string;
  card: CarCardProps;
}

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

const mapCardWithVehicleToPlayerCard = (
  card: CardWithVehicle
): PlayerCard => {
  const makeModel = `${card.year} ${card.make} ${card.model}`.trim();
  const rarity = gradeToRarity(card.grade);
  const value = card.value ?? 0;

  return {
    id: String(card.id),
    cardId: String(card.id),
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

const CreateTradeModal: React.FC<CreateTradeModalProps> = ({
  onClose,
  onSubmit,
}) => {
  const { user } = useAuth();
  const { filteredTrades, trades } = useTrade();

  const [ownedCardEntities, setOwnedCardEntities] = useState<CardWithVehicle[]>(
    []
  );
  const [isLoadingOwned, setIsLoadingOwned] = useState<boolean>(true);

  const [offeredCardId, setOfferedCardId] = useState<string | null>(null);
  const [selectedOfferedUiId, setSelectedOfferedUiId] = useState<string | null>(
    null
  );

  const [price, setPrice] = useState("");

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

  const allOpenTrades: OpenTrade[] = Array.isArray(filteredTrades)
    ? filteredTrades
    : Array.isArray(trades)
    ? trades
    : [];

  const userOwnedCardIdsInOpenTrades = new Set(
    allOpenTrades
      .filter((t) => getTradeUserId(t) === user?.id)
      .map((t) => getTradeCardId(t))
      .filter((id): id is string => !!id)
  );

  const ownedCards: PlayerCard[] = ownedCardEntities
    .map(mapCardWithVehicleToPlayerCard)
    .filter((pc) => !userOwnedCardIdsInOpenTrades.has(pc.cardId));

  const canSubmit =
    !!offeredCardId && price.trim().length > 0;

  const handleSubmit = () => {
    if (!offeredCardId) return;

    onSubmit({
      offeredCardId,
      price: Number(price),
    });

    onClose();
  };

  return (
    <div className={styles.backdrop}>
      <div className={styles.modal}>
        <div className={styles.header}>
          <div className="header-1">Create Trade</div>
          <Button size="regular" variant="secondary" onClick={onClose}>
            Close
          </Button>
        </div>

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
