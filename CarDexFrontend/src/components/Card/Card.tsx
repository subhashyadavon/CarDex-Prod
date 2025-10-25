import React from "react";
import styles from "./Card.module.css";

export type Rarity = "FACTORY" | "LIMITED_RUN" | "NISMO";

export type CarCardProps = {
  makeModel: string;   // "LEXUS LC 500"
  title: string;       // "The Daily"
  imageUrl: string;
  stats: { label: string; value: number }[]; // expects 4 items
  rarity: Rarity;
  price: number;
  crestUrl?: string;   // small JDM badge image
  onClick?: () => void;
};

const rarityClass: Record<Rarity, string> = {
  FACTORY: styles.rarityFactory,
  LIMITED_RUN: styles.rarityLimited,
  NISMO: styles.rarityNismo,
};

export default function CarCard({
  makeModel,
  title,
  imageUrl,
  stats,
  rarity,
  price,
  crestUrl,
  onClick,
}: CarCardProps) {
  return (
    <article className={`${styles.card} ${rarityClass[rarity]}`} onClick={onClick}>
      <header className={styles.header}>
        <div className={styles.preTitle}>{makeModel}</div>
        <h3 className={styles.title}>{title}</h3>
        {crestUrl && <img className={styles.crest} src={crestUrl} alt="" />}
      </header>

      <div className={styles.mediaWrap}>
        <img className={styles.media} src={imageUrl} alt={title} />
      </div>

      <section className={styles.stats}>
        {stats.slice(0,4).map((s) => (
          <div key={s.label} className={styles.stat}>
            <div className={styles.statValue}>{s.value}</div>
            <div className={styles.statLabel}>{s.label}</div>
          </div>
        ))}
      </section>

      <footer className={styles.footer}>
        <span className={styles.badge}>{rarity.replace("_"," ")}</span>
        <span className={styles.price}>
          <span className={styles.coin} aria-hidden>🪙</span>
          {price.toLocaleString()}
        </span>
      </footer>
    </article>
  );
}
