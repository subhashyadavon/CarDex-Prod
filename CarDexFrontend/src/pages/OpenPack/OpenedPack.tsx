import React, { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import styles from "./OpenedPack.module.css";

type Vehicle = {
  id: string; // Assuming the id is a string based on your API response
  year: string;
  make: string;
  model: string;
  stat1: number;
  stat2: number;
  statN: number;
  value: number;
  image: string; // image URL
  name: string; // Card name 
};

type State = { packName?: string; cards?: Vehicle[] };

const OpenPack: React.FC = () => {
  const navigate = useNavigate();
  const { state } = useLocation() as { state: State };
  const cards = state?.cards ?? [];
  const heading = state?.packName
    ? `${state.packName} — Opened Cards`
    : "Opened Cards";

  const [play, setPlay] = useState(false);

  useEffect(() => {
    const t = setTimeout(() => setPlay(true), 60);
    return () => clearTimeout(t);
  }, []);

  // // Debugging log for cards and image URLs
  // useEffect(() => {
  //   console.log("Cards Data:", cards);
  //   cards.forEach((v) => {
  //     console.log(`Card ID: ${v.id}, Image URL: ${v.image}`);
  //   });
  // }, [cards]);

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
              cards.map((v) => (
                <article
                  key={v.id}
                  className={styles.cardTile}
                  title={v.name} 
                >
                  <div className={styles.cardImageWrap}>
                    
                    {v.image ? (
                      <img
                        src={v.image}
                        alt={v.name} // Use card's name for the alt text
                        className={styles.cardImage}
                      />
                    ) : (
                      <div className={styles.noImage}>Image not available</div>
                    )}
                  </div>

                  {/* Show only the card name below the image */}
                  <div className={styles.cardName}>{v.name}</div>
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
