import { useAuth } from "../hooks/useAuth";
import { LoginRequest } from "../services/authService";

export type LoginField = "username" | "password" | "form";

// Result type returned by the engine after validation/login attempt
export type LoginAttemptResult =
  | { ok: true }
  | { ok: false; field?: LoginField; message: string };

/**
 * validateInputs
 * ----------------
 * Pure client-side validation for username/password.
 * - Ensures username is not empty
 * - Ensures password is present and meets minimal length requirements
 */
function validateInputs(
  username: string,
  password: string
): LoginAttemptResult {
  // Normalize user input types
  let u: string;
  if (typeof username === "string") {
    u = username.trim();
  } else {
    u = "";
  }

  let p: string;
  if (typeof password === "string") {
    p = password;
  } else {
    p = "";
  }

  // Username must be provided
  if (u.length === 0) {
    return { ok: false, field: "username", message: "Username is required." };
  }

  // Password must be provided
  if (p.length === 0) {
    return { ok: false, field: "password", message: "Password is required." };
  }

  // Basic password length check (adjust if your policy differs)
  if (p.length < 6) {
    return {
      ok: false,
      field: "password",
      message: "Password must be at least 6 characters.",
    };
  }

  // If all checks pass, declare success
  return { ok: true };
}

/**
 * useAuthEngine
 * ----------------
 * It performs:
 *   1) Input validation (validateInputs)
 *   2) Server/DB validation by calling context.login({ username, password })
 */
export function useAuthEngine() {
  // - login: calls backend to authenticate and sets user/token on success
  const { login } = useAuth();

  /**
   * validateAndLogin
   * -----------------
   * Runs local checks first, then attempts the real login via context.
   * Returns:
   *   - { ok: true } on success
   *   - { ok: false, field, message } on failure with a sensible error message
   */
  async function validateAndLogin(
    email: string,
    password: string
  ): Promise<LoginAttemptResult> {
    // 1) Client-side input validation
    const inputCheck = validateInputs(email, password);
    if (!inputCheck.ok) {
      return inputCheck;
    }

    // 2) Build payload expected by your authService/login
    const payload: LoginRequest = { email, password };

    // 3) Attempt server/DB validation via context.login()
    try {
      await login(payload); // If valid, the context will set user/token and persist them
      return { ok: true };
    } catch (err: any) {
      // If the backend rejects credentials or an error occurs,
      let message = "Login failed. Please try again.";
      if (err && typeof err.message === "string") {
        if (err.message.length > 0) {
          message = err.message;
        }
      }
      return { ok: false, field: "form", message };
    }
  }

  //call these in the log in page to use the AuthEngine
  return {
    validateAndLogin,
    validateInputs,
  };
}
