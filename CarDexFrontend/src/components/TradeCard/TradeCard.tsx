import React from "react";
import styles from "./TradeCard.module.css";

import Card, { CarCardProps } from "../Card/Card";
import Button from "../Button/Button";

export type Trade = {
  id: string;
  status: "open" | "completed";
  price: number;
  tradeType?: "FOR_PRICE" | "FOR_CARD";
  card: CarCardProps;
  isOwnTrade?: boolean;
};

export interface TradeCardProps {
  trade: Trade;
  onBuy?: (tradeId: string) => void;
}

const TradeCard: React.FC<TradeCardProps> = ({ trade, onBuy }) => {
  const { card, price, tradeType, status, isOwnTrade } = trade;

  const isForPrice = tradeType === "FOR_PRICE";
  const showPrice = isForPrice && price > 0;

  const handleBuyClick = () => {
    if (!onBuy || isOwnTrade) return;
    onBuy(trade.id);
  };

  return (
    <div className={styles.tradeCard}>
      <div className={styles.cardWrapper}>
        <Card {...card} />
      </div>

      <div className={styles.footer}>
        <div className={styles.priceInfo}>
          {showPrice ? (
            <>
              <span className={styles.priceLabel}>Price</span>
              <span className={styles.priceValue}>
                {price.toLocaleString()} Cr
              </span>
            </>
          ) : tradeType === "FOR_CARD" ? (
            <span className={styles.priceLabel}>Card-for-card trade</span>
          ) : (
            <span className={styles.priceLabel}>&nbsp;</span>
          )}
        </div>
      </div>

      {status === "open" && (
        <div className={styles.bottomActionBar}>
          {isOwnTrade ? (
            <Button
              size="large"
              variant="primary"
              disabled
              className={styles.buyButton}
            >
              Your listing
            </Button>
          ) : (
            onBuy && (
              <Button
                size="large"
                variant="primary"
                className={styles.buyButton}
                onClick={handleBuyClick}
              >
                Buy
              </Button>
            )
          )}
        </div>
      )}
    </div>
  );
};

export default TradeCard;
