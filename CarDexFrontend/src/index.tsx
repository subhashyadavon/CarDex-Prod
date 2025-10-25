import React from "react";
import ReactDOM from "react-dom/client";
import jdmLogo from "./assets/jdm.png";
import lc500 from "./assets/lc500.png";


/* -------------------- inject font + styles (single-file setup) -------------------- */
const style = document.createElement("style");
style.innerHTML = `
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700;800;900&display=swap');

:root{
  --gradient-light1:#E01F5F; --gradient-light2:#E05720;
  --gradient-dark1:#381010;  --gradient-dark2:#2C1818;
  --content-primary:#FFD8D7; --content-secondary:#A37271;
  --block-primary:#2B0202;   --block-secondary:#5D2B2A; --block-tertiary:#180303;
}
html,body,#root{height:100%}
body{margin:0;background:#1f0f0f;color:var(--content-primary);font-family:Inter,system-ui,-apple-system,Segoe UI,Roboto,Arial}

/* ---- CARD (pixel-tuned to the screenshot) ---- */
.cdx-card{
  width:456px; border-radius:24px; padding:22px 22px 18px;
  background:var(--block-secondary); color:var(--content-primary);
  box-shadow:0 14px 36px rgba(0,0,0,.48); display:grid; gap:18px;
  border:1px solid rgba(255,216,215,.05);
}
.cdx-header{position:relative; padding-right:116px;}
.cdx-pre{font-size:13px; line-height:1.1; letter-spacing:.28em; font-weight:700; color:var(--content-secondary); text-transform:uppercase;}
.cdx-title{margin:6px 0 0; font-size:36px; font-weight:800; line-height:1.02;}
.cdx-crest{position:absolute; right:0; top:0; height:50px; width:96px; object-fit:cover; border-radius:12px; background:var(--block-tertiary); padding:6px 8px; box-shadow:0 2px 10px rgba(0,0,0,.35);}

.cdx-mediaWrap{background:var(--block-tertiary); border-radius:16px; padding:8px; box-shadow:inset 0 0 0 1px rgba(255,216,215,.06);}
.cdx-media{display:block; width:100%; height:288px; object-fit:cover; border-radius:12px; filter:sepia(.8) saturate(.9) contrast(1.06) brightness(1.02);}

.cdx-stats{display:grid; grid-template-columns:repeat(2,1fr); gap:34px 28px; padding:4px 4px 0;}
.cdx-stat{text-align:center;}
.cdx-val{font-size:88px; font-weight:900; line-height:.95; letter-spacing:-.01em;}
.cdx-lbl{margin-top:10px; font-size:15px; letter-spacing:.42em; color:var(--content-secondary);}

.cdx-footer{margin-top:4px; display:flex; justify-content:space-between; align-items:center;}
.cdx-badge{padding:12px 16px; border-radius:12px; font-weight:800; font-size:18px; letter-spacing:.30em; background:#7A4545; color:#F8EAEA; box-shadow:inset 0 0 0 1px rgba(0,0,0,.25);}
.cdx-price{display:inline-flex; align-items:center; gap:10px; font-weight:800; font-size:18px; padding:12px 16px; border-radius:12px; background:var(--block-tertiary); box-shadow:inset 0 0 0 1px rgba(255,216,215,.06);}
.cdx-coin{transform:translateY(1px);}

/* container background to match vibe */
.cdx-bg{min-height:100vh; display:flex; align-items:flex-start; justify-content:center; padding:40px 24px;
  background:linear-gradient(180deg,var(--gradient-dark1),var(--gradient-dark2));
}
`;
document.head.appendChild(style);

/* -------------------- Card component (inline) -------------------- */
type Rarity = "FACTORY" | "LIMITED_RUN" | "NISMO";
type Stat = { label: string; value: number };

function CarCard(props: {
  makeModel: string;
  title: string;
  imageUrl: string;
  crestUrl?: string;
  stats: Stat[];            // expects 4 stats
  rarity: Rarity;
  price: number;
}) {
  const { makeModel, title, imageUrl, crestUrl, stats, rarity, price } = props;

  return (
    <article className="cdx-card" aria-label={`${makeModel} – ${title} card`}>
      <header className="cdx-header">
        <div className="cdx-pre">{makeModel}</div>
        <h3 className="cdx-title">{title}</h3>
        {crestUrl ? <img className="cdx-crest" src={crestUrl} alt="" /> : null}
      </header>

      <div className="cdx-mediaWrap">
        <img className="cdx-media" src={imageUrl} alt={title} />
      </div>

      <section className="cdx-stats">
        {stats.slice(0, 4).map((s) => (
          <div key={s.label} className="cdx-stat">
            <div className="cdx-val">{s.value}</div>
            <div className="cdx-lbl">{s.label}</div>
          </div>
        ))}
      </section>

      <footer className="cdx-footer">
        <span className="cdx-badge">{rarity.replace("_", " ")}</span>
        <span className="cdx-price"><span className="cdx-coin" aria-hidden>🪙</span>{price.toLocaleString()}</span>
      </footer>
    </article>
  );
}

/* -------------------- Preview page -------------------- */
function CardPreview() {
  return (
    <div className="cdx-bg">
      <CarCard
        makeModel="LEXUS LC 500"
        title="The Daily"
        crestUrl={jdmLogo}
        imageUrl={lc500}           // local car photo
        stats={[
          { label: "POWER", value: 471 },
          { label: "WEIGHT", value: 1345 },
          { label: "SPEED", value: 640 },
          { label: "BRAKING", value: 1502 },
        ]}
        rarity="FACTORY"
        price={115_999}
      />
    </div>
  );
}

/* -------------------- Mount -------------------- */
const root = document.getElementById("root");
if (!root) throw new Error("Root element #root not found");

ReactDOM.createRoot(root).render(
  <React.StrictMode>
    <CardPreview />
  </React.StrictMode>
);
