import React from "react";
import styles from "./TextInput.module.css";
import "../../App.css";

export type InputSize = "regular" | "large";
export type InputType = "text" | "password" | "search";

export type InputProps = {
  value?: string;
  onChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
  size?: InputSize;
  type?: InputType;
  placeholder?: string;
  icon?: React.ReactNode;
  error?: string;
  disabled?: boolean;
  label?: string;
  required?: boolean;
  maxLength?: number;
  autoComplete?: string;
  className?: string;
  id?: string;
  name?: string;
};

export default function Input({
  value,
  onChange,
  size = "regular",
  type = "text",
  placeholder,
  icon,
  error,
  disabled = false,
  label,
  required = false,
  maxLength,
  autoComplete,
  className = "",
  id,
  name,
}: InputProps) {
  const inputClasses = [
    styles.input,
    styles[size],
    icon ? styles.hasIcon : "",
    error ? styles.error : "",
    className,
  ]
    .filter(Boolean)
    .join(" ");

  const inputId = id || name || `input-${Math.random().toString(36).substr(2, 9)}`;

  return (
    <div className={styles.inputWrapper}>
      {label && (
        <label htmlFor={inputId} className="card-2">
          {label}
          {required && <span className={styles.required}>*</span>}
        </label>
      )}
      <div className={styles.inputContainer}>
        {icon && <span className={styles.icon}>{icon}</span>}
        <input
          id={inputId}
          name={name}
          type={type}
          value={value}
          onChange={onChange}
          placeholder={placeholder}
          disabled={disabled}
          required={required}
          maxLength={maxLength}
          autoComplete={autoComplete}
          className={inputClasses}
        />
      </div>
      {error && <span className={styles.errorMessage}>{error}</span>}
    </div>
  );
}