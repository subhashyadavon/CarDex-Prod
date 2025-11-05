# ProtectedRoute.tsx

**Location:** `src/routes/ProtectedRoute.tsx`  

---

## Description

`ProtectedRoute` is a higher-order route component that restricts access to authenticated users.  
It integrates with the global authentication system (`AuthContext`) through the `useAuth()` hook and ensures that only users with a valid session (`isAuthenticated === true`) can access protected pages such as `/app`.

If the authentication state is still initializing, a loading message is displayed.  
If the user is not authenticated, they are automatically redirected to the login page (`/`).

---

## Implementation Summary

- Imports `Navigate` from `react-router-dom` for redirection.
- Imports `useAuth()` from `src/hooks/useAuth` to access authentication state.
- Reads:
  - `isAuthenticated` – indicates if the user is logged in.
  - `isLoading` – indicates if authentication state is initializing.
- Returns:
  - `<Navigate to="/" replace />` if unauthenticated.
  - `<div>Loading...</div>` while loading.
  - `children` if authenticated.

---

## Example Usage

```tsx
import ProtectedRoute from "./routes/ProtectedRoute";
import App from "./App";
import Login from "./pages/Login/Login";

<Routes>
  <Route path="/" element={<Login />} />
  <Route
    path="/app"
    element={
      <ProtectedRoute>
        <App />
      </ProtectedRoute>
    }
  />
</Routes>
