// src/components/Card/Card.tsx
import React from "react";
import styles from "./Card.module.css";
import "../../App.css"; // make sure global typography is available
import coinIcon from "../../assets/coin.png";

export type CardRarity = "factory" | "limited" | "nismo";

export interface CarCardProps {
  makeModel: string;
  cardName: string;
  imageUrl: string;
  stat1Label?: string;
  stat1Value?: string;
  stat2Label?: string;
  stat2Value?: string;
  stat3Label?: string;
  stat3Value?: string;
  stat4Label?: string;
  stat4Value?: string;
  grade?: string;
  value?: string;
  rarity?: CardRarity;
  collectionImageUrl?: string;
}

const Card: React.FC<CarCardProps> = ({
  makeModel,
  cardName,
  imageUrl,
  stat1Label = "POWER",
  stat1Value = "471",
  stat2Label = "WEIGHT",
  stat2Value = "1345",
  stat3Label = "BRAKING",
  stat3Value = "471",
  stat4Label = "TORQUE",
  stat4Value = "1345",
  grade = "FACTORY",
  value = "115,999",
  rarity = "factory",
  collectionImageUrl,
}) => {
  return (
    <div className={`${styles.card} ${styles[rarity]}`}>
      {/* HEADER */}
      <div className={styles.header}>
        <div>
          <div className={`card-4 ${styles.makeModel}`}>{makeModel}</div>
          <div className={`card-2 ${styles.cardName}`}>{cardName}</div>
        </div>

        {collectionImageUrl && (
          <img
            src={collectionImageUrl}
            alt="Collection badge"
            className={styles.collectionBadge}
          />
        )}
      </div>

      {/* IMAGE */}
      <div className={styles.imageWrapper}>
        <img src={imageUrl} alt={cardName} className={styles.image} />
      </div>

      <div className={styles.stats}>
        <div className={styles.stat}>
          <div className="card-1">{stat1Value}</div>
          <div className="card-4">{stat1Label}</div>
        </div>
        <div className={styles.stat}>
          <div className="card-1">{stat2Value}</div>
          <div className="card-4">{stat2Label}</div>
        </div>
        <div className={styles.stat}>
          <div className="card-1">{stat3Value}</div>
          <div className="card-4">{stat3Label}</div>
        </div>
        <div className={styles.stat}>
          <div className="card-1">{stat4Value}</div>
          <div className="card-4">{stat4Label}</div>
        </div>
      </div>

      {/* FOOTER */}
      <div className={styles.footer}>
        <div className="card-4">{grade}</div>
        <div className={styles.value}>
          <img src={coinIcon} alt="Coin icon" className={styles.valueIcon} />
          <span className="card-3">{value}</span>
        </div>
      </div>
    </div>
  );
};

export default Card;
