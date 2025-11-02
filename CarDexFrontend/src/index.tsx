import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import "./App.css";

// Demo page imports, for DEMO_MODE usage
import PackDemo from "./components/Pack/Demo";
import CardDemo from "./components/Card/CardDemo";
import InputDemo from "./components/TextInput/Demo";
// if you actually need the Login page directly later, you can keep this:
import Login from "./pages/Login/Login";

const DEMO_MODE = true;

/* -------------------- Mount -------------------- */
const root = document.getElementById("root");
if (!root) throw new Error("Root element #root not found");

if (DEMO_MODE) {
  ReactDOM.createRoot(root).render(
    <React.StrictMode>
      <PackDemo />
      <CardDemo />
      <InputDemo />
    </React.StrictMode>
  );
} else {
  ReactDOM.createRoot(root).render(
    <React.StrictMode>
      <App />
    </React.StrictMode>
  );
}
