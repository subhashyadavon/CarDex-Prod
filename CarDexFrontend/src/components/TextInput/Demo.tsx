import React, { useState } from "react";
import Input from "./TextInput";
import "../../App.css";

// Example icon components
const UserIcon = () => (
  <svg width="20" height="20" viewBox="0 0 20 20" fill="currentColor">
    <path d="M10 10c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
  </svg>
);

const LockIcon = () => (
  <svg width="20" height="20" viewBox="0 0 20 20" fill="currentColor">
    <path d="M15 7h-1V5c0-2.76-2.24-5-5-5S4 2.24 4 5v2H3c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V9c0-1.1-.9-2-2-2zM6 5c0-1.66 1.34-3 3-3s3 1.34 3 3v2H6V5z"/>
  </svg>
);

const SearchIcon = () => (
  <svg width="20" height="20" viewBox="0 0 20 20" fill="currentColor">
    <path d="M12.9 14.32a8 8 0 1 1 1.41-1.41l5.35 5.33-1.42 1.42-5.33-5.34zM8 14A6 6 0 1 0 8 2a6 6 0 0 0 0 12z"/>
  </svg>
);

export default function InputShowcase() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [search, setSearch] = useState("");
  const [emailError, setEmailError] = useState("");

  const handleEmailChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setUsername(value);
    
    // Simple email validation
    if (value && !value.includes("@")) {
      setEmailError("Please enter a valid email address");
    } else {
      setEmailError("");
    }
  };

  return (
    <div className="bg-gradient-dark" style={{ minHeight: "100vh", padding: "48px" }}>
      <div style={{ maxWidth: "800px", margin: "0 auto" }}>
        <h1 className="titlecase" style={{ marginBottom: "48px", textAlign: "center" }}>
          INPUTS
        </h1>

        {/* Regular Size - Basic */}
        <section style={{ marginBottom: "48px" }}>
          <h2 className="header-2" style={{ marginBottom: "16px" }}>Regular Size - Basic</h2>
          <div style={{ display: "flex", flexDirection: "column", gap: "16px", maxWidth: "400px" }}>
            <Input 
              size="regular" 
              placeholder="Enter text..."
            />
            <Input 
              size="regular" 
              placeholder="With label"
              label="Text Input"
            />
            <Input 
              size="regular" 
              placeholder="Required field"
              label="Required Input"
              required
            />
          </div>
        </section>

        {/* Large Size - Basic */}
        <section style={{ marginBottom: "48px" }}>
          <h2 className="header-2" style={{ marginBottom: "16px" }}>Large Size - Basic</h2>
          <div style={{ display: "flex", flexDirection: "column", gap: "16px", maxWidth: "400px" }}>
            <Input 
              size="large" 
              placeholder="Enter text..."
            />
            <Input 
              size="large" 
              placeholder="With label"
              label="Text Input"
            />
          </div>
        </section>

        {/* With Icons */}
        <section style={{ marginBottom: "48px" }}>
          <h2 className="header-2" style={{ marginBottom: "16px" }}>With Icons</h2>
          <div style={{ display: "flex", flexDirection: "column", gap: "16px", maxWidth: "400px" }}>
            <Input 
              size="regular" 
              icon={<UserIcon />}
              placeholder="Username"
              label="Username"
            />
            <Input 
              size="large" 
              icon={<LockIcon />}
              type="password"
              placeholder="Password"
              label="Password"
            />
            <Input 
              size="regular" 
              icon={<SearchIcon />}
              type="search"
              placeholder="Search cards..."
              label="Search"
            />
          </div>
        </section>

        {/* Error State */}
        <section style={{ marginBottom: "48px" }}>
          <h2 className="header-2" style={{ marginBottom: "16px" }}>Error State</h2>
          <div style={{ display: "flex", flexDirection: "column", gap: "16px", maxWidth: "400px" }}>
            <Input 
              size="regular" 
              placeholder="Enter email"
              label="Email"
              error="This field is required"
            />
            <Input 
              size="large" 
              icon={<UserIcon />}
              placeholder="Enter username"
              label="Username"
              error="Username must be at least 3 characters"
            />
          </div>
        </section>

        {/* Disabled State */}
        <section style={{ marginBottom: "48px" }}>
          <h2 className="header-2" style={{ marginBottom: "16px" }}>Disabled State</h2>
          <div style={{ display: "flex", flexDirection: "column", gap: "16px", maxWidth: "400px" }}>
            <Input 
              size="regular" 
              placeholder="Disabled input"
              label="Disabled"
              disabled
              value="Cannot edit"
            />
            <Input 
              size="large" 
              icon={<LockIcon />}
              placeholder="Disabled with icon"
              disabled
            />
          </div>
        </section>

        {/* Login Form Example */}
        <section style={{ marginBottom: "48px" }}>
          <h2 className="header-2" style={{ marginBottom: "16px" }}>Login Form Example</h2>
          <div style={{ 
            display: "flex", 
            flexDirection: "column", 
            gap: "20px", 
            maxWidth: "400px",
            padding: "32px",
            background: "var(--block-secondary)",
            borderRadius: "12px"
          }}>
            <Input 
              size="large"
              type="text"
              icon={<UserIcon />}
              placeholder="Enter your username"
              label="Username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
              autoComplete="username"
            />
            <Input 
              size="large"
              type="password"
              icon={<LockIcon />}
              placeholder="Enter your password"
              label="Password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              autoComplete="current-password"
            />
          </div>
        </section>

        {/* Search Example */}
        <section style={{ marginBottom: "48px" }}>
          <h2 className="header-2" style={{ marginBottom: "16px" }}>Search Example</h2>
          <div style={{ maxWidth: "600px" }}>
            <Input 
              size="large"
              type="search"
              icon={<SearchIcon />}
              placeholder="Search for cards, players, teams..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
            {search && (
              <p style={{ marginTop: "12px", color: "rgba(255, 255, 255, 0.6)", fontSize: "14px" }}>
                Searching for: "{search}"
              </p>
            )}
          </div>
        </section>

        {/* Validation Example */}
        <section style={{ marginBottom: "48px" }}>
          <h2 className="header-2" style={{ marginBottom: "16px" }}>Validation Example</h2>
          <div style={{ maxWidth: "400px" }}>
            <Input 
              size="regular"
              type="text"
              icon={<UserIcon />}
              placeholder="user@example.com"
              label="Email Address"
              value={username}
              onChange={handleEmailChange}
              error={emailError}
              required
            />
          </div>
        </section>
      </div>
    </div>
  );
}