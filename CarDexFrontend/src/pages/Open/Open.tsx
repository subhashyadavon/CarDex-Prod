import React from "react";
import PackShop from "../../components/Pack/PackShop";
import packsData from "../../data/packs.json";
import styles from "./Open.module.css"; // <-- import the module

const Open: React.FC = () => {
  // When you wire this up, replace with real data from context/API
  const ownedCards: any[] = [];

  return (
    <div className={styles.container}>
      {/* Owned Cards Section */}
      <section className={styles.ownedSection}>
        <h2 className="header-1" style={{ marginBottom: "0.75rem" }}>
          Owned Cards
        </h2>

        {/* Grid container (empty for now) */}
        <div className={styles.grid}>
          {ownedCards.length === 0 ? (
            <div className={`body-1 ${styles.emptyMessage}`}>
              No cards owned...
            </div>
          ) : (
            ownedCards.map((card) => (
              <div key={card.id} /* your future card tile here */ />
            ))
          )}
        </div>
      </section>

      {/* Pack Shop */}
      <PackShop packs={packsData} />
    </div>
  );
};

export default Open;
