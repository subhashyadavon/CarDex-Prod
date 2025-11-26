import React from "react";
import styles from "./Pack.module.css";

import coinImage from "../../assets/coin.png";

export type PackProps = {
  name: string; // "The Daily Booster"
  packType: string; // "BOOSTER PACK"
  imageUrl: string; // Main pack image (180x90)
  price?: number; // Made optional + safe default
  onClick?: () => void;
};

export default function Pack({
  name,
  packType,
  imageUrl,
  price,
  onClick,
}: PackProps) {
  // safe check for the numeric value
  const safePrice =
    typeof price === "number" && !Number.isNaN(price) ? price : 0;

  return (
    <article className={styles.pack} onClick={onClick}>
      {/* Header */}
      <header className={styles.header}>
        <div className={styles.headerText}>
          <div className={`${styles.packType} card-4`}>{packType}</div>
          <h3 className={`${styles.name} card-2`}>{name}</h3>
        </div>
      </header>

      {/* Main Pack Image */}
      <div className={styles.imageWrap}>
        <img className={styles.image} src={imageUrl} alt={name} />
      </div>

      {/* Footer with Price */}
      <footer className={styles.footer}>
        <span className={styles.coinIcon} aria-hidden>
          <img src={coinImage} alt="" className={styles.coinImage} />
        </span>
        <span className={`${styles.price} card-2`}>
          {safePrice.toLocaleString()}
        </span>
      </footer>
    </article>
  );
}
