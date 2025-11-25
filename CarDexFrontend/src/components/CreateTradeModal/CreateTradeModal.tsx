// src/components/CreateTradeModal/CreateTradeModal.tsx
import React, { useState } from "react";
import styles from "./CreateTradeModal.module.css";
import "../../App.css";

import Button from "../Button/Button";
import TextInput from "../TextInput/TextInput";
import Card, { CarCardProps } from "../Card/Card";

type TradeKind = "currency" | "card";

export interface CreateTradePayload {
  offeredCardId: string;
  type: TradeKind;
  price?: number;
  requestedCardId?: string;
}

interface CreateTradeModalProps {
  mode: "sell" | "trade"; // which button opened it
  onClose: () => void;
  onSubmit: (payload: CreateTradePayload) => void;
}

// Simple mock of owned cards for now.
// Later you can pass this in via props if needed.
interface PlayerCard {
  id: string;
  card: CarCardProps;
}

const mockOwnedCards: PlayerCard[] = [
  {
    id: "my1",
    card: {
      makeModel: "Nissan GT-R",
      cardName: "R35 Premium Edition",
      imageUrl: "/assets/cards/gtr-r35.png",
      stat1Label: "POWER",
      stat1Value: "565",
      stat2Label: "WEIGHT",
      stat2Value: "1740",
      stat3Label: "BRAKING",
      stat3Value: "9.1",
      stat4Label: "TORQUE",
      stat4Value: "467",
      grade: "FACTORY",
      value: "125,000",
      rarity: "factory",
    },
  },
  {
    id: "my2",
    card: {
      makeModel: "Toyota Supra",
      cardName: "A80 Twin Turbo",
      imageUrl: "/assets/cards/supra-a80.png",
      stat1Label: "POWER",
      stat1Value: "320",
      stat2Label: "WEIGHT",
      stat2Value: "1560",
      stat3Label: "BRAKING",
      stat3Value: "8.7",
      stat4Label: "TORQUE",
      stat4Value: "315",
      grade: "LIMITED",
      value: "210,000",
      rarity: "limited",
    },
  },
];

const CreateTradeModal: React.FC<CreateTradeModalProps> = ({
  mode,
  onClose,
  onSubmit,
}) => {
  const ownedCards = mockOwnedCards;

  const [tradeType, setTradeType] = useState<TradeKind>(
    mode === "sell" ? "currency" : "card"
  );
  const [offeredCardId, setOfferedCardId] = useState<string | null>(null);
  const [requestedCardId, setRequestedCardId] = useState<string | null>(null);
  const [price, setPrice] = useState("");

  const canSubmit =
    !!offeredCardId &&
    (tradeType === "currency"
      ? price.trim().length > 0
      : requestedCardId !== null);

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
        requestedCardId,
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

        {/* SELECT CARD TO SELL */}
        <div className={styles.section}>
          <div className="card-2">Select Card to Sell</div>
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
          <div className={styles.section}>
            <div className="card-2">Select Card to Buy</div>
            <div className={styles.cardRow}>
              {ownedCards.map((item) => (
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
