// src/pages/Trade/Trade.tsx

import React, { useState, useEffect, useRef } from "react";
import TextInput from "../../components/TextInput/TextInput";
import Button from "../../components/Button/Button";
import TradeCard, {
  Trade as UiTrade,
} from "../../components/TradeCard/TradeCard";
import CreateTradeModal, {
  CreateTradePayload,
} from "../../components/CreateTradeModal/CreateTradeModal";
import styles from "./Trade.module.css";

import { useTrade } from "../../hooks/useTrade";
import { useAuth } from "../../hooks/useAuth";
import { OpenTrade, TradeEnum, GradeEnum } from "../../types/types";
import { cardService, Vehicle } from "../../services/cardService";
import { CarCardProps } from "../../components/Card/Card";
import { CreateTradeRequest } from "../../services/tradeService";

// ---------- helpers ----------

// Get the card id off an OpenTrade (handles cardId or card_id)
const getTradeCardId = (trade: OpenTrade): string | null => {
  const t: any = trade;
  const raw = t.cardId ?? t.card_id ?? null;
  if (raw === null || raw === undefined) return null;
  return String(raw);
};

// GradeEnum -> rarity string used by Card component
const gradeToRarity = (grade: GradeEnum | string): CarCardProps["rarity"] => {
  if (grade === GradeEnum.NISMO || grade === "NISMO") return "nismo";
  if (grade === GradeEnum.LIMITED_RUN || grade === "LIMITED_RUN")
    return "limited";
  return "factory";
};

/**
 * Try to find the best matching vehicle for a given card.
 * We support:
 *  - Exact match by vehicleId (if backend ever sends id)
 *  - Fuzzy match by make+model appearing in card.name / description
 */
const findVehicleForCard = (
  card: any | undefined,
  vehicles: Vehicle[]
): Vehicle | null => {
  if (!card || vehicles.length === 0) return null;

  // 1. Try by explicit vehicle id if present
  const rawVehicleId =
    (card as any)?.vehicleId ??
    (card as any)?.vehicle_id ??
    null;

  if (rawVehicleId != null) {
    const idStr = String(rawVehicleId);
    const byId = vehicles.find(
      (v) => v.id != null && String(v.id) === idStr
    );
    if (byId) return byId;
  }

  // 2. Try by matching make+model against card text
  const name: string = (card?.name ?? "").toString();
  const description: string = (card?.description ?? "").toString();
  const combined = `${name} ${description}`.toLowerCase();

  if (!combined.trim()) return null;

  // Example vehicle fields:
  // { year, make, model, stat1, stat2, stat3, value }
  const byName = vehicles.find((v) => {
    const full = `${v.make} ${v.model}`.toLowerCase();
    return combined.includes(full) || full.includes(name.toLowerCase());
  });

  if (byName) return byName;

  return null;
};

// Map OpenTrade + (optional) Card DTO + vehicles array -> UI Trade type used by <TradeCard />
const mapOpenTradeToUiTrade = (
  trade: OpenTrade,
  card: any | undefined,
  vehicles: Vehicle[]
): UiTrade => {
  const isForPrice = trade.type === TradeEnum.FOR_PRICE;

  const tradeCardId = getTradeCardId(trade);

  const name: string | undefined = card?.name;
  const description: string | undefined = card?.description;

  // Resolve vehicle from list
  const vehicle = findVehicleForCard(card, vehicles);

  const make = vehicle?.make ?? undefined;
  const model = vehicle?.model ?? undefined;
  const year = vehicle?.year ?? undefined;

  const makeModel =
    (make && model && `${make} ${model}`) ||
    description ||
    name ||
    (tradeCardId ? `Card #${tradeCardId}` : "Card");

  const cardName =
    name ||
    description ||
    (make && model ? `${year ?? ""} ${make} ${model}`.trim() : makeModel);

  const grade = card?.grade ?? "FACTORY";
  const rarity: CarCardProps["rarity"] = gradeToRarity(grade);

  // Prefer card.value, then vehicle.value
  const cardValueNumber: number | undefined =
    card?.value != null ? card.value : vehicle?.value;

  const cardValueString =
    cardValueNumber != null ? cardValueNumber.toLocaleString() : "";

  const tradePrice = trade.price ?? 0;

  // ---- vehicle stats -> Card stats ----
  // From backend: stat1, stat2, stat3
  const stat1 = vehicle?.stat1 ?? null;
  const stat2 = vehicle?.stat2 ?? null;
  const stat3 = vehicle?.stat3 ?? null;

  // For now we only have 3 stats; stat4 is VALUE in UI
  const stat4 = null;

  // Price shown in the TradeCard footer
  // FOR_PRICE -> use tradePrice (fallback to card value)
  // FOR_CARD  -> force 0 so it doesn't show the card's value as "price"
  let displayPriceNumber: number;
  if (isForPrice) {
    displayPriceNumber = tradePrice || cardValueNumber || 0;
  } else {
    displayPriceNumber = 0;
  }

  return {
    id: String(trade.id),
    status: "open", // this page shows open trades
    price: displayPriceNumber, // "Price ... Cr" in TradeCard footer
    tradeType: isForPrice ? "FOR_PRICE" : "FOR_CARD",
    card: {
      makeModel,
      cardName,
      imageUrl:
        card?.imageUrl ||
        (vehicle as any)?.image || // vehicle.image if present
        "/assets/cards/placeholder-card.png",

      // Map DB stats into UI stats with your existing labels
      stat1Label: "POWER",
      stat1Value: stat1 != null ? String(stat1) : "--",

      stat2Label: "WEIGHT",
      stat2Value: stat2 != null ? String(stat2) : "--",

      stat3Label: "BRAKING",
      stat3Value: stat3 != null ? String(stat3) : "--",

      stat4Label: "VALUE",
      stat4Value: cardValueString || "--",

      grade,
      value: cardValueString,
      rarity,
    },
  };
};

// ---------- component ----------

const TradeSection: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState("");

  const [showCreateModal, setShowCreateModal] = useState(false);
  const [modalMode, setModalMode] = useState<"sell" | "trade">("trade");

  const { user } = useAuth();

  const {
    filteredTrades,
    loadTrades,
    isLoading,
    refreshTrades,
    createTrade,
    acceptTrade,
  } = useTrade();

  // Always treat filteredTrades as array
  const safeFilteredTrades: OpenTrade[] = Array.isArray(filteredTrades)
    ? filteredTrades
    : [];

  // Cache of card details keyed by card id
  const [cardCache, setCardCache] = useState<Record<string, any>>({});

  // Vehicles list from /cards/vehicles
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);

  // Only load trades once
  const hasLoadedRef = useRef(false);

  // Load open trades once
  useEffect(() => {
    if (hasLoadedRef.current) return;
    hasLoadedRef.current = true;

    loadTrades().catch((err) =>
      console.error("[TradeSection] Failed to load trades:", err)
    );
  }, [loadTrades]);

  // Load ALL vehicles once (for stats)
  useEffect(() => {
    const fetchVehicles = async () => {
      try {
        const vehiclesResponse = await cardService.getAllVehicles();
        setVehicles(vehiclesResponse);
      } catch (err) {
        console.error("[TradeSection] Failed to load vehicles:", err);
      }
    };

    fetchVehicles();
  }, []);

  // When trades change, fetch card details for any card ids we don't have yet
  useEffect(() => {
    const allCardIds = safeFilteredTrades
      .map((t) => getTradeCardId(t))
      .filter((id): id is string => !!id);

    const missingIds = allCardIds.filter((id) => !(id in cardCache));

    if (missingIds.length === 0) return;

    const fetchCards = async () => {
      try {
        const results = await Promise.all(
          missingIds.map((id) => cardService.getCardById(id))
        );

        setCardCache((prev) => {
          const next: Record<string, any> = { ...prev };
          results.forEach((card: any) => {
            if (card && card.id != null) {
              next[String(card.id)] = card;
            }
          });
          return next;
        });
      } catch (err) {
        console.error("[TradeSection] Failed to load cards for trades:", err);
      }
    };

    fetchCards();
  }, [safeFilteredTrades, cardCache]);

  // Convert to UI TradeCard format (using loaded card + vehicles where available)
  const uiTrades: UiTrade[] = safeFilteredTrades.map((trade) => {
    const tradeCardId = getTradeCardId(trade);
    const cardDetails = tradeCardId ? cardCache[tradeCardId] : undefined;
    return mapOpenTradeToUiTrade(trade, cardDetails, vehicles);
  });

  // Local search
  const filteredUiTrades = uiTrades.filter((trade) => {
    if (!searchTerm.trim()) return true;
    const q = searchTerm.toLowerCase();
    return (
      trade.card.makeModel.toLowerCase().includes(q) ||
      trade.card.cardName.toLowerCase().includes(q)
    );
  });

  // Handle buying a FOR_PRICE trade
  const handleBuyTrade = async (tradeId: string) => {
    try {
      console.log("[TradeSection] Buying trade:", tradeId);
      await acceptTrade(tradeId);
      await refreshTrades();
    } catch (err) {
      console.error("[TradeSection] Failed to buy trade:", err);
    }
  };

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

        <Button
          size="large"
          variant="primary"
          onClick={() => {
            refreshTrades().catch((err) =>
              console.error("[TradeSection] Failed to refresh trades:", err)
            );
          }}
        >
          Search
        </Button>

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

      {/* Create Trade Modal */}
      {showCreateModal && (
        <CreateTradeModal
          mode={modalMode}
          onClose={() => {
            setShowCreateModal(false);
            // Reload trades whenever modal closes
            refreshTrades().catch((err) =>
              console.error("[TradeSection] Failed to refresh trades:", err)
            );
          }}
          onSubmit={async (payload: CreateTradePayload) => {
            if (!user) {
              console.error("[TradeSection] Cannot create trade: no user");
              return;
            }

            try {
              let request: CreateTradeRequest;

              if (payload.type === "currency") {
                // FOR_PRICE trade
                request = {
                  userId: user.id,
                  cardId: payload.offeredCardId,
                  type: TradeEnum.FOR_PRICE,
                  price: payload.price ?? 0,
                  wantCardId: null,
                };
              } else {
                // FOR_CARD trade
                request = {
                  userId: user.id,
                  cardId: payload.offeredCardId,
                  type: TradeEnum.FOR_CARD,
                  price: 0, // numeric zero instead of null
                  wantCardId: payload.requestedCardId ?? null, // CARD ID we want
                };
              }

              console.log("[TradeSection] Creating trade with payload:", request);
              await createTrade(request);
            } catch (err) {
              console.error("[TradeSection] Failed to create trade:", err);
            }
          }}
        />
      )}

      {/* Loading / Empty / List */}
      {isLoading ? (
        <p className={styles.emptyState}>Loading trades...</p>
      ) : filteredUiTrades.length === 0 ? (
        <p className={styles.emptyState}>No open trades currently.</p>
      ) : (
        <>
          <h2 className={`header-2 ${styles.sectionHeader}`}>Open Trades</h2>

          <div className={styles.tradeList}>
            {filteredUiTrades.map((trade) => (
              <TradeCard key={trade.id} trade={trade} onBuy={handleBuyTrade} />
            ))}
          </div>
        </>
      )}
    </div>
  );
};

export default TradeSection;
