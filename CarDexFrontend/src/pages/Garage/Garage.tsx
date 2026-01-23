import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import styles from "./Garage.module.css";
import Garage from "../../components/Garage/Garage";
import CollectionProgressCard from "../../components/CollectionProgressCard/CollectionProgressCard";
import { userService } from "../../services/userService";
import { cardService } from "../../services/cardService";
import { CollectionProgress, CardWithVehicle } from "../../types/types";
import { useAuth } from "../../hooks/useAuth";

const GarageSection: React.FC = () => {
  const { user, isAuthenticated, updateUserCurrency, refreshUser } = useAuth();
  const navigate = useNavigate();
  const [cards, setCards] = useState<CardWithVehicle[]>([]);
  const [collectionProgress, setCollectionProgress] = useState<CollectionProgress[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Redirect to login if not authenticated
  useEffect(() => {
    if (!isAuthenticated && !isLoading) {
      navigate('/login');
    }
  }, [isAuthenticated, isLoading, navigate]);

  const fetchGarageData = async () => {
    if (!user) return;
    setIsLoading(true);
    setError(null);

    try {
      // Fetch both cards and collection progress in parallel
      const [cardsResponse, progressData] = await Promise.all([
        cardService.getUserCardsWithVehicles(user.id),
        userService.getCollectionProgress(user.id)
      ]);

      // Set cards data
      setCards(cardsResponse.cards);

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

  useEffect(() => {
    // Don't fetch if user is not logged in
    if (!user) {
      setIsLoading(false);
      setError("Please log in to view your garage.");
      return;
    }

    fetchGarageData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [user?.id]); // Re-fetch when user changes

  const handleSellCard = async (cardId: string) => {
    if (!user) return;

    try {
      const response = await cardService.quickSellCard(cardId);

      // Update local cards list
      setCards(prev => prev.filter(c => c.id !== cardId));

      // Refresh global user state (currency)
      if (refreshUser) {
        await refreshUser();
      }

      console.log(`Successfully sold card ${cardId}. New currency: ${response.newUserCurrency}`);
    } catch (err) {
      console.error("Failed to sell card:", err);
      alert("Failed to sell card. It might be listed in a trade.");
    }
  };

  if (error) {
    return (
      <div className={styles.garagePage}>
        <div className={styles.error}>
          <p>{error}</p>
          <button onClick={fetchGarageData} className={styles.refreshButton}>
            Retry
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.garagePage}>
      <div className={styles.garageHeader}>
        <h2 className={styles.sectionTitle}>My Garage</h2>
        <button
          onClick={async () => {
            await fetchGarageData();
            if (refreshUser) await refreshUser();
          }}
          className={styles.refreshButton}
          disabled={isLoading}
        >
          {isLoading ? "Refreshing..." : "Refresh Inventory"}
        </button>
      </div>

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
        <h2 className={styles.sectionTitle}>Owned Cards</h2>
        <Garage cards={cards} isLoading={isLoading} onSellCard={handleSellCard} />
      </section>
    </div>
  );
};

export default GarageSection;
