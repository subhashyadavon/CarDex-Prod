import React, { useState } from "react";
import TextInput from "../../components/TextInput/TextInput";
import Button from "../../components/Button/Button";
import TradeCard, { Trade } from "../../components/TradeCard/TradeCard";
import CreateTradeModal from "../../components/CreateTradeModal/CreateTradeModal"; // ✅ NEW
import styles from "./Trade.module.css";

const mockTrades: Trade[] = [
  {
    id: "t1",
    status: "open",
    price: 125000,
    card: {
      makeModel: "Nissan GT-R",
      cardName: "R35 Premium Edition",
      imageUrl:
        "https://kucmypknqwnnmmdhduto.supabase.co/storage/v1/object/public/images/vehicles/skyline-gtr-r34-vspec.jpg",
      stat1Label: "POWER",
      stat1Value: "565",
      stat2Label: "WEIGHT",
      stat2Value: "1740",
      stat3Label: "BRAKING",
      stat3Value: "9.1",
      stat4Label: "TORQUE",
      stat4Value: "467",
      grade: "FACTORY",
      value: "125,000",
      rarity: "factory",
    },
  },
  {
    id: "t2",
    status: "open",
    price: 210000,
    card: {
      makeModel: "Toyota Supra",
      cardName: "A80 Twin Turbo",
      imageUrl:
        "https://kucmypknqwnnmmdhduto.supabase.co/storage/v1/object/public/images/vehicles/supra-mk4.jpg",
      stat1Label: "POWER",
      stat1Value: "320",
      stat2Label: "WEIGHT",
      stat2Value: "1560",
      stat3Label: "BRAKING",
      stat3Value: "8.7",
      stat4Label: "TORQUE",
      stat4Value: "315",
      grade: "LIMITED",
      value: "210,000",
      rarity: "limited",
    },
  },
  {
    id: "t3",
    status: "completed",
    price: 340000,
    card: {
      makeModel: "Nissan GT-R",
      cardName: "NISMO",
      imageUrl:
        "https://kucmypknqwnnmmdhduto.supabase.co/storage/v1/object/public/images/vehicles/skyline-gtr-r34.jpg",
      stat1Label: "POWER",
      stat1Value: "600",
      stat2Label: "WEIGHT",
      stat2Value: "1720",
      stat3Label: "BRAKING",
      stat3Value: "9.6",
      stat4Label: "TORQUE",
      stat4Value: "481",
      grade: "NISMO",
      value: "340,000",
      rarity: "nismo",
    },
  },
  {
    id: "t4",
    status: "open",
    price: 95000,
    card: {
      makeModel: "Mazda RX-7",
      cardName: "FD Spirit R",
      imageUrl:
        "https://kucmypknqwnnmmdhduto.supabase.co/storage/v1/object/public/images/vehicles/rx7-fd.jpg",
      stat1Label: "POWER",
      stat1Value: "276",
      stat2Label: "WEIGHT",
      stat2Value: "1280",
      stat3Label: "BRAKING",
      stat3Value: "8.4",
      stat4Label: "TORQUE",
      stat4Value: "231",
      grade: "LIMITED",
      value: "95,000",
      rarity: "limited",
    },
  },
  {
    id: "t5",
    status: "cancelled",
    price: 180000,
    card: {
      makeModel: "Honda NSX",
      cardName: "NA1 Type R",
      imageUrl:
        "https://kucmypknqwnnmmdhduto.supabase.co/storage/v1/object/public/images/vehicles/nsx.jpg",
      stat1Label: "POWER",
      stat1Value: "276",
      stat2Label: "WEIGHT",
      stat2Value: "1350",
      stat3Label: "BRAKING",
      stat3Value: "8.9",
      stat4Label: "TORQUE",
      stat4Value: "217",
      grade: "FACTORY",
      value: "180,000",
      rarity: "factory",
    },
  },
  {
    id: "t6",
    status: "open",
    price: 265000,
    card: {
      makeModel: "Subaru Impreza",
      cardName: "22B STi",
      imageUrl:
        "https://kucmypknqwnnmmdhduto.supabase.co/storage/v1/object/public/images/vehicles/evo-iv.jpg",
      stat1Label: "POWER",
      stat1Value: "276",
      stat2Label: "WEIGHT",
      stat2Value: "1270",
      stat3Label: "BRAKING",
      stat3Value: "8.8",
      stat4Label: "TORQUE",
      stat4Value: "268",
      grade: "LIMITED",
      value: "265,000",
      rarity: "limited",
    },
  },
];

const TradeSection: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState("");

  // ✅ NEW: modal state (tiny addition)
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [modalMode, setModalMode] = useState<"sell" | "trade">("trade");

  const filteredTrades = mockTrades.filter((trade) => {
    if (!searchTerm.trim()) return true;
    const q = searchTerm.toLowerCase();
    return (
      trade.card.makeModel.toLowerCase().includes(q) ||
      trade.card.cardName.toLowerCase().includes(q)
    );
  });

  return (
    <div className={styles.tradeContainer}>
      {/* Toolbar */}
      <div className={styles.tradeToolbar}>
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

        <Button size="large" variant="primary">
          Search
        </Button>

        {/* ✅ Hook up modal open */}
        <Button
          size="large"
          variant="secondary"
          onClick={() => {
            setModalMode("trade");
            setShowCreateModal(true);
          }}
        >
          Trade Card
        </Button>

        {/* ✅ Hook up modal open */}
        <Button
          size="large"
          variant="secondary"
          onClick={() => {
            setModalMode("sell");
            setShowCreateModal(true);
          }}
        >
          Sell Card
        </Button>
      </div>

      {/* ✅ Render modal if open */}
      {showCreateModal && (
        <CreateTradeModal
          mode={modalMode}
          onClose={() => setShowCreateModal(false)}
          onSubmit={(payload) => {
            // later: send to backend
            console.log("Create trade payload:", payload);
          }}
        />
      )}

      {/* Empty State */}
      {filteredTrades.length === 0 ? (
        <p className={styles.emptyState}>No open trades currently.</p>
      ) : (
        <>
          {/* Section Header */}
          <h2 className={`header-2 ${styles.sectionHeader}`}>Open Trades</h2>

          {/* List of TradeCards */}
          <div className={styles.tradeList}>
            {filteredTrades.map((trade) => (
              <TradeCard key={trade.id} trade={trade} />
            ))}
          </div>
        </>
      )}
    </div>
  );
};

export default TradeSection;
