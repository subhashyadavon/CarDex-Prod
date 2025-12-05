import React, { useEffect, useState } from "react";
import "../../App.css";
import styles from "./Register.module.css";
import Button from "../../components/Button/Button";
import Input from "../../components/TextInput/TextInput";
import logo from "../../assets/logo_full.png";
import { UserIcon, LockIcon } from "../../components/Icons";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";

const Register: React.FC = () => {
  // Form state
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  // UI/UX state
  const [showErrors, setShowErrors] = useState(false);
  const [authError, setAuthError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const navigate = useNavigate();

  const { register, isLoading, isAuthenticated } = useAuth() as {
    register: (payload: {
      username: string;
      password: string;
    }) => Promise<void>;
    isLoading: boolean;
    isAuthenticated: boolean;
  };

  useEffect(() => {
    if (isAuthenticated) {
      navigate("/app");
    }
  }, [isAuthenticated, navigate]);

  const extractErrorMessage = (err: any): string => {
    if (err?.response?.data) {
      const data = err.response.data;
      if (typeof data === "string") return data;
      if (data.message) return data.message;
      if (data.errors) {
        const errorValues = Object.values(data.errors).flat();
        if (errorValues.length > 0) return errorValues.join(" ");
      }
      if (typeof data === "object") {
        try {
          const values = Object.values(data).flat();
          const stringValues = values.filter(v => typeof v === "string");
          if (stringValues.length > 0) return stringValues.join(" ");
          return JSON.stringify(data);
        } catch (e) {
          // ignore parsing errors
        }
      }
    }
    return err?.message || "Registration failed. Please try again.";
  };

  const handleSubmit = async () => {
    // Reset messages
    setAuthError(null);
    setSuccessMessage(null);

    // Basic client validations
    const missing = !username || !password || !confirmPassword;
    const mismatch = password !== confirmPassword;

    if (missing || mismatch) {
      setShowErrors(true);
      if (mismatch) {
        setAuthError("Passwords do not match.");
      }
      return;
    }

    try {
      setShowErrors(false);
      setAuthError(null);

      await register({ username, password });

      setSuccessMessage("Account created successfully. Redirecting...");
      navigate("/app");
    } catch (err: any) {
      console.error("Registration error:", err);
      let backendMessage = extractErrorMessage(err);

      // Custom overrides for specific messages
      if (backendMessage.toLowerCase().includes("username already exists")) {
        backendMessage = "Username already exists, Try different one!";
      }

      setAuthError(backendMessage);
    }
  };

  const displayError =
    (showErrors &&
      ((!username && "Username is required") ||
        (!password && "Password is required") ||
        (!confirmPassword && "Please confirm your password"))) ||
    authError;

  return (
    <div className={`${styles.screen} bg-gradient-dark`}>
      <div className={styles.container}>
        <img src={logo} alt="CarDex Logo" className={styles.logo} />

        <div className={styles.card}>

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
            error={showErrors && !username ? "Username is required" : ""}
          />

          <Input
            size="large"
            type="password"
            icon={<LockIcon />}
            placeholder="Create a password"
            label="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            autoComplete="new-password"
            error={showErrors && !password ? "Password is required" : ""}
          />

          <Input
            size="large"
            type="password"
            icon={<LockIcon />}
            placeholder="Confirm your password"
            label="Confirm Password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
            autoComplete="new-password"
            error={
              showErrors && !confirmPassword
                ? "Please confirm your password"
                : password && confirmPassword && password !== confirmPassword
                  ? "Passwords do not match"
                  : ""
            }
          />

          <Button
            onClick={handleSubmit}
            size="large"
            variant="primary"
            className={styles.submitButton}
            disabled={isLoading}
          >
            {isLoading ? "Creating account..." : "Create Account"}
          </Button>

          {displayError && (
            <p className={`${styles.errorMessage} body-2`}>{displayError}</p>
          )}

          {successMessage && !displayError && (
            <p className={`${styles.successMessage} body-2`}>
              {successMessage}
            </p>
          )}

          <p
            className="body-2"
            style={{ textAlign: "center", marginTop: "0.75rem", opacity: 0.9, color: "white" }}
          >
            Already have an account?{" "}
            <Button
              type="button"
              onClick={() => navigate("/login")}
              className={styles.submitButton} // make it look like a link
            >
              Log in
            </Button>
          </p>
        </div>
      </div>
    </div>
  );
};

export default Register;
