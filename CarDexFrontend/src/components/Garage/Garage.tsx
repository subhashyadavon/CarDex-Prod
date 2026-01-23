import React from "react";
import styles from "./Garage.module.css";
import Card, { CardRarity } from "../Card/Card";
import { CardWithVehicle, GradeEnum } from "../../types/types";

interface GarageProps {
  cards: CardWithVehicle[];
  isLoading?: boolean;
  onSellCard?: (cardId: string) => void;
}

/**
 * Maps backend grade enum to card rarity for styling
 */
const gradeToRarity = (grade: string): CardRarity => {
  switch (grade) {
    case GradeEnum.NISMO:
      return "nismo";
    case GradeEnum.LIMITED_RUN:
      return "limited";
    case GradeEnum.FACTORY:
    default:
      return "factory";
  }
};

const Garage: React.FC<GarageProps> = ({ cards, isLoading = false, onSellCard }) => {
  if (isLoading) {
    return (
      <section className={styles.garage}>
        <div className={styles.panel}>
          <p className={styles.loading}>Loading your cars...</p>
        </div>
      </section>
    );
  }

  return (
    <section className={styles.garage}>
      <div className={styles.panel}>
        {cards.length === 0 ? (
          <p className={styles.empty}>
            No cars yet. Open some packs to start your collection!
          </p>
        ) : (
          cards.map((card) => (
            <Card
              key={card.id}
              makeModel={`${card.year} ${card.make}`}
              cardName={card.model}
              imageUrl={card.vehicleImage}
              stat1Label="POWER"
              stat1Value={card.stat1.toString()}
              stat2Label="TOP SPEED"
              stat2Value={card.stat2.toString()}
              stat3Label="WEIGHT"
              stat3Value={card.stat3.toString()}
              stat4Label="VALUE"
              stat4Value={card.value.toLocaleString()}
              grade={card.grade}
              value={card.value.toLocaleString()}
              rarity={gradeToRarity(card.grade)}
              onSell={onSellCard ? () => onSellCard(card.id) : undefined}
            />
          ))
        )}
      </div>
    </section>
  );
};

export default Garage;
