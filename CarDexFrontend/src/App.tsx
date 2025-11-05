import React, { useState } from "react";
import Header, { NavItem } from "./components/Header/Header";
import "./App.css";
import PackShop from "./components/Pack/PackShop";
import logo from "./assets/logo_full.png";
import coinIcon from "./assets/coin.png";
import packsData from "./data/packs.json";
import Garage from "./components/Garage/Garage";

function AppContent() {
  const [activeNav, setActiveNav] = useState<NavItem>("OPEN");

  return (
    <div className="bg-gradient-dark" style={{ minHeight: "100vh" }}>
      <Header
        activeNav={activeNav}
        onNavChange={setActiveNav}
        coinBalance={115999}
        userLevel={22}
        logoUrl={logo}
        coinIconUrl={coinIcon}
      />

      {/* Main content area */}
      <main style={{ padding: "20px" }}>
        {/* Page title */}
        <div
          className="header-1"
          style={{ color: "var(--content-primary)", marginBottom: "24px" }}
        >
          {activeNav === "OPEN" && "Open Packs"}
          {activeNav === "GARAGE" && "My Garage"}
          {activeNav === "TRADE" && "Trade Center"}
        </div>

        {/* Page body / actual content */}
        {activeNav === "OPEN" && <PackShop packs={packsData} />}

        {activeNav === "GARAGE" && (
          <>
            <p
              className="body-1"
              style={{
                color: "var(--content-secondary)",
                marginBottom: "16px",
              }}
            >
              Your cars will show here.
            </p>
            <Garage />
          </>
        )}

        {activeNav === "TRADE" && (
          <p className="body-1" style={{ color: "var(--content-secondary)" }}>
            Trade center coming soon.
          </p>
        )}
      </main>
    </div>
  );
}

function App() {
  return <AppContent />;
}

export default App;
