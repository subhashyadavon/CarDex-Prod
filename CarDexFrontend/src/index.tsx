import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import "./App.css";
import DemoButton from "./components/Button/Demo";

/* -------------------- Mount -------------------- */
const root = document.getElementById("root");
if (!root) throw new Error("Root element #root not found");

ReactDOM.createRoot(root).render(
  <React.StrictMode>
    <DemoButton />
  </React.StrictMode>
);
