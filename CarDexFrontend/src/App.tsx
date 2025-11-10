import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import Header, { NavItem } from "./components/Header/Header";
import "./App.css";
import logo from "./assets/logo_full.png";
import coinIcon from "./assets/coin.png";
import { useAuth } from "./hooks/useAuth";
import Open from "./pages/Open/Open";
import GaragePage from "./pages/Garage/Garage";
import Trade from "./pages/Trade/Trade";

function AppContent() {
  const [activeNav, setActiveNav] = useState<NavItem>("OPEN");
  const navigate = useNavigate();
  const { logout } = useAuth();

  const handleLogout = async () => {
    await logout();
    navigate("/login");
  };

  return (
    <div className="bg-gradient-dark" style={{ minHeight: "100vh" }}>
      <Header
        activeNav={activeNav}
        onNavChange={setActiveNav}
        coinBalance={115999}
        userLevel={22}
        logoUrl={logo}
        coinIconUrl={coinIcon}
        onLogout={handleLogout}
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
        {activeNav === "OPEN" && <Open />}
        {activeNav === "GARAGE" && <GaragePage />}
        {activeNav === "TRADE" && <Trade />}
      </main>
    </div>
  );
}

function App() {
  return <AppContent />;
}

export default App;
