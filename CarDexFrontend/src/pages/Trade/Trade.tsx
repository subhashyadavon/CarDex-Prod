import React, { useState } from "react";
import TextInput from "../../components/TextInput/TextInput";
import Button from "../../components/Button/Button";
import styles from "./Trade.module.css";

const TradeSection: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState("");

  return (
    <div className={styles.tradeContainer}>
      <div className={styles.tradeToolbar}>
        {/* Search Input */}
        <TextInput
          type="search"
          size="large"
          placeholder="Search for trades..."
          value={searchTerm}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
            setSearchTerm(e.target.value)
          }
          className={styles.searchInput}
        />

        {/* Search Button */}
        <Button size="large" variant="primary">
          Search
        </Button>

        {/* Trade Card */}
        <Button size="large" variant="secondary">
          Trade Card
        </Button>

        {/* Sell Card */}
        <Button size="large" variant="secondary">
          Sell Card
        </Button>
      </div>
    </div>
  );
};

export default TradeSection;
