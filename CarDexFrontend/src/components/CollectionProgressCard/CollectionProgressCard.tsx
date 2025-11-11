import React from "react";
import styles from "./CollectionProgressCard.module.css";
import "../../App.css"; // Import global typography

export interface CollectionProgressCardProps {
  collectionName: string;
  collectionImage: string;
  ownedVehicles: number;
  totalVehicles: number;
  percentage: number;
}

const CollectionProgressCard: React.FC<CollectionProgressCardProps> = ({
  collectionName,
  collectionImage,
  ownedVehicles,
  totalVehicles,
  percentage,
}) => {
  return (
    <div className={styles.card}>
      {/* Collection Image/Icon */}
      <div className={styles.imageWrapper}>
        <img 
          src={collectionImage} 
          alt={collectionName}
          className={styles.image}
          onError={(e) => {
            // Fallback if image doesn't exist
            e.currentTarget.src = '/images/collections/placeholder.jpg';
          }}
        />
      </div>

      {/* Collection Name */}
      <div className={`card-2 ${styles.name}`}>{collectionName}</div>

      {/* Progress Stats */}
      <div className={styles.stats}>
        <span className={`card-1 ${styles.percentage}`}>{percentage}%</span>
        <span className={`card-1 ${styles.separator}`}>,</span>
        <span className={`card-1 ${styles.fraction}`}>{ownedVehicles}/{totalVehicles}</span>
      </div>
    </div>
  );
};

export default CollectionProgressCard;
