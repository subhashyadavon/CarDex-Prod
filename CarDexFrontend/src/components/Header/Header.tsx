import React, { useState } from "react";
import styles from "./Header.module.css";
import Button from "../Button/Button";

export type NavItem = "OPEN" | "GARAGE" | "TRADE";

export type HeaderProps = {
  activeNav: NavItem;
  onNavChange: (nav: NavItem) => void;
  coinBalance?: number;
  userLevel?: number;
  logoUrl?: string;
  coinIconUrl?: string;
  onLogout?: () => void; // Optional logout handler
};

export default function Header({
  activeNav,
  onNavChange,
  coinBalance,
  userLevel,
  logoUrl,
  coinIconUrl,
  onLogout,
}: HeaderProps) {
  const navItems: NavItem[] = ["OPEN", "GARAGE", "TRADE"];

  return (
    <header className={styles.header}>
      {/* Logo Section */}
      <div className={styles.logo}>
        {logoUrl ? (
          <img src={logoUrl} alt="no img" className={styles.logoImage} />
        ) : (
          <span className={`${styles.logoText} header-2`}>CARDEX</span>
        )}
      </div>

      {/* Navigation Section */}
      <nav className={styles.nav}>
        {navItems.map((item) => (
          <button
            key={item}
            onClick={() => onNavChange(item)}
            className={`${styles.navButton} ${
              activeNav === item ? styles.navActive : styles.navInactive
            } header-1`}
          >
            {item}
          </button>
        ))}
      </nav>

      {/* User Info Section */}
      <div className={styles.userInfo}>
        <div className={styles.balance}>
          <span className={styles.coinIcon} aria-hidden>
            {coinIconUrl ? (
              <img src={coinIconUrl} alt="" className={styles.coinImage} />
            ) : (
              "🪙"
            )}
          </span>
          <span className="header-2">{coinBalance !== undefined ? coinBalance.toLocaleString() : "0"}</span>
        </div>
        <div className={`${styles.level} header-2`}>{userLevel || "0"}</div>
        
        {/* Logout Button */}
        {onLogout && (
          <Button
            onClick={onLogout}
            variant="secondary"
            size="regular"
          >
            Logout
          </Button>
        )}
      </div>
    </header>
  );
}