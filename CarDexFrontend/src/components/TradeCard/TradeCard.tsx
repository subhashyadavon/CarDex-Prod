// src/components/TradeCard/TradeCard.tsx
import React from "react";
import Card, { CarCardProps } from "../Card/Card";
import styles from "./TradeCard.module.css";

export type TradeStatus = "open" | "completed" | "cancelled";

export interface Trade {
  id: string;
  status: TradeStatus;
  price: number;
  card: CarCardProps;
}

interface TradeCardProps {
  trade: Trade;
}

const TradeCard: React.FC<TradeCardProps> = ({ trade }) => {
  const { card, status, price } = trade;

  const statusLabel =
    status === "open"
      ? "Open"
      : status === "completed"
      ? "Completed"
      : "Cancelled";

  return (
    <div className={styles.tradeCard}>
      <div className={styles.cardWrapper}>
        <Card {...card} />
      </div>

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
          <span className={`body-2 ${styles.label}`}>Price</span>
          <span className={`body-1 ${styles.price}`}>
            {price.toLocaleString()} Cr
          </span>
        </div>
      </div>
    </div>
  );
};

export default TradeCard;
