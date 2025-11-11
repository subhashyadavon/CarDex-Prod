import React, { useEffect, useState } from "react";
import styles from "./Garage.module.css";
import Garage from "../../components/Garage/Garage";
import CollectionProgressCard from "../../components/CollectionProgressCard/CollectionProgressCard";
import { userService } from "../../services/userService";
import { CollectionProgress } from "../../types/types";

const GarageSection: React.FC = () => {
  const [collectionProgress, setCollectionProgress] = useState<CollectionProgress[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // TEMPORARY: Using hardcoded userId
  // TODO: Get from AuthContext when authentication is integrated
  const userId = 1;

  useEffect(() => {
    const fetchGarageData = async () => {
      setIsLoading(true);
      setError(null);

      try {
        // Fetch collection progress
        const progressData = await userService.getCollectionProgress(userId);

        // Sort collections by percentage (highest first)
        const sortedCollections = progressData.collections.sort(
          (a, b) => b.percentage - a.percentage
        );

        setCollectionProgress(sortedCollections);
      } catch (err) {
        console.error("Failed to load garage data:", err);
        setError("Failed to load your garage. Please try again.");
      } finally {
        setIsLoading(false);
      }
    };

    fetchGarageData();
  }, [userId]);

  if (error) {
    return (
      <div className={styles.garagePage}>
        <div className={styles.error}>
          <p>{error}</p>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.garagePage}>
      {/* SECTION 1: Collection Progress */}
      <section className={styles.collectionProgressSection}>
        <h2 className={styles.sectionTitle}>Collection Progress</h2>
        
        {isLoading ? (
          <p className={styles.loading}>Loading collections...</p>
        ) : collectionProgress.length === 0 ? (
          <p className={styles.empty}>
            Open packs to start your collection!
          </p>
        ) : (
          <div className={styles.progressGrid}>
            {collectionProgress.map((collection) => (
              <CollectionProgressCard
                key={collection.collectionId}
                collectionName={collection.collectionName}
                collectionImage={collection.collectionImage}
                ownedVehicles={collection.ownedVehicles}
                totalVehicles={collection.totalVehicles}
                percentage={collection.percentage}
              />
            ))}
          </div>
        )}
      </section>

      {/* SECTION 2: All Cards */}
      <section className={styles.allCardsSection}>
        <h2 className={styles.sectionTitle}>All Cards</h2>
        <Garage isLoading={isLoading} />
      </section>
    </div>
  );
};

export default GarageSection;
