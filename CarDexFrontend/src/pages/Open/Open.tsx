import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import styles from "./Open.module.css";

import { collectionService } from "../../services/collectionService";
import type {
  Collection,
  CollectionDetailedResponse,
  CollectionCard,
} from "../../types/types";
import Pack from "../../components/Pack/Pack";


// Picks 3 cards randomly from the list of cards after opening a pack
function pickThreeRandom<T>(arr: T[]): T[] {
  const copy = [...arr];
  const out: T[] = [];
  for (let i = 0; i < 3 && copy.length > 0; i++) {
    const idx = Math.floor(Math.random() * copy.length);
    out.push(copy.splice(idx, 1)[0]);
  }
  return out;
}

const Open: React.FC = () => {
  const navigate = useNavigate();

  // Collections from backend  
  const [collections, setCollections] = useState<Collection[]>([]);
  const [isLoadingCollections, setIsLoadingCollections] = useState(false);
  const [collectionsError, setCollectionsError] = useState<string | null>(null);

  useEffect(() => {
    const fetchCollections = async () => {
      setIsLoadingCollections(true);
      setCollectionsError(null);
      try {
        const data = await collectionService.getAllCollections(); 
        setCollections(data);
      } catch (err) {
        console.error("Failed to fetch collections:", err);
        setCollectionsError("Could not load collections.");
      } finally {
        setIsLoadingCollections(false);
      }
    };

    fetchCollections();
  }, []);

  const handleCollectionClick = async (index: number) => {
    const collection = collections[index];
    if (!collection) return;

    try {
      // fetches the cards from the collection
      const detailed: CollectionDetailedResponse =
        await collectionService.getCollectionById(collection.id);

      const cards: CollectionCard[] = detailed.cards ?? [];
      const picked = pickThreeRandom(cards);

      navigate("/openedPack", {
        state: {
          packName: detailed.name ?? collection.name,
          cards: picked,
        },
      });
    } catch (err) {
      console.error("Failed to fetch detailed collection:", err);
      navigate("/openedPack", {
        state: {
          packName: collection.name,
          cards: [],
        },
      });
    }
  };

  const ownedCards: any[] = [];

  return (
    <div className={styles.container}>
      {/* Owned Cards Section */}
      <section className={styles.ownedSection}>
        <h2 className="header-1" style={{ marginBottom: "0.75rem" }}>
          Owned Cards
        </h2>

        <div className={styles.placeholderGrid}>
          {ownedCards.length === 0 ? (
            <div className={`body-1 ${styles.emptyMessage}`}>
              No cards owned...
            </div>
          ) : (
            ownedCards.map((card) => (
              <div key={card.id} className={styles.ownedCardTile}>
                <div className="body-2">Card #{card.id}</div>
              </div>
            ))
          )}
        </div>
      </section>

      {/* Collections Section */}
      <section className={styles.shop}>
        <header className={styles.header}>
          <h1 className={styles.title}>PACK SHOP</h1>
        </header>

        {isLoadingCollections ? (
          <div className={`body-1 ${styles.emptyMessage}`}>
            Loading collections...
          </div>
        ) : collectionsError ? (
          <div className={`body-1 ${styles.emptyMessage}`}>
            {collectionsError}
          </div>
        ) : (
          <div className={styles.grid}>
            {collections.map((collection, idx) => (
              <div
                key={collection.id}
                data-collection-index={idx}
                onClick={() => handleCollectionClick(idx)}
              >
                <Pack
                  name={collection.name}
                  packType="Collection"
                  imageUrl={collection.image}
                  price={collection.packPrice ?? 0}
                />
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  );
};

export default Open;
