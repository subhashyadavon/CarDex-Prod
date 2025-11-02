// src/components/Input/Input.tsx
import React from "react";
import styles from "./Input.module.css";
import "../../App.css"; // to get your global vars

type InputProps = {
  label?: string;
  name?: string;
  id?: string;
  type?: string;
  value?: string;
  placeholder?: string;
  onChange?: (value: string) => void;
  required?: boolean;
  error?: string;
  className?: string;
};

const Input: React.FC<InputProps> = ({
  label,
  name,
  id,
  type = "text",
  value,
  placeholder,
  onChange,
  required = false,
  error,
  className = "",
}) => {
  const inputId = id || name || label || "input";

  return (
    <div className={`${styles.field} ${className}`}>
      {label && (
        <label htmlFor={inputId} className={styles.label}>
          {label.toUpperCase()}
        </label>
      )}
      <input
        id={inputId}
        name={name}
        type={type}
        value={value}
        placeholder={placeholder}
        required={required}
        onChange={(e) => onChange && onChange(e.target.value)}
        className={`${styles.input} ${error ? styles.inputError : ""}`}
      />
      {error && <p className={styles.error}>{error}</p>}
    </div>
  );
};

export default Input;
