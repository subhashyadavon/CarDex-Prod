import React from "react";
import styles from "./Button.module.css";

/* Per README and Styleguide, 3 properties to configure the BUTTON with. */
export type ButtonSize = "regular" | "large";
export type ButtonVariant = "primary" | "secondary";
export type ButtonType = "button" | "submit" | "reset";

export type ButtonProps = {
  children?: React.ReactNode;
  size?: ButtonSize;
  variant?: ButtonVariant;
  icon?: React.ReactNode;
  iconOnly?: boolean;
  onClick?: () => void;
  disabled?: boolean;
  type?: ButtonType;
  className?: string;
};

export default function Button({
  children,
  size = "regular",
  variant = "primary",
  icon,
  iconOnly = false,
  onClick,
  disabled = false,
  type = "button",
  className = "",
}: ButtonProps) {
  const buttonClasses = [
    styles.button,
    styles[size],
    styles[variant],
    iconOnly ? styles.iconOnly : "",
    className,
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <button
      type={type}
      className={buttonClasses}
      onClick={onClick}
      disabled={disabled}
    >
      {icon && <span className={styles.icon}>{icon}</span>}
      {!iconOnly && children && <span className={styles.label}>{children}</span>}
    </button>
  );
}