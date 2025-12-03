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

// Get the card id off an OpenTrade (handles cardId or card_id)
const getTradeCardId = (trade: OpenTrade): string | null => {
  const t: any = trade;
  const raw = t.cardId ?? t.card_id ?? null;
  if (raw === null || raw === undefined) return null;
  return String(raw);
};

// Get the user id off an OpenTrade (handles userId or user_id)
const getTradeUserId = (trade: OpenTrade): string | null => {
  const t: any = trade;
  const raw = t.userId ?? t.user_id ?? null;
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

const findVehicleForCard = (
  card: any | undefined,
  vehicles: Vehicle[]
): Vehicle | null => {
  if (!card || vehicles.length === 0) return null;

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

  const name: string = (card?.name ?? "").toString();
  const description: string = (card?.description ?? "").toString();
  const combined = `${name} ${description}`.toLowerCase();

  if (!combined.trim()) return null;

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
  const tradeCardId = getTradeCardId(trade);

  const name: string | undefined = card?.name;
  const description: string | undefined = card?.description;

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

  const cardValueNumber: number | undefined =
    card?.value != null ? card.value : vehicle?.value;

  const cardValueString =
    cardValueNumber != null ? cardValueNumber.toLocaleString() : "";

  const tradePrice = trade.price ?? 0;

  const stat1 = vehicle?.stat1 ?? null;
  const stat2 = vehicle?.stat2 ?? null;
  const stat3 = vehicle?.stat3 ?? null;
  const stat4 = null;

  const displayPriceNumber = tradePrice || cardValueNumber || 0;

  const vehicleImageUrl =
    vehicle?.imageUrl ||
    (vehicle as any)?.image ||
    card?.imageUrl ||
    "/assets/cards/placeholder-card.png";

  return {
    id: String(trade.id),
    status: "open",
    price: displayPriceNumber,
    tradeType: "FOR_PRICE",
    card: {
      makeModel,
      cardName,
      imageUrl: vehicleImageUrl,
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

type TradeWithUi = {
  open: OpenTrade;
  ui: UiTrade;
};

const TradeSection: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [showCreateModal, setShowCreateModal] = useState(false);

  const { user } = useAuth();

  const {
    filteredTrades,
    loadTrades,
    isLoading,
    refreshTrades,
    createTrade,
    acceptTrade,
  } = useTrade();

  const safeFilteredTrades: OpenTrade[] = (Array.isArray(filteredTrades)
    ? filteredTrades
    : []
  ).filter((t) => t.type === TradeEnum.FOR_PRICE);

  const [cardCache, setCardCache] = useState<Record<string, any>>({});
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);

  const hasLoadedRef = useRef(false);

  useEffect(() => {
    if (hasLoadedRef.current) return;
    hasLoadedRef.current = true;

    loadTrades().catch((err) =>
      console.error("[TradeSection] Failed to load trades:", err)
    );
  }, [loadTrades]);

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

  const tradesWithUi: TradeWithUi[] = safeFilteredTrades.map((trade) => {
    const tradeCardId = getTradeCardId(trade);
    const cardDetails = tradeCardId ? cardCache[tradeCardId] : undefined;
    const baseUi = mapOpenTradeToUiTrade(trade, cardDetails, vehicles);

    const ownerId = getTradeUserId(trade);
    const isOwnTrade = ownerId != null && user?.id === ownerId;

    const uiWithOwnerFlag: UiTrade = {
      ...(baseUi as any),
      isOwnTrade,
    };

    return {
      open: trade,
      ui: uiWithOwnerFlag,
    };
  });

  const filteredTradesWithUi = tradesWithUi.filter(({ ui }) => {
    if (!searchTerm.trim()) return true;
    const q = searchTerm.toLowerCase();
    return (
      ui.card.makeModel.toLowerCase().includes(q) ||
      ui.card.cardName.toLowerCase().includes(q)
    );
  });

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
            setShowCreateModal(true);
          }}
        >
          Sell Card
        </Button>
      </div>

      {showCreateModal && (
        <CreateTradeModal
          onClose={() => {
            setShowCreateModal(false);
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
              const request: CreateTradeRequest = {
                userId: user.id,
                cardId: payload.offeredCardId,
                type: TradeEnum.FOR_PRICE,
                price: payload.price ?? 0,
                wantCardId: null,
              };

              console.log("[TradeSection] Creating trade with payload:", request);
              await createTrade(request);
            } catch (err) {
              console.error("[TradeSection] Failed to create trade:", err);
            }
          }}
        />
      )}

      {isLoading ? (
        <p className={styles.emptyState}>Loading trades...</p>
      ) : filteredTradesWithUi.length === 0 ? (
        <p className={styles.emptyState}>No open trades currently.</p>
      ) : (
        <>
          <h2 className={`header-2 ${styles.sectionHeader}`}>Open Trades</h2>

          <div className={styles.tradeList}>
            {filteredTradesWithUi.map(({ ui }) => (
              <TradeCard
                key={ui.id}
                trade={ui}
                onBuy={ui.isOwnTrade ? undefined : handleBuyTrade}
              />
            ))}
          </div>
        </>
      )}
    </div>
  );
};

export default TradeSection;
