import React from "react";
import PackShop from "../../components/Pack/PackShop";
import packsData from "../../data/packs.json";
import styles from "./Open.module.css"; 

const Open: React.FC = () => {
  const ownedCards: any[] = [];

  return (
    <div className={styles.container}>
      {/* Owned Cards Section */}
      <section className={styles.ownedSection}>
        <h2 className="header-1" style={{ marginBottom: "0.75rem" }}>
          Owned Cards
        </h2>

        <div className={styles.grid}>
          {ownedCards.length === 0 ? (
            <div className={`body-1 ${styles.emptyMessage}`}>
              No cards owned...
            </div>
          ) : (
            ownedCards.map((card) => (
              <div key={card.id}/>
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
