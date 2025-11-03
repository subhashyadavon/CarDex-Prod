import React from "react";
import styles from "./PackShop.module.css";
import Pack from "../Pack/Pack";

interface PackInfo {
  name: string;
  packType: string;
  imageUrl: string;
  price: number;
}

interface PackShopProps {
  packs: PackInfo[];
}

const PackShop: React.FC<PackShopProps> = ({ packs }) => {
  return (
    <section className={styles.shop}>
      <header className={styles.header}>
        <h1 className={styles.title}>PACK SHOP</h1>
      </header>

      <div className={styles.grid}>
        {packs.map((pack) => (
          <Pack
            key={pack.name}
            name={pack.name}
            packType={pack.packType}
            imageUrl={pack.imageUrl}
            price={pack.price}
          />
        ))}
      </div>
    </section>
  );
};

export default PackShop;
