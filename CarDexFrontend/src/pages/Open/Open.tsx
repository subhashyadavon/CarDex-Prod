import React from "react";
import PackShop from "../../components/Pack/PackShop";
import packsData from "../../data/packs.json";

const OpenSection: React.FC = () => {
  return (
    <div>
      <PackShop packs={packsData} />
    </div>
  );
};

export default OpenSection;
