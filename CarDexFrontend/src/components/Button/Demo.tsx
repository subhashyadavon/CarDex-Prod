import React from "react";
import Button from "./Button";
import "../../App.css";

// Example icon component (you can replace with your actual icons)
const SettingsIcon = () => (
  <svg width="20" height="20" viewBox="0 0 20 20" fill="currentColor">
    <path d="M10 6a4 4 0 100 8 4 4 0 000-8zm-2 4a2 2 0 114 0 2 2 0 01-4 0z"/>
    <path d="M10 0C8.68678 0 7.625 1.06178 7.625 2.375c0 .34834.07495.67862.20996.97656l-1.41992 1.41993C5.67862 4.62495 5.34834 4.55 5 4.55c-1.38071 0-2.5 1.11929-2.5 2.5 0 .34834.07495.67862.20996.97656L1.29004 9.43652C.992097 9.30151.661818 9.2265.313477 9.2265-1.06819 9.2265-2.1875 10.3458-2.1875 11.7265c0 .3483.07495.6786.20996.9766l1.41993 1.4199c-.13501.2979-.20996.6282-.20996.9766 0 1.3807 1.11929 2.5 2.5 2.5.34834 0 .67862-.075.97656-.21l1.41993 1.42c-.13501.298-.20996.628-.20996.976 0 1.313 1.06178 2.375 2.375 2.375 1.3132 0 2.375-1.062 2.375-2.375 0-.348-.075-.678-.21-.976l1.42-1.42c.298.135.628.21.976.21 1.381 0 2.5-1.119 2.5-2.5 0-.348-.075-.678-.21-.976l1.42-1.42c.298.135.628.21.976.21 1.381 0 2.5-1.119 2.5-2.5 0-.348-.075-.678-.21-.976l1.42-1.42c.135.298.21.628.21.976 0 1.3132 1.062 2.375 2.375 2.375S20 11.3132 20 10s-1.062-2.375-2.375-2.375c-.348 0-.678.075-.976.21l-1.42-1.42c.135-.298.21-.628.21-.976 0-1.3807-1.119-2.5-2.5-2.5-.348 0-.678.075-.976.21l-1.42-1.41993C10.678 1.59995 10.348 1.525 10 1.525z"/>
  </svg>
);

export default function ButtonShowcase() {
  return (
    <div className="bg-gradient-dark" style={{ minHeight: "100vh", padding: "48px" }}>
      <div style={{ maxWidth: "800px", margin: "0 auto" }}>
        <h1 className="titlecase" style={{ marginBottom: "48px", textAlign: "center" }}>
          BUTTONS
        </h1>

        {/* Regular Size - Primary */}
        <section style={{ marginBottom: "48px" }}>
          <h2 className="header-2" style={{ marginBottom: "16px" }}>Regular Size - Primary</h2>
          <div style={{ display: "flex", gap: "16px", flexWrap: "wrap" }}>
            <Button size="regular" variant="primary" icon={<SettingsIcon />}>
              Button
            </Button>
            <Button size="regular" variant="primary">
              Button
            </Button>
            <Button size="regular" variant="primary" icon={<SettingsIcon />} iconOnly />
          </div>
        </section>

        {/* Regular Size - Secondary */}
        <section style={{ marginBottom: "48px" }}>
          <h2 className="header-2" style={{ marginBottom: "16px" }}>Regular Size - Secondary</h2>
          <div style={{ display: "flex", gap: "16px", flexWrap: "wrap" }}>
            <Button size="regular" variant="secondary" icon={<SettingsIcon />}>
              Button
            </Button>
            <Button size="regular" variant="secondary">
              Button
            </Button>
            <Button size="regular" variant="secondary" icon={<SettingsIcon />} iconOnly />
          </div>
        </section>

        {/* Large Size - Primary */}
        <section style={{ marginBottom: "48px" }}>
          <h2 className="header-2" style={{ marginBottom: "16px" }}>Large Size - Primary</h2>
          <div style={{ display: "flex", gap: "16px", flexWrap: "wrap" }}>
            <Button size="large" variant="primary" icon={<SettingsIcon />}>
              Button
            </Button>
            <Button size="large" variant="primary">
              Button
            </Button>
            <Button size="large" variant="primary" icon={<SettingsIcon />} iconOnly />
          </div>
        </section>

        {/* Large Size - Secondary */}
        <section style={{ marginBottom: "48px" }}>
          <h2 className="header-2" style={{ marginBottom: "16px" }}>Large Size - Secondary</h2>
          <div style={{ display: "flex", gap: "16px", flexWrap: "wrap" }}>
            <Button size="large" variant="secondary" icon={<SettingsIcon />}>
              Button
            </Button>
            <Button size="large" variant="secondary">
              Button
            </Button>
            <Button size="large" variant="secondary" icon={<SettingsIcon />} iconOnly />
          </div>
        </section>

        {/* Disabled State */}
        <section style={{ marginBottom: "48px" }}>
          <h2 className="header-2" style={{ marginBottom: "16px" }}>Disabled State</h2>
          <div style={{ display: "flex", gap: "16px", flexWrap: "wrap" }}>
            <Button size="regular" variant="primary" disabled>
              Button
            </Button>
            <Button size="large" variant="secondary" icon={<SettingsIcon />} disabled>
              Button
            </Button>
          </div>
        </section>

        {/* Usage Examples */}
        <section style={{ marginBottom: "48px" }}>
          <h2 className="header-2" style={{ marginBottom: "16px" }}>Common Use Cases</h2>
          <div style={{ display: "flex", gap: "16px", flexWrap: "wrap" }}>
            <Button 
              size="large" 
              variant="primary"
              onClick={() => alert("Pack opened!")}
            >
              Open Pack
            </Button>
            <Button 
              size="regular" 
              variant="secondary"
              onClick={() => alert("Card added to garage!")}
            >
              Add to Garage
            </Button>
            <Button 
              size="regular" 
              variant="primary"
              icon={<span>⚙️</span>}
              iconOnly
              onClick={() => alert("Settings clicked!")}
            />
          </div>
        </section>
      </div>
    </div>
  );
}