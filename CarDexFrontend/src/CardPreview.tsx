import React from "react";
import CarCard from "./components/Card/Card";

export default function CardPreview(){
  return (
    <div style={{
      minHeight:"100vh",padding:24,
      background:"linear-gradient(180deg, var(--gradient-dark1), var(--gradient-dark2))"
    }}>
      <CarCard
        makeModel="LEXUS LC 500"
        title="The Daily"
        imageUrl="https://images.unsplash.com/photo-1549921296-3a6b89e3a5a2?q=80&w=1200&auto=format&fit=crop"
        crestUrl="https://i.imgur.com/2yaf2mE.png"     // or your own JDM badge
        stats={[
          { label:"POWER", value:471 },
          { label:"WEIGHT", value:1345 },
          { label:"SPEED", value:640 },
          { label:"BRAKING", value:1502 },
        ]}
        rarity="FACTORY"
        price={115_999}
      />
    </div>
  );
}
