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
import { cardService } from "../../services/cardService";
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

// Map OpenTrade + (optional) Card DTO -> UI Trade type used by <TradeCard />
// Card DTO example:
// { id, name, description, grade, value, imageUrl, ... }
const mapOpenTradeToUiTrade = (trade: OpenTrade, card?: any): UiTrade => {
  const isForPrice = trade.type === TradeEnum.FOR_PRICE;

  const tradeCardId = getTradeCardId(trade);

  const name: string | undefined = card?.name;
  const description: string | undefined = card?.description;

  const makeModel =
    description || name || (tradeCardId ? `Card #${tradeCardId}` : "Card");

  const cardName = name || description || makeModel;

  const grade = card?.grade ?? "FACTORY";
  const rarity: CarCardProps["rarity"] = gradeToRarity(grade);

  const tradePrice = trade.price ?? 0;
  const cardValueNumber: number | undefined = card?.value;
  const cardValueString =
    cardValueNumber != null ? cardValueNumber.toLocaleString() : "";

  // Price shown in the TradeCard footer – if the trade has a price, use that;
  // otherwise fall back to card's own value.
  const displayPriceNumber = tradePrice || cardValueNumber || 0;

  return {
    id: String(trade.id),
    status: "open", // this page shows open trades
    price: displayPriceNumber, // "Price ... Cr" in TradeCard footer
    card: {
      makeModel,
      cardName,
      imageUrl: card?.imageUrl || "/assets/cards/placeholder-card.png",
      // Let Card.tsx use its defaults for other stats
      grade,
      value: cardValueString, // value shown with coin icon in card footer
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
  } = useTrade();

  // Always treat filteredTrades as array
  const safeFilteredTrades: OpenTrade[] = Array.isArray(filteredTrades)
    ? filteredTrades
    : [];

  // Cache of card details keyed by card id
  const [cardCache, setCardCache] = useState<Record<string, any>>({});

  // Only load trades once
  const hasLoadedRef = useRef(false);

  useEffect(() => {
    if (hasLoadedRef.current) return;
    hasLoadedRef.current = true;

    loadTrades().catch((err) =>
      console.error("[TradeSection] Failed to load trades:", err)
    );
  }, [loadTrades]);

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

  // Convert to UI TradeCard format (using loaded card details where available)
  const uiTrades: UiTrade[] = safeFilteredTrades.map((trade) => {
    const tradeCardId = getTradeCardId(trade);
    const cardDetails = tradeCardId ? cardCache[tradeCardId] : undefined;
    return mapOpenTradeToUiTrade(trade, cardDetails);
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
                  price: null,
                  wantCardId: payload.requestedCardId ?? null,
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
              <TradeCard key={trade.id} trade={trade} />
            ))}
          </div>
        </>
      )}
    </div>
  );
};

export default TradeSection;
