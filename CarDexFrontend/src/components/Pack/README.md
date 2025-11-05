# Pack Component

CARDEX's Pack component, showcased using our **FFPU system**.

## F - Files
- **Pack.tsx** - Pack component with TypeScript types
- **Pack.module.css** - Pack styles with gradient background
- **Demo.tsx** - Demo showcase with examples

## F - Features

### Dimensions
- **Width**: 200px
- **Height**: 300px
- **Pack Image**: 180px × 90px (centered)

### Visual Features
- Gradient background (orange at top → black at bottom)
- Decorative line separator
- Tilted pack image with hover animation
- Glass-morphism footer
- Hover effects with lift and glow

## P - Properties

```typescript
type PackProps = 
{
  name: string;           // Pack name (e.g., "Starter Pack")
  packType: string;       // Type label (e.g., "BOOSTER PACK")
  imageUrl: string;       // Main pack image (180×90px recommended)
  price: number;          // Pack price in coins
  onClick?: () => void;   // Click handler for opening pack
};
```

## U - Usage

### Basic Pack
```tsx
import Pack from "./Pack";

<Pack
  name="Starter Pack"
  packType="BOOSTER PACK"
  imageUrl="/assets/packs/starter.png"
  price={2500}
  onClick={handleOpenPack}
/>
```

### Pack with Icon
```tsx
<Pack
  name="Premium Collection"
  packType="BOOSTER PACK"
  imageUrl="/assets/packs/premium.png"
  iconUrl="/assets/icons/premium-badge.png"
  price={5000}
  onClick={handleOpenPack}
/>
```

### Pack with Custom Coin Icon
```tsx
import coinIcon from "./assets/coin.png";

<Pack
  name="Ultra Rare Pack"
  packType="LIMITED EDITION"
  imageUrl="/assets/packs/ultra.png"
  price={15000}
  coinIconUrl={coinIcon}
  onClick={handleOpenPack}
/>
```

## Pack Grid Layout

### Responsive Grid
```tsx
<div style={{
  display: "grid",
  gridTemplateColumns: "repeat(auto-fill, minmax(200px, 1fr))",
  gap: "32px",
  justifyItems: "center"
}}>
  <Pack {...pack1Props} />
  <Pack {...pack2Props} />
  <Pack {...pack3Props} />
</div>
```

### Fixed 3-Column Layout
```tsx
<div style={{
  display: "grid",
  gridTemplateColumns: "repeat(3, 200px)",
  gap: "32px",
  justifyContent: "center"
}}>
  <Pack {...pack1Props} />
  <Pack {...pack2Props} />
  <Pack {...pack3Props} />
</div>
```

## Real-World Integration

### Pack Shop Page
```tsx
import { useState } from "react";
import Pack from "./Pack";

function PackShop() {
  const [userCoins, setUserCoins] = useState(50000);
  
  const packs = [
    {
      id: 1,
      name: "Starter Pack",
      packType: "BOOSTER PACK",
      imageUrl: "/packs/starter.png",
      price: 2500
    },
    {
      id: 2,
      name: "Premium Pack",
      packType: "PREMIUM",
      imageUrl: "/packs/premium.png",
      iconUrl: "/icons/premium.png",
      price: 5000
    }
  ];

  const handleOpenPack = (pack) => {
    if (userCoins >= pack.price) {
      setUserCoins(userCoins - pack.price);
      // Open pack logic here
      console.log(`Opening ${pack.name}!`);
    } else {
      alert("Not enough coins!");
    }
  };

  return (
    <div>
      <h1>Pack Shop - Coins: {userCoins}</h1>
      <div style={{ display: "grid", gap: "32px" }}>
        {packs.map(pack => (
          <Pack
            key={pack.id}
            {...pack}
            onClick={() => handleOpenPack(pack)}
          />
        ))}
      </div>
    </div>
  );
}
```

### With State Management
```tsx
import Pack from "./Pack";

function PackList({ packs, onPurchase }) {
  return (
    <div className="pack-grid">
      {packs.map(pack => (
        <Pack
          key={pack.id}
          name={pack.name}
          packType={pack.type}
          imageUrl={pack.image}
          iconUrl={pack.icon}
          price={pack.price}
          onClick={() => onPurchase(pack)}
        />
      ))}
    </div>
  );
}
```
