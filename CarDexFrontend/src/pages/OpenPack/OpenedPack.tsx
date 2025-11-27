import React, { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import styles from "./OpenedPack.module.css";

// NEW: use the DTO that matches backend CardDetailedResponse
import type { CardDetailed } from "../../types/types";

// CHANGED: state now expects cards: CardDetailed[]
type State = { packName?: string; cards?: CardDetailed[] };

const OpenPack: React.FC = () => {
  const navigate = useNavigate();
  const { state } = useLocation() as { state: State };

  // CHANGED: cards are CardDetailed[] from backend
  const cards = state?.cards ?? [];
  const heading = state?.packName
    ? `${state.packName} — Opened Cards`
    : "Opened Cards";

  const [play, setPlay] = useState(false);

  useEffect(() => {
    const t = setTimeout(() => setPlay(true), 60);
    return () => clearTimeout(t);
  }, []);

  return (
    <div className={`bg-gradient-dark ${styles.page}`}>
      <div className={styles.content}>
        {/* Add animations */}
        <section className={`${styles.panel} ${play ? styles.play : ""}`}>
          <h2 className="header-1" style={{ marginBottom: "0.75rem" }}>
            {heading}
          </h2>

          <div className={styles.grid}>
            {cards.length === 0 ? (
              <div className={`body-1 ${styles.emptyMessage}`}>
                No cards drawn.
              </div>
            ) : (
              cards.map((c) => (
                <article
                  key={c.id}
                  className={styles.cardTile}
                  title={c.name} // CHANGED: use backend card name
                >
                  <div className={styles.cardImageWrap}>
                    {c.imageUrl ? (
                      <img
                        src={c.imageUrl}
                        alt={c.name}
                        className={styles.cardImage}
                      />
                    ) : (
                      <div className={styles.noImage}>Image not available</div>
                    )}
                  </div>

                  {/* Show card name below the image */}
                  <div className={styles.cardName}>{c.name}</div>
                </article>
              ))
            )}
          </div>

          <div className={styles.actions}>
            <button onClick={() => navigate(-1)} className={styles.backBtn}>
              Back
            </button>
          </div>
        </section>
      </div>
    </div>
  );
};

export default OpenPack;
