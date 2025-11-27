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
        console.log("Collections from API:", data);
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

    const cardsWithImage =
      detailed.cards?.map((card) => ({
        ...card,
        image: card.imageUrl, 
      })) ?? [];

    console.log(cardsWithImage);

    const picked = pickThreeRandom(cardsWithImage);

    // Navigate to the openedPack page with the picked cards
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

  return (
    <div className={styles.container}>
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
                  imageUrl={collection.imageUrl}
                  price={collection.price ?? 0}
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
