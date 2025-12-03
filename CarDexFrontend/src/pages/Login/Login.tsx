import React, { useState, useEffect } from "react";
import "../../App.css";
import styles from "./Login.module.css";
import Button from "../../components/Button/Button";
import Input from "../../components/TextInput/TextInput";
import logo from "../../assets/logo_full.png";
import { UserIcon, LockIcon } from "../../components/Icons";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";

const Login: React.FC = () => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [showErrors, setShowErrors] = useState(false);
  const [authError, setAuthError] = useState<string | null>(null);

  const navigate = useNavigate();
  const { login, isLoading, isAuthenticated } = useAuth();

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

      await login({ username, password });
      navigate("/app");
    } catch (err: any) {
      // Extract error message from various possible response formats
      let errorMessage = "Invalid credentials. Please check your username and password.";

      if (err?.response?.data) {
        // Check for 'detail' field first (standard ProblemDetails from GlobalExceptionHandler)
        if (err.response.data.detail) {
          errorMessage = err.response.data.detail;
        }
        // If data has a message property, use it
        else if (typeof err.response.data.message === 'string') {
          errorMessage = err.response.data.message;
        }
        // If data is a string itself, use it
        else if (typeof err.response.data === 'string') {
          errorMessage = err.response.data;
        }
        // If it's an object with error property
        else if (err.response.data.error) {
          errorMessage = err.response.data.error;
        }
      }
      // Fallback to error message if available
      else if (err?.message && typeof err.message === 'string') {
        errorMessage = err.message;
      }

      setAuthError(errorMessage);
    }
  };

  const displayError =
    (showErrors && (!username || !password) && "Please fill in all fields.") ||
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

          {authError === "Username does not exist" && (
            <p
              className="body-2"
              style={{
                textAlign: "center",
                marginTop: "0.75rem",
                opacity: 0.9,
                color: "white",
              }}
            >
              Don’t have an account?{" "}
              <Button
                type="button"
                onClick={() => navigate("/register")}
                className={styles.submitButton} // make it look like a link
              >
                Create one
              </Button>
            </p>
          )}

          {displayError && (
            <p className={`${styles.errorMessage} body-2`}>{displayError}</p>
          )}

          {isAuthenticated && !displayError && (
            <p className={`${styles.successMessage} body-2`}>
              Logged in successfully.
            </p>
          )}
        </div>
      </div>
    </div>
  );
};

export default Login;
