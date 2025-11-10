import React from "react";
import { useLocation, useNavigate } from "react-router-dom";
import styles from "./OpenedPack.module.css";

type Vehicle = {
  id: number;
  year: string;
  make: string;
  model: string;
  stat1: number;
  stat2: number;
  statN: number;
  value: number;
  image: string;
};

type State = { packName?: string; cards?: Vehicle[] };

const OpenPack: React.FC = () => {
  const navigate = useNavigate();
  const { state } = useLocation() as { state: State };
  const cards = state?.cards ?? [];
  const heading = state?.packName
    ? `${state.packName} — Opened Cards`
    : "Opened Cards";

  return (
    <div className={`bg-gradient-dark ${styles.page}`}>
      {/* Foreground panel */}
      <div className={styles.content}>
        <section className={styles.panel}>
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
                  title={`${v.year} ${v.make} ${v.model}`}
                >
                  <div className={styles.cardImageWrap}>
                    <img
                      src={v.image}
                      alt={`${v.make} ${v.model}`}
                      className={styles.cardImage}
                    />
                  </div>
                  <div className={styles.cardMeta}>
                    <div className="card-2">
                      {v.year} {v.make}
                    </div>
                    <div className="card-1">{v.model}</div>
                    <div className="card-4">
                      S1 {v.stat1} · S2 {v.stat2} · Wt {v.statN}
                    </div>
                  </div>
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
