import React from "react";
import styles from "./Garage.module.css";
import Card, { CardRarity } from "../Card/Card";

// TEMPORARY: Keep mock data imports for vehicle details
// When backend provides vehicle details with cards, remove these
import userCars from "../../data/usercars.json";
import vehicles from "../../data/vehicles.json";

type UserCar = {
  id: number;
  user_id: number | string;
  vehicle_id: number;
  collection_id: number;
  grade: string;
  value: number;
};

type Vehicle = {
  id: number;
  year: string;
  make: string;
  model: string;
  stat1: number;
  stat2: number;
  statN: number;
  value: number;
  image: string;
};

interface GarageProps {
  isLoading?: boolean;
}

const gradeToRarity = (grade: string): CardRarity => {
  switch (grade) {
    case "NISMO":
      return "nismo";
    case "LIMITED_RUN":
      return "limited";
    case "FACTORY":
    default:
      return "factory";
  }
};

const Garage: React.FC<GarageProps> = ({ isLoading = false }) => {
  // TEMPORARY: Still using mock data
  // When backend provides user cards, this will come from props
  const allCars = userCars as UserCar[];

  if (isLoading) {
    return (
      <section className={styles.garage}>
        <div className={styles.panel}>
          <p className={styles.loading}>Loading your cars...</p>
        </div>
      </section>
    );
  }

  const cards = allCars
    .map((uc) => {
      const vehicle = (vehicles as Vehicle[]).find(
        (v) => v.id === uc.vehicle_id
      );
      if (!vehicle) return null;

      return {
        id: uc.id,
        makeModel: `${vehicle.year} ${vehicle.make}`,
        cardName: vehicle.model,
        imageUrl: vehicle.image,
        stat1Label: "POWER",
        stat1Value: vehicle.stat1.toString(),
        stat2Label: "TOP SPEED",
        stat2Value: vehicle.stat2.toString(),
        stat3Label: "WEIGHT",
        stat3Value: vehicle.statN.toString(),
        stat4Label: "VALUE",
        stat4Value: vehicle.value.toString(),
        grade: uc.grade,
        value: uc.value.toLocaleString(),
        rarity: gradeToRarity(uc.grade),
      };
    })
    .filter(Boolean);

  return (
    <section className={styles.garage}>
      <div className={styles.panel}>
        {cards.length === 0 ? (
          <p className={styles.empty}>No cars yet. Open some packs to start your collection!</p>
        ) : (
          cards.map((car: any) => (
            <Card
              key={car.id}
              makeModel={car.makeModel}
              cardName={car.cardName}
              imageUrl={car.imageUrl}
              stat1Label={car.stat1Label}
              stat1Value={car.stat1Value}
              stat2Label={car.stat2Label}
              stat2Value={car.stat2Value}
              stat3Label={car.stat3Label}
              stat3Value={car.stat3Value}
              stat4Label={car.stat4Label}
              stat4Value={car.stat4Value}
              grade={car.grade}
              value={car.value}
              rarity={car.rarity}
            />
          ))
        )}
      </div>
    </section>
  );
};

export default Garage;
