import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import App from "./App";
import Login from "./pages/Login/Login";
import Register from "./pages/Register/Register";
import { AuthProvider } from "./context/AuthContext";
import { GameProvider } from "./context/GameContext";
import { TradeProvider } from "./context/TradeContext";

const root = ReactDOM.createRoot(
  document.getElementById("root") as HTMLElement
);

root.render(
  <React.StrictMode>
    <AuthProvider>
      <GameProvider>
        <TradeProvider>
          <BrowserRouter>
            <Routes>
              {/* When someone goes to /, send them to /login */}
              <Route path="/" element={<Navigate to="/login" replace />} />

              {/* Login page */}
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />

              {/* Main app */}
              <Route path="/app" element={<App />} />
            </Routes>
          </BrowserRouter>
        </TradeProvider>
      </GameProvider>
    </AuthProvider>
  </React.StrictMode>
);
