import React, { useMemo } from "react";
import { useNavigate } from "react-router-dom";
import PackShop from "../../components/Pack/PackShop";
import packsData from "../../data/packs.json";
import collectionsData from "../../data/collections.json";
import vehiclesData from "../../data/vehicles.json";
import styles from "./Open.module.css";

type Pack = (typeof packsData)[number];
type Collection = (typeof collectionsData)[number];
type Vehicle = (typeof vehiclesData)[number];

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

  // Index vehicles once for quick lookup
  const vehicleById = useMemo(() => {
    const m = new Map<number, Vehicle>();
    (vehiclesData as Vehicle[]).forEach((v) => m.set(v.id, v));
    return m;
  }, []);

  const ownedCards: { id: number }[] = []; // wire to your state/API later

  const handlePackGridClick: React.MouseEventHandler<HTMLDivElement> = (e) => {
    const tile = (e.target as HTMLElement).closest<HTMLElement>(
      "[data-pack-index]"
    );
    if (!tile) return;

    const idxStr = tile.getAttribute("data-pack-index");
    if (!idxStr) return;

    const pack = (packsData as Pack[])[Number(idxStr)];
    if (!pack) return;

    // 1) Find the collection
    const collection = (collectionsData as Collection[]).find(
      (c) => c.id === pack.collectionId
    );
    if (!collection) {
      // Navigate with empty cards if collection missing
      navigate("/openedPack", { state: { packName: pack.name, cards: [] } });
      return;
    }

    // 2) Build candidate vehicle objects
    const candidates = (collection.vehicles ?? [])
      .map((id) => vehicleById.get(id))
      .filter(Boolean) as Vehicle[];

    // 3) Pick 3 random vehicles
    const picked = pickThreeRandom(candidates);

    // 4) Navigate to OpenPack with the actual vehicle objects 
    navigate("/openedPack", {
      state: {
        packName: pack.name,
        cards: picked,
      },
    });
  };

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
            ownedCards.map((card) => <div key={card.id} />)
          )}
        </div>
      </section>

      {/* Pack Shop (display-only). Clicks handled here */}
      <div onClick={handlePackGridClick}>
        <PackShop packs={packsData as Pack[]} />
      </div>
    </div>
  );
};

export default Open;
