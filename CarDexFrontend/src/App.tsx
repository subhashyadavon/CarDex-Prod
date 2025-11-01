import React, { useState } from "react";
import Header, { NavItem } from "./components/Header/Header";
import "./App.css";

import logo from "./assets/logo_full.png";
import coinIcon from "./assets/coin.png";


function App() {
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
      <main style={{ padding: "48px" }}>
        <div className="header-1" style={{ color: "var(--content-primary)", marginBottom: "24px" }}>
          {activeNav === "OPEN" && "Open Packs"}
          {activeNav === "GARAGE" && "My Garage"}
          {activeNav === "TRADE" && "Trade Center"}
        </div>
        
        {/* Your page content goes here */}
        <p className="body-1" style={{ color: "var(--content-secondary)" }}>
          Content for {activeNav} section
        </p>
      </main>
    </div>
  );
}

export default App;