import React, { useState, useEffect } from "react";
import "../../App.css";
import styles from "./Login.module.css";
import Button from "../../components/Button/Button";
import Input from "../../components/TextInput/TextInput";
import logo from "../../assets/logo_full.png";
import { UserIcon, LockIcon } from "../../components/Icons";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";

const Login: React.FC = () => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [showErrors, setShowErrors] = useState(false);
  const [authError, setAuthError] = useState<string | null>(null);

  const navigate = useNavigate();
  const { login, isLoading, isAuthenticated } = useAuth();

  // If already logged in (e.g. token from localStorage), go straight to app
  useEffect(() => {
    if (isAuthenticated) {
      navigate("/app");
    }
  }, [isAuthenticated, navigate]);

  const handleSubmit = async () => {
    if (!username || !password) {
      setShowErrors(true);
      setAuthError(null);
      return;
    }

    try {
      setShowErrors(false);
      setAuthError(null);

      // Backend expects "email", but UI is "username" → map username → email
      console.log("[Login] submitting", { username, password });
      await login({ email: username, password });

      navigate("/app");
    } catch (err: any) {
      console.error("[Login] login failed:", err);

      // Try to surface a useful backend error if present
      const backendMessage =
        err?.response?.data?.message ||
        err?.response?.data ||
        err?.message;

      setAuthError(
        backendMessage
          ? `Login failed: ${backendMessage}`
          : "Invalid username or password. Please try again."
      );
    }
  };

  const displayError =
    (showErrors &&
      (!username || !password) &&
      "Please fill in all fields.") || authError;

  return (
    <div className={`bg-gradient-dark ${styles.screen}`}>
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
            placeholder="Enter your password"
            label="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            autoComplete="current-password"
            error={showErrors && !password ? "Password is required" : ""}
          />

          <Button
            onClick={handleSubmit}
            size="large"
            variant="primary"
            className={styles.submitButton}
            disabled={isLoading}
          >
            {isLoading ? "Logging in..." : "Login"}
          </Button>

          {displayError && (
            <p
              className="body-2"
              style={{ color: "#ff6b6b", marginTop: "0.75rem" }}
            >
              {displayError}
            </p>
          )}

          {isAuthenticated && !displayError && (
            <p
              className="body-2"
              style={{
                color: "var(--content-secondary)",
                marginTop: "0.5rem",
              }}
            >
              Logged in successfully.
            </p>
          )}
        </div>
      </div>
    </div>
  );
};

export default Login;
