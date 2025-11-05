/**
 * useAuth Hook - Easy Access to Authentication State
 * 
 * This is a custom hook that makes using AuthContext easier and safer.
 * 
 * WHAT IS A CUSTOM HOOK?
 * A custom hook is a reusable function that uses React hooks (like useState, useContext).
 * By convention, hook names start with "use" (useAuth, useGame, useTrade, etc.)
 * 
 * WHY CREATE THIS WRAPPER?
 * Without this hook, every component would need to:
 *   const context = useContext(AuthContext);
 *   if (!context) throw error;
 *   const { user, login } = context;
 * 
 * With this hook, components just do:
 *   const { user, login } = useAuth();
 * 
 * BENEFITS:
 * 1. Shorter code: One line instead of three
 * 2. Error handling: Catches common mistake (using hook outside provider)
 * 3. Type safety: TypeScript knows the return type
 * 4. Single source: If we change AuthContext structure, only update here
 * 
 * HOW TO USE IN COMPONENTS:
 * 
 * import { useAuth } from '../hooks/useAuth';
 * 
 * function MyComponent() {
 *   const { user, login, logout, isAuthenticated } = useAuth();
 *   
 *   if (!isAuthenticated) {
 *     return <LoginForm onLogin={login} />;
 *   }
 *   
 *   return <div>Welcome, {user.username}!</div>;
 * }
 */

import { useContext } from 'react';
import { AuthContext } from '../context/AuthContext';

/**
 * useAuth: Hook to access authentication state and functions
 * 
 * WHAT IT DOES:
 * 1. Calls useContext(AuthContext) to get current auth state
 * 2. Checks if context exists (would be undefined if used outside AuthProvider)
 * 3. Throws helpful error if used incorrectly
 * 4. Returns context (user, token, login, logout, etc.)
 * 
 * ERROR HANDLING:
 * If you try to use this hook in a component that's NOT wrapped by <AuthProvider>,
 * it will throw a clear error message explaining the problem.
 * 
 * Example of CORRECT usage:
 *   <AuthProvider>
 *     <MyComponent />  ← Can use useAuth() here ✓
 *   </AuthProvider>
 * 
 * Example of INCORRECT usage:
 *   <MyComponent />  ← Can't use useAuth() here ✗ (not inside AuthProvider)
 * 
 * @returns AuthContextType - Object with user, token, login, logout, etc.
 * @throws Error if called outside of AuthProvider
 */
export const useAuth = () => {
  // Get the current value from AuthContext
  // This will be undefined if we're not inside an <AuthProvider>
  const context = useContext(AuthContext);
  
  // Safety check: Make sure we're inside AuthProvider
  // This catches a common mistake where you forget to wrap your app with providers
  if (context === undefined) {
    throw new Error(
      'useAuth must be used within an AuthProvider. ' +
      'Wrap your app with <AuthProvider> in App.tsx'
    );
  }
  
  // Return the context value (user, login, logout, etc.)
  // Components can destructure what they need:
  //   const { user, login } = useAuth();
  //   const { isAuthenticated } = useAuth();
  return context;
};
