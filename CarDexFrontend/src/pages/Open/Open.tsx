import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import styles from "./Open.module.css";

import { collectionService } from "../../services/collectionService";
// NEW: use packService to purchase + open packs
import { packService } from "../../services/packService";
import type { Collection } from "../../types/types";
import Pack from "../../components/Pack/Pack";

// (Still here if you ever want to use it again)
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

  // NEW: track pack opening state + errors
  const [isOpeningPackId, setIsOpeningPackId] = useState<string | null>(null);
  const [packError, setPackError] = useState<string | null>(null);

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

  // CHANGED: this now purchases + opens a real pack instead of faking it on the frontend
  const handleCollectionClick = async (index: number) => {
    const collection = collections[index];
    if (!collection) return;

    setPackError(null);
    setIsOpeningPackId(collection.id);

    try {
      // 1️⃣ Purchase a pack for this collection
      const purchase = await packService.purchasePack(collection.id);
      const packId = purchase.pack.id; // CHANGED: use backend PackResponse.Id
      console.log("Purchased pack:", purchase);

      // 2️⃣ Open the pack that was just purchased
      const opened = await packService.openPack(packId);
      console.log("Opened pack:", opened);

      // 3️⃣ Navigate to openedPack screen with the actual cards returned
      navigate("/openedPack", {
        state: {
          packName: collection.name,
          packId,
          collectionId: collection.id,
          cards: opened.cards, // CardDetailed[]
          pack: opened.pack,
        },
      });
    } catch (err: any) {
      console.error("Failed to purchase/open pack:", err);
      setPackError(
        err?.response?.data?.message || "Could not open pack. Please try again."
      );
    } finally {
      setIsOpeningPackId(null);
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
          <>
            {/* NEW: show any error from purchase/open */}
            {packError && (
              <div className={`body-1 ${styles.errorMessage}`}>{packError}</div>
            )}

            <div className={styles.grid}>
              {collections.map((collection, idx) => (
                // CHANGED: button for accessibility; can change back to <div> if preferred
                <button
                  key={collection.id}
                  type="button"
                  className={styles.packButton}
                  data-collection-index={idx}
                  onClick={() => handleCollectionClick(idx)}
                  disabled={isOpeningPackId === collection.id}
                >
                  <Pack
                    name={collection.name}
                    packType="Collection"
                    imageUrl={collection.imageUrl || collection.image}
                    // use either price or packPrice, whichever is set
                    price={collection.price ?? collection.packPrice ?? 0}
                  />
                  {isOpeningPackId === collection.id && (
                    <span className={styles.loadingText}>Opening...</span>
                  )}
                </button>
              ))}
            </div>
          </>
        )}
      </section>
    </div>
  );
};

export default Open;
