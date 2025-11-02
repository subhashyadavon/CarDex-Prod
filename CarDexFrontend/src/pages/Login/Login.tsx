import React, { useState } from "react";
import "../../App.css";
import styles from "./Login.module.css";
import Button from "../../components/Button/Button";
import Input from "../../components/Input/Input";
import logo from "../../assets/logo_full.png";

const Login: React.FC = () => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log("Login:", { username, password });
  };

  return (
    <div className={`bg-gradient-dark ${styles.screen}`}>
      <div className={styles.container}>
        <img src={logo} alt="CarDex Logo" className={styles.logo} />

        <form className={styles.card} onSubmit={handleSubmit}>
          <Input
            label="Username"
            value={username}
            onChange={setUsername}
            required
          />

          <Input
            label="Password"
            type="password"
            value={password}
            onChange={setPassword}
            required
          />

          <Button
            type="submit"
            size="large"
            variant="primary"
            className={styles.submitButton}
          >
            Login
          </Button>
        </form>
      </div>
    </div>
  );
};

export default Login;
