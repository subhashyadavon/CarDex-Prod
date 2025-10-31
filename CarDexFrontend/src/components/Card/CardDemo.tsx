import React from "react";
import Card from "./Card";
import jdmLogo from "../../assets/jdm_classics.png"; // ✅ Correct import
import carImage from "../../assets/lc500.png";


const CardDemo: React.FC = () => {
  return (
    <div
      style={{
        background: "linear-gradient(45deg, #381010, #2C1818)",
        minHeight: "100vh",
        padding: "48px",
      }}
    >
      <h1
        className="header-1"
        style={{
          color: "var(--content-primary)",
          marginBottom: "32px",
          textAlign: "center",
        }}
      >
        CarDex — Card Showcase
      </h1>

      <div
        style={{
          display: "flex",
          justifyContent: "center",
          flexWrap: "wrap",
          gap: "24px",
        }}
      >
        <Card
          makeModel="LEXUS LC 500"
          cardName="The Daily"
          imageUrl={carImage}
          stat1Label="POWER"
          stat1Value="471"
          stat2Label="WEIGHT"
          stat2Value="1345"
          stat3Label="BRAKING"
          stat3Value="471"
          stat4Label="TORQUE"
          stat4Value="1345"
          grade="FACTORY"
          value="115,999"
          rarity="factory"
          collectionImageUrl={jdmLogo} // ✅ pass the imported image
        />
        <Card
          makeModel="LEXUS LC 500"
          cardName="The Daily"
          imageUrl={carImage}
          stat1Label="POWER"
          stat1Value="471"
          stat2Label="WEIGHT"
          stat2Value="1345"
          stat3Label="BRAKING"
          stat3Value="471"
          stat4Label="TORQUE"
          stat4Value="1345"
          grade="LIMITED RUN"
          value="115,999"
          rarity="limited"
          collectionImageUrl={jdmLogo}
        />
        <Card
          makeModel="LEXUS LC 500"
          cardName="The Daily"
          imageUrl={carImage}
          stat1Label="POWER"
          stat1Value="471"
          stat2Label="WEIGHT"
          stat2Value="1345"
          stat3Label="BRAKING"
          stat3Value="471"
          stat4Label="TORQUE"
          stat4Value="1345"
          grade="NISMO"
          value="115,999"
          rarity="nismo"
          collectionImageUrl={jdmLogo}
        />
      </div>
    </div>
  );
};

export default CardDemo;
