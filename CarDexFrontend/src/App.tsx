import React, { useState, useEffect } from "react";
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
  const [userCurrency, setUserCurrency] = useState(0);
  const navigate = useNavigate();
  const { logout, user } = useAuth();

  // Load user currency when component mounts or user changes
  useEffect(() => {
    if (user?.id) {
      console.log('[App] User from auth context:', user);
      console.log('[App] User currency value:', user.currency);
      console.log('[App] User currency type:', typeof user.currency);

      // Use the currency directly from auth context
      // The auth context gets this from the login response which includes full user data with currency
      if (user.currency !== undefined && user.currency !== null) {
        setUserCurrency(user.currency);
        console.log('[App] Set userCurrency to:', user.currency);
      } else {
        console.warn('[App] User currency is undefined or null');
        setUserCurrency(0);
      }
    }
  }, [user?.id, user?.currency]);

  const handleLogout = async () => {
    await logout();
    navigate("/login");
  };

  return (
    <div className="bg-gradient-dark" style={{ minHeight: "100vh" }}>
      <Header
        activeNav={activeNav}
        onNavChange={setActiveNav}
        coinBalance={userCurrency}
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
