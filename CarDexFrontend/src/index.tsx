import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import "./App.css";
import PackDemo from "./components/Pack/Demo";
import CardDemo from "./components/Card/CardDemo";
import InputDemo from "./components/TextInput/Demo";
import Login from "./pages/Login/Login";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";

const DEMO_MODE = false;

/* -------------------- Mount -------------------- */
const root = document.getElementById("root");
if (!root) throw new Error("Root element #root not found");

const app = (
  <React.StrictMode>
    <BrowserRouter>
      {DEMO_MODE ? (
        <>
          <PackDemo />
          <CardDemo />
          <InputDemo />
        </>
      ) : (
        <Routes>
          {/* login page */}
          <Route path="/login" element={<Login />} />

          {/* your main app */}
          <Route path="/app" element={<App />} />

          {/* default: go to login */}
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      )}
    </BrowserRouter>
  </React.StrictMode>
);

ReactDOM.createRoot(root).render(app);
