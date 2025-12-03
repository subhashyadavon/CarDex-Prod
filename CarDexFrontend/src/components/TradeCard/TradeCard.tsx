import React from "react";
import Card, { CarCardProps } from "../Card/Card";
import Button from "../Button/Button";
import styles from "./TradeCard.module.css";

export type TradeStatus = "open" | "completed" | "cancelled";

export interface Trade {
  id: string;
  status: TradeStatus;
  price: number;
  card: CarCardProps;
  tradeType: "FOR_PRICE" | "FOR_CARD";
}

interface TradeCardProps {
  trade: Trade;
  onBuy?: (tradeId: string) => void;
}

const TradeCard: React.FC<TradeCardProps> = ({ trade, onBuy }) => {
  const { card, status, price, tradeType } = trade;

  const statusLabel =
    status === "open"
      ? "Open"
      : status === "completed"
      ? "Completed"
      : "Cancelled";

  const typeLabel = tradeType === "FOR_PRICE" ? "For Price" : "For Card";

  const showBuy =
    status === "open" &&
    tradeType === "FOR_PRICE" &&
    typeof onBuy === "function";

  return (
    <div className={styles.tradeCard}>
      {/* Card preview */}
      <div className={styles.cardWrapper}>
        <Card {...card} />
      </div>

      {/* Info section including BUY button */}
      <div className={styles.infoSection}>
        <div className={styles.infoRow}>
          <span className={`body-2 ${styles.label}`}>Status</span>
          <span
            className={
              status === "open"
                ? styles.statusOpen
                : status === "completed"
                ? styles.statusCompleted
                : styles.statusCancelled
            }
          >
            {statusLabel}
          </span>
        </div>

        <div className={styles.infoRow}>
          <span className={`body-2 ${styles.label}`}>Type</span>
          <span className="body-1">{typeLabel}</span>
        </div>

        <div className={styles.infoRow}>
          <span className={`body-2 ${styles.label}`}>Price</span>
          <span className={`body-1 ${styles.price}`}>
            {price?.toLocaleString()} Cr
          </span>
        </div>

        {showBuy && (
          <Button
            size="large"
            variant="primary"
            className={styles.buyButton}
            onClick={() => onBuy!(trade.id)}
          >
            Buy
          </Button>
        )}
      </div>
    </div>
  );
};

export default TradeCard;
