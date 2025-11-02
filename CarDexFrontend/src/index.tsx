import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import "./App.css";
import DemoPack from "./components/Pack/Demo";
import CardDemo from "./components/Card/CardDemo";
import InputDemo from "./components/TextInput/Demo";


/* -------------------- Mount -------------------- */
const root = document.getElementById("root");
if (!root) throw new Error("Root element #root not found");

ReactDOM.createRoot(root).render(
  <React.StrictMode>
    <InputDemo/>
  </React.StrictMode>
);
