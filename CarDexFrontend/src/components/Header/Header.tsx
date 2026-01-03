import React, { useState, useRef, useEffect } from "react";
import styles from "./Header.module.css";
import { SettingsIcon, UserIcon, LogoutIcon, PenIcon } from "../Icons";

export type NavItem = "OPEN" | "GARAGE" | "TRADE" | "PROFILE" | "EDIT_PROFILE";

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
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const navItems: NavItem[] = ["OPEN", "GARAGE", "TRADE"];

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsDropdownOpen(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleProfileClick = (mode: NavItem) => {
    onNavChange(mode);
    setIsDropdownOpen(false);
  };

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
            className={`${styles.navButton} ${activeNav === item ? styles.navActive : styles.navInactive
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
        {userLevel !== undefined && userLevel !== null && (
          <div className={`${styles.level} header-2`}>{userLevel}</div>
        )}

        {/* Settings Dropdown */}
        <div className={styles.settingsWrapper} ref={dropdownRef}>
          <button
            className={`${styles.settingsButton} ${isDropdownOpen ? styles.settingsButtonActive : ""}`}
            onClick={() => setIsDropdownOpen(!isDropdownOpen)}
            aria-label="Settings"
          >
            <SettingsIcon />
          </button>

          {isDropdownOpen && (
            <div className={styles.dropdown}>
              <button className={styles.dropdownItem} onClick={() => handleProfileClick("PROFILE")}>
                <UserIcon /> Profile
              </button>
              <button className={styles.dropdownItem} onClick={() => handleProfileClick("EDIT_PROFILE")}>
                <PenIcon /> Edit Profile
              </button>
              <div className={styles.dropdownDivider} />
              {onLogout && (
                <button
                  className={`${styles.dropdownItem} ${styles.logoutItem}`}
                  onClick={() => {
                    onLogout();
                    setIsDropdownOpen(false);
                  }}
                >
                  <LogoutIcon /> Logout
                </button>
              )}
            </div>
          )}
        </div>
      </div>
    </header>
  );
}