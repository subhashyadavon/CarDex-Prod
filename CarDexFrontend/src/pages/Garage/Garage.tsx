import React from "react";
import Garage from "../../components/Garage/Garage";

const GarageSection: React.FC = () => {
  return (
    <>
      <p
        className="body-1"
        style={{ color: "var(--content-secondary)", marginBottom: 16 }}
      >
        Your cars will show here.
      </p>
      <Garage />
    </>
  );
};

export default GarageSection;
