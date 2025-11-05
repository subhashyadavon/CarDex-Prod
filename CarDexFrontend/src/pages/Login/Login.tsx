import React from "react";
import "../../App.css";
import styles from "./Login.module.css";
import Button from "../../components/Button/Button";
import Input from "../../components/TextInput/TextInput";
import logo from "../../assets/logo_full.png";
import { UserIcon, LockIcon } from "../../components/Icons";
import { useNavigate } from "react-router-dom";

const Login: React.FC = () => {
  const navigate = useNavigate();

  const handleSubmit = () => {
    navigate("/app");
  };

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
          />

          <Input
            size="large"
            type="password"
            icon={<LockIcon />}
            placeholder="Enter your password"
            label="Password"
          />

          <Button
            onClick={handleSubmit}
            size="large"
            variant="primary"
            className={styles.submitButton}
          >
            Login
          </Button>
        </div>
      </div>
    </div>
  );
};

export default Login;
